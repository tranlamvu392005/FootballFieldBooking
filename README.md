# ⚽ Football Field Booking Management System

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/Language-C%23-blue.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red.svg)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Frontend-Bootstrap%205-purple.svg)](https://getbootstrap.com/)

A modern, responsive web application for real-time football pitch discovery, conflict-free time slot reservation, automated deposit handling, and pitch performance analytics.

---

## 📖 Table of Contents
- [Overview](#-overview)
- [Key Features](#-key-features)
- [System Architecture & Tech Stack](#-system-architecture--tech-stack)
- [Database Schema](#-database-schema)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation & Setup](#installation--setup)
- [Team & Contributors](#-team--contributors)
- [License](#-license)

---

## 🌟 Overview

The **Football Field Booking System** addresses the limitations of traditional manual pitch rentals (phone calls, double-booking risks, untracked deposits). By applying the MVC architectural pattern with ASP.NET Core and Entity Framework Core, the system automates time slot management, tracks reservation lifecycles, and provides pitch owners with a centralized revenue analytics dashboard.

---

## ✨ Key Features

### 👤 Customer (Players)
* **Pitch Discovery & Filtering:** Browse active fields filtered by pitch type (5-a-side, 7-a-side), location, and dynamic hourly price rates.
* **Real-time Slot Verification:** View real-time available time slots with conflict prevention to avoid duplicate bookings.
* **Reservation & Checkout:** Select desired time frames, place booking requests, and choose payment methods (Cash on arrival / Bank transfer).
* **Order History & Tracking:** Track reservation statuses in real time (`Pending`, `Confirmed`, `Completed`, `Cancelled`).
* **Rating & Reviews:** Rate pitches (1–5 stars) and write comments post-match, dynamically updating average pitch scores.
* **User Profile:** Manage personal contact information and view booking statistics.

### 🛡️ Administrator (Pitch Owners)
* **Interactive Dashboard:** High-level metrics tracking total revenue, monthly trends, confirmed vs. pending bookings, and popular pitches[cite: 2].
* **Pitch Management:** CRUD operations for football pitches, court types, and hourly time slots (including peak/weekend pricing)[cite: 2].
* **Booking Moderation:** Review pending reservations, approve/confirm orders, or handle cancellations[cite: 2].
* **Revenue Analytics:** Visual reporting breakdown by month and pitch type[cite: 2].

---

## 🛠️ System Architecture & Tech Stack

* **Architectural Pattern:** Model-View-Controller (MVC)[cite: 2]
* **Backend:** 
  * C# & ASP.NET Core MVC[cite: 2]
* Entity Framework Core (ORM)[cite: 2]
  * ASP.NET Core Identity (Authentication, Role-based Authorization)[cite: 2]
  * Integrated Email Notification Service (SMTP)[cite: 2]
* **Frontend:** 
  * Razor Views (`.cshtml`), Razor ViewComponents, Custom TagHelpers[cite: 2]
  * Bootstrap 5, HTML5, CSS3[cite: 2]
  * JavaScript, jQuery, AJAX (dynamic slot loading)[cite: 2]
* **Database:** Microsoft SQL Server[cite: 2]
* **Tools:** Visual Studio 2022, Git & GitHub[cite: 2]

---

## 🗄️ Database Schema

Core entities mapped via EF Core[cite: 2]:
* **`AspNetUsers` / `UserProfiles`:** System credentials and extended player profiles[cite: 2].
* **`FootballFields`:** Pitch master data, active status, and calculated review aggregates[cite: 2].
* **`FieldTypes`:** Pitch specifications (e.g., 5-player, 7-player)[cite: 2].
* **`TimeSlots`:** Defined time intervals, hourly rates, and weekend flags[cite: 2].
* **`Bookings`:** Core reservation entity enforcing time-conflict constraints[cite: 2].
* **`Payments`:** Payment method tracking and transaction completion status[cite: 2].
* **`Reviews`:** User rating scores and comments linked to verified reservations[cite: 2].

---

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
* [Microsoft SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Developer edition)
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (with *ASP.NET and web development* workload) or [VS Code](https://code.visualstudio.com/)

---

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/tranlamvu392005/FootballFieldBooking.git…https://github.com/tranlamvu392005/FootballFieldBooking.git)
   cd FootballFieldBooking
