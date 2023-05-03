using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainExpireAndSslExpire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserDomains",
                newName: "DomainExpire");

            migrationBuilder.CreateTable(
                name: "SslExpires",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SslExpires", x => new { x.ChatId, x.Domain });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DomainExpire",
                newName: "UserDomains");

            migrationBuilder.DropTable(
                name: "SslExpires");
        }
    }
}
