using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace matdev.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskBudgetLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "Tasks",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskID",
                table: "BudgetExpenditures",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetExpenditures_TaskID",
                table: "BudgetExpenditures",
                column: "TaskID");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetExpenditures_Tasks_TaskID",
                table: "BudgetExpenditures",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "TaskID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetExpenditures_Tasks_TaskID",
                table: "BudgetExpenditures");

            migrationBuilder.DropIndex(
                name: "IX_BudgetExpenditures_TaskID",
                table: "BudgetExpenditures");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskID",
                table: "BudgetExpenditures");
        }
    }
}
