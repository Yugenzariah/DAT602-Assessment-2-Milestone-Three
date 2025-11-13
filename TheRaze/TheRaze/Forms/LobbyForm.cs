using System;
using System.Data;
using System.Windows.Forms;
using TheRaze.Data;

namespace TheRaze.Forms
{
    public partial class LobbyForm : Form
    {
        private readonly LobbyDao _lobby = new LobbyDao();

        public LobbyForm()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                // Set welcome message with player info
                if (Session.PlayerId.HasValue)
                {
                    int highScore = GetPlayerHighScore();
                    lblWelcome.Text = $"Welcome, {Session.Username}! (High Score: {highScore})";
                }
                else
                {
                    lblWelcome.Text = "Welcome, Guest!";
                }

                // Load games list
                LoadGames();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing lobby: {ex.Message}", "Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGames()
        {
            try
            {
                lblStatus.Text = "Loading games...";
                lblStatus.ForeColor = System.Drawing.Color.Blue;

                var gamesTable = _lobby.GetGamesList();

                // Check if error occurred
                if (gamesTable.Columns.Contains("Error"))
                {
                    lblStatus.Text = "Error loading games";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show("Failed to load games list. Please try refreshing.",
                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgvGames.DataSource = gamesTable;

                // Format the DataGridView
                if (dgvGames.Columns.Count > 0)
                {
                    if (dgvGames.Columns.Contains("GameID"))
                        dgvGames.Columns["GameID"].Width = 80;
                    if (dgvGames.Columns.Contains("Name"))
                        dgvGames.Columns["Name"].Width = 200;
                    if (dgvGames.Columns.Contains("Status"))
                        dgvGames.Columns["Status"].Width = 100;
                    if (dgvGames.Columns.Contains("StartedAt"))
                        dgvGames.Columns["StartedAt"].Width = 120;
                    if (dgvGames.Columns.Contains("PlayerCount"))
                    {
                        dgvGames.Columns["PlayerCount"].HeaderText = "Players";
                        dgvGames.Columns["PlayerCount"].Width = 80;
                    }
                }

                lblStatus.Text = $"Loaded {gamesTable.Rows.Count} game(s)";
                lblStatus.ForeColor = System.Drawing.Color.Green;
            }
            catch (InvalidOperationException ex)
            {
                lblStatus.Text = "Connection error";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Database connection error: {ex.Message}\n\nPlease ensure MySQL is running.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading games";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Error loading games: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnJoinGame_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation checks
                if (!Session.PlayerId.HasValue)
                {
                    MessageBox.Show("You must be logged in to join a game.", "Not Logged In",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dgvGames.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a game to join.", "No Game Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selectedRow = dgvGames.SelectedRows[0];

                // Validate row data
                if (selectedRow.Cells["GameID"].Value == null)
                {
                    MessageBox.Show("Invalid game selection. Please try again.", "Invalid Selection",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var gameId = Convert.ToUInt32(selectedRow.Cells["GameID"].Value);
                var gameName = selectedRow.Cells["Name"].Value?.ToString() ?? "Unknown Game";

                lblStatus.Text = "Joining game...";
                lblStatus.ForeColor = System.Drawing.Color.Blue;

                // Attempt to join game
                var (status, message, playerGameId) = _lobby.JoinGame(Session.PlayerId.Value, gameId);

                if (status == "SUCCESS" || status == "ALREADY_JOINED")
                {
                    // Update session with game information
                    Session.CurrentGameId = gameId;
                    Session.CurrentPlayerGameId = playerGameId;

                    MessageBox.Show(message, status == "SUCCESS" ? "Joined Game" : "Resume Game",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Navigate to game board
                    var gameBoard = new GameForm();
                    gameBoard.Show();
                    this.Hide();
                }
                else
                {
                    lblStatus.Text = "Failed to join game";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show(message, "Join Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (InvalidCastException ex)
            {
                lblStatus.Text = "Data error";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Error reading game data: {ex.Message}", "Data Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                lblStatus.Text = "Connection error";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Database connection error: {ex.Message}\n\nPlease ensure MySQL is running.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error joining game";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Error joining game: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreateGame_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation check
                if (!Session.PlayerId.HasValue)
                {
                    MessageBox.Show("You must be logged in to create a game.", "Not Logged In",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get game name from user
                string gameName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter game name:",
                    "Create New Game",
                    $"{Session.Username}'s Game");

                if (string.IsNullOrWhiteSpace(gameName))
                {
                    return; // User cancelled
                }

                lblStatus.Text = "Creating game...";
                lblStatus.ForeColor = System.Drawing.Color.Blue;

                // Attempt to create game
                var (status, message, gameId) = _lobby.CreateGame(gameName);

                if (status == "SUCCESS" && gameId.HasValue)
                {
                    lblStatus.Text = "Game created successfully!";
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                    MessageBox.Show($"{message}\n\nGame ID: {gameId}", "Game Created",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGames(); // Refresh the games list
                }
                else
                {
                    lblStatus.Text = "Failed to create game";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show(message, "Creation Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                lblStatus.Text = "Connection error";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Database connection error: {ex.Message}\n\nPlease ensure MySQL is running.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error creating game";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Error creating game: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                LoadGames();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing games list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to logout?",
                    "Confirm Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Clear session data
                    Session.Clear();

                    // Return to login
                    var loginForm = new LoginForm();
                    loginForm.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetPlayerHighScore()
        {
            try
            {
                var auth = new AuthDao();
                var playerInfo = auth.GetPlayerInfo(Session.Username);
                return playerInfo?.highscore ?? 0;
            }
            catch (Exception ex)
            {
                // Log error but return 0 instead of showing message
                System.Diagnostics.Debug.WriteLine($"Error getting high score: {ex.Message}");
                return 0;
            }
        }
    }
}