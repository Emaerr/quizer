using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UndoRemakeQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Questions",
                table: "Quizs");

            migrationBuilder.DropColumn(
                name: "Answers",
                table: "Questions");

            migrationBuilder.AddColumn<int>(
                name: "QuizId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuestionId",
                table: "Answers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "Answers");

            migrationBuilder.AddColumn<string>(
                name: "Questions",
                table: "Quizs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answers",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
