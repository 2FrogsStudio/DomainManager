using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class FixDigitalOceanName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Providers",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "DigitalOcean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Providers",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Digital Ocean");
        }
    }
}
