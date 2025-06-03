using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzequielMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchedLocationToMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WatchedLocation",
                table: "Movies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WatchedLocation",
                table: "Movies");
        }
    }
}
