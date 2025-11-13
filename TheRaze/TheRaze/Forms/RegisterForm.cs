using System;
using System.Windows.Forms;
using TheRaze.Data;
using TheRaze.Utils;

namespace TheRaze.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly AuthDao _auth = new AuthDao();

        public RegisterForm()
        {
            InitializeComponent();
        }

        public RegisterForm(string suggestedUsername)
        {
            InitializeComponent();
            txtUsername.Text = suggestedUsername;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                // Get and trim input values
                var user = txtUsername.Text.Trim();
                var email = txtEmail.Text.Trim();
                var pass = txtPassword.Text;

                // Input validation
                if (string.IsNullOrWhiteSpace(user))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (user.Length < 3)
                {
                    MessageBox.Show("Username must be at least 3 characters long.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (user.Length > 50)
                {
                    MessageBox.Show("Username must be 50 characters or less.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Please enter an email address.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                if (!email.Contains("@") || !email.Contains("."))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Please enter a password.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (pass.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                // Hash password and register
                var hash = HashHelper.FakeHash(pass);
                var (status, message, playerId) = _auth.Register(user, email, hash);

                if (status == "SUCCESS")
                {
                    MessageBox.Show($"{message}\n\nYou can now login with your credentials.",
                        "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show(message, "Registration Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}\n\nPlease ensure MySQL is running.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed:\n{ex.Message}\n\nPlease try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                // Log unexpected errors but still close
                System.Diagnostics.Debug.WriteLine($"Error closing form: {ex.Message}");
                this.Close();
            }
        }
    }
}