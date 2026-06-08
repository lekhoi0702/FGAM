using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Models;

namespace WarehouseAccessAPI.Services;

public class TransactionService
{
    private const int ActiveRecordStatus = 1;
    private static readonly string[] ExternalGuestPrefixes = ["SU", "TT", "KH"];
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly FgamContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GuestApi _guestApi;

    public TransactionService(FgamContext db, IHttpClientFactory httpClientFactory, IOptions<GuestApi> guestApi)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _guestApi = guestApi.Value;
    }

    public async Task<Response<LookupByCardResponseDto>> LookupByCardAsync(LookupByCardRequestDto request, CancellationToken cancellationToken = default)
    {
        var cardInput = request?.CardNumber?.Trim();
        if (string.IsNullOrWhiteSpace(cardInput))
        {
            return new Response<LookupByCardResponseDto>(false, null, "CardNumber is required");
        }

        if (!long.TryParse(cardInput, out long parsedCardNumber))
        {
            return new Response<LookupByCardResponseDto>(false, null, "CardNumber must be a valid number");
        }

        var employee = await _db.Employees
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Factory)
            .FirstOrDefaultAsync(x => x.CardNumber == parsedCardNumber, cancellationToken);

        if (employee is null)
        {
            return new Response<LookupByCardResponseDto>(false, null, "Card not found");
        }

        var openTransaction = await _db.Transactions
            .AsNoTracking()
            .Where(x => x.EmployeeId == employee.EmployeeId && x.CheckoutTime == null)
            .OrderByDescending(x => x.CheckInTime)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new LookupByCardResponseDto
        {
            CardNumber = cardInput,
            EmployeeId = employee.EmployeeId,
            EmployeeName = employee.EmployeeName,
            FactoryId = employee.FactoryId,
            FactoryName = employee.Factory?.FactoryName,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.DepartmentName,
            PurposeName = "Làm việc",
            IsInside = openTransaction is not null,
            OpenTransactionId = openTransaction?.TransactionId.ToString()
        };

        var normalizedEmployeeId = employee.EmployeeId.Trim();
        var isExternal = ExternalGuestPrefixes.Any(prefix => normalizedEmployeeId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (isExternal)
        {
            var guest = await TryLookupGuestAsync(employee.FactoryId, normalizedEmployeeId, cancellationToken);
            if (guest is not null)
            {
                if (!string.IsNullOrWhiteSpace(guest.Guestname)) dto.EmployeeName = guest.Guestname.Trim();
                if (!string.IsNullOrWhiteSpace(guest.Company)) dto.FactoryName = guest.Company.Trim();
                if (!string.IsNullOrWhiteSpace(guest.Purpose)) dto.PurposeName = guest.Purpose.Trim();
                if (!string.IsNullOrWhiteSpace(guest.Deptcontact)) dto.ContactPerson = guest.Deptcontact.Trim();
                dto.IsExternalGuestDataApplied = true;
            }
        }

        return new Response<LookupByCardResponseDto>(true, dto, "Success");
    }

    public async Task<Response<TransactionDetailDto>> CreateCheckInAsync(CreateTransactionCheckInRequestDto request)
    {
        if (request is null)
        {
            return new Response<TransactionDetailDto>(false, null, "Request is required");
        }

        var loginUserId = request.LoginUserId?.Trim();
        if (string.IsNullOrWhiteSpace(loginUserId))
        {
            return new Response<TransactionDetailDto>(false, null, "loginUserId is required");
        }

        Employee? matchedEmployee = null;
        var employeeId = request.EmployeeId?.Trim();
        var cardInput = request.CardNumber?.Trim();

        long? cardNumber = null;
        if (!string.IsNullOrWhiteSpace(cardInput))
        {
            if (long.TryParse(cardInput, out long parsedCard))
            {
                cardNumber = parsedCard;
            }
        }

        if (!string.IsNullOrWhiteSpace(employeeId))
        {
            matchedEmployee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        }
        else if (cardNumber.HasValue)
        {
            matchedEmployee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.CardNumber == cardNumber);
        }

        if (matchedEmployee is not null)
        {
            var isInside = await _db.Transactions.AsNoTracking().AnyAsync(x => x.EmployeeId == matchedEmployee.EmployeeId && x.CheckoutTime == null);
            if (isInside)
            {
                return new Response<TransactionDetailDto>(false, null, "Employee is already checked in and not checked out");
            }
        }

        var factoryId = request.FactoryId?.Trim() ?? matchedEmployee?.FactoryId;
        var departmentId = request.DepartmentId?.Trim() ?? matchedEmployee?.DepartmentId;
        var employeeName = string.IsNullOrWhiteSpace(request.EmployeeName) ? matchedEmployee?.EmployeeName : request.EmployeeName.Trim();
        var companyName = request.CompanyName?.Trim() ?? matchedEmployee?.Factory?.FactoryName ?? "Khách";
        var purposeName = request.PurposeName?.Trim() ?? "Làm việc";

        if (string.IsNullOrWhiteSpace(employeeName))
        {
            return new Response<TransactionDetailDto>(false, null, "EmployeeName is required");
        }
        if (string.IsNullOrWhiteSpace(factoryId))
        {
            return new Response<TransactionDetailDto>(false, null, "FactoryId is required");
        }
        if (string.IsNullOrWhiteSpace(departmentId))
        {
            return new Response<TransactionDetailDto>(false, null, "DepartmentId is required");
        }

        var now = DateTime.Now;
        if (matchedEmployee is null)
        {
            // Auto register a virtual employee if guest not exists
            var fallbackEmployeeId = string.IsNullOrWhiteSpace(employeeId) ? ("GUEST_" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()) : employeeId;
            matchedEmployee = new Employee
            {
                EmployeeId = fallbackEmployeeId,
                EmployeeName = employeeName,
                CardNumber = cardNumber,
                FactoryId = factoryId,
                DepartmentId = departmentId,
                RecordStatus = ActiveRecordStatus,
                CreateDate = now,
                CreateId = loginUserId ?? "SYSTEM",
                UpdateDate = now,
                UpdateId = loginUserId ?? "SYSTEM"
            };
            _db.Employees.Add(matchedEmployee);
            await _db.SaveChangesAsync();
        }

        var contactPerson = request.ContactPerson?.Trim() ?? loginUserId;

        var entity = new Transaction
        {
            TransactionId = DateTime.Now.ToFileTime(),
            FactoryId = factoryId,
            EmployeeId = matchedEmployee.EmployeeId,
            EmployeeName = employeeName,
            CardNumber = cardNumber,
            CompanyName = companyName,
            Photo = NormalizePhoto(request.Photo),
            CheckInTime = now,
            CheckoutTime = null,
            RecordStatus = ActiveRecordStatus,
            CreateId = loginUserId ?? "SYSTEM",
            CreateDate = now,
            UpdateId = loginUserId ?? "SYSTEM",
            UpdateDate = now,
            Purpose = purposeName
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync();

        var dto = await MapTransactionToDto(entity.TransactionId);
        return new Response<TransactionDetailDto>(true, dto, "Success");
    }

    public async Task<Response<TransactionDetailDto>> ConfirmCheckOutAsync(ConfirmCheckOutRequestDto request)
    {
        if (!long.TryParse(request?.TransactionId, out var transactionId))
        {
            return new Response<TransactionDetailDto>(false, null, "TransactionId is required");
        }

        var transaction = await _db.Transactions.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
        if (transaction is null)
        {
            return new Response<TransactionDetailDto>(false, null, "Transaction not found");
        }
        if (transaction.CheckoutTime is not null)
        {
            return new Response<TransactionDetailDto>(false, null, "Already checked out");
        }

        transaction.CheckoutTime = DateTime.Now;
        transaction.UpdateDate = DateTime.Now;
        await _db.SaveChangesAsync();

        var dto = await MapTransactionToDto(transaction.TransactionId);
        return new Response<TransactionDetailDto>(true, dto, "Success");
    }

    public async Task<Response<List<TransactionDetailDto>>> GetLiveMonitorAsync(string? keyword, int take = 200)
    {
        var query = _db.Transactions.AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(e => e.Department)
            .Include(x => x.Factory)
            .Where(x => x.CheckoutTime == null);

        var k = keyword?.Trim();
        if (!string.IsNullOrWhiteSpace(k))
        {
            if (long.TryParse(k, out long parsedCard))
            {
                query = query.Where(x => x.EmployeeId.Contains(k) || x.EmployeeName.Contains(k) || x.CompanyName.Contains(k) || x.CardNumber == parsedCard);
            }
            else
            {
                query = query.Where(x => x.EmployeeId.Contains(k) || x.EmployeeName.Contains(k) || x.CompanyName.Contains(k));
            }
        }

        var items = await query.OrderByDescending(x => x.CheckInTime).Take(Math.Clamp(take, 1, 1000)).ToListAsync();
        var dto = new List<TransactionDetailDto>();
        foreach (var item in items)
        {
            dto.Add(MapTransactionToDto(item));
        }
        return new Response<List<TransactionDetailDto>>(true, dto, "Success");
    }

    public async Task<Response<TransactionDashboardStatsDto>> GetDashboardStatsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var todayRows = await _db.Transactions.AsNoTracking()
            .Where(x => x.CheckInTime >= today && x.CheckInTime < tomorrow 
                     && x.CheckoutTime != null 
                     && x.CheckoutTime >= today && x.CheckoutTime < tomorrow)
            .Select(x => new { x.CardNumber, x.EmployeeId, x.EmployeeName })
            .ToListAsync();

        var uniqueTodayPeople = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in todayRows)
        {
            var cardNumberKey = row.CardNumber.HasValue ? row.CardNumber.Value.ToString() : string.Empty;
            var employeeIdKey = (row.EmployeeId ?? string.Empty).Trim().ToUpperInvariant();
            var employeeNameKey = (row.EmployeeName ?? string.Empty).Trim().ToUpperInvariant();
            uniqueTodayPeople.Add($"{cardNumberKey}|{employeeIdKey}|{employeeNameKey}");
        }

        var totalRecords = await _db.Transactions.AsNoTracking().CountAsync();
        var stats = new TransactionDashboardStatsDto
        {
            TodayUniqueVisitors = uniqueTodayPeople.Count,
            TotalRecords = totalRecords
        };

        return new Response<TransactionDashboardStatsDto>(true, stats, "Success");
    }

    public IQueryable<Transaction> GetHistoryQuery(string? keyword, DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Transactions.AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(e => e.Department)
            .Include(x => x.Factory)
            .AsQueryable();

        if (fromDate.HasValue) query = query.Where(x => x.CheckInTime >= fromDate.Value);
        if (toDate.HasValue)
        {
            var toDateValue = toDate.Value;
            if (toDateValue.TimeOfDay == TimeSpan.Zero)
            {
                var nextDay = toDateValue.Date.AddDays(1);
                query = query.Where(x => x.CheckInTime < nextDay);
            }
            else
            {
                query = query.Where(x => x.CheckInTime <= toDateValue);
            }
        }
        var k = keyword?.Trim();
        if (!string.IsNullOrWhiteSpace(k))
        {
            if (long.TryParse(k, out long parsedCard))
            {
                query = query.Where(x => x.EmployeeId.Contains(k) || x.EmployeeName.Contains(k) || x.CompanyName.Contains(k) || x.CardNumber == parsedCard);
            }
            else
            {
                query = query.Where(x => x.EmployeeId.Contains(k) || x.EmployeeName.Contains(k) || x.CompanyName.Contains(k));
            }
        }
        return query;
    }

    public async Task<Response<PagedResult<TransactionDetailDto>>> GetHistoryAsync(
        string? keyword, 
        DateTime? fromDate, 
        DateTime? toDate, 
        int page = 1, 
        int pageSize = 15)
    {
        var query = GetHistoryQuery(keyword, fromDate, toDate);
        var total = await query.CountAsync();

        var items = await query.OrderByDescending(x => x.CheckInTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dto = items.Select(MapTransactionToDto).ToList();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var pagedResult = new PagedResult<TransactionDetailDto>
        {
            Items = dto,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = totalPages
        };

        return new Response<PagedResult<TransactionDetailDto>>(true, pagedResult, "Success");
    }

    public async Task<byte[]> ExportHistoryExcelBytesAsync(string? keyword, DateTime? fromDate, DateTime? toDate)
    {
        var query = GetHistoryQuery(keyword, fromDate, toDate);
        var items = await query.OrderByDescending(x => x.CheckInTime).Take(5000).ToListAsync();
        var rows = items.Select(MapTransactionToDto).ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Transactions");
        
        ws.ShowGridLines = true;

        ws.Cell(1, 1).Value = "ĐĂNG KÝ RA VÀO KHO THÀNH PHẨM";
        ws.Range(1, 1, 1, 8).Merge();
        ws.Cell(1, 1).Style.Font.FontSize = 24;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(1).Height = 45;

        ws.Cell(2, 1).Value = "STT";
        ws.Cell(2, 2).Value = "Ngày";
        ws.Cell(2, 3).Value = "Mã NV";
        ws.Cell(2, 4).Value = "Họ Tên";
        ws.Cell(2, 5).Value = "Công ty";
        ws.Cell(2, 6).Value = "Giờ Vào";
        ws.Cell(2, 7).Value = "Giờ Ra";
        ws.Cell(2, 8).Value = "Mục đích";

        var headerRow = ws.Row(2);
        headerRow.Height = 26;

        for (int col = 1; col <= 8; col++)
        {
            var cell = ws.Cell(2, col);
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0E4391");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D3D3D3");
        }

        var row = 3;
        var stt = 1;
        foreach (var item in rows)
        {
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = item.CheckInTime?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(row, 3).Value = item.EmployeeId ?? "";
            ws.Cell(row, 4).Value = item.EmployeeName ?? "";
            ws.Cell(row, 5).Value = item.FactoryNameAlias ?? "";
            ws.Cell(row, 6).Value = item.CheckInTime?.ToString("HH:mm:ss") ?? "";
            ws.Cell(row, 7).Value = item.CheckoutTime?.ToString("HH:mm:ss") ?? "";
            ws.Cell(row, 8).Value = item.Purpose ?? "Làm việc";

            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int col = 1; col <= 8; col++)
            {
                var cell = ws.Cell(row, col);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E0E0E0");
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            ws.Row(row).Height = 20;
            row++;
        }

        ws.Columns(1, 8).AdjustToContents(2, Math.Max(2, row - 1));

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<TransactionDetailDto> MapTransactionToDto(long transactionId)
    {
        var transaction = await _db.Transactions.AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(e => e.Department)
            .Include(x => x.Factory)
            .FirstAsync(x => x.TransactionId == transactionId);
        return MapTransactionToDto(transaction);
    }

    private static TransactionDetailDto MapTransactionToDto(Transaction x)
    {
        var displayEmployeeId = x.EmployeeId;
        if (displayEmployeeId != null && displayEmployeeId.StartsWith("GUEST_", StringComparison.OrdinalIgnoreCase))
        {
            displayEmployeeId = null;
        }

        return new TransactionDetailDto
        {
            TransactionId = x.TransactionId.ToString(),
            CardNumber = x.CardNumber.HasValue ? x.CardNumber.Value.ToString() : null,
            EmployeeId = displayEmployeeId,
            EmployeeName = x.EmployeeName,
            FactoryId = x.FactoryId,
            FactoryName = x.Factory?.FactoryName,
            DepartmentId = x.Employee?.DepartmentId,
            DepartmentName = x.Employee?.Department?.DepartmentName,
            FactoryNameAlias = x.CompanyName,
            Purpose = x.Purpose,
            Photo = x.Photo,
            CheckInTime = ToLocalOffset(x.CheckInTime),
            CheckoutTime = ToLocalOffset(x.CheckoutTime),
            CreateDate = ToLocalOffset(x.CreateDate),
            UpdateDate = ToLocalOffset(x.UpdateDate)
        };
    }

    private static DateTimeOffset ToLocalOffset(DateTime value)
    {
        var localTime = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        return new DateTimeOffset(localTime, TimeZoneInfo.Local.GetUtcOffset(localTime));
    }

    private static DateTimeOffset? ToLocalOffset(DateTime? value)
    {
        return value.HasValue ? ToLocalOffset(value.Value) : null;
    }

    private static string? NormalizePhoto(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        var marker = "base64,";
        var markerIndex = trimmed.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return markerIndex >= 0 ? trimmed[(markerIndex + marker.Length)..] : trimmed;
    }

    private HttpClient CreateGuestApiClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_guestApi.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(_guestApi.TimeoutSeconds <= 0 ? 8 : _guestApi.TimeoutSeconds);
        return client;
    }

    private async Task<ExternalGuestRecord?> TryLookupGuestAsync(string company, string userId, CancellationToken cancellationToken)
    {
        try
        {
            using var client = CreateGuestApiClient();
            var tokenResponse = await client.GetAsync($"/token?company={Uri.EscapeDataString(company)}", cancellationToken);
            if (!tokenResponse.IsSuccessStatusCode) return null;
            var tokenRaw = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            string? jwtToken = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(tokenRaw);
                if (doc.RootElement.TryGetProperty("token", out var tokenProp))
                {
                    jwtToken = tokenProp.GetString();
                }
            }
            catch
            {
                jwtToken = tokenRaw.Trim().Trim('"');
            }

            if (string.IsNullOrWhiteSpace(jwtToken)) return null;

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/guest/list?com={Uri.EscapeDataString(company)}&limit=10&cardnum={Uri.EscapeDataString(userId)}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var records = JsonSerializer.Deserialize<List<ExternalGuestRecord>>(content, JsonOptions) ?? [];
            return records.FirstOrDefault(x => string.Equals(x.Cardnum?.Trim(), userId, StringComparison.OrdinalIgnoreCase))
                ?? records.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private sealed class ExternalGuestRecord
    {
        public string? Company { get; set; }
        public string? Guestname { get; set; }
        public string? Guestno { get; set; }
        public string? Deptcontact { get; set; }
        public string? Cardnum { get; set; }
        public string? Cardtype { get; set; }
        public string? Purpose { get; set; }
    }
}
