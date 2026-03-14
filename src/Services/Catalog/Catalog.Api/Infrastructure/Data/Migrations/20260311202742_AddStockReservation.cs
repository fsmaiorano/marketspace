using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "Catalogs",
                newName: "StockAvailable");

            migrationBuilder.AddColumn<int>(
                name: "StockReserved",
                table: "Catalogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Catalogs",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockReserved",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Catalogs");

            migrationBuilder.RenameColumn(
                name: "StockAvailable",
                table: "Catalogs",
                newName: "Stock");
        }
    }
}
