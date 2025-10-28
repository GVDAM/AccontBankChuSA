using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountsChu.API.Migrations
{
    public partial class correçaoemnomenclaturadecoluna : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "transaction",
                newName: "account_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "transaction",
                newName: "AccountId");
        }
    }
}
