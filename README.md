# 🚗 EliteCar — Luxury Car Rental System

An enterprise-grade, luxury car rental platform built with **ASP.NET Core (.NET 10.0) MVC**, **Entity Framework Core**, **SQL Server**, and **Stripe Payment Gateway**.

---

## ✨ Features

### 👤 Driver & Customer Portal
* **Car Rental Showcase & Date Search**: Real-time vehicle availability search by pickup and return dates with automatic conflict detection.
* **Smart Filtering**: Filter cars by transmission (Automatic/Manual), fuel type (Petrol/Diesel/EV), price, and category.
* **Interactive Leaflet GPS Maps**: Station location and GPS pickup pin rendering (unlocked upon confirmed reservation).
* **Digital Identity Verification**: One-time upload and automated/admin approval of driving license and government IDs.
* **Stripe Checkout**: Seamless and secure payment gateway integration with 256-bit SSL encryption.
* **Tax Invoices & Receipts**: Instant GST-compliant invoice generation with printable and downloadable PDF receipts.
* **Driver Profile & Security**: Manage personal details, update password with modern secure controls, and view booking history.

### 🛡️ Executive Admin Command Center
* **Live KPI Dashboard**: Real-time tracking of gross revenue, total reservations, vehicle inventory readiness (Available, On Trip, In Service), and customer registrations.
* **Car Rental Management**: Full CRUD operations for vehicles with detailed technical specs (transmission, fuel, seats, mileage, reg number, daily rates) and Leaflet GPS pin configuration.
* **Reservation Dossier Stream**: Filter reservations by status (`Paid & Confirmed`, `Pending`, `Cancelled`) with customer profiles and station logistics.
* **Customer Verification Queue**: Review submitted government credentials with one-click approve/reject actions and feedback notes.
* **User Accounts & Access**: Monitor registered customers, verify status badges, and regulate access.

---

## 🛠️ Technology Stack

| Layer | Technology |
|---|---|
| **Framework** | .NET 10.0 (ASP.NET Core MVC) |
| **ORM** | Entity Framework Core (Code-First / DbContext) |
| **Database** | Microsoft SQL Server (LocalDB / Express) |
| **Payments** | Stripe API (Stripe.net) |
| **Mapping & GPS** | Leaflet.js & OpenStreetMap |
| **Frontend** | Razor Views (`.cshtml`), Bootstrap 5, FontAwesome 6, Custom Luxury CSS |
| **Authentication** | Session-based Auth & Role-based Access Control (Admin / User) |

---

## 🚀 Getting Started

### Prerequisites
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Developer Edition)
* [Visual Studio 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/)

### 1. Clone the Repository
```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPOSITORY.git
cd YOUR_REPOSITORY
```

### 2. Configure Database & Stripe
Update your database connection string and Stripe API keys in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "EliteCarConnection": "Data Source=localhost;Initial Catalog=CarrentalsystemDB;Integrated Security=True;Encrypt=False"
  },
  "Stripe": {
    "PublishableKey": "pk_test_your_publishable_key",
    "SecretKey": "sk_test_your_secret_key"
  }
}
```

### 3. Apply Migrations / Create Database
```bash
dotnet ef database update
```
*(Or ensure the database `CarrentalsystemDB` is created in SQL Server)*

### 4. Run the Application
```bash
dotnet build
dotnet run
```

Navigate to: `https://localhost:7186` (or the configured port).

---

## 📂 Project Structure

```
├── Controllers/         # MVC Controllers (Admin, User, Home, Account)
├── Data/                # DbContext & Database configuration
├── Models/              # Domain entities (Car, Booking, User, UserVerification)
├── Repository/          # Data access services
├── Views/               # Razor View templates
│   ├── Admin/           # Admin panel pages (Dashboard, ManageCar, Bookings, Users, Verification)
│   ├── User/            # Driver portal pages (CarDetails, BookCar, MyBookings, Receipt)
│   ├── Home/            # Public luxury landing page
│   └── Shared/          # Master layouts (_AdminLayout, _UserLayout, _Layout)
├── wwwroot/             # Static assets (Custom luxury CSS, JavaScript, Images)
├── appsettings.json     # Configuration file
└── Program.cs           # Application bootstrap & middleware pipeline
```

---

## 📜 License
This project is licensed under the [MIT License](LICENSE).
