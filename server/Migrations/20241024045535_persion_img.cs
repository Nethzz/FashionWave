using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegApi.Migrations
{
    /// <inheritdoc />
    public partial class persion_img : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Imagepath",
                table: "PersonalDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imagepath",
                table: "PersonalDetails");
        }
    }
}
