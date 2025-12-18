using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentCore.Migrations
{
    /// <inheritdoc />
    public partial class AssignmentUploadFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentContentType",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentOriginalName",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AttachmentSize",
                table: "Assignments",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentContentType",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AttachmentOriginalName",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AttachmentSize",
                table: "Assignments");
        }
    }
}
