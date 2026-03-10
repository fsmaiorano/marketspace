using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemsToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Items",
                table: "Payments",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Items",
                table: "Payments");
        }
    }
}
