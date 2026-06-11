using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolTertiaryProgression2023Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsianUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EuropeanPakehaUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InternationalFeePayingUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaoriUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MelaaUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OtherUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PacificUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalUniversity2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UeRate",
                table: "schools",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsianUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "EuropeanPakehaUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "InternationalFeePayingUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "MaoriUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "MelaaUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "OtherUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "PacificUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "TotalLeavers2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "TotalUniversity2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "UeRate",
                table: "schools");
        }
    }
}
