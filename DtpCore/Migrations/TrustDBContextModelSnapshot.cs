﻿// <auto-generated />
using System;
using DtpCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DtpCore.Migrations
{
    [DbContext(typeof(TrustDBContext))]
    partial class TrustDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("DtpCore.Model.BlockchainProof", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<long>("BlockTime");

                    b.Property<string>("Blockchain");

                    b.Property<int>("Confirmations");

                    b.Property<byte[]>("MerkleRoot");

                    b.Property<byte[]>("Receipt");

                    b.Property<int>("RetryAttempts");

                    b.Property<string>("Status");

                    b.HasKey("DatabaseID");

                    b.ToTable("BlockchainProof");
                });

            modelBuilder.Entity("DtpCore.Model.KeyValue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<byte[]>("Value");

                    b.HasKey("ID");

                    b.HasIndex("Key");

                    b.ToTable("KeyValues");
                });

            modelBuilder.Entity("DtpCore.Model.Package", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Algorithm");

                    b.Property<uint>("Created");

                    b.Property<byte[]>("Id")
                        .IsRequired();

                    b.HasKey("DatabaseID");

                    b.HasAlternateKey("Id");

                    b.ToTable("Package");
                });

            modelBuilder.Entity("DtpCore.Model.Timestamp", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Algorithm");

                    b.Property<string>("Blockchain");

                    b.Property<int>("BlockchainProofDatabaseID");

                    b.Property<int>("PackageDatabaseID");

                    b.Property<byte[]>("Receipt");

                    b.Property<long>("Registered");

                    b.Property<string>("Service");

                    b.Property<byte[]>("Source");

                    b.Property<int>("TrustDatabaseID");

                    b.HasKey("DatabaseID");

                    b.HasIndex("BlockchainProofDatabaseID");

                    b.HasIndex("PackageDatabaseID");

                    b.HasIndex("Source");

                    b.HasIndex("TrustDatabaseID");

                    b.ToTable("Timestamp");
                });

            modelBuilder.Entity("DtpCore.Model.Trust", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Activate");

                    b.Property<string>("Algorithm");

                    b.Property<string>("Claim");

                    b.Property<short>("Cost");

                    b.Property<uint>("Created");

                    b.Property<uint>("Expire");

                    b.Property<byte[]>("Id");

                    b.Property<string>("Note");

                    b.Property<int?>("PackageDatabaseID");

                    b.Property<bool>("Replaced");

                    b.Property<string>("Type");

                    b.HasKey("DatabaseID");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("PackageDatabaseID");

                    b.ToTable("Trust");
                });

            modelBuilder.Entity("DtpCore.Model.WorkflowContainer", b =>
                {
                    b.Property<int>("DatabaseID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<string>("Data");

                    b.Property<long>("NextExecution");

                    b.Property<string>("State");

                    b.Property<string>("Tag");

                    b.Property<string>("Type");

                    b.HasKey("DatabaseID");

                    b.HasIndex("State");

                    b.HasIndex("Type");

                    b.ToTable("Workflow");
                });

            modelBuilder.Entity("DtpCore.Model.Package", b =>
                {
                    b.OwnsOne("DtpCore.Model.ServerIdentity", "Server", b1 =>
                        {
                            b1.Property<int?>("PackageDatabaseID");

                            b1.Property<string>("Address");

                            b1.Property<byte[]>("Signature");

                            b1.Property<string>("Type");

                            b1.ToTable("Package");

                            b1.HasOne("DtpCore.Model.Package")
                                .WithOne("Server")
                                .HasForeignKey("DtpCore.Model.ServerIdentity", "PackageDatabaseID")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("DtpCore.Model.Timestamp", b =>
                {
                    b.HasOne("DtpCore.Model.BlockchainProof")
                        .WithMany("Timestamps")
                        .HasForeignKey("BlockchainProofDatabaseID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DtpCore.Model.Package")
                        .WithMany("Timestamps")
                        .HasForeignKey("PackageDatabaseID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DtpCore.Model.Trust")
                        .WithMany("Timestamps")
                        .HasForeignKey("TrustDatabaseID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DtpCore.Model.Trust", b =>
                {
                    b.HasOne("DtpCore.Model.Package")
                        .WithMany("Trusts")
                        .HasForeignKey("PackageDatabaseID");

                    b.OwnsOne("DtpCore.Model.IssuerIdentity", "Issuer", b1 =>
                        {
                            b1.Property<int?>("TrustDatabaseID");

                            b1.Property<string>("Address");

                            b1.Property<byte[]>("Signature");

                            b1.Property<string>("Type");

                            b1.HasIndex("Address");

                            b1.ToTable("Trust");

                            b1.HasOne("DtpCore.Model.Trust")
                                .WithOne("Issuer")
                                .HasForeignKey("DtpCore.Model.IssuerIdentity", "TrustDatabaseID")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("DtpCore.Model.Scope", "Scope", b1 =>
                        {
                            b1.Property<int?>("TrustDatabaseID");

                            b1.Property<string>("Type");

                            b1.Property<string>("Value");

                            b1.ToTable("Trust");

                            b1.HasOne("DtpCore.Model.Trust")
                                .WithOne("Scope")
                                .HasForeignKey("DtpCore.Model.Scope", "TrustDatabaseID")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("DtpCore.Model.SubjectIdentity", "Subject", b1 =>
                        {
                            b1.Property<int?>("TrustDatabaseID");

                            b1.Property<string>("Address");

                            b1.Property<byte[]>("Signature");

                            b1.Property<string>("Type");

                            b1.HasIndex("Address");

                            b1.ToTable("Trust");

                            b1.HasOne("DtpCore.Model.Trust")
                                .WithOne("Subject")
                                .HasForeignKey("DtpCore.Model.SubjectIdentity", "TrustDatabaseID")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
