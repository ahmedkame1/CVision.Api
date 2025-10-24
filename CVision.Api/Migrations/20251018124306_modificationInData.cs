using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVision.Api.Migrations
{
    /// <inheritdoc />
    public partial class modificationInData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_CVs_CvId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "AspNetUsers",
                newName: "ApplicationRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_ApplicationRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_CVs_CvId",
                table: "Applications",
                column: "CvId",
                principalTable: "CVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_ApplicationRoleId",
                table: "AspNetUsers",
                column: "ApplicationRoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_CVs_CvId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_ApplicationRoleId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ApplicationRoleId",
                table: "AspNetUsers",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_ApplicationRoleId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_CVs_CvId",
                table: "Applications",
                column: "CvId",
                principalTable: "CVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleId",
                table: "AspNetUsers",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
        }
    }
}
