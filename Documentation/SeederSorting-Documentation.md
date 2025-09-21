# Seeder Sorting Documentation

## Table of Contents
- [Overview](#overview)
- [Core Concepts](#core-concepts)
  - [Dependency Management](#dependency-management)
  - [IBaseEntityDataSeeder Dependencies Property](#ibaseentitydataseeder-dependencies-property)
  - [Declaring Dependencies](#declaring-dependencies)
- [How Seeder Sorting Works](#how-seeder-sorting-works)
  - [Topological Sorting Algorithm](#topological-sorting-algorithm)
  - [Execution Flow](#execution-flow)
- [Working with Dependencies](#working-with-dependencies)
  - [Basic Dependency Declaration](#basic-dependency-declaration)
  - [Multiple Dependencies](#multiple-dependencies)
  - [No Dependencies](#no-dependencies)
- [Circular Dependency Detection](#circular-dependency-detection)
  - [Detection Mechanism](#detection-mechanism)
  - [Example of Circular Dependency](#example-of-circular-dependency)
- [Implementation Details](#implementation-details)
  - [SeederSorter Class](#seedersorter-class)
  - [Algorithm Complexity](#algorithm-complexity)
  - [Key Data Structures](#key-data-structures)
- [Practical Examples](#practical-examples)
  - [Example 1: Simple Dependency Chain](#example-1-simple-dependency-chain)
  - [Example 2: Complex Dependency Graph](#example-2-complex-dependency-graph)
  - [Example 3: Parallel Execution Paths](#example-3-parallel-execution-paths)
- [Best Practices](#best-practices)
  - [1. Declare All Dependencies Explicitly](#1-declare-all-dependencies-explicitly)
  - [2. Avoid Circular Dependencies](#2-avoid-circular-dependencies)
  - [3. Minimize Dependencies](#3-minimize-dependencies)
  - [4. Use Consistent Naming](#4-use-consistent-naming)
- [Performance Considerations](#performance-considerations)
  - [Algorithm Efficiency](#algorithm-efficiency)
  - [Large Dependency Graphs](#large-dependency-graphs)
- [Summary](#summary)

## Overview

Seeder sorting is a core feature in InzSeeder that ensures seeders execute in the correct order based on their declared dependencies. This document provides a comprehensive guide on how seeder sorting works, its implementation details, and best practices for managing dependencies between seeders.

## Core Concepts

### Dependency Management

InzSeeder uses a topological sorting algorithm to arrange seeders in execution order. This ensures that any seeder that depends on another seeder will execute only after its dependencies have completed.

### IBaseEntityDataSeeder Dependencies Property

Each seeder implements the `IBaseEntityDataSeeder` interface, which includes a `Dependencies` property:

```csharp
public interface IBaseEntityDataSeeder
{
    /// <summary>
    /// Gets the unique name of this seeder.
    /// </summary>
    string SeedName { get; }

    /// <summary>
    /// Gets the collection of seeder types that this seeder depends on.
    /// </summary>
    IEnumerable<Type> Dependencies { get; }
}
```

### Declaring Dependencies

Seeders declare their dependencies by returning a collection of seeder types:

```csharp
public class ProductCategorySeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "product-categories";

    // This seeder depends on CategorySeeder
    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];
    
    // ... rest of implementation
}
```

## How Seeder Sorting Works

### Topological Sorting Algorithm

The `SeederSorter` class implements a topological sort algorithm using depth-first search (DFS) to arrange seeders in dependency order:

1. **Build Lookup**: Create a dictionary mapping seeder names to seeder instances for quick access
2. **Visit Seeders**: For each seeder, recursively visit its dependencies first
3. **Track Visiting**: Maintain a set of seeders currently in the recursion stack to detect circular dependencies
4. **Track Visited**: Maintain a set of fully processed seeders to avoid redundant processing
5. **Collect Results**: Add seeders to the sorted list only after all their dependencies have been processed

### Execution Flow

The sorting process occurs in the `EnvironmentSeedingOrchestrator`:

```csharp
// Filter seeders based on environment and configuration
var seedersToRun = FilterSeedersByProfile(allSeedersAsBaseEntitySeeder, profile).ToList();

// Sort seeders based on their dependencies
var sortedSeeders = SeederSorter.Sort(seedersToRun).ToList();

// Execute seeders in sorted order
foreach (var seeder in sortedSeeders)
{
    // Execute seeder
}
```

## Working with Dependencies

### Basic Dependency Declaration

Declare dependencies by specifying the types of seeders your seeder depends on:

```csharp
public class UserProfileSeeder : IEntityDataSeeder<UserProfile, UserProfileSeedModel>
{
    public string SeedName => "user-profiles";

    // This seeder depends on UserSeeder
    public IEnumerable<Type> Dependencies => [typeof(UserSeeder)];
    
    // ... implementation
}
```

### Multiple Dependencies

A seeder can depend on multiple other seeders:

```csharp
public class OrderSeeder : IEntityDataSeeder<Order, OrderSeedModel>
{
    public string SeedName => "orders";

    // This seeder depends on both UserSeeder and ProductSeeder
    public IEnumerable<Type> Dependencies => [
        typeof(UserSeeder),
        typeof(ProductSeeder)
    ];
    
    // ... implementation
}
```

### No Dependencies

For seeders with no dependencies, return an empty collection:

```csharp
public class CategorySeeder : IEntityDataSeeder<Category, CategorySeedModel>
{
    public string SeedName => "categories";
    
    // No dependencies
    public IEnumerable<Type> Dependencies { get; } = [];
    
    // ... implementation
}
```

## Circular Dependency Detection

### Detection Mechanism

The `SeederSorter` actively detects circular dependencies and throws an `InvalidOperationException` when detected:

1. **Track Visiting State**: Maintain a set of seeders currently in the recursion stack
2. **Check Before Visit**: Before visiting a seeder, check if it's already in the visiting set
3. **Throw Exception**: If a seeder is found in the visiting set, a circular dependency exists

### Example of Circular Dependency

```csharp
// Seeder A depends on Seeder B
public class SeederA : IBaseEntityDataSeeder
{
    public string SeedName => "SeederA";
    public IEnumerable<Type> Dependencies => [typeof(SeederB)];
}

// Seeder B depends on Seeder A (circular!)
public class SeederB : IBaseEntityDataSeeder
{
    public string SeedName => "SeederB";
    public IEnumerable<Type> Dependencies => [typeof(SeederA)];
}
```

When attempting to sort these seeders, an exception will be thrown:

```
InvalidOperationException: Circular dependency detected involving seeder 'SeederA'.
```

## Implementation Details

### SeederSorter Class

The `SeederSorter` is an internal static class that provides the sorting functionality:

```csharp
internal static class SeederSorter
{
    public static IEnumerable<IBaseEntityDataSeeder> Sort(IEnumerable<IBaseEntityDataSeeder> seeders)
    {
        // Implementation details...
    }
    
    private static void Visit(
        IBaseEntityDataSeeder seeder,
        IDictionary<string, IBaseEntityDataSeeder> seederLookup,
        ICollection<IBaseEntityDataSeeder> sortedSeeders,
        ISet<string> visited,
        ISet<string> visiting
    )
    {
        // Recursive visit implementation...
    }
}
```

### Algorithm Complexity

- **Time Complexity**: O(V + E) where V is the number of seeders and E is the number of dependencies
- **Space Complexity**: O(V) for the visited and visiting sets

### Key Data Structures

1. **seederLookup**: Dictionary for O(1) seeder access by name
2. **visited**: Set of fully processed seeders
3. **visiting**: Set of seeders in current recursion stack
4. **sortedSeeders**: Final list of seeders in execution order

## Practical Examples

### Example 1: Simple Dependency Chain

```csharp
// CategorySeeder (no dependencies)
public class CategorySeeder : IEntityDataSeeder<Category, CategorySeedModel>
{
    public string SeedName => "categories";
    public IEnumerable<Type> Dependencies { get; } = [];
}

// ProductSeeder (depends on CategorySeeder)
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];
}

// OrderSeeder (depends on ProductSeeder)
public class OrderSeeder : IEntityDataSeeder<Order, OrderSeedModel>
{
    public string SeedName => "orders";
    public IEnumerable<Type> Dependencies => [typeof(ProductSeeder)];
}
```

Execution order: `CategorySeeder` → `ProductSeeder` → `OrderSeeder`

### Example 2: Complex Dependency Graph

```csharp
// UserSeeder (no dependencies)
public class UserSeeder : IEntityDataSeeder<User, UserSeedModel>
{
    public string SeedName => "users";
    public IEnumerable<Type> Dependencies { get; } = [];
}

// CategorySeeder (no dependencies)
public class CategorySeeder : IEntityDataSeeder<Category, CategorySeedModel>
{
    public string SeedName => "categories";
    public IEnumerable<Type> Dependencies { get; } = [];
}

// ProductSeeder (depends on CategorySeeder)
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];
}

// OrderSeeder (depends on UserSeeder and ProductSeeder)
public class OrderSeeder : IEntityDataSeeder<Order, OrderSeedModel>
{
    public string SeedName => "orders";
    public IEnumerable<Type> Dependencies => [
        typeof(UserSeeder),
        typeof(ProductSeeder)
    ];
}
```

Execution order: `UserSeeder` → `CategorySeeder` → `ProductSeeder` → `OrderSeeder`

### Example 3: Parallel Execution Paths

```csharp
// UserSeeder (no dependencies)
public class UserSeeder : IEntityDataSeeder<User, UserSeedModel>
{
    public string SeedName => "users";
    public IEnumerable<Type> Dependencies { get; } = [];
}

// CategorySeeder (no dependencies)
public class CategorySeeder : IEntityDataSeeder<Category, CategorySeedModel>
{
    public string SeedName => "categories";
    public IEnumerable<Type> Dependencies { get; } = [];
}

// UserProfileSeeder (depends on UserSeeder)
public class UserProfileSeeder : IEntityDataSeeder<UserProfile, UserProfileSeedModel>
{
    public string SeedName => "user-profiles";
    public IEnumerable<Type> Dependencies => [typeof(UserSeeder)];
}

// ProductSeeder (depends on CategorySeeder)
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];
}
```

Execution order: 
1. `UserSeeder` and `CategorySeeder` (can run in parallel, order not guaranteed)
2. `UserProfileSeeder` (after `UserSeeder`)
3. `ProductSeeder` (after `CategorySeeder`)

## Best Practices

### 1. Declare All Dependencies Explicitly

Always declare all dependencies, even indirect ones:

```csharp
// Good: Explicitly declare all dependencies
public class OrderItemSeeder : IEntityDataSeeder<OrderItem, OrderItemSeedModel>
{
    public string SeedName => "order-items";
    public IEnumerable<Type> Dependencies => [
        typeof(OrderSeeder),
        typeof(ProductSeeder)
    ];
}

// Avoid: Relying on transitive dependencies
public class OrderItemSeeder : IEntityDataSeeder<OrderItem, OrderItemSeedModel>
{
    public string SeedName => "order-items";
    // Missing ProductSeeder dependency even though OrderSeeder depends on it
    public IEnumerable<Type> Dependencies => [typeof(OrderSeeder)];
}
```

### 2. Avoid Circular Dependencies

Design your data model to avoid circular dependencies:

```csharp
// Instead of circular references, consider:
// 1. Making one direction optional
// 2. Using separate seeders for relationship data
// 3. Breaking the circular dependency in your data model
```

### 3. Minimize Dependencies

Only declare necessary dependencies to maximize parallel execution:

```csharp
// Good: Only declare what you directly need
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];
}

// Avoid: Declaring unnecessary dependencies
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    // UserSeeder not actually needed for Product seeding
    public IEnumerable<Type> Dependencies => [
        typeof(CategorySeeder),
        typeof(UserSeeder)  // Unnecessary dependency
    ];
}
```

### 4. Use Consistent Naming

Use descriptive, consistent names for seeders:

```csharp
// Good: Clear, descriptive names
public class UserSeeder : IEntityDataSeeder<User, UserSeedModel>
{
    public string SeedName => "users";
}

public class UserProfileSeeder : IEntityDataSeeder<UserProfile, UserProfileSeedModel>
{
    public string SeedName => "user-profiles";
}

// Avoid: Generic or unclear names
public class Seeder1 : IEntityDataSeeder<User, UserSeedModel>
{
    public string SeedName => "s1";
}
```

## Performance Considerations

### Algorithm Efficiency

The topological sort algorithm is highly efficient:
- **Linear time complexity**: O(V + E)
- **Minimal memory overhead**: Only stores necessary state information
- **Single pass**: Each seeder is processed exactly once

### Large Dependency Graphs

For applications with many seeders:
- Dependencies are resolved once at startup
- Minimal impact on seeding performance
- Parallel execution of independent seeders

## Summary

Seeder sorting is a fundamental feature that ensures data integrity by executing seeders in the correct order:

1. **Declarative Dependencies**: Seeders declare their dependencies explicitly
2. **Topological Sorting**: The system arranges seeders using a proven algorithm
3. **Circular Detection**: Prevents infinite loops with clear error messages
4. **Flexible Execution**: Allows parallel execution of independent seeders