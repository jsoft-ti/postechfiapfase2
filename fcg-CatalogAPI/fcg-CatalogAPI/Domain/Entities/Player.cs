namespace Domain.Entities;

public class Player
{
    public Guid Id { get; private set; }
    public string DisplayName { get; private set; }
    public Guid UserId { get; private set; }
    public IReadOnlyCollection<LibraryGame> Library => _library.AsReadOnly();
    private readonly List<LibraryGame> _library;

    private Player()
    {
        DisplayName = string.Empty;
        _library = new List<LibraryGame>();
    }

    public Player(Guid userId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName não pode ser vazio.");

        Id = Guid.NewGuid();
        UserId = userId;
        DisplayName = displayName;
        _library = new List<LibraryGame>();
    }

    public void DisplayNameChange(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName não pode ser vazio.");
        DisplayName = displayName;
    }

    public LibraryGame AddGame(GalleryGame gallery)
    {
        if (_library.Any(g => g.Gallery.Game.Title == gallery.Game.Title))
            throw new InvalidOperationException("Jogo já existe na biblioteca.");

        var libraryGame = new LibraryGame(gallery.Id, this.Id, gallery.FinalPrice);
        _library.Add(libraryGame);
        return libraryGame;
    }
}