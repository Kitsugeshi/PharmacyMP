using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyMP.Models;
using System.Security.Claims;

namespace PharmacyMP.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly PharmacyDbContext _context;

        public AuthorizationController(PharmacyDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            if (ModelState.IsValid)
            {
                user.RoleId = 2;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var userCart = new Cart();
                userCart.Id = user.Id;
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
                await Authorization(user.Login, user.Password);
                return RedirectToAction("Index", "Products");
            }
            else
            {
                return View(user);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Authorization(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Email и/или пароль не установлены.");
            }

            var worker = await _context.Users.FirstOrDefaultAsync(w => w.Login == login && w.Password == password);
            if (worker == null)
            {
                return Unauthorized("Такого пользователя не найдено. Проверьте, правильно ли введены логин и пароль.");
            }

            // Получение должности пользователя из базы данных
            var jobtitle = await _context.Roles.FirstOrDefaultAsync(j => j.Id == worker.RoleId);
            if (jobtitle == null)
            {
                return NotFound("Должность не найдена.");
            }

            // Создание claims для аутентификации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, worker.Login),
                new Claim(ClaimTypes.Role, jobtitle.Name),
                new Claim("Id", worker.Id.ToString(), ClaimValueTypes.Integer32)
                // Другие claims, если необходимо
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Аутентификация пользователя
            if (HttpContext != null)
            {
                await  HttpContext.SignInAsync(claimsPrincipal);
            }

            if (HttpContext != null)
            {
            return RedirectToAction("Index", "Products");
            }

            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Products");
        }
    }
}
