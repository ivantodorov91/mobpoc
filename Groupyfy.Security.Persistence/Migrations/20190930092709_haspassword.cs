using Microsoft.EntityFrameworkCore.Migrations;

namespace Groupyfy.Security.Persistence.Migrations
{
    public partial class haspassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPassword",
                table: "AspNetRoles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPassword",
                table: "AspNetRoles");
        }
    }
}
