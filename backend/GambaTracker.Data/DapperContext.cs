using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace GambaTracker.Data;

public class DapperContext
{
    private readonly IConfiguration _config;

    public DapperContext(IConfiguration config)
    {
        _config = config;
    }

    public IDbConnection CreateConnection() =>
        new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
}