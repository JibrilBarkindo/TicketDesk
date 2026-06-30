using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using TicketDesk.Api.Data;
using TicketDesk.Api.Middleware;
using TicketDesk.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("Default") ?? "Data Source=ticketdesk.db"));

builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TicketDesk API",
        Version = "v1",
        Description = "A small support-ticket tracking API built with ASP.NET Core and EF Core."
    }));

var app = builder.Build();

// --- Database ---------------------------------------------------------------
// EnsureCreated is used here for a frictionless demo run. In a real deployment
// you would replace this with EF Core migrations (see README).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// --- Pipeline ---------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Exposed so an integration-test project could use WebApplicationFactory<Program>.
public partial class Program { }
