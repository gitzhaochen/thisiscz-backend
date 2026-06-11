using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolEthnicityTotalLeavers2023Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsianTotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EuropeanPakehaTotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InternationalFeePayingTotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaoriTotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MelaaTotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OtherTotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PacificTotalLeavers2023",
                table: "schools",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsianTotalLeavers2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "EuropeanPakehaTotalLeavers2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "InternationalFeePayingTotalLeavers2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "MaoriTotalLeavers2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "MelaaTotalLeavers2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "OtherTotalLeavers2023",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "PacificTotalLeavers2023",
                table: "schools");
        }
    }
}
