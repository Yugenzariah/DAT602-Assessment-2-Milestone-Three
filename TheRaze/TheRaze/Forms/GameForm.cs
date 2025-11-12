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

namespace TheRaze.Forms
{
    public partial class GameForm : Form
    {
        private readonly GameDao _game = new GameDao();

        public GameForm()
        {
            InitializeComponent();
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtPlayerGameId.Text, out var pgId))
                {
                    MessageBox.Show("Enter a valid PlayerGameID.");
                    return;
                }
                if (!uint.TryParse(txtTargetTileId.Text, out var tileId))
                {
                    MessageBox.Show("Enter a valid TargetTileID.");
                    return;
                }

                // Properly destructure the tuple
                var (status, message) = _game.MovePlayer(pgId, tileId);

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
                MessageBox.Show($"Move failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddScore_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtPlayerGameId.Text, out var pgId))
                {
                    MessageBox.Show("Enter a valid PlayerGameID.");
                    return;
                }
                if (!int.TryParse(txtDelta.Text, out var delta))
                {
                    MessageBox.Show("Enter a valid Score Δ.");
                    return;
                }

                // Properly destructure the tuple (note 3 values)
                var (status, message, newScore) = _game.AddScore(pgId, delta);

                if (status == "SUCCESS")
                {
                    string scoreInfo = newScore.HasValue ? $"New Score: {newScore}" : "";
                    MessageBox.Show($"{message}\n{scoreInfo}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Score update failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnAdmin_Click(object sender, EventArgs e)
        {
            new AdminForm().ShowDialog(this);
        }

        private void btnPickup_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtPlayerGameId.Text, out var pgId))
                {
                    MessageBox.Show("Enter a valid PlayerGameID.");
                    return;
                }
                if (!uint.TryParse(txtItemId.Text, out var itemId))
                {
                    MessageBox.Show("Enter a valid ItemID.");
                    return;
                }
                if (!short.TryParse(txtQuantity.Text, out var qty))
                {
                    MessageBox.Show("Enter a valid Quantity.");
                    return;
                }

                // Properly destructure the tuple
                var (status, message) = _game.PickupItem(pgId, itemId, qty);

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
                MessageBox.Show($"Pickup failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetBoard_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtResetGameId.Text, out var gameId))
                {
                    MessageBox.Show("Enter a valid GameID.");
                    return;
                }
                if (!int.TryParse(txtWidth.Text, out var width) || width <= 0)
                {
                    MessageBox.Show("Enter a valid Width.");
                    return;
                }
                if (!int.TryParse(txtHeight.Text, out var height) || height <= 0)
                {
                    MessageBox.Show("Enter a valid Height.");
                    return;
                }

                // Properly destructure the tuple
                var (status, message) = _game.ResetBoard(gameId, width, height);

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
                MessageBox.Show($"Reset board failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPlaceItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtPlaceTileId.Text, out var tileId))
                {
                    MessageBox.Show("Enter a valid Tile ID.");
                    return;
                }
                if (!uint.TryParse(txtPlaceItemId.Text, out var itemId))
                {
                    MessageBox.Show("Enter a valid Item ID.");
                    return;
                }
                if (!short.TryParse(txtPlaceQty.Text, out var qty))
                {
                    MessageBox.Show("Enter a valid Quantity.");
                    return;
                }

                // Properly destructure the tuple
                var (status, message) = _game.PlaceItemOnTile(tileId, itemId, qty);

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
                MessageBox.Show($"Place item failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            try
            {
                if (!uint.TryParse(txtPlayerIdToDelete.Text, out var playerId))
                {
                    MessageBox.Show("Enter a valid Player ID to Delete.");
                    return;
                }

                var result = MessageBox.Show(
                    "Are you sure you want to delete this account? This cannot be undone!",
                    "Delete Account",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var auth = new AuthDao();
                    auth.DeleteOwnAccount(playerId);
                    MessageBox.Show("Account deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete account failed: " + ex.Message);
            }
        }
    }
}
