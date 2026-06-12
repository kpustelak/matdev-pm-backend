using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace matdev.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendLabOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinalReportFileName",
                table: "LabOrders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalReportLink",
                table: "LabOrders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PredictedCompletionDate",
                table: "LabOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestReportFileName",
                table: "LabOrders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestReportLink",
                table: "LabOrders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalReportFileName",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "FinalReportLink",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "PredictedCompletionDate",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "TestReportFileName",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "TestReportLink",
                table: "LabOrders");
        }
    }
}
