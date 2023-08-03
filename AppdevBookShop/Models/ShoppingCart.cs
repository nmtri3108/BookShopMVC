using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppdevBookShop.Models;

public class ShoppingCart
{
    [Key]
    public int Id { get; set; }
        
    public int Count { get; set; }
        
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public User User{ get; set; }
        
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [NotMapped] 
    public int Price { get; set; }
}