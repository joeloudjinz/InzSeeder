namespace InzSeeder.Core.Contracts;

/// <summary>
/// Represents an environment-aware seeder that can determine whether it should run in a specific environment.
/// </summary>
public interface IEnvironmentAwareSeeder : IBaseEntityDataSeeder
{
    /// <summary>
    /// Determines whether this seeder should run in the specified environment.
    /// </summary>
    /// <param name="environment">The environment name.</param>
    /// <returns>True if the seeder should run in the specified environment, false otherwise.</returns>
    bool ShouldRunInEnvironment(string environment);
}