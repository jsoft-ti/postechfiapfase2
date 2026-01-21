using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IGalleryRepository
{
    Task<bool> OwnsGalleryGameAsync(string EAN);
    Task<bool> OwnsGalleryGameAsync(Guid id);
    Task<GalleryGame?> GetGalleryGameByIdAsync(Guid id);
    Task<GalleryGame?> GetByGameIdAsync(Guid gameId);
    Task<IEnumerable<GalleryGame>> GetAllGalleryGamesAsync();
    Task<GalleryGame> AddToGalleryGameAsync(GalleryGame game);
    Task UpdateGalleryGameAsync(GalleryGame game);
    Task RemoveFromGalleryGameAsync(GalleryGame game);
    
    // Promotion and price operations
    Task<IEnumerable<GalleryGame>> GetPromotionalGalleryGameAsync();
    Task<IEnumerable<GalleryGame>> GetGalleryGameByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<bool> IsAvailableForPurchaseAsync(Guid id);
    
    // Filtering and search
    Task<IEnumerable<GalleryGame>> GetGalleryGamesByGenreAsync(string genre);
    Task<IEnumerable<GalleryGame>> SearchGalleryGamesAsync(string searchTerm);
    
    Task<int> SaveChangesAsync();
}