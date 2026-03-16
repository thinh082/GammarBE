using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Fullname { get; set; }

    public string? Status { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Generation> Generations { get; set; } = new List<Generation>();

    public virtual ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

    public virtual ICollection<UserUsage> UserUsages { get; set; } = new List<UserUsage>();

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}
