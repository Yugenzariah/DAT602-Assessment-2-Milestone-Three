using System;
using System.Windows.Forms;
using TheRaze.Data;
using TheRaze.Utils;

namespace TheRaze.Forms
{
    public partial class AdminForm : Form
    {
        private readonly AdminDao _admin = new AdminDao();

        public AdminForm()
        {
            InitializeComponent();
        }

        private void btnKill_Click(object sender, EventArgs e)
        {
            try
            {
                // Input validation
                if (!uint.TryParse(txtGameId.Text, out var gameId))
                {
                    MessageBox.Show("Please enter a valid Game ID (positive number).", "Invalid Input",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGameId.Focus();
                    return;
                }

                // Confirm action
                var confirm = MessageBox.Show(
                    $"Are you sure you want to kill game {gameId}?\n\nThis will remove all players from the game.",
                    "Confirm Kill Game",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                // Execute kill game
                var (status, message) = _admin.KillGame(gameId);

                if (status == "SUCCESS")
                {
                    MessageBox.Show(message, "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtGameId.Clear();
                }
                else
                {
                    MessageBox.Show(message, "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kill game failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Get and validate input
                var u = txtU.Text.Trim();
                var email = txtE.Text.Trim();
                var p = txtP.Text;
                var isAdmin = chkAdmin.Checked;

                // Input validation
                if (string.IsNullOrWhiteSpace(u))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtU.Focus();
                    return;
                }

                if (u.Length < 3)
                {
                    MessageBox.Show("Username must be at least 3 characters.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtU.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Please enter an email address.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtE.Focus();
                    return;
                }

                if (!email.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtE.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(p))
                {
                    MessageBox.Show("Please enter a password.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtP.Focus();
                    return;
                }

                if (p.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtP.Focus();
                    return;
                }

                // Execute add player
                var (status, message, playerId) = _admin.AddPlayer(u, email, HashHelper.FakeHash(p), isAdmin);

                if (status == "SUCCESS")
                {
                    string idInfo = playerId.HasValue ? $"\nPlayer ID: {playerId}" : "";
                    MessageBox.Show($"{message}{idInfo}", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear form
                    txtU.Clear();
                    txtE.Clear();
                    txtP.Clear();
                    chkAdmin.Checked = false;
                }
                else
                {
                    MessageBox.Show(message, "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Add player failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate player ID
                if (!uint.TryParse(txtPlayerId.Text, out var playerId))
                {
                    MessageBox.Show("Please enter a valid Player ID (positive number).", "Invalid Input",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPlayerId.Focus();
                    return;
                }

                // Get and validate other inputs
                var u = txtU2.Text.Trim();
                var email = txtE2.Text.Trim();
                var p = txtP2.Text;
                var isAdmin = chkAdmin2.Checked;
                var isLocked = chkLocked2.Checked;

                // Input validation
                if (string.IsNullOrWhiteSpace(u))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtU2.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Please enter an email address.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtE2.Focus();
                    return;
                }

                if (!email.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtE2.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(p))
                {
                    MessageBox.Show("Please enter a password.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtP2.Focus();
                    return;
                }

                // Execute update
                var (status, message) = _admin.UpdatePlayer(playerId, u, email, HashHelper.FakeHash(p), isAdmin, isLocked);

                if (status == "SUCCESS")
                {
                    MessageBox.Show(message, "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(message, "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update player failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate player ID
                if (!uint.TryParse(txtPlayerIdDel.Text, out var playerId))
                {
                    MessageBox.Show("Please enter a valid Player ID (positive number).", "Invalid Input",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPlayerIdDel.Focus();
                    return;
                }

                // Confirm deletion
                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete player {playerId}?\n\nThis action cannot be undone.",
                    "Confirm Delete Player",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                // Execute delete
                var (status, message) = _admin.DeletePlayer(playerId);

                if (status == "SUCCESS")
                {
                    MessageBox.Show(message, "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtPlayerIdDel.Clear();
                }
                else
                {
                    MessageBox.Show(message, "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete player failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}