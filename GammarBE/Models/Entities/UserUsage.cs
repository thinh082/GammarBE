using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class UserUsage
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly? Date { get; set; }

    public int? TotalGen { get; set; }

    public decimal? TotalCost { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
