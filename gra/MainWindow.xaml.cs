using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace gra
{
    public class Player
    {
        public Rectangle rectangle { get; set; }
        public int playerSpeed { get; set; }
        public int heartCounter { get; set; }
        public Player(int playerSpeed,int heartCounter)
        {
            this.playerSpeed = playerSpeed;
            this.heartCounter = heartCounter;
        }
    }
    public partial class MainWindow : Window
    {
        public Player player = new Player(6,3);

        public bool goLeft = false;
        public bool goRight = false;
        public int step = 3;
        public int windowHeight;
        public int windowWidth;
        public int enemyDirection = 1;
        public bool moveDown = false;
        public int bulletStep = 5;
        public int enemyBulletStep = 2;
        private Stopwatch stopwatch = new Stopwatch();
        private Stopwatch stopwatch2 = new Stopwatch();
        private int shootCooldown = 600;
        private int enemyShootCooldown = 2500;
        public int score = 0;
        public DispatcherTimer timer = new DispatcherTimer();
        public int stage = 1;
        public MainWindow()
        {
            InitializeComponent();
            player.rectangle = playerRectangle;
            main_canvas.Focus();

            windowWidth =(int) Application.Current.MainWindow.Width;
            windowHeight = (int)Application.Current.MainWindow.Height;

            drawEnemies();
            drawHearts();
            drawScore();
            drawLevel();

            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            movePlayer();
            moveBullet();
            moveEnemies();
            moveEnemyBullet();
            if (!stopwatch2.IsRunning || stopwatch2.ElapsedMilliseconds > enemyShootCooldown)
            {
                stopwatch2.Restart();
                generateEnemyBullet();
            }
            checkBulletCollision();
            checkEnemyBulletCollision();
            lookForWin();
        }

        private void lookForWin()
        {
            if (player.heartCounter == 0 || main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "enemy").Any(enemy=>Canvas.GetTop(enemy) + enemy.Height >= Canvas.GetTop(player.rectangle) - 30))
            {
                MessageBox.Show("You lose");
                timer.Stop();
                drawMenu();
            }
            else if (!main_canvas.Children.OfType<Rectangle>().Where(rectangle=>(string)rectangle.Tag == "enemy").Any())
            {
                stage += 1;
                switch (stage)
                {
                    case 2:
                        drawEnemies();
                        drawLevel();
                        enemyShootCooldown = 2000;
                        break;
                    case 3:
                        drawEnemies();
                        drawLevel();
                        enemyShootCooldown = 1500;
                        break;
                    case 4:
                        MessageBox.Show("You won!");
                        timer.Stop();
                        drawMenu();
                        break;
                }
            }
            
        }

        private void drawLevel()
        {
            if (main_canvas.Children.OfType<TextBlock>().Any(textblock => (string)textblock.Tag == "stage"))
            {
                var textBlock = main_canvas.Children.OfType<TextBlock>().Where(t => (string)t.Tag == "stage").ToList();
                textBlock[0].Text = $"Current Stage: {stage}";
            }
            else
            {
                TextBlock textBlock = new TextBlock()
                {
                    Width = 175,
                    Height = 25,
                    FontFamily = new FontFamily("Arial"),
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Tag = "stage"
                };
                textBlock.Text = $"Current Stage: {stage}";
                main_canvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, windowWidth - 200);
                Canvas.SetTop(textBlock, 10);
            }
        }

        private void drawMenu()
        {
            main_canvas.Children.Clear();
            TextBlock textBlock = new TextBlock()
            {
                Width = windowWidth - 200,
                Height = 250,
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("pack://application:,,,/Fonts/#ITC Machine Std"),
                FontSize = 45,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
            };
            textBlock.Text = $" SPACE INVADERS v0.01 " +
                $"\n\n your score: {score}";
            main_canvas.Children.Add(textBlock);
            Canvas.SetLeft(textBlock, 100);
            Canvas.SetTop(textBlock, 100);

            Button btn1 = new Button()
            {
                Width = 200,
                Height = 50,
                FontFamily = new FontFamily("pack://application:,,,/Fonts/#ITC Machine Std"),
                FontSize = 25,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
            };
            btn1.Content = "Play Again";
            btn1.Click += restartGame;
            main_canvas.Children.Add(btn1);

            Canvas.SetLeft(btn1, windowWidth/2-100);
            Canvas.SetTop(btn1, 350);

            Button btn2 = new Button()
            {
                Width = 200,
                Height = 50,
                FontFamily = new FontFamily("pack://application:,,,/Fonts/#ITC Machine Std"),
                FontSize = 25,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
            };
            btn2.Content = "Exit Game";
            btn2.Click += exitGame; ;
            main_canvas.Children.Add(btn2);

            Canvas.SetLeft(btn2, windowWidth / 2 - 100);
            Canvas.SetTop(btn2, 450);
        }

        private void exitGame(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void restartGame(object sender, RoutedEventArgs e)
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void drawScore()
        {
            if(main_canvas.Children.OfType<TextBlock>().Any(textblock =>(string) textblock.Tag == "score"))
            {
                var textBlock = main_canvas.Children.OfType<TextBlock>().Where(t => (string)t.Tag == "score").ToList();
                textBlock[0].Text = $"Score: {score}";
            }
            else
            {
                TextBlock textBlock = new TextBlock()
                {
                    Width = 150,
                    Height = 25,
                    FontFamily = new FontFamily("Arial"),
                    FontSize = 22,
                    FontWeight= FontWeights.Bold,
                    Foreground = Brushes.White,
                    Tag = "score"
                };
                textBlock.Text = $"Score: {score}";

                main_canvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, 10);
                Canvas.SetTop(textBlock, 10);
            } 
        }

        private void generateEnemyBullet()
        {
            Rectangle bullet = new Rectangle()
            {
                Width = 6,
                Height = 15,
                Tag = "enemyBullet",
                Fill = Brushes.Red,
            };
            main_canvas.Children.Add(bullet);
            var random = new Random();
            var enemies = main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "enemy").ToList();
            var randomEnemy = enemies[random.Next(enemies.Count())];
            Canvas.SetLeft(bullet, Canvas.GetLeft(randomEnemy) + randomEnemy.Width / 2);
            Canvas.SetTop(bullet, Canvas.GetTop(randomEnemy) + randomEnemy.Height + 2);
        }

        private void moveEnemyBullet()
        {
            foreach (var bullet in main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "enemyBullet"))
            {
                Canvas.SetTop(bullet, Canvas.GetTop(bullet) + enemyBulletStep);
            }
        }

        private void checkEnemyBulletCollision()
        {
            var toRemove = new List<Rectangle>();
            var hearts = main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "heart");
            foreach (var heart in hearts) { toRemove.Add(heart); };
            Rect rect_player = new Rect(Canvas.GetLeft(player.rectangle), Canvas.GetTop(player.rectangle), player.rectangle.Width, player.rectangle.Height);
            foreach (var enemyBullet in main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "enemyBullet"))
            {
                var enemyBulletRect = new Rect(Canvas.GetLeft(enemyBullet), Canvas.GetTop(enemyBullet), enemyBullet.Width, enemyBullet.Height);
                if (rect_player.IntersectsWith(enemyBulletRect))
                {
                    toRemove.Add(enemyBullet);
                    player.heartCounter--;
                }
                foreach (var bullet in main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "bullet"))
                {
                    var bulletRect = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);
                    if (bulletRect.IntersectsWith(enemyBulletRect))
                    {
                        toRemove.Add(bullet);
                        toRemove.Add(enemyBullet);
                    }
                }
                if (Canvas.GetTop(enemyBullet) + enemyBullet.Height >= windowHeight)
                    toRemove.Add(enemyBullet);
            }
            foreach (var trash in toRemove)
                main_canvas.Children.Remove(trash);
            drawHearts();
        }
        private void checkBulletCollision()
        {
            var toRemove = new List<Rectangle>();
            foreach(var bullet in main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "bullet"))
            {
                var bulletRect = new Rect(Canvas.GetLeft(bullet),Canvas.GetTop(bullet),bullet.Width,bullet.Height);
                foreach(var enemy in main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "enemy"))
                {
                    var enemyRect = new Rect(Canvas.GetLeft(enemy),Canvas.GetTop(enemy),enemy.Width,enemy.Height);
                    if(bulletRect.IntersectsWith(enemyRect))
                    {
                        toRemove.Add(bullet);
                        toRemove.Add(enemy);
                        score += 10;
                        drawScore();
                    }
                }
                if(Canvas.GetTop(bullet) + bullet.Height <= 0)
                    toRemove.Add(bullet);
            }
            foreach(var trash in toRemove)
                main_canvas.Children.Remove(trash);
        }
        private void keyDown(object sender, KeyEventArgs e)
        {
            
            switch (e.Key)
            {
                case Key.A:
                    goLeft = true;
                    break;
                case Key.D:
                    goRight = true;
                    break;
            }
            if (e.Key == Key.Space && (!stopwatch.IsRunning || stopwatch.ElapsedMilliseconds > shootCooldown))
            {
                stopwatch.Restart();
                shoot();
            }
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            goLeft = false;
            goRight = false;
        }
        
        private void shoot()
        {
            Rectangle bullet = new Rectangle()
            {
                Width = 6,
                Height = 12,
                Tag = "bullet",
                Fill = Brushes.Yellow,
            };
            main_canvas.Children.Add(bullet);
            Canvas.SetLeft(bullet, Canvas.GetLeft(player.rectangle) + player.rectangle.Width / 2);
            Canvas.SetTop(bullet, Canvas.GetTop(player.rectangle) - 2);
        }

        private void moveBullet()
        {
            foreach(var bullet in main_canvas.Children.OfType<Rectangle>().Where(rectangle=>(string)rectangle.Tag == "bullet"))
            {
                Canvas.SetTop(bullet,Canvas.GetTop(bullet) - bulletStep);
            }
        }
        private void drawHearts()
        {
            ImageBrush heartSkin = new ImageBrush();
            heartSkin.ImageSource = new BitmapImage(new Uri($@"pack://application:,,,/images/heart.gif"));
            int heart_position_x = 20;
            int heart_position_y = windowHeight - 100;
            for (int i=0;i<player.heartCounter;i++)
            {
                var heartRectangle = new Rectangle()
                {
                    Width = 20,
                    Height = 20,
                    Tag = "heart",
                    Fill = heartSkin,
                };
                main_canvas.Children.Add(heartRectangle);
                Canvas.SetLeft(heartRectangle, heart_position_x);
                Canvas.SetTop(heartRectangle, heart_position_y);
                heart_position_x += 30;
            }
        }
        private void drawEnemies()
        {
            int enemy_position_y = 30;

            for (int j = 1; j <= 3; j++)
            {
                ImageBrush enemySkin = new ImageBrush();
                int enemy_position_x = 90;
                for (int i = 0; i < 10; i++)
                {
                    enemySkin.ImageSource = new BitmapImage(new Uri($@"pack://application:,,,/images/invader{j}.gif"));
                    Rectangle enemy = new Rectangle()
                    {
                        Width = 50,
                        Height = 30,
                        Tag = "enemy",
                        Fill = enemySkin
                    };
                    main_canvas.Children.Add(enemy);
                    Canvas.SetLeft(enemy, enemy_position_x);
                    Canvas.SetTop(enemy, enemy_position_y);
                    enemy_position_x += 60;
                }
                enemy_position_y += 40;
            }
        }

        private void movePlayer()
        {
            Rect rect_player = new Rect(Canvas.GetLeft(player.rectangle), Canvas.GetTop(player.rectangle), player.rectangle.Width, player.rectangle.Height);
            int left = (int)Canvas.GetLeft(player.rectangle);
            if (goLeft && left >= 0)
                left -= player.playerSpeed;
            if (goRight && (left + rect_player.Width + 15 + player.playerSpeed) <= windowWidth)
                left += player.playerSpeed ;

            Canvas.SetLeft(player.rectangle, left);
        }

        private void moveEnemies()
        {
            moveDown = false;
            foreach (var enemy in main_canvas.Children.OfType<Rectangle>().Where(rectangle =>(string)rectangle.Tag == "enemy")) 
            {
                if (Canvas.GetLeft(enemy) + step <= 0 || Canvas.GetLeft(enemy) + enemy.Width + step >= windowWidth)
                {
                    enemyDirection *= -1;
                    moveDown = true;
                    break;
                }
            }
            foreach (var enemy in main_canvas.Children.OfType<Rectangle>().Where(rectangle => (string)rectangle.Tag == "enemy"))
            {
                Canvas.SetLeft(enemy, Canvas.GetLeft(enemy) + step * enemyDirection);
                if(moveDown)
                    Canvas.SetTop(enemy,Canvas.GetTop(enemy) + 10);
            }
        }
    }
}
