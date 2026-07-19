using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCarPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Cars",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Cars"
                SET "PublicId" = SUBSTRING(MD5(RANDOM()::text || CLOCK_TIMESTAMP()::text || "Id"::text), 1, 32)
                WHERE "PublicId" IS NULL OR "PublicId" = '';
                """
            );

            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "Cars",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_PublicId",
                table: "Cars",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cars_PublicId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Cars");
        }
    }
}
