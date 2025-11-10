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
            try
            {
                var user = txtUsername.Text.Trim();
                var pass = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(user))
                {
                    MessageBox.Show("Please enter a username.");
                    return;
                }

                var status = _auth.TryLogin(user, HashHelper.FakeHash(pass));
                switch (status)
                {
                    case "OK":
                        var playerInfo = _auth.GetPlayerInfo(user);
                        if (playerInfo.HasValue)
                        {
                            Session.PlayerId = playerInfo.Value.playerId;
                            Session.Username = playerInfo.Value.username;
                            Session.IsAdmin = playerInfo.Value.isAdmin;

                            MessageBox.Show($"Welcome back, {user}!");

                            // Open Lobby
                            var lobby = new LobbyForm();
                            lobby.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Error loading player information.");
                        }
                        break;
                    case "LOCKED":
                        MessageBox.Show("Account locked. Contact admin.");
                        break;
                    case "BADPASS":
                        MessageBox.Show("Incorrect password.");
                        break;
                    case "UNKNOWN":
                        if (MessageBox.Show("User not found. Register now?", "Register",
                                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            new RegisterForm(user).ShowDialog(this);
                        }
                        break;
                    default:
                        MessageBox.Show("Unexpected status: " + status);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message);
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
