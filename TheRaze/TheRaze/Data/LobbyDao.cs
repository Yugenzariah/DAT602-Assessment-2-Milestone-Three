using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace TheRaze.Data
{
    public class LobbyDao
    {
        public DataTable GetGamesList()
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("sp_get_games_list", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public (string status, uint? playerGameId) JoinGame(uint playerId, uint gameId)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("sp_join_game", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_playerId", playerId);
            cmd.Parameters.AddWithValue("@p_gameId", gameId);

            var statusParam = new MySqlParameter("@p_status", MySqlDbType.VarChar, 50)
            { Direction = ParameterDirection.Output };
            var pgIdParam = new MySqlParameter("@p_playerGameId", MySqlDbType.UInt32)
            { Direction = ParameterDirection.Output };

            cmd.Parameters.Add(statusParam);
            cmd.Parameters.Add(pgIdParam);

            cmd.ExecuteNonQuery();

            string status = statusParam.Value?.ToString() ?? "ERROR: Unknown";
            uint? playerGameId = pgIdParam.Value != DBNull.Value
                ? Convert.ToUInt32(pgIdParam.Value)
                : null;

            return (status, playerGameId);
        }

        public uint CreateGame(string gameName)
        {
            using var cn = Db.GetOpenConnection();
            using var cmd = new MySqlCommand("sp_create_game", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_gameName", gameName);

            var gameIdParam = new MySqlParameter("@p_gameId", MySqlDbType.UInt32)
            { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(gameIdParam);

            cmd.ExecuteNonQuery();

            return Convert.ToUInt32(gameIdParam.Value);
        }
    }
}
