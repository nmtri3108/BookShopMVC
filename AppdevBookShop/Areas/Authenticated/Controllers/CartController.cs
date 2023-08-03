using System.Security.Claims;
using AppdevBookShop.Contanst;
using AppdevBookShop.Data;
using AppdevBookShop.Models;
using AppdevBookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppdevBookShop.Areas.Authenticated.Controllers;

[Area(SD.Authenticated_Area)]
[Authorize]
public class CartController : BaseController
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    [BindProperty] 
    public ShoppingCartVM ShoppingCartVM { get; set; }


    public CartController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET
    public IActionResult Index()
    {
        // lấy id của người đang đăng nhập hiện tại
        var currentUserId = GetCurrentUserId();

        // tạo shopping vm
        // lấy dữ liệu cart của user đang đăng nhập
        ShoppingCartVM = new ShoppingCartVM()
        {
            OrderHeader = new OrderHeader(),
            ListCarts = _db.ShoppingCarts.Where(_ => _.UserId == currentUserId).Include(x => x.Product)
        };
        // set giá trị total là bằng 0
        ShoppingCartVM.OrderHeader.Total = 0;
        // get thêm thông tin object user và gán nó vào property User trong order header
        ShoppingCartVM.OrderHeader.User = _db.Users.FirstOrDefault(x => x.Id == currentUserId);
        // Tính tổng số tiền các sản phẩm bên trong giỏ hàng
        foreach (var list in ShoppingCartVM.ListCarts)
        {
            ShoppingCartVM.OrderHeader.Total += (list.Price * list.Count);
        }

        return View(ShoppingCartVM);
    }

    public IActionResult Plus(int CartId)
    {
        var cart = _db.ShoppingCarts.Include(x => x.Product).FirstOrDefault(x => x.Id == CartId);
        cart.Count += 1;
        cart.Price = cart.Product.Price * cart.Count;
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int CartId)
    {
        var cart = _db.ShoppingCarts.Include(x => x.Product).FirstOrDefault(x => x.Id == CartId);
        if (cart.Count == 1)
        {
            var cnt = _db.ShoppingCarts.Where(u => u.UserId == cart.UserId).ToList().Count;
            _db.ShoppingCarts.Remove(cart);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
        }
        else
        {
            cart.Count -= 1;
            cart.Price = cart.Product.Price * cart.Count;
            _db.SaveChanges();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Remove(int CartId)
    {
        var cart = _db.ShoppingCarts.Include(x => x.Product).FirstOrDefault(x => x.Id == CartId);
        var cnt = _db.ShoppingCarts.Where(u => u.UserId == cart.UserId).ToList().Count;
        _db.ShoppingCarts.Remove(cart);
        _db.SaveChanges();
        HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Summary()
    {
        var currentUserId = GetCurrentUserId();

        ShoppingCartVM = new ShoppingCartVM()
        {
            OrderHeader = new OrderHeader(),
            ListCarts = _db.ShoppingCarts.Where(u => u.UserId == currentUserId)
                .Include(u => u.Product)
        };

        ShoppingCartVM.OrderHeader.User = _db.Users.FirstOrDefault(u => u.Id == currentUserId);

        foreach (var list in ShoppingCartVM.ListCarts)
        {
            ShoppingCartVM.OrderHeader.Total += (list.Product.Price * list.Count);
        }

        ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.User.Address;
        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult SummaryPost()
    {
        var currentUserId = GetCurrentUserId();

        ShoppingCartVM.OrderHeader.User = _db.Users.FirstOrDefault(u => u.Id == currentUserId);

        ShoppingCartVM.ListCarts = _db.ShoppingCarts.Where(u => u.UserId == currentUserId)
            .Include(u => u.Product);
        ShoppingCartVM.OrderHeader.UserId = currentUserId;
        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.User.FullName;
        ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.User.PhoneNumber;
        ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.User.Address;
        _db.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
        _db.SaveChanges();

        foreach (var item in ShoppingCartVM.ListCarts)
        {
            item.Price = item.Product.Price;

            // update quantity of the products
            var productDb = _db.Products.Find(item.ProductId);
            if (productDb.Quantity >= item.Count)
            {
                productDb.Quantity -= item.Count;
            }
            else
            {
                item.Count = productDb.Quantity;
                productDb.Quantity = 0;
            }

            _db.Products.Update(productDb);

            OrderDetails orderDetails = new OrderDetails()
            {
                ProductId = item.ProductId,
                OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                Price = item.Price,
                Quantity = item.Count
            };

            ShoppingCartVM.OrderHeader.Total += orderDetails.Quantity * orderDetails.Price;
            _db.OrderDetails.Add(orderDetails);
        }

        _db.ShoppingCarts.RemoveRange(ShoppingCartVM.ListCarts);
        _db.SaveChanges();
        HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

        return RedirectToAction("OrderConfirmation", "Cart", 
            new { id = ShoppingCartVM.OrderHeader.Id });
    }

    public IActionResult OrderConfirmation(int id)
    {
        return View(id);
    }
}