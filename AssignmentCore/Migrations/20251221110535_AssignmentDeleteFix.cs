using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentCore.Migrations
{
    /// <inheritdoc />
    public partial class AssignmentDeleteFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                table: "AssignmentSubmissions");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                table: "AssignmentSubmissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                table: "AssignmentSubmissions");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                table: "AssignmentSubmissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id");
        }
    }
}
