using Refit;

namespace AccountsChu.Domain.Services
{
    public interface IBrasilApiService
    {
        [Get("/feriados/v1/2025")]
        Task<List<Holydays>> GetHolydays();
    }

    public class Holydays
    {
        public string Date { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
