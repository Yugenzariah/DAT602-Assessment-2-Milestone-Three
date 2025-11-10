using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheRaze.Data
{
    public class AuthDao
    {
        public string TryLogin(string username, string passwordHash)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("sp_login_player", cn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_username", username);
            cmd.Parameters.AddWithValue("@p_passwordhash", passwordHash);

            var outParam = new MySqlParameter("@p_status", MySqlDbType.VarChar, 20)
            { Direction = System.Data.ParameterDirection.Output };
            cmd.Parameters.Add(outParam);

            cmd.ExecuteNonQuery();
            return (string)outParam.Value; // "OK" | "LOCKED" | "BADPASS" | "UNKNOWN"
        }

        public void Register(string username, string email, string passwordHash)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("sp_register_player", cn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@p_username", username);
            cmd.Parameters.AddWithValue("@p_email", email);
            cmd.Parameters.AddWithValue("@p_passwordhash", passwordHash);
            cmd.ExecuteNonQuery();
        }

        public void DeleteOwnAccount(uint playerId)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("sp_admin_delete_player", cn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@p_playerId", playerId);
            cmd.ExecuteNonQuery();
        }

        public (uint playerId, string username, bool isAdmin, int highscore)? GetPlayerInfo(string username)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("sp_get_player_info", cn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    Convert.ToUInt32(reader["PlayerID"]),
                    reader["Username"].ToString(),
                    Convert.ToBoolean(reader["IsAdmin"]),
                    Convert.ToInt32(reader["Highscore"])
                );
            }
            return null;
        }
    }
}
