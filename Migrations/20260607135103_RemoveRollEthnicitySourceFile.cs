using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRollEthnicitySourceFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceFile",
                table: "roll_ethnicity_fact");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceFile",
                table: "roll_ethnicity_fact",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
