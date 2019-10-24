using Microsoft.EntityFrameworkCore.Migrations;

namespace DtpCore.Migrations
{
    public partial class Icon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "IdentityMetadata",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "IdentityMetadata");
        }
    }
}
