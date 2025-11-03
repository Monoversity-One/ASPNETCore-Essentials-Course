# ASP.NET Core Essentials

A comprehensive learning repository containing 20 focused modules that demonstrate core ASP.NET Core concepts through practical, runnable examples. Each module is a complete, self-contained project with detailed comments and interactive testing capabilities.

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK or later ([Download](https://dotnet.microsoft.com/download))
- **IDE (choose one):**
  - Visual Studio 2022
  - VS Code with C# Dev Kit extension
  - JetBrains Rider

### Running a Module

**Option 1: Visual Studio 2022**
1. Open `ASPNETCore-Essentials.sln`
2. Right-click any project in Solution Explorer
3. Select "Set as Startup Project"
4. Press `F5` or click "Run"

**Option 2: VS Code**
1. Open the repository folder
2. Install C# Dev Kit extension
3. Open any module folder (e.g., `src/01-MinimalApiBasics`)
4. Press `F5` or use the Run menu

**Option 3: Command Line**
```bash
# Navigate to any module
cd src/01-MinimalApiBasics

# Run the project
dotnet run

# The app will start and display URLs:
# https://localhost:5001
# http://localhost:5000
```

### Testing APIs
Most modules include Swagger UI for interactive testing:
1. Run the module
2. Navigate to `https://localhost:{port}/swagger`
3. Expand endpoints and click "Try it out"
4. Fill in parameters and click "Execute"

## 📚 Modules Overview

Each module is a standalone project demonstrating specific ASP.NET Core concepts. Click the links to explore the code.

### Fundamentals (1-4)

#### [01. Minimal API Basics](./src/01-MinimalApiBasics)
Hello world, route parameters, query strings, request binding, response types, CRUD operations, async endpoints, HTTP methods
- 15+ endpoint examples
- Complete CRUD operations
- Data validation
- Swagger documentation

#### [02. Middleware and Routing](./src/02-MiddlewareAndRouting)
Custom middleware, request pipeline, endpoint routing, route constraints, middleware ordering
- Custom middleware components
- Pipeline visualization
- Route constraint examples

#### [03. Configuration and Logging](./src/03-ConfigAndLogging)
appsettings.json, environment variables, configuration providers, ILogger, structured logging
- Multiple configuration sources
- Structured logging patterns
- Log levels and filtering

#### [04. Dependency Injection & Options](./src/04-DIAndOptions)
Service lifetimes (Singleton, Scoped, Transient), options pattern, typed clients, service registration
- Service lifetime demonstrations
- Options pattern implementation
- Configuration binding

### Web UI (5-6)

#### [05. MVC Controllers & Views](./src/05-MvcControllersViews)
Controllers, views, layouts, partial views, tag helpers, view components, model binding
- Complete MVC pattern
- Razor view engine
- Tag helpers and view components

#### [06. Razor Pages](./src/06-RazorPages)
Page models, page handlers, TempData, validation, forms, routing
- Page-based architecture
- Form handling
- Model validation

### Web APIs (7-10)

#### [07. Web API Fundamentals](./src/07-WebApiFundamentals)
RESTful APIs, DTOs, model validation, API versioning, Swagger/OpenAPI documentation
- RESTful design patterns
- DTO mapping
- API versioning strategies

#### [08. Entity Framework Core](./src/08-EfCoreSqlite)
DbContext, relationships (one-to-many), LINQ queries, filtering, pagination, aggregations, seeding data, SQLite database
- Complete relational model
- Advanced queries
- Eager loading
- Data seeding

#### [09. Authentication](./src/09-AuthCookieJwt)
Cookie authentication, JWT bearer tokens, identity basics, authorization policies, claims
- Cookie-based auth
- JWT token generation
- Authorization policies

#### [10. Filters, Model Binding & Validation](./src/10-FiltersModelBindingValidation)
Action filters, exception filters, result filters, custom model binders, validation attributes
- Custom filters
- Model binding customization
- Validation patterns

### Performance & Reliability (11-13)

#### [11. Caching & Compression](./src/11-CachingAndCompression)
In-memory caching, response caching, distributed cache patterns, response compression (Gzip, Brotli)
- Memory cache
- Response caching
- Compression middleware

#### [12. Background Services](./src/12-BackgroundServices)
IHostedService, BackgroundService, timed background tasks, queued work items, graceful shutdown
- Hosted services
- Background task patterns
- Graceful shutdown handling

#### [13. Health Checks & Observability](./src/13-HealthChecksObservability)
Health check endpoints, custom health checks, database health checks, OpenTelemetry integration, distributed tracing, metrics
- Multiple health endpoints
- Custom health checks
- OpenTelemetry integration
- Distributed tracing

### Real-time & RPC (14-15)

#### [14. SignalR Real-time Communication](./src/14-SignalRRealtime)
WebSocket connections, hubs, real-time messaging, broadcasting, groups, streaming data, JavaScript client
- Chat hub with private messages
- Notification broadcasting
- Stock ticker streaming
- Interactive HTML client

#### [15. gRPC Services](./src/15-GrpcServices)
Protocol Buffers, unary RPC, server streaming, client streaming, bidirectional streaming, gRPC reflection, CRUD operations
- All 4 RPC types
- Protocol Buffers definitions
- gRPC reflection
- Complete CRUD service

### Security & Resilience (16-19)

#### [16. Rate Limiting](./src/16-RateLimiting)
Fixed window, sliding window, token bucket, concurrency limiters, per-IP limiting, custom rate limit policies
- 6 rate limiting strategies
- Custom rejection handlers
- Interactive testing interface

#### [17. CORS & Security Headers](./src/17-CorsAndSecurity)
Cross-Origin Resource Sharing policies, security headers (CSP, HSTS, X-Frame-Options), origin validation
- Multiple CORS policies
- Comprehensive security headers
- Clickjacking protection
- XSS protection

#### [18. File Upload & Download](./src/18-FileUploadDownload)
Single/multiple file uploads, chunked uploads for large files, file streaming, progress tracking, content-type detection
- Single/multiple uploads
- Chunked upload for large files
- Streaming downloads
- Progress tracking

#### [19. Error Handling & Problem Details](./src/19-ErrorHandling)
Global exception handling, RFC 7807 Problem Details, custom exceptions, validation errors, status code pages
- RFC 7807 implementation
- Global exception handler
- Custom exception types
- Standardized error responses

### Testing (20)

#### [20. Testing](./src/20-Testing)
Unit tests with xUnit, integration tests with WebApplicationFactory, mocking with NSubstitute, FluentAssertions, test patterns (AAA), repository tests, service tests, API tests
- 23 unit tests
- 13 integration tests
- Mocking with NSubstitute
- FluentAssertions
- AAA pattern


## 🎯 Learning Path

### Beginner Track (Weeks 1-2)
Start with modules 1-6 to understand ASP.NET Core fundamentals and web UI development.

**Week 1: Core Fundamentals**
- **Day 1-2:** [Module 01](./src/01-MinimalApiBasics) - Minimal APIs
  - Understand endpoints, routing, request/response
  - Practice: Create your own API endpoints
- **Day 3-4:** [Module 02](./src/02-MiddlewareAndRouting) - Middleware
  - Learn request pipeline
  - Practice: Create custom middleware
- **Day 5:** [Module 03](./src/03-ConfigAndLogging) - Configuration
  - Configuration sources
  - Practice: Add custom configuration
- **Day 6-7:** [Module 04](./src/04-DIAndOptions) - Dependency Injection
  - Service lifetimes
  - Practice: Create and inject your own services

**Week 2: Web Development**
- **Day 1-3:** [Module 05](./src/05-MvcControllersViews) - MVC
  - Controllers, views, models
  - Practice: Build a simple CRUD UI
- **Day 4-5:** [Module 06](./src/06-RazorPages) - Razor Pages
  - Page-based architecture
  - Practice: Create form-based pages
- **Day 6-7:** [Module 07](./src/07-WebApiFundamentals) - Web API
  - RESTful API design
  - Practice: Build a complete API

### Intermediate Track (Weeks 3-4)
Progress through modules 7-12 to learn API development, data access, authentication, and performance optimization.

**Week 3: Data & Security**
- **Day 1-3:** [Module 08](./src/08-EfCoreSqlite) - Entity Framework
  - Database operations, relationships
  - Practice: Design your own data model
- **Day 4-5:** [Module 09](./src/09-AuthCookieJwt) - Authentication
  - Cookie and JWT auth
  - Practice: Secure your API
- **Day 6-7:** [Module 10](./src/10-FiltersModelBindingValidation) - Filters & Validation
  - Request/response pipeline customization
  - Practice: Create custom filters

**Week 4: Performance & Advanced**
- **Day 1-2:** [Module 11](./src/11-CachingAndCompression) - Caching
  - Memory and response caching
  - Practice: Add caching to your API
- **Day 3:** [Module 12](./src/12-BackgroundServices) - Background Services
  - Long-running tasks
  - Practice: Create a scheduled task
- **Day 4:** [Module 13](./src/13-HealthChecksObservability) - Health Checks
  - Monitoring and observability
  - Practice: Add health checks to your app
- **Day 5:** [Module 14](./src/14-SignalRRealtime) - SignalR
  - Real-time communication
  - Practice: Build a simple chat
- **Day 6:** [Module 15](./src/15-GrpcServices) - gRPC
  - High-performance RPC
  - Practice: Create a gRPC service
- **Day 7:** Review and build a project combining multiple concepts

### Advanced Track (Week 5)
Explore modules 13-20 for real-time communication, gRPC, security, error handling, and testing strategies.

**Week 5: Security & Production**
- **Day 1:** [Module 16](./src/16-RateLimiting) - Rate Limiting
  - API throttling
  - Practice: Add rate limits to your API
- **Day 2:** [Module 17](./src/17-CorsAndSecurity) - CORS & Security
  - Cross-origin security
  - Practice: Secure your frontend-backend communication
- **Day 3:** [Module 18](./src/18-FileUploadDownload) - File Upload
  - File handling
  - Practice: Add file upload to your app
- **Day 4:** [Module 19](./src/19-ErrorHandling) - Error Handling
  - Proper error responses
  - Practice: Implement global error handling
- **Day 5-7:** [Module 20](./src/20-Testing) - Testing
  - Unit and integration testing
  - Practice: Write tests for your code

## 🧪 Running Tests

### All Tests
```bash
# From repository root
dotnet test

# With detailed output
dotnet test --verbosity normal
```

### Module 20 Tests (Unified Project)
```bash
# Navigate to the testing module
cd src/20-Testing

# Run all tests (unit + integration)
dotnet test

# Watch mode (auto-run on changes)
dotnet watch test
```

**Test Summary:**
- **Unit Tests:** 23 tests (ProductService, ProductRepository)
- **Integration Tests:** 13 tests (Full API testing)
- **Total:** 36 comprehensive tests

## 📋 Module Ports Reference

Each module runs on a unique port to avoid conflicts when running multiple modules:

| Module | HTTPS Port | HTTP Port | Swagger URL |
|--------|-----------|-----------|-------------|
| [01 - Minimal API](./src/01-MinimalApiBasics) | 51010 | 51012 | https://localhost:51010/swagger |
| [02 - Middleware](./src/02-MiddlewareAndRouting) | 51020 | 51022 | https://localhost:51020/swagger |
| [03 - Configuration](./src/03-ConfigAndLogging) | 51030 | 51032 | https://localhost:51030/swagger |
| [04 - DI & Options](./src/04-DIAndOptions) | 51040 | 51042 | https://localhost:51040/swagger |
| [05 - MVC](./src/05-MvcControllersViews) | 51050 | 51052 | https://localhost:51050 |
| [06 - Razor Pages](./src/06-RazorPages) | 51060 | 51062 | https://localhost:51060 |
| [07 - Web API](./src/07-WebApiFundamentals) | 51070 | 51072 | https://localhost:51070/swagger |
| [08 - EF Core](./src/08-EfCoreSqlite) | 51080 | 51082 | https://localhost:51080/swagger |
| [09 - Authentication](./src/09-AuthCookieJwt) | 51090 | 51092 | https://localhost:51090/swagger |
| [10 - Filters](./src/10-FiltersModelBindingValidation) | 51100 | 51102 | https://localhost:51100/swagger |
| [11 - Caching](./src/11-CachingAndCompression) | 51110 | 51112 | https://localhost:51110/swagger |
| [12 - Background](./src/12-BackgroundServices) | 51120 | 51122 | https://localhost:51120/swagger |
| [13 - Health Checks](./src/13-HealthChecksObservability) | 51130 | 51132 | https://localhost:51130/swagger |
| [14 - SignalR](./src/14-SignalRRealtime) | 51140 | 51142 | https://localhost:51140 |
| [15 - gRPC](./src/15-GrpcServices) | 51150 | 51152 | N/A (use grpcurl) |
| [16 - Rate Limiting](./src/16-RateLimiting) | 51160 | 51162 | https://localhost:51160/swagger |
| [17 - CORS](./src/17-CorsAndSecurity) | 51170 | 51172 | https://localhost:51170/swagger |
| [18 - File Upload](./src/18-FileUploadDownload) | 51180 | 51182 | https://localhost:51180 |
| [19 - Error Handling](./src/19-ErrorHandling) | 51190 | 51192 | https://localhost:51190/swagger |
| [20 - Testing](./src/20-Testing) | 51200 | 51202 | https://localhost:51200/swagger |

## 💡 Special Module Instructions

### SignalR (Module 14)
1. Run the module: `cd src/14-SignalRRealtime && dotnet run`
2. Open `https://localhost:51140` in browser
3. Use the interactive HTML client
4. Open multiple browser tabs to test real-time features

### gRPC (Module 15)
Install grpcurl from https://github.com/fullstorydev/grpcurl

```bash
# Run the module
cd src/15-GrpcServices
dotnet run

# In another terminal, test with grpcurl:
# List services
grpcurl -plaintext localhost:51152 list

# Call unary RPC
grpcurl -plaintext -d '{"name": "World"}' localhost:51152 greeter.Greeter/SayHello

# List products
grpcurl -plaintext localhost:51152 products.ProductService/GetAllProducts
```

### Entity Framework (Module 08)
The SQLite database is created at `src/08-EfCoreSqlite/store.db`

```bash
# View database with VS Code SQLite extension
# Or use DB Browser for SQLite

# To reset database:
cd src/08-EfCoreSqlite
rm store.db
dotnet run  # Database will be recreated with seed data
```

### File Upload (Module 18)
1. Run the module: `cd src/18-FileUploadDownload && dotnet run`
2. Open `https://localhost:51180` in browser
3. Use the interactive HTML interface
4. Uploaded files are stored in `src/18-FileUploadDownload/uploads/`


## 🛠️ Technologies & Packages

### Core Technologies
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core 8.0** - ORM for data access
- **SQLite** - Embedded database (no external database required)

### Real-time & Communication
- **SignalR** - WebSocket-based real-time communication
- **gRPC** - High-performance RPC framework
- **Protocol Buffers** - Efficient serialization

### Observability & Monitoring
- **OpenTelemetry** - Distributed tracing and metrics
- **Health Checks** - Application health monitoring
- **Structured Logging** - ILogger with structured data

### Testing
- **xUnit** - Test framework
- **NSubstitute** - Mocking library
- **FluentAssertions** - Fluent assertion library
- **WebApplicationFactory** - Integration testing

### Security & Performance
- **Rate Limiting** - Built-in ASP.NET Core rate limiting
- **CORS** - Cross-origin resource sharing
- **Response Compression** - Gzip and Brotli compression
- **Response Caching** - HTTP caching

## 🔧 Troubleshooting

### Port Already in Use
If you get a port conflict error:
1. Edit `Properties/launchSettings.json` in the module
2. Change the port numbers
3. Save and run again

### Certificate Trust Issues (HTTPS)
```bash
# Trust the development certificate
dotnet dev-certs https --trust
```

### NuGet Package Restore Issues
```bash
# From repository root
dotnet restore

# Or for specific project
cd src/01-MinimalApiBasics
dotnet restore
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Database Issues (Module 08)
```bash
# Delete the database and restart
cd src/08-EfCoreSqlite
rm store.db
dotnet run
# Database will be recreated with seed data
```

## 📖 Key Concepts Demonstrated

✅ **Minimal APIs** and traditional MVC patterns <br/>
✅ **Dependency injection** and service lifetimes <br/>
✅ **Configuration** and logging best practices <br/>
✅ **Entity Framework Core** with relationships <br/>
✅ **Authentication** and authorization <br/>
✅ **Real-time communication** with SignalR <br/>
✅ **High-performance RPC** with gRPC <br/>
✅ **Rate limiting** and security headers <br/>
✅ **Error handling** and problem details <br/>
✅ **Comprehensive testing** strategies <br/>
✅ **Background services** and hosted services <br/>
✅ **Health checks** and observability <br/>
✅ **File upload/download** with streaming <br/>
✅ **CORS** and web security <br/>
✅ **Caching** and compression <br/>

## 🎓 Tips for Success

1. **Read the code comments** - Each module has detailed explanations
2. **Experiment** - Modify the code and see what happens
3. **Use the debugger** - Set breakpoints and step through code
4. **Check the logs** - Console output shows what's happening
5. **Test everything** - Use Swagger, browser, or Postman
6. **Build projects** - Combine concepts from multiple modules
7. **Write tests** - Practice TDD with Module 20 examples

## 📚 Additional Resources

### Official Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)
- [gRPC Documentation](https://docs.microsoft.com/aspnet/core/grpc)

### Recommended Tools
- **[Postman](https://www.postman.com/)** - API testing
- **[grpcurl](https://github.com/fullstorydev/grpcurl)** - gRPC testing
- **[DB Browser for SQLite](https://sqlitebrowser.org/)** - Database viewer

### VS Code Extensions
- **C# Dev Kit** - C# development support
- **REST Client** - Test HTTP requests
- **SQLite Viewer** - View SQLite databases
- **Thunder Client** - API testing

## 📊 Project Statistics

- **Total Modules:** 20
- **Total Test Projects:** 1 (unified in Module 20)
- **Total Tests:** 36 (23 unit + 13 integration)
- **Lines of Code:** ~3,500+ across all modules
- **.NET Version:** 8.0
- **Cross-platform:** Windows, macOS, Linux

## 🤝 Contributing

This is a learning repository. Feel free to:
- Fork and experiment
- Suggest improvements via issues
- Share your learning projects built with these concepts

## 📝 License

MIT License - feel free to use this repository for learning and teaching purposes.

## 🚀 What's Next?

After completing these modules, you'll be ready to:
1. **Build production applications** with ASP.NET Core
2. **Design RESTful APIs** following best practices
3. **Implement real-time features** with SignalR
4. **Create high-performance services** with gRPC
5. **Secure your applications** with proper authentication and authorization
6. **Monitor and observe** your applications in production
7. **Write comprehensive tests** for your code
8. **Handle errors gracefully** with standardized responses

---

**Happy Learning!** 🎉
