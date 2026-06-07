using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolExtraColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "schools",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressSuburb",
                table: "schools",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoEdStatus",
                table: "schools",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EqiIndex",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalStudents",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "schools",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "AddressSuburb",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "CoEdStatus",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "EqiIndex",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "TotalStudents",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "schools");
        }
    }
}
