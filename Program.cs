using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// PostgreSQL connection
builder.Services.AddSingleton<NpgsqlConnection>(sp =>
{
    var conn = new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    conn.Open();
    return conn;
});

// Add WebSocket support
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging API V1");
});

// Enable WebSockets
app.UseWebSockets();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Log application start
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started");

app.Run();
