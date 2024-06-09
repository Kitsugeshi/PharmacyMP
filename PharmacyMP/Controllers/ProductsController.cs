using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyMP.Models;

namespace Pharmacy.Controllers
{
    public class ProductsController : Controller
    {
        private readonly PharmacyDbContext _context;

        public ProductsController(PharmacyDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
              return _context.Products != null ? 
                          View(await _context.Products.ToListAsync()) :
                          Problem("Нет данных в таблице dbo.Products.");
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddToCart(int? id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            var cartItem = new CartItem();
            var cartProduct = await _context.CartItems.FirstOrDefaultAsync(ci => ci.ProductId == id && ci.CartId.ToString() == userId);
            

            if (cartProduct == null)
            {
                cartItem.CartId = Convert.ToInt32(userId);
                cartItem.ProductId = id;
                var prod = _context.Products.FirstOrDefault(c => c.Id == cartItem.ProductId);
                cartItem.Quantity = 1;
                cartItem.Price = prod.Price;
                _context.CartItems.Add(cartItem);
                prod.Quantity -= 1;
                _context.Update(prod);
            }
            else
            {
                var prod = _context.Products.FirstOrDefault(c => c.Id == cartProduct.ProductId);
                cartProduct.Quantity += 1;
                cartProduct.Price += prod.Price;
                _context.Update(cartProduct);
                prod.Quantity -= 1;
                _context.Update(prod);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> PutAway(int? id)
        {
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id);
            var prod = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);

            if (item.Quantity > 1)
            {
                item.Quantity -= 1;
                item.Price -= prod.Price;
                _context.Update(item);
                prod.Quantity += 1;
                _context.Update(prod);
            }
            else
            {
                _context.CartItems.Remove(item);
                prod.Quantity += 1;
                _context.Update(prod);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("PersonalAccount", "Users");
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> BuyAllProducts()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            var cartItems = await _context.CartItems
                .Where(c => c.CartId.ToString() == userId).
                Include(p => p.Product)
                .ToListAsync();

            if (cartItems != null)
            {
                _context.RemoveRange(cartItems);
            }
            else
            {
                return Problem("");
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("PersonalAccount", "Users");
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Description,Quantity,Symptoms,IfPrescription")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Description,Quantity,Symptoms,IfPrescription")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'PharmacyDbContext.Products'  is null.");
            }


            var cartItems = await _context.CartItems.Where(m => m.ProductId == id).ToListAsync();
            if (cartItems != null)
            {
                _context.CartItems.RemoveRange(cartItems);
            }
            await _context.SaveChangesAsync();

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        [HttpGet]
        public JsonResult GetProductNames(string term)
        {
            var productNames = _context.Products
                .Where(p => p.Name.Contains(term))
                .Select(p => p.Name)
                .ToList();

            return Json(productNames);
        }
    }
}
