using AccountsChu.Domain.Entities;

namespace AccountsChu.Domain.Repositories
{
    public interface ITransactionRepository
    {
        Task CreateTransaction(Transaction transaction);
        Task<List<Transaction>> GetAllTransactions(int accountId);
    }
}
