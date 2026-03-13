using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class MediaAsset
{
    public Guid Id { get; set; }

    public Guid? GenId { get; set; }

    public string? PublicId { get; set; }

    public string FileUrl { get; set; } = null!;

    public int? FileSize { get; set; }

    public string? Extension { get; set; }

    public string? Dimension { get; set; }

    public string? AssetType { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? UserId { get; set; }

    public virtual Generation? Gen { get; set; }

    public virtual User? User { get; set; }
}
