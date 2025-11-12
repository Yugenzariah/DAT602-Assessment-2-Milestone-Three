using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace TheRaze.Data
{
    public class GameDao
    {
        /// <summary>
        /// Moves a player to an adjacent tile.
        /// </summary>
        public (string status, string message) MovePlayer(uint playerGameId, uint targetTileId)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_move_player", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_playerGameId", playerGameId);
            cmd.Parameters.AddWithValue("@p_targetTileId", targetTileId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }

        /// <summary>
        /// Updates a player's score by delta amount.
        /// </summary>
        public (string status, string message, int? newScore) AddScore(uint playerGameId, int delta)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_update_score", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_playerGameId", playerGameId);
            cmd.Parameters.AddWithValue("@p_delta", delta);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string status = reader["Status"].ToString();
                string message = reader["Message"].ToString();
                int? newScore = null;

                // NewScore only exists when status is SUCCESS
                if (status == "SUCCESS" && reader["NewScore"] != DBNull.Value)
                {
                    newScore = Convert.ToInt32(reader["NewScore"]);
                }

                return (status, message, newScore);
            }

            return ("ERROR", "No response from database", null);
        }

        /// <summary>
        /// Resets a game board to specified dimensions.
        /// </summary>
        public (string status, string message) ResetBoard(uint gameId, int width, int height)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_reset_board", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_gameId", gameId);
            cmd.Parameters.AddWithValue("@p_width", width);
            cmd.Parameters.AddWithValue("@p_height", height);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }

        /// <summary>
        /// Places an item on a tile. Validates tile and item exist.
        /// </summary>
        public (string status, string message) PlaceItemOnTile(uint tileId, uint itemId, short quantity)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_place_item_on_tile", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_tileId", tileId);
            cmd.Parameters.AddWithValue("@p_itemId", itemId);
            cmd.Parameters.AddWithValue("@p_qty", quantity);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }

        /// <summary>
        /// Player picks up an item from their current tile.
        /// </summary>
        public (string status, string message) PickupItem(uint playerGameId, uint itemId, short quantity)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_pickup_item", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_playerGameId", playerGameId);
            cmd.Parameters.AddWithValue("@p_itemId", itemId);
            cmd.Parameters.AddWithValue("@p_qty", quantity);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }

        /// <summary>
        /// Moves an item from one tile to an adjacent tile (NPC movement).
        /// </summary>
        public (string status, string message) MoveTileItem(uint tileItemId, uint targetTileId)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("store_procedure_move_tile_item", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_tileItemId", tileItemId);
            cmd.Parameters.AddWithValue("@p_targetTileId", targetTileId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader["Status"].ToString(), reader["Message"].ToString());
            }

            return ("ERROR", "No response from database");
        }
    }
}