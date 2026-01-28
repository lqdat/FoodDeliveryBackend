using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDeliveryBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminApprovalSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    RegionCode = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "approval_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false),
                    RegionCode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chain_owner_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    BusinessName = table.Column<string>(type: "text", nullable: false),
                    BusinessRegistrationNumber = table.Column<string>(type: "text", nullable: true),
                    RegionCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chain_owner_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "approval_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformerRole = table.Column<int>(type: "integer", nullable: true),
                    PerformerAccountType = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_logs_approval_requests_ApprovalRequestId",
                        column: x => x.ApprovalRequestId,
                        principalTable: "approval_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "store_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChainOwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    RegionCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_store_accounts_chain_owner_accounts_ChainOwnerId",
                        column: x => x.ChainOwnerId,
                        principalTable: "chain_owner_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "foods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_foods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_foods_store_accounts_StoreAccountId",
                        column: x => x.StoreAccountId,
                        principalTable: "store_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "store_managers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_managers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_store_managers_store_accounts_StoreAccountId",
                        column: x => x.StoreAccountId,
                        principalTable: "store_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAccounts_Email",
                table: "admin_accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminAccounts_RegionCode",
                table: "admin_accounts",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLogs_CreatedAt",
                table: "approval_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLogs_RequestId",
                table: "approval_logs",
                column: "ApprovalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_Entity",
                table: "approval_requests",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_RegionCode",
                table: "approval_requests",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_Status",
                table: "approval_requests",
                column: "CurrentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ChainOwnerAccounts_Email",
                table: "chain_owner_accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChainOwnerAccounts_RegionCode",
                table: "chain_owner_accounts",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_ChainOwnerAccounts_Status",
                table: "chain_owner_accounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_Status",
                table: "foods",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_StoreAccountId",
                table: "foods",
                column: "StoreAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreAccounts_ChainOwnerId",
                table: "store_accounts",
                column: "ChainOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreAccounts_RegionCode",
                table: "store_accounts",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_StoreAccounts_Status",
                table: "store_accounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StoreManagers_Email",
                table: "store_managers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_StoreManagers_Status",
                table: "store_managers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StoreManagers_StoreAccountId",
                table: "store_managers",
                column: "StoreAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_accounts");

            migrationBuilder.DropTable(
                name: "approval_logs");

            migrationBuilder.DropTable(
                name: "foods");

            migrationBuilder.DropTable(
                name: "store_managers");

            migrationBuilder.DropTable(
                name: "approval_requests");

            migrationBuilder.DropTable(
                name: "store_accounts");

            migrationBuilder.DropTable(
                name: "chain_owner_accounts");
        }
    }
}
