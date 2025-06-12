using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace utad.reFresh.core.Migrations
{
    /// <inheritdoc />
    public partial class ingredientsearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngredientSearches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Query = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientSearches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IngredientIngredientSearch",
                columns: table => new
                {
                    IngredientSearchesId = table.Column<int>(type: "integer", nullable: false),
                    IngredientsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientIngredientSearch", x => new { x.IngredientSearchesId, x.IngredientsId });
                    table.ForeignKey(
                        name: "FK_IngredientIngredientSearch_IngredientSearches_IngredientSea~",
                        column: x => x.IngredientSearchesId,
                        principalTable: "IngredientSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientIngredientSearch_Ingredients_IngredientsId",
                        column: x => x.IngredientsId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientIngredientSearch_IngredientsId",
                table: "IngredientIngredientSearch",
                column: "IngredientsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientIngredientSearch");

            migrationBuilder.DropTable(
                name: "IngredientSearches");
        }
    }
}
