using Microsoft.AspNetCore.Http;

namespace TheBestBookstore.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsAjax(this HttpRequest request)
        {
            if (request == null)
                return false;

            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
