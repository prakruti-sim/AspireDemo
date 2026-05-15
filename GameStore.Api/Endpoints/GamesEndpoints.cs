using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet("/", async (GameStoreContext dbContext)
            => await dbContext.Games
                              .Include(game => game.Genre)
                              .Select(game => new GameSummaryDto(
                                game.Id,
                                game.Name,
                                game.Genre!.Name,
                                game.Price,
                                game.ReleaseDate
                              ))
                              .AsNoTracking()
                              .ToListAsync());

        // GET /games/1
        group.MapGet("/{id}", async (
            int id, 
            GameStoreContext dbContext, 
            IDistributedCache cache, 
            ILogger<Program> logger) =>
        {
            var cacheJson = await cache.GetStringAsync($"game:{id}");
            if(cacheJson is not null)
            {
                return Results.Ok(JsonSerializer.Deserialize<GameDetailsDto>(cacheJson));
            }

            logger.LogInformation("Cache miss for game with id {GameId}", id);

            Game? game = await dbContext.Games.FindAsync(id);

          if (game is null)
            {
                return Results.NotFound();
            }
            var dto = game.ToGameDetailsDto();
           
            //cacheJson = JsonSerializer.Serialize(dto);
            await cache.SetStringAsync(
                key: $"game:{id}", 
                value: JsonSerializer.Serialize(dto), 
                options: new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return Results.Ok(dto);
        })
        .WithName(GetGameEndpointName);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = gameDto.Id }, gameDto);
        });

        // PUT /games/1
        group.MapPut("/{id}", async (
            int id,
            UpdateGameDto updatedGame,
            GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            existingGame.Name = updatedGame.Name;
            existingGame.GenreId = updatedGame.GenreId;
            existingGame.Price = updatedGame.Price;
            existingGame.ReleaseDate = updatedGame.ReleaseDate;

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                            .Where(game => game.Id == id)
                            .ExecuteDeleteAsync();

            return Results.NoContent();
        });
    }
}
