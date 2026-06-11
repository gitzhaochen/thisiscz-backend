using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class MoveTertiaryProgressionToSeparateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "school_tertiary_progression",
                columns: table => new
                {
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    TotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    TotalUniversity = table.Column<int>(type: "integer", nullable: true),
                    AsianUniversity = table.Column<int>(type: "integer", nullable: true),
                    EuropeanPakehaUniversity = table.Column<int>(type: "integer", nullable: true),
                    MaoriUniversity = table.Column<int>(type: "integer", nullable: true),
                    PacificUniversity = table.Column<int>(type: "integer", nullable: true),
                    MelaaUniversity = table.Column<int>(type: "integer", nullable: true),
                    OtherUniversity = table.Column<int>(type: "integer", nullable: true),
                    InternationalFeePayingUniversity = table.Column<int>(type: "integer", nullable: true),
                    AsianTotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    EuropeanPakehaTotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    MaoriTotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    PacificTotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    MelaaTotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    OtherTotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    InternationalFeePayingTotalLeavers = table.Column<int>(type: "integer", nullable: true),
                    UeRate = table.Column<double>(type: "double precision", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_tertiary_progression", x => new { x.SchoolId, x.Year });
                    table.ForeignKey(
                        name: "FK_school_tertiary_progression_schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "schools",
                        principalColumn: "SchoolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_school_tertiary_progression_Year",
                table: "school_tertiary_progression",
                column: "Year");

            migrationBuilder.Sql(
                """
                INSERT INTO school_tertiary_progression (
                    "SchoolId",
                    "Year",
                    "TotalLeavers",
                    "TotalUniversity",
                    "AsianUniversity",
                    "EuropeanPakehaUniversity",
                    "MaoriUniversity",
                    "PacificUniversity",
                    "MelaaUniversity",
                    "OtherUniversity",
                    "InternationalFeePayingUniversity",
                    "AsianTotalLeavers",
                    "EuropeanPakehaTotalLeavers",
                    "MaoriTotalLeavers",
                    "PacificTotalLeavers",
                    "MelaaTotalLeavers",
                    "OtherTotalLeavers",
                    "InternationalFeePayingTotalLeavers",
                    "UeRate",
                    "UpdatedAt"
                )
                SELECT
                    "SchoolId",
                    2023,
                    "TotalLeavers2023",
                    "TotalUniversity2023",
                    "AsianUniversity2023",
                    "EuropeanPakehaUniversity2023",
                    "MaoriUniversity2023",
                    "PacificUniversity2023",
                    "MelaaUniversity2023",
                    "OtherUniversity2023",
                    "InternationalFeePayingUniversity2023",
                    "AsianTotalLeavers2023",
                    "EuropeanPakehaTotalLeavers2023",
                    "MaoriTotalLeavers2023",
                    "PacificTotalLeavers2023",
                    "MelaaTotalLeavers2023",
                    "OtherTotalLeavers2023",
                    "InternationalFeePayingTotalLeavers2023",
                    "UeRate",
                    "UpdatedAt"
                FROM schools
                WHERE "TotalLeavers2023" IS NOT NULL
                   OR "TotalUniversity2023" IS NOT NULL
                   OR "AsianUniversity2023" IS NOT NULL
                   OR "EuropeanPakehaUniversity2023" IS NOT NULL
                   OR "MaoriUniversity2023" IS NOT NULL
                   OR "PacificUniversity2023" IS NOT NULL
                   OR "MelaaUniversity2023" IS NOT NULL
                   OR "OtherUniversity2023" IS NOT NULL
                   OR "InternationalFeePayingUniversity2023" IS NOT NULL
                   OR "AsianTotalLeavers2023" IS NOT NULL
                   OR "EuropeanPakehaTotalLeavers2023" IS NOT NULL
                   OR "MaoriTotalLeavers2023" IS NOT NULL
                   OR "PacificTotalLeavers2023" IS NOT NULL
                   OR "MelaaTotalLeavers2023" IS NOT NULL
                   OR "OtherTotalLeavers2023" IS NOT NULL
                   OR "InternationalFeePayingTotalLeavers2023" IS NOT NULL
                   OR "UeRate" IS NOT NULL;
                """
            );

            migrationBuilder.DropColumn(name: "AsianTotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "AsianUniversity2023", table: "schools");
            migrationBuilder.DropColumn(name: "EuropeanPakehaTotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "EuropeanPakehaUniversity2023", table: "schools");
            migrationBuilder.DropColumn(name: "InternationalFeePayingTotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "InternationalFeePayingUniversity2023", table: "schools");
            migrationBuilder.DropColumn(name: "MaoriTotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "MaoriUniversity2023", table: "schools");
            migrationBuilder.DropColumn(name: "MelaaTotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "MelaaUniversity2023", table: "schools");
            migrationBuilder.DropColumn(name: "OtherTotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "OtherUniversity2023", table: "schools");
            migrationBuilder.DropColumn(name: "PacificTotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "PacificUniversity2023", table: "schools");
            migrationBuilder.DropColumn(name: "TotalLeavers2023", table: "schools");
            migrationBuilder.DropColumn(name: "TotalUniversity2023", table: "schools");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "AsianTotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "AsianUniversity2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "EuropeanPakehaTotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "EuropeanPakehaUniversity2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "InternationalFeePayingTotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "InternationalFeePayingUniversity2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "MaoriTotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "MaoriUniversity2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "MelaaTotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "MelaaUniversity2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "OtherTotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "OtherUniversity2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "PacificTotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "PacificUniversity2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "TotalLeavers2023", table: "schools", type: "integer", nullable: true);
            migrationBuilder.AddColumn<int>(name: "TotalUniversity2023", table: "schools", type: "integer", nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE schools AS s
                SET
                    "TotalLeavers2023" = p."TotalLeavers",
                    "TotalUniversity2023" = p."TotalUniversity",
                    "AsianUniversity2023" = p."AsianUniversity",
                    "EuropeanPakehaUniversity2023" = p."EuropeanPakehaUniversity",
                    "MaoriUniversity2023" = p."MaoriUniversity",
                    "PacificUniversity2023" = p."PacificUniversity",
                    "MelaaUniversity2023" = p."MelaaUniversity",
                    "OtherUniversity2023" = p."OtherUniversity",
                    "InternationalFeePayingUniversity2023" = p."InternationalFeePayingUniversity",
                    "AsianTotalLeavers2023" = p."AsianTotalLeavers",
                    "EuropeanPakehaTotalLeavers2023" = p."EuropeanPakehaTotalLeavers",
                    "MaoriTotalLeavers2023" = p."MaoriTotalLeavers",
                    "PacificTotalLeavers2023" = p."PacificTotalLeavers",
                    "MelaaTotalLeavers2023" = p."MelaaTotalLeavers",
                    "OtherTotalLeavers2023" = p."OtherTotalLeavers",
                    "InternationalFeePayingTotalLeavers2023" = p."InternationalFeePayingTotalLeavers"
                FROM school_tertiary_progression AS p
                WHERE p."SchoolId" = s."SchoolId"
                  AND p."Year" = 2023;
                """
            );

            migrationBuilder.DropTable(name: "school_tertiary_progression");
        }
    }
}
