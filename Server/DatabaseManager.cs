using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;

namespace Server
{

    class DatabaseManager
    {
        private string _connStr = "Server=localhost;Database=game_db;Uid=root;Pwd=비밀번호；Max Pool Size=10;";

        public async Task<(int success, string nickname, int winCount)> VerifyLogin(string userId, string password)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    await conn.OpenAsync();
                    string sql = "SELECT password_hash, nickname, wincount FROM accounts WHERE user_id = @uid";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string Password = reader["password_hash"].ToString();
                                string nickname = reader["nickname"].ToString();
                                int wincount = Convert.ToInt32(reader["wincount"]);

                                if (password == Password)
                                    return (1, nickname, wincount);
                                else
                                    return (3, null, 0);
                            }
                            else
                            {
                                return (2, null, 0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Error] {ex.Message}");
            }
            return (0, null, 0);

        }

        public async Task<(bool s, int i)> UpdatePlayerWinCount(string userId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE accounts SET wincount = wincount + 1 WHERE user_id = @uid";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0) return (false, 0);
                    }

                    string selectSql = "SELECT wincount FROM accounts WHERE user_id = @uid";
                    using (var selectCmd = new MySqlCommand(selectSql, conn))
                    {
                        selectCmd.Parameters.AddWithValue("@uid", userId);
                        object result = await selectCmd.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out int currentWinCount))
                        {
                            return (true, currentWinCount);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[DB Error - UpdateWin] {ex.Message}");
                return (false, 0);
            }

            return (false, 0);
        }

        public async Task<(bool s, int i)> GetPlayerWinCount(string userId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    await conn.OpenAsync();

                    string selectSql = "SELECT wincount FROM accounts WHERE user_id = @uid";
                    using (var selectCmd = new MySqlCommand(selectSql, conn))
                    {
                        selectCmd.Parameters.AddWithValue("@uid", userId);
                        object result = await selectCmd.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out int currentWinCount))
                        {
                            return (true, currentWinCount);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Error - UpdateWin] {ex.Message}");
                return (false, 0);
            }

            return (false, 0);
        }
        public async Task<(bool success, string message)> RegisterRequest(string userId, string password, string nickname)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    await conn.OpenAsync();
                    string sql = "SELECT user_id, nickname FROM accounts WHERE user_id = @uid OR nickname = @nick" ;
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        cmd.Parameters.AddWithValue("@nick", nickname);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (reader["user_id"].ToString() == userId)
                                    return (false, "이미 존재하는 아이디입니다.");
                                if (reader["nickname"].ToString() == nickname)
                                    return (false, "이미 존재하는 닉네임입니다.");
                            }
                        }
                    }
                    string insertSql = "INSERT INTO accounts (user_id, password_hash, nickname) VALUES (@uid, @pw, @nick)";
                    using (var insertCmd = new MySqlCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@uid",userId);
                        insertCmd.Parameters.AddWithValue("@pw", password);
                        insertCmd.Parameters.AddWithValue("@nick", nickname);
                        await insertCmd.ExecuteNonQueryAsync();
                        return (true, "회원가입 성공");
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Error] {ex.Message}");
            }
            return (false, null);

        }

        public bool TestConnection()
        {
            try
            {
                using ( var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
