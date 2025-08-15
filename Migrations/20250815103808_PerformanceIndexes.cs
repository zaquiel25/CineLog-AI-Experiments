using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzequielMovies.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_BlacklistedMovies_UserId",
                table: "BlacklistedMovies");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Movies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Genres",
                table: "Movies",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Director",
                table: "Movies",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "BlacklistedMovies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId_DateAdded",
                table: "WishlistItems",
                columns: new[] { "UserId", "DateAdded" });

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId_TmdbId",
                table: "WishlistItems",
                columns: new[] { "UserId", "TmdbId" });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_DateCreated",
                table: "Movies",
                columns: new[] { "UserId", "DateCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_DateWatched",
                table: "Movies",
                columns: new[] { "UserId", "DateWatched" },
                filter: "[DateWatched] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_Director",
                table: "Movies",
                columns: new[] { "UserId", "Director" },
                filter: "[Director] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_Genres",
                table: "Movies",
                columns: new[] { "UserId", "Genres" },
                filter: "[Genres] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_ReleasedYear",
                table: "Movies",
                columns: new[] { "UserId", "ReleasedYear" },
                filter: "[ReleasedYear] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_Title",
                table: "Movies",
                columns: new[] { "UserId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_TmdbId",
                table: "Movies",
                columns: new[] { "UserId", "TmdbId" },
                filter: "[TmdbId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId_UserRating",
                table: "Movies",
                columns: new[] { "UserId", "UserRating" },
                filter: "[UserRating] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedMovies_UserId_BlacklistedDate",
                table: "BlacklistedMovies",
                columns: new[] { "UserId", "BlacklistedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedMovies_UserId_Title",
                table: "BlacklistedMovies",
                columns: new[] { "UserId", "Title" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId_DateAdded",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId_TmdbId",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_DateCreated",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_DateWatched",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_Director",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_Genres",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_ReleasedYear",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_Title",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_TmdbId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_UserId_UserRating",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_BlacklistedMovies_UserId_BlacklistedDate",
                table: "BlacklistedMovies");

            migrationBuilder.DropIndex(
                name: "IX_BlacklistedMovies_UserId_Title",
                table: "BlacklistedMovies");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Genres",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Director",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "BlacklistedMovies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_UserId",
                table: "Movies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedMovies_UserId",
                table: "BlacklistedMovies",
                column: "UserId");
        }
    }
}
