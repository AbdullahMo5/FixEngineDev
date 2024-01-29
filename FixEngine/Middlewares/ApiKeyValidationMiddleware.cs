using FixEngine.Auth;
using System.Net;

namespace FixEngine.Middlewares
{
    public class ApiKeyValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiKeyValidation _apiKeyValidation;

        public ApiKeyValidationMiddleware(RequestDelegate next, IApiKeyValidation apiKeyValidation)
        {
            _next = next;
            _apiKeyValidation = apiKeyValidation;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            string? userApiKey = context.Request.Headers[Shared.Constants.ApiKeyHeaderName];
            bool isApiKeyExists = !string.IsNullOrWhiteSpace(userApiKey);
            if(!isApiKeyExists) {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            if(!_apiKeyValidation.Validate(userApiKey!)) {
                context.Response.StatusCode= (int)HttpStatusCode.Unauthorized;
                return;
            }
            await _next(context);
        }
    }
}
