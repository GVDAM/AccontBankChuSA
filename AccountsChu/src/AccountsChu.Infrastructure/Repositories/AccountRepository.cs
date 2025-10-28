using AccountsChu.Infrastructure.Data;
using AccountsChu.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using AccountsChu.Domain.Entities;

namespace AccountsChu.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> VerifyAccountByCustomer(int customerId)
        {
            return await _context.Accounts
                .AsNoTracking()
                .AnyAsync(a => a.CustomerId == customerId);
        }

        public async Task<Account> GetAccountByCustomer(int customerId)
        {
            return await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.CustomerId == customerId);
        }

        public async Task<Account> GetByAgencyAndNumberAsync(short agency, int number)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Agency == agency && a.Number == number);
        }

        public async Task<bool> VerifyAccountByAgencyAndNumberAsync(short agency, int number)
        {
            return await _context.Accounts
                .AnyAsync(a => a.Agency == agency && a.Number == number);
        }

        public async Task<bool> HasBalanceEnough(decimal amount, int customerId)
        {
            return await _context.Accounts
                .AsNoTracking()
                .AnyAsync(a => a.CustomerId == customerId && a.Balance >= amount);
        }

        public async Task Update(Account account)
        {
            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
