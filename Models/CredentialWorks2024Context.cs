using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAppPedalaCom.Models;

public partial class CredentialWorks2024Context : DbContext
{
    public CredentialWorks2024Context()
    {
    }

    public CredentialWorks2024Context(DbContextOptions<CredentialWorks2024Context> options)
        : base(options)
    {
    }

    public virtual DbSet<CwCustomer> CwCustomers { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=CredentialWorks2024;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CwCustomer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B835574E4B");

            entity.ToTable("CwCustomer");

            entity.Property(e => e.CustomerId)
                .HasColumnName("CustomerID");
            entity.Property(e => e.CompanyName).HasMaxLength(128);
            entity.Property(e => e.EmailAddress).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(128);
            entity.Property(e => e.PasswordSalt).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.Title).HasMaxLength(3);
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ErrorLog__D65247E2BA20A182");

            entity.ToTable("ErrorLog");

            entity.Property(e => e.Id)
                .HasColumnName("Id");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.StackTrace).HasMaxLength(255);
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            entity.Property(e => e.ErrorCode).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
