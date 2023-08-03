using AppdevBookShop.Models;

namespace AppdevBookShop.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCart> ListCarts { get; set; }
    public OrderHeader OrderHeader { get; set; }
}