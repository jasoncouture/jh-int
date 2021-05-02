using Microsoft.EntityFrameworkCore.Migrations;

namespace BillManager.Core.Database.Migrations
{
    public partial class AddNameAndTotalToBills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Bills",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Bills",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Bills");
        }
    }
}
