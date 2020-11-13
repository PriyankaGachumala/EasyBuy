using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyBuy.Data;
using EasyBuy.Models;
using EasyBuy.Models.ViewModels;
using EasyBuy.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public ProductViewModel ProductVM { get; set; }

        public ProductController(ApplicationDbContext db,IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            ProductVM = new ProductViewModel()
            {
                Category = _db.Category,
                Product = new Models.Product()
            };

        }

        //Get-Index
        public async Task<IActionResult> Index()
        {
            var products=await _db.Product.Include(s=>s.Category).Include(s=>s.SubCategory).ToListAsync();
            return View(products);
        }

        //Get-Create View
        public IActionResult Create() 
        {
            return View(ProductVM);
        }

        //Get-CreatePost 
        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            ProductVM.Product.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(ProductVM);
            }
            _db.Product.Add(ProductVM.Product);
            await _db.SaveChangesAsync();

            //Images saving section
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var productFromDb = await _db.Product.FindAsync(ProductVM.Product.Id);
            if (files.Count > 0)
            {
                //files uploaded
                var uploads = Path.Combine(webRootPath, "Images");
                var extension = Path.GetExtension(files[0].FileName);
                using (var filesStream = new FileStream(Path.Combine(uploads, ProductVM.Product.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                productFromDb.Image = @"\Images\" + ProductVM.Product.Id + extension;
            }
            else 
            {
                //If no file id uploaded by user, use default image
                var uploads = Path.Combine(webRootPath, @"Images\" + StaticDetails.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\Images\" + ProductVM.Product.Id + ".png");
                productFromDb.Image = @"\Images\" + ProductVM.Product.Id + ".png";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        //Get-Edit View
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductVM.Product = await _db.Product.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(s => s.Id == id);
            ProductVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == ProductVM.Product.CategoryId).ToListAsync();

            if (ProductVM.Product == null) 
            {
                return NotFound();
            }
            return View(ProductVM);
        }

        //Get-EditPost 
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductVM.Product.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                ProductVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == ProductVM.Product.CategoryId).ToListAsync();
                return View(ProductVM);
            }

            //Images saving section
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var productFromDb = await _db.Product.FindAsync(ProductVM.Product.Id);
            if (files.Count > 0)
            {
                //If User selects new Image
                var uploads = Path.Combine(webRootPath, "Images");
                var newExtension = Path.GetExtension(files[0].FileName);
                var imagePath = Path.Combine(webRootPath, productFromDb.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                using (var filesStream = new FileStream(Path.Combine(uploads, ProductVM.Product.Id + newExtension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                productFromDb.Image = @"\Images\" + ProductVM.Product.Id + newExtension;
            }
            productFromDb.Name = ProductVM.Product.Name;
            productFromDb.Description = ProductVM.Product.Description;
            productFromDb.Price = ProductVM.Product.Price;
            productFromDb.CategoryId = ProductVM.Product.CategoryId;
            productFromDb.SubCategoryId = ProductVM.Product.SubCategoryId;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Get-Details View
        public async Task<IActionResult> Details(int?id) 
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductVM.Product = await _db.Product.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(s => s.Id == id);
            if (ProductVM.Product == null)
            {
                NotFound();
            }
            return View(ProductVM);
        }

        //Get-Delete View
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductVM.Product = await _db.Product.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(s => s.Id == id);
            if (ProductVM.Product == null)
            {
                return NotFound();
            }
            return View(ProductVM);
        
        }

        //Get-DeletePost
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) 
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            Product product = await _db.Product.FindAsync(id);
            if (product != null)
            {
                var imagePath = Path.Combine(webRootPath, product.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath)) 
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.Product.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}