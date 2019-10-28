﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using vscodecore.Models;

namespace vscodecore.Migrations
{
    [DbContext(typeof(EFCoreWebFussballContext))]
    [Migration("20191011112435_InitialDatabase")]
    partial class InitialDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("vscodecore.Models.Contester", b =>
                {
                    b.Property<int>("ContesterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<int>("GamesPlayed")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.Property<int?>("TournamentId")
                        .HasColumnType("int");

                    b.Property<int>("TournamentWon")
                        .HasColumnType("int");

                    b.HasKey("ContesterId");

                    b.HasIndex("TournamentId");

                    b.ToTable("Contesters");
                });

            modelBuilder.Entity("vscodecore.Models.Tournament", b =>
                {
                    b.Property<int>("TournamentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WinnerContesterId")
                        .HasColumnType("int");

                    b.HasKey("TournamentId");

                    b.HasIndex("WinnerContesterId");

                    b.ToTable("Tournaments");
                });

            modelBuilder.Entity("vscodecore.Models.Contester", b =>
                {
                    b.HasOne("vscodecore.Models.Tournament", null)
                        .WithMany("Contesters")
                        .HasForeignKey("TournamentId");
                });

            modelBuilder.Entity("vscodecore.Models.Tournament", b =>
                {
                    b.HasOne("vscodecore.Models.Contester", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerContesterId");
                });
#pragma warning restore 612, 618
        }
    }
}
