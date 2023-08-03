using AppdevBookShop.Models;

namespace AppdevBookShop.ViewModels;

public class OrderDetailsVM
{
    public OrderHeader OrderHeader { get; set; }
    public IEnumerable<OrderDetails> OrderDetails { get; set; }
}