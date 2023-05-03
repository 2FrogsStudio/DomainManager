using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class FixDomainMmonitorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DomainMonitorByChat_DnsMonitor_DomainMonitorId",
                table: "DomainMonitorByChat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DnsMonitor",
                table: "DnsMonitor");

            migrationBuilder.RenameTable(
                name: "DnsMonitor",
                newName: "DomainMonitor");

            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "DomainMonitor",
                newName: "LastUpdateDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "DomainMonitor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DomainMonitor",
                table: "DomainMonitor",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainMonitorByChat_DomainMonitor_DomainMonitorId",
                table: "DomainMonitorByChat",
                column: "DomainMonitorId",
                principalTable: "DomainMonitor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DomainMonitorByChat_DomainMonitor_DomainMonitorId",
                table: "DomainMonitorByChat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DomainMonitor",
                table: "DomainMonitor");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "DomainMonitor");

            migrationBuilder.RenameTable(
                name: "DomainMonitor",
                newName: "DnsMonitor");

            migrationBuilder.RenameColumn(
                name: "LastUpdateDate",
                table: "DnsMonitor",
                newName: "LastUpdate");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DnsMonitor",
                table: "DnsMonitor",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainMonitorByChat_DnsMonitor_DomainMonitorId",
                table: "DomainMonitorByChat",
                column: "DomainMonitorId",
                principalTable: "DnsMonitor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
