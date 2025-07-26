using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezequiel_Movies.Migrations
{
    /// <summary>
    /// Migration to add missing performance indexes for BlacklistedMovies and WishlistItems tables
    /// </summary>
    public partial class AddMissingPerformanceIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add indexes for WishlistItems table (only if they don't exist)
            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId_Title",
                table: "WishlistItems",
                columns: new[] { "UserId", "Title" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove indexes for WishlistItems table
            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId_Title",
                table: "WishlistItems");
        }
    }
}
