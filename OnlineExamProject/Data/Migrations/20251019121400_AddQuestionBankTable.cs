using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineExamProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionBank",
                columns: table => new
                {
                    QuestionBankId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionCount = table.Column<int>(type: "int", nullable: false),
                    OptionA = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OptionB = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OptionC = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OptionD = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OptionE = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CorrectOption = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Difficulty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBank", x => x.QuestionBankId);
                    table.CheckConstraint("CK_QuestionBank_CorrectOption", "[CorrectOption] IN ('A', 'B', 'C', 'D', 'E')");
                    table.CheckConstraint("CK_QuestionBank_Difficulty", "[Difficulty] IN ('Kolay', 'Orta', 'Zor')");
                    table.ForeignKey(
                        name: "FK_QuestionBank_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionBank_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBank_CourseId",
                table: "QuestionBank",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBank_TeacherId",
                table: "QuestionBank",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionBank");
        }
    }
}
