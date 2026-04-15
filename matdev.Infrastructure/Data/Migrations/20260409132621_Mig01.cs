using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace matdev.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Mig01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "LabOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabOrders_StatusID",
                table: "LabOrders",
                column: "StatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrders_LabOrderStatuses_StatusID",
                table: "LabOrders",
                column: "StatusID",
                principalTable: "LabOrderStatuses",
                principalColumn: "LabOrderStatusID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabOrders_LabOrderStatuses_StatusID",
                table: "LabOrders");

            migrationBuilder.DropIndex(
                name: "IX_LabOrders_StatusID",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "LabOrders");
        }
    }
}
