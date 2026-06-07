using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNzSchoolTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthorityClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LevelClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrgType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TerritorialAuthority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schools", x => x.Id);
                    table.UniqueConstraint("AK_schools_SchoolId", x => x.SchoolId);
                });

            migrationBuilder.CreateTable(
                name: "roll_ethnicity_fact",
                columns: table => new
                {
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    YearLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Ethnicity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentCount = table.Column<int>(type: "integer", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roll_ethnicity_fact", x => new { x.SchoolId, x.Year, x.YearLevel, x.Ethnicity });
                    table.ForeignKey(
                        name: "FK_roll_ethnicity_fact_schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "schools",
                        principalColumn: "SchoolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_roll_ethnicity_fact_SchoolId_Year",
                table: "roll_ethnicity_fact",
                columns: new[] { "SchoolId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_schools_AuthorityClass_LevelClass_Status",
                table: "schools",
                columns: new[] { "AuthorityClass", "LevelClass", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_schools_Region",
                table: "schools",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_schools_SchoolId",
                table: "schools",
                column: "SchoolId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "roll_ethnicity_fact");

            migrationBuilder.DropTable(
                name: "schools");
        }
    }
}
