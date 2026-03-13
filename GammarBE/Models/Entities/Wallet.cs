using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class Wallet
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public decimal? Balance { get; set; }

    public decimal? Total { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }
}
