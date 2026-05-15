using GameStore.Api.Data;
using GameStore.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

//Services are configured in the builder, so we can just call MapDefaultEndpoints to map the default endpoints for health checks, etc.
builder.AddServiceDefaults();

builder.Services.AddValidation();
builder.AddGameStoreDb();

builder.AddKeyedRedisDistributedCache("redis");

var app = builder.Build();
//Services are configured in the builder, so we can just call MapDefaultEndpoints to map the default endpoints for health checks, etc.
app.MapDefaultEndpoints();

app.MapGamesEndpoints();
app.MapGenresEndpoints();

app.MigrateDb();

app.Run();
