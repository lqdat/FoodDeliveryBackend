using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDeliveryBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDriverDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CriminalRecordUrl",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseNumber",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceExpiryDate",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceUrl",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseExpiryDate",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationExpiryDate",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriminalRecordUrl",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DriverLicenseNumber",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiryDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "InsuranceUrl",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LicenseExpiryDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "RegistrationExpiryDate",
                table: "Drivers");
        }
    }
}
