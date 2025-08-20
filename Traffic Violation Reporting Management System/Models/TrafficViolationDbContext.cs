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

    // --- NEW: Notifications DbSet ---
    public virtual DbSet<Notification> Notifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Default");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fine>(entity =>
        {
            entity.ToTable(tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.FineId).HasName("PK__Fines__F3C688D1F4569110");

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
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.IssuedByNavigation).WithMany(p => p.Fines)
                .HasPrincipalKey(p => p.VehicleNumber)
                .HasForeignKey(d => d.IssuedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Fines__issued_by__6B24EA82");

            entity.HasOne(d => d.Report).WithMany(p => p.Fines)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK_Fines_Reports");
        });

        modelBuilder.Entity<FineResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__FineResp__EBECD8969A41E06F");

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
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Fine).WithMany(p => p.FineResponses)
                .HasForeignKey(d => d.FineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FineRespo__fine___693CA210");

            entity.HasOne(d => d.User).WithMany(p => p.FineResponses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FineRespo__user___6A30C649");
        });

        modelBuilder.Entity<FineViolationBehavior>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FineViol__3213E83F3504CDE3");

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
            entity.HasKey(e => e.OtpId).HasName("PK__OTPs__122D946A0E9E5F53");

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
                .HasMaxLength(10)
                .HasColumnName("phone_number");

            entity.HasOne(d => d.EmailNavigation).WithMany(p => p.Otps)
                .HasPrincipalKey(p => p.Email)
                .HasForeignKey(d => d.Email);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__779B7C58D8807B27");

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
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TimeOfViolation)
                .HasColumnType("datetime")
                .HasColumnName("time_of_violation");

            entity.HasOne(d => d.Reporter).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reports__reporte__6FE99F9F");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__85C600AFB6B875FE");

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
                .HasConstraintName("FK__Transacti__user___70DDC3D8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F85D7E3DD");

            entity.HasIndex(e => e.Cccd, "UQ_Users_CCCD").IsUnique();
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();
            entity.HasIndex(e => e.Cccd, "UQ__Users__37D42BFA9E95A16E").IsUnique();
            entity.HasIndex(e => e.PhoneNumber, "UQ__Users__A1936A6B0324D836").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Cccd)
                .HasMaxLength(12)
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
                .HasMaxLength(10)
                .HasColumnName("phone_number");
            entity.Property(e => e.Role).HasColumnName("role");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__F2947BC1A45A117C");

            entity.HasIndex(e => e.VehicleNumber, "UQ__Vehicles__2D703C2AEC8D68DB").IsUnique();

            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .HasColumnName("brand");
            entity.Property(e => e.ChassicNo)
                .HasMaxLength(20)
                .HasColumnName("chassic_no");
            entity.Property(e => e.Color)
                .HasMaxLength(30)
                .HasColumnName("color");
            entity.Property(e => e.EngineNo)
                .HasMaxLength(20)
                .HasColumnName("engine_no");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.OwnerCccd)
                .HasMaxLength(12)
                .HasColumnName("owner_cccd");
            entity.Property(e => e.OwnerName)
                .HasMaxLength(100)
                .HasColumnName("owner_name");
            entity.Property(e => e.OwnerPhoneNumber)
                .HasMaxLength(10)
                .HasColumnName("owner_phone_number");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.VehicleNumber)
                .HasMaxLength(20)
                .HasColumnName("vehicle_number");
        });

        modelBuilder.Entity<ViolationBehavior>(entity =>
        {
            entity.HasKey(e => e.BehaviorId).HasName("PK__Violatio__6D1B62336FC21DF4");

            entity.Property(e => e.BehaviorId).HasColumnName("behavior_id");
            entity.Property(e => e.MaxFineAmount).HasColumnName("max_fine_amount");
            entity.Property(e => e.MinFineAmount).HasColumnName("min_fine_amount");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);

            entity.ToTable("Notifications");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasMaxLength(50).HasColumnName("type");
            entity.Property(e => e.Title).HasMaxLength(200).HasColumnName("title");
            entity.Property(e => e.Message).HasMaxLength(500).HasColumnName("message");
            entity.Property(e => e.DataJson).HasColumnName("data_json");
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("datetime")
                  .HasDefaultValueSql("(getdate())");

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }, "IX_Notifications_User_Read_Created");

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Notifications_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
