using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace utad.reFresh.core.Migrations
{
    /// <inheritdoc />
    public partial class off : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "RecipeIngredients");

            migrationBuilder.AddColumn<string>(
                name: "OpenFoodFactsProductSearchBarcode",
                table: "Ingredients",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OpenFoodFactsProductSearches",
                columns: table => new
                {
                    Barcode = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenFoodFactsProductSearches", x => x.Barcode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_OpenFoodFactsProductSearchBarcode",
                table: "Ingredients",
                column: "OpenFoodFactsProductSearchBarcode");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_OpenFoodFactsProductSearches_OpenFoodFactsProdu~",
                table: "Ingredients",
                column: "OpenFoodFactsProductSearchBarcode",
                principalTable: "OpenFoodFactsProductSearches",
                principalColumn: "Barcode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_OpenFoodFactsProductSearches_OpenFoodFactsProdu~",
                table: "Ingredients");

            migrationBuilder.DropTable(
                name: "OpenFoodFactsProductSearches");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_OpenFoodFactsProductSearchBarcode",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "OpenFoodFactsProductSearchBarcode",
                table: "Ingredients");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RecipeIngredients",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
