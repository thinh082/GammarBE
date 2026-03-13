using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class AiProvider
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Url { get; set; }

    public string? ApiKey { get; set; }

    public bool? IsActive { get; set; }

    public int? Priority { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GenerationLog> GenerationLogs { get; set; } = new List<GenerationLog>();

    public virtual ICollection<Generation> Generations { get; set; } = new List<Generation>();
}
