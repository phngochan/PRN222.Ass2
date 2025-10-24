using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN222.Ass2.EVDealerSys.DAL.Migrations
{
    public partial class AddManagerReviewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "ActivityLogs");

            migrationBuilder.AddColumn<string>(
                name: "ManagerNotes",
                table: "VehicleAllocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDate",
                table: "VehicleAllocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "VehicleAllocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAllocations_ReviewedByUserId",
                table: "VehicleAllocations",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAllocations_Users_ReviewedByUserId",
                table: "VehicleAllocations",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAllocations_Users_ReviewedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropIndex(
                name: "IX_VehicleAllocations_ReviewedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "ManagerNotes",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "ReviewDate",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "ActivityLogs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
