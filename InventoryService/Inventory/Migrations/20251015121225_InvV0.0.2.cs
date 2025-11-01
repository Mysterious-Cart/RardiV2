using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Migrations
{
    /// <inheritdoc />
    public partial class InvV002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImports_Products_ProductId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImports_Suppliers_SupplierId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProfiles_ProductSupplierProfiles_ProductSupplierProf~",
                schema: "Inventory",
                table: "ProductProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ProductProfiles_ProductSupplierProfilesProductId_ProductSup~",
                schema: "Inventory",
                table: "ProductProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ProductImports_ProductId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropIndex(
                name: "IX_ProductImports_SupplierId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropColumn(
                name: "ProductSupplierProfilesProductId",
                schema: "Inventory",
                table: "ProductProfiles");

            migrationBuilder.DropColumn(
                name: "ProductSupplierProfilesSupplierId",
                schema: "Inventory",
                table: "ProductProfiles");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.RenameColumn(
                name: "ImportPrice",
                schema: "Inventory",
                table: "ProductSupplierProfiles",
                newName: "LatestImportPrice");

            migrationBuilder.RenameColumn(
                name: "Stock",
                schema: "Inventory",
                table: "Products",
                newName: "TotalStock");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                schema: "Inventory",
                table: "ProductImports",
                newName: "ProductSupplierProfileId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Inventory",
                table: "ProductSupplierProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductProfileProductId",
                schema: "Inventory",
                table: "ProductImports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductProfileSupplierId",
                schema: "Inventory",
                table: "ProductImports",
                type: "uuid",
                nullable: true);

            migrationBuilder.RenameTable(
                name: "ProductImports",
                schema: "Inventory",
                newName: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductProfileProductId_ProductProfileSupplierId",
                schema: "Inventory",
                table: "Orders",
                columns: new[] { "ProductProfileProductId", "ProductProfileSupplierId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductSupplierProfiles_ProductProfileProduc~",
                schema: "Inventory",
                table: "Orders",
                columns: new[] { "ProductProfileProductId", "ProductProfileSupplierId" },
                principalSchema: "Inventory",
                principalTable: "ProductSupplierProfiles",
                principalColumns: new[] { "ProductId", "SupplierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImports_ProductSupplierProfiles_ProductProfileProduc~",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropIndex(
                name: "IX_ProductImports_ProductProfileProductId_ProductProfileSuppli~",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Inventory",
                table: "ProductSupplierProfiles");

            migrationBuilder.DropColumn(
                name: "ProductProfileProductId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.DropColumn(
                name: "ProductProfileSupplierId",
                schema: "Inventory",
                table: "ProductImports");

            migrationBuilder.RenameColumn(
                name: "LatestImportPrice",
                schema: "Inventory",
                table: "ProductSupplierProfiles",
                newName: "ImportPrice");

            migrationBuilder.RenameColumn(
                name: "TotalStock",
                schema: "Inventory",
                table: "Products",
                newName: "Stock");

            migrationBuilder.RenameColumn(
                name: "ProductSupplierProfileId",
                schema: "Inventory",
                table: "ProductImports",
                newName: "SupplierId");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductSupplierProfilesProductId",
                schema: "Inventory",
                table: "ProductProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductSupplierProfilesSupplierId",
                schema: "Inventory",
                table: "ProductProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "Inventory",
                table: "ProductImports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                schema: "Inventory",
                table: "ProductImports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProductProfiles_ProductSupplierProfilesProductId_ProductSup~",
                schema: "Inventory",
                table: "ProductProfiles",
                columns: new[] { "ProductSupplierProfilesProductId", "ProductSupplierProfilesSupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImports_ProductId",
                schema: "Inventory",
                table: "ProductImports",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImports_SupplierId",
                schema: "Inventory",
                table: "ProductImports",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImports_Products_ProductId",
                schema: "Inventory",
                table: "ProductImports",
                column: "ProductId",
                principalSchema: "Inventory",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImports_Suppliers_SupplierId",
                schema: "Inventory",
                table: "ProductImports",
                column: "SupplierId",
                principalSchema: "Inventory",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProfiles_ProductSupplierProfiles_ProductSupplierProf~",
                schema: "Inventory",
                table: "ProductProfiles",
                columns: new[] { "ProductSupplierProfilesProductId", "ProductSupplierProfilesSupplierId" },
                principalSchema: "Inventory",
                principalTable: "ProductSupplierProfiles",
                principalColumns: new[] { "ProductId", "SupplierId" });
        }
    }
}
