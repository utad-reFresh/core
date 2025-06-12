using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace utad.reFresh.core.Migrations
{
    /// <inheritdoc />
    public partial class recipeovrehaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_Recipes_RecipeId",
                table: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_RecipeId",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "Ingredients");

            migrationBuilder.RenameColumn(
                name: "PricePerServing",
                table: "Recipes",
                newName: "SpoonacularScore");

            migrationBuilder.RenameColumn(
                name: "Instructions",
                table: "Recipes",
                newName: "SpoonacularSourceUrl");

            migrationBuilder.AddColumn<int>(
                name: "AggregateLikes",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Cheap",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CookingMinutes",
                table: "Recipes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditsText",
                table: "Recipes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "Cuisines",
                table: "Recipes",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "DairyFree",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "Diets",
                table: "Recipes",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "DishTypes",
                table: "Recipes",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "GlutenFree",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "HealthScore",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Recipes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LowFodmap",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "Occasions",
                table: "Recipes",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "PreparationMinutes",
                table: "Recipes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReadyInMinutes",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Servings",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceName",
                table: "Recipes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Recipes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Sustainable",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Vegan",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Vegetarian",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VeryHealthy",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VeryPopular",
                table: "Recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WeightWatcherSmartPoints",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    IngredientId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NameClean = table.Column<string>(type: "text", nullable: false),
                    Original = table.Column<string>(type: "text", nullable: false),
                    AmountMetric = table.Column<double>(type: "double precision", nullable: false),
                    UnitShortMetric = table.Column<string>(type: "text", nullable: false),
                    UnitLongMetric = table.Column<string>(type: "text", nullable: false),
                    AmountImperial = table.Column<double>(type: "double precision", nullable: false),
                    UnitShortImperial = table.Column<string>(type: "text", nullable: false),
                    UnitLongImperial = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => new { x.RecipeId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Step = table.Column<string>(type: "text", nullable: false),
                    RecipeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_IngredientId",
                table: "RecipeIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId",
                table: "RecipeSteps",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "RecipeSteps");

            migrationBuilder.DropColumn(
                name: "AggregateLikes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Cheap",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CookingMinutes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CreditsText",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Cuisines",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DairyFree",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Diets",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DishTypes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "GlutenFree",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "HealthScore",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "LowFodmap",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Occasions",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "PreparationMinutes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ReadyInMinutes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Servings",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SourceName",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Sustainable",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Vegan",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Vegetarian",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "VeryHealthy",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "VeryPopular",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "WeightWatcherSmartPoints",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "SpoonacularSourceUrl",
                table: "Recipes",
                newName: "Instructions");

            migrationBuilder.RenameColumn(
                name: "SpoonacularScore",
                table: "Recipes",
                newName: "PricePerServing");

            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "Ingredients",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_RecipeId",
                table: "Ingredients",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_Recipes_RecipeId",
                table: "Ingredients",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }
    }
}
