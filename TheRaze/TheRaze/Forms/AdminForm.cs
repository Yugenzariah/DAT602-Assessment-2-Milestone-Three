using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                if (!uint.TryParse(txtGameId.Text, out var gameId))
                {
                    MessageBox.Show("Enter a valid GameID.");
                    return;
                }

                // Properly destructure the tuple
                var (status, message) = _admin.KillGame(gameId);

                if (status == "SUCCESS")
                {
                    MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kill failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var u = txtU.Text.Trim();
                var email = txtE.Text.Trim();
                var p = txtP.Text;
                var isAdmin = chkAdmin.Checked;

                if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(p))
                {
                    MessageBox.Show("Please fill username, email, and password.");
                    return;
                }

                // Properly destructure the tuple (note 3 values)
                var (status, message, playerId) = _admin.AddPlayer(u, email, HashHelper.FakeHash(p), isAdmin);

                if (status == "SUCCESS")
                {
                    string idInfo = playerId.HasValue ? $"PlayerID: {playerId}" : "";
                    MessageBox.Show($"{message}\n{idInfo}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Add failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtPlayerId.Text, out var playerId))
                {
                    MessageBox.Show("Enter a valid PlayerID.");
                    return;
                }

                var u = txtU2.Text.Trim();
                var email = txtE2.Text.Trim();
                var p = txtP2.Text;
                var isAdmin = chkAdmin2.Checked;
                var isLocked = chkLocked2.Checked;

                if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(p))
                {
                    MessageBox.Show("Please fill username, email, and password.");
                    return;
                }

                // Properly destructure the tuple
                var (status, message) = _admin.UpdatePlayer(playerId, u, email, HashHelper.FakeHash(p), isAdmin, isLocked);

                if (status == "SUCCESS")
                {
                    MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtPlayerIdDel.Text, out var playerId))
                {
                    MessageBox.Show("Enter a valid PlayerID.");
                    return;
                }

                // Properly destructure the tuple
                var (status, message) = _admin.DeletePlayer(playerId);

                if (status == "SUCCESS")
                {
                    MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
