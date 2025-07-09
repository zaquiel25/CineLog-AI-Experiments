using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzequielMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddPosterUrlToBlacklistedMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PosterUrl",
                table: "BlacklistedMovies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PosterUrl",
                table: "BlacklistedMovies");
        }
    }
}
