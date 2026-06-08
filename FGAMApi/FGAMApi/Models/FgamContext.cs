using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FGAMApi.Models;

public partial class FgamContext : DbContext
{
    public FgamContext()
    {
    }

    public FgamContext(DbContextOptions<FgamContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Factory> Factories { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<Whitelist> Whitelists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ADMIN-PC;Database=FGAM;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(e => new { e.FactoryId, e.EmployeeId }, "UQ_Accounts_Factory_Employee").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CreateID");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.FactoryId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("FactoryID");
            entity.Property(e => e.Passwrd).IsUnicode(false);
            entity.Property(e => e.RecordStatus).HasDefaultValue(1);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("UpdateID");

            entity.HasOne(d => d.Employee).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accounts_Employees");

            entity.HasOne(d => d.Factory).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.FactoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accounts_Factories");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => e.DepartmentName, "UQ_Departments_Name").IsUnique();

            entity.Property(e => e.DepartmentId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DepartmentID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CreateID");
            entity.Property(e => e.DepartmentName).HasMaxLength(50);
            entity.Property(e => e.RecordStatus).HasDefaultValue(1);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("UpdateID");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.CardNumber, "UQ_Employees_CardNumber").IsUnique();

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CreateID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DepartmentID");
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.FactoryId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("FactoryID");
            entity.Property(e => e.RecordStatus).HasDefaultValue(1);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("UpdateID");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Departments");

            entity.HasOne(d => d.Factory).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FactoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Factories");
        });

        modelBuilder.Entity<Factory>(entity =>
        {
            entity.HasIndex(e => e.FactoryName, "UQ_Factories_Name").IsUnique();

            entity.Property(e => e.FactoryId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("FactoryID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CreateID");
            entity.Property(e => e.FactoryName).HasMaxLength(50);
            entity.Property(e => e.RecordStatus).HasDefaultValue(1);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("UpdateID");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("TransactionID");
            entity.Property(e => e.CheckInTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CheckoutTime).HasColumnType("datetime");
            entity.Property(e => e.CompanyName).HasMaxLength(50);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CreateID");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.FactoryId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("FactoryID");
            entity.Property(e => e.Photo).IsUnicode(false);
            entity.Property(e => e.Purpose).HasMaxLength(100);
            entity.Property(e => e.RecordStatus).HasDefaultValue(1);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("UpdateID");

            entity.HasOne(d => d.Employee).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Employees");

            entity.HasOne(d => d.Factory).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.FactoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Factories");
        });

        modelBuilder.Entity<Whitelist>(entity =>
        {
            entity.HasIndex(e => new { e.FactoryId, e.EmployeeId }, "UQ_Whitelists_Factory_Employee").IsUnique();

            entity.Property(e => e.WhitelistId).HasColumnName("WhitelistID");
            entity.Property(e => e.Avatar).IsUnicode(false);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CreateID");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.FactoryId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("FactoryID");
            entity.Property(e => e.RecordStatus).HasDefaultValue(1);
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("UpdateID");

            entity.HasOne(d => d.Employee).WithMany(p => p.Whitelists)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Whitelists_Employees");

            entity.HasOne(d => d.Factory).WithMany(p => p.Whitelists)
                .HasForeignKey(d => d.FactoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Whitelists_Factories");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
