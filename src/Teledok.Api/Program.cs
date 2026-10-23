using Teledok.Application;
using Teledok.Infrastructure.Postgres;
using Teledok.Shared.Framework;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructurePostgres(builder.Configuration);

var app = builder.Build();

app.UseApiDefaults();

await app.RunAsync();
