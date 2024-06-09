using PharmacyMP.Models;

namespace Pharmacy.ViewModels
{
    public class AccountViewModel
    {
        public User User { get; set; }

        public List<CartItem> CartItems { get; set; }
        public List<Product> Products { get; set; }
    }
}
