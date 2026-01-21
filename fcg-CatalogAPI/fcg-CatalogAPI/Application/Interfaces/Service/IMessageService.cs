using Application.Dto.Request;

namespace Application.Interfaces.Service;

public interface IMessageService
{
     Task Handle(OrderPlacedEventRequestDto message);
     
}