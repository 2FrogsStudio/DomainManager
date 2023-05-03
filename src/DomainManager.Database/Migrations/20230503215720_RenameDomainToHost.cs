using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class RenameDomainToHost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Domain",
                table: "SslMonitor",
                newName: "Host");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Host",
                table: "SslMonitor",
                newName: "Domain");
        }
    }
}
