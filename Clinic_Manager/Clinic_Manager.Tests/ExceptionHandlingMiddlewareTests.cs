using System.Net;
using System.Text.Json;
using Clinic_Manager.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clinic_Manager.Tests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ApiRequest_ReturnsJsonErrorResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/patients";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            next: (innerContext) => throw new ArgumentException("Błędne dane wejściowe"),
            logger: NullLogger<ExceptionHandlingMiddleware>.Instance
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var jsonString = await reader.ReadToEndAsync();
        
        var jsonDoc = JsonDocument.Parse(jsonString);
        var root = jsonDoc.RootElement;

        Assert.Equal(400, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Błędne dane wejściowe", root.GetProperty("message").GetString());
        Assert.Contains("ArgumentException", root.GetProperty("detailed").GetString());
    }

    [Fact]
    public async Task InvokeAsync_NonApiRequest_RethrowsException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/Patients/Details";

        var expectedException = new InvalidOperationException("Błąd bazy danych w widoku");
        var middleware = new ExceptionHandlingMiddleware(
            next: (innerContext) => throw expectedException,
            logger: NullLogger<ExceptionHandlingMiddleware>.Instance
        );

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context)
        );
        Assert.Same(expectedException, thrownException);
    }
}
