using System.Diagnostics;
using AppdevBookShop.Areas.Authenticated.Controllers;
using AppdevBookShop.Contanst;
using AppdevBookShop.Data;
using Microsoft.AspNetCore.Mvc;
using AppdevBookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AppdevBookShop.Areas.UnAuthenticated.Controllers;

[Area(SD.UnAuthenticated_Area)]
public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        var productList = _db.Products.Include(p => p.Category).ToList();
        var currentUserId = GetCurrentUserId();
        if (currentUserId != null)
        {
            var count = _db.ShoppingCarts.Where(u => u.UserId == currentUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
        }

        return View(productList);
    }


    public IActionResult Details(int id)
    {
        var productFromDb = _db.Products.Where(p => p.Id == id).Include(c => c.Category).First();
        ShoppingCart shoppingCart = new ShoppingCart()
        {
            Product = productFromDb,
            ProductId = productFromDb.Id
        };
        return View(shoppingCart);
    }

    [HttpPost]
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public IActionResult Details(ShoppingCart CartObject)
    {
        CartObject.Id = 0;
        if (CartObject.Count > 0)
        {
            // lấy id của user đang đăng nhập
            var currentUserId = GetCurrentUserId();
            CartObject.UserId = currentUserId;
            
            ShoppingCart cartFromDb = _db.ShoppingCarts.Where(
                    u => u.UserId == CartObject.UserId && u.ProductId == CartObject.ProductId).Include(u => u.Product)
                .FirstOrDefault();
            if (cartFromDb == null)
            {
                //no records exists in database for that product for that user
                _db.ShoppingCarts.Add(CartObject);
            }
            else
            {
                cartFromDb.Count += CartObject.Count;
                _db.ShoppingCarts.Update(cartFromDb);
            }

            _db.SaveChanges();
            // store to sesion
            var count = _db.ShoppingCarts.Where(c => c.UserId == CartObject.UserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
            return RedirectToAction(nameof(Index));
        }
        else
        {
            var productFromDb = _db.Products.Where(u => u.Id == CartObject.ProductId).Include(c => c.Category).FirstOrDefault();
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };
            return View(shoppingCart);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}