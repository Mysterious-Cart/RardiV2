using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Payment");

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                schema: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CartID = table.Column<Guid>(type: "uuid", nullable: true),
                    PlateNumber = table.Column<string>(type: "text", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    TransactBy = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentTypeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalSchema: "Payment",
                        principalTable: "PaymentTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentInfos",
                schema: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentInfos_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalSchema: "Payment",
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentInfos_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Payment",
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransactionItems",
                schema: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionItems_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Payment",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_PaymentTypeId",
                schema: "Payment",
                table: "PaymentInfos",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_TransactionId",
                schema: "Payment",
                table: "PaymentInfos",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_TransactionId",
                schema: "Payment",
                table: "TransactionItems",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PaymentTypeId",
                schema: "Payment",
                table: "Transactions",
                column: "PaymentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentInfos",
                schema: "Payment");

            migrationBuilder.DropTable(
                name: "TransactionItems",
                schema: "Payment");

            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "Payment");

            migrationBuilder.DropTable(
                name: "PaymentTypes",
                schema: "Payment");
        }
    }
}
