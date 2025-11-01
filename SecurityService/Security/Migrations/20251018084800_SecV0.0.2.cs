using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Security.Migrations
{
    /// <inheritdoc />
    public partial class SecV002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "Security",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "Security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LocationId",
                schema: "Security",
                table: "AspNetUsers",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Locations_LocationId",
                schema: "Security",
                table: "AspNetUsers",
                column: "LocationId",
                principalSchema: "Security",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Locations_LocationId",
                schema: "Security",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "Security");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LocationId",
                schema: "Security",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "Security",
                table: "AspNetUsers");
        }
    }
}
