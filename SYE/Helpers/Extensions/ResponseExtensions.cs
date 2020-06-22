using Microsoft.AspNetCore.Http.Features;

namespace SYE.Helpers.Extensions
{
    public static class ResponseExtensions
    {
        public static void SetResponse(this IHttpResponseFeature responseFeature, string message, int statusCode)
        {
            responseFeature.ReasonPhrase = message;
            responseFeature.StatusCode = statusCode;            
        }
    }
}
