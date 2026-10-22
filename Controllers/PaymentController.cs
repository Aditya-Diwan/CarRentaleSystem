using CarRentalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace CarRentalSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext db;

        public PaymentController(
            ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        // STRIPE CHECKOUT

        public IActionResult Checkout(
            decimal amount)
        {
            var options =
                new SessionCreateOptions
                {
                    PaymentMethodTypes =
                        new List<string>
                        {
                            "card"
                        },

                    LineItems =
                        new List<SessionLineItemOptions>
                        {
                            new SessionLineItemOptions
                            {
                                PriceData =
                                    new SessionLineItemPriceDataOptions
                                    {
                                        Currency = "inr",

                                        UnitAmount =
                                            (long)(amount * 100),

                                        ProductData =
                                            new SessionLineItemPriceDataProductDataOptions
                                            {
                                                Name =
                                                    "EliteCar Booking Payment"
                                            }
                                    },

                                Quantity = 1
                            }
                        },

                    Mode = "payment",

                    SuccessUrl =
                        "https://localhost:7079/Payment/Success",

                    CancelUrl =
                        "https://localhost:7079/Payment/Cancel"
                };

            var service =
                new SessionService();

            Session session =
                service.Create(options);

            return Redirect(session.Url);
        }

        // PAYMENT SUCCESS

        public IActionResult Success()
        {
            int? userId =
                HttpContext.Session.GetInt32(
                    "UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            int carId =
                HttpContext.Session.GetInt32(
                    "CarId") ?? 0;

            string? startDate =
                HttpContext.Session.GetString(
                    "StartDate");

            string? endDate =
                HttpContext.Session.GetString(
                    "EndDate");

            int totalDays =
                HttpContext.Session.GetInt32(
                    "TotalDays") ?? 0;

            decimal totalAmount =
                Convert.ToDecimal(
                    HttpContext.Session.GetString(
                        "TotalAmount"));

            // GET CAR DETAILS

            var car =
                db.Cars.FirstOrDefault(x =>
                    x.Id == carId);

            if (car == null)
            {
                return RedirectToAction(
                    "Index",
                    "User");
            }

            Booking booking =
                new Booking()
                {
                    UserId = userId.Value,

                    CarId = carId,

                    StartDate =
                        Convert.ToDateTime(
                            startDate),

                    EndDate =
                        Convert.ToDateTime(
                            endDate),

                    TotalDays =
                        totalDays,

                    TotalAmount =
                        totalAmount,

                    BookingDate =
                        DateTime.Now,

                    Status = "Paid",

                    // LOCATION SNAPSHOT

                    PickupLocation =
                        car.PickupLocation,

                    Address =
                        car.Address,

                    Landmark =
                        car.Landmark,

                    Latitude =
                        car.Latitude,

                    Longitude =
                        car.Longitude,

                    ReturnLocation =
                        car.PickupLocation,

                    PaymentStatus = "Paid",

                    PaymentMethod = "Stripe"
                };

            // MARK CAR RENTED
            car.Available = false;
            car.VehicleStatus = "Booked";

            db.Bookings.Add(booking);

            db.SaveChanges();

            return RedirectToAction(
                "BookingSuccess",
                "User",
                new
                {
                    bookingId =
                        booking.Id
                });
        }
        // PAYMENT CANCEL

        public IActionResult Cancel()
        {
            return View();
        }
    }
}