using System.Text.Json;
using InzSeeder.Core.Models;
using InzSeeder.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for handling audit logging of seeding operations.
/// </summary>
public class SeedingAuditService
{
    private readonly ILogger<SeedingAuditService> _logger;
    private string _auditLogPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedingAuditService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SeedingAuditService(ILogger<SeedingAuditService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs a seeding operation.
    /// </summary>
    /// <param name="seederName">The name of the seeder.</param>
    /// <param name="operation">The operation that was performed.</param>
    /// <param name="details">Additional details about the operation.</param>
    /// <param name="success">Whether the operation was successful.</param>
    /// <param name="errorMessage">Error message if the operation failed.</param>
    /// <param name="operatorName">The operator who initiated the operation (if available).</param>
    public async Task LogOperationAsync(
        string seederName,
        string operation,
        string details,
        bool success,
        string? errorMessage = null,
        string? operatorName = null
    )
    {
        if (string.IsNullOrEmpty(_auditLogPath)) UpdateAuditLogPath();

        try
        {
            var auditLog = new SeedingAuditLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Environment = EnvironmentUtility.Environment(),
                SeederName = seederName,
                Operation = operation,
                Details = details,
                Success = success,
                ErrorMessage = errorMessage,
                Operator = operatorName
            };

            // Log to file
            await WriteAuditLogToFileAsync(auditLog);

            // Log to console/logger
            if (success)
            {
                _logger.LogInformation(
                    "Seeding Operation: Environment={Environment}, Seeder={SeederName}, Operation={Operation}, Details={Details}",
                    EnvironmentUtility.Environment(), seederName, operation, details
                );
            }
            else
            {
                _logger.LogError(
                    "Seeding Operation Failed: Environment={Environment}, Seeder={SeederName}, Operation={Operation}, Details={Details}, Error={ErrorMessage}",
                    EnvironmentUtility.Environment(), seederName, operation, details, errorMessage
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log seeding operation");
        }
    }


    /// <summary>
    /// Gets all audit logs from the current session.
    /// </summary>
    /// <returns>A list of audit logs.</returns>
    public async Task<List<SeedingAuditLog>> GetAuditLogsAsync()
    {
        if (string.IsNullOrEmpty(_auditLogPath)) throw new InvalidOperationException("Audit log path is not set, make sure to call SetEnvironment(environment) after initializing the auditor");

        var auditLogs = new List<SeedingAuditLog>();

        try
        {
            if (File.Exists(_auditLogPath))
            {
                var content = await File.ReadAllTextAsync(_auditLogPath);
                var lines = content.Split([Environment.NewLine + Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    try
                    {
                        var auditLog = JsonSerializer.Deserialize<SeedingAuditLog>(line);
                        if (auditLog != null)
                        {
                            auditLogs.Add(auditLog);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize audit log entry");
                        // Skip invalid entries
                        continue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read audit logs from file");
        }

        return auditLogs;
    }

    /// <summary>
    /// Updates the audit log path based on the current environment and timestamp.
    /// </summary>
    private void UpdateAuditLogPath()
    {
        try
        {
            // Generate a new audit log file with timestamp for each run
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            _auditLogPath = Path.Combine(AppContext.BaseDirectory, "logs", EnvironmentUtility.Environment(), $"seeding-log-{timestamp}.log");

            // Ensure the logs directory exists
            var logDirectory = Path.GetDirectoryName(_auditLogPath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update audit log path");
            _auditLogPath = string.Empty; // Reset path on error
        }
    }

    /// <summary>
    /// Writes an audit log entry to a file.
    /// </summary>
    /// <param name="auditLog">The audit log entry to write.</param>
    private async Task WriteAuditLogToFileAsync(SeedingAuditLog auditLog)
    {
        if (string.IsNullOrEmpty(_auditLogPath)) throw new InvalidOperationException("Audit log path is not set, make sure to call SetEnvironment(environment) after initializing the auditor");

        try
        {
            var json = JsonSerializer.Serialize(auditLog, new JsonSerializerOptions { WriteIndented = true });
            await File.AppendAllTextAsync(_auditLogPath, json + Environment.NewLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log to file");
        }
    }
}