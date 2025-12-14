using System.Security.Cryptography;
using System.Text;

namespace SmartParking_Api.Auth;


public class ApiKeyCheck
{
    private const string HeaderName = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly string _expectedKey;


    public ApiKeyCheck(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _expectedKey = config["Auth:ApiKey"]
            ?? throw new InvalidOperationException("Auth:ApiKey não está configurada (user-secrets/appsettings).");
    }
    
    
    public async Task InvokeAsync(HttpContext context)
    {
        //pra n dar cabo de CORS
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Chave da API em falta.");
            return;
        }

        var providedKey = provided.ToString();
        
        var a = Encoding.UTF8.GetBytes(providedKey);
        var b = Encoding.UTF8.GetBytes(_expectedKey);

        var ok = a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);

        if (!ok)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Chave de API inválida.");
            return;
        }

        await _next(context);
    }

}