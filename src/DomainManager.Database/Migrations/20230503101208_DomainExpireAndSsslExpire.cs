using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DomainManager.Migrations
{
    /// <inheritdoc />
    public partial class DomainExpireAndSsslExpire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SslExpires",
                table: "SslExpires");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "SslExpires");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SslExpires",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpireDate",
                table: "SslExpires",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "SslExpires",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "DomainExpire",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SslExpires",
                table: "SslExpires",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SslExpires",
                table: "SslExpires");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SslExpires");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "SslExpires");

            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "SslExpires");

            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "DomainExpire");

            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "SslExpires",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SslExpires",
                table: "SslExpires",
                columns: new[] { "ChatId", "Domain" });
        }
    }
}
