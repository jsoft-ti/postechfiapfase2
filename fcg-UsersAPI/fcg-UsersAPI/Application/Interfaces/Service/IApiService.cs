namespace Application.Interfaces.Service;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data);
}
