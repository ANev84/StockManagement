using StockApi.StockDataService;
using StockApi.StockDataService.Cache;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStockDataService, FileStockDataService>();

// Redis configuration as a distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "StockApi_";
});

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));

// Temporary provider
builder.Services.AddSingleton<InMemoryStockCacheService>();
builder.Services.AddSingleton<CacheServiceFactory>();


// IStockCacheService created through a factory
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<CacheServiceFactory>();
    return factory.CreateAsync().GetAwaiter().GetResult();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
