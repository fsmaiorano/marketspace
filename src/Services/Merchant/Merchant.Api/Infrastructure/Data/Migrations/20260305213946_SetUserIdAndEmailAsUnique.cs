using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Merchant.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetUserIdAndEmailAsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Merchants_Email",
                table: "Merchants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_UserId",
                table: "Merchants",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Merchants_Email",
                table: "Merchants");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_UserId",
                table: "Merchants");
        }
    }
}
