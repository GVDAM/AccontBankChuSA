using AccountsChu.Domain.Repositories;
using AccountsChu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AccountsChu.Domain.Entities;

namespace AccountsChu.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateCustomer(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<Customer?> GetCustomerLogin(string email, string password)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Password == password && x.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> IsAlreadyCreated(string email)
        {
            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }
    }
}
