using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizTimeLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeLimit",
                table: "Quiz",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeLimit",
                table: "Quiz");
        }
    }
}
