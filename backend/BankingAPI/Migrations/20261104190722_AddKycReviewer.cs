using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddKycReviewer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewedBy",
                table: "KycDocuments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KycDocuments_ReviewedBy",
                table: "KycDocuments",
                column: "ReviewedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_KycDocuments_AspNetUsers_ReviewedBy",
                table: "KycDocuments",
                column: "ReviewedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KycDocuments_AspNetUsers_ReviewedBy",
                table: "KycDocuments");

            migrationBuilder.DropIndex(
                name: "IX_KycDocuments_ReviewedBy",
                table: "KycDocuments");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "KycDocuments");
        }
    }
}
