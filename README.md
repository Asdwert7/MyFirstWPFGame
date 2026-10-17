# MyFirstWPFGame - модульное и мутационное тестирование

Платформа: .NET 8, xUnit, coverlet, Stryker.NET. WPF-оболочка **Только на Windows🪟**.

## Что запустить (Windows, PowerShell; на Mac см. раздел ниже)

Нужен .NET 8 SDK. Команды выполняются из корня (рядом с `MyFirstWPFGame.sln`).

```powershell
# 1. Тесты (должно быть: всего 106, пройдено 106)
dotnet test

# 2. Покрытие кода (строки/ветви в таблицу 9 отчёта), см. подробности в разделе для macOS
dotnet test --collect:"XPlat Code Coverage"

# 3. Мутационное тестирование (таблица 16 отчёта). Версия 4.8.1: последняя 5.x на .NET 8 не ставится
dotnet tool install -g dotnet-stryker --version 4.8.1

cd GameCore.Tests
dotnet stryker                           # итерация 2 (все тесты)
cd ..\EnemyAI.Tests
dotnet stryker                           # итерация 2 (все тесты)
cd ..
# итерация 1 (без Round2): в версии 4.x флага --test-case-filter нет,
# поэтому файл *Round2Tests.cs временно выносим, запускаем dotnet stryker и возвращаем файл на место
```

## Для macOS (проверено на .NET 8.0.426, Apple Silicon)

WPF-проект на Mac не собирается, поэтому тесты запускаем по проектам (или с флагом `-p:EnableWindowsTargeting=true`).
Нужен именно **.NET 8 SDK** (на одном .NET 10 тесты на net8 не запустятся). Вводите блоки **по порядку, сверху вниз**. Фильтры пишем в **одинарных** кавычках (в zsh `!` в двойных ломает команду).

**0. Подготовка (один раз): установка .NET 8 и инструментов**

```bash
setopt interactive_comments    # чтобы zsh понимал комментарии "# ..." (иначе: command not found: #)
# скачиваем установщик и ставим .NET 8 рядом с системным (в ~/.dotnet8)
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh && chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0 --install-dir $HOME/.dotnet8
# инструменты: сводка покрытия и мутационное тестирование (версия Stryker 4.8.1 — последняя, что ставится на .NET 8)
export DOTNET_ROOT=$HOME/.dotnet8 PATH=$HOME/.dotnet8:$HOME/.dotnet/tools:$PATH
dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet tool install -g dotnet-stryker --version 4.8.1
```

**Начало КАЖДОГО нового окна терминала** (без этого будет `command not found` или не тот .NET):

```bash
setopt interactive_comments
export DOTNET_ROOT=$HOME/.dotnet8 PATH=$HOME/.dotnet8:$HOME/.dotnet/tools:$PATH
cd /путь/к/MyFirstWPFGame        # папка, где лежит MyFirstWPFGame.sln (в ней все команды ниже)
dotnet --version                 # должно быть 8.x; reportgenerator и dotnet-stryker должны находиться
```

**1. Тесты (всего 106 = 49 + 57):**

```bash
# все 49 тестов модуля GameCore (GameLogicTests 24 + GameLogicRound2Tests 25) → Passed 49
dotnet test GameCore.Tests
# все 57 тестов модуля EnemyAI (EnemyBrainTests 33 + EnemyBrainRound2Tests 24) → Passed 57
dotnet test EnemyAI.Tests
# оба проекта сразу, итого 106 (флаг нужен только на Mac, чтобы не падал WPF-проект)
dotnet test -p:EnableWindowsTargeting=true
```

**2. Покрытие и проценты для таблицы 9** (две итерации; проценты берём из `Summary.txt` каждого модуля):

```bash
# ---------- ИТЕРАЦИЯ 1: только первый набор тестов (без Round2) ----------
rm -rf GameCore.Tests/TestResults EnemyAI.Tests/TestResults cov      # чистим старые результаты
# 24 теста GameCore + замер покрытия → GameCore.Tests/TestResults/.../coverage.cobertura.xml
dotnet test GameCore.Tests --collect:"XPlat Code Coverage" --filter 'FullyQualifiedName!~Round2'
# 33 теста EnemyAI + замер покрытия → EnemyAI.Tests/TestResults/.../coverage.cobertura.xml
dotnet test EnemyAI.Tests  --collect:"XPlat Code Coverage" --filter 'FullyQualifiedName!~Round2'
# сводка по каждому модулю отдельно: нужны строки "Line coverage" и "Branch coverage"
reportgenerator -reports:"GameCore.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:cov/iter1_GameCore -reporttypes:TextSummary
reportgenerator -reports:"EnemyAI.Tests/TestResults/*/coverage.cobertura.xml"  -targetdir:cov/iter1_EnemyAI  -reporttypes:TextSummary
grep -E "Line coverage|Branch coverage" cov/iter1_GameCore/Summary.txt   
grep -E "Line coverage|Branch coverage" cov/iter1_EnemyAI/Summary.txt    

# ---------- ИТЕРАЦИЯ 2: все тесты (с Round2) ----------
rm -rf GameCore.Tests/TestResults EnemyAI.Tests/TestResults          # снова чистим, чтобы итерации не смешались
# все 49 тестов GameCore + покрытие
dotnet test GameCore.Tests --collect:"XPlat Code Coverage"
# все 57 тестов EnemyAI + покрытие
dotnet test EnemyAI.Tests  --collect:"XPlat Code Coverage"
reportgenerator -reports:"GameCore.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:cov/iter2_GameCore -reporttypes:TextSummary
reportgenerator -reports:"EnemyAI.Tests/TestResults/*/coverage.cobertura.xml"  -targetdir:cov/iter2_EnemyAI  -reporttypes:TextSummary
grep -E "Line coverage|Branch coverage" cov/iter2_GameCore/Summary.txt   # ожидаемо 100% и 100%
grep -E "Line coverage|Branch coverage" cov/iter2_EnemyAI/Summary.txt    # ожидаемо 98.9% и 100%
```

**3. Красный прогон (4 упавших из 57):**

```bash
# дефект 1: MovePlayer не учитывает ширину корабля (ловит тест MovePlayer_RightBorder_ShipStaysFullyInsideField)
sed -i '' 's/double maxX = fieldWidth - playerWidth;/double maxX = fieldWidth;/' GameCore/GameLogic.cs
# дефект 2: GetFrontLine берёт верхнего врага колонки вместо нижнего (ловят 3 теста EnemyAI: GetFrontLine_... и два ChooseShooter_...)
sed -i '' 's/g.OrderByDescending(e => e.Y).First()/g.OrderBy(e => e.Y).First()/' EnemyAI/EnemyBrain.cs
# первый набор тестов GameCore (24, без Round2) на сломанном коде → Failed 1, Passed 23  (рисунок 1)
dotnet test GameCore.Tests --filter 'FullyQualifiedName!~Round2'
# первый набор тестов EnemyAI (33, без Round2) на сломанном коде → Failed 3, Passed 30  (рисунок 1)
dotnet test EnemyAI.Tests  --filter 'FullyQualifiedName!~Round2'
# чиним обратно (возвращаем оба исправления)
sed -i '' 's/double maxX = fieldWidth;/double maxX = fieldWidth - playerWidth;/' GameCore/GameLogic.cs
sed -i '' 's/g.OrderBy(e => e.Y).First()/g.OrderByDescending(e => e.Y).First()/' EnemyAI/EnemyBrain.cs
# те же наборы после исправления → 24 из 24 (рисунок 2)
dotnet test GameCore.Tests --filter 'FullyQualifiedName!~Round2'
# те же наборы после исправления → 33 из 33 (рисунок 2)
dotnet test EnemyAI.Tests  --filter 'FullyQualifiedName!~Round2'
```

**4. Stryker.** На .NET 8 последняя версия 5.x не ставится, берём 4.8.1. Флага `--test-case-filter` в 4.x нет,
поэтому для итерации 1 файл `*Round2Tests.cs` временно выносим, а после запуска возвращаем:

```bash
# (stryker уже установлен в шаге 0)

# итерация 2: Stryker мутирует GameLogic.cs, на каждом мутанте гоняет все 49 тестов GameCore
# отчёт: GameCore.Tests/StrykerOutput/<дата>/reports/mutation-report.html
(cd GameCore.Tests && dotnet stryker)      # ожидаемо 96,55 %
# итерация 2: Stryker мутирует EnemyBrain.cs, на каждом мутанте гоняет все 57 тестов EnemyAI
(cd EnemyAI.Tests  && dotnet stryker)      # ожидаемо 98,80 %

# итерация 1: те же проекты, но без Round2 (GameCore — 24 теста, EnemyAI — 33 теста)
for P in GameCore EnemyAI; do
  F=$(ls $P.Tests/*Round2Tests.cs)
  mv "$F" /tmp/                                              # убираем тесты второго раунда
  (cd $P.Tests && dotnet stryker --output StrykerOutput/iter1)  # мутации против первого набора тестов
  mv /tmp/$(basename "$F") "$F"           # обязательно вернуть файл на место
done
# ожидаемо: GameCore 75,86 %, EnemyAI 78,31 %
open GameCore.Tests/StrykerOutput/iter1/reports/mutation-report.html
```

## Папка damaged/

там поломанное
