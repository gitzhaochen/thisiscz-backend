using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThisisczApi.Migrations
{
    /// <inheritdoc />
    public partial class ShortenCarPublicIdTo16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Cars"
                SET "PublicId" = SUBSTRING(
                  MD5(COALESCE("PublicId", '') || ':' || "Id"::text || ':' || RANDOM()::text || ':' || CLOCK_TIMESTAMP()::text),
                  1,
                  16
                )
                WHERE "PublicId" IS NULL OR LENGTH("PublicId") <> 16;
                """
            );

            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "Cars",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "Cars",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);
        }
    }
}
