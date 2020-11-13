using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyBuy.Data;
using EasyBuy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        //for create View,just use IActionResult because we are not passing anything , just returning bach to the view
        //Get-Create View
        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0) 
                {
                    //when we are saving image in database, image type should be byte of streams. Data will be uploaded as stream of bytes in database
                    byte[] pictureInBytes = null;
                    using (var fileStream = files[0].OpenReadStream()) 
                    {
                        using var memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);
                        pictureInBytes = memoryStream.ToArray();
                    }
                    coupon.CouponImage = pictureInBytes;
                }
                _db.Coupon.Add(coupon);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Edit(int?id) 
        {
            if (id == null) 
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.SingleOrDefaultAsync(c => c.Id == id);
            if (couponFromDb == null) 
            {
                return NotFound();
            }
            return View(couponFromDb);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon) 
        {
            if (coupon.Id == 0)
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.Where(s => s.Id == coupon.Id).FirstOrDefaultAsync();
            //FirstorDefault returns first element of sequence, if not found it returns Nullable.

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0) 
                {
                    byte[] couponPictureInBytes = null;
                    using (var fileStream = files[0].OpenReadStream())
                    {
                        using var memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);
                        couponPictureInBytes = memoryStream.ToArray();
                    }
                    couponFromDb.CouponImage = couponPictureInBytes;
                }
                couponFromDb.Name = coupon.Name;
                couponFromDb.CouponType = coupon.CouponType;
                couponFromDb.Discount = coupon.Discount;
                couponFromDb.MinimunOrderAmount = coupon.MinimunOrderAmount;
                couponFromDb.IsActive = coupon.IsActive;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Details(int? id) 
        {
            if (id == null) 
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.SingleOrDefaultAsync(c => c.Id == id);
            if (couponFromDb == null)
            {
                return NotFound();
            }
            return View(couponFromDb);
        }

        public async Task<IActionResult> Delete(int? id) 
        {
            if (id == null) 
            {
                return NotFound();
            }
            var coupon = await _db.Coupon.SingleOrDefaultAsync(c=>c.Id==id);
            if (coupon == null) 
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id) 
        {
            var coupon = await _db.Coupon.SingleOrDefaultAsync(c => c.Id == id);
            if (coupon == null) 
            {
                return NotFound();
            }
            _db.Coupon.Remove(coupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}