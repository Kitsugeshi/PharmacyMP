using System;
using System.Collections.Generic;

namespace PharmacyMP.Models;

public partial class Product
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Price { get; set; }

    public string? Description { get; set; }

    public int? Quantity { get; set; }

    public string? Symptoms { get; set; }

    public string? IfPrescription { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
