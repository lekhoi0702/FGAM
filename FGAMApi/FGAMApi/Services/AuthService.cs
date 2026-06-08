using FGAMApi.DTOs;
using FGAMApi.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
namespace FGAMApi.Services
{
    public class AuthService
    {
        private readonly FgamContext _context;
        private readonly TokenService _tokenService;

        public AuthService(FgamContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.FactoryId == dto.FactoryId && x.EmployeeId == dto.EmployeeId);

            if (account == null || !BCrypt.Net.BCrypt.Verify(dto.Password, account.Passwrd))
                return null;

            return _tokenService.GenerateToken(account);
        }



    }
}
