using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace matdev.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetPlanLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetPlanLines",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanID = table.Column<int>(type: "integer", nullable: false),
                    CategoryID = table.Column<int>(type: "integer", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AlertThresholdPercent = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetPlanLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_BudgetPlanLines_BudgetCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "BudgetCategories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetPlanLines_BudgetPlans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "BudgetPlans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPlanLines_CategoryID",
                table: "BudgetPlanLines",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPlanLines_PlanID_CategoryID",
                table: "BudgetPlanLines",
                columns: new[] { "PlanID", "CategoryID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetPlanLines");
        }
    }
}
