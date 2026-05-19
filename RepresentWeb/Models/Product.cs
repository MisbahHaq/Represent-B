using System.ComponentModel.DataAnnotations;

namespace representweb.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public int Stock { get; set; }
    }
}