# LibraryMS — Library Management System

A full-stack library automation system built with **ASP.NET Core Web API** and **Vue 3**, developed as part of an internship project (İndas). It supports role-based access for admins and members, book/author/category management, loan tracking with automatic overdue fine calculation, and analytical reports.

## Features

- **Authentication & Authorization** — JWT-based login/register, role-based access control (Admin / Member), with live client-side email format validation
- **Book, Author, Category Management** — full CRUD, with search, filtering, sorting, and pagination
- **Copy Tracking** — each book has a total and available copy count; borrowing/returning automatically adjusts availability, and a book only shows as "Borrowed" once all copies are checked out
- **Loan System** — members can borrow/return books themselves, and admins can issue a loan directly to any member from the admin panel; active loan tracking and overdue detection with separate search filters by book and by member
- **Fine Calculation** — automatic late fee calculation based on business days (weekends and official holidays excluded), with an admin action to mark individual fines as paid
- **Reports** — borrowed books, overdue books, and fine reports for admins
- **Admin Panel** — manage books, authors (with biography), categories (with description), and view loan/fine reports
- **Book Covers & Descriptions** — books can have a cover image and a summary shown on their detail page
- **Barcode Scanning** — admins can scan a book's ISBN barcode (or enter it manually) to auto-fill title, author, category, description and cover image via the Google Books API, with duplicate ISBN detection before adding
- **Rate Limiting** — login endpoint is protected against brute-force attempts
- **API Documentation** — fully documented and testable via Swagger UI, including JWT bearer auth
- **Logging** — structured logging via Serilog, written to daily rolling log files
- **Animations** — page transitions and staggered list animations on the frontend via Motion for Vue
- **Unit Tests** — a dedicated test project covering core service and repository logic

## Tech Stack

**Backend**
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core + PostgreSQL (Npgsql)
- JWT Bearer Authentication
- FluentValidation
- AutoMapper
- Serilog (console + file sinks)
- Swagger / OpenAPI

**Frontend**
- Vue 3 (Composition API, `<script setup>`)
- Vue Router
- Axios
- motion-v (Motion for Vue) for UI animations
- html5-qrcode for ISBN barcode scanning
- Google Books API for barcode-based book lookup

## Project Structure

```
LibraryManagementSystem/
├── LibraryManagementSystem.API/    # ASP.NET Core backend
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   ├── Dtos/
│   ├── Validators/
│   ├── Mapping/
│   └── Migrations/
├── library-frontend/                # Vue 3 frontend
│   └── src/
│       ├── views/
│       ├── components/
│       └── router/
├── LibraryManagementSystem.Tests/   # Unit tests for services and repositories
└── scripts/                         # One-off data seeding utilities (Node.js)
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- PostgreSQL running locally

### 1. Backend Setup

```bash
cd LibraryManagementSystem.API
```

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=library_management_db;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
}
```

Apply migrations and run the API:

```bash
dotnet ef database update
dotnet run
```

The API will start on `http://localhost:5239`. Swagger UI is available at `http://localhost:5239/swagger`.

On first run, a default admin account is seeded automatically:

| Field    | Value              |
|----------|--------------------|
| Email    | admin@library.com  |
| Password | Admin123!          |

### 2. Frontend Setup

```bash
cd library-frontend
npm install
npm run dev
```

The app will be available at `http://localhost:5173`.

### 3. (Optional) Seed Sample Data

The `scripts/` folder contains Node.js utilities to bulk-populate the library with well-known books, authors, categories, and cover images for demo/testing purposes.

```bash
node scripts/seed-books.mjs books-seed-data.json
node scripts/backfill-covers.mjs
```

## API Overview

| Area       | Endpoint                          | Access        |
|------------|------------------------------------|---------------|
| Auth       | `POST /api/auth/register`          | Public        |
| Auth       | `POST /api/auth/login`             | Public (rate-limited) |
| Books      | `GET /api/books`                   | Public        |
| Books      | `GET /api/books/search`            | Public        |
| Books      | `POST /api/books`                  | Admin         |
| Books      | `PUT /api/books/{id}`              | Admin         |
| Books      | `DELETE /api/books/{id}`           | Admin         |
| Authors    | `GET /api/authors`                 | Public        |
| Authors    | `POST /api/authors`                | Admin         |
| Authors    | `PUT /api/authors/{id}`            | Admin         |
| Authors    | `DELETE /api/authors/{id}`         | Admin         |
| Categories | `GET /api/categories`              | Public        |
| Categories | `POST /api/categories`             | Admin         |
| Categories | `PUT /api/categories/{id}`         | Admin         |
| Categories | `DELETE /api/categories/{id}`      | Admin         |
| Loans      | `GET /api/loans/my`                | Member        |
| Loans      | `GET /api/loans`                   | Admin         |
| Loans      | `POST /api/loans/borrow`           | Member, Admin (can issue to any member) |
| Loans      | `POST /api/loans/return`           | Member, Admin |
| Fines      | `GET /api/fines/my`                | Member        |
| Fines      | `GET /api/fines`                   | Admin         |
| Fines      | `PUT /api/fines/{id}/pay`          | Member, Admin |
| Reports    | `GET /api/reports/*`               | Admin         |

Full request/response schemas are available in Swagger UI.

**Note on barcode scanning:** the scan feature does not require a dedicated backend endpoint. The frontend calls the public Google Books API directly (`GET https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}`) to look up a scanned ISBN, then uses the existing `POST /api/books`, `POST /api/authors` and `POST /api/categories` endpoints to save the result.

## Security Notes

- Passwords are hashed using ASP.NET Core's `PasswordHasher`.
- All write operations (create/update/delete) require a valid JWT with the `Admin` role.
- Members can only access and modify their own loan/fine records.
- The login endpoint is limited to 5 requests per minute per server instance to mitigate brute-force attempts.

## Author

Developed by Gözde Yılıkyılmaz as part of an internship project.