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

    public virtual DbSet<Complaint> Complaints { get; set; }

    public virtual DbSet<ComplaintResponse> ComplaintResponses { get; set; }

    public virtual DbSet<Fine> Fines { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(ConnectionString);
        }

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId).HasName("PK__Complain__A771F61CB09DF78E");

            entity.Property(e => e.ComplaintId).HasColumnName("complaint_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(0)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Report).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK__Complaint__repor__571DF1D5");

            entity.HasOne(d => d.User).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Complaint__user___5812160E");
        });

        modelBuilder.Entity<ComplaintResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__Complain__EBECD89628CE01E8");

            entity.Property(e => e.ResponseId).HasColumnName("response_id");
            entity.Property(e => e.ComplaintId).HasColumnName("complaint_id");
            entity.Property(e => e.RespondedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("responded_at");
            entity.Property(e => e.ResponderId).HasColumnName("responder_id");
            entity.Property(e => e.ResponseText).HasColumnName("response_text");

            entity.HasOne(d => d.Complaint).WithMany(p => p.ComplaintResponses)
                .HasForeignKey(d => d.ComplaintId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Complaint__compl__5535A963");

            entity.HasOne(d => d.Responder).WithMany(p => p.ComplaintResponses)
                .HasForeignKey(d => d.ResponderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Complaint__respo__5629CD9C");
        });

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.FineId).HasName("PK__Fines__F3C688D15BC7D36F");

            entity.Property(e => e.FineId).HasColumnName("fine_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
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
                .HasConstraintName("FK__Fines__issued_by__59063A47");

            entity.HasOne(d => d.Report).WithMany(p => p.Fines)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Fines__report_id__59FA5E80");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F27E83BE2");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__user___5AEE82B9");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__OTPs__122D946A80567727");

            entity.ToTable("OTPs");

            entity.Property(e => e.OtpId).HasColumnName("otpId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
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
                .HasForeignKey(d => d.Email)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OTPs__email__Users");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__779B7C58ED52C955");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
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
                .HasConstraintName("FK__Reports__reporte__5CD6CB2B");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FC07A7864");

            entity.HasIndex(e => e.Cccd, "UQ__Users__37D42BFA13EC9B04").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "UQ__Users__A1936A6BEC9404E3").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__email").IsUnique();

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
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
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
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__F2947BC185BD68DB");

            entity.HasIndex(e => e.VehicleNumber, "UQ__Vehicles__2D703C2AA55675F2").IsUnique();

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
