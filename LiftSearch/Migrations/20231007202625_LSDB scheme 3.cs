using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiftSearch.Migrations
{
    /// <inheritdoc />
    public partial class LSDBscheme3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "luggage",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "smoking",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "luggage",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "pets",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "description",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "Users",
                newName: "tripsCountTraveler");

            migrationBuilder.RenameColumn(
                name: "setsCount",
                table: "Trips",
                newName: "tripStatus");

            migrationBuilder.RenameColumn(
                name: "pets",
                table: "Trips",
                newName: "seatsCount");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Trips",
                newName: "tripDate");

            migrationBuilder.RenameColumn(
                name: "comment",
                table: "Trips",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Passengers",
                newName: "registrationStatus");

            migrationBuilder.RenameColumn(
                name: "tripsCount",
                table: "Drivers",
                newName: "tripsCountDriver");

            migrationBuilder.RenameColumn(
                name: "registeredSince",
                table: "Drivers",
                newName: "registeredDriverDate");

            migrationBuilder.RenameColumn(
                name: "lastTrip",
                table: "Drivers",
                newName: "lastTripDate");

            migrationBuilder.RenameColumn(
                name: "cancelledCount",
                table: "Drivers",
                newName: "cancelledCountDriver");

            migrationBuilder.AddColumn<int>(
                name: "cancelledCountTraveler",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "driverStatus",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "registrationDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "lastEditTime",
                table: "Trips",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "Passengers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "driverBio",
                table: "Drivers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancelledCountTraveler",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "driverStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "registrationDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "lastEditTime",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "driverBio",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "tripsCountTraveler",
                table: "Users",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "tripStatus",
                table: "Trips",
                newName: "setsCount");

            migrationBuilder.RenameColumn(
                name: "tripDate",
                table: "Trips",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "seatsCount",
                table: "Trips",
                newName: "pets");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Trips",
                newName: "comment");

            migrationBuilder.RenameColumn(
                name: "registrationStatus",
                table: "Passengers",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "tripsCountDriver",
                table: "Drivers",
                newName: "tripsCount");

            migrationBuilder.RenameColumn(
                name: "registeredDriverDate",
                table: "Drivers",
                newName: "registeredSince");

            migrationBuilder.RenameColumn(
                name: "lastTripDate",
                table: "Drivers",
                newName: "lastTrip");

            migrationBuilder.RenameColumn(
                name: "cancelledCountDriver",
                table: "Drivers",
                newName: "cancelledCount");

            migrationBuilder.AddColumn<int>(
                name: "luggage",
                table: "Trips",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "smoking",
                table: "Trips",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "Passengers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "luggage",
                table: "Passengers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "pets",
                table: "Passengers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Drivers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
