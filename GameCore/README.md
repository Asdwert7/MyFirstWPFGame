# GameCore — документация модуля

**Разработчик:** Студент А. **Язык/платформа:** C# 12, .NET 8 (согласовано с тестировщиком). **Тест-фреймворк:** xUnit.

Модуль содержит чистую (без WPF и без состояния) игровую логику Space Invaders. Все функции — статические методы класса `GameCore.GameLogic`.
Система координат: начало в левом верхнем углу, ось Y направлена вниз.

| № | Функция | Контракт |
|---|---------|----------|
| 1 | `double MovePlayer(double x, int direction, double speed, double fieldWidth, double playerWidth)` | Новая левая координата корабля = `x + sign(direction) * speed`, ограниченная диапазоном `[0; fieldWidth - playerWidth]`. `speed < 0` → `ArgumentOutOfRangeException`. |
| 2 | `bool IsColliding(Rect a, Rect b)` | Пересечение прямоугольников (AABB). Прямоугольники, касающиеся только границей, **не** пересекаются. |
| 3 | `int GetEnemyScore(EnemyType type)` | `Squid` = 30, `Crab` = 20, `Octopus` = 10. Неизвестное значение → `ArgumentOutOfRangeException`. |
| 4 | `bool CanShoot(double now, double lastShotTime, double cooldown)` | `true`, если `now - lastShotTime >= cooldown`. |
| 5 | `bool IsOutOfBounds(Rect r, double fieldWidth, double fieldHeight)` | `true`, если прямоугольник **полностью** вне поля `[0; fieldWidth] x [0; fieldHeight]`. Касание границы снаружи вылетом не считается. |
| 6 | `int LoseLife(int lives)` | `lives - 1`, но не меньше 0. |
| 7 | `bool IsGameOver(int lives)` | `true`, если `lives <= 0`. |
| 8 | `double GetLevelSpeedMultiplier(int level)` | `min(1.0 + 0.1 * (level - 1), 3.0)`. `level < 1` → `ArgumentOutOfRangeException`. |

Запуск тестов: `dotnet test GameCore.Tests`.
