﻿// <auto-generated />
using DomainManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DomainManager.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230502234422_FixDigitalOceanName")]
    partial class FixDigitalOceanName
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DomainManager.Models.Provider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Providers");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "DigitalOcean"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Gandy"
                        });
                });

            modelBuilder.Entity("DomainManager.Models.UserTokens", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<int>("ProviderId")
                        .HasColumnType("integer");

                    b.Property<string>("Secret")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id", "ProviderId");

                    b.HasIndex("ProviderId");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("DomainManager.Models.UserTokens", b =>
                {
                    b.HasOne("DomainManager.Models.Provider", "Provider")
                        .WithMany("UserTokens")
                        .HasForeignKey("ProviderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Provider");
                });

            modelBuilder.Entity("DomainManager.Models.Provider", b =>
                {
                    b.Navigation("UserTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
