using GameStore.Api.Dtos;

namespace GameStore.Api.Models;

internal static class GameExtensions
{
    public static GameDetailsDto ToGameDetailsDto(this Game game)
    {
        return new GameDetailsDto(
            game.Id,
            game.Name,
            game.GenreId,
            game.Price,
            game.ReleaseDate
        );
    }
}
