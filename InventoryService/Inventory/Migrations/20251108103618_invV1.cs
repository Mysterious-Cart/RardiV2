using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Migrations
{
    /// <inheritdoc />
    public partial class invV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Inventory");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    EnglishNormalizedName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TotalStock = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryProductEntity",
                schema: "Inventory",
                columns: table => new
                {
                    CategoriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryProductEntity", x => new { x.CategoriesId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_CategoryProductEntity_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalSchema: "Inventory",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryProductEntity_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductProfiles",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    SKU = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductProfiles_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTransfers",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTransfers_Location_FromLocationId",
                        column: x => x.FromLocationId,
                        principalSchema: "Inventory",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTransfers_Location_ToLocationId",
                        column: x => x.ToLocationId,
                        principalSchema: "Inventory",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTransfers_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSupplierProfiles",
                schema: "Inventory",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    LatestImportPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    isDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSupplierProfiles", x => new { x.ProductId, x.SupplierId });
                    table.ForeignKey(
                        name: "FK_ProductSupplierProfiles_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSupplierProfiles_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "Inventory",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductSupplierProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductProfileProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductProfileSupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ImportPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_ProductSupplierProfiles_ProductProfileProductId_Prod~",
                        columns: x => new { x.ProductProfileProductId, x.ProductProfileSupplierId },
                        principalSchema: "Inventory",
                        principalTable: "ProductSupplierProfiles",
                        principalColumns: new[] { "ProductId", "SupplierId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryProductEntity_ProductsId",
                schema: "Inventory",
                table: "CategoryProductEntity",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductProfileProductId_ProductProfileSupplierId",
                schema: "Inventory",
                table: "Orders",
                columns: new[] { "ProductProfileProductId", "ProductProfileSupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductProfiles_ProductId",
                schema: "Inventory",
                table: "ProductProfiles",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSupplierProfiles_SupplierId",
                schema: "Inventory",
                table: "ProductSupplierProfiles",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTransfers_FromLocationId",
                schema: "Inventory",
                table: "ProductTransfers",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTransfers_ProductId",
                schema: "Inventory",
                table: "ProductTransfers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTransfers_ToLocationId",
                schema: "Inventory",
                table: "ProductTransfers",
                column: "ToLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryProductEntity",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProductProfiles",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProductTransfers",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProductSupplierProfiles",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "Inventory");
        }
    }
}
