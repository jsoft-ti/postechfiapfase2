using Domain.Entities;

namespace Application.Interfaces.Service;

public interface IMessageService
{
     Task Handle(UserRegisterResultDto message);
     
}