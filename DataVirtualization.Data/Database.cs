using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace DataVirtualization.Data
{
    using Models;

    public class Database
    {
        private readonly IDbConnection _connection = new SqliteConnection("Data Source=App.db");

        #region Get All Users

        public int GetUsersCount()
        {
            const string sql =
                """
                SELECT *
                FROM UserStats
                LIMIT 1;
                """;

            var result = _connection.ExecuteScalar<int>(sql);
            return result;
        }
        public IEnumerable<User> GetUserRange(int startIndex, int limit)
        {
            const string sql =
                """
                SELECT * 
                FROM user
                WHERE Id > @startIndex
                ORDER BY Id
                LIMIT @limit;
                """;

            return _connection.Query<User>(sql, new { startIndex, limit });
        }
        public IEnumerable<User> GetUserPrevious(int endIndex, int limit)
        {
            const string sql =
                """
                SELECT u.*
                FROM (SELECT *
                      FROM user
                      WHERE Id < @endIndex
                      ORDER BY Id DESC
                      LIMIT @limit) u
                ORDER BY u.Id ASC;
                """;

            return _connection.Query<User>(sql, new { endIndex, limit });
        }

        #endregion

        #region Search Users

        public int SearchUsersCount(string searchText)
        {
            const string sql =
                """
                SELECT COUNT(*) 
                FROM user 
                WHERE Name 
                LIKE @searchText;
                """;

            return _connection.ExecuteScalar<int>(sql, new { searchText = $"%{searchText}%" });
        }
        public IEnumerable<User> SearchUserRange(string searchText, int offset, int limit)
        {
            const string sql =
                """
                SELECT *
                FROM user
                WHERE Name LIKE @searchText
                ORDER BY Id
                LIMIT @limit
                OFFSET @offset;
                """;

            return _connection.Query<User>(sql, new { searchText = $"%{searchText}%", offset, limit });
        }
        public IEnumerable<User> SearchUserNext(string searchText, int startIndex, int limit)
        {
            const string sql =
                """
                SELECT *
                FROM user
                WHERE Id > @startIndex
                    AND Name LIKE @searchText
                ORDER BY Id
                LIMIT @limit;
                """;

            return _connection.Query<User>(sql, new { searchText = $"%{searchText}%", startIndex, limit });
        }
        public IEnumerable<User> SearchUserPrevious(string searchText, int endIndex, int limit)
        {
            const string sql =
                """
                SELECT *
                FROM (SELECT *
                      FROM user
                      WHERE Id < @endIndex
                        AND Name LIKE @searchText
                      ORDER BY Id DESC
                      LIMIT @limit) q
                ORDER BY q.Id ASC;
                """;

            return _connection.Query<User>(sql, new { searchText = $"%{searchText}%", endIndex, limit });
        }

        #endregion
    }
}
