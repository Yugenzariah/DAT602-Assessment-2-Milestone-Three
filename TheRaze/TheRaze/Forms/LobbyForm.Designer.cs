namespace TheRaze.Forms
{
    partial class LobbyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblWelcome = new Label();
            dgvGames = new DataGridView();
            btnJoinGame = new Button();
            btnCreateGame = new Button();
            btnRefresh = new Button();
            label1 = new Label();
            btnLogout = new Button();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvGames).BeginInit();
            SuspendLayout();
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.BackColor = Color.Transparent;
            lblWelcome.Font = new Font("Cooper Black", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblWelcome.ForeColor = SystemColors.ControlLightLight;
            lblWelcome.Location = new Point(135, 9);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(166, 32);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Welcome, !";
            // 
            // dgvGames
            // 
            dgvGames.AllowUserToAddRows = false;
            dgvGames.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGames.Location = new Point(12, 122);
            dgvGames.MultiSelect = false;
            dgvGames.Name = "dgvGames";
            dgvGames.ReadOnly = true;
            dgvGames.RowHeadersWidth = 51;
            dgvGames.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGames.Size = new Size(887, 241);
            dgvGames.TabIndex = 1;
            // 
            // btnJoinGame
            // 
            btnJoinGame.Location = new Point(12, 488);
            btnJoinGame.Name = "btnJoinGame";
            btnJoinGame.Size = new Size(161, 29);
            btnJoinGame.TabIndex = 2;
            btnJoinGame.Text = "Join Selected Game";
            btnJoinGame.UseVisualStyleBackColor = true;
            btnJoinGame.Click += btnJoinGame_Click;
            // 
            // btnCreateGame
            // 
            btnCreateGame.Location = new Point(262, 488);
            btnCreateGame.Name = "btnCreateGame";
            btnCreateGame.Size = new Size(161, 29);
            btnCreateGame.TabIndex = 3;
            btnCreateGame.Text = "Create New Game";
            btnCreateGame.UseVisualStyleBackColor = true;
            btnCreateGame.Click += btnCreateGame_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(489, 488);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(161, 29);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Refresh List";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Cooper Black", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.ControlLightLight;
            label1.Location = new Point(12, 98);
            label1.Name = "label1";
            label1.Size = new Size(167, 21);
            label1.TabIndex = 5;
            label1.Text = "Available Games:";
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(738, 488);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(161, 29);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = SystemColors.Highlight;
            lblStatus.Location = new Point(12, 533);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(21, 20);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "\"\"";
            // 
            // LobbyForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.e52fb07e65f66f9eaf34764b8a679c851;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(911, 572);
            Controls.Add(lblStatus);
            Controls.Add(btnLogout);
            Controls.Add(label1);
            Controls.Add(btnRefresh);
            Controls.Add(btnCreateGame);
            Controls.Add(btnJoinGame);
            Controls.Add(dgvGames);
            Controls.Add(lblWelcome);
            Name = "LobbyForm";
            Text = "LobbyForm";
            ((System.ComponentModel.ISupportInitialize)dgvGames).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblWelcome;
        private DataGridView dgvGames;
        private Button btnJoinGame;
        private Button btnCreateGame;
        private Button btnRefresh;
        private Label label1;
        private Button btnLogout;
        private Label lblStatus;
    }
}