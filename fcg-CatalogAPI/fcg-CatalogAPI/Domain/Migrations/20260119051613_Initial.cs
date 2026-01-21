using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fcg");

            migrationBuilder.CreateTable(
                name: "Games",
                schema: "fcg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EAN = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubTitle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Genre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "fcg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GalleryGames",
                schema: "fcg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    StartOf = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndOf = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryGames_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "fcg",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                schema: "fcg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "fcg",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                schema: "fcg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "fcg",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchases_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "fcg",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LibraryGames",
                schema: "fcg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GalleryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryGames_GalleryGames_GalleryId",
                        column: x => x.GalleryId,
                        principalSchema: "fcg",
                        principalTable: "GalleryGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "fcg",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                schema: "fcg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    GalleryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalSchema: "fcg",
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_GalleryGames_GalleryId",
                        column: x => x.GalleryId,
                        principalSchema: "fcg",
                        principalTable: "GalleryGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "fcg",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_GalleryId",
                schema: "fcg",
                table: "CartItems",
                columns: new[] { "CartId", "GalleryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_GalleryId",
                schema: "fcg",
                table: "CartItems",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_PlayerId",
                schema: "fcg",
                table: "CartItems",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_PlayerId",
                schema: "fcg",
                table: "Carts",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryGames_GameId",
                schema: "fcg",
                table: "GalleryGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_EAN",
                schema: "fcg",
                table: "Games",
                column: "EAN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryGames_GalleryId",
                schema: "fcg",
                table: "LibraryGames",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryGames_PlayerId",
                schema: "fcg",
                table: "LibraryGames",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserId",
                schema: "fcg",
                table: "Players",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_GameId",
                schema: "fcg",
                table: "Purchases",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PlayerId",
                schema: "fcg",
                table: "Purchases",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems",
                schema: "fcg");

            migrationBuilder.DropTable(
                name: "LibraryGames",
                schema: "fcg");

            migrationBuilder.DropTable(
                name: "Purchases",
                schema: "fcg");

            migrationBuilder.DropTable(
                name: "Carts",
                schema: "fcg");

            migrationBuilder.DropTable(
                name: "GalleryGames",
                schema: "fcg");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "fcg");

            migrationBuilder.DropTable(
                name: "Games",
                schema: "fcg");
        }
    }
}
