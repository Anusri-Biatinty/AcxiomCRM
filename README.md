# AcxiomCRM

A web-based Customer Relationship Management (CRM) application developed to manage customer information and perform common customer management operations through a simple and structured interface.

## Overview

AcxiomCRM provides a basic CRM system where customer records can be created, viewed, updated, and deleted. The application uses a .NET backend with Entity Framework Core for database operations and SQL Server for storing application data.

The project was developed with a focus on keeping the application simple, organized, and easy to use while following a standard backend and database structure.

## Key Features

- Add new customer records
- View existing customer information
- Update customer details
- Delete customer records
- Store customer data in a relational database
- REST API-based backend
- Entity Framework Core database operations
- Database migrations using Entity Framework Core
- Input validation and basic error handling
- Simple and responsive user interface

## Technologies Used

| Technology | Purpose |
|---|---|
| C# | Application development |
| ASP.NET Core | Backend and Web API |
| Entity Framework Core | Database access and ORM |
| SQL Server | Data storage |
| HTML | Page structure |
| CSS | Styling |
| JavaScript | Client-side functionality |
| Git | Version control |
| GitHub | Source code hosting |

## Application Structure

```text
AcxiomCRM
│
├── Controllers/        # API controllers
├── Models/             # Application models
├── Data/               # Database context and configuration
├── Migrations/         # Entity Framework database migrations
├── Services/           # Application/business logic
├── wwwroot/            # Frontend/static files
│
├── Program.cs          # Application entry point
├── appsettings.json    # Application configuration
└── README.md           # Project documentation
```

## Database

The application uses **SQL Server** for persistent data storage.

**Entity Framework Core** is used to communicate with the database and manage database schema changes through migrations.

The database connection is configured in:

```text
appsettings.json
```

Make sure the connection string is updated according to your local SQL Server configuration before running the application.

## Getting Started

### Prerequisites

Make sure the following are installed:

- .NET SDK
- SQL Server
- Git
- Visual Studio or Visual Studio Code

### 1. Clone the Repository

```bash
git clone <repository-url>
cd AcxiomCRM
```

### 2. Configure the Database

Open `appsettings.json` and configure the SQL Server connection string.

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=AcxiomCRM;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Use the connection details appropriate for your local SQL Server setup.

### 3. Apply Database Migrations

Run:

```bash
dotnet ef database update
```

This creates or updates the required database tables.

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Run the Application

```bash
dotnet run
```

The terminal will display the local URL where the application is running.

## API Operations

The application provides API operations for managing customer records.

| Operation | Purpose |
|---|---|
| GET | Retrieve customer records |
| GET by ID | Retrieve a specific customer |
| POST | Create a new customer |
| PUT | Update an existing customer |
| DELETE | Delete a customer |

The exact API routes can be found in the controller files of the project.

## Validation and Error Handling

The application includes basic validation to ensure that required customer information is provided before it is stored.

Appropriate responses are returned for common situations such as:

- Invalid input
- Customer not found
- Duplicate or invalid data
- Database-related errors

## Development

The project follows a structured approach separating controllers, models, database access, and application logic. This makes the code easier to understand and maintain.

Entity Framework Core migrations are used to keep the database structure synchronized with the application models.

## Future Enhancements

Some possible improvements for future versions include:

- User authentication and authorization
- Role-based access control
- Customer search and filtering
- Customer activity/history tracking
- Dashboard and analytics
- Improved validation
- Pagination for large customer lists
- Cloud deployment

## Author

**Anusri**  
B.Tech – Computer Science Engineering

---

## License

This project was developed for educational and assessment purposes.
