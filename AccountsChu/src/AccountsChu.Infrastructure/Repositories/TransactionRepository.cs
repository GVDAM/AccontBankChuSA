using AccountsChu.Infrastructure.Data;
using AccountsChu.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using AccountsChu.Domain.Entities;

namespace AccountsChu.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {

        private readonly AppDbContext _context;
        public TransactionRepository(AppDbContext context)
        {
             _context = context;
        }


        public async Task CreateTransaction(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Transaction>> GetAllTransactions(int accountId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Where(x => x.SenderAccountId == accountId || x.ReceiverAccountId == accountId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
