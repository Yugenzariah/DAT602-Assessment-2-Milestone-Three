namespace TheRaze.Forms
{
    partial class GameForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlGameBoard = new Panel();
            grpPlayerInfo = new GroupBox();
            chkIsYourTurn = new CheckBox();
            lblPosition = new Label();
            lblHP = new Label();
            lblScore = new Label();
            lblPlayerName = new Label();
            grpCurrentTile = new GroupBox();
            btnPickupItem = new Button();
            lstTileItems = new ListBox();
            lblTileInfo = new Label();
            grpInventory = new GroupBox();
            lstInventory = new ListBox();
            btnRefresh = new Button();
            btnLeaveGame = new Button();
            lblStatus = new Label();
            btnDeleteAccount = new Button();
            btnAdmin = new Button();
            grpPlayerInfo.SuspendLayout();
            grpCurrentTile.SuspendLayout();
            grpInventory.SuspendLayout();
            SuspendLayout();
            // 
            // pnlGameBoard
            // 
            pnlGameBoard.BorderStyle = BorderStyle.FixedSingle;
            pnlGameBoard.Location = new Point(20, 60);
            pnlGameBoard.Name = "pnlGameBoard";
            pnlGameBoard.Size = new Size(600, 600);
            pnlGameBoard.TabIndex = 0;
            // 
            // grpPlayerInfo
            // 
            grpPlayerInfo.Controls.Add(chkIsYourTurn);
            grpPlayerInfo.Controls.Add(lblPosition);
            grpPlayerInfo.Controls.Add(lblHP);
            grpPlayerInfo.Controls.Add(lblScore);
            grpPlayerInfo.Controls.Add(lblPlayerName);
            grpPlayerInfo.Location = new Point(640, 60);
            grpPlayerInfo.Name = "grpPlayerInfo";
            grpPlayerInfo.Size = new Size(320, 200);
            grpPlayerInfo.TabIndex = 1;
            grpPlayerInfo.TabStop = false;
            grpPlayerInfo.Text = "Player Info";
            // 
            // chkIsYourTurn
            // 
            chkIsYourTurn.AutoSize = true;
            chkIsYourTurn.Enabled = false;
            chkIsYourTurn.Location = new Point(10, 150);
            chkIsYourTurn.Name = "chkIsYourTurn";
            chkIsYourTurn.Size = new Size(93, 24);
            chkIsYourTurn.TabIndex = 4;
            chkIsYourTurn.Text = "Your Turn";
            chkIsYourTurn.UseVisualStyleBackColor = true;
            // 
            // lblPosition
            // 
            lblPosition.AutoSize = true;
            lblPosition.Location = new Point(10, 120);
            lblPosition.Name = "lblPosition";
            lblPosition.Size = new Size(101, 20);
            lblPosition.TabIndex = 3;
            lblPosition.Text = "Position: (0, 0)";
            // 
            // lblHP
            // 
            lblHP.AutoSize = true;
            lblHP.Location = new Point(10, 90);
            lblHP.Name = "lblHP";
            lblHP.Size = new Size(58, 20);
            lblHP.TabIndex = 2;
            lblHP.Text = "HP: 100";
            // 
            // lblScore
            // 
            lblScore.AutoSize = true;
            lblScore.Location = new Point(10, 60);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(62, 20);
            lblScore.TabIndex = 1;
            lblScore.Text = "Score: 0";
            // 
            // lblPlayerName
            // 
            lblPlayerName.AutoSize = true;
            lblPlayerName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPlayerName.Location = new Point(10, 30);
            lblPlayerName.Name = "lblPlayerName";
            lblPlayerName.Size = new Size(58, 20);
            lblPlayerName.TabIndex = 0;
            lblPlayerName.Text = "Player:";
            // 
            // grpCurrentTile
            // 
            grpCurrentTile.Controls.Add(btnPickupItem);
            grpCurrentTile.Controls.Add(lstTileItems);
            grpCurrentTile.Controls.Add(lblTileInfo);
            grpCurrentTile.Location = new Point(640, 280);
            grpCurrentTile.Name = "grpCurrentTile";
            grpCurrentTile.Size = new Size(320, 200);
            grpCurrentTile.TabIndex = 2;
            grpCurrentTile.TabStop = false;
            grpCurrentTile.Text = "Current Tile";
            // 
            // btnPickupItem
            // 
            btnPickupItem.Location = new Point(10, 150);
            btnPickupItem.Name = "btnPickupItem";
            btnPickupItem.Size = new Size(140, 35);
            btnPickupItem.TabIndex = 2;
            btnPickupItem.Text = "Pick Up Item";
            btnPickupItem.UseVisualStyleBackColor = true;
            btnPickupItem.Click += btnPickupItem_Click;
            // 
            // lstTileItems
            // 
            lstTileItems.FormattingEnabled = true;
            lstTileItems.ItemHeight = 20;
            lstTileItems.Location = new Point(10, 60);
            lstTileItems.Name = "lstTileItems";
            lstTileItems.Size = new Size(290, 84);
            lstTileItems.TabIndex = 1;
            // 
            // lblTileInfo
            // 
            lblTileInfo.AutoSize = true;
            lblTileInfo.Location = new Point(10, 30);
            lblTileInfo.Name = "lblTileInfo";
            lblTileInfo.Size = new Size(112, 20);
            lblTileInfo.TabIndex = 0;
            lblTileInfo.Text = "Tile Type: Floor";
            // 
            // grpInventory
            // 
            grpInventory.Controls.Add(lstInventory);
            grpInventory.Location = new Point(640, 500);
            grpInventory.Name = "grpInventory";
            grpInventory.Size = new Size(320, 160);
            grpInventory.TabIndex = 3;
            grpInventory.TabStop = false;
            grpInventory.Text = "Inventory";
            // 
            // lstInventory
            // 
            lstInventory.FormattingEnabled = true;
            lstInventory.ItemHeight = 20;
            lstInventory.Location = new Point(10, 30);
            lstInventory.Name = "lstInventory";
            lstInventory.Size = new Size(290, 124);
            lstInventory.TabIndex = 0;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(20, 680);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(120, 40);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Refresh Board";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnLeaveGame
            // 
            btnLeaveGame.Location = new Point(500, 680);
            btnLeaveGame.Name = "btnLeaveGame";
            btnLeaveGame.Size = new Size(120, 40);
            btnLeaveGame.TabIndex = 5;
            btnLeaveGame.Text = "Leave Game";
            btnLeaveGame.UseVisualStyleBackColor = true;
            btnLeaveGame.Click += btnLeaveGame_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.Blue;
            lblStatus.Location = new Point(150, 690);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 20);
            lblStatus.TabIndex = 6;
            // 
            // btnDeleteAccount
            // 
            btnDeleteAccount.BackColor = Color.IndianRed;
            btnDeleteAccount.ForeColor = Color.White;
            btnDeleteAccount.Location = new Point(840, 680);
            btnDeleteAccount.Name = "btnDeleteAccount";
            btnDeleteAccount.Size = new Size(120, 40);
            btnDeleteAccount.TabIndex = 7;
            btnDeleteAccount.Text = "Delete Account";
            btnDeleteAccount.UseVisualStyleBackColor = false;
            btnDeleteAccount.Click += btnDeleteAccount_Click;
            // 
            // btnAdmin
            // 
            btnAdmin.Location = new Point(710, 680);
            btnAdmin.Name = "btnAdmin";
            btnAdmin.Size = new Size(120, 40);
            btnAdmin.TabIndex = 8;
            btnAdmin.Text = "Admin Panel";
            btnAdmin.UseVisualStyleBackColor = true;
            btnAdmin.Click += btnAdmin_Click;
            // 
            // GameForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 741);
            Controls.Add(btnAdmin);
            Controls.Add(btnDeleteAccount);
            Controls.Add(lblStatus);
            Controls.Add(btnLeaveGame);
            Controls.Add(btnRefresh);
            Controls.Add(grpInventory);
            Controls.Add(grpCurrentTile);
            Controls.Add(grpPlayerInfo);
            Controls.Add(pnlGameBoard);
            Name = "GameForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "The Raze - Game Board";
            Load += GameForm_Load;
            grpPlayerInfo.ResumeLayout(false);
            grpPlayerInfo.PerformLayout();
            grpCurrentTile.ResumeLayout(false);
            grpCurrentTile.PerformLayout();
            grpInventory.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlGameBoard;
        private GroupBox grpPlayerInfo;
        private Label lblPlayerName;
        private Label lblScore;
        private Label lblHP;
        private Label lblPosition;
        private CheckBox chkIsYourTurn;
        private GroupBox grpCurrentTile;
        private Label lblTileInfo;
        private ListBox lstTileItems;
        private Button btnPickupItem;
        private GroupBox grpInventory;
        private ListBox lstInventory;
        private Button btnRefresh;
        private Button btnLeaveGame;
        private Label lblStatus;
        private Button btnDeleteAccount;
        private Button btnAdmin;
    }
}