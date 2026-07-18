using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MileageKm = table.Column<int>(type: "integer", nullable: false),
                    Transmission = table.Column<int>(type: "integer", nullable: false),
                    EngineDisplacementL = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    FuelType = table.Column<int>(type: "integer", nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ContactWechat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Country = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    City = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SellerType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SourcePlatform = table.Column<int>(type: "integer", nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PostTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PostContent = table.Column<string>(type: "text", nullable: false),
                    ImageUrls = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_CreatedAt",
                table: "Cars",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Manufacturer",
                table: "Cars",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_MileageKm",
                table: "Cars",
                column: "MileageKm");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Model",
                table: "Cars",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Price",
                table: "Cars",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_SellerType",
                table: "Cars",
                column: "SellerType");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_SourcePlatform",
                table: "Cars",
                column: "SourcePlatform");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_SourceUrl",
                table: "Cars",
                column: "SourceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Status",
                table: "Cars",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Year",
                table: "Cars",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
