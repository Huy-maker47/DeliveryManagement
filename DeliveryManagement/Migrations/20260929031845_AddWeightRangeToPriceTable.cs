using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightRangeToPriceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxWeight",
                table: "PriceTables",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinWeight",
                table: "PriceTables",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxWeight",
                table: "PriceTables");

            migrationBuilder.DropColumn(
                name: "MinWeight",
                table: "PriceTables");
        }
    }
}
