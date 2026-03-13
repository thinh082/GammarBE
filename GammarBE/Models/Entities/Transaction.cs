using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid? WalletId { get; set; }

    public decimal Amount { get; set; }

    public string? Type { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Wallet? Wallet { get; set; }
}
