# Environment Seeding Orchestration Documentation

## Table of Contents
- [Overview](#overview)
- [Core Concepts](#core-concepts)
  - [EnvironmentSeedingOrchestrator](#environmentseedingorchestrator)
  - [Key Components](#key-components)
- [Orchestration Process](#orchestration-process)
  - [Validation Phase](#validation-phase)
  - [Filtering Phase](#filtering-phase)
  - [Sorting Phase](#sorting-phase)
  - [Execution Phase](#execution-phase)
- [Filtering Logic](#filtering-logic)
  - [Explicit Enablement](#explicit-enablement)
  - [Strict Mode](#strict-mode)
  - [Environment Awareness](#environment-awareness)
  - [Environment Compatibility](#environment-compatibility)
- [Sorting Mechanism](#sorting-mechanism)
  - [Topological Sort](#topological-sort)
  - [Dependency Resolution](#dependency-resolution)
  - [Circular Dependency Detection](#circular-dependency-detection)
- [Execution Process](#execution-process)
  - [Transaction Management](#transaction-management)
  - [Batch Processing](#batch-processing)
  - [Idempotency](#idempotency)
- [Error Handling](#error-handling)
  - [Validation Errors](#validation-errors)
  - [Execution Errors](#execution-errors)
  - [Rollback Mechanism](#rollback-mechanism)
- [Performance Considerations](#performance-considerations)
  - [Memory Management](#memory-management)
  - [Database Operations](#database-operations)
- [Implementation Details](#implementation-details)
  - [EnvironmentSeedingOrchestrator Class](#environmentseedingorchestrator-class)
  - [FilterSeedersByProfile Method](#filterseedersbyprofile-method)
  - [Generic Method Invocation](#generic-method-invocation)
- [Practical Examples](#practical-examples)
  - [Simple Orchestration](#simple-orchestration)
  - [Complex Orchestration with Dependencies](#complex-orchestration-with-dependencies)
- [Best Practices](#best-practices)
  - [1. Proper Error Handling](#1-proper-error-handling)
  - [2. Efficient Dependency Declaration](#2-efficient-dependency-declaration)
  - [3. Environment Configuration](#3-environment-configuration)
- [Summary](#summary)

## Overview

The Environment Seeding Orchestration is a core feature of InzSeeder that manages the complete data seeding process from start to finish. It ensures seeders are executed in the correct order based on dependencies, filtered appropriately for the current environment, and run within a transactional context to maintain data integrity.

This document provides a comprehensive guide to understanding how the orchestration process works, its implementation details, and best practices for working with environment-aware seeding.

## Core Concepts

### EnvironmentSeedingOrchestrator

The `EnvironmentSeedingOrchestrator` is an internal static class that serves as the main entry point for executing the data seeding process. It is responsible for coordinating all aspects of the seeding workflow, including validation, filtering, sorting, and execution.

### Key Components

1. **Validation Service**: Validates the seeding configuration before execution
2. **Filtering Logic**: Determines which seeders should run based on environment and configuration
3. **Sorting Algorithm**: Arranges seeders in dependency order using topological sorting
4. **Execution Engine**: Runs seeders within a transactional context
5. **Error Handling**: Manages failures and ensures transaction rollback when needed

## Orchestration Process

The orchestration process follows a sequential workflow that ensures proper execution of all seeders:

### Validation Phase

Before any seeding begins, the orchestrator validates the seeding configuration using the `SeedingProfileValidationService`. If the configuration is invalid, an `InvalidOperationException` is thrown and the process is aborted.

```csharp
if (!validationService.ValidateSettings(settings))
{
    throw new InvalidOperationException("Invalid seeding configuration detected. Seeding process aborted.");
}
```

### Filtering Phase

The orchestrator retrieves all registered seeders and filters them based on the current environment and seeding profile using the `FilterSeedersByProfile` method:

```csharp
var seedersToRun = FilterSeedersByProfile(allSeedersAsBaseEntitySeeder, profile).ToList();
```

### Sorting Phase

Filtered seeders are sorted based on their declared dependencies using the `SeederSorter.Sort` method:

```csharp
var sortedSeeders = SeederSorter.Sort(seedersToRun).ToList();
```

### Execution Phase

Seeders are executed in sorted order within a database transaction:

```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
// ... execute seeders ...
await transaction.CommitAsync(cancellationToken);
```

## Filtering Logic

The filtering process determines which seeders should run based on multiple factors, following a specific priority order.

### Explicit Enablement

If a seeder is explicitly listed in the `EnabledSeeders` configuration, it will always run regardless of other settings:

```csharp
// Check if seeder is explicitly enabled for this profile
if (profile.EnabledSeeders != null && profile.EnabledSeeders.Contains(seeder.SeedName)) return true;
```

### Strict Mode

When `StrictMode` is enabled, only explicitly enabled seeders will run. All other filtering mechanisms are bypassed:

```csharp
// If in strict mode, only explicitly enabled seeders should run
if (profile.StrictMode && (profile.EnabledSeeders == null || !profile.EnabledSeeders.Contains(seeder.SeedName))) return false;
```

### Environment Awareness

Seeders implementing the `IEnvironmentAwareSeeder` interface can provide custom logic to determine if they should run in the current environment:

```csharp
// Check if seeder has environment awareness
if (seeder is IEnvironmentAwareSeeder envAwareSeeder) 
    return envAwareSeeder.ShouldRunInEnvironment(EnvironmentUtility.Environment());
```

### Environment Compatibility

Seeders with the `EnvironmentCompatibilityAttribute` are filtered based on the attribute settings:

```csharp
// Check if seeder has environment compatibility attribute
if (!seeder.IsAllowedInEnvironment(EnvironmentUtility.Environment())) return false;
```

## Sorting Mechanism

The sorting mechanism ensures seeders execute in the correct order based on their dependencies.

### Topological Sort

The `SeederSorter` implements a topological sort algorithm using depth-first search to arrange seeders in dependency order:

1. **Build Lookup**: Create a dictionary mapping seeder names to seeder instances for quick access
2. **Visit Seeders**: For each seeder, recursively visit its dependencies first
3. **Track Visiting**: Maintain a set of seeders currently in the recursion stack to detect circular dependencies
4. **Track Visited**: Maintain a set of fully processed seeders to avoid redundant processing
5. **Collect Results**: Add seeders to the sorted list only after all their dependencies have been processed

### Dependency Resolution

Dependencies are resolved by matching the dependency types declared in the `Dependencies` property with registered seeder instances:

```csharp
foreach (var dependencyType in seeder.Dependencies)
{
    var dependencySeeder = seederLookup.Values.FirstOrDefault(s => s.GetType() == dependencyType);
    if (dependencySeeder != null) Visit(dependencySeeder, seederLookup, sortedSeeders, visited, visiting);
}
```

### Circular Dependency Detection

The sorting algorithm actively detects circular dependencies and throws an `InvalidOperationException` when detected:

```csharp
// If the seeder is already in the current recursion stack, we have a circular dependency.
if (!visiting.Add(seederName)) 
    throw new InvalidOperationException($"Circular dependency detected involving seeder '{seederName}'.");
```

## Execution Process

The execution process handles the actual running of seeders with proper transaction management and error handling.

### Transaction Management

All seeders execute within a single database transaction to ensure atomicity:

```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
try
{
    // Execute all seeders
    await transaction.CommitAsync(cancellationToken);
}
catch (Exception)
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```

### Batch Processing

Each seeder's data is processed in batches to manage memory usage and database performance. The `SeederExecutor` handles the detailed process of:

1. Retrieving seed data and its hash
2. Checking for existing seed history
3. Deserializing data from JSON
4. Processing entities in configurable batch sizes
5. Creating new or updating existing entities
6. Recording seed history

### Idempotency

The execution process ensures idempotency by checking content hashes before processing:

```csharp
var existingSeedHistory = await dbContext.Set<SeedHistory>()
    .FirstOrDefaultAsync(sh => sh.SeedIdentifier == seedName, cancellationToken);
    
if (existingSeedHistory != null && existingSeedHistory.ContentHash == hash)
{
    // Skip processing if content hasn't changed
    return;
}
```

## Error Handling

The orchestration process includes comprehensive error handling to ensure data integrity and provide meaningful error messages.

### Validation Errors

Invalid seeding configurations result in immediate termination of the process:

```csharp
if (!validationService.ValidateSettings(settings))
{
    throw new InvalidOperationException("Invalid seeding configuration detected. Seeding process aborted.");
}
```

### Execution Errors

Individual seeder failures are logged and re-thrown to ensure transaction rollback:

```csharp
catch (Exception ex)
{
    logger.LogCritical(ex, "Seeder '{SeederName}' execution failed catastrophically.", seeder.SeedName);
    throw; // Re-throw to ensure the transaction is rolled back.
}
```

### Rollback Mechanism

When any seeder fails, the entire transaction is rolled back to maintain data consistency:

```csharp
catch (Exception ex)
{
    await transaction.RollbackAsync(cancellationToken);
    logger.LogError(ex, "Environment-aware data seeding process failed. Transaction has been rolled back.");
    throw;
}
```

## Performance Considerations

The orchestration process is designed with performance in mind, especially for large-scale seeding operations.

### Memory Management

The process uses batch processing to manage memory usage effectively:

```csharp
for (var i = 0; i < models.Count; i += batchSize)
{
    var batch = models.Skip(i).Take(batchSize).ToList();
    // Process batch...
}
```

### Database Operations

Database operations are optimized through:

1. **Batched Saves**: Entities are saved in configurable batch sizes
2. **Single Transaction**: All operations occur within a single transaction
3. **Efficient Queries**: Existing entities are loaded once and cached in memory

## Implementation Details

### EnvironmentSeedingOrchestrator Class

The orchestrator is implemented as an internal static class with a single public method:

```csharp
internal static class EnvironmentSeedingOrchestrator
{
    public static async Task Orchestrate(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Implementation details...
    }
    
    private static IEnumerable<IBaseEntityDataSeeder> FilterSeedersByProfile(
        IEnumerable<IBaseEntityDataSeeder> seeders, 
        SeedingProfile profile)
    {
        // Implementation details...
    }
}
```

### FilterSeedersByProfile Method

This method implements the complex filtering logic with proper priority handling:

```csharp
private static IEnumerable<IBaseEntityDataSeeder> FilterSeedersByProfile(
    IEnumerable<IBaseEntityDataSeeder> seeders, 
    SeedingProfile profile)
{
    return seeders.Where(seeder =>
    {
        // Explicit enablement check
        if (profile.EnabledSeeders != null && profile.EnabledSeeders.Contains(seeder.SeedName)) 
            return true;

        // Strict mode check
        if (profile.StrictMode && (profile.EnabledSeeders == null || 
            !profile.EnabledSeeders.Contains(seeder.SeedName))) 
            return false;

        // Environment awareness check
        if (seeder is IEnvironmentAwareSeeder envAwareSeeder) 
            return envAwareSeeder.ShouldRunInEnvironment(EnvironmentUtility.Environment());

        // Environment compatibility check
        if (!seeder.IsAllowedInEnvironment(EnvironmentUtility.Environment())) 
            return false;

        // Default behavior - run if EnabledSeeders is null or if the seeder is in the EnabledSeeders list
        return profile.EnabledSeeders == null || profile.EnabledSeeders.Contains(seeder.SeedName);
    });
}
```

### Generic Method Invocation

The orchestrator uses reflection to invoke the generic `SeederExecutor.Execute` method:

```csharp
var genericExecuteMethod = typeof(SeederExecutor).GetMethod(
    "Execute",
    BindingFlags.NonPublic | BindingFlags.Static
);

var concreteExecuteMethod = genericExecuteMethod.MakeGenericMethod(entityType, modelType);
var task = (Task?)concreteExecuteMethod.Invoke(null, [seeder, serviceProvider, cancellationToken]);
```

## Practical Examples

### Simple Orchestration

A basic orchestration with minimal configuration:

```csharp
// Configuration
{
  "Seeder": {
    "Profile": {
      "StrictMode": false,
      "EnabledSeeders": []
    }
  }
}

// Seeders
public class CategorySeeder : BaseEntitySeeder<Category, CategorySeedModel>
{
    public string SeedName => "categories";
    public IEnumerable<Type> Dependencies { get; } = [];
    // ... implementation
}

// Orchestration result: CategorySeeder runs in Development environment
```

### Complex Orchestration with Dependencies

A complex orchestration with multiple seeders and dependencies:

```csharp
// Configuration
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["users", "products", "orders"]
    }
  }
}

// Seeders
[EnvironmentCompatibility(productionSafe: true)]
public class UserSeeder : BaseEntitySeeder<User, UserSeedModel>, IEnvironmentAwareSeeder
{
    public string SeedName => "users";
    public IEnumerable<Type> Dependencies { get; } = [];
    
    public bool ShouldRunInEnvironment(string environment) => true;
    // ... implementation
}

public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];
    // ... implementation
}

public class OrderSeeder : BaseEntitySeeder<Order, OrderSeedModel>
{
    public string SeedName => "orders";
    public IEnumerable<Type> Dependencies => [typeof(UserSeeder), typeof(ProductSeeder)];
    // ... implementation
}

// Execution order: UserSeeder → CategorySeeder → ProductSeeder → OrderSeeder
```

## Best Practices

### 1. Proper Error Handling

Always implement proper error handling in custom seeders:

```csharp
public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>
{
    public void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        try
        {
            existingEntity.Name = model.Name;
            existingEntity.Price = model.Price;
        }
        catch (Exception ex)
        {
            // Log specific errors for troubleshooting
            throw new InvalidOperationException($"Failed to update product {model.Id}", ex);
        }
    }
}
```

### 2. Efficient Dependency Declaration

Declare only necessary dependencies to maximize parallel execution:

```csharp
// Good: Only declare what you directly need
public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>
{
    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];
}

// Avoid: Declaring unnecessary dependencies
public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>
{
    // UserSeeder not actually needed for Product seeding
    public IEnumerable<Type> Dependencies => [
        typeof(CategorySeeder),
        typeof(UserSeeder)  // Unnecessary dependency
    ];
}
```

### 3. Environment Configuration

Use appropriate environment configurations for different scenarios:

```json
// Development
{
  "Seeder": {
    "Profile": {
      "StrictMode": false,
      "EnabledSeeders": ["users", "products", "test-data"]
    }
  }
}

// Production
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["categories", "essential-config"]
    }
  }
}
```

## Summary

The Environment Seeding Orchestration is a sophisticated system that manages the complete data seeding workflow:

1. **Validation**: Ensures configuration integrity before execution
2. **Filtering**: Applies environment-specific filtering rules
3. **Sorting**: Arranges seeders in dependency order using topological sorting
4. **Execution**: Runs seeders within a transactional context with proper error handling
5. **Performance**: Optimizes memory usage and database operations through batching

The orchestration process combines multiple filtering mechanisms (explicit enablement, strict mode, environment awareness, and compatibility attributes) with a robust dependency resolution system to provide a flexible yet reliable seeding solution that works across different environments and scenarios.