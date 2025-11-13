using System;
using System.Drawing;
using System.Windows.Forms;
using TheRaze.Data;
using MySql.Data.MySqlClient;

namespace TheRaze.Forms
{
    public partial class GameForm : Form
    {
        private readonly GameDao _game = new GameDao();
        private readonly AuthDao _auth = new AuthDao();
        private const int BOARD_SIZE = 5;
        private const int TILE_SIZE = 120;
        private Button[,] tileButtons = new Button[BOARD_SIZE, BOARD_SIZE];
        private System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer npcTimer = new System.Windows.Forms.Timer();

        public GameForm()
        {
            InitializeComponent();
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            // Hide admin button if not admin
            if (btnAdmin != null)
            {
                btnAdmin.Visible = Session.IsAdmin;
            }

            // Check if player is in a game
            if (!Session.CurrentPlayerGameId.HasValue || !Session.CurrentGameId.HasValue)
            {
                MessageBox.Show("You are not in a game. Please join a game from the lobby.", "Not In Game");
                var lobby = new LobbyForm();
                lobby.Show();
                this.Close();
                return;
            }

            InitializeGameBoard();

            // Auto-refresh every 2 seconds
            refreshTimer.Interval = 2000;
            refreshTimer.Tick += (s, ev) => RefreshGameState();
            refreshTimer.Start();

            npcTimer.Interval = 15000;  // Move items every 15 seconds
            npcTimer.Tick += (s, ev) => MoveNPCItems();
            npcTimer.Start();

            RefreshGameState();
        }

        private void InitializeGameBoard()
        {
            pnlGameBoard.Controls.Clear();

            // Create 5x5 grid of tile buttons
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    var btn = new Button
                    {
                        Size = new Size(TILE_SIZE - 5, TILE_SIZE - 5),
                        Location = new Point(x * TILE_SIZE, y * TILE_SIZE),
                        Text = $"({x},{y})",
                        Tag = new Point(x, y),
                        BackColor = (x == 0 && y == 0) ? Color.LightGreen : Color.LightGray,
                        Font = new Font("Arial", 10, FontStyle.Bold)
                    };

                    btn.Click += TileButton_Click;
                    tileButtons[x, y] = btn;
                    pnlGameBoard.Controls.Add(btn);
                }
            }
        }

        private void TileButton_Click(object sender, EventArgs e)
        {
            if (!Session.CurrentPlayerGameId.HasValue)
            {
                MessageBox.Show("You are not in a game.", "Error");
                return;
            }

            var btn = (Button)sender;
            var targetPos = (Point)btn.Tag;

            // Get target tile ID from database
            var targetTileId = GetTileIdAt(targetPos.X, targetPos.Y);

            if (!targetTileId.HasValue)
            {
                MessageBox.Show("Invalid tile.", "Error");
                return;
            }

            // Attempt to move
            var (status, message) = _game.MovePlayer(Session.CurrentPlayerGameId.Value, targetTileId.Value);

            lblStatus.Text = message;
            lblStatus.ForeColor = status == "SUCCESS" ? Color.Green : Color.Red;

            if (status == "SUCCESS")
            {
                RefreshGameState();
            }
            else
            {
                MessageBox.Show(message, "Move Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshGameState()
        {
            try
            {
                if (!Session.CurrentPlayerGameId.HasValue) return;

                // Get player game state
                using var cn = Db.GetOpenConnection();

                // Get player position and stats
                using (var cmd = new MySqlCommand(
                    @"SELECT pg.Score, pg.HP, pg.IsTurn, t.X, t.Y, t.TileType, p.Username
                      FROM PlayerGame pg
                      JOIN Player p ON p.PlayerID = pg.PlayerID
                      LEFT JOIN Tile t ON t.TileID = pg.CurrentTileID
                      WHERE pg.PlayerGameID = @pgId", cn))
                {
                    cmd.Parameters.AddWithValue("@pgId", Session.CurrentPlayerGameId.Value);
                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        lblPlayerName.Text = $"Player: {reader["Username"]}";
                        lblScore.Text = $"Score: {reader["Score"]}";
                        lblHP.Text = $"HP: {reader["HP"]}";

                        if (reader["X"] != DBNull.Value)
                        {
                            int x = Convert.ToInt32(reader["X"]);
                            int y = Convert.ToInt32(reader["Y"]);
                            lblPosition.Text = $"Position: ({x}, {y})";
                            lblTileInfo.Text = $"Tile Type: {reader["TileType"]}";

                            // Highlight player position
                            UpdateTileColors(x, y);
                        }

                        chkIsYourTurn.Checked = Convert.ToBoolean(reader["IsTurn"]);
                    }
                }

                // Get items on current tile
                RefreshTileItems();

                // Get inventory
                RefreshInventory();

                // Get other players' positions
                ShowOtherPlayers();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void UpdateTileColors(int playerX, int playerY)
        {
            // Reset all tiles
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (x == 0 && y == 0)
                        tileButtons[x, y].BackColor = Color.LightGreen; // Home
                    else
                        tileButtons[x, y].BackColor = Color.LightGray;

                    tileButtons[x, y].Text = $"({x},{y})";
                }
            }

            // Highlight player position
            if (playerX < BOARD_SIZE && playerY < BOARD_SIZE)
            {
                tileButtons[playerX, playerY].BackColor = Color.Yellow;
                tileButtons[playerX, playerY].Text = $"({playerX},{playerY})\nYOU";
            }
        }

        private void ShowOtherPlayers()
        {
            if (!Session.CurrentGameId.HasValue) return;

            try
            {
                using var cn = Db.GetOpenConnection();
                using var cmd = new MySqlCommand(
                    @"SELECT p.Username, t.X, t.Y
                      FROM PlayerGame pg
                      JOIN Player p ON p.PlayerID = pg.PlayerID
                      LEFT JOIN Tile t ON t.TileID = pg.CurrentTileID
                      WHERE pg.GameID = @gameId 
                      AND pg.PlayerGameID != @currentPGId
                      AND t.X IS NOT NULL", cn);

                cmd.Parameters.AddWithValue("@gameId", Session.CurrentGameId.Value);
                cmd.Parameters.AddWithValue("@currentPGId", Session.CurrentPlayerGameId.Value);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int x = Convert.ToInt32(reader["X"]);
                    int y = Convert.ToInt32(reader["Y"]);
                    string username = reader["Username"].ToString();

                    if (x < BOARD_SIZE && y < BOARD_SIZE)
                    {
                        // Show other players in red
                        if (tileButtons[x, y].BackColor != Color.Yellow) // Don't override current player
                        {
                            tileButtons[x, y].BackColor = Color.LightCoral;
                            tileButtons[x, y].Text = $"({x},{y})\n{username}";
                        }
                    }
                }
            }
            catch { }
        }

        private void RefreshTileItems()
        {
            lstTileItems.Items.Clear();

            if (!Session.CurrentPlayerGameId.HasValue) return;

            try
            {
                using var cn = Db.GetOpenConnection();
                using var cmd = new MySqlCommand(
                    @"SELECT i.ItemID, i.Name, ti.Quantity, i.Points
                      FROM PlayerGame pg
                      JOIN TileItem ti ON ti.TileID = pg.CurrentTileID
                      JOIN Item i ON i.ItemID = ti.ItemID
                      WHERE pg.PlayerGameID = @pgId", cn);

                cmd.Parameters.AddWithValue("@pgId", Session.CurrentPlayerGameId.Value);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lstTileItems.Items.Add(new ItemDisplay
                    {
                        ItemID = Convert.ToUInt32(reader["ItemID"]),
                        Name = reader["Name"].ToString(),
                        Quantity = Convert.ToInt16(reader["Quantity"]),
                        Points = Convert.ToInt16(reader["Points"])
                    });
                }
            }
            catch { }

            lstTileItems.DisplayMember = "DisplayText";
        }

        private void RefreshInventory()
        {
            lstInventory.Items.Clear();

            if (!Session.CurrentPlayerGameId.HasValue) return;

            try
            {
                using var cn = Db.GetOpenConnection();
                using var cmd = new MySqlCommand(
                    @"SELECT i.Name, inv.Quantity
                      FROM Inventory inv
                      JOIN Item i ON i.ItemID = inv.ItemID
                      WHERE inv.PlayerGameID = @pgId", cn);

                cmd.Parameters.AddWithValue("@pgId", Session.CurrentPlayerGameId.Value);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lstInventory.Items.Add($"{reader["Name"]} x{reader["Quantity"]}");
                }
            }
            catch { }
        }

        private uint? GetTileIdAt(int x, int y)
        {
            if (!Session.CurrentGameId.HasValue) return null;

            try
            {
                using var cn = Db.GetOpenConnection();
                using var cmd = new MySqlCommand(
                    "SELECT TileID FROM Tile WHERE GameID = @gameId AND X = @x AND Y = @y", cn);

                cmd.Parameters.AddWithValue("@gameId", Session.CurrentGameId.Value);
                cmd.Parameters.AddWithValue("@x", x);
                cmd.Parameters.AddWithValue("@y", y);

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToUInt32(result) : null;
            }
            catch
            {
                return null;
            }
        }

        private void btnPickupItem_Click(object sender, EventArgs e)
        {
            if (lstTileItems.SelectedItem == null)
            {
                MessageBox.Show("Select an item to pick up.", "No Item Selected");
                return;
            }

            var item = (ItemDisplay)lstTileItems.SelectedItem;

            var (status, message) = _game.PickupItem(
                Session.CurrentPlayerGameId.Value,
                item.ItemID,
                1); // Pick up 1 at a time

            lblStatus.Text = message;
            lblStatus.ForeColor = status == "SUCCESS" ? Color.Green : Color.Red;

            if (status == "SUCCESS")
            {
                MessageBox.Show(message, "Item Picked Up", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshGameState();
            }
            else
            {
                MessageBox.Show(message, "Pickup Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshGameState();
        }

        private void btnLeaveGame_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to leave the game?",
                "Confirm Leave",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                refreshTimer.Stop();

                var lobby = new LobbyForm();
                lobby.Show();
                this.Close();
            }
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Session.PlayerId.HasValue)
                {
                    MessageBox.Show("You must be logged in.", "Not Logged In");
                    return;
                }

                // Confirm deletion
                var result = MessageBox.Show(
                    "Are you sure you want to delete your account? This action cannot be undone.",
                    "Confirm Account Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var (status, message) = _auth.DeleteOwnAccount(Session.PlayerId.Value);

                    if (status == "SUCCESS")
                    {
                        MessageBox.Show("Your account has been deleted.", "Account Deleted",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Stop timer
                        refreshTimer.Stop();

                        // Clear session
                        Session.Clear();

                        // Return to login
                        var loginForm = new LoginForm();
                        loginForm.Show();

                        // Close this form
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(message, "Deletion Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting account: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            // Check if user is an admin
            if (!Session.IsAdmin)
            {
                MessageBox.Show(
                    "Access Denied: You do not have administrator privileges.",
                    "Unauthorized",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // If admin, open admin form
            var adminForm = new AdminForm();
            adminForm.ShowDialog(this);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer.Stop();
            base.OnFormClosing(e);
        }

        // Helper class for displaying items
        private class ItemDisplay
        {
            public uint ItemID { get; set; }
            public string Name { get; set; }
            public short Quantity { get; set; }
            public short Points { get; set; }

            public string DisplayText => $"{Name} x{Quantity} ({Points} pts)";
        }

        private void MoveNPCItems()
        {
            if (!Session.CurrentGameId.HasValue) return;

            try
            {
                using var cn = Db.GetOpenConnection();

                // Get all items on the board with their positions
                using var cmd = new MySqlCommand(
                    @"SELECT ti.TileItemID, ti.TileID, ti.ItemID, t.X, t.Y, t.GameID
              FROM TileItem ti
              JOIN Tile t ON t.TileID = ti.TileID
              WHERE t.GameID = @gameId
              ORDER BY RAND()
              LIMIT 3", cn);  // Move up to 3 random items each cycle

                cmd.Parameters.AddWithValue("@gameId", Session.CurrentGameId.Value);

                var itemsToMove = new System.Collections.Generic.List<(uint tileItemId, int x, int y)>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemsToMove.Add((
                            Convert.ToUInt32(reader["TileItemID"]),
                            Convert.ToInt32(reader["X"]),
                            Convert.ToInt32(reader["Y"])
                        ));
                    }
                }

                // Move each selected item to a random adjacent tile
                foreach (var item in itemsToMove)
                {
                    MoveItemToRandomAdjacentTile(item.tileItemId, item.x, item.y);
                }

                // Refresh board after NPC moves
                if (itemsToMove.Count > 0)
                {
                    RefreshGameState();
                    lblStatus.Text = $"NPC moved {itemsToMove.Count} item(s)";
                    lblStatus.ForeColor = Color.Purple;
                }
            }
            catch (Exception ex)
            {
                // Silently fail - NPC movement is not critical
                System.Diagnostics.Debug.WriteLine($"NPC movement error: {ex.Message}");
            }
        }

        private void MoveItemToRandomAdjacentTile(uint tileItemId, int currentX, int currentY)
        {
            try
            {
                // Get possible adjacent tiles (up, down, left, right)
                var directions = new[]
                {
            (currentX, currentY - 1),  // Up
            (currentX, currentY + 1),  // Down
            (currentX - 1, currentY),  // Left
            (currentX + 1, currentY)   // Right
        };

                // Filter valid tiles (within board boundaries)
                var validDirections = directions
                    .Where(d => d.Item1 >= 0 && d.Item1 < BOARD_SIZE && d.Item2 >= 0 && d.Item2 < BOARD_SIZE)
                    .ToList();

                if (validDirections.Count == 0) return;

                // Pick random direction
                var random = new Random();
                var (newX, newY) = validDirections[random.Next(validDirections.Count)];

                // Get target tile ID
                var targetTileId = GetTileIdAt(newX, newY);
                if (!targetTileId.HasValue) return;

                // Move the item using existing DAO method
                var (status, message) = _game.MoveTileItem(tileItemId, targetTileId.Value);

                // for debugging
                if (status == "SUCCESS")
                {
                    System.Diagnostics.Debug.WriteLine($"NPC moved item from ({currentX},{currentY}) to ({newX},{newY})");
                }
            }
            catch
            {
                // Silently fail - not critical
            }
        }
    }
}