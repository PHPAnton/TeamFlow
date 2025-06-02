using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserToTaskItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_Users_UserId",
                table: "TaskItem");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TaskItem",
                newName: "AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItem_UserId",
                table: "TaskItem",
                newName: "IX_TaskItem_AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_Users_AssignedUserId",
                table: "TaskItem",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_Users_AssignedUserId",
                table: "TaskItem");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                table: "TaskItem",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItem_AssignedUserId",
                table: "TaskItem",
                newName: "IX_TaskItem_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_Users_UserId",
                table: "TaskItem",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
