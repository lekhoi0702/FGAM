using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WarehouseAccessAPI.Common;
using WarehouseAccessAPI.Dtos;
using WarehouseAccessAPI.Models;

namespace WarehouseAccessAPI.Services;

public class AuthService
{
    private readonly FgamContext _db;

    public AuthService(FgamContext db)
    {
        _db = db;
    }

    public async Task<Response<LoginUserProfileDto>> LoginAsync(LoginRequestDto request)
    {
        var employeeId = request?.UserId?.Trim();
        var factoryId = request?.FactoryId?.Trim();
        var password = request?.Password?.Trim();

        if (string.IsNullOrWhiteSpace(employeeId) || string.IsNullOrWhiteSpace(factoryId) || string.IsNullOrWhiteSpace(password))
        {
            return new Response<LoginUserProfileDto>(false, null, "UserId, FactoryId and Password are required");
        }

        var account = await _db.Accounts
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Factory)
            .FirstOrDefaultAsync(a => a.FactoryId == factoryId && a.EmployeeId == employeeId);

        if (account is null || !VerifyPassword(password, account.Passwrd))
        {
            return new Response<LoginUserProfileDto>(false, null, "Login information is invalid");
        }

        if (account.RecordStatus == 0)
        {
            return new Response<LoginUserProfileDto>(false, null, "Account is disabled");
        }
        var profile = new LoginUserProfileDto
        {
            UserId = account.EmployeeId,
            FullName = account.Employee?.EmployeeName ?? "Employee",
            DepartmentName = account.Employee?.Department?.DepartmentName ?? "N/A",
            FactoryId = account.FactoryId,
            FactoryName = account.Factory?.FactoryName ?? "Factory",
            RecordStatus = account.RecordStatus.ToString()
        };

        return new Response<LoginUserProfileDto>(true, profile, "Login success");
    }

    private static bool VerifyPassword(string plainPassword, string storedPassword)
    {
        if (string.IsNullOrWhiteSpace(storedPassword)) return false;

        var parts = storedPassword.Split('$');
        if (parts.Length != 3 || parts[0] != "pbkdf2") return false;

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                plainPassword, 
                salt, 
                100_000, 
                HashAlgorithmName.SHA256, 
                32
            );
            
            return CryptographicOperations.FixedTimeEquals(expectedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }
}
