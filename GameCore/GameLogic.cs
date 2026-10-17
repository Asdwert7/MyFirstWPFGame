namespace GameCore;

/// <summary>Прямоугольник в экранных координатах (ось Y направлена вниз).</summary>
public readonly record struct Rect(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;
}

/// <summary>Тип вражеского корабля (определяет награду за уничтожение).</summary>
public enum EnemyType
{
    Squid,
    Crab,
    Octopus
}

/// <summary>
/// Модуль «Игровая логика Space Invaders» (разработчик: Студент А).
/// Чистые функции без зависимости от WPF, поэтому модуль полностью тестируется.
/// </summary>
public static class GameLogic
{
    /// <summary>Максимальный множитель скорости на высоких уровнях.</summary>
    public const double MaxSpeedMultiplier = 3.0;

    /// <summary>
    /// Функция 1. Перемещение корабля игрока по горизонтали.
    /// Корабль не должен выходить за границы поля [0; fieldWidth - playerWidth].
    /// </summary>
    /// <param name="x">Текущая левая координата корабля.</param>
    /// <param name="direction">Направление: отрицательное — влево, 0 — стоит, положительное — вправо.</param>
    /// <param name="speed">Шаг за кадр (не может быть отрицательным).</param>
    public static double MovePlayer(double x, int direction, double speed, double fieldWidth, double playerWidth)
    {
        if (speed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speed));
        }

        double newX = x + Math.Sign(direction) * speed;
        double maxX = fieldWidth - playerWidth;

        if (newX < 0)
        {
            return 0;
        }

        if (newX > maxX)
        {
            return maxX;
        }

        return newX;
    }

    /// <summary>
    /// Функция 2. Пересечение двух прямоугольников (AABB).
    /// Прямоугольники, которые только касаются границами, пересекающимися не считаются.
    /// </summary>
    public static bool IsColliding(Rect a, Rect b)
    {
        return a.X < b.Right
            && a.Right > b.X
            && a.Y < b.Bottom
            && a.Bottom > b.Y;
    }

    /// <summary>Функция 3. Очки за уничтожение врага: Squid — 30, Crab — 20, Octopus — 10.</summary>
    public static int GetEnemyScore(EnemyType type)
    {
        return type switch
        {
            EnemyType.Squid => 30,
            EnemyType.Crab => 20,
            EnemyType.Octopus => 10,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    /// <summary>
    /// Функция 4. Можно ли произвести выстрел.
    /// Выстрел разрешён, если с момента предыдущего прошло не меньше cooldown секунд.
    /// </summary>
    public static bool CanShoot(double now, double lastShotTime, double cooldown)
    {
        return now - lastShotTime >= cooldown;
    }

    /// <summary>
    /// Функция 5. Снаряд полностью вышел за пределы игрового поля [0; fieldWidth] x [0; fieldHeight].
    /// </summary>
    public static bool IsOutOfBounds(Rect r, double fieldWidth, double fieldHeight)
    {
        return r.Bottom < 0
            || r.Y > fieldHeight
            || r.Right < 0
            || r.X > fieldWidth;
    }

    /// <summary>Функция 6. Потеря одной жизни; количество жизней не опускается ниже нуля.</summary>
    public static int LoseLife(int lives)
    {
        return lives > 0 ? lives - 1 : 0;
    }

    /// <summary>Функция 7. Игра окончена, когда жизней не осталось.</summary>
    public static bool IsGameOver(int lives)
    {
        return lives <= 0;
    }

    /// <summary>
    /// Функция 8. Множитель скорости для уровня: 1.0 + 0.1 * (level - 1), но не более <see cref="MaxSpeedMultiplier"/>.
    /// </summary>
    public static double GetLevelSpeedMultiplier(int level)
    {
        if (level < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        return Math.Min(1.0 + 0.1 * (level - 1), MaxSpeedMultiplier);
    }
}
