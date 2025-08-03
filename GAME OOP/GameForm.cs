using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using EZInput;

namespace GAME_OOP
{
    public partial class GameForm : Form
    {
        // Game Objects
        PictureBox player;
        PictureBox enemy;
        List<PictureBox> playerBullets = new List<PictureBox>();
        List<PictureBox> enemyBullets = new List<PictureBox>();

        // Game Variables
        Random random;
        string enemyDirection = "left";
        int enemySpeed = 7;
        int bulletSpeed = 15;
        int playerHealth = 200;
        int enemyHealth = 100;
        bool gameRunning = true;
        int shootCooldown = 0;
        const int SHOOT_DELAY = 8;

        // Health Display
        Label playerHealthLabel;
        Label enemyHealthLabel;

        public GameForm()
        {
            InitializeComponent();
            this.Load += GameForm_Load; // Connect the Load event
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            StartNewGame();
        }

        private void StartNewGame()
        {
            // Initialize random
            random = new Random();

            // Clear any existing game objects
            ClearGameObjects();

            // Setup health display
            InitializeHealthDisplay();

            // Create game objects
            CreatePlayer();
            CreateEnemy();

            // Start game loop
            gameRunning = true;
            GameLoop.Start();
        }

        private void ClearGameObjects()
        {
            // Clear player
            if (player != null)
            {
                this.Controls.Remove(player);
                player.Dispose();
                player = null;
            }

            // Clear enemy
            if (enemy != null)
            {
                this.Controls.Remove(enemy);
                enemy.Dispose();
                enemy = null;
            }

            // Clear bullets
            foreach (var bullet in playerBullets)
            {
                if (bullet != null && !bullet.IsDisposed)
                {
                    this.Controls.Remove(bullet);
                    bullet.Dispose();
                }
            }
            playerBullets.Clear();

            foreach (var bullet in enemyBullets)
            {
                if (bullet != null && !bullet.IsDisposed)
                {
                    this.Controls.Remove(bullet);
                    bullet.Dispose();
                }
            }
            enemyBullets.Clear();

            // Clear health labels
            if (playerHealthLabel != null)
            {
                this.Controls.Remove(playerHealthLabel);
                playerHealthLabel.Dispose();
                playerHealthLabel = null;
            }

            if (enemyHealthLabel != null)
            {
                this.Controls.Remove(enemyHealthLabel);
                enemyHealthLabel.Dispose();
                enemyHealthLabel = null;
            }
        }

        private void InitializeHealthDisplay()
        {
            playerHealthLabel = new Label();
            playerHealthLabel.Text = $"Player: {playerHealth}/200";
            playerHealthLabel.ForeColor = Color.Green;
            playerHealthLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            playerHealthLabel.Top = 10;
            playerHealthLabel.Left = 10;
            playerHealthLabel.AutoSize = true;
            this.Controls.Add(playerHealthLabel);
            playerHealthLabel.BringToFront();

            enemyHealthLabel = new Label();
            enemyHealthLabel.Text = $"Enemy: {enemyHealth}/100";
            enemyHealthLabel.ForeColor = Color.Red;
            enemyHealthLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            enemyHealthLabel.Top = 40;
            enemyHealthLabel.Left = 10;
            enemyHealthLabel.AutoSize = true;
            this.Controls.Add(enemyHealthLabel);
            enemyHealthLabel.BringToFront();
        }

        private void CreatePlayer()
        {
            player = new PictureBox();
            player.Image = Properties.Resources.Ship21;
            player.SizeMode = PictureBoxSizeMode.AutoSize;
            player.BackColor = Color.Transparent;
            player.Top = this.ClientSize.Height / 2 + 80;
            player.Left = (this.ClientSize.Width - player.Width) / 2;
            this.Controls.Add(player);
        }

        private void CreateEnemy()
        {
            enemy = new PictureBox();
            enemy.Image = Properties.Resources.Ship5;
            enemy.SizeMode = PictureBoxSizeMode.AutoSize;
            enemy.BackColor = Color.Transparent;
            enemy.Top = 50;
            enemy.Left = random.Next(50, this.ClientSize.Width - 50);
            this.Controls.Add(enemy);
        }

        private void GameLoop_Tick(object sender, EventArgs e)
        {
            if (!gameRunning) return;

            MovePlayer();
            MoveEnemy();
            HandleShooting();
            MoveBullets();
            CheckCollisions();
            UpdateHealth();
        }

        private void MovePlayer()
        {
            if (Keyboard.IsKeyPressed(Key.RightArrow) && player.Right < this.ClientSize.Width)
                player.Left += 10;

            if (Keyboard.IsKeyPressed(Key.LeftArrow) && player.Left > 0)
                player.Left -= 10;
        }

        private void MoveEnemy()
        {
            if (enemyDirection == "left")
            {
                enemy.Left -= enemySpeed;
                if (enemy.Left <= 0)
                    enemyDirection = "right";
            }
            else
            {
                enemy.Left += enemySpeed;
                if (enemy.Right >= this.ClientSize.Width)
                    enemyDirection = "left";
            }
        }

        private void HandleShooting()
        {
            if (shootCooldown > 0)
                shootCooldown--;

            if (Keyboard.IsKeyPressed(Key.Space) && shootCooldown == 0)
            {
                ShootBullet(true);
                shootCooldown = SHOOT_DELAY;
            }

            if (random.Next(0, 100) < 10)
                ShootBullet(false);
        }

        private void ShootBullet(bool isPlayer)
        {
            PictureBox bullet = new PictureBox();
            bullet.Image = isPlayer ? Properties.Resources.shotplayer : Properties.Resources.shotenemy;
            bullet.SizeMode = PictureBoxSizeMode.AutoSize;
            bullet.BackColor = Color.Transparent;

            if (isPlayer)
            {
                bullet.Top = player.Top - bullet.Height;
                bullet.Left = player.Left + (player.Width / 2) - (bullet.Width / 2);
                playerBullets.Add(bullet);
            }
            else
            {
                bullet.Top = enemy.Bottom;
                bullet.Left = enemy.Left + (enemy.Width / 2) - (bullet.Width / 2);
                enemyBullets.Add(bullet);
            }

            this.Controls.Add(bullet);
            bullet.BringToFront();
        }

        private void MoveBullets()
        {
            // Player bullets
            for (int i = playerBullets.Count - 1; i >= 0; i--)
            {
                playerBullets[i].Top -= bulletSpeed;
                if (playerBullets[i].Bottom < 0)
                {
                    this.Controls.Remove(playerBullets[i]);
                    playerBullets[i].Dispose();
                    playerBullets.RemoveAt(i);
                }
            }

            // Enemy bullets
            for (int i = enemyBullets.Count - 1; i >= 0; i--)
            {
                enemyBullets[i].Top += bulletSpeed;
                if (enemyBullets[i].Top > this.ClientSize.Height)
                {
                    this.Controls.Remove(enemyBullets[i]);
                    enemyBullets[i].Dispose();
                    enemyBullets.RemoveAt(i);
                }
            }
        }

        private void CheckCollisions()
        {
            // Player bullets hit enemy
            for (int i = playerBullets.Count - 1; i >= 0; i--)
            {
                if (playerBullets[i].Bounds.IntersectsWith(enemy.Bounds))
                {
                    enemyHealth -= 10;
                    this.Controls.Remove(playerBullets[i]);
                    playerBullets[i].Dispose();
                    playerBullets.RemoveAt(i);

                    if (enemyHealth <= 0)
                        GameOver(true);
                }
            }

            // Enemy bullets hit player
            for (int i = enemyBullets.Count - 1; i >= 0; i--)
            {
                if (enemyBullets[i].Bounds.IntersectsWith(player.Bounds))
                {
                    playerHealth -= 10;
                    this.Controls.Remove(enemyBullets[i]);
                    enemyBullets[i].Dispose();
                    enemyBullets.RemoveAt(i);

                    if (playerHealth <= 0)
                        GameOver(false);
                }
            }
        }

        private void UpdateHealth()
        {
            playerHealthLabel.Text = $"Player: {playerHealth}/200";
            enemyHealthLabel.Text = $"Enemy: {enemyHealth}/100";

            playerHealthLabel.ForeColor = playerHealth > 100 ? Color.Green :
                                       playerHealth > 50 ? Color.Yellow : Color.Red;
        }

        private void GameOver(bool playerWon)
        {
            gameRunning = false;
            GameLoop.Stop();

            Form resultForm = new Form();
            resultForm.Width = 300;
            resultForm.Height = 200;
            resultForm.Text = "Game Over";
            resultForm.StartPosition = FormStartPosition.CenterScreen;

            Label lblResult = new Label();
            lblResult.Text = playerWon ? "You Won!" : "You Lost!";
            lblResult.Font = new Font("Arial", 20, FontStyle.Bold);
            lblResult.AutoSize = true;
            lblResult.Top = 30;
            lblResult.Left = 100;
            resultForm.Controls.Add(lblResult);

            Button btnPlayAgain = new Button();
            btnPlayAgain.Text = "Play Again";
            btnPlayAgain.Top = 100;
            btnPlayAgain.Left = 50;
            btnPlayAgain.Click += (s, e) => {
                resultForm.Close();
                ResetGame();
            };
            resultForm.Controls.Add(btnPlayAgain);

            Button btnExit = new Button();
            btnExit.Text = "Exit";
            btnExit.Top = 100;
            btnExit.Left = 150;
            btnExit.Click += (s, e) => Application.Exit();
            resultForm.Controls.Add(btnExit);

            resultForm.ShowDialog();
        }

        private void ResetGame()
        {
            // Reset health
            playerHealth = 200;
            enemyHealth = 100;

            // Start new game
            StartNewGame();
        }
    }
} 