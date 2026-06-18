using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _configuration;

        public UserRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("INSERT INTO Users (Id, Name, Email, PasswordHash, GoogleId) VALUES (@Id, @Name, @Email, @PasswordHash, @GoogleId)", user);
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var conn = _configuration.CreateConnection();
            var users = await conn.QueryAsync<User>("SELECT * FROM Users");
            return users;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            var user = await conn.QuerySingleAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Users SET Name = @Name, Email = @Email, PasswordHash = @PasswordHash, GoogleId = @GoogleId WHERE Id = @Id", user);
        }
    }
}