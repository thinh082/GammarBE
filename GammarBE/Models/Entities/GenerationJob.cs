using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class GenerationJob
{
    public Guid Id { get; set; }

    public Guid? GenId { get; set; }

    public string? Status { get; set; }

    public int? RetryCount { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public virtual Generation? Gen { get; set; }
}
