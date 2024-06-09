using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;
using PharmacyMP.Controllers;
using PharmacyMP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyTests
{
    public class AuthorizationControllerTests
    {
        [Fact]
        public async Task Registration_ValidUser_RedirectsToChatIndex()
        {
            // Arrange
            var user = new User
            {
                Login = "Test User",
                Password = "test"
            };

            using (var context = new PharmacyDbContext())
            {

                var httpContext = new Mock<HttpContext>();
                var authService = new Mock<IAuthenticationService>();
                httpContext.Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService))).Returns(authService.Object);
                var controller = new AuthorizationController(new PharmacyDbContext());

                // Act
                var result = await controller.Registration(user);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

                Assert.Equal("Index", redirectToActionResult.ActionName);
                Assert.Equal("Products", redirectToActionResult.ControllerName);
                var createdUser = context.Users.FirstOrDefault(u => u.Login == user.Login);
                var cart = context.Carts.FirstOrDefault(c => c.Id == createdUser.Id);

                Assert.Equal(user.Login, createdUser.Login);
                Assert.Equal(user.Password, createdUser.Password);


                context.Carts.Remove(cart);
                context.SaveChanges();
                context.Users.Remove(createdUser);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task Authorization_ValidCredentials_RedirectsToChatIndex()
        {
            var user = new List<User>
            {
                new User
                {
                    Login = "Test User",
                    Password = "test",
                    RoleId = 1,
                }
            }.AsQueryable();

            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            }.AsQueryable();

            // Arrange
            var mockContsxt = new Mock<PharmacyDbContext>();
            mockContsxt.Setup(m => m.Users).ReturnsDbSet(user);
            mockContsxt.Setup(m => m.Roles).ReturnsDbSet(roles);

            var httpContext = new Mock<HttpContext>();
            var authService = new Mock<IAuthenticationService>();
            httpContext.Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService))).Returns(authService.Object);
            var controller = new AuthorizationController(mockContsxt.Object);
            // Act
            var result = await controller.Authorization(user.First().Login, user.First().Password);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);


            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Authorization_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var user = new List<User>
            {
                new User
                {
                    Id = 1,
                    Login = "Test User",
                    Password = "test",
                    RoleId = 1,
                },
                new User
                {
                    Id = 2,
                    Login = "Test User1",
                    Password = "test1",
                    RoleId = 2,
                }
            }.AsQueryable();

            // Arrange
            var mockContsxt = new Mock<PharmacyDbContext>();
            mockContsxt.Setup(m => m.Users).ReturnsDbSet(user);

            var controller = new AuthorizationController(mockContsxt.Object);

            // Act
            var result = await controller.Authorization("invaliduser", "invalidpassword");

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
