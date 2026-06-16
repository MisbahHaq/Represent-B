# Represent B

Represent B is an ASP.NET Core MVC e-commerce application built with .NET 8.0. It is an online store frontend and admin system for browsing products, managing carts and orders, handling customer accounts, and supporting customer messages through an in-site chatbot.

## What the Project Does

The application provides a complete shopping workflow:

- **Product Catalog**
  - Browse men's, women's, and vault collections.
  - View product details, images, pricing, stock, tags, sizes, and colors.
  - Search products through autocomplete suggestions.

- **Shopping Cart and Checkout**
  - Add products to cart with size and color options.
  - Store cart items in session for guests and in the database for logged-in users.
  - Checkout with delivery and payment method selection.
  - Required checkout fields for phone number, country/region, city, and address.
  - Editable checkout address that is saved back to the user profile/session.
  - Create orders with order items and show an order confirmation page.

- **Customer Accounts**
  - Login, signup, profile management, username updates, and address updates.
  - Session-based authentication using `UserEmail`, `UserName`, `UserAddress`, and `IsAdmin`.
  - Customer order history from the profile page.

- **Bookmarks**
  - Logged-in users can bookmark products.
  - Bookmarks are stored in the database and linked to the user email.

- **Chatbot and Customer Support**
  - In-site chatbot for order status, cancellation requests, and admin chat.
  - Cancellation requests and admin chat messages are saved as `SupportRequest` records.
  - Cancel Order and Chat With Admin actions require the customer to be logged in.
  - Logged-in users receive admin replies in the chatbot through a reply polling endpoint.
  - Admin can mark support requests as read, reply to requests, resolve them, or cancel related orders.

- **Admin Dashboard**
  - Admin login and dashboard overview.
  - Product management: create, edit, delete, and view products.
  - Order management: view orders, update order status, and inspect order details.
  - Support management: view all chatbot requests, unread requests, cancellation requests, and respond to customers.

## Technology Stack

- **Backend**: ASP.NET Core MVC 8.0
- **Language**: C#
- **Frontend**: Razor Views, HTML, CSS, Bootstrap, JavaScript
- **Database**: SQL Server / LocalDB
- **ORM**: Entity Framework Core
- **Build Tool**: .NET CLI / MSBuild

## Main Project Structure

```text
RepresentWeb/
  Controllers/       MVC controllers for accounts, cart, products, orders, admin, bookmarks, and chatbot
  Data/              EF Core context, initializer, and migrations
  Models/            Domain models and view models
  Views/             Razor views for pages and admin screens
  wwwroot/           Static assets such as CSS, JavaScript, libraries, and images
  Properties/        Launch settings
  appsettings.json   Application configuration and database connection string
```

## Database Models

The application uses these main entities:

- `Product`
- `Order`
- `OrderItem`
- `User`
- `ShoppingCart`
- `Bookmark`
- `SupportRequest`

EF Core `DbSet`s are configured in `RepresentWeb/Data/ApplicationDbContext.cs`.

## Authentication and Authorization

The app uses ASP.NET Core session state for authentication instead of ASP.NET Identity.

Important session values include:

- `UserEmail`
- `UserName`
- `UserAddress`
- `AuthToken`
- `IsAdmin`

Customer-only actions check for `UserEmail`. Admin-only actions check for `IsAdmin == "true"`.

## Chatbot Support Flow

The chatbot supports three main actions:

1. **Order Status**
   - Customer enters an order number.
   - The app returns the current order status.

2. **Cancel Order**
   - Customer must be logged in.
   - Customer enters an order number and cancellation reason.
   - The app creates a `SupportRequest` with type `Cancellation`.
   - Admin can review and cancel the order from the admin panel.

3. **Chat With Admin**
   - Customer must be logged in.
   - Customer sends a message, optionally with an order number.
   - The app creates a `SupportRequest` with type `AdminChat`.
   - Admin can reply from the admin panel.
   - Customer receives admin replies in the chatbot.

## Admin Features

Admin users can access:

- Dashboard statistics
- Product list
- Product create/edit/delete pages
- Order list and order details
- Order status updates
- Support request lists
- Unread support requests
- Cancellation requests
- Reply to customer messages
- Resolve support requests
- Cancel orders from chatbot cancellation requests

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server, SQL Server Express, or LocalDB
- Visual Studio, VS Code, or another C# editor

### Restore Packages

```bash
dotnet restore RepresentWeb/RepresentWeb.csproj
```

### Configure Database

The default connection string uses LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RepresentDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Edit `RepresentWeb/appsettings.json` if your SQL Server instance is different.

### Apply Migrations

```bash
dotnet ef database update --project RepresentWeb/RepresentWeb.csproj
```

If EF tools are not installed globally:

```bash
dotnet tool install --global dotnet-ef
```

### Run the Application

```bash
dotnet run --project RepresentWeb/RepresentWeb.csproj
```

## Default Admin Login

The admin account is handled as a special session-based login case:

```text
Email: admin@represent.com
Password: Qwerty123
```

## Common Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build "Represent B.sln"

# Run the app
dotnet run --project RepresentWeb/RepresentWeb.csproj

# Add a migration
dotnet ef migrations add MigrationName --project RepresentWeb/RepresentWeb.csproj

# Update the database
dotnet ef database update --project RepresentWeb/RepresentWeb.csproj
```

## Notes

- Build output folders such as `bin/` and `obj/` are generated by the .NET SDK.
- Existing nullable reference warnings are from older project code and do not currently block builds.
- Product images and store content are managed through the admin product pages.
