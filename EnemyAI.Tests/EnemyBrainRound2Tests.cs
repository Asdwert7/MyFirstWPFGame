using Xunit;

namespace EnemyAI.Tests;

/// <summary>
/// Вторая итерация тестов EnemyAI (Студент А): добавлены по результатам мутационного тестирования —
/// граничные значения, проверка аргументов и пропущенные ветки.
/// </summary>
public class EnemyBrainRound2Tests
{
    private const double FieldWidth = 600;
    private const double Margin = 10;

    private static Enemy E(int id, int column, int row, double x, double y, bool alive = true)
        => new(id, column, row, x, y, 30, 20, alive);

    // ---------- GetFrontLine ----------

    [Fact]
    public void GetFrontLine_NullArgument_Throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => EnemyBrain.GetFrontLine(null!));
        // проверка имени параметра: без явной проверки в методе исключение бросил бы LINQ с именем "source"
        Assert.Equal("enemies", ex.ParamName);
    }

    [Fact]
    public void GetFrontLine_ResultIsOrderedByColumn()
    {
        // враги переданы в «обратном» порядке колонок
        var enemies = new[] { E(7, 2, 0, 80, 0), E(5, 0, 0, 0, 0), E(6, 1, 0, 40, 0) };
        var front = EnemyBrain.GetFrontLine(enemies);
        Assert.Equal(new[] { 5, 6, 7 }, front.Select(e => e.Id).ToArray());
    }

    // ---------- IsAtEdge ----------

    [Fact]
    public void IsAtEdge_RightBorderExactlyOnLimit_ReturnsTrue()
    {
        var enemies = new[] { E(1, 0, 0, 560, 0) }; // 560 + 30 == 590 == 600 - 10
        Assert.True(EnemyBrain.IsAtEdge(enemies, Direction.Right, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_RightBorderJustBeforeLimit_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 559, 0) }; // 589 < 590
        Assert.False(EnemyBrain.IsAtEdge(enemies, Direction.Right, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_LeftBorderExactlyOnLimit_ReturnsTrue()
    {
        var enemies = new[] { E(1, 0, 0, 10, 0) };
        Assert.True(EnemyBrain.IsAtEdge(enemies, Direction.Left, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_LeftBorderJustBeforeLimit_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 11, 0) };
        Assert.False(EnemyBrain.IsAtEdge(enemies, Direction.Left, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_OnlyRightmostEnemyTouchesWall_ReturnsTrue()
    {
        // край определяется самым правым врагом, а не самым левым
        var enemies = new[] { E(1, 0, 0, 100, 0), E(2, 1, 0, 565, 0) };
        Assert.True(EnemyBrain.IsAtEdge(enemies, Direction.Right, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_OnlyLeftmostEnemyTouchesWall_ReturnsTrue()
    {
        // край определяется самым левым врагом, а не самым правым
        var enemies = new[] { E(1, 0, 0, 5, 0), E(2, 1, 0, 300, 0) };
        Assert.True(EnemyBrain.IsAtEdge(enemies, Direction.Left, FieldWidth, Margin));
    }

    [Fact]
    public void IsAtEdge_DeadEnemyAtWall_IsIgnored()
    {
        var enemies = new[] { E(1, 0, 0, 570, 0, alive: false), E(2, 1, 0, 100, 0) };
        Assert.False(EnemyBrain.IsAtEdge(enemies, Direction.Right, FieldWidth, Margin));
    }

    // ---------- GetFormationSpeed ----------

    [Fact]
    public void GetFormationSpeed_AliveMoreThanTotal_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemyBrain.GetFormationSpeed(41, 40, 1, 2.0));
    }

    [Fact]
    public void GetFormationSpeed_NegativeAlive_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemyBrain.GetFormationSpeed(-1, 40, 1, 2.0));
    }

    [Fact]
    public void GetFormationSpeed_LevelBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemyBrain.GetFormationSpeed(10, 40, 0, 2.0));
    }

    [Fact]
    public void GetFormationSpeed_LastEnemyAlive_IsAlmostFourTimesFaster()
    {
        // ratio = 39/40 = 0.975 -> 1 * (1 + 2.925) = 3.925
        Assert.Equal(3.925, EnemyBrain.GetFormationSpeed(1, 40, 1, 1.0), 6);
    }

    // ---------- GetShootChance ----------

    [Fact]
    public void GetShootChance_CustomBase_IsScaledByLevel()
    {
        // 0.05 * (1 + 0.25 * 2) = 0.075 (ниже ограничения 0.1)
        Assert.Equal(0.075, EnemyBrain.GetShootChance(3, 0.05), 6);
    }

    // ---------- ChooseShooter ----------

    [Fact]
    public void ChooseShooter_RollEqualToChance_ReturnsNull()
    {
        var enemies = new[] { E(1, 0, 0, 0, 0) };
        Assert.Null(EnemyBrain.ChooseShooter(enemies, 0.5, 0.5, 0.0));
    }

    [Fact]
    public void ChooseShooter_PickRollEqualsOne_ReturnsLastFrontLineEnemy()
    {
        var enemies = new[] { E(1, 0, 0, 0, 0), E(2, 1, 0, 40, 0) };
        var shooter = EnemyBrain.ChooseShooter(enemies, 0.0, 0.5, 1.0);
        Assert.NotNull(shooter);
        Assert.Equal(2, shooter!.Id);
    }

    [Fact]
    public void ChooseShooter_MiddlePickRoll_PicksMiddleColumn()
    {
        var enemies = new[] { E(1, 0, 0, 0, 0), E(2, 1, 0, 40, 0), E(3, 2, 0, 80, 0) };
        var shooter = EnemyBrain.ChooseShooter(enemies, 0.0, 0.5, 0.5); // (int)(0.5 * 3) = 1
        Assert.NotNull(shooter);
        Assert.Equal(2, shooter!.Id);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void ChooseShooter_PickRollOutOfRange_Throws(double pickRoll)
    {
        var enemies = new[] { E(1, 0, 0, 0, 0) };
        Assert.Throws<ArgumentOutOfRangeException>(() => EnemyBrain.ChooseShooter(enemies, 0.0, 0.5, pickRoll));
    }

    // ---------- HasReachedPlayer ----------

    [Fact]
    public void HasReachedPlayer_BottomExactlyOnPlayerLine_ReturnsTrue()
    {
        var enemies = new[] { E(1, 0, 0, 100, 440) }; // 440 + 20 == 460
        Assert.True(EnemyBrain.HasReachedPlayer(enemies, 460));
    }

    [Fact]
    public void HasReachedPlayer_BottomJustAbovePlayerLine_ReturnsFalse()
    {
        var enemies = new[] { E(1, 0, 0, 100, 439) };
        Assert.False(EnemyBrain.HasReachedPlayer(enemies, 460));
    }

    [Fact]
    public void HasReachedPlayer_OnlyOneOfSeveralEnemiesIsLow_ReturnsTrue()
    {
        var enemies = new[] { E(1, 0, 0, 100, 50), E(2, 1, 0, 140, 445) };
        Assert.True(EnemyBrain.HasReachedPlayer(enemies, 460));
    }

    // ---------- StepFormation ----------

    [Fact]
    public void StepFormation_DeadEnemies_AreNotChanged()
    {
        var dead = E(1, 0, 0, 100, 50, alive: false);
        var moved = EnemyBrain.StepFormation(new[] { dead }, Direction.Right, 2.0, false, 12);
        Assert.Equal(dead, moved[0]);

        var movedDown = EnemyBrain.StepFormation(new[] { dead }, Direction.Right, 2.0, true, 12);
        Assert.Equal(dead, movedDown[0]);
    }

    [Fact]
    public void StepFormation_DoesNotModifySourceObjects()
    {
        var original = E(1, 0, 0, 100, 50);
        EnemyBrain.StepFormation(new[] { original }, Direction.Right, 2.0, false, 12);
        Assert.Equal(100, original.X);
    }
}
