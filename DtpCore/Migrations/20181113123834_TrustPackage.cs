using Microsoft.EntityFrameworkCore.Migrations;

namespace DtpCore.Migrations
{
    public partial class TrustPackage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrustPackage",
                columns: table => new
                {
                    DatabaseID = table.Column<int>(nullable: false),
                    TrustID = table.Column<int>(nullable: false),
                    PackageID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustPackage", x => new { x.TrustID, x.PackageID });
                    table.ForeignKey(
                        name: "FK_TrustPackage_Package_PackageID",
                        column: x => x.PackageID,
                        principalTable: "Package",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrustPackage_Trust_TrustID",
                        column: x => x.TrustID,
                        principalTable: "Trust",
                        principalColumn: "DatabaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrustPackage_PackageID",
                table: "TrustPackage",
                column: "PackageID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustPackage");
        }
    }
}
