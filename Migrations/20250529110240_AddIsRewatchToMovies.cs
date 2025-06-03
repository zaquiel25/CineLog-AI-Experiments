using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzequielMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRewatchToMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRewatch",
                table: "Movies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRewatch",
                table: "Movies");
        }
    }
}
