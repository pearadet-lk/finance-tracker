using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "DeletedAt", "Icon", "IsDeleted", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "#10b981", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(2610), null, "💼", false, "Salary", "Income", null },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "#3b82f6", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(5389), null, "💻", false, "Freelance", "Income", null },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), "#f59e0b", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(5409), null, "🍽️", false, "Food & Dining", "Expense", null },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), "#8b5cf6", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(5413), null, "🚗", false, "Transport", "Expense", null },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), "#ec4899", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(5416), null, "🛍️", false, "Shopping", "Expense", null },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), "#ef4444", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(5424), null, "⚡", false, "Utilities", "Expense", null },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), "#06b6d4", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(5439), null, "🏥", false, "Healthcare", "Expense", null },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), "#f97316", new DateTime(2026, 3, 31, 3, 57, 27, 894, DateTimeKind.Utc).AddTicks(5442), null, "🎮", false, "Entertainment", "Expense", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
