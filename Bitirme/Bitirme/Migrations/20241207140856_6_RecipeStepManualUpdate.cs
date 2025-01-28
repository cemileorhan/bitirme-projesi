using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bitirme.Migrations
{
    /// <inheritdoc />
    public partial class _6_RecipeStepManualUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartVideo",
                table: "RecipeSteps",
                newName: "VideoStart");

            migrationBuilder.RenameColumn(
                name: "EndVideo",
                table: "RecipeSteps",
                newName: "VideoEnd");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoStart",
                table: "RecipeSteps",
                newName: "StartVideo");

            migrationBuilder.RenameColumn(
                name: "VideoEnd",
                table: "RecipeSteps",
                newName: "EndVideo");
        }
    }
}
