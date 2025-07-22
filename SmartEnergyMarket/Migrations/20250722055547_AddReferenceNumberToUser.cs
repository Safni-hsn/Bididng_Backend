using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEnergyMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceNumberToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "AspNetUsers");
        }
    }
}
