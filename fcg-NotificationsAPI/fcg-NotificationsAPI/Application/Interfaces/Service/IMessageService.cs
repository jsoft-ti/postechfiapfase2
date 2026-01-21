using Application.Dto.Result;

namespace Application.Interfaces.Service;

public interface IMessageService
{
     Task Handle();
     
}