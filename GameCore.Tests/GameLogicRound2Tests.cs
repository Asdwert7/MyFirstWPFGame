using Xunit;

namespace GameCore.Tests;

/// <summary>
/// Вторая итерация тестов GameCore (Студент Б): добавлены по результатам мутационного тестирования —
/// каждый тест убивает выживших мутантов первой итерации (граничные значения, пропущенные ветки).
/// </summary>
public class GameLogicRound2Tests
{
    // ---------- MovePlayer ----------

    [Fact]
    public void MovePlayer_ZeroSpeed_IsAllowedAndKeepsPosition()
    {
        // убивает мутант speed < 0 -> speed <= 0
        Assert.Equal(100, GameLogic.MovePlayer(100, 1, 0, 600, 40));
    }

    [Theory]
    [InlineData(5, 102.0)]
    [InlineData(-3, 98.0)]
    public void MovePlayer_DirectionIsNormalizedToSign(int direction, double expected)
    {
        // направление важно только по знаку: шаг всегда равен speed
        Assert.Equal(expected, GameLogic.MovePlayer(100, direction, 2, 600, 40));
    }

    [Fact]
    public void MovePlayer_ExactlyOnRightLimit_StaysOnLimit()
    {
        Assert.Equal(560, GameLogic.MovePlayer(550, 1, 10, 600, 40));
    }

    [Fact]
    public void MovePlayer_ExactlyOnLeftLimit_StaysOnLimit()
    {
        Assert.Equal(0, GameLogic.MovePlayer(10, -1, 10, 600, 40));
    }

    // ---------- IsColliding: касание границ не считается столкновением ----------

    [Fact]
    public void IsColliding_TouchingByRightSide_ReturnsFalse()
    {
        Assert.False(GameLogic.IsColliding(new Rect(0, 0, 10, 10), new Rect(10, 0, 10, 10)));
    }

    [Fact]
    public void IsColliding_TouchingByLeftSide_ReturnsFalse()
    {
        Assert.False(GameLogic.IsColliding(new Rect(10, 0, 10, 10), new Rect(0, 0, 10, 10)));
    }

    [Fact]
    public void IsColliding_TouchingByBottomSide_ReturnsFalse()
    {
        Assert.False(GameLogic.IsColliding(new Rect(0, 0, 10, 10), new Rect(0, 10, 10, 10)));
    }

    [Fact]
    public void IsColliding_TouchingByTopSide_ReturnsFalse()
    {
        Assert.False(GameLogic.IsColliding(new Rect(0, 10, 10, 10), new Rect(0, 0, 10, 10)));
    }

    [Fact]
    public void IsColliding_OverlapByOneUnit_ReturnsTrue()
    {
        Assert.True(GameLogic.IsColliding(new Rect(0, 0, 10, 10), new Rect(9, 9, 10, 10)));
    }

    // ---------- GetEnemyScore ----------

    [Fact]
    public void GetEnemyScore_UnknownType_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameLogic.GetEnemyScore((EnemyType)99));
    }

    // ---------- CanShoot ----------

    [Fact]
    public void CanShoot_ExactlyCooldown_ReturnsTrue()
    {
        // 1.5 - 1.0 == 0.5 точно: убивает мутант >= -> >
        Assert.True(GameLogic.CanShoot(1.5, 1.0, 0.5));
    }

    // ---------- IsOutOfBounds ----------

    [Fact]
    public void IsOutOfBounds_BulletTouchesTopEdgeFromOutside_ReturnsFalse()
    {
        // Bottom == 0
        Assert.False(GameLogic.IsOutOfBounds(new Rect(100, -10, 4, 10), 600, 500));
    }

    [Fact]
    public void IsOutOfBounds_BulletStartsExactlyOnBottomEdge_ReturnsFalse()
    {
        // Y == fieldHeight
        Assert.False(GameLogic.IsOutOfBounds(new Rect(100, 500, 4, 10), 600, 500));
    }

    [Fact]
    public void IsOutOfBounds_FarLeft_ReturnsTrue()
    {
        Assert.True(GameLogic.IsOutOfBounds(new Rect(-20, 100, 4, 10), 600, 500));
    }

    [Fact]
    public void IsOutOfBounds_FarRight_ReturnsTrue()
    {
        Assert.True(GameLogic.IsOutOfBounds(new Rect(620, 100, 4, 10), 600, 500));
    }

    [Fact]
    public void IsOutOfBounds_BulletTouchesLeftEdgeFromOutside_ReturnsFalse()
    {
        // Right == 0
        Assert.False(GameLogic.IsOutOfBounds(new Rect(-4, 100, 4, 10), 600, 500));
    }

    [Fact]
    public void IsOutOfBounds_BulletStartsExactlyOnRightEdge_ReturnsFalse()
    {
        // X == fieldWidth
        Assert.False(GameLogic.IsOutOfBounds(new Rect(600, 100, 4, 10), 600, 500));
    }

    // ---------- LoseLife / IsGameOver ----------

    [Fact]
    public void LoseLife_NoLivesLeft_StaysAtZero()
    {
        Assert.Equal(0, GameLogic.LoseLife(0));
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(1, false)]
    public void IsGameOver_AroundZero(int lives, bool expected)
    {
        Assert.Equal(expected, GameLogic.IsGameOver(lives));
    }

    // ---------- GetLevelSpeedMultiplier ----------

    [Theory]
    [InlineData(21, 3.0)]
    [InlineData(22, 3.0)]
    [InlineData(100, 3.0)]
    public void GetLevelSpeedMultiplier_IsCappedAtMaximum(int level, double expected)
    {
        Assert.Equal(expected, GameLogic.GetLevelSpeedMultiplier(level), 6);
    }

    [Fact]
    public void GetLevelSpeedMultiplier_JustBelowCap_IsNotCapped()
    {
        Assert.Equal(2.9, GameLogic.GetLevelSpeedMultiplier(20), 6);
    }
}
