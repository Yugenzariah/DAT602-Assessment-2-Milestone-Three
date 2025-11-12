using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace TheRaze.Data
{
    public class AdminDao
    {
        /// <summary>
        /// Kills a game and removes all player records from it.
        /// </summary>
        public (string status, string message) KillGame(uint gameId)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_kill_game", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_gameId", gameId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }

        /// <summary>
        /// Admin adds a new player. Checks for duplicate username/email.
        /// </summary>
        public (string status, string message, uint? playerId) AddPlayer(string username, string email, string passwordHash, bool isAdmin)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_admin_add_player", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_username", username);
            cmd.Parameters.AddWithValue("@p_email", email);
            cmd.Parameters.AddWithValue("@p_passwordhash", passwordHash);
            cmd.Parameters.AddWithValue("@p_isAdmin", isAdmin ? 1 : 0);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string status = reader["Status"].ToString();
                string message = reader["Message"].ToString();
                uint? playerId = null;

                // PlayerID only exists when status is SUCCESS
                if (status == "SUCCESS" && reader["PlayerID"] != DBNull.Value)
                {
                    playerId = Convert.ToUInt32(reader["PlayerID"]);
                }

                return (status, message, playerId);
            }

            return ("ERROR", "No response from database", null);
        }

        /// <summary>
        /// Admin updates player information.
        /// </summary>
        public (string status, string message) UpdatePlayer(uint playerId, string username, string email, string passwordHash, bool isAdmin, bool isLocked)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_admin_update_player", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_playerId", playerId);
            cmd.Parameters.AddWithValue("@p_username", username);
            cmd.Parameters.AddWithValue("@p_email", email);
            cmd.Parameters.AddWithValue("@p_passwordhash", passwordHash);
            cmd.Parameters.AddWithValue("@p_isAdmin", isAdmin ? 1 : 0);
            cmd.Parameters.AddWithValue("@p_isLocked", isLocked ? 1 : 0);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }

        /// <summary>
        /// Admin deletes a player. Checks if player exists first.
        /// </summary>
        public (string status, string message) DeletePlayer(uint playerId)
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