using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;

namespace AccountsChu.Infrastructure.Handlers
{
    public class CachingHttpMessageHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;

        public CachingHttpMessageHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cacheKey = request.RequestUri.ToString();

            if (_cache.TryGetValue(cacheKey, out CachedResponse cached))
            {
                var response = new HttpResponseMessage(cached.StatusCode)
                {
                    Content = new StringContent(cached.Content)
                };

                foreach (var header in cached.Headers)
                    response.Headers.TryAddWithoutValidation(header.Key, header.Value);

                foreach (var header in cached.ContentHeaders)
                    response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);

                return response;
            }

            var realResponse = await base.SendAsync(request, cancellationToken);

            if (realResponse.IsSuccessStatusCode)
            {
                var contentString = await realResponse.Content.ReadAsStringAsync();

                var cachedResponse = new CachedResponse
                {
                    StatusCode = realResponse.StatusCode,
                    Content = contentString,
                    Headers = realResponse.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
                    ContentHeaders = realResponse.Content.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray())
                };

                _cache.Set(cacheKey, cachedResponse, TimeSpan.FromMinutes(10));
            }

            return realResponse;
        }

        private class CachedResponse
        {
            public System.Net.HttpStatusCode StatusCode { get; set; }
            public string Content { get; set; } = string.Empty;
            public Dictionary<string, string[]> Headers { get; set; } = new();
            public Dictionary<string, string[]> ContentHeaders { get; set; } = new();
        }
    }
}
