using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace TheRaze.Data
{
    public class LobbyDao
    {
        /// <summary>
        /// Gets list of all active games (read-only query).
        /// </summary>
        public DataTable GetGamesList()
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_get_games_list", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Player joins a game or resumes if already joined.
        /// </summary>
        public (string status, string message, uint? playerGameId) JoinGame(uint playerId, uint gameId)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_join_game", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_playerId", playerId);
            cmd.Parameters.AddWithValue("@p_gameId", gameId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string status = reader["Status"].ToString();
                string message = reader["Message"].ToString();
                uint? playerGameId = null;

                // PlayerGameID exists for both SUCCESS and ALREADY_JOINED
                if (reader["PlayerGameID"] != DBNull.Value)
                {
                    playerGameId = Convert.ToUInt32(reader["PlayerGameID"]);
                }

                return (status, message, playerGameId);
            }

            return ("ERROR", "No response from database", null);
        }

        /// <summary>
        /// Creates a new game with a board.
        /// </summary>
        public (string status, string message, uint? gameId) CreateGame(string gameName)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_create_game", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_gameName", gameName);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string status = reader["Status"].ToString();
                string message = reader["Message"].ToString();
                uint? gameId = null;

                // GameID only exists when status is SUCCESS
                if (status == "SUCCESS" && reader["GameID"] != DBNull.Value)
                {
                    gameId = Convert.ToUInt32(reader["GameID"]);
                }

                return (status, message, gameId);
            }

            return ("ERROR", "No response from database", null);
        }
    }
}