using Xunit;

namespace GameCore.Tests;

/// <summary>
/// Модульные тесты модуля GameCore (автор тестов: Студент Б, тестировщик).
/// Первая итерация: типичные сценарии и очевидные граничные случаи.
/// </summary>
public class GameLogicTests
{
    // ---------- MovePlayer ----------

    [Fact]
    public void MovePlayer_Right_IncreasesX()
    {
        double result = GameLogic.MovePlayer(100, 1, 5, 600, 40);
        Assert.Equal(105, result);
    }

    [Fact]
    public void MovePlayer_Left_DecreasesX()
    {
        double result = GameLogic.MovePlayer(100, -1, 5, 600, 40);
        Assert.Equal(95, result);
    }

    [Fact]
    public void MovePlayer_NoDirection_StaysInPlace()
    {
        double result = GameLogic.MovePlayer(100, 0, 5, 600, 40);
        Assert.Equal(100, result);
    }

    [Fact]
    public void MovePlayer_LeftBorder_ClampsToZero()
    {
        double result = GameLogic.MovePlayer(2, -1, 5, 600, 40);
        Assert.Equal(0, result);
    }

    [Fact]
    public void MovePlayer_RightBorder_ShipStaysFullyInsideField()
    {
        // Поле 600, корабль 40 => максимально допустимая левая координата 560.
        double result = GameLogic.MovePlayer(555, 1, 10, 600, 40);
        Assert.Equal(560, result);
    }

    [Fact]
    public void MovePlayer_NegativeSpeed_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameLogic.MovePlayer(100, 1, -1, 600, 40));
    }

    // ---------- IsColliding ----------

    [Fact]
    public void IsColliding_OverlappingRects_ReturnsTrue()
    {
        var a = new Rect(0, 0, 10, 10);
        var b = new Rect(5, 5, 10, 10);
        Assert.True(GameLogic.IsColliding(a, b));
    }

    [Fact]
    public void IsColliding_SeparatedHorizontally_ReturnsFalse()
    {
        var a = new Rect(0, 0, 10, 10);
        var b = new Rect(50, 0, 10, 10);
        Assert.False(GameLogic.IsColliding(a, b));
    }

    [Fact]
    public void IsColliding_SeparatedVertically_ReturnsFalse()
    {
        var a = new Rect(0, 0, 10, 10);
        var b = new Rect(0, 50, 10, 10);
        Assert.False(GameLogic.IsColliding(a, b));
    }

    [Fact]
    public void IsColliding_OneInsideAnother_ReturnsTrue()
    {
        var outer = new Rect(0, 0, 100, 100);
        var inner = new Rect(40, 40, 5, 5);
        Assert.True(GameLogic.IsColliding(outer, inner));
        Assert.True(GameLogic.IsColliding(inner, outer));
    }

    // ---------- GetEnemyScore ----------

    [Theory]
    [InlineData(EnemyType.Squid, 30)]
    [InlineData(EnemyType.Crab, 20)]
    [InlineData(EnemyType.Octopus, 10)]
    public void GetEnemyScore_ReturnsPointsForType(EnemyType type, int expected)
    {
        Assert.Equal(expected, GameLogic.GetEnemyScore(type));
    }

    // ---------- CanShoot ----------

    [Fact]
    public void CanShoot_CooldownPassed_ReturnsTrue()
    {
        Assert.True(GameLogic.CanShoot(2.0, 1.0, 0.5));
    }

    [Fact]
    public void CanShoot_CooldownNotPassed_ReturnsFalse()
    {
        Assert.False(GameLogic.CanShoot(1.2, 1.0, 0.5));
    }

    // ---------- IsOutOfBounds ----------

    [Fact]
    public void IsOutOfBounds_AboveField_ReturnsTrue()
    {
        Assert.True(GameLogic.IsOutOfBounds(new Rect(100, -20, 4, 10), 600, 500));
    }

    [Fact]
    public void IsOutOfBounds_BelowField_ReturnsTrue()
    {
        Assert.True(GameLogic.IsOutOfBounds(new Rect(100, 520, 4, 10), 600, 500));
    }

    [Fact]
    public void IsOutOfBounds_InsideField_ReturnsFalse()
    {
        Assert.False(GameLogic.IsOutOfBounds(new Rect(100, 100, 4, 10), 600, 500));
    }

    // ---------- LoseLife / IsGameOver ----------

    [Fact]
    public void LoseLife_DecreasesByOne()
    {
        Assert.Equal(2, GameLogic.LoseLife(3));
        Assert.Equal(0, GameLogic.LoseLife(1));
    }

    [Fact]
    public void IsGameOver_NoLives_ReturnsTrue()
    {
        Assert.True(GameLogic.IsGameOver(0));
    }

    [Fact]
    public void IsGameOver_HasLives_ReturnsFalse()
    {
        Assert.False(GameLogic.IsGameOver(3));
    }

    // ---------- GetLevelSpeedMultiplier ----------

    [Fact]
    public void GetLevelSpeedMultiplier_FirstLevel_IsOne()
    {
        Assert.Equal(1.0, GameLogic.GetLevelSpeedMultiplier(1), 6);
    }

    [Fact]
    public void GetLevelSpeedMultiplier_SixthLevel_IsOnePointFive()
    {
        Assert.Equal(1.5, GameLogic.GetLevelSpeedMultiplier(6), 6);
    }

    [Fact]
    public void GetLevelSpeedMultiplier_LevelBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameLogic.GetLevelSpeedMultiplier(0));
    }
}
