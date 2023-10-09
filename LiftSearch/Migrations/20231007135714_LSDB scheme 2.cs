using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LiftSearch.Migrations
{
    /// <inheritdoc />
    public partial class LSDBscheme2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Users_userdataId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_Travelers_travelerId",
                table: "Passengers");

            migrationBuilder.DropTable(
                name: "Travelers");

            migrationBuilder.RenameColumn(
                name: "userdataId",
                table: "Drivers",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_Drivers_userdataId",
                table: "Drivers",
                newName: "IX_Drivers_userId");

            migrationBuilder.AlterColumn<int>(
                name: "startTime",
                table: "Trips",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "price",
                table: "Trips",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "endTime",
                table: "Trips",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "Trips",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "startAdress",
                table: "Passengers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "endAdress",
                table: "Passengers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Drivers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "lastTrip",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "registeredSince",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Users_userId",
                table: "Drivers",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_Users_travelerId",
                table: "Passengers",
                column: "travelerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Users_userId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_Users_travelerId",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "description",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "lastTrip",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "registeredSince",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Drivers",
                newName: "userdataId");

            migrationBuilder.RenameIndex(
                name: "IX_Drivers_userId",
                table: "Drivers",
                newName: "IX_Drivers_userdataId");

            migrationBuilder.AlterColumn<int>(
                name: "startTime",
                table: "Trips",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "price",
                table: "Trips",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "endTime",
                table: "Trips",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "Trips",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "startAdress",
                table: "Passengers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "endAdress",
                table: "Passengers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Travelers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userdataId = table.Column<int>(type: "integer", nullable: false),
                    cancelledCount = table.Column<int>(type: "integer", nullable: false),
                    tripsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Travelers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Travelers_Users_userdataId",
                        column: x => x.userdataId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Travelers_userdataId",
                table: "Travelers",
                column: "userdataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Users_userdataId",
                table: "Drivers",
                column: "userdataId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_Travelers_travelerId",
                table: "Passengers",
                column: "travelerId",
                principalTable: "Travelers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
