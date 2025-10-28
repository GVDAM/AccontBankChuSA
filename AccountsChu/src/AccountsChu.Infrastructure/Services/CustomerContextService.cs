using AccountsChu.Domain.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AccountsChu.Infrastructure.Services
{
    public class CustomerContextService : ICustomerContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCustomerId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            return userId != null ? int.Parse(userId) : null;
        }
    }
}
