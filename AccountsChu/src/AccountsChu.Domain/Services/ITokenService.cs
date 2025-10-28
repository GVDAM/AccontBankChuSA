using AccountsChu.Domain.Entities;

namespace AccountsChu.Domain.Services
{
    public interface ITokenService
    {
        string GenerateToken(Customer customer);
    }
}
