# InzSeeder StrictMode Documentation

## Overview

StrictMode is a crucial configuration option in InzSeeder that controls how seeders are filtered and executed based on the current environment configuration. This document provides a comprehensive guide on how StrictMode works with other configuration options and features.

## Core Concepts

### SeederConfiguration Structure

The seeder configuration consists of two main components:

```csharp
public class SeederConfiguration
{
    /// <summary>
    /// Gets or sets the profile of the current environment.
    /// </summary>
    public SeedingProfile Profile { get; set; } = new();

    /// <summary>
    /// Gets or sets the batch processing settings.
    /// </summary>
    public SeederBatchSettings BatchSettings { get; set; } = new();
}

public class SeedingProfile
{
    /// <summary>
    /// Gets or sets a value indicating whether strict mode is enabled.
    /// In strict mode, only explicitly enabled seeders will run.
    /// </summary>
    public bool StrictMode { get; set; }

    /// <summary>
    /// Gets or sets the list of seeders that are enabled for this profile.
    /// </summary>
    public List<string>? EnabledSeeders { get; set; }
}
```

### Key Components

1. **StrictMode**: Determines filtering behavior
2. **EnabledSeeders**: Explicitly allowed seeders
3. **IEnvironmentAwareSeeder**: Interface for custom environment logic
4. **EnvironmentCompatibilityAttribute**: Declarative environment restrictions
5. **Environment-specific JSON files**: Data files like `users.Development.json`

## How StrictMode Works

### StrictMode = FALSE (Default/Lenient Mode)

When StrictMode is disabled (false), the filtering process follows this logic:

1. **Check Explicit Enablement**: If seeder is in `EnabledSeeders` list → RUN
2. **Check Environment Awareness**: If seeder implements `IEnvironmentAwareSeeder` → Use custom logic
3. **Check Compatibility Attributes**: If seeder has `EnvironmentCompatibilityAttribute` → Apply restrictions
4. **Default Behavior**: If no restrictions apply → RUN

### StrictMode = TRUE (Strict Mode)

When StrictMode is enabled (true), the filtering is more restrictive:

1. **Check Explicit Enablement**: If seeder is in `EnabledSeeders` list → RUN
2. **Strict Restriction**: If not explicitly enabled → DO NOT RUN (bypass all other checks)

## Working with EnabledSeeders

### Basic Configuration

```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": false,
      "EnabledSeeders": ["products", "users", "categories"]
    }
  }
}
```

In a production environment:

```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["categories"]
    }
  }
}
```

### Behavior Examples

#### Development Configuration (StrictMode = false)
```csharp
// With configuration:
// EnabledSeeders: ["products", "users"]
// StrictMode: false

// Results:
// products seeder: RUNS (explicitly enabled)
// users seeder: RUNS (explicitly enabled)
// categories seeder: MAY RUN (depends on other factors)
```

#### Production Configuration (StrictMode = true)
```csharp
// With configuration:
// EnabledSeeders: ["categories"]
// StrictMode: true

// Results:
// categories seeder: RUNS (explicitly enabled)
// products seeder: DOES NOT RUN (not explicitly enabled + strict mode)
// users seeder: DOES NOT RUN (not explicitly enabled + strict mode)
```

## Using IEnvironmentAwareSeeder

### Interface Definition

```csharp
public interface IEnvironmentAwareSeeder
{
    bool ShouldRunInEnvironment(string environment);
}
```

### Implementation Example

```csharp
public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>, IEnvironmentAwareSeeder
{
    public bool ShouldRunInEnvironment(string environment)
    {
        // Custom logic to determine if this seeder should run
        return environment?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true ||
               environment?.Equals("Staging", StringComparison.OrdinalIgnoreCase) == true;
    }
    
    // ... other implementation
}
```

### Interaction with StrictMode

#### When StrictMode = FALSE:
- `IEnvironmentAwareSeeder.ShouldRunInEnvironment()` is called for seeders NOT in `EnabledSeeders`
- Allows fine-grained control over environment-specific execution

#### When StrictMode = TRUE:
- `IEnvironmentAwareSeeder.ShouldRunInEnvironment()` is NEVER called for seeders NOT in `EnabledSeeders`
- Only explicitly enabled seeders run, regardless of custom environment logic

## Using EnvironmentCompatibilityAttribute

### Attribute Definition

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class EnvironmentCompatibilityAttribute : Attribute
{
    public bool ProductionSafe { get; }
    public string[] AllowedEnvironments { get; }
    
    public EnvironmentCompatibilityAttribute(bool productionSafe = false, params string[] allowedEnvironments)
    {
        ProductionSafe = productionSafe;
        AllowedEnvironments = allowedEnvironments ?? [];
    }
}
```

### Usage Examples

#### Production-Safe Seeder

```csharp
[EnvironmentCompatibility(productionSafe: true)]
public class CategorySeeder : BaseEntitySeeder<Category, CategorySeedModel>
{
    // This seeder is marked as safe for production
}
```

#### Environment-Restricted Seeder

```csharp
[EnvironmentCompatibility(allowedEnvironments: "Development", "Staging")]
public class UserSeeder : BaseEntitySeeder<User, UserSeedModel>
{
    // This seeder can only run in Development or Staging environments
}
```

### Logic Implementation

The `IsAllowedInEnvironment` extension method implements the following rules:

1. **No Attribute**: Seeder allowed in ALL environments
2. **Empty AllowedEnvironments**: Seeder allowed in ALL environments
3. **Specified Environments**: Seeder only allowed in listed environments (case-insensitive)

### Interaction with StrictMode

#### When StrictMode = FALSE:
- `EnvironmentCompatibilityAttribute` restrictions are applied to seeders NOT in `EnabledSeeders`
- Provides declarative environment restrictions

#### When StrictMode = TRUE:
- `EnvironmentCompatibilityAttribute` restrictions are NOT applied to seeders NOT in `EnabledSeeders`
- Only explicitly enabled seeders run, regardless of attribute settings

## Environment-Specific JSON Files

### File Naming Convention

```
SeedData/
├── users.json                 # Default data
├── users.Development.json     # Development-specific data
├── users.Production.json      # Production-specific data
├── users.Staging.json         # Staging-specific data
└── Users/
    ├── users.Development.json # Nested directory structure
    └── users.Production.json
```

### Data Loading Logic

The `EmbeddedResourceSeedDataProvider` follows this priority:

1. **Environment-Specific First**: Look for `seedname.Environment.json`
2. **Default Fallback**: Look for `seedname.json`
3. **Directory Structure**: Support nested directories (users in Users/ subdirectory)

### Interaction with Configuration

The environment used for data loading comes from:
1. `ASPNETCORE_ENVIRONMENT` environment variable
2. Command-line `--environment` argument
3. Configuration in `SeederConfiguration.Environment`

## Comprehensive Examples

### Example 1: Development Environment Configuration

```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": false,
      "EnabledSeeders": ["products", "users"]
    }
  }
}
```

With seeders:
- `ProductSeeder` (no restrictions)
- `UserSeeder` (restricted to Development/Staging via attribute)
- `CategorySeeder` (marked production-safe)

Result:
- `products`: RUNS (explicitly enabled)
- `users`: RUNS (explicitly enabled)
- `categories`: RUNS (not explicitly enabled but allowed in lenient mode)

### Example 2: Production Environment Configuration

```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["categories"]
    }
  }
}
```

With the same seeders as above:

Result:
- `categories`: RUNS (explicitly enabled)
- `products`: DOES NOT RUN (not explicitly enabled + strict mode)
- `users`: DOES NOT RUN (not explicitly enabled + strict mode)

### Example 3: Mixed Configuration with Custom Logic

```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": false,
      "EnabledSeeders": ["categories"]
    }
  }
}
```

With:
- `ProductSeeder` implementing `IEnvironmentAwareSeeder` (returns false for Staging)
- `UserSeeder` with `[EnvironmentCompatibility(allowedEnvironments: "Development")]`
- `CategorySeeder` (no restrictions)

Result:
- `categories`: RUNS (explicitly enabled)
- `products`: DOES NOT RUN (custom logic returns false for Staging)
- `users`: DOES NOT RUN (attribute restriction for Development only)

## Best Practices

### 1. Use StrictMode in Production

Always enable StrictMode in production environments:

```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["essential-data-only"]
    }
  }
}
```

### 2. Explicitly List All Required Seeders

In strict mode, be explicit about what should run:

```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["categories", "system-config", "lookup-data"]
    }
  }
}
```

### 3. Use Attributes for Declarative Restrictions

Mark seeders with appropriate attributes:

```csharp
[EnvironmentCompatibility(productionSafe: true, allowedEnvironments: "Development", "Staging", "Production")]
public class EssentialDataSeeder : BaseEntitySeeder<EssentialData, EssentialDataModel>
{
    // Safe for all environments
}

[EnvironmentCompatibility(allowedEnvironments: "Development", "Staging")]
public class TestDataSeeder : BaseEntitySeeder<TestData, TestSeedModel>
{
    // Only for non-production environments
}
```

### 4. Implement Custom Logic When Needed

Use `IEnvironmentAwareSeeder` for complex environment logic:

```csharp
public class ConditionalSeeder : BaseEntitySeeder<MyEntity, MyModel>, IEnvironmentAwareSeeder
{
    public bool ShouldRunInEnvironment(string environment)
    {
        // Complex logic based on multiple factors
        return environment != "Production" || SomeExternalCondition();
    }
}
```

### 5. Organize Seed Data Files

Structure your seed data appropriately:

```
SeedData/
├── essential/
│   ├── categories.Production.json
│   └── system-config.Production.json
├── test/
│   ├── users.Development.json
│   └── test-data.Development.json
└── shared/
    └── lookup-data.json
```

## Common Scenarios

### Scenario 1: Adding a New Seeder

When adding a new seeder to your application:

1. **Development**: Add to `EnabledSeeders` in the Development environment configuration
2. **Testing**: Verify it works with current StrictMode settings
3. **Production**: Only add to `EnabledSeeders` in the Production environment configuration if StrictMode=true

### Scenario 2: Environment Migration

When moving from one environment to another:

1. **Check StrictMode**: Understand the filtering behavior in the target environment
2. **Review EnabledSeeders**: Ensure required seeders are listed in the target environment configuration
3. **Verify Attributes**: Confirm environment compatibility
4. **Check Custom Logic**: Does `IEnvironmentAwareSeeder` logic prevent execution?
5. **Validate Data Files**: Do the required JSON files exist?

### Scenario 3: Troubleshooting Seeding Issues

When seeders aren't running as expected:

1. **Check StrictMode**: Is it preventing execution in the current environment?
2. **Verify Enablement**: Is the seeder in `EnabledSeeders` in the current environment configuration?
3. **Review Attributes**: Are there environment restrictions?
4. **Check Custom Logic**: Does `IEnvironmentAwareSeeder` logic prevent execution?
5. **Validate Data Files**: Do the required JSON files exist?

## Advanced Configuration Patterns

### Pattern 1: Environment-Specific Configurations

Different environments use different configuration files with the appropriate settings:

Development (`appsettings.Development.json`):
```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": false,
      "EnabledSeeders": ["products", "users", "categories", "test-data"]
    },
    "BatchSettings": {
      "DefaultBatchSize": 50
    }
  }
}
```

Staging (`appsettings.Staging.json`):
```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["products", "users", "categories"]
    },
    "BatchSettings": {
      "DefaultBatchSize": 200
    }
  }
}
```

Production (`appsettings.Production.json`):
```json
{
  "Seeder": {
    "Profile": {
      "StrictMode": true,
      "EnabledSeeders": ["categories", "essential-config"]
    },
    "BatchSettings": {
      "DefaultBatchSize": 100
    }
  }
}
```

### Pattern 2: Role-Based Seeding

```csharp
[EnvironmentCompatibility(allowedEnvironments: "Development", "Staging")]
public class AdminUserSeeder : BaseEntitySeeder<User, UserSeedModel>
{
    // Only for non-production environments
}

[EnvironmentCompatibility(productionSafe: true)]
public class SystemConfigSeeder : BaseEntitySeeder<SystemConfig, SystemConfigModel>
{
    // Safe for all environments
}
```

## Summary

StrictMode provides a powerful mechanism for controlling seeder execution:
- **False (Default)**: Flexible, allows automatic filtering based on environment awareness
- **True (Strict)**: Predictable, only explicitly enabled seeders run

When combined with `EnabledSeeders`, `IEnvironmentAwareSeeder`, `EnvironmentCompatibilityAttribute`, and environment-specific JSON files, StrictMode enables precise control over your data seeding process across different environments, ensuring both flexibility in development and safety in production.