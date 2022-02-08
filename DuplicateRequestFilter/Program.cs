using Microsoft.Extensions.Caching.Distributed;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var redis = context.RequestServices.GetRequiredService<IDistributedCache>();
    var requestId = context.Request.Headers["x-request-id"].FirstOrDefault()?.ToString();

    if (string.IsNullOrEmpty(requestId))
    {
        context.Response.StatusCode = HttpStatusCode.BadRequest.GetHashCode();
        await context.Response.WriteAsync("x-request-id is missing");
        return;
    }

    var cacheValue = await redis.GetStringAsync(requestId);

    if (string.IsNullOrEmpty(cacheValue) is false)
    {
        context.Response.StatusCode = HttpStatusCode.Conflict.GetHashCode();
        return;
    }

    await redis.SetStringAsync(requestId, requestId, options: new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
    });

    await next();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
