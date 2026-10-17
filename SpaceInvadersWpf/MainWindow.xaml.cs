using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using EnemyAI;
using GameCore;
using Rect = GameCore.Rect; // в WPF есть свой System.Windows.Rect

namespace SpaceInvadersWpf;

/// <summary>
/// Оболочка игры: рисование, ввод и игровой цикл. Вся логика вынесена в модули
/// GameCore (правила игры) и EnemyAI (поведение врагов), которые покрыты модульными тестами.
/// Управление: ← → — движение, Пробел — выстрел, R — перезапуск после проигрыша.
/// </summary>
public partial class MainWindow : Window
{
    private const double FieldWidth = 600;
    private const double FieldHeight = 500;
    private const double PlayerWidth = 40;
    private const double PlayerHeight = 20;
    private const double PlayerTop = 460;
    private const double PlayerSpeed = 6;
    private const double BulletSpeed = 9;
    private const double EnemyBulletSpeed = 5;
    private const double ShotCooldown = 0.4;
    private const double TickSeconds = 0.016;
    private const int Rows = 5;
    private const int Columns = 8;

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly Random _rng = new();
    private readonly Dictionary<int, EnemyType> _types = new();
    private readonly List<Rect> _playerBullets = new();
    private readonly List<Rect> _enemyBullets = new();

    private List<Enemy> _enemies = new();
    private Direction _direction = Direction.Right;
    private double _playerX;
    private int _moveDirection;
    private bool _shootHeld;
    private double _time;
    private double _lastShotTime = -10;
    private int _score;
    private int _lives = 3;
    private int _level = 1;
    private bool _gameOver;

    public MainWindow()
    {
        InitializeComponent();
        StartNewGame();
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    private void StartNewGame()
    {
        _score = 0;
        _lives = 3;
        _level = 1;
        _gameOver = false;
        _playerX = (FieldWidth - PlayerWidth) / 2;
        _playerBullets.Clear();
        _enemyBullets.Clear();
        SpawnWave();
    }

    private void SpawnWave()
    {
        _enemies = new List<Enemy>();
        _types.Clear();
        _direction = Direction.Right;
        int id = 0;
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                _enemies.Add(new Enemy(id, col, row, 40 + col * 50, 40 + row * 34, 30, 20));
                _types[id] = row == 0 ? EnemyType.Squid : row <= 2 ? EnemyType.Crab : EnemyType.Octopus;
                id++;
            }
        }
    }

    private void Tick()
    {
        _time += TickSeconds;
        if (_gameOver)
        {
            Draw();
            return;
        }

        // игрок
        _playerX = GameLogic.MovePlayer(_playerX, _moveDirection, PlayerSpeed, FieldWidth, PlayerWidth);
        if (_shootHeld && GameLogic.CanShoot(_time, _lastShotTime, ShotCooldown))
        {
            _playerBullets.Add(new Rect(_playerX + PlayerWidth / 2 - 2, PlayerTop - 10, 4, 10));
            _lastShotTime = _time;
        }

        MoveBullets();
        MoveEnemies();
        EnemyShooting();
        ResolveCollisions();

        if (!_gameOver && EnemyBrain.HasReachedPlayer(_enemies, PlayerTop))
        {
            _gameOver = true;
        }

        if (!_gameOver && _enemies.All(e => !e.IsAlive))
        {
            _level++;
            SpawnWave();
        }

        Draw();
    }

    private void MoveBullets()
    {
        double speed = BulletSpeed * GameLogic.GetLevelSpeedMultiplier(_level);
        for (int i = _playerBullets.Count - 1; i >= 0; i--)
        {
            var b = _playerBullets[i];
            b = new Rect(b.X, b.Y - speed, b.Width, b.Height);
            if (GameLogic.IsOutOfBounds(b, FieldWidth, FieldHeight))
            {
                _playerBullets.RemoveAt(i);
            }
            else
            {
                _playerBullets[i] = b;
            }
        }

        for (int i = _enemyBullets.Count - 1; i >= 0; i--)
        {
            var b = _enemyBullets[i];
            b = new Rect(b.X, b.Y + EnemyBulletSpeed, b.Width, b.Height);
            if (GameLogic.IsOutOfBounds(b, FieldWidth, FieldHeight))
            {
                _enemyBullets.RemoveAt(i);
            }
            else
            {
                _enemyBullets[i] = b;
            }
        }
    }

    private void MoveEnemies()
    {
        int alive = _enemies.Count(e => e.IsAlive);
        if (alive == 0)
        {
            return;
        }

        double speed = EnemyBrain.GetFormationSpeed(alive, _types.Count, _level, 0.5);
        bool atEdge = EnemyBrain.IsAtEdge(_enemies, _direction, FieldWidth, 10);
        _enemies = EnemyBrain.StepFormation(_enemies, _direction, speed, atEdge, 12).ToList();
        _direction = EnemyBrain.NextDirection(_direction, atEdge);
    }

    private void EnemyShooting()
    {
        double chance = EnemyBrain.GetShootChance(_level);
        var shooter = EnemyBrain.ChooseShooter(_enemies, _rng.NextDouble(), chance, _rng.NextDouble());
        if (shooter != null)
        {
            _enemyBullets.Add(new Rect(shooter.X + shooter.Width / 2 - 2, shooter.Y + shooter.Height, 4, 10));
        }
    }

    private void ResolveCollisions()
    {
        // пули игрока против врагов
        for (int i = _playerBullets.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < _enemies.Count; j++)
            {
                var e = _enemies[j];
                if (!e.IsAlive)
                {
                    continue;
                }

                if (GameLogic.IsColliding(_playerBullets[i], new Rect(e.X, e.Y, e.Width, e.Height)))
                {
                    _enemies[j] = e with { IsAlive = false };
                    _score += GameLogic.GetEnemyScore(_types[e.Id]);
                    _playerBullets.RemoveAt(i);
                    break;
                }
            }
        }

        // пули врагов против игрока
        var player = new Rect(_playerX, PlayerTop, PlayerWidth, PlayerHeight);
        for (int i = _enemyBullets.Count - 1; i >= 0; i--)
        {
            if (GameLogic.IsColliding(_enemyBullets[i], player))
            {
                _enemyBullets.RemoveAt(i);
                _lives = GameLogic.LoseLife(_lives);
                if (GameLogic.IsGameOver(_lives))
                {
                    _gameOver = true;
                }
            }
        }
    }

    private void Draw()
    {
        Field.Children.Clear();
        AddRect(_playerX, PlayerTop, PlayerWidth, PlayerHeight, Brushes.LimeGreen);

        foreach (var enemy in _enemies.Where(en => en.IsAlive))
        {
            var brush = _types[enemy.Id] switch
            {
                EnemyType.Squid => Brushes.Magenta,
                EnemyType.Crab => Brushes.Orange,
                _ => Brushes.Cyan
            };
            AddRect(enemy.X, enemy.Y, enemy.Width, enemy.Height, brush);
        }

        foreach (var b in _playerBullets)
        {
            AddRect(b.X, b.Y, b.Width, b.Height, Brushes.White);
        }

        foreach (var b in _enemyBullets)
        {
            AddRect(b.X, b.Y, b.Width, b.Height, Brushes.Red);
        }

        StatusText.Text = _gameOver
            ? $"GAME OVER — счёт {_score}. Нажмите R для новой игры"
            : $"Счёт: {_score}   Жизни: {_lives}   Уровень: {_level}";
    }

    private void AddRect(double x, double y, double w, double h, Brush brush)
    {
        var r = new Rectangle { Width = w, Height = h, Fill = brush };
        Canvas.SetLeft(r, x);
        Canvas.SetTop(r, y);
        Field.Children.Add(r);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left: _moveDirection = -1; break;
            case Key.Right: _moveDirection = 1; break;
            case Key.Space: _shootHeld = true; break;
            case Key.R when _gameOver: StartNewGame(); break;
        }
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left when _moveDirection == -1: _moveDirection = 0; break;
            case Key.Right when _moveDirection == 1: _moveDirection = 0; break;
            case Key.Space: _shootHeld = false; break;
        }
    }
}
