using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.App.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Operations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Categories",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Categories");
        }
    }
}
