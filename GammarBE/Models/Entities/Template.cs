using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class Template
{
    public int Id { get; set; }

    public string Tittle { get; set; } = null!;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? BasePrompt { get; set; }

    public string? UiConfig { get; set; }

    public bool? IsPremium { get; set; }

    public virtual ICollection<Generation> Generations { get; set; } = new List<Generation>();
}
