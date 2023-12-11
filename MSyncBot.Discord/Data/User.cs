using DSharpPlus.Entities;
using MySql.Data.MySqlClient;

namespace MSyncBot.Discord.Data;

public class User
{
    public ulong Id;
    public string Username;
    public string AvatarUrl;

    public User(DiscordUser user)
    {
        Id = user.Id;
        Username = user.Username;
        AvatarUrl = user.AvatarUrl;
    }
    
    public User(ulong id, string username, string avatarUrl)
    {
        Id = id;
        Username = username;
        AvatarUrl = avatarUrl;
    }

    public async Task AddAsync()
    {
        try
        {
            Bot.Logger.LogProcess($"Adding a new user: {Username} ({Id}) to the database...");
            
            var sqlQuery = "INSERT INTO Users (Id, Username, AvatarUrl)" +
                " VALUES (@Id, @Username, AvatarUrl)";
            await Bot.Database.ExecuteNonQueryAsync(sqlQuery, 
                new MySqlParameter("Id", Id),
                new MySqlParameter("Username", Username), 
                new MySqlParameter("AvatarUrl", AvatarUrl));
            
            Bot.Logger.LogSuccess("The new user has been successfully added to the database.");
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
    }

    public async Task<User> GetInfoAsync()
    {
        MySqlDataReader reader = null;
        try
        {
            var sqlQuery = "SELECT * FROM Users WHERE Id = @Id";
            reader = await Bot.Database.ExecuteQueryAsync(sqlQuery,
                new MySqlParameter("Id", Id));
            if (await reader.ReadAsync())
            {
                return new User(
                    reader.GetUInt64("Id"),
                    reader.GetString("Username"),
                    reader.GetString("AvatarUrl"));
            }
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
        finally
        {
            await reader.CloseAsync();
        }

        return null;
    }

    public async Task DeleteAsync()
    {
        try
        {
            Bot.Logger.LogProcess($"Deleting the user: {Username} ({Id}) from the database...");
            
            var sqlQuery = "DELETE FROM Users WHERE Id = @Id";
            await Bot.Database.ExecuteNonQueryAsync(sqlQuery, 
                new MySqlParameter("Id", Id));
            
            Bot.Logger.LogSuccess("The user has been successfully deleted from the database.");
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
    }
}