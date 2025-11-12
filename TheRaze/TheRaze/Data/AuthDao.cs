using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace TheRaze.Data
{
    public class AuthDao
    {
        /// <summary>
        /// Attempts to login a player. Returns status, message, and playerID if successful.
        /// </summary>
        public (string status, string message, uint? playerId) TryLogin(string username, string passwordHash)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_login_player", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_username", username);
            cmd.Parameters.AddWithValue("@p_passwordhash", passwordHash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string status = reader["Status"].ToString();
                string message = reader["Message"].ToString();
                uint? playerId = null;

                if (status == "OK" && reader["PlayerID"] != DBNull.Value)
                {
                    playerId = Convert.ToUInt32(reader["PlayerID"]);
                }

                return (status, message, playerId);
            }

            return ("ERROR", "No response from database", null);
        }

        /// <summary>
        /// Registers a new player. Checks for duplicate username/email.
        /// </summary>
        public (string status, string message, uint? playerId) Register(string username, string email, string passwordHash)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_register_player", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_username", username);
            cmd.Parameters.AddWithValue("@p_email", email);
            cmd.Parameters.AddWithValue("@p_passwordhash", passwordHash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string status = reader["Status"].ToString();
                string message = reader["Message"].ToString();
                uint? playerId = null;

                if (status == "SUCCESS" && reader["PlayerID"] != DBNull.Value)
                {
                    playerId = Convert.ToUInt32(reader["PlayerID"]);
                }

                return (status, message, playerId);
            }

            return ("ERROR", "No response from database", null);
        }

        /// <summary>
        /// Gets player information by username (read-only query).
        /// </summary>
        public (uint playerId, string username, bool isAdmin, int highscore)? GetPlayerInfo(string username)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_get_player_info", cn);
            cmd.CommandType = CommandType.StoredProcedure;

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

        /// <summary>
        /// Allows a player to delete their own account.
        /// </summary>
        public (string status, string message) DeleteOwnAccount(uint playerId)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_admin_delete_player", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_playerId", playerId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }
    }
}