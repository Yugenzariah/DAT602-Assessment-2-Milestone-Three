using System;
using System.Windows.Forms;
using TheRaze.Data;
using TheRaze.Forms;
using TheRaze.Utils;


namespace TheRaze
{
    public partial class LoginForm : Form
    {
        private readonly AuthDao _auth = new AuthDao();

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var user = txtUsername.Text.Trim();
            var pass = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Please enter username and password.");
                return;
            }

            try
            {
                var hash = HashHelper.FakeHash(pass);

                // Properly destructure the tuple
                var (status, message, playerId) = _auth.TryLogin(user, hash);

                switch (status)
                {
                    case "OK":
                        if (playerId.HasValue)
                        {
                            var playerInfo = _auth.GetPlayerInfo(user);
                            if (playerInfo.HasValue)
                            {
                                Session.PlayerId = playerInfo.Value.playerId;
                                Session.Username = playerInfo.Value.username;
                                Session.IsAdmin = playerInfo.Value.isAdmin;

                                MessageBox.Show(message, "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                var lobby = new LobbyForm();
                                lobby.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Error loading player information.");
                            }
                        }
                        break;

                    case "LOCKED":
                        MessageBox.Show(message, "Account Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "BADPASS":
                        MessageBox.Show(message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "UNKNOWN":
                        MessageBox.Show(message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    default:
                        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            new RegisterForm(txtUsername.Text.Trim()).ShowDialog(this);
        }

        private void btnTestDb_Click(object sender, EventArgs e)
        {
            try
            {
                using var cn = new MySql.Data.MySqlClient.MySqlConnection(
                    "Server=127.0.0.1;Port=3306;Database=theraze;Uid=root;Pwd=Prime_Yugenzariah@23%;SslMode=None;");
                cn.Open();
                MessageBox.Show("Connected OK to: " + cn.ServerVersion);
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB connect failed: " + ex.Message);
            }
        }
    }
}
