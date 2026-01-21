using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Mappers;
using Application.Repositories;
using Domain.Entities;
using Domain.Service;

namespace Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IGalleryRepository _galleryRepository;
    private readonly IPlayerRepository _playerRepository;

    public CartService(
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IGalleryRepository galleryRepository,
        IPlayerRepository playerRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _galleryRepository = galleryRepository;
        _playerRepository = playerRepository;
    }

    public async Task<OperationResult> AddItemAsync(CartItemRequestDto request)
    {
        // Verifica se o player existe
        var player = await _playerRepository.GetByIdAsync(request.PlayerId);
        
        if (player == null)
            return OperationResult.Failure("User não encontrado. Por favor, complete o cadastro do perfil.");

        var gallery = await _galleryRepository.GetGalleryGameByIdAsync(request.GalleryId);

        if (gallery == null)
            return OperationResult.Failure("Jogo não encontrado.");

        try
        {
            var cart = await _cartRepository.GetOrCreateByPlayerIdAsync(player.Id);
            var ownsItem = await _cartItemRepository.OwnsItemAsync(player.Id, gallery.Id, cart.Id);

            if (!ownsItem)
                await _cartItemRepository.AddAsync(player.Id, gallery.Id, cart.Id);

            return OperationResult.Success();
        }
        catch (Exception e)
        {
            return OperationResult.Failure(e.Message);
        }
    }

    public async Task<OperationResult> RemoveItemAsync(CartItemRequestDto request)
    {
        var cart = await _cartRepository.GetByPlayerIdAsync(request.PlayerId);
        if (cart == null)
            return OperationResult.Failure("Carrinho não encontrado.");

        await _cartItemRepository.RemoveAsync(cart.PlayerId, request.GalleryId, cart.Id);
        return OperationResult.Success();
    }

    public async Task<OperationResult<CartResponseDto>> GetCartByPlayerIdAsync(Guid playerId)
    {
        var cart = await _cartRepository.GetByPlayerIdAsync(playerId);
        if (cart == null)
            return OperationResult<CartResponseDto>.Success(new CartResponseDto
            {
                PlayerId = playerId,
                Items = []
            });

        var cartDto = CartMapper.ToDto(cart);

        // Busca os preços da galeria para cada item do carrinho
        foreach (var item in cartDto.Items)
        {
            var galleryGame = await _galleryRepository.GetByGameIdAsync(item.GameId);
            if (galleryGame != null)
            {
                item.Price = galleryGame.FinalPrice;
            }
        }

        return OperationResult<CartResponseDto>.Success(cartDto);
    }

    public async Task<OperationResult<CartResponseDto>> GetCartByUserIdAsync(Guid userId)
    {
        var player = await _playerRepository.GetByUserIdAsync(userId);
        if (player == null)
            return OperationResult<CartResponseDto>.Failure("Player não encontrado. Por favor, complete o cadastro do perfil.");

        var cart = await _cartRepository.GetByPlayerIdAsync(player.Id);
        if (cart == null)
            return OperationResult<CartResponseDto>.Success(new CartResponseDto
            {
                PlayerId = player.Id,
                Items = []
            });

        var cartDto = CartMapper.ToDto(cart);

        // Busca os preços da galeria para cada item do carrinho
        foreach (var item in cartDto.Items)
        {
            var galleryGame = await _galleryRepository.GetByGameIdAsync(item.GameId);
            if (galleryGame != null)
            {
                item.Price = galleryGame.FinalPrice;
            }
        }

        return OperationResult<CartResponseDto>.Success(cartDto);
    }

    public async Task<OperationResult> ClearCartAsync(Guid playerId)
    {
        var cart = await _cartRepository.GetByPlayerIdAsync(playerId);
        if (cart == null)
            return OperationResult.Failure("Carrinho não encontrado.");

        foreach (var item in cart.Items)
        {
            await _cartItemRepository.RemoveAsync(item.PlayerId, item.PlayerId, item.GalleryId);
        }

        return OperationResult.Success();
    }
}
