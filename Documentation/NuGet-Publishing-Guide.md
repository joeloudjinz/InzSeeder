# Publishing NuGet Package

This document explains how to publish the InzSeeder.Core library as a NuGet package using the GitHub Actions workflow with trusted publishing. For safety, the workflow is configured to be manually triggered only.

## GitHub Actions Workflow

The repository includes a GitHub Actions workflow that packs and publishes the NuGet package when manually triggered. The workflow uses NuGet's trusted publishing feature, which eliminates the need for long-lived API keys.

### Workflow Triggers

The workflow is triggered only manually through the GitHub Actions interface for safety:
1. Go to the "Actions" tab in your repository
2. Select "Publish NuGet Package" workflow
3. Click "Run workflow" and confirm

### Workflow Steps

1. **Setup**: The workflow sets up the .NET environment (version 9.0.x)
2. **Restore**: Restores all project dependencies
3. **Build**: Builds the solution in Release configuration
4. **Pack**: Creates the NuGet package from the InzSeeder.Core project
5. **Login**: Uses trusted publishing to obtain a temporary API key
6. **Publish**: Publishes the package to NuGet.org using the temporary API key

## Publishing a New Version

To publish a new version of the NuGet package:

1. Update the version in `InzSeeder.Core/InzSeeder.Core.csproj`:
   ```xml
   <PropertyGroup>
     <!-- ... other properties ... -->
     <PackageVersion>2.0.0</PackageVersion>
     <!-- ... other properties ... -->
   </PropertyGroup>
   ```

2. Commit and push your changes:
   ```bash
   git add InzSeeder.Core/InzSeeder.Core.csproj
   git commit -m "Bump version to 2.0.0"
   git push
   ```

3. Trigger the workflow manually:
   - Go to the "Actions" tab in your repository
   - Select "Publish NuGet Package" workflow
   - Click "Run workflow" and confirm

## Trusted Publishing Setup

Instead of using long-lived API keys, this workflow uses NuGet's trusted publishing feature. To set this up:

1. Create a trusted publishing policy on NuGet.org:
   - Log into NuGet.org
   - Click your username and select "Trusted Publishing"
   - Add a new trusted publishing policy with:
     * Repository Owner: Your GitHub username or organization
     * Repository name: InzSeeder
     * Workflow file name: publish-nuget.yml
     * Environment: (leave blank unless using GitHub Environments)

2. Add your NuGet username as a GitHub secret:
   - Go to Settings > Secrets and variables > Actions
   - Click "New repository secret"
   - Name it `NUGET_USERNAME`
   - Enter your NuGet.org username as the value

With trusted publishing, no long-lived API keys are stored in your repository. Instead, GitHub Actions securely exchanges an OIDC token with NuGet.org to obtain a temporary API key for each publish operation.

## Local Publishing

To pack and publish locally, you'll still need a traditional API key:

1. Generate an API key at https://www.nuget.org/account/apikeys
2. Pack the NuGet package:
   ```bash
   cd InzSeeder.Core
   dotnet pack --configuration Release
   ```

3. Publish the package:
   ```bash
   dotnet nuget push bin/Release/Inz.Seeder.2.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
   ```