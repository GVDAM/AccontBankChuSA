using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountsChu.API.Migrations
{
    public partial class Correçãoemrelacionamentodetransação : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_account",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_receiver_account",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_sender_account",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "idx_transaction_account_id",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "transaction");

            migrationBuilder.AddForeignKey(
                name: "fk_receiver_account",
                table: "transaction",
                column: "receiver_account_id",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sender_account",
                table: "transaction",
                column: "sender_account_id",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_receiver_account",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_sender_account",
                table: "transaction");

            migrationBuilder.AddColumn<int>(
                name: "account_id",
                table: "transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_transaction_account_id",
                table: "transaction",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "fk_account",
                table: "transaction",
                column: "account_id",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_receiver_account",
                table: "transaction",
                column: "receiver_account_id",
                principalTable: "customer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sender_account",
                table: "transaction",
                column: "sender_account_id",
                principalTable: "customer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
