using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Pharmacy.ViewModels;
using PharmacyMP.Controllers;
using PharmacyMP.Models;
using System.Security.Claims;

namespace PharmacyTests
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithAListOfUsers()
        {
            //Arrange
            var users = new List<User>
            {
                new User { Id = 1, Name = "User1", Role = new Role { Name = "Admin" } },
                new User { Id = 2, Name = "User2", Role = new Role { Name = "User" } }
            }.AsQueryable();

            var mockContext = new Mock<PharmacyDbContext>();
            mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            var controller = new UsersController(mockContext.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task PersonalAccount_ReturnsViewResult_WithAccountViewModel()
        {
            // Arrange
            var userId = "1";

            var users = new List<User>
            {
                new User { Id = 1, Name = "User1", Role = new Role { Name = "Admin" } }
            }.AsQueryable();

            var cartItems = new List<CartItem>
            {
                new CartItem { CartId = 1, Product = new Product { Name = "Product1" } },
                new CartItem { CartId = 1, Product = new Product { Name = "Product2" } }
            }.AsQueryable();

            var mockContext = new Mock<PharmacyDbContext>();
            mockContext.Setup(x => x.Users).ReturnsDbSet(users);
            mockContext.Setup(x => x.CartItems).ReturnsDbSet(cartItems);

            var claims = new List<Claim>
            {
                new Claim("Id", userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

            var controller = new UsersController(mockContext.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var result = await controller.PersonalAccount();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AccountViewModel>(viewResult.ViewData.Model);
            Assert.Equal(2, model.CartItems.Count);
            Assert.Equal(2, model.Products.Count);
            Assert.Equal("User1", model.User.Name);
            Assert.Equal("Product1", model.Products[0].Name);
            Assert.Equal("Product2", model.Products[1].Name);
        }

        [Fact]
        public void Create_ReturnsViewResult_WithRoleIdSelectList()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            }.AsQueryable();

            var mockContext = new Mock<PharmacyDbContext>();
            mockContext.Setup(x => x.Roles).ReturnsDbSet(roles);

            var controller = new UsersController(mockContext.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewData = viewResult.ViewData["RoleId"];
            Assert.IsType<SelectList>(viewData);
        }
        [Fact]
        public async Task Create_Post_ReturnsRedirectToAction_WhenModelIsValid()
        {
            // Arrange
            var mockContext = new Mock<PharmacyDbContext>();
            var users = new List<User>().AsQueryable();
            var carts = new List<Cart>().AsQueryable();
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin" }
            }.AsQueryable();

            // Setup DbSets
            mockContext.Setup(x => x.Users).ReturnsDbSet(users);
            mockContext.Setup(x => x.Carts).ReturnsDbSet(carts);
            mockContext.Setup(x => x.Roles).ReturnsDbSet(roles);

            var controller = new UsersController(mockContext.Object);

            // Mock user authentication
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var newUser = new User
            {
                Id = 1,
                Login = "testlogin",
                Password = "testpassword",
                RoleId = 1,
                Name = "Test User",
                Gender = "Male",
                Phone = "123456789",
                Email = "test@example.com"
            };

            // Act
            var result = await controller.Create(newUser);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithUser()
        {
            // Arrange
            var mockContext = new Mock<PharmacyDbContext>();
            var users = new List<User>
            {
                new User { Id = 1, Name = "Test User" }
            }.AsQueryable(); // Коллекция с одним пользователем

            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }

            }.AsQueryable();

            mockContext.Setup(x => x.Users).ReturnsDbSet(users);
            mockContext.Setup(x => x.Roles).ReturnsDbSet(roles);

            var controller = new UsersController(mockContext.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<User>(viewResult.ViewData.Model);
            Assert.Equal("Test User", model.Name);
        }

        [Fact]
        public async Task Edit_Post_ReturnsRedirectToActionResult_WhenModelIsValid()
        {
            // Arrange
            var mockContext = new Mock<PharmacyDbContext>();
            var users = new List<User>
            {
                new User { Id = 1, Name = "Test User" }
            }.AsQueryable();

            mockContext.Setup(x => x.Users).ReturnsDbSet(users);

            var controller = new UsersController(mockContext.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var editedUser = new User
            {
                Id = 1,
                Name = "Edited Test User"
            };

            // Act
            var result = await controller.Edit(1, editedUser);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
    }
}