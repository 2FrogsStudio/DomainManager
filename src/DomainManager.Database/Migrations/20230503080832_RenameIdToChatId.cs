using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdToChatId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserDomains",
                newName: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "UserDomains",
                newName: "Id");
        }
    }
}
