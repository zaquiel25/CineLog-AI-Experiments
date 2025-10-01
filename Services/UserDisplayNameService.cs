using Ezequiel_Movies.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ezequiel_Movies.Services
{
    /// <summary>
    /// Service for retrieving user display names from the database.
    /// TECHNICAL: DisplayName column exists in AspNetUsers but not in IdentityUser model,
    /// requiring direct database access via ADO.NET.
    /// </summary>
    public class UserDisplayNameService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserDisplayNameService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Gets the display name for a user, falling back to username if not set.
        /// </summary>
        /// <param name="userId">The user ID to lookup</param>
        /// <param name="fallbackUsername">Username to use if DisplayName is null or empty</param>
        /// <returns>Display name or fallback username</returns>
        public async Task<string> GetDisplayNameAsync(string userId, string fallbackUsername)
        {
            try
            {
                // Access DisplayName via direct SQL - column exists but not in IdentityUser model
                using var command = _dbContext.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT DisplayName FROM AspNetUsers WHERE Id = @id";

                var parameter = new SqlParameter("@id", userId);
                command.Parameters.Add(parameter);

                // Ensure connection is open
                if (_dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    await _dbContext.Database.OpenConnectionAsync();
                }

                var result = await command.ExecuteScalarAsync();
                var displayName = result as string;

                return !string.IsNullOrWhiteSpace(displayName) ? displayName : fallbackUsername;
            }
            catch
            {
                // If anything fails, return the fallback username
                return fallbackUsername;
            }
        }
    }
}