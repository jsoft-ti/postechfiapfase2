using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Domain.Data.Contexts;

public class FcgDbContext : DbContext
{
    public FcgDbContext(DbContextOptions<FcgDbContext> options)
        : base(options) { }

    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<GalleryGame> GalleryGames { get; set; } = null!;
    public DbSet<LibraryGame> LibraryGames { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Purchase> Purchases { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("fcg");

        // --- Game ---
        builder.Entity<Game>(game =>
        {
            game.ToTable("Games");
            game.HasKey(g => g.Id);
            game.HasIndex(g => g.EAN).IsUnique();
        });

        // --- GalleryGame ---
        builder.Entity<GalleryGame>(gallery =>
        {
            gallery.ToTable("GalleryGames");
            gallery.HasKey(g => g.Id);

            gallery.HasOne(g => g.Game)
                .WithMany()
                .HasForeignKey(g => g.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            gallery.OwnsOne(g => g.Promotion, promo =>
            {
                promo.Property(p => p.Type).HasColumnName("Type");
                promo.Property(p => p.Value).HasColumnName("Value");
                promo.Property(p => p.StartOf).HasColumnName("StartOf");
                promo.Property(p => p.EndOf).HasColumnName("EndOf");
            });
        });

        // --- Player ---
        builder.Entity<Player>(player =>
        {
            player.ToTable("Players");
            player.HasKey(p => p.Id);
            player.HasIndex(p => p.UserId).IsUnique();
        });

        // --- LibraryGame ---
        builder.Entity<LibraryGame>(library =>
        {
            library.ToTable("LibraryGames");
            library.HasKey(l => l.Id);

            library.HasOne(l => l.Gallery)
                .WithMany()
                .HasForeignKey(l => l.GalleryId)
                .OnDelete(DeleteBehavior.Cascade);

            library.HasOne(l => l.Player)
                .WithMany(p => p.Library)
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- PurchaseGame ---
        // --- Purchase ---
        builder.Entity<Purchase>(purchase =>
        {
            purchase.ToTable("Purchases");

            // Chave prim�ria
            purchase.HasKey(p => p.Id);

            // Propriedades
            purchase.Property(p => p.PurchaseDate)
                .IsRequired();

            // Relacionamento com Game
            purchase.HasOne(p => p.Game)
                .WithMany() // Game n�o possui ICollection<Purchase>
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento com Player
            purchase.HasOne(p => p.Player)
                .WithMany() // Player n�o possui ICollection<Purchase>
                .HasForeignKey(p => p.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Cart ---
        builder.Entity<Cart>(cart =>
        {
            cart.ToTable("Carts");

            cart.HasKey(c => c.Id);

            cart.Property(c => c.PlayerId)
                .IsRequired();

            cart.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // --- CartItem ---
        builder.Entity<CartItem>(item =>
        {
            item.ToTable("CartItems");
            item.HasKey(i => i.Id);

            item.HasIndex(i => new { i.CartId, i.GalleryId }).IsUnique();

            item.HasOne(i => i.Gallery)
                .WithMany()
                .HasForeignKey(i => i.GalleryId)
                .OnDelete(DeleteBehavior.Cascade);

            item.HasOne(i => i.Player)
                .WithMany()
                .HasForeignKey(i => i.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}