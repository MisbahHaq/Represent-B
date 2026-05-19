# Represent B

Represent B is an ASP.NET Core MVC e-commerce web application built with .NET 8.0. The application provides basic product management functionality for an online store.

## Features

- **Product Management**: Create, read, update, and delete products
- **Product Catalog**: View all available products in a catalog view
- **Product Details**: View detailed information about individual products
- **Data Validation**: Built-in validation for product data using Data Annotations
- **Responsive Design**: Bootstrap-based UI for responsive layouts

## Technology Stack

- **Backend**: ASP.NET Core MVC 8.0
- **Data Access**: Entity Framework Core
- **Database**: SQL Server (via EF Core)
- **Frontend**: HTML, CSS, Bootstrap, JavaScript
- **Build System**: MSBuild

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or SQL Server Express

### Installation

1. Clone the repository
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Update the database connection string in `appsettings.json` if needed
4. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

### Running the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` (or `http://localhost:5000`).

## Project Structure

- `/Controllers` - Contains the ProductsController handling HTTP requests
- `/Models` - Contains the Product data model
- `/Views` - Contains Razor views for the UI
- `/Data` - Contains the ApplicationDbContext for EF Core
- `/Migrations` - Contains EF Core database migrations
- `/wwwroot` - Contains static files (CSS, JavaScript, images)

## API Endpoints

The ProductsController exposes the following RESTful endpoints:

- `GET /Products` - List all products
- `GET /Products/Details/{id}` - Get details for a specific product
- `GET /Products/Create` - Show form to create a new product
- `POST /Products/Create` - Handle creation of a new product
- `GET /Products/Edit/{id}` - Show form to edit an existing product
- `POST /Products/Edit/{id}` - Handle updating an existing product
- `GET /Products/Delete/{id}` - Show confirmation to delete a product
- `POST /Products/Delete/{id}` - Handle deletion of a product

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
