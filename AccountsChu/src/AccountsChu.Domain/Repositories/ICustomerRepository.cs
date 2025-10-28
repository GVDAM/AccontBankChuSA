using AccountsChu.Domain.Entities;

namespace AccountsChu.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task CreateCustomer(Customer customer);
        Task<Customer?> GetCustomerLogin(string email, string password);
        Task<bool> IsAlreadyCreated(string email);
    }
}
