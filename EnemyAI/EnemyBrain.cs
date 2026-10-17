namespace EnemyAI;

/// <summary>Направление горизонтального движения вражеского строя.</summary>
public enum Direction
{
    Left = -1,
    Right = 1
}

/// <summary>Вражеский корабль. Ось Y направлена вниз: чем больше Y, тем ниже корабль на экране.</summary>
public sealed record Enemy(
    int Id,
    int Column,
    int Row,
    double X,
    double Y,
    double Width,
    double Height,
    bool IsAlive = true);

/// <summary>
/// Модуль «Искусственный интеллект врагов» (разработчик: Студент Б).
/// Случайные числа передаются параметрами (roll, pickRoll), поэтому поведение полностью детерминировано и тестируемо.
/// </summary>
public static class EnemyBrain
{
    /// <summary>Верхняя граница вероятности выстрела за один тик.</summary>
    public const double MaxShootChance = 0.1;

    /// <summary>
    /// Функция 1. «Передняя линия»: в каждой колонке — самый нижний живой враг (с максимальным Y).
    /// Только такие враги имеют право стрелять. Результат упорядочен по номеру колонки.
    /// </summary>
    public static IReadOnlyList<Enemy> GetFrontLine(IEnumerable<Enemy> enemies)
    {
        ArgumentNullException.ThrowIfNull(enemies);

        return enemies
            .Where(e => e.IsAlive)
            .GroupBy(e => e.Column)
            .OrderBy(g => g.Key)
            .Select(g => g.OrderByDescending(e => e.Y).First())
            .ToList();
    }

    /// <summary>
    /// Функция 2. Достиг ли строй края поля в текущем направлении движения.
    /// Вправо: правая граница любого живого врага >= fieldWidth - margin.
    /// Влево: левая граница любого живого врага <= margin. Если живых врагов нет — false.
    /// </summary>
    public static bool IsAtEdge(IEnumerable<Enemy> enemies, Direction direction, double fieldWidth, double margin)
    {
        var alive = enemies.Where(e => e.IsAlive).ToList();
        if (alive.Count == 0)
        {
            return false;
        }

        return direction == Direction.Right
            ? alive.Max(e => e.X + e.Width) >= fieldWidth - margin
            : alive.Min(e => e.X) <= margin;
    }

    /// <summary>Функция 3. Следующее направление: у края разворот, иначе прежнее.</summary>
    public static Direction NextDirection(Direction current, bool atEdge)
    {
        if (!atEdge)
        {
            return current;
        }

        return current == Direction.Right ? Direction.Left : Direction.Right;
    }

    /// <summary>
    /// Функция 4. Скорость строя: чем меньше врагов осталось, тем он быстрее.
    /// baseSpeed * (1 + 3 * доля_уничтоженных) * (1 + 0.1 * (level - 1)). Если живых нет — 0.
    /// </summary>
    public static double GetFormationSpeed(int aliveCount, int totalCount, int level, double baseSpeed)
    {
        if (totalCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCount));
        }

        if (aliveCount < 0 || aliveCount > totalCount)
        {
            throw new ArgumentOutOfRangeException(nameof(aliveCount));
        }

        if (level < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        if (aliveCount == 0)
        {
            return 0;
        }

        double killedRatio = (double)(totalCount - aliveCount) / totalCount;
        double levelFactor = 1.0 + 0.1 * (level - 1);
        return baseSpeed * (1.0 + 3.0 * killedRatio) * levelFactor;
    }

    /// <summary>
    /// Функция 5. Вероятность выстрела врага за тик: baseChance * (1 + 0.25 * (level - 1)),
    /// но не более <see cref="MaxShootChance"/>.
    /// </summary>
    public static double GetShootChance(int level, double baseChance = 0.01)
    {
        if (level < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        return Math.Min(baseChance * (1.0 + 0.25 * (level - 1)), MaxShootChance);
    }

    /// <summary>
    /// Функция 6. Выбор стреляющего врага.
    /// Если roll >= shootChance — никто не стреляет (null). Иначе из передней линии выбирается враг
    /// с индексом (int)(pickRoll * размер_линии). roll и pickRoll — «случайные» числа из [0; 1].
    /// </summary>
    public static Enemy? ChooseShooter(IEnumerable<Enemy> enemies, double roll, double shootChance, double pickRoll)
    {
        if (pickRoll < 0 || pickRoll > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pickRoll));
        }

        if (roll >= shootChance)
        {
            return null;
        }

        var front = GetFrontLine(enemies);
        if (front.Count == 0)
        {
            return null;
        }

        int index = (int)(pickRoll * front.Count);
        if (index >= front.Count)
        {
            index = front.Count - 1;
        }

        return front[index];
    }

    /// <summary>
    /// Функция 7. Вторжение удалось: нижняя граница какого-либо живого врага достигла линии игрока (playerTop).
    /// </summary>
    public static bool HasReachedPlayer(IEnumerable<Enemy> enemies, double playerTop)
    {
        return enemies.Any(e => e.IsAlive && e.Y + e.Height >= playerTop);
    }

    /// <summary>
    /// Функция 8. Один шаг строя. Если descend = true, живые враги опускаются на descendStep по Y;
    /// иначе сдвигаются по X на speed в направлении direction. Мёртвые враги не изменяются.
    /// Исходная коллекция не модифицируется — возвращаются новые объекты.
    /// </summary>
    public static IReadOnlyList<Enemy> StepFormation(
        IEnumerable<Enemy> enemies, Direction direction, double speed, bool descend, double descendStep)
    {
        return enemies
            .Select(e =>
            {
                if (!e.IsAlive)
                {
                    return e;
                }

                return descend
                    ? e with { Y = e.Y + descendStep }
                    : e with { X = e.X + (int)direction * speed };
            })
            .ToList();
    }
}
