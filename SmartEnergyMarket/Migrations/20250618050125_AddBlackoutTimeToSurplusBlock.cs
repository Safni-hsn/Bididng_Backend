using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEnergyMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddBlackoutTimeToSurplusBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BidPrice",
                table: "SurplusBids",
                newName: "PricePerKwh");

            migrationBuilder.AddColumn<DateTime>(
                name: "BlackoutEndTime",
                table: "SurplusBlocks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BlackoutStartTime",
                table: "SurplusBlocks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "BlockId",
                table: "SurplusBids",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<double>(
                name: "BidAmountKwh",
                table: "SurplusBids",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BidTime",
                table: "SurplusBids",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_SurplusBids_BlockId",
                table: "SurplusBids",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_SurplusBids_UserId",
                table: "SurplusBids",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurplusBids_AspNetUsers_UserId",
                table: "SurplusBids",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurplusBids_SurplusBlocks_BlockId",
                table: "SurplusBids",
                column: "BlockId",
                principalTable: "SurplusBlocks",
                principalColumn: "BlockId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurplusBids_AspNetUsers_UserId",
                table: "SurplusBids");

            migrationBuilder.DropForeignKey(
                name: "FK_SurplusBids_SurplusBlocks_BlockId",
                table: "SurplusBids");

            migrationBuilder.DropIndex(
                name: "IX_SurplusBids_BlockId",
                table: "SurplusBids");

            migrationBuilder.DropIndex(
                name: "IX_SurplusBids_UserId",
                table: "SurplusBids");

            migrationBuilder.DropColumn(
                name: "BlackoutEndTime",
                table: "SurplusBlocks");

            migrationBuilder.DropColumn(
                name: "BlackoutStartTime",
                table: "SurplusBlocks");

            migrationBuilder.DropColumn(
                name: "BidAmountKwh",
                table: "SurplusBids");

            migrationBuilder.DropColumn(
                name: "BidTime",
                table: "SurplusBids");

            migrationBuilder.RenameColumn(
                name: "PricePerKwh",
                table: "SurplusBids",
                newName: "BidPrice");

            migrationBuilder.AlterColumn<int>(
                name: "BlockId",
                table: "SurplusBids",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
