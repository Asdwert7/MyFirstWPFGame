# Что сломать для мутационного тестирования

Исходники в `GameCore/` и `EnemyAI/` **не изменены**. В этой папке лежат уже «сломанные» копии
(`GameLogic.cs`, `EnemyBrain.cs`) — каждая правка помечена комментарием `// MUTANT N`.

## Как применить
```bash
cp GameCore/GameLogic.cs   /tmp/GameLogic.orig.cs   && cp damaged/GameLogic.cs   GameCore/GameLogic.cs
cp EnemyAI/EnemyBrain.cs   /tmp/EnemyBrain.orig.cs  && cp damaged/EnemyBrain.cs  EnemyAI/EnemyBrain.cs
dotnet test        # должны падать тесты
# вернуть как было:
cp /tmp/GameLogic.orig.cs GameCore/GameLogic.cs && cp /tmp/EnemyBrain.orig.cs EnemyAI/EnemyBrain.cs
```
(Либо `git stash`/`git checkout`, но файлы в репозитории пока не закоммичены — лучше копия.)
Можно применять мутанты по одному, правя вручную по таблицам ниже.

## GameCore/GameLogic.cs
| # | Функция | Было | Стало |
|---|---------|------|-------|
| 1 | MovePlayer | `fieldWidth - playerWidth` | `fieldWidth` |
| 2 | MovePlayer | `speed < 0` | `speed <= 0` |
| 3 | IsColliding | `a.X < b.Right` | `a.X <= b.Right` |
| 4 | GetEnemyScore | `Crab => 20` | `Crab => 25` |
| 5 | CanShoot | `>= cooldown` | `> cooldown` |
| 6 | IsOutOfBounds | `r.Bottom < 0` | `r.Bottom <= 0` |
| 7 | LoseLife | `lives > 0 ? lives - 1 : 0` | `lives - 1` |
| 8 | IsGameOver | `lives <= 0` | `lives < 0` |
| 9 | GetLevelSpeedMultiplier | `0.1 * (level - 1)` | `0.1 * level` |

## EnemyAI/EnemyBrain.cs
| # | Функция | Было | Стало |
|---|---------|------|-------|
| 1 | GetFrontLine | `OrderByDescending(e => e.Y)` | `OrderBy(e => e.Y)` |
| 2 | IsAtEdge | `>= fieldWidth - margin` | `> fieldWidth - margin` |
| 3 | NextDirection | `current == Right ? Left : Right` | `current` |
| 4 | GetFormationSpeed | `3.0 * killedRatio` | `2.0 * killedRatio` |
| 5 | GetShootChance | `Math.Min(...)` | `Math.Max(...)` |
| 6 | ChooseShooter | `roll >= shootChance` | `roll > shootChance` |
| 7 | ChooseShooter | `index >= front.Count` | `index > front.Count` |
| 8 | HasReachedPlayer | `>= playerTop` | `> playerTop` |
| 9 | StepFormation | `(int)direction * speed` | `speed` |

Ожидание: тесты должны «убивать» мутантов (падать). Мутант, при котором все тесты проходят, — выжил, нужен новый тест.
