using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiftSearch.Migrations
{
    /// <inheritdoc />
    public partial class LSDBscheme4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tripsCountTraveler",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "tripsCountDriver",
                table: "Drivers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "tripsCountTraveler",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "tripsCountDriver",
                table: "Drivers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
