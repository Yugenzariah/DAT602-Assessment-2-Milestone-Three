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
                // Input validation
                var user = txtUsername.Text.Trim();
                var pass = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(user))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Please enter a password.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                // Hash password and attempt login
                var hash = HashHelper.FakeHash(pass);
                var (status, message, playerId) = _auth.TryLogin(user, hash);

                switch (status)
                {
                    case "OK":
                        if (playerId.HasValue)
                        {
                            // Get additional player information
                            var playerInfo = _auth.GetPlayerInfo(user);
                            if (playerInfo.HasValue)
                            {
                                // Initialize session
                                Session.PlayerId = playerInfo.Value.playerId;
                                Session.Username = playerInfo.Value.username;
                                Session.IsAdmin = playerInfo.Value.isAdmin;

                                MessageBox.Show(message, "Login Successful",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Navigate to lobby
                                var lobby = new LobbyForm();
                                lobby.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Error loading player information. Please try again.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Login succeeded but player ID was not returned.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;

                    case "LOCKED":
                        MessageBox.Show(message, "Account Locked",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "BADPASS":
                        MessageBox.Show(message, "Login Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPassword.Clear();
                        txtPassword.Focus();
                        break;

                    case "UNKNOWN":
                        MessageBox.Show(message, "User Not Found",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "ERROR":
                        MessageBox.Show($"Login error: {message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                    default:
                        MessageBox.Show($"Unexpected response: {message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}\n\nPlease ensure MySQL is running.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred during login:\n{ex.Message}\n\nPlease try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                var registerForm = new RegisterForm(txtUsername.Text.Trim());
                registerForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening registration form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestDb_Click(object sender, EventArgs e)
        {
            try
            {
                using var cn = new MySql.Data.MySqlClient.MySqlConnection(
                    "Server=127.0.0.1;Port=3306;Database=theraze;Uid=root;Pwd=Prime_Yugenzariah@23%;SslMode=None;");
                cn.Open();
                MessageBox.Show($"? Database connection successful!\n\nServer Version: {cn.ServerVersion}",
                    "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"Database connection failed:\n\n{ex.Message}\n\nPlease check:\n• MySQL service is running\n• Database 'theraze' exists\n• Username/password are correct",
                    "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection test failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}