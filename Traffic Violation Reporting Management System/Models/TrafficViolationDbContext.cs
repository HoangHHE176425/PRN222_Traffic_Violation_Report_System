using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class TrafficViolationDbContext : DbContext
{
    public TrafficViolationDbContext()
    {
    }

    public TrafficViolationDbContext(DbContextOptions<TrafficViolationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Fine> Fines { get; set; }

    public virtual DbSet<FineResponse> FineResponses { get; set; }

    public virtual DbSet<FineViolationBehavior> FineViolationBehaviors { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<ViolationBehavior> ViolationBehaviors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Default");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.FineId).HasName("PK__Fines__F3C688D17C6514D0");

            entity.Property(e => e.FineId).HasColumnName("fine_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IssuedBy)
                .HasMaxLength(20)
                .HasColumnName("issued_by");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(0)
                .HasColumnName("status");

            entity.HasOne(d => d.IssuedByNavigation).WithMany(p => p.Fines)
                .HasPrincipalKey(p => p.VehicleNumber)
                .HasForeignKey(d => d.IssuedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Fines__issued_by__7A672E12");

            entity.HasOne(d => d.Report).WithMany(p => p.Fines)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK_Fines_Reports");
        });

        modelBuilder.Entity<FineResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__FineResp__EBECD8967CC34D2E");

            entity.Property(e => e.ResponseId).HasColumnName("response_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.FineId).HasColumnName("fine_id");
            entity.Property(e => e.MediaPath)
                .HasMaxLength(255)
                .HasColumnName("media_path");
            entity.Property(e => e.Reply).HasColumnName("reply");
            entity.Property(e => e.Status)
                .HasDefaultValue(0)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Fine).WithMany(p => p.FineResponses)
                .HasForeignKey(d => d.FineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FineRespo__fine___0A688BB1");

            entity.HasOne(d => d.User).WithMany(p => p.FineResponses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FineRespo__user___0B5CAFEA");
        });

        modelBuilder.Entity<FineViolationBehavior>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FineViol__3213E83FBB245126");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateFineAmount"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BehaviorId).HasColumnName("behavior_id");
            entity.Property(e => e.DecidedAmount).HasColumnName("decided_amount");
            entity.Property(e => e.FineId).HasColumnName("fine_id");

            entity.HasOne(d => d.Behavior).WithMany(p => p.FineViolationBehaviors)
                .HasForeignKey(d => d.BehaviorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FVB_Behavior");

            entity.HasOne(d => d.Fine).WithMany(p => p.FineViolationBehaviors)
                .HasForeignKey(d => d.FineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FVB_Fine");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__OTPs__122D946AE5D4E378");

            entity.ToTable("OTPs");

            entity.Property(e => e.OtpId).HasColumnName("otpId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IsUsed).HasColumnName("isUsed");
            entity.Property(e => e.Otpcode)
                .HasMaxLength(10)
                .HasColumnName("OTPCode");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");

            entity.HasOne(d => d.EmailNavigation).WithMany(p => p.Otps)
                .HasPrincipalKey(p => p.Email)
                .HasForeignKey(d => d.Email);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__779B7C58F7E8661A");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.MediaPath)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.MediaType).HasMaxLength(50);
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(0)
                .HasColumnName("status");
            entity.Property(e => e.TimeOfViolation)
                .HasColumnType("datetime")
                .HasColumnName("time_of_violation");

            entity.HasOne(d => d.Reporter).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reports__reporte__5535A963");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__85C600AF806EBC3A");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__user___503BEA1C");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F6E73C7D3");

            entity.HasIndex(e => e.Cccd, "UQ_Users_CCCD").IsUnique();

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.HasIndex(e => e.Cccd, "UQ__Users__37D42BFA4E13D8A8").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "UQ__Users__A1936A6B6A1C57EE").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .HasColumnName("cccd");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.Role).HasColumnName("role");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__F2947BC1894820FA");

            entity.HasIndex(e => e.VehicleNumber, "UQ__Vehicles__2D703C2A7BAFC735").IsUnique();

            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .HasColumnName("brand");
            entity.Property(e => e.ChassicNo)
                .HasMaxLength(50)
                .HasColumnName("chassic_no");
            entity.Property(e => e.Color)
                .HasMaxLength(30)
                .HasColumnName("color");
            entity.Property(e => e.EngineNo)
                .HasMaxLength(50)
                .HasColumnName("engine_no");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.OwnerCccd)
                .HasMaxLength(20)
                .HasColumnName("owner_cccd");
            entity.Property(e => e.OwnerName)
                .HasMaxLength(100)
                .HasColumnName("owner_name");
            entity.Property(e => e.OwnerPhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("owner_phone_number");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(0)
                .HasColumnName("status");
            entity.Property(e => e.VehicleNumber)
                .HasMaxLength(20)
                .HasColumnName("vehicle_number");
        });

        modelBuilder.Entity<ViolationBehavior>(entity =>
        {
            entity.HasKey(e => e.BehaviorId).HasName("PK__Violatio__6D1B6233CCDB7555");

            entity.Property(e => e.BehaviorId).HasColumnName("behavior_id");
            entity.Property(e => e.MaxFineAmount).HasColumnName("max_fine_amount");
            entity.Property(e => e.MinFineAmount).HasColumnName("min_fine_amount");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
