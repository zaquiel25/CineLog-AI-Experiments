using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzequielMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddOverviewToMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Overview",
                table: "Movies");
        }
    }
}
