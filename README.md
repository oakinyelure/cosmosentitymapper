# Cosmos Entity Mapper

A simple and intuitive entity wrapper for Microsoft's Azure Cosmos DB that provides Entity Framework-like experience with a unit of work and repository pattern.

[![License](https://img.shields.io/badge/License-GPL%203.0-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Cosmos.Entity.Mapper.svg)](https://www.nuget.org/packages/Cosmos.Entity.Mapper/)

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [Entities](#entities)
  - [Attributes](#attributes)
  - [Context](#context)
- [Configuration](#configuration)
- [CRUD Operations](#crud-operations)
- [Querying Data](#querying-data)
- [Dependency Injection](#dependency-injection)
- [Advanced Features](#advanced-features)
- [Best Practices](#best-practices)
- [License](#license)

## Introduction

Cosmos Entity Mapper is a wrapper on top of Azure Cosmos DB that works similar to Entity Framework but with different extensions. It simplifies working with Cosmos DB by providing:

- **Entity Framework-like experience**: Familiar API for .NET developers
- **Unit of Work pattern**: Manage database operations efficiently
- **Repository pattern**: Access collections through strongly-typed repositories
- **Attribute-based configuration**: No complex configuration files needed
- **LINQ support**: Write queries using LINQ expressions
- **Async/await support**: Full asynchronous operation support

## Features

- 🚀 **Easy to use**: Simple setup with minimal configuration
- 📦 **No migrations required**: Works directly with your Cosmos DB
- 🎯 **Type-safe**: Strongly-typed access to your collections
- ⚡ **Bulk operations**: Efficient batch operations for adding, updating, and deleting documents
- 🔍 **Rich querying**: LINQ-based querying with materialization extensions
- 🔌 **Dependency injection ready**: Built-in support for ASP.NET Core DI
- 📊 **Pagination support**: Offset-based pagination out of the box

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Cosmos.Entity.Mapper
```

Or via Package Manager Console:

```powershell
Install-Package Cosmos.Entity.Mapper
```

## Quick Start

### 1. Define Your Entity

Create a model class and decorate it with the required attributes:

```csharp
using Cosmos.Entity.Mapper.Schema;

[Collection("Users")]
public class User
{
    [DocumentKey]
    public string Id { get; set; }
    
    [DocumentPartition]
    public string UserId { get; set; }
    
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 2. Create Your Context

Create a context class that inherits from `CosmosDbContext`:

```csharp
using Cosmos.Entity.Mapper;
using Cosmos.Entity.Mapper.Schema;

[Database("MyDatabase")]
public class MyDbContext : CosmosDbContext
{
    public MyDbContext(string connectionString) : base(connectionString)
    {
    }
    
    public CollectionSet<User> Users { get; set; }
}
```

### 3. Use Your Context

```csharp
var connectionString = "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=your-key;Database=MyDatabase";

using (var context = new MyDbContext(connectionString))
{
    // Add a new user
    var user = new User
    {
        Id = Guid.NewGuid().ToString(),
        UserId = "user123",
        Name = "John Doe",
        Email = "john@example.com",
        CreatedAt = DateTime.UtcNow
    };
    
    await context.Users.AddAsync(user);
    
    // Query users
    var users = await context.Users
        .Where(u => u.Name.Contains("John"))
        .ToDocumentListAsync();
}
```

## Core Concepts

### Entities

Entities are plain C# classes (POCOs) that represent documents in your Cosmos DB collections. They must be decorated with at least the following attributes:

- `[DocumentKey]`: Marks the unique identifier property
- `[DocumentPartition]`: Marks the partition key property

```csharp
public class Product
{
    [DocumentKey]
    public string Id { get; set; }
    
    [DocumentPartition]
    public string CategoryId { get; set; }
    
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### Attributes

#### DatabaseAttribute

Defines the database name for your context:

```csharp
[Database("ProductionDatabase")]
public class MyDbContext : CosmosDbContext
{
    // ...
}
```

#### CollectionAttribute

Maps an entity to a specific Cosmos DB container:

```csharp
[Collection("ProductsContainer")]
public class Product
{
    // ...
}
```

#### DocumentKeyAttribute

Marks the property that serves as the unique identifier (id) for the document:

```csharp
[DocumentKey]
public string Id { get; set; }
```

#### DocumentPartitionAttribute

Marks the property that serves as the partition key:

```csharp
[DocumentPartition]
public string PartitionKey { get; set; }
```

### Context

The `CosmosDbContext` represents a session with your Cosmos DB database. It provides:

- **Database**: Direct access to the Cosmos `Database` instance
- **Client**: Access to the underlying `CosmosClient`
- **CollectionSet properties**: Strongly-typed access to your collections

```csharp
public class ApplicationDbContext : CosmosDbContext
{
    public ApplicationDbContext(string connectionString) : base(connectionString)
    {
    }
    
    public CollectionSet<User> Users { get; set; }
    public CollectionSet<Product> Products { get; set; }
    public CollectionSet<Order> Orders { get; set; }
}
```

## Configuration

### Connection String

The connection string must contain:
- `AccountEndpoint`: Your Cosmos DB endpoint URL
- `AccountKey`: Your account key
- `Database`: Database name (optional if specified via `[Database]` attribute)

```csharp
var connectionString = "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=your-key==;Database=MyDatabase";
```

### Using ContextOptions

For advanced configuration, use `ContextOptions`:

```csharp
var options = new ContextOptions
{
    ConnectionString = connectionString,
    AllowBulkExecution = true,
    BulkBatchSize = 100,
    ApplicationName = "MyApp",
    MaxRetryAttemptsOnRateLimitedRequests = 9,
    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
};

public class MyDbContext : CosmosDbContext
{
    public MyDbContext(ContextOptionsBase options) : base(options)
    {
    }
    
    public CollectionSet<User> Users { get; set; }
}

var context = new MyDbContext(options);
```

#### Configuration Options

- `ConnectionString`: Cosmos DB connection string
- `AllowBulkExecution`: Enable bulk execution mode (default: false)
- `BulkBatchSize`: Number of operations per batch (default: 100)
- `ApplicationName`: Name for your application
- `MaxRetryAttemptsOnRateLimitedRequests`: Maximum retry attempts
- `MaxRetryWaitTimeOnRateLimitedRequests`: Maximum wait time for retries

## CRUD Operations

### Create (Add)

#### Add a Single Document

```csharp
var user = new User
{
    Id = Guid.NewGuid().ToString(),
    UserId = "user123",
    Name = "Jane Doe"
};

var addedUser = await context.Users.AddAsync(user);
```

#### Add Multiple Documents (Bulk)

```csharp
var users = new List<User>
{
    new User { Id = "1", UserId = "u1", Name = "User 1" },
    new User { Id = "2", UserId = "u2", Name = "User 2" },
    new User { Id = "3", UserId = "u3", Name = "User 3" }
};

int count = await context.Users.AddRangeAsync(users);
Console.WriteLine($"{count} users added successfully");
```

### Read (Find)

#### Find by ID and Partition Key

```csharp
var user = await context.Users.FindAsync("user-id-123", "partition-key");
```

#### Find by ID (without partition key)

```csharp
var user = await context.Users.FindAsync("user-id-123");
```

### Update

#### Update a Single Document

Uses upsert semantics - creates the document if it doesn't exist:

```csharp
user.Name = "Updated Name";
var updatedUser = await context.Users.UpdateAsync(user);
```

#### Update Multiple Documents (Bulk)

```csharp
foreach (var user in users)
{
    user.UpdatedAt = DateTime.UtcNow;
}

int count = await context.Users.UpdateRangeAsync(users);
Console.WriteLine($"{count} users updated");
```

### Delete (Remove)

#### Remove a Single Document

```csharp
var user = await context.Users.FindAsync("user-id", "partition-key");
await context.Users.RemoveAsync(user);
```

#### Remove Multiple Documents (Bulk)

```csharp
var usersToDelete = await context.Users
    .Where(u => u.IsDeleted == true)
    .ToDocumentListAsync();

int count = await context.Users.RemoveRangeAsync(usersToDelete);
Console.WriteLine($"{count} users removed");
```

## Querying Data

Cosmos Entity Mapper provides rich LINQ support for querying:

### Basic Queries

```csharp
// Simple where clause
var activeUsers = await context.Users
    .Where(u => u.IsActive == true)
    .ToDocumentListAsync();

// Multiple conditions
var results = await context.Users
    .Where(u => u.Age > 18 && u.Country == "USA")
    .ToDocumentListAsync();

// OrderBy
var orderedUsers = await context.Users
    .OrderBy(u => u.Name)
    .ToDocumentListAsync();
```

### Materialization Extensions

The library provides special async methods for materializing queries:

#### ToDocumentListAsync

Materializes the query to a list:

```csharp
var users = await context.Users
    .Where(u => u.IsActive)
    .ToDocumentListAsync();
```

#### FirstDocumentAsync / FirstOrDefaultDocumentAsync

Get the first element:

```csharp
var user = await context.Users
    .Where(u => u.Email == "john@example.com")
    .FirstDocumentAsync();

// Returns null if not found
var userOrNull = await context.Users
    .FirstOrDefaultDocumentAsync(u => u.Id == "123");
```

#### SingleDocumentAsync / SingleOrDefaultDocumentAsync

Get a single element (throws if multiple exist):

```csharp
var user = await context.Users
    .SingleDocumentAsync(u => u.Email == "unique@example.com");

// Returns null if not found
var userOrNull = await context.Users
    .SingleOrDefaultDocumentAsync(u => u.Id == "123");
```

#### DocumentCountAsync

Get the count of documents:

```csharp
int userCount = await context.Users
    .Where(u => u.IsActive)
    .DocumentCountAsync();
```

#### AnyDocumentAsync

Check if any documents match:

```csharp
bool hasActiveUsers = await context.Users
    .AnyDocumentAsync(u => u.IsActive);
```

#### AllDocumentAsync

Check if all documents match a condition:

```csharp
bool allVerified = await context.Users
    .AllDocumentAsync(u => u.IsVerified == true);
```

#### AsDocumentEnumerableAsync

Stream results as an async enumerable:

```csharp
await foreach (var user in context.Users.AsDocumentEnumerableAsync())
{
    Console.WriteLine(user.Name);
}
```

### Pagination

#### Offset-Based Pagination

```csharp
int page = 1;
int pageSize = 20;

var users = await context.Users
    .OrderBy(u => u.CreatedAt)
    .PaginateByOffset(page, pageSize)
    .ToDocumentListAsync();
```

**Note**: Microsoft recommends using continuation tokens over offset-based pagination for better consistency in scenarios with concurrent updates.

### Dynamic Ordering

Order by property name dynamically:

```csharp
using System.ComponentModel;

var users = await context.Users
    .OrderByDynamicProperty("Name", ListSortDirection.Ascending)
    .ToDocumentListAsync();
```

### Using AsMaterializable

To avoid conflicts with Entity Framework Core in the same codebase:

```csharp
var users = await context.Users
    .Where(u => u.IsActive)
    .AsMaterializable()
    .ToListAsync();
```

## Dependency Injection

Cosmos Entity Mapper has built-in support for ASP.NET Core dependency injection:

### Register in Startup.cs or Program.cs

#### Option 1: Using Connection String

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = Configuration.GetConnectionString("CosmosDb");
    services.AddCosmosEntityMapper<MyDbContext>(connectionString);
}
```

#### Option 2: Using ContextOptions

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var options = new ContextOptions
    {
        ConnectionString = Configuration.GetConnectionString("CosmosDb"),
        AllowBulkExecution = true,
        BulkBatchSize = 100
    };
    
    services.AddCosmosEntityMapper<MyDbContext>(options);
}
```

#### Option 3: Using Configuration Delegate

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCosmosEntityMapper<MyDbContext>(options =>
    {
        options.ConnectionString = Configuration.GetConnectionString("CosmosDb");
        options.AllowBulkExecution = true;
        options.BulkBatchSize = 100;
        options.ApplicationName = "MyApplication";
    });
}
```

### Inject in Controllers/Services

```csharp
public class UserService
{
    private readonly MyDbContext _context;
    
    public UserService(MyDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .ToDocumentListAsync();
    }
}
```

## Advanced Features

### Accessing Native Cosmos DB APIs

You can access the underlying Cosmos DB SDK directly:

```csharp
// Access the Database instance
var database = context.Database;

// Access the CosmosClient
var client = context.Client;

// Access the Container
var container = context.Users.Container;
```

### Type Projection

Project to derived types:

```csharp
public class BaseEntity
{
    [DocumentKey]
    public string Id { get; set; }
    
    [DocumentPartition]
    public string PartitionKey { get; set; }
}

public class DerivedEntity : BaseEntity
{
    public string AdditionalProperty { get; set; }
}

var derived = context.BaseEntities.OfType<DerivedEntity>();
```

### Bulk Operations

For optimal performance with large datasets, enable bulk execution:

```csharp
var options = new ContextOptions
{
    ConnectionString = connectionString,
    AllowBulkExecution = true,  // Enable bulk mode
    BulkBatchSize = 100          // Operations per batch
};
```

Bulk mode automatically groups operations by partition key for better performance.

### Custom Query Options

The library automatically configures query options for optimal performance:

- **MaxConcurrency**: -1 (unlimited parallelism)
- **EnableScanInQuery**: true
- **MaxBufferedItemCount**: -1 (unlimited buffering)
- **PopulateIndexMetrics**: true

## Best Practices

### 1. Entity Design

- Always use `[DocumentKey]` and `[DocumentPartition]` attributes
- Design partition keys carefully for optimal performance
- Keep document size under 2MB

```csharp
[Collection("Orders")]
public class Order
{
    [DocumentKey]
    public string Id { get; set; }
    
    [DocumentPartition]
    public string CustomerId { get; set; }  // Good partition key
    
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; }
}
```

### 2. Use Bulk Operations for Large Datasets

```csharp
// Good - Bulk operation
await context.Users.AddRangeAsync(largeUserList);

// Avoid - Individual operations in a loop
foreach (var user in largeUserList)
{
    await context.Users.AddAsync(user);  // Less efficient
}
```

### 3. Leverage Async/Await

All operations are async - always use async methods:

```csharp
// Good
var users = await context.Users.ToDocumentListAsync();

// Avoid
var users = context.Users.ToDocumentListAsync().Result;  // Can cause deadlocks
```

### 4. Dispose Context Properly

```csharp
// Using statement ensures proper disposal
using (var context = new MyDbContext(connectionString))
{
    // Your operations
}

// Or with async
await using (var context = new MyDbContext(connectionString))
{
    // Your operations
}
```

### 5. Use Dependency Injection

Register your context in DI container for better lifecycle management:

```csharp
services.AddCosmosEntityMapper<MyDbContext>(connectionString);
```

### 6. Query Optimization

- Use appropriate indexes in Cosmos DB
- Filter early in LINQ queries
- Use projection to retrieve only needed properties
- Leverage partition keys in queries when possible

```csharp
// Good - Filtered early
var user = await context.Users
    .Where(u => u.Id == "123")
    .FirstOrDefaultDocumentAsync();

// Less efficient - Retrieves all then filters
var allUsers = await context.Users.ToDocumentListAsync();
var user = allUsers.FirstOrDefault(u => u.Id == "123");
```

### 7. Error Handling

Handle Cosmos DB exceptions appropriately:

```csharp
using Microsoft.Azure.Cosmos;

try
{
    var user = await context.Users.FindAsync("id", "partition");
}
catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    // Handle not found
}
catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
{
    // Handle rate limiting
}
```

## License

This project is licensed under the GNU General Public License v3.0 or later - see the [License](License) file for details.

Copyright © oakinyelure
