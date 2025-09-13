# Save The Bee Bulgaria - Honey Web Platform

A comprehensive e-commerce web platform for Bulgarian honey products, built with ASP.NET Core 6.0. The platform connects beekeepers with customers, offering a marketplace for honey, propolis, bee pollen, and related products.

## 🏗️ Project Architecture

This project follows a **Clean Architecture** pattern with clear separation of concerns:

```
SaveTheBeeBulgaria/
├── HoneyWebPlatform.Web/                    # Presentation Layer (MVC)
├── HoneyWebPlatform.Data/                   # Data Access Layer
├── HoneyWebPlatform.Data.Models/            # Domain Models
├── HoneyWebPlatform.Services.Data/          # Business Logic Layer
├── HoneyWebPlatform.Services.Data.Models/   # Service Models
├── HoneyWebPlatform.Services.Mapping/       # AutoMapper Configuration
├── HoneyWebPlatform.Web.Infrastructure/     # Infrastructure Services
├── HoneyWebPlatform.Web.ViewModels/         # View Models
├── HoneyWebPlatform.Common/                 # Shared Constants
└── HoneyWebPlatform.Services.Tests/         # Unit Tests
```

## 🛠️ Technologies Used

### Backend Technologies
- **.NET 6.0** - Core framework
- **ASP.NET Core MVC** - Web application framework
- **Entity Framework Core 6.0** - ORM for data access
- **SQL Server** - Primary database
- **SQLite** - Development database
- **AutoMapper** - Object-to-object mapping
- **SignalR** - Real-time communication

### Frontend Technologies
- **Razor Views** - Server-side rendering
- **Bootstrap** - CSS framework
- **jQuery** - JavaScript library
- **HTML5/CSS3** - Markup and styling
- **JavaScript** - Client-side functionality

### Authentication & Security
- **ASP.NET Core Identity** - User management
- **Google OAuth** - Social authentication
- **Facebook OAuth** - Social authentication
- **reCAPTCHA** - Bot protection
- **Anti-forgery tokens** - CSRF protection

### Additional Features
- **Email Services** - SMTP integration
- **File Upload** - Image handling
- **Caching** - Memory caching
- **Real-time Chat** - SignalR hubs
- **Shopping Cart** - Session management
- **Order Management** - E-commerce functionality

## 📊 Core Entities

### User Management
- **ApplicationUser** - Extended Identity user with profile information
- **Beekeeper** - Specialized user role for product sellers
- **Admin** - Administrative role

### Product Catalog
- **Honey** - Main product entity with categories, pricing, and beekeeper info
- **Propolis** - Secondary product type
- **BeePollen** - Additional product offering
- **Category** - Product categorization
- **Flavour** - Product flavor profiles

### E-commerce
- **Cart** - Shopping cart functionality
- **CartItem** - Individual cart items
- **Order** - Customer orders with status tracking
- **OrderItem** - Individual order line items

### Content Management
- **Post** - Blog posts and articles
- **Comment** - User comments on posts
- **SubscribedEmail** - Newsletter subscriptions

## 🎯 Key Features

### For Customers
- **Product Browsing** - Browse honey, propolis, and bee pollen
- **Advanced Filtering** - Filter by category, price, origin
- **Shopping Cart** - Add/remove items with real-time updates
- **User Registration** - Account creation and management
- **Order Tracking** - Track order status
- **Blog Reading** - Educational content about beekeeping

### For Beekeepers
- **Product Management** - Add, edit, and manage products
- **Profile Management** - Manage beekeeper profile and farm images
- **Order Management** - View and process customer orders
- **Analytics** - View sales statistics

### For Administrators
- **User Management** - Manage all users and beekeepers
- **Content Management** - Manage blog posts and comments
- **Order Oversight** - Monitor all orders across the platform
- **Statistics** - Platform-wide analytics

## 🚀 Getting Started

### Prerequisites
- .NET 6.0 SDK
- SQL Server (or SQLite for development)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SaveTheBeeBulgaria
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection strings**
   - Edit `appsettings.json` or `appsettings.Development.json`
   - Configure your database connection string

4. **Run database migrations**
   ```bash
   dotnet ef database update --project HoneyWebPlatform.Data --startup-project HoneyWebPlatform.Web
   ```

5. **Run the application**
   ```bash
   dotnet run --project HoneyWebPlatform.Web
   ```

### Configuration

#### Database Setup
- **Development**: Uses SQLite (LocalDB)
- **Production**: Uses SQL Server with Azure connection

#### Email Configuration
Configure SMTP settings in `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  }
}
```

#### OAuth Configuration
Set up Google and Facebook OAuth in `Program.cs`:
```csharp
.AddGoogle(options => {
    options.ClientId = "YOUR_GOOGLE_CLIENT_ID";
    options.ClientSecret = "YOUR_GOOGLE_CLIENT_SECRET";
})
.AddFacebook(options => {
    options.AppId = "YOUR_FACEBOOK_APP_ID";
    options.AppSecret = "YOUR_FACEBOOK_APP_SECRET";
})
```

## 🏢 Business Logic

### Service Layer Architecture
The application uses a service-oriented architecture with interfaces for:
- **IHoneyService** - Honey product management
- **IPropolisService** - Propolis product management
- **IBeekeeperService** - Beekeeper management
- **ICategoryService** - Category management
- **IOrderService** - Order processing
- **ICartService** - Shopping cart operations
- **IPostService** - Blog post management
- **IEmailSender** - Email notifications

### Real-time Features
- **ChatHub** - Real-time customer support chat
- **CartHub** - Real-time cart updates
- **OrderHub** - Real-time order notifications for admins

## 🧪 Testing

The project includes unit tests in the `HoneyWebPlatform.Services.Tests` project:
- Service layer testing
- Database seeder for test data
- XUnit test framework

Run tests with:
```bash
dotnet test
```

## 🚀 Deployment

### Azure App Service Deployment
The project is configured for deployment to Azure App Service using GitHub Actions:

1. **Workflow Configuration**
   - Located in `.github/workflows/master_savethebeebulgaria.yml`
   - Builds and deploys on push to master branch
   - Uses Azure App Service deployment

2. **Environment Setup**
   - Configure Azure App Service
   - Set up connection strings
   - Configure OAuth providers
   - Set up email settings

### Docker Support
The project includes Docker configuration:
- `Dockerfile` for containerization
- `docker-compose.yml` for local development
- Multi-stage build for optimized production images

## 📁 Project Structure Details

### Data Layer (`HoneyWebPlatform.Data`)
- **DbContext** - Entity Framework context
- **Configurations** - Entity configurations
- **Migrations** - Database migration files

### Services Layer (`HoneyWebPlatform.Services.Data`)
- **Interfaces** - Service contracts
- **Implementations** - Business logic implementation
- **Models** - Service-specific models

### Web Layer (`HoneyWebPlatform.Web`)
- **Controllers** - MVC controllers
- **Views** - Razor view templates
- **Areas** - Admin and Identity areas
- **Hubs** - SignalR hubs for real-time features
- **wwwroot** - Static files (CSS, JS, images)

### Infrastructure (`HoneyWebPlatform.Web.Infrastructure`)
- **Extensions** - Custom extension methods
- **Middlewares** - Custom middleware components
- **ModelBinders** - Custom model binding

## 🔧 Development Guidelines

### Code Organization
- Follow Clean Architecture principles
- Use dependency injection throughout
- Implement proper error handling
- Use async/await for I/O operations

### Database Design
- Use Entity Framework Code First approach
- Implement proper relationships between entities
- Use data annotations for validation
- Follow naming conventions

### Security Best Practices
- Use HTTPS in production
- Implement proper authentication and authorization
- Validate all user inputs
- Use anti-forgery tokens
- Implement rate limiting

## 📞 Support

For support and questions:
- Email: savethebee2024@gmail.com
- GitHub Issues: Create an issue in the repository

## 📄 License

This project is proprietary software developed for Save The Bee Bulgaria initiative.

---

**Built with ❤️ for Bulgarian beekeepers and honey lovers**


