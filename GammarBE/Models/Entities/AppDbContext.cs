using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GammarBE.Models.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiProvider> AiProviders { get; set; }

    public virtual DbSet<Generation> Generations { get; set; }

    public virtual DbSet<GenerationJob> GenerationJobs { get; set; }

    public virtual DbSet<GenerationLog> GenerationLogs { get; set; }

    public virtual DbSet<MediaAsset> MediaAssets { get; set; }

    public virtual DbSet<Template> Templates { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    public virtual DbSet<UserUsage> UserUsages { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:Connection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_providers_pkey");

            entity.ToTable("ai_providers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApiKey).HasColumnName("api_key");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Priority)
                .HasDefaultValue(0)
                .HasColumnName("priority");
            entity.Property(e => e.Url).HasColumnName("url");
        });

        modelBuilder.Entity<Generation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Generations_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Cost)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("cost");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Enum)
                .HasMaxLength(50)
                .HasColumnName("enum");
            entity.Property(e => e.InputData)
                .HasColumnType("jsonb")
                .HasColumnName("input_data");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
            entity.Property(e => e.NegativePrompt).HasColumnName("negative_prompt");
            entity.Property(e => e.Params)
                .HasColumnType("jsonb")
                .HasColumnName("params");
            entity.Property(e => e.Prompt).HasColumnName("prompt");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.Url).HasColumnName("url");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Provider).WithMany(p => p.Generations)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Generations_provider_id_fkey");

            entity.HasOne(d => d.Template).WithMany(p => p.Generations)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Generations_template_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Generations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Generations_user_id_fkey");
        });

        modelBuilder.Entity<GenerationJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Generation_jobs_pkey");

            entity.ToTable("Generation_jobs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.FinishedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("finished_at");
            entity.Property(e => e.GenId).HasColumnName("gen_id");
            entity.Property(e => e.RetryCount)
                .HasDefaultValue(0)
                .HasColumnName("retry_count");
            entity.Property(e => e.StartedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Gen).WithMany(p => p.GenerationJobs)
                .HasForeignKey(d => d.GenId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Generation_jobs_gen_id_fkey");
        });

        modelBuilder.Entity<GenerationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Generation_logs_pkey");

            entity.ToTable("Generation_logs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Error).HasColumnName("error");
            entity.Property(e => e.GenId).HasColumnName("gen_id");
            entity.Property(e => e.PromptParams)
                .HasColumnType("jsonb")
                .HasColumnName("prompt_params");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.ResponseParams)
                .HasColumnType("jsonb")
                .HasColumnName("response_params");

            entity.HasOne(d => d.Gen).WithMany(p => p.GenerationLogs)
                .HasForeignKey(d => d.GenId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Generation_logs_gen_id_fkey");

            entity.HasOne(d => d.Provider).WithMany(p => p.GenerationLogs)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("Generation_logs_provider_id_fkey");
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_assets_pkey");

            entity.ToTable("media_assets");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AssetType)
                .HasMaxLength(50)
                .HasColumnName("asset_type");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Dimension)
                .HasMaxLength(50)
                .HasColumnName("dimension");
            entity.Property(e => e.Extension)
                .HasMaxLength(10)
                .HasColumnName("extension");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.GenId).HasColumnName("gen_id");
            entity.Property(e => e.PublicId)
                .HasMaxLength(255)
                .HasColumnName("public_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Gen).WithMany(p => p.MediaAssets)
                .HasForeignKey(d => d.GenId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("media_assets_gen_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.MediaAssets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("media_assets_user_id_fkey");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Templates_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BasePrompt).HasColumnName("base_prompt");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsPremium)
                .HasDefaultValue(false)
                .HasColumnName("is_premium");
            entity.Property(e => e.Tittle)
                .HasMaxLength(255)
                .HasColumnName("tittle");
            entity.Property(e => e.UiConfig)
                .HasColumnType("jsonb")
                .HasColumnName("ui_config");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Transaction_pkey");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(15, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Transaction_wallet_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("fullname");
            entity.Property(e => e.Password)
                .HasMaxLength(1000)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_favorites_pkey");

            entity.ToTable("user_favorites");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.GenId).HasColumnName("gen_id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Gen).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.GenId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_favorites_gen_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_favorites_user_id_fkey");
        });

        modelBuilder.Entity<UserUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_usage_pkey");

            entity.ToTable("User_usage");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("date");
            entity.Property(e => e.TotalCost)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_cost");
            entity.Property(e => e.TotalGen)
                .HasDefaultValue(0)
                .HasColumnName("total_gen");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserUsages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("User_usage_user_id_fkey");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Wallet_pkey");

            entity.ToTable("Wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("balance");
            entity.Property(e => e.Total)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Wallet_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
