using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinibusTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddStopImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResimUrl",
                table: "Stops",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResimUrl",
                table: "Stops");
        }
    }
}
