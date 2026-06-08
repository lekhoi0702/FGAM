using FGAMApi.DTOs;
using FGAMApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FGAMApi.Services
{
    public class TransactionService
    {
        private readonly FgamContext _context;

        public TransactionService(FgamContext context)
        {
            _context = context;
        }

        private IQueryable<TransactionDto> BuildQuery()
        {
            return
                from t in _context.Transactions.AsNoTracking()
                join f in _context.Factories.AsNoTracking()
                    on t.FactoryId equals f.FactoryId
                join e in _context.Employees.AsNoTracking()
                    on t.EmployeeId equals e.EmployeeId
                where t.RecordStatus == 1
                select new TransactionDto
                {
                    FactoryId = t.FactoryId,
                    FactoryName = f.FactoryName,
                    TransactionId = t.TransactionId,
                    CompanyName = t.CompanyName,
                    EmployeeId = t.EmployeeId,
                    EmployeeName = t.EmployeeName ?? e.EmployeeName,
                    CardNumber = t.CardNumber ?? e.CardNumber,
                    Photo = t.Photo,
                    CheckInTime = t.CheckInTime,
                    CheckoutTime = t.CheckoutTime,
                    Purpose = t.Purpose
                };
        }

        public async Task<List<TransactionDto>> GetAllAsync()
        {
            return await BuildQuery()
                .OrderByDescending(x => x.CheckInTime)
                .ToListAsync();
        }

        public async Task<TransactionDto?> GetByIdAsync(long transactionId)
        {
            return await BuildQuery()
                .FirstOrDefaultAsync(x => x.TransactionId == transactionId);
        }

        public async Task<(bool Success, string Message, TransactionDto? Data)> CheckInAsync(TransactionCreateDto dto)
        {
            var factoryExists = await _context.Factories
                .AnyAsync(x => x.FactoryId == dto.FactoryId);
            if (!factoryExists)
            {
                return (false, "FactoryId is invalid", null);
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeId == dto.EmployeeId && x.FactoryId == dto.FactoryId);
            if (employee == null)
            {
                return (false, "EmployeeId is invalid", null);
            }

            var now = DateTime.Now;
            var transactionId = now.ToFileTime();

            var transactionExists = await _context.Transactions
                .AnyAsync(x => x.TransactionId == transactionId);
            while (transactionExists)
            {
                transactionId++;
                transactionExists = await _context.Transactions
                    .AnyAsync(x => x.TransactionId == transactionId);
            }

            var transaction = new Transaction
            {
                FactoryId = dto.FactoryId,
                TransactionId = transactionId,
                CompanyName = dto.CompanyName,
                EmployeeId = dto.EmployeeId,
                EmployeeName = employee.EmployeeName,
                CardNumber = employee.CardNumber,
                Photo = dto.Photo,
                CheckInTime = now,
                CheckoutTime = null,
                RecordStatus = 1,
                CreateDate = now,
                UpdateDate = now,
                Purpose = dto.Purpose
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(transaction.TransactionId);
            return (true, "Created successfully", data);
        }

        public async Task<(bool Success, string Message, TransactionDto? Data)> CheckOutAsync(long transactionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(x => x.TransactionId == transactionId && x.RecordStatus == 1);

            if (transaction == null)
            {
                return (false, "Transaction not found", null);
            }

            if (transaction.CheckoutTime != null)
            {
                return (false, "Transaction already checked out", null);
            }

            transaction.CheckoutTime = DateTime.Now;
            transaction.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            var data = await GetByIdAsync(transaction.TransactionId);
            return (true, "Updated successfully", data);
        }
    }
}
