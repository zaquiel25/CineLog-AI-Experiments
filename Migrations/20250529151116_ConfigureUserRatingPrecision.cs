using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzequielMovies.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureUserRatingPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UserRating",
                table: "Movies",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserRating",
                table: "Movies");
        }
    }
}
