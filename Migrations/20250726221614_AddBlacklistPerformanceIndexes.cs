using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzequielMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddBlacklistPerformanceIndexes : Migration
    {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_BlacklistedMovies_UserId_TmdbId",
            table: "BlacklistedMovies",
            columns: new[] { "UserId", "TmdbId" });
    }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BlacklistedMovies_UserId_TmdbId",
                table: "BlacklistedMovies");
        }
    }
}
