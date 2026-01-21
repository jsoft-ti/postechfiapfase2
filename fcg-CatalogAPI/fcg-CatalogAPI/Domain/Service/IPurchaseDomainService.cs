using Domain.Entities;

namespace Domain.Service;

public interface IPurchaseDomainService
{
    Purchase RegisterPurchase(Player player, GalleryGame game);
}
