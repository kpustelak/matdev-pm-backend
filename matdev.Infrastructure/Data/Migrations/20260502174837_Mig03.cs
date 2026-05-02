using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace matdev.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Mig03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskCategoryID",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskCategories",
                columns: table => new
                {
                    TaskCategoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategories", x => x.TaskCategoryID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskCategoryID",
                table: "Tasks",
                column: "TaskCategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskCategories_TaskCategoryID",
                table: "Tasks",
                column: "TaskCategoryID",
                principalTable: "TaskCategories",
                principalColumn: "TaskCategoryID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskCategories_TaskCategoryID",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "TaskCategories");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TaskCategoryID",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskCategoryID",
                table: "Tasks");
        }
    }
}
