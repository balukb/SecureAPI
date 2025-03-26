# MF Secure API 🚀

MF Secure API is a **.NET 8 Web API** designed for secure authentication and authorization using **JWT tokens and API key validation**. This API is specifically built for access by an authorized and **pre-registered domains**.

## 🔐 Key Features
- **Authentication & Authorization**
  - Implements **JWT-based authentication** with **access tokens**.
  - API key authentication ensures only pre-approved domains can access the API.

- **Security Measures**
  - Blocks unauthorized access using API keys and app verification.
  - Tokens expire after a set time and can be refreshed securely.
  - Middleware for **global error handling and request logging**.

- **Logging & Monitoring**
  - **Request tracking middleware** to log all API requests into the database.
  - **Error handling middleware** to capture and store errors.

- **Domain-Based Access Control**
  - Only **whitelisted domains** can access the API.
  - Unauthorized sources are blocked.

## 🛠️ Tech Stack
- **Backend:** ASP.NET Core 8, Entity Framework Core
- **Database:** SQL Server
- **Security:** JWT, API Key Authentication
- **Logging:** Middleware-based request and error logging

## 🚀 Deployment & Usage
- Can be deployed on **IIS (Internet Information Services)**.
- Supports **EF Core migrations** for database schema updates.
- API endpoints require valid authentication.

## 📂 Folder Structure
MF_SecureApi/
│── Controllers/          # API Controllers (Auth, Secure, etc.)
│── Middleware/           # Custom Middleware (Error handling, Request logging)
│── Models/               # Data models (User, RefreshToken, RequestLog, etc.)
│── Data/                 # Database Context and Migrations
│── Services/             # Business Logic (TokenService, API Key Validation, etc.)
│── appsettings.json      # Application Configuration (JWT, Database, etc.)
│── Program.cs            # API Startup Configuration
│── Startup.cs            # Service Configuration and Middleware Setup