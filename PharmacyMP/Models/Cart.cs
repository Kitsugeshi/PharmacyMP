using System;
using System.Collections.Generic;

namespace PharmacyMP.Models;

public partial class Cart
{
    public int Id { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User IdNavigation { get; set; } = null!;
}
