using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDeliveryBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationImageAndActionUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Notifications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Notifications");
        }
    }
}
