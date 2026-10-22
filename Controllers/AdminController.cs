using CarRentalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;

        public AdminController(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }
        private void LoadAdminProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                var admin = db.Users.FirstOrDefault(x => x.Id == userId);

                if (admin != null)
                {
                    ViewBag.ProfileImage = admin.ProfileImage;
                    ViewBag.UserName = admin.FullName;
                    ViewBag.UserRole = admin.Role;
                }
            }
        }
        // =========================================
        // DASHBOARD STATISTICS
        // =========================================

        public IActionResult Dashboard()
        {
            LoadAdminProfile();
            // TOTAL CARS

            ViewBag.TotalCars =
                db.Cars.Count();

            // TOTAL USERS

            ViewBag.TotalUsers =
                db.Users.Count(x =>
                    x.Role == "User");

            // TOTAL BOOKINGS

            ViewBag.TotalBookings =
                db.Bookings.Count();

            // TOTAL REVENUE

            ViewBag.TotalRevenue =
                db.Bookings.Sum(x =>
                    (decimal?)x.TotalAmount) ?? 0;

            // AVAILABLE CARS

            ViewBag.AvailableCars =
                db.Cars.Count(x =>
                    x.Available);

            // BLOCKED USERS

            ViewBag.BlockedUsers =
                db.Users.Count(x =>
                    x.IsBlocked);

            // PENDING VERIFICATIONS

            ViewBag.PendingVerifications =
                db.UserVerifications.Count(x =>
                    x.Status == "Pending");

            // APPROVED VERIFICATIONS

            ViewBag.ApprovedVerifications =
                db.UserVerifications.Count(x =>
                    x.Status == "Approved");

            // BOOKED & MAINTENANCE CARS
            ViewBag.BookedCars =
                db.Cars.Count(x =>
                    !x.Available || x.VehicleStatus == "Booked");

            ViewBag.MaintenanceCars =
                db.Cars.Count(x =>
                    x.VehicleStatus == "Under Maintenance");

            // RECENT BOOKINGS FOR DASHBOARD
            ViewBag.RecentBookings =
                db.Bookings
                .Include(x => x.Car)
                .Include(x => x.User)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToList();

            // PENDING VERIFICATIONS FOR ACTION CENTER
            ViewBag.PendingVerificationsList =
                db.UserVerifications
                .Include(x => x.User)
                .Where(x => x.Status == "Pending")
                .OrderByDescending(x => x.UploadDate)
                .Take(4)
                .ToList();

            return View();
        }
        // =========================================
        // Profile 
        // =========================================
        public IActionResult Profile()
        {
            LoadAdminProfile();

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = db.Users.FirstOrDefault(x => x.Id == userId);

            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Dashboard Summary

            ViewBag.TotalUsers = db.Users.Count(x => x.Role == "User");

            ViewBag.TotalCars = db.Cars.Count();

            ViewBag.TotalBookings = db.Bookings.Count();

            ViewBag.TotalRevenue =
                db.Bookings.Sum(x => (decimal?)x.TotalAmount) ?? 0;

            return View(admin);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(
    User model,
    IFormFile? ProfilePhoto)
        {
            var admin =
                db.Users.FirstOrDefault(x => x.Id == model.Id);

            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            admin.FullName = model.FullName;
            admin.PhoneNumber = model.PhoneNumber;
            admin.Address = model.Address;
            admin.City = model.City;
            admin.State = model.State;
            admin.Pincode = model.Pincode;
            admin.DateOfBirth = model.DateOfBirth;

            if (ProfilePhoto != null &&
                ProfilePhoto.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await ProfilePhoto.CopyToAsync(ms);

                    admin.ProfileImage = ms.ToArray();
                }
            }

            db.SaveChanges();

            HttpContext.Session.SetString(
                "Name",
                admin.FullName!);

            TempData["Success"] =
                "Profile updated successfully.";

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == "Admin");
            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Please provide both current and new passwords.";
                return RedirectToAction("Profile");
            }

            if (admin.Password != currentPassword.Trim())
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters long.";
                return RedirectToAction("Profile");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirm password do not match.";
                return RedirectToAction("Profile");
            }

            admin.Password = newPassword.Trim();
            await db.SaveChangesAsync();

            TempData["Success"] = "Administrator password has been updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var admin = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Role == "Admin");
                if (admin == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int totalAdmins = await db.Users.CountAsync(x => x.Role == "Admin");
                if (totalAdmins <= 1)
                {
                    TempData["Error"] = "Cannot delete the only administrator account in the system.";
                    return RedirectToAction("Profile");
                }

                var verifications = await db.UserVerifications.Where(x => x.UserId == userId).ToListAsync();
                if (verifications.Any()) db.UserVerifications.RemoveRange(verifications);

                var bookings = await db.Bookings.Where(x => x.UserId == userId).ToListAsync();
                if (bookings.Any()) db.Bookings.RemoveRange(bookings);

                db.Users.Remove(admin);
                await db.SaveChangesAsync();

                HttpContext.Session.Clear();
                TempData["Success"] = "Administrator account deleted successfully.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Account deletion failed: " + (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction("Profile");
            }
        }
        // =========================================
        // CAR MANAGEMENT
        // =========================================

        public IActionResult AddCar()
        {
            LoadAdminProfile();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCar(
            Car car,
            IFormFile? ImageFile)
        {
            LoadAdminProfile();

            if (string.IsNullOrWhiteSpace(car.CarName))
            {
                ViewBag.Error = "Vehicle Name is required.";
                return View(car);
            }

            if (string.IsNullOrWhiteSpace(car.CarModel))
            {
                ViewBag.Error = "Vehicle Model is required.";
                return View(car);
            }

            if (string.IsNullOrWhiteSpace(car.Transmission))
            {
                ViewBag.Error = "Please select a transmission type.";
                return View(car);
            }

            if (string.IsNullOrWhiteSpace(car.FuelType))
            {
                ViewBag.Error = "Please select a fuel type.";
                return View(car);
            }

            if (car.PricePerDay <= 0)
            {
                ViewBag.Error = "Rental price per day must be greater than 0.";
                return View(car);
            }

            if (!string.IsNullOrWhiteSpace(car.RegistrationNumber))
            {
                car.RegistrationNumber = car.RegistrationNumber.Trim().ToUpper();
                bool exists = await db.Cars.AnyAsync(x => x.RegistrationNumber == car.RegistrationNumber);
                if (exists)
                {
                    ViewBag.Error = $"A vehicle with registration number '{car.RegistrationNumber}' already exists.";
                    return View(car);
                }
            }
            else
            {
                car.RegistrationNumber = null;
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ImageFile.CopyToAsync(memoryStream);
                    car.CarImage = memoryStream.ToArray();
                }
            }

            if (string.IsNullOrWhiteSpace(car.VehicleStatus))
            {
                car.VehicleStatus = "Available";
            }

            car.Available = (car.VehicleStatus == "Available");

            try
            {
                db.Cars.Add(car);
                await db.SaveChangesAsync();
                TempData["Success"] = "Vehicle added successfully!";
                return RedirectToAction("ManageCar");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to save vehicle: " + (ex.InnerException?.Message ?? ex.Message);
                return View(car);
            }
        }

        public IActionResult ManageCar(string? transmission, string? fuel, string? status, string? search)
        {
            LoadAdminProfile();
            var query = db.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(transmission) && transmission != "All")
            {
                query = query.Where(x => x.Transmission == transmission);
            }

            if (!string.IsNullOrEmpty(fuel) && fuel != "All")
            {
                query = query.Where(x => x.FuelType == fuel);
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(x => x.VehicleStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(x => (x.CarName != null && x.CarName.ToLower().Contains(s))
                                      || (x.CarModel != null && x.CarModel.ToLower().Contains(s))
                                      || (x.RegistrationNumber != null && x.RegistrationNumber.ToLower().Contains(s))
                                      || (x.CarType != null && x.CarType.ToLower().Contains(s))
                                      || (x.PickupLocation != null && x.PickupLocation.ToLower().Contains(s)));
            }

            ViewBag.TotalCars = db.Cars.Count();
            ViewBag.AvailableCars = db.Cars.Count(x => x.VehicleStatus == "Available" || (x.VehicleStatus == null && x.Available));
            ViewBag.BookedCars = db.Cars.Count(x => x.VehicleStatus == "Booked");
            ViewBag.MaintenanceCars = db.Cars.Count(x => x.VehicleStatus == "Under Maintenance");

            ViewBag.CurrentTransmission = transmission ?? "All";
            ViewBag.CurrentFuel = fuel ?? "All";
            ViewBag.CurrentStatus = status ?? "All";
            ViewBag.CurrentSearch = search ?? "";

            return View(query.OrderByDescending(x => x.Id).ToList());
        }

        public IActionResult EditCar(int? id, int? carId)
        {
            LoadAdminProfile();
            int? targetId = id ?? carId;
            if (targetId.HasValue)
            {
                HttpContext.Session.SetInt32("AdminEditCarId", targetId.Value);
                return RedirectToAction("EditCar");
            }

            int? sessionCarId = HttpContext.Session.GetInt32("AdminEditCarId");
            if (sessionCarId == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("ManageCar");
            }

            var car = db.Cars.Find(sessionCarId.Value);

            if (car == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("ManageCar");
            }

            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> EditCar(
            Car car,
            IFormFile? ImageFile)
        {
            LoadAdminProfile();

            var oldCar =
                await db.Cars.FindAsync(car.Id);

            if (oldCar == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction(
                    "ManageCar");
            }

            if (string.IsNullOrWhiteSpace(car.CarName))
            {
                ViewBag.Error = "Vehicle Name is required.";
                return View(car);
            }

            if (string.IsNullOrWhiteSpace(car.CarModel))
            {
                ViewBag.Error = "Vehicle Model is required.";
                return View(car);
            }

            if (string.IsNullOrWhiteSpace(car.Transmission))
            {
                ViewBag.Error = "Please select a transmission type.";
                return View(car);
            }

            if (string.IsNullOrWhiteSpace(car.FuelType))
            {
                ViewBag.Error = "Please select a fuel type.";
                return View(car);
            }

            if (car.PricePerDay <= 0)
            {
                ViewBag.Error = "Rental price per day must be greater than 0.";
                return View(car);
            }

            if (!string.IsNullOrWhiteSpace(car.RegistrationNumber))
            {
                car.RegistrationNumber = car.RegistrationNumber.Trim().ToUpper();
                bool exists = await db.Cars.AnyAsync(x => x.RegistrationNumber == car.RegistrationNumber && x.Id != car.Id);
                if (exists)
                {
                    ViewBag.Error = $"A vehicle with registration number '{car.RegistrationNumber}' already exists.";
                    return View(car);
                }
                oldCar.RegistrationNumber = car.RegistrationNumber;
            }
            else
            {
                oldCar.RegistrationNumber = null;
            }

            // UPDATE IMAGE
            if (ImageFile != null &&
                ImageFile.Length > 0)
            {
                using (var memoryStream =
                    new MemoryStream())
                {
                    await ImageFile.CopyToAsync(
                        memoryStream);

                    oldCar.CarImage =
                        memoryStream.ToArray();
                }
            }

            // BASIC DETAILS
            oldCar.CarName = car.CarName;
            oldCar.CarModel = car.CarModel;
            oldCar.FuelType = car.FuelType;
            oldCar.Transmission = car.Transmission;
            oldCar.PricePerDay = car.PricePerDay;

            // NEW VEHICLE DETAILS
            oldCar.CarType = car.CarType;
            oldCar.ManufacturingYear = car.ManufacturingYear;
            oldCar.SeatingCapacity = car.SeatingCapacity;

            // VEHICLE STATUS
            oldCar.VehicleStatus = string.IsNullOrWhiteSpace(car.VehicleStatus) ? "Available" : car.VehicleStatus;
            oldCar.Available = (oldCar.VehicleStatus == "Available");

            // LOCATION
            oldCar.PickupLocation = car.PickupLocation;
            oldCar.Address = car.Address;
            oldCar.Landmark = car.Landmark;
            oldCar.Latitude = car.Latitude;
            oldCar.Longitude = car.Longitude;

            try
            {
                await db.SaveChangesAsync();
                TempData["Success"] = "Vehicle updated successfully!";
                return RedirectToAction(
                    "ManageCar");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to update vehicle: " + (ex.InnerException?.Message ?? ex.Message);
                return View(car);
            }
        }

        public async Task<IActionResult> DeleteCar(
            int? id,
            int? carId)
        {
            int targetId = carId ?? id ?? 0;
            var car =
                await db.Cars.FindAsync(targetId);

            if (car != null)
            {
                try
                {
                    var bookings =
                        db.Bookings
                        .Where(x => x.CarId == targetId)
                        .ToList();

                    db.Bookings.RemoveRange(
                        bookings);

                    db.Cars.Remove(car);

                    await db.SaveChangesAsync();
                    TempData["Success"] = "Vehicle deleted successfully!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Failed to delete vehicle: " + (ex.InnerException?.Message ?? ex.Message);
                }
            }

            return RedirectToAction(
                "ManageCar");
        }

        // =========================================
        // USER MANAGEMENT
        // =========================================

        public IActionResult Users()
        {
            LoadAdminProfile();
            var users =
                db.Users
                .Where(x =>
                    x.Role == "User")
                .ToList();

            return View(users);
        }

        public IActionResult BlockUser(int? id, int? userId)
        {
            int targetId = userId ?? id ?? 0;
            var user =
                db.Users.Find(targetId);

            if (user != null)
            {
                user.IsBlocked = true;

                db.SaveChanges();
            }

            return RedirectToAction(
                "Users");
        }

        public IActionResult UnblockUser(int? id, int? userId)
        {
            LoadAdminProfile();
            int targetId = userId ?? id ?? 0;
            var user =
                db.Users.Find(targetId);

            if (user != null)
            {
                user.IsBlocked = false;

                db.SaveChanges();
            }

            return RedirectToAction(
                "Users");
        }

        public IActionResult DeleteUser(int? id, int? userId)
        {
            LoadAdminProfile();
            int targetId = userId ?? id ?? 0;
            var user =
                db.Users.Find(targetId);

            if (user != null)
            {
                // DELETE BOOKINGS

                var bookings =
                    db.Bookings
                    .Where(x => x.UserId == targetId)
                    .ToList();

                db.Bookings.RemoveRange(
                    bookings);

                // DELETE VERIFICATIONS

                var verifications =
                    db.UserVerifications
                    .Where(x => x.UserId == targetId)
                    .ToList();

                db.UserVerifications
                    .RemoveRange(verifications);

                db.Users.Remove(user);

                db.SaveChanges();
            }

            return RedirectToAction(
                "Users");
        }

        // =========================================
        // BOOKING MANAGEMENT
        // =========================================

        public IActionResult Bookings()
        {
            LoadAdminProfile();
            var bookings =
                db.Bookings
                .Include(x => x.User)
                .Include(x => x.Car)
                .OrderByDescending(x =>
                    x.BookingDate)
                .ToList();

            return View(bookings);
        }

        // =========================================
        // DOCUMENT VERIFICATION MANAGEMENT
        // =========================================

        public IActionResult Verification()
        {
            LoadAdminProfile();
            var verifications =
                db.UserVerifications
                .Include(x => x.User)
                .OrderByDescending(x =>
                    x.UploadDate)
                .ToList();

            return View(verifications);
        }

        public IActionResult ApproveVerification(int? id, int? verificationId)
        {
            int targetId = verificationId ?? id ?? 0;
            var verification =
                db.UserVerifications
                .FirstOrDefault(x => x.Id == targetId);

            if (verification != null)
            {
                verification.Status = "Approved";

                verification.Remarks =
                    "Documents verified successfully";

                verification.VerifiedDate =
                    DateTime.Now;

                var user =
                    db.Users.Find(verification.UserId);

                if (user != null)
                {
                    user.IsVerified = true;
                }

                db.SaveChanges();
            }

            return RedirectToAction("Verification");
        }
        public IActionResult RejectVerification(int? id, int? verificationId)
        {
            int targetId = verificationId ?? id ?? 0;
            var verification =
                db.UserVerifications
                .FirstOrDefault(x => x.Id == targetId);

            if (verification != null)
            {
                verification.Status = "Rejected";

                verification.Remarks =
                    "Verification rejected by admin";

                verification.VerifiedDate =
                    DateTime.Now;

                var user =
                    db.Users.Find(verification.UserId);

                if (user != null)
                {
                    user.IsVerified = false;
                }

                db.SaveChanges();
            }

            return RedirectToAction("Verification");
        }
        public IActionResult DeleteVerification(
            int? id,
            int? verificationId)
        {
            int targetId = verificationId ?? id ?? 0;
            var verification =
                db.UserVerifications
                .FirstOrDefault(x =>
                    x.Id == targetId);

            if (verification != null)
            {
                db.UserVerifications
                    .Remove(verification);

                db.SaveChanges();
            }

            return RedirectToAction(
                "Verification");
        }
    }
}

