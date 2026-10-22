using CarRentalSystem.Models;
using CarRentalSystem.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly Account account;

        public AccountController(Account accountService)
        {
            account = accountService;
        }

        // LOGIN PAGE

        public IActionResult Login()
        {
            return View();
        }

        // LOGIN

        [HttpPost]
        public IActionResult Login(
            string email,
            string password,
            string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both Email and Password.";
                return View();
            }

            try
            {
                var user =
                    account.Login(
                        email.Trim(),
                        password);

                if (user == null)
                {
                    ViewBag.Error =
                        "Invalid Email or Password";

                    return View();
                }

                if (user.Role != role)
                {
                    ViewBag.Error =
                        "Invalid Role selected.";

                    return View();
                }

                if (user.IsBlocked)
                {
                    ViewBag.Error =
                        "Your account has been blocked by Admin";

                    return View();
                }

                HttpContext.Session.SetInt32(
                    "UserId",
                    user.Id);

                HttpContext.Session.SetString(
                    "Role",
                    user.Role ?? "User");

                HttpContext.Session.SetString(
                    "Name",
                    user.FullName ?? "User");

                // ADMIN
                if (user.Role == "Admin")
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Admin");
                }

                // USER
                return RedirectToAction(
                    "Index",
                    "User");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Login failed: " + (ex.InnerException?.Message ?? ex.Message);
                return View();
            }
        }

        // REGISTER PAGE

        public IActionResult Register()
        {
            return View();
        }

        // REGISTER

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ViewBag.Error = "Full Name is required.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ViewBag.Error = "Email is required.";
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ViewBag.Error = "Password is required.";
                return View(user);
            }

            user.Role = "User";

            try
            {
                account.Register(user);
                TempData["Success"] = "Account created successfully! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException?.Message ?? ex.Message;
                if (msg.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Error = "An account with this email address already exists.";
                }
                else
                {
                    ViewBag.Error = "Registration failed: " + msg;
                }
                return View(user);
            }
        }

        // LOGOUT

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login");
        }
    }
}