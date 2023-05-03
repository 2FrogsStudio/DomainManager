using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class AddChatRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainExpire");

            migrationBuilder.DropTable(
                name: "SslExpires");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.CreateTable(
                name: "DnsMonitor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnsMonitor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SslMonitor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    LastUpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Issuer = table.Column<string>(type: "text", nullable: false),
                    NotAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NotBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Errors = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SslMonitor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainMonitorByChat",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    DomainMonitorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainMonitorByChat", x => new { x.ChatId, x.DomainMonitorId });
                    table.ForeignKey(
                        name: "FK_DomainMonitorByChat_DnsMonitor_DomainMonitorId",
                        column: x => x.DomainMonitorId,
                        principalTable: "DnsMonitor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SslMonitorByChat",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    SslMonitorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SslMonitorByChat", x => new { x.ChatId, x.SslMonitorId });
                    table.ForeignKey(
                        name: "FK_SslMonitorByChat_SslMonitor_SslMonitorId",
                        column: x => x.SslMonitorId,
                        principalTable: "SslMonitor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainMonitorByChat_DomainMonitorId",
                table: "DomainMonitorByChat",
                column: "DomainMonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_SslMonitorByChat_SslMonitorId",
                table: "SslMonitorByChat",
                column: "SslMonitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainMonitorByChat");

            migrationBuilder.DropTable(
                name: "SslMonitorByChat");

            migrationBuilder.DropTable(
                name: "DnsMonitor");

            migrationBuilder.DropTable(
                name: "SslMonitor");

            migrationBuilder.CreateTable(
                name: "DomainExpire",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainExpire", x => new { x.ChatId, x.Domain });
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SslExpires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Errors = table.Column<int>(type: "integer", nullable: false),
                    Issuer = table.Column<string>(type: "text", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NotBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SslExpires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.Id, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_UserTokens_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "DigitalOcean" },
                    { 2, "Gandy" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_ProviderId",
                table: "UserTokens",
                column: "ProviderId");
        }
    }
}
