namespace Application.Interfaces;
public interface IGame
{
    int Id { get; set; }
    string? Description { get; }
    string? Genre { get; }
    decimal? Price { get; }
    abstract string GetDescription();
}
