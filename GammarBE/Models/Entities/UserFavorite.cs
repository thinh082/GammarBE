using System;
using System.Collections.Generic;

namespace GammarBE.Models.Entities;

public partial class UserFavorite
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? GenId { get; set; }

    public string? Type { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Generation? Gen { get; set; }

    public virtual User? User { get; set; }
}
