using AppdevBookShop.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppdevBookShop.ViewModels;

public class ProductVM
{
    public Product Product { get; set; }
    public IEnumerable<SelectListItem> CategoryList { get; set; }
}