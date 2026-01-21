using Application.Dto.Response;
using Domain.Entities;

namespace Application.Mappers;

public static class CartMapper
{
    public static CartResponseDto ToDto(Cart cart)
    {
        if (cart == null)
            return null!;

        return new CartResponseDto
        {
            Id = cart.Id,
            PlayerId = cart.PlayerId,
            Items = cart.Items.Select(ToDto).ToList()
        };
    }

    private static CartItemResponseDto ToDto(CartItem item)
    {
        return new CartItemResponseDto
        {
            GameId = item.Gallery.Id,
            Title = item.Gallery.Game.Title,
            SubTitle = item.Gallery.Game.SubTitle ?? string.Empty,
            Genre = item.Gallery.Game.Genre,
            Description = item.Gallery.Game.Description ?? string.Empty,
            Price = 0 // O pre�o deve vir da GalleryGame, n�o do Game
        };
    }
}