using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegApi.Migrations
{
    /// <inheritdoc />
    public partial class amountStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmountStatus",
                table: "AddProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountStatus",
                table: "AddProducts");
        }
    }
}
