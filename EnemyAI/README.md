# EnemyAI — документация модуля

**Разработчик:** Студент Б. **Язык/платформа:** C# 12, .NET 8 (согласовано с тестировщиком). **Тест-фреймворк:** xUnit.

Модуль отвечает за поведение вражеского строя. Все функции — статические методы класса `EnemyAI.EnemyBrain`.
Случайные числа в модуль **не генерируются**, а передаются параметрами (`roll`, `pickRoll` ∈ [0; 1]) — поэтому поведение детерминировано.
Тип `Enemy` — неизменяемая запись: `Id, Column, Row, X, Y, Width, Height, IsAlive`. Ось Y направлена вниз.

| № | Функция | Контракт |
|---|---------|----------|
| 1 | `IReadOnlyList<Enemy> GetFrontLine(IEnumerable<Enemy> enemies)` | В каждой колонке — самый нижний (максимальный `Y`) **живой** враг; результат отсортирован по `Column`. `null` → `ArgumentNullException("enemies")`. |
| 2 | `bool IsAtEdge(IEnumerable<Enemy> enemies, Direction direction, double fieldWidth, double margin)` | Вправо: `max(X + Width) >= fieldWidth - margin`. Влево: `min(X) <= margin`. Учитываются только живые; если живых нет — `false`. |
| 3 | `Direction NextDirection(Direction current, bool atEdge)` | У края — противоположное направление, иначе прежнее. |
| 4 | `double GetFormationSpeed(int aliveCount, int totalCount, int level, double baseSpeed)` | `baseSpeed * (1 + 3 * доля_убитых) * (1 + 0.1 * (level - 1))`; `0`, если живых нет. `totalCount <= 0`, `aliveCount` вне `[0; totalCount]`, `level < 1` → `ArgumentOutOfRangeException`. |
| 5 | `double GetShootChance(int level, double baseChance = 0.01)` | `min(baseChance * (1 + 0.25 * (level - 1)), 0.1)`. `level < 1` → `ArgumentOutOfRangeException`. |
| 6 | `Enemy? ChooseShooter(IEnumerable<Enemy> enemies, double roll, double shootChance, double pickRoll)` | `pickRoll` вне `[0; 1]` → `ArgumentOutOfRangeException`. `roll >= shootChance` → `null`. Иначе враг из передней линии с индексом `(int)(pickRoll * Count)` (не больше `Count - 1`); пустая линия → `null`. |
| 7 | `bool HasReachedPlayer(IEnumerable<Enemy> enemies, double playerTop)` | `true`, если у какого-либо живого врага `Y + Height >= playerTop`. |
| 8 | `IReadOnlyList<Enemy> StepFormation(IEnumerable<Enemy> enemies, Direction direction, double speed, bool descend, double descendStep)` | `descend = true` → живые опускаются на `descendStep` (X не меняется); иначе сдвиг по X на `speed` в сторону `direction`. Мёртвые и исходные объекты не изменяются. |

Запуск тестов: `dotnet test EnemyAI.Tests`.
