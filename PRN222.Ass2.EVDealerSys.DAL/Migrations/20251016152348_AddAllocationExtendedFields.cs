using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN222.Ass2.EVDealerSys.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAllocationExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "VehicleAllocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "VehicleAllocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "VehicleAllocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DesiredDeliveryDate",
                table: "VehicleAllocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "VehicleAllocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedByUserId",
                table: "VehicleAllocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedColor",
                table: "VehicleAllocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipmentDate",
                table: "VehicleAllocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffSuggestion",
                table: "VehicleAllocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAllocations_ApprovedByUserId",
                table: "VehicleAllocations",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAllocations_RequestedByUserId",
                table: "VehicleAllocations",
                column: "RequestedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAllocations_Users_ApprovedByUserId",
                table: "VehicleAllocations",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleAllocations_Users_RequestedByUserId",
                table: "VehicleAllocations",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAllocations_Users_ApprovedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleAllocations_Users_RequestedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropIndex(
                name: "IX_VehicleAllocations_ApprovedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropIndex(
                name: "IX_VehicleAllocations_RequestedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "DesiredDeliveryDate",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "RequestedColor",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "ShipmentDate",
                table: "VehicleAllocations");

            migrationBuilder.DropColumn(
                name: "StaffSuggestion",
                table: "VehicleAllocations");
        }
    }
}
