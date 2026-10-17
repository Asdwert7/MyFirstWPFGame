using Xunit;

namespace EnemyAI.Tests;

/// <summary>
/// Модульные тесты модуля EnemyAI (автор тестов: Студент А, тестировщик).
/// Первая итерация: типичные сценарии и очевидные граничные случаи.
/// </summary>
public class EnemyBrainTests
{
    private const double FieldWidth = 600;
    private const double Margin = 10;

    private static Enemy E(int id, int column, int row, double x, double y, bool alive = true)
        => new(id, column, row, x, y, 30, 20, alive);

    /// <summary>Строй 2x2: колонки 0 и 1, верхний ряд Y=0, нижний ряд Y=40.</summary>
    private static Enemy[] Grid2X2() => new[]
    {
        E(1, 0, 0, 0, 0), E(2, 0, 1, 0, 40),
        E(3, 1, 0, 40, 0), E(4, 1, 1, 40, 40)
    };

    // ---------- GetFrontLine ----------

    [Fact]
    public void GetFrontLine_ReturnsLowestAliveEnemyOfEachColumn()
    {
        var front = EnemyBrain.GetFrontLine(Grid2X2());
        Assert.Equal(new[] { 2, 4 }, front.Select(e => e.Id).ToArray());
    }

    [Fact]
    public void GetFrontLine_SkipsDeadEnemies()
    {
        var enemies = new[] { E(1, 0, 0, 0, 0), E(2, 0, 1, 0, 40, alive: false) };
        var front = EnemyBrain.GetFrontLine(enemies);
        Assert.Equal(new[] { 1 }, front.Select(e => e.Id).ToArray());
    }

    [Fact]
    public void GetFrontLine_NoEnemies_ReturnsEmpty()
    {
        Assert.Empty(EnemyBrain.GetFrontLine(new List<Enemy>()));
    }

    [Fact]
    public void GetFrontLine_AllDead_ReturnsEmpty()
    {
        var enemies = new[] { E(1, 0, 0, 0, 0, alive: false), E(2, 1, 0, 40, 0, alive: false) };
        Assert.Empty(EnemyBrain.GetFrontLine(enemies));
    }

    // ---------- IsAtEdge ----------

    [Fact]
    public void IsAtEdge_MovingRightNearRightWall_ReturnsTrue()
    {
        var enemies = new[] { E(1, 0, 0, 565, 0) }; // правая граница 595 >= 590
        Assert.True(EnemyBrain.IsAtEdge(enemies, Direction.Right, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_MovingRightFarFromWall_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 100, 0) };
        Assert.False(EnemyBrain.IsAtEdge(enemies, Direction.Right, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_MovingLeftNearLeftWall_ReturnsTrue()
    {
        var enemies = new[] { E(1, 0, 0, 5, 0) };
        Assert.True(EnemyBrain.IsAtEdge(enemies, Direction.Left, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_MovingLeftFarFromWall_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 100, 0) };
        Assert.False(EnemyBrain.IsAtEdge(enemies, Direction.Left, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_NearRightWallButMovingLeft_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 565, 0) };
        Assert.False(EnemyBrain.IsAtEdge(enemies, Direction.Left, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_NoEnemies_ReturnsFalse()
    {
        Assert.False(EnemyBrain.IsAtEdge(new List<Enemy>(), Direction.Right, FieldWidth, Margin));
    }

    // ---------- NextDirection ----------

    [Theory]
    [InlineData(Direction.Right, true, Direction.Left)]
    [InlineData(Direction.Left, true, Direction.Right)]
    [InlineData(Direction.Right, false, Direction.Right)]
    [InlineData(Direction.Left, false, Direction.Left)]
    public void NextDirection_FlipsOnlyAtEdge(Direction current, bool atEdge, Direction expected)
    {
        Assert.Equal(expected, EnemyBrain.NextDirection(current, atEdge));
    }

    // ---------- GetFormationSpeed ----------

    [Fact]
    public void GetFormationSpeed_AllAlive_FirstLevel_EqualsBaseSpeed()
    {
        Assert.Equal(2.0, EnemyBrain.GetFormationSpeed(40, 40, 1, 2.0), 6);
    }

    [Fact]
    public void GetFormationSpeed_HalfKilled_IsTwoAndHalfTimesFaster()
    {
        // 2.0 * (1 + 3 * 0.5) * 1 = 5.0
        Assert.Equal(5.0, EnemyBrain.GetFormationSpeed(20, 40, 1, 2.0), 6);
    }

    [Fact]
    public void GetFormationSpeed_ThirdLevel_AddsTwentyPercent()
    {
        Assert.Equal(1.2, EnemyBrain.GetFormationSpeed(40, 40, 3, 1.0), 6);
    }

    [Fact]
    public void GetFormationSpeed_NoAliveEnemies_ReturnsZero()
    {
        Assert.Equal(0.0, EnemyBrain.GetFormationSpeed(0, 40, 1, 2.0), 6);
    }

    [Fact]
    public void GetFormationSpeed_ZeroTotal_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemyBrain.GetFormationSpeed(0, 0, 1, 2.0));
    }

    // ---------- GetShootChance ----------

    [Fact]
    public void GetShootChance_FirstLevel_EqualsBase()
    {
        Assert.Equal(0.01, EnemyBrain.GetShootChance(1), 6);
    }

    [Fact]
    public void GetShootChance_FifthLevel_IsDoubled()
    {
        Assert.Equal(0.02, EnemyBrain.GetShootChance(5), 6);
    }

    [Fact]
    public void GetShootChance_HighLevel_IsCapped()
    {
        Assert.Equal(EnemyBrain.MaxShootChance, EnemyBrain.GetShootChance(100), 6);
    }

    [Fact]
    public void GetShootChance_LevelBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemyBrain.GetShootChance(0));
    }

    // ---------- ChooseShooter ----------

    [Fact]
    public void ChooseShooter_LowRoll_PicksFirstFrontLineEnemy()
    {
        var shooter = EnemyBrain.ChooseShooter(Grid2X2(), 0.0, 0.5, 0.0);
        Assert.NotNull(shooter);
        Assert.Equal(2, shooter!.Id);
    }

    [Fact]
    public void ChooseShooter_HighPickRoll_PicksLastFrontLineEnemy()
    {
        var shooter = EnemyBrain.ChooseShooter(Grid2X2(), 0.0, 0.5, 0.99);
        Assert.NotNull(shooter);
        Assert.Equal(4, shooter!.Id);
    }

    [Fact]
    public void ChooseShooter_RollAboveChance_ReturnsNull()
    {
        Assert.Null(EnemyBrain.ChooseShooter(Grid2X2(), 0.9, 0.5, 0.0));
    }

    [Fact]
    public void ChooseShooter_NoEnemies_ReturnsNull()
    {
        Assert.Null(EnemyBrain.ChooseShooter(new List<Enemy>(), 0.0, 0.5, 0.0));
    }

    // ---------- HasReachedPlayer ----------

    [Fact]
    public void HasReachedPlayer_EnemyFarAbove_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 100, 100) };
        Assert.False(EnemyBrain.HasReachedPlayer(enemies, 460));
    }

    [Fact]
    public void HasReachedPlayer_EnemyOverlapsPlayerLine_ReturnsTrue()
    {
        var enemies = new[] { E(1, 0, 0, 100, 445) }; // нижняя граница 465 >= 460
        Assert.True(EnemyBrain.HasReachedPlayer(enemies, 460));
    }

    [Fact]
    public void HasReachedPlayer_OnlyDeadEnemyIsLow_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 100, 445, alive: false), E(2, 1, 0, 140, 50) };
        Assert.False(EnemyBrain.HasReachedPlayer(enemies, 460));
    }

    // ---------- StepFormation ----------

    [Fact]
    public void StepFormation_Right_MovesEnemiesAlongX()
    {
        var moved = EnemyBrain.StepFormation(new[] { E(1, 0, 0, 100, 50) }, Direction.Right, 2.0, false, 12);
        Assert.Equal(102, moved[0].X);
        Assert.Equal(50, moved[0].Y);
    }

    [Fact]
    public void StepFormation_Left_MovesEnemiesAgainstX()
    {
        var moved = EnemyBrain.StepFormation(new[] { E(1, 0, 0, 100, 50) }, Direction.Left, 2.0, false, 12);
        Assert.Equal(98, moved[0].X);
    }

    [Fact]
    public void StepFormation_Descend_MovesEnemiesDownOnly()
    {
        var moved = EnemyBrain.StepFormation(new[] { E(1, 0, 0, 100, 50) }, Direction.Right, 2.0, true, 12);
        Assert.Equal(100, moved[0].X);
        Assert.Equal(62, moved[0].Y);
    }
}
