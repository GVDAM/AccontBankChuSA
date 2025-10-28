using AccountsChu.Domain.Entities;

namespace AccountsChu.Domain.Repositories
{
    public interface IAccountRepository
    {
        Task<Account> GetByAgencyAndNumberAsync(short agency, int number);
        Task<bool> VerifyAccountByAgencyAndNumberAsync(short agency, int number);
        Task AddAsync(Account account);
        Task<bool> HasBalanceEnough(decimal amount, int customerId);
        Task<bool> VerifyAccountByCustomer(int customerId);
        Task Update(Account account);
        Task<Account> GetAccountByCustomer(int customerId);
    }
}
