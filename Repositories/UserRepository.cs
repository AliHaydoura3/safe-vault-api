using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

public class UserRepository
{
    private readonly string _connectionString = "database";

    public IdentityUser GetUserByCredentials(string username, string passwordHash)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "SELECT Id, UserName, Email FROM AspNetUsers WHERE UserName = @Username AND PasswordHash = @PasswordHash",
            connection
        );

        command.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = passwordHash;

        connection.Open();
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new IdentityUser
            {
                Id = reader.GetString(0),
                UserName = reader.GetString(1),
                Email = reader.GetString(2),
            };
        }

        return null;
    }
}
