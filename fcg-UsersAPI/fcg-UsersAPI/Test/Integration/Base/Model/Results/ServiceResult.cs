using System.Text.Json.Serialization;

namespace Test.Integration.Base.Model.Results;


/// <summary>
/// Representa o resultado de uma opera��o de servi�o
/// Wrapper padr�o usado pela API para todas as respostas
/// </summary>
/// <typeparam name="T">Tipo de dado retornado</typeparam>
public class ServiceResult<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();
}
