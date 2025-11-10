using MySqlX.XDevAPI;
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

        private void LoadGames()
        {
            try
            {
                lblStatus.Text = "Loading games...";
                lblStatus.ForeColor = System.Drawing.Color.Blue;

                var gamesTable = _lobby.GetGamesList();
                dgvGames.DataSource = gamesTable;

                // Format the DataGridView
                if (dgvGames.Columns.Count > 0)
                {
                    dgvGames.Columns["GameID"].Width = 80;
                    dgvGames.Columns["Name"].Width = 200;
                    dgvGames.Columns["Status"].Width = 100;
                    dgvGames.Columns["StartedAt"].Width = 120;
                    dgvGames.Columns["PlayerCount"].HeaderText = "Players";
                    dgvGames.Columns["PlayerCount"].Width = 80;
                }

                lblStatus.Text = $"Loaded {gamesTable.Rows.Count} game(s)";
                lblStatus.ForeColor = System.Drawing.Color.Green;
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
                if (!Session.PlayerId.HasValue)
                {
                    MessageBox.Show("You must be logged in to join a game.", "Not Logged In");
                    return;
                }

                if (dgvGames.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a game to join.", "No Game Selected");
                    return;
                }

                var selectedRow = dgvGames.SelectedRows[0];
                var gameId = Convert.ToUInt32(selectedRow.Cells["GameID"].Value);
                var gameName = selectedRow.Cells["Name"].Value.ToString();
                var gameStatus = selectedRow.Cells["Status"].Value.ToString();

                // Check if game is joinable
                if (gameStatus == "Finished" || gameStatus == "Killed")
                {
                    MessageBox.Show($"Cannot join game '{gameName}' - it has ended.", "Game Ended");
                    return;
                }

                lblStatus.Text = "Joining game...";
                lblStatus.ForeColor = System.Drawing.Color.Blue;

                var (status, playerGameId) = _lobby.JoinGame(Session.PlayerId.Value, gameId);

                if (status == "SUCCESS")
                {
                    Session.CurrentGameId = gameId;
                    Session.CurrentPlayerGameId = playerGameId;

                    MessageBox.Show($"Successfully joined '{gameName}'!", "Joined Game",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Open game board
                    var gameBoard = new GameForm();
                    gameBoard.Show();
                    this.Hide();
                }
                else if (status == "ALREADY_JOINED")
                {
                    Session.CurrentGameId = gameId;
                    Session.CurrentPlayerGameId = playerGameId;

                    MessageBox.Show($"Resuming your game in '{gameName}'...", "Resume Game",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var gameBoard = new GameForm();
                    gameBoard.Show();
                    this.Hide();
                }
                else
                {
                    lblStatus.Text = "Failed to join game";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show(status, "Join Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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
                if (!Session.PlayerId.HasValue)
                {
                    MessageBox.Show("You must be logged in to create a game.", "Not Logged In");
                    return;
                }

                // Prompt for game name
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

                uint gameId = _lobby.CreateGame(gameName);

                lblStatus.Text = "Game created successfully!";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                MessageBox.Show($"Game '{gameName}' created successfully!", "Game Created",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh the games list
                LoadGames();
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
            LoadGames();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Session.Clear();

                var loginForm = new LoginForm();
                loginForm.Show();
                this.Close();
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
            catch
            {
                return 0;
            }
        }
    }
}