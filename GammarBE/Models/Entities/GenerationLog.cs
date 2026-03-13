using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class GenerationLog
{
    public Guid Id { get; set; }

    public Guid? GenId { get; set; }

    public int? ProviderId { get; set; }

    public string? PromptParams { get; set; }

    public string? ResponseParams { get; set; }

    public string? Error { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Generation? Gen { get; set; }

    public virtual AiProvider? Provider { get; set; }
}
