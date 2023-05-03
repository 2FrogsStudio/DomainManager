﻿// <auto-generated />
using System;
using DomainManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DomainManager.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False")
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DomainManager.Models.DomainMonitor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Domain")
                        .IsRequired()
                        .HasColumnType("text")
                        .UseCollation("case_insensitive");

                    b.Property<DateTime?>("LastUpdate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("DnsMonitor");
                });

            modelBuilder.Entity("DomainManager.Models.DomainMonitorByChat", b =>
                {
                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<int>("DomainMonitorId")
                        .HasColumnType("integer");

                    b.HasKey("ChatId", "DomainMonitorId");

                    b.HasIndex("DomainMonitorId");

                    b.ToTable("DomainMonitorByChat");
                });

            modelBuilder.Entity("DomainManager.Models.SslMonitor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Domain")
                        .IsRequired()
                        .HasColumnType("text")
                        .UseCollation("case_insensitive");

                    b.Property<int>("Errors")
                        .HasColumnType("integer");

                    b.Property<string>("Issuer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastUpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("NotAfter")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("NotBefore")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("SslMonitor");
                });

            modelBuilder.Entity("DomainManager.Models.SslMonitorByChat", b =>
                {
                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<int>("SslMonitorId")
                        .HasColumnType("integer");

                    b.HasKey("ChatId", "SslMonitorId");

                    b.HasIndex("SslMonitorId");

                    b.ToTable("SslMonitorByChat");
                });

            modelBuilder.Entity("DomainManager.Models.DomainMonitorByChat", b =>
                {
                    b.HasOne("DomainManager.Models.DomainMonitor", "DomainMonitor")
                        .WithMany("DomainMonitors")
                        .HasForeignKey("DomainMonitorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DomainMonitor");
                });

            modelBuilder.Entity("DomainManager.Models.SslMonitorByChat", b =>
                {
                    b.HasOne("DomainManager.Models.SslMonitor", "SslMonitor")
                        .WithMany("SslMonitors")
                        .HasForeignKey("SslMonitorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SslMonitor");
                });

            modelBuilder.Entity("DomainManager.Models.DomainMonitor", b =>
                {
                    b.Navigation("DomainMonitors");
                });

            modelBuilder.Entity("DomainManager.Models.SslMonitor", b =>
                {
                    b.Navigation("SslMonitors");
                });
#pragma warning restore 612, 618
        }
    }
}
