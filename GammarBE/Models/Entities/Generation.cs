using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class Generation
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public int? TemplateId { get; set; }

    public int? ProviderId { get; set; }

    public string? Prompt { get; set; }

    public string? NegativePrompt { get; set; }

    public string? Model { get; set; }

    public string? Url { get; set; }

    public string? Params { get; set; }

    public string? Enum { get; set; }

    public decimal? Cost { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? InputData { get; set; }

    public virtual ICollection<GenerationJob> GenerationJobs { get; set; } = new List<GenerationJob>();

    public virtual ICollection<GenerationLog> GenerationLogs { get; set; } = new List<GenerationLog>();

    public virtual ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();

    public virtual AiProvider? Provider { get; set; }

    public virtual Template? Template { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
}
