using System.Collections.Generic;
using representweb.Models;

namespace representweb.Models
{
    public class ProfileViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Product> RecentlyViewed { get; set; } = new List<Product>();
    }
}