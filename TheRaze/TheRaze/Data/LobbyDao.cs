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
            try
            {
                using var cn = Db.GetOpenConnection();
                using var cmd = new MySqlCommand("store_procedure_get_games_list", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                var adapter = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (MySqlException ex)
            {
                // Database-specific errors - return empty DataTable with error info
                System.Diagnostics.Debug.WriteLine($"Database error in GetGamesList: {ex.Message}");
                var errorTable = new DataTable();
                errorTable.Columns.Add("Error", typeof(string));
                errorTable.Rows.Add($"Database error: {ex.Message}");
                return errorTable;
            }
            catch (InvalidOperationException ex)
            {
                // Connection errors - return empty DataTable
                System.Diagnostics.Debug.WriteLine($"Connection error in GetGamesList: {ex.Message}");
                var errorTable = new DataTable();
                errorTable.Columns.Add("Error", typeof(string));
                errorTable.Rows.Add($"Connection error: {ex.Message}");
                return errorTable;
            }
            catch (Exception ex)
            {
                // Any other unexpected errors
                System.Diagnostics.Debug.WriteLine($"Unexpected error in GetGamesList: {ex.Message}");
                var errorTable = new DataTable();
                errorTable.Columns.Add("Error", typeof(string));
                errorTable.Rows.Add($"Unexpected error: {ex.Message}");
                return errorTable;
            }
        }

        /// <summary>
        /// Player joins a game or resumes if already joined.
        /// </summary>
        public (string status, string message, uint? playerGameId) JoinGame(uint playerId, uint gameId)
        {
            try
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
            catch (MySqlException ex)
            {
                // Database-specific errors (connection, constraint violations, etc.)
                return ("ERROR", $"Database error: {ex.Message}", null);
            }
            catch (InvalidOperationException ex)
            {
                // Connection or command execution errors
                return ("ERROR", $"Connection error: {ex.Message}", null);
            }
            catch (InvalidCastException ex)
            {
                // Data type conversion errors
                return ("ERROR", $"Data conversion error: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                // Any other unexpected errors
                return ("ERROR", $"Unexpected error joining game: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Creates a new game with a board.
        /// </summary>
        public (string status, string message, uint? gameId) CreateGame(string gameName)
        {
            try
            {
                // Client-side validation
                if (string.IsNullOrWhiteSpace(gameName))
                {
                    return ("ERROR", "Game name cannot be empty", null);
                }

                if (gameName.Length > 60)
                {
                    return ("ERROR", "Game name must be 60 characters or less", null);
                }

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
            catch (MySqlException ex)
            {
                // Database-specific errors (connection, unique constraint violations)
                return ("ERROR", $"Database error: {ex.Message}", null);
            }
            catch (InvalidOperationException ex)
            {
                // Connection or command execution errors
                return ("ERROR", $"Connection error: {ex.Message}", null);
            }
            catch (InvalidCastException ex)
            {
                // Data type conversion errors
                return ("ERROR", $"Data conversion error: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                // Any other unexpected errors
                return ("ERROR", $"Unexpected error creating game: {ex.Message}", null);
            }
        }
    }
}