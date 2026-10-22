using CarRentalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext db;

        public UserController(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }
        private void LoadProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                var user = db.Users.FirstOrDefault(x => x.Id == userId);

                if (user != null)
                {
                    ViewBag.ProfileImage = user.ProfileImage;
                    ViewBag.UserName = user.FullName;
                    ViewBag.UserRole = user.Role;
                }
            }
        }
        // CAR LIST

        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            LoadProfile();

            var today = DateTime.Today;

            // 1. Release cars whose booking EndDate has passed
            var pastBookings = db.Bookings
                .Include(b => b.Car)
                .Where(b => (b.Status == "Confirmed" || b.Status == "Active" || b.PaymentStatus == "Paid" || b.Status == "Paid")
                            && b.EndDate.Date < today)
                .ToList();

            bool changes = false;
            foreach (var b in pastBookings)
            {
                b.Status = "Completed";
                changes = true;
                if (b.Car != null && b.Car.VehicleStatus != "Under Maintenance" && b.Car.VehicleStatus != "Inactive")
                {
                    bool hasActiveOther = db.Bookings.Any(o =>
                        o.CarId == b.CarId &&
                        o.Id != b.Id &&
                        (o.Status == "Confirmed" || o.Status == "Active" || o.PaymentStatus == "Paid" || o.Status == "Paid") &&
                        o.StartDate.Date <= today && o.EndDate.Date >= today);

                    if (!hasActiveOther)
                    {
                        b.Car.Available = true;
                        b.Car.VehicleStatus = "Available";
                    }
                }
            }
            if (changes)
            {
                db.SaveChanges();
            }

            // 2. Fetch all cars and determine effective status based on dates
            var allCars = db.Cars.ToList();

            if (startDate.HasValue && endDate.HasValue)
            {
                var sDate = startDate.Value.Date;
                var eDate = endDate.Value.Date;

                foreach (var car in allCars)
                {
                    if (car.VehicleStatus != "Under Maintenance" && car.VehicleStatus != "Inactive")
                    {
                        bool isBookedForDates = db.Bookings.Any(b =>
                            b.CarId == car.Id &&
                            b.Status != "Cancelled" &&
                            (b.Status == "Confirmed" || b.Status == "Active" || b.PaymentStatus == "Paid" || b.Status == "Paid") &&
                            sDate < b.EndDate.Date &&
                            eDate > b.StartDate.Date);

                        car.VehicleStatus = isBookedForDates ? "Booked" : "Available";
                        car.Available = !isBookedForDates;
                    }
                }
            }
            else
            {
                foreach (var car in allCars)
                {
                    if (car.VehicleStatus != "Under Maintenance" && car.VehicleStatus != "Inactive")
                    {
                        bool isBookedToday = db.Bookings.Any(b =>
                            b.CarId == car.Id &&
                            b.Status != "Cancelled" &&
                            (b.Status == "Confirmed" || b.Status == "Active" || b.PaymentStatus == "Paid" || b.Status == "Paid") &&
                            b.StartDate.Date <= today &&
                            b.EndDate.Date >= today);

                        car.VehicleStatus = isBookedToday ? "Booked" : "Available";
                        car.Available = !isBookedToday;
                    }
                }
            }

            // Put available cars in front!
            var sortedCars = allCars
                .OrderByDescending(c => c.VehicleStatus == "Available")
                .ThenBy(c => c.VehicleStatus == "Booked" ? 1 : 2)
                .ThenByDescending(c => c.Id)
                .ToList();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                ViewBag.IsVerified = user?.IsVerified ?? false;
                ViewBag.ActiveUserBookingsCount = db.Bookings.Count(b => b.UserId == userId && (b.Status == "Confirmed" || b.Status == "Active" || b.Status == "Paid"));
            }
            ViewBag.TotalCarsCount = allCars.Count;
            ViewBag.AvailableCount = sortedCars.Count(c => c.VehicleStatus == "Available");
            ViewBag.BookedCount = sortedCars.Count(c => c.VehicleStatus == "Booked");

            return View(sortedCars);
        }
        // ===============================
        // MY PROFILE
        // ===============================

        public IActionResult Profile()
        {
            LoadProfile();
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            // Booking Summary

            ViewBag.TotalBookings = db.Bookings
                .Count(x => x.UserId == userId);

            ViewBag.ActiveBookings = db.Bookings
                .Count(x => x.UserId == userId &&
                            (x.Status == "Approved" || x.Status == "Paid"));

            ViewBag.CompletedBookings = db.Bookings
                .Count(x => x.UserId == userId &&
                            x.Status == "Completed");

            ViewBag.CancelledBookings = db.Bookings
                .Count(x => x.UserId == userId &&
                            x.Status == "Cancelled");

            ViewBag.TotalSpent = db.Bookings
                .Where(x => x.UserId == userId &&
                            x.Status != "Cancelled")
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(User model, IFormFile? ProfilePhoto)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == model.Id);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.Pincode = model.Pincode;
            user.DrivingLicenseNumber = model.DrivingLicenseNumber;
            user.DateOfBirth = model.DateOfBirth;

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await ProfilePhoto.CopyToAsync(ms);
                    user.ProfileImage = ms.ToArray();
                }
            }

            db.SaveChanges();

            // Update session values
            HttpContext.Session.SetString("Name", user.FullName!);

            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction("Profile");
        }
        // CAR DETAILS
        public IActionResult CarDetails(int? id, int? carId)
        {
            LoadProfile();
            int? targetId = id ?? carId;
            if (targetId.HasValue)
            {
                HttpContext.Session.SetInt32("SelectedCarId", targetId.Value);
                return RedirectToAction("CarDetails");
            }

            int? sessionCarId = HttpContext.Session.GetInt32("SelectedCarId");
            if (sessionCarId == null)
            {
                return RedirectToAction("Index");
            }

            var car = db.Cars.Find(sessionCarId.Value);
            if (car == null)
            {
                return RedirectToAction("Index");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            bool isVerified = false;
            bool hasBookedThisCar = false;

            if (userId != null)
            {
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                if (user != null)
                {
                    isVerified = user.IsVerified;
                }

                hasBookedThisCar = db.Bookings.Any(b =>
                    b.UserId == userId &&
                    b.CarId == sessionCarId.Value &&
                    b.Status != "Cancelled");
            }

            ViewBag.IsVerified = isVerified;
            ViewBag.HasBookedThisCar = hasBookedThisCar;

            return View(car);
        }

        // BOOK CAR PAGE
        public IActionResult BookCar(int? id, int? carId)
        {
            LoadProfile();
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!user.IsVerified)
            {
                TempData["Error"] = "Please complete identity verification and wait for admin approval before booking.";
                return RedirectToAction("Verification");
            }

            int? targetId = id ?? carId;
            if (targetId.HasValue)
            {
                HttpContext.Session.SetInt32("SelectedCarId", targetId.Value);
                return RedirectToAction("BookCar");
            }

            int? sessionCarId = HttpContext.Session.GetInt32("SelectedCarId");
            if (sessionCarId == null)
            {
                return RedirectToAction("Index");
            }

            var car = db.Cars.Find(sessionCarId.Value);
            if (car == null)
            {
                return RedirectToAction("Index");
            }

            return View(car);
        }
        // SAVE BOOKING

        [HttpPost]
        public IActionResult BookCar(
            int CarId,
            DateTime StartDate,
            DateTime EndDate)
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");
            var user =
    db.Users.FirstOrDefault(x =>
        x.Id == userId);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (!user.IsVerified)
            {
                TempData["Error"] =
                    "Your identity is not verified yet.";

                return RedirectToAction(
                    "Verification");
            }

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var car = db.Cars.Find(CarId);

            if (car == null)
            {
                return RedirectToAction("Index");
            }

            // CHECK CAR AVAILABILITY
            if (car.VehicleStatus == "Under Maintenance" || car.VehicleStatus == "Inactive")
            {
                ViewBag.Error = "This vehicle is currently under maintenance or unavailable.";
                return View(car);
            }

            // VALIDATE DATES
            if (StartDate >= EndDate)
            {
                ViewBag.Error = "End Date must be greater than Start Date.";
                return View(car);
            }

            if (StartDate.Date < DateTime.Today)
            {
                ViewBag.Error = "Pickup Date cannot be in the past.";
                return View(car);
            }

            // CHECK OVERLAPPING BOOKINGS
            bool isAlreadyBooked = db.Bookings.Any(b =>
                b.CarId == CarId &&
                b.Status != "Cancelled" &&
                (b.Status == "Confirmed" || b.Status == "Active" || b.PaymentStatus == "Paid" || b.Status == "Paid") &&
                StartDate.Date < b.EndDate.Date &&
                EndDate.Date > b.StartDate.Date);

            if (isAlreadyBooked)
            {
                ViewBag.Error = "This vehicle is already reserved for the selected dates. Please choose a different date range.";
                return View(car);
            }

            int days =
                (EndDate - StartDate).Days;

            decimal totalAmount =
                days * car.PricePerDay;

            // SAVE BOOKING DATA IN SESSION

            HttpContext.Session.SetInt32(
                "CarId",
                CarId);

            HttpContext.Session.SetString(
                "StartDate",
                StartDate.ToString("yyyy-MM-dd"));

            HttpContext.Session.SetString(
                "EndDate",
                EndDate.ToString("yyyy-MM-dd"));

            HttpContext.Session.SetInt32(
                "TotalDays",
                days);

            HttpContext.Session.SetString(
                "TotalAmount",
                totalAmount.ToString());

            bool alreadyBooked = db.Bookings.Any(b =>
    b.CarId == CarId &&
    b.Status != "Cancelled" &&
    StartDate < b.EndDate &&
    EndDate > b.StartDate);

            if (alreadyBooked)
            {
                ViewBag.Error = "This car is already booked for the selected dates.";
                return View(car);
            }
            if (StartDate >= EndDate)
            {
                ViewBag.Error = "End Date must be greater than Start Date.";
                return View(car);
            }
            return RedirectToAction(
                "Checkout",
                "Payment",
                new
                {
                    amount = totalAmount
                });
            
        }

        public IActionResult MyBookings()
        {
            LoadProfile();
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var bookings =
                db.Bookings
                .Include(x => x.Car)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.BookingDate)
                .ToList();

            return View(bookings);
        }

    // BOOKING SUCCESS PAGE

    public IActionResult BookingSuccess(
            int bookingId)
        {
            LoadProfile();
            var booking =
                db.Bookings
                .Include(x => x.Car)
                .Include(x => x.User)
                .FirstOrDefault(x =>
                    x.Id == bookingId);

            if (booking == null)
            {
                return RedirectToAction(
                    "Index");
            }

            return View(booking);
        }

        // RECEIPT / INVOICE PAGE
        public IActionResult Receipt(int? bookingId, int? id)
        {
            LoadProfile();
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? targetBookingId = bookingId ?? id;
            if (targetBookingId.HasValue)
            {
                HttpContext.Session.SetInt32("ReceiptBookingId", targetBookingId.Value);
                return RedirectToAction("Receipt");
            }

            int? sessionBookingId = HttpContext.Session.GetInt32("ReceiptBookingId");
            if (sessionBookingId == null)
            {
                return RedirectToAction("MyBookings");
            }

            var booking = db.Bookings
                .Include(x => x.Car)
                .Include(x => x.User)
                .FirstOrDefault(x => x.Id == sessionBookingId.Value);

            if (booking == null)
            {
                return RedirectToAction("MyBookings");
            }

            string? role = HttpContext.Session.GetString("Role");
            if (booking.UserId != userId && role != "Admin")
            {
                return RedirectToAction("MyBookings");
            }

            return View(booking);
        }

        // VERIFICATION PAGE

        public IActionResult Verification()
        {
            LoadProfile();
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var verification =
                db.UserVerifications
                .FirstOrDefault(x =>
                    x.UserId == userId);

            return View(verification);
        }

        // SAVE VERIFICATION

        [HttpPost]
        public async Task<IActionResult> Verification(
            IFormFile GovernmentId,
            IFormFile LicenseFront,
            IFormFile LicenseBack,
            IFormFile DigitalSignature)
        {
            // CHECK USER LOGIN

            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // CHECK REQUIRED DOCUMENTS

            if (GovernmentId == null ||
                LicenseFront == null ||
                LicenseBack == null ||
                DigitalSignature == null)
            {
                TempData["Message"] =
                    "Please upload all required documents.";

                return RedirectToAction(
                    "Verification");
            }

            // PREVENT MULTIPLE SUBMISSIONS

            var oldVerification =
                db.UserVerifications
                .FirstOrDefault(x =>
                    x.UserId == userId);

            if (oldVerification != null)
            {
                TempData["Message"] =
                    "Verification already submitted.";

                return RedirectToAction(
                    "Verification");
            }

            UserVerification verification =
                new UserVerification();

            verification.UserId =
                userId.Value;

            // SAVE GOVERNMENT ID

            using (var ms =
                new MemoryStream())
            {
                await GovernmentId.CopyToAsync(ms);

                verification.GovernmentId =
                    ms.ToArray();
            }

            // SAVE LICENSE FRONT

            using (var ms =
                new MemoryStream())
            {
                await LicenseFront.CopyToAsync(ms);

                verification.LicenseFront =
                    ms.ToArray();
            }

            // SAVE LICENSE BACK

            using (var ms =
                new MemoryStream())
            {
                await LicenseBack.CopyToAsync(ms);

                verification.LicenseBack =
                    ms.ToArray();
            }

            // SAVE DIGITAL SIGNATURE

            using (var ms =
                new MemoryStream())
            {
                await DigitalSignature.CopyToAsync(ms);

                verification.DigitalSignature =
                    ms.ToArray();
            }

            verification.Status =
                "Pending";

            verification.UploadDate =
                DateTime.Now;

            db.UserVerifications.Add(
                verification);

            await db.SaveChangesAsync();

            TempData["Message"] =
                "Documents uploaded successfully.";

            return RedirectToAction(
                "Verification");
        }
       
        // EDIT VERIFICATION PAGE

        public IActionResult EditVerification()
        {
            LoadProfile();
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var verification =
                db.UserVerifications
                .FirstOrDefault(x =>
                    x.UserId == userId);

            if (verification == null)
            {
                return RedirectToAction(
                    "Verification");
            }

            return View(verification);
        }

        // UPDATE VERIFICATION

        [HttpPost]
        public async Task<IActionResult> EditVerification(
            IFormFile? GovernmentId,
            IFormFile? LicenseFront,
            IFormFile? LicenseBack,
            IFormFile? DigitalSignature)
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var verification =
                db.UserVerifications
                .FirstOrDefault(x =>
                    x.UserId == userId);

            if (verification == null)
            {
                return RedirectToAction(
                    "Verification");
            }

            if (GovernmentId != null)
            {
                using var ms = new MemoryStream();

                await GovernmentId.CopyToAsync(ms);

                verification.GovernmentId =
                    ms.ToArray();
            }

            if (LicenseFront != null)
            {
                using var ms = new MemoryStream();

                await LicenseFront.CopyToAsync(ms);

                verification.LicenseFront =
                    ms.ToArray();
            }

            if (LicenseBack != null)
            {
                using var ms = new MemoryStream();

                await LicenseBack.CopyToAsync(ms);

                verification.LicenseBack =
                    ms.ToArray();
            }

            if (DigitalSignature != null)
            {
                using var ms = new MemoryStream();

                await DigitalSignature.CopyToAsync(ms);

                verification.DigitalSignature =
                    ms.ToArray();
            }

            verification.Status = "Pending";

            verification.Remarks = null;

            verification.VerifiedDate = null;

            await db.SaveChangesAsync();

            TempData["Message"] =
                "Verification updated successfully.";

            return RedirectToAction(
                "Verification");
        }

        // DELETE VERIFICATION

        public IActionResult DeleteVerification()
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var verification =
                db.UserVerifications
                .FirstOrDefault(x =>
                    x.UserId == userId);

            if (verification != null)
            {
                db.UserVerifications.Remove(
                    verification);

                db.SaveChanges();
            }

            TempData["Message"] =
                "Verification deleted successfully.";

            return RedirectToAction(
                "Verification");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Please fill in all password fields.";
                return RedirectToAction("Profile");
            }

            if (user.Password != currentPassword.Trim())
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
                TempData["Error"] = "New password and confirmation password do not match.";
                return RedirectToAction("Profile");
            }

            user.Password = newPassword.Trim();
            db.SaveChanges();

            TempData["Success"] = "Your password has been changed successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAccount()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                    return RedirectToAction("Login", "Account");

                var user = db.Users.FirstOrDefault(x => x.Id == userId);

                if (user == null)
                    return RedirectToAction("Login", "Account");

                // Release any cars currently reserved by this user
                var activeUserBookings = db.Bookings
                    .Include(b => b.Car)
                    .Where(b => b.UserId == userId && (b.Status == "Confirmed" || b.Status == "Active" || b.PaymentStatus == "Paid" || b.Status == "Paid"))
                    .ToList();

                foreach (var b in activeUserBookings)
                {
                    if (b.Car != null && b.Car.VehicleStatus != "Under Maintenance" && b.Car.VehicleStatus != "Inactive")
                    {
                        b.Car.Available = true;
                        b.Car.VehicleStatus = "Available";
                    }
                }

                // Delete all verification records
                var verifications = db.UserVerifications
                                      .Where(x => x.UserId == userId)
                                      .ToList();

                if (verifications.Any())
                    db.UserVerifications.RemoveRange(verifications);

                // Delete all bookings
                var bookings = db.Bookings
                                 .Where(x => x.UserId == userId)
                                 .ToList();

                if (bookings.Any())
                    db.Bookings.RemoveRange(bookings);

                db.SaveChanges();

                // Delete user
                db.Users.Remove(user);
                db.SaveChanges();

                HttpContext.Session.Clear();

                TempData["Success"] = "Your account has been deleted successfully.";

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Account deletion failed: " + (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction("Profile");
            }
        }

    }
}