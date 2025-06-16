using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartEnergyMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddSurplusBlockFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SurplusBlocks",
                table: "SurplusBlocks");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SurplusBlocks");

            migrationBuilder.RenameColumn(
                name: "MinBidPrice",
                table: "SurplusBlocks",
                newName: "MinBidPricePerKwh");

            migrationBuilder.RenameColumn(
                name: "EnergyKWh",
                table: "SurplusBlocks",
                newName: "BlockSizeKwh");

            migrationBuilder.AddColumn<string>(
                name: "BlockId",
                table: "SurplusBlocks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AllocatedToUserId",
                table: "SurplusBlocks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AvailableEnergyKwh",
                table: "SurplusBlocks",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockTime",
                table: "SurplusBlocks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurplusBlocks",
                table: "SurplusBlocks",
                column: "BlockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SurplusBlocks",
                table: "SurplusBlocks");

            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "SurplusBlocks");

            migrationBuilder.DropColumn(
                name: "AllocatedToUserId",
                table: "SurplusBlocks");

            migrationBuilder.DropColumn(
                name: "AvailableEnergyKwh",
                table: "SurplusBlocks");

            migrationBuilder.DropColumn(
                name: "BlockTime",
                table: "SurplusBlocks");

            migrationBuilder.RenameColumn(
                name: "MinBidPricePerKwh",
                table: "SurplusBlocks",
                newName: "MinBidPrice");

            migrationBuilder.RenameColumn(
                name: "BlockSizeKwh",
                table: "SurplusBlocks",
                newName: "EnergyKWh");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SurplusBlocks",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurplusBlocks",
                table: "SurplusBlocks",
                column: "Id");
        }
    }
}
