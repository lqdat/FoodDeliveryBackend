using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDeliveryBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessageSoftDeletePerSide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCustomer",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByDriver",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedByCustomer",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "DeletedByDriver",
                table: "ChatMessages");
        }
    }
}
