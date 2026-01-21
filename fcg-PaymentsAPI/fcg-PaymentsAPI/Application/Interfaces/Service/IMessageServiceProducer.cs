using Application.Dto.Request;

namespace Application.Interfaces.Service;

public interface IMessageServiceProducer
{
    Task Handle(PaymentStatusDto message);
     
}