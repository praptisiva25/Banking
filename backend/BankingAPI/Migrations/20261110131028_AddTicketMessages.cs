using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BankingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupportTicketId = table.Column<int>(type: "integer", nullable: false),
                    SenderUserId = table.Column<int>(type: "integer", nullable: false),
                    SenderRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessages_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketMessages_SupportTickets_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedOfficerId",
                table: "SupportTickets",
                column: "AssignedOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_SenderUserId",
                table: "TicketMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_SupportTicketId_SentAt",
                table: "TicketMessages",
                columns: new[] { "SupportTicketId", "SentAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_AspNetUsers_AssignedOfficerId",
                table: "SupportTickets",
                column: "AssignedOfficerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_AspNetUsers_UserId",
                table: "SupportTickets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_AspNetUsers_AssignedOfficerId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_AspNetUsers_UserId",
                table: "SupportTickets");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_AssignedOfficerId",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets");
        }
    }
}
