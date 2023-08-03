using AppdevBookShop.Contanst;
using AppdevBookShop.Data;
using AppdevBookShop.Models;
using AppdevBookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppdevBookShop.Areas.Authenticated.Controllers;

[Area(SD.Authenticated_Area)]
[Authorize(Roles = SD.StoreOwner_Role)]
public class BooksController : BaseController
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _hostEnvironment;

    public BooksController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
    {
        _db = db;
        _hostEnvironment = hostEnvironment;
    }

    public IActionResult Index()
    {
        var listAllData = _db.Products.Include(X=>X.Category).ToList();
        ViewData["Message"] = TempData["Message"];
        return View(listAllData);
    }

    [HttpGet]
    public IActionResult Upsert(int? id)
    {
        ProductVM productVm = new ProductVM()
        {
            Product = new Product(),
            CategoryList = categoriesSelectListItems()
        };

        // create
        if (id == null)
        {
            return View(productVm);
        }

        //update
        productVm.Product = _db.Products.Find(id);

        return View(productVm);
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVm)
    {
        
        // validate dữ liệu
        if (productVm.Product.Title != String.Empty 
            && productVm.Product.NoPage != String.Empty 
            && productVm.Product.Price > 0 
            && productVm.Product.Quantity > 0 
            && productVm.Product.CategoryId != 0)
        {

            // webRootPath - đường dẫn của máy tính tới folder wwwroot 
            string webRootPath = _hostEnvironment.WebRootPath;
            // lấy file người truyền vào
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                // generate filename
                string fileName = Guid.NewGuid().ToString();
                
                // tạo ra đường dẫn để tới folder product
                var uploads = Path.Combine(webRootPath, @"images/products");
                // lấy ra đuôi file
                var extension = Path.GetExtension(files[0].FileName);
                
                // trong trường hợp update người dùng thay thế bức ảnh thành bức ảnh mới
                if (productVm.Product.ImageUrl != null)
                {
                    // to edit path so we need to delete the old path and update new one
                    var imagePath = Path.Combine(webRootPath, productVm.Product.ImageUrl.TrimStart('/'));
                    // kiểm tra xem bức ảnh đó có nằm trong folder ko 
                    if (System.IO.File.Exists(imagePath))
                    {
                        // nếu có xóa đi
                        System.IO.File.Delete(imagePath);
                    }
                }

                // lưu dữ liệu bức ảnh vào folder products
                using (var filesStreams =
                       new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStreams);
                }

                // lưu đường dẫn file
                productVm.Product.ImageUrl = @"/images/products/" + fileName + extension;
            }

            // create
            if (productVm.Product.Id == 0)
            {
                _db.Products.Add(productVm.Product);
                _db.SaveChanges();
                TempData["Message"] = "Success: Add Successfully";
                return RedirectToAction(nameof(Index));
            }

            // update
            var productDb = _db.Products.Find(productVm.Product.Id);
            productDb.Author = productVm.Product.Author;
            productDb.Title = productVm.Product.Title;
            productDb.Category = productVm.Product.Category;
            productDb.Description = productVm.Product.Description;
            productDb.Price = productVm.Product.Price;
            productDb.NoPage = productVm.Product.NoPage;
            productDb.ImageUrl = productVm.Product.ImageUrl;
            productDb.Quantity = productVm.Product.Quantity;

            _db.Products.Update(productDb);
            _db.SaveChanges();
            TempData["Message"] = "Success: Update Successfully";
            return RedirectToAction(nameof(Index));
        }

        // trường hợp validate ko thành công thì mình sẽ trả về dữ liệu cũ
        ViewData["Message"] = "Error: Invalid Input, Please Recheck Again";
        // lấy lại dữ liệu cho category list bởi vì khi post về thì category list == null
        productVm.CategoryList = categoriesSelectListItems();

        return View(productVm);
    }

    [NonAction]
    private IEnumerable<SelectListItem> categoriesSelectListItems()
    {
        var categories = _db.Categories.Where(_=>_.Status == SD.Category_Status_Approved).ToList();
        var result = categories.Select(i => new SelectListItem()
        {
            Text = i.Name,
            Value = i.Id.ToString()
        });
        return result;
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        if (id == null)
        {
            ViewData["Message"] = "Error: Id input null";
        }

        var productNeedToDelete = _db.Products.Find(id);
        _db.Products.Remove(productNeedToDelete);
        _db.SaveChanges();
        TempData["Message"] = "Success: Delete Successfully";
        return RedirectToAction(nameof(Index));
    }
}