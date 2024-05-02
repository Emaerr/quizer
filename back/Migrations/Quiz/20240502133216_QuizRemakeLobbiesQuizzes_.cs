using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizer.Migrations.Quiz
{
    /// <inheritdoc />
    public partial class QuizRemakeLobbiesQuizzes_ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Answers_CorrectAnswerId1",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CorrectAnswerId1",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CorrectAnswerId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CorrectAnswerId1",
                table: "Questions");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "Answers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "Answers");

            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswerId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswerId1",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CorrectAnswerId1",
                table: "Questions",
                column: "CorrectAnswerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Answers_CorrectAnswerId1",
                table: "Questions",
                column: "CorrectAnswerId1",
                principalTable: "Answers",
                principalColumn: "Id");
        }
    }
}
