using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(EAN), IsUnique = true)]
public class Game
{
    public Guid Id { get; private set; }

    [Required]
    public string EAN { get; private set; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "O título do jogo deve ter entre {2} e {1} caracteres.")]
    public string Title { get; private set; } = string.Empty;

    [StringLength(50, MinimumLength = 1, ErrorMessage = "O subtítulo deve ter entre {2} e {1} caracteres.")]
    public string? SubTitle { get; private set; }

    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "O gênero deve ter entre {2} e {1} caracteres.")]
    public string Genre { get; private set; } = string.Empty;

    [StringLength(500, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre {2} e {1} caracteres.")]
    public string? Description { get; private set; }

    public Decimal? Price { get; set; } = new decimal(0.0);

    protected Game() { }

    public Game(string ean, string title, string genre, string? description, decimal? price, string? subTitle = null)
    {
        if (string.IsNullOrWhiteSpace(ean)) throw new ArgumentException("EAN obrigatório");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Título obrigatório");
        if (string.IsNullOrWhiteSpace(genre)) throw new ArgumentException("Gênero obrigatório");

        Id = Guid.NewGuid();
        EAN = ean;
        Title = title;
        Genre = genre;
        Description = description;
        SubTitle = subTitle;
        Price = price;
    }

    public void Update(string ean, string title, string genre, string? description, decimal? price, string? subTitle = null)
    {
        if (string.IsNullOrWhiteSpace(ean)) throw new ArgumentException("EAN obrigatório");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Título obrigatório");
        if (string.IsNullOrWhiteSpace(genre)) throw new ArgumentException("Gênero obrigatório");

        this.EAN = ean;
        this.Title = title;
        this.Genre = genre;
        this.Description = description;
        this.SubTitle = subTitle;
        Price = price;
    }

    public string GetDescription()
    {
        var baseTitle = string.IsNullOrWhiteSpace(SubTitle) ? Title : $"{Title}: {SubTitle} : {Price}";
        return $"{baseTitle} ({Description})";
    }
}
