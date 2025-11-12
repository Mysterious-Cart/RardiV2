using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customer.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Customer");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "Customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Locations = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                schema: "Customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VIN = table.Column<string>(type: "text", nullable: true),
                    Make = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerVehicleProfiles",
                schema: "Customer",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlateNumber = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerVehicleProfiles", x => new { x.CustomerId, x.VehicleId });
                    table.ForeignKey(
                        name: "FK_CustomerVehicleProfiles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Customer",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerVehicleProfiles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalSchema: "Customer",
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                schema: "Customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerVehicleProfileCustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerVehicleProfileVehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_CustomerVehicleProfiles_CustomerVehicleProfileCustome~",
                        columns: x => new { x.CustomerVehicleProfileCustomerId, x.CustomerVehicleProfileVehicleId },
                        principalSchema: "Customer",
                        principalTable: "CustomerVehicleProfiles",
                        principalColumns: new[] { "CustomerId", "VehicleId" });
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                schema: "Customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalSchema: "Customer",
                        principalTable: "Carts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                schema: "Customer",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerVehicleProfileCustomerId_CustomerVehicleProfi~",
                schema: "Customer",
                table: "Carts",
                columns: new[] { "CustomerVehicleProfileCustomerId", "CustomerVehicleProfileVehicleId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicleProfiles_VehicleId",
                schema: "Customer",
                table: "CustomerVehicleProfiles",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems",
                schema: "Customer");

            migrationBuilder.DropTable(
                name: "Carts",
                schema: "Customer");

            migrationBuilder.DropTable(
                name: "CustomerVehicleProfiles",
                schema: "Customer");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "Customer");

            migrationBuilder.DropTable(
                name: "Vehicles",
                schema: "Customer");
        }
    }
}
