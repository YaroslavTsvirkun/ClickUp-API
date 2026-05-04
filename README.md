# ClickUp.Client

`ClickUp.Client` — це .NET-бібліотека для роботи з ClickUp API v2 з PowerShell-скриптів та C#-коду. Вона побудована поверх Refit, тому HTTP-контракти описані декларативно, а не через ручне складання `HttpRequestMessage`.

Основний сценарій — автоматизація ClickUp з PowerShell: читання задач, перегляд backlog-ів, створення задач, оновлення статусів, додавання коментарів і робота з тегами. Методи повертають JSON-рядки, які у PowerShell зручно передавати в `ConvertFrom-Json`.

## Можливості

- Категорії endpoint-ів повторюють структуру ClickUp Reference: `Users`, `Tasks`, `Tags`, `Lists`, `Spaces`, `Folders`, `Comments`.
- Для PowerShell є простий facade `ClickUpClient` з властивостями `$client.Tasks`, `$client.Tags`, `$client.Users` тощо.
- Для C# є DI-реєстрація через `AddClickUpApiClient`.
- Авторизація виконується через raw `Authorization` header з `CLICKUP_API_TOKEN`.
- Є retry для `429`, `5xx` і тимчасових транспортних помилок.
- Необроблені endpoint-и можна викликати через `$client.Raw`.
- Тести винесені в окремий xUnit v3 проєкт і не виконують реальних запитів до ClickUp.

## Структура

```text
D:\Code\ClickUp API
├─ ClickUp.Client.slnx
├─ NuGet.config
├─ ClickUp.Client
│  ├─ Abstractions      # Refit-контракти API, розбиті за категоріями ClickUp
│  ├─ Categories        # PowerShell-friendly клієнти: Tasks, Tags, Users...
│  ├─ Infrastructure    # auth, retry, Refit/HttpClient factory, error mapping
│  ├─ Models            # DTO для request payloads
│  └─ ClickUpClient.cs  # головний facade
├─ ClickUp.Client.Tests
│  └─ xUnit v3 тести без реальних HTTP-запитів
├─ examples
│  └─ List-ClickUpTasks.ps1
```

## Вимоги

- .NET SDK 8 або новіший.
- PowerShell 7 або новіший для скриптів.
- ClickUp personal API token у змінній середовища `CLICKUP_API_TOKEN`.

Токен передається в ClickUp як сире значення заголовка `Authorization`. Префікс `Bearer` додавати не потрібно.

## Швидкий старт

Зібрати solution:

```powershell
dotnet build .\ClickUp.Client.slnx -c Release
```

Задати токен:

```powershell
$env:CLICKUP_API_TOKEN = 'pk_your_personal_token'
```

Підключити бібліотеку в PowerShell:

```powershell
$dllDir = '.\ClickUp.Client\bin\Release\net8.0'

Get-ChildItem -LiteralPath $dllDir -Filter '*.dll' |
    Where-Object { $_.Name -ne 'ClickUp.Client.dll' } |
    ForEach-Object {
        Add-Type -Path $_.FullName
    }

Add-Type -Path (Join-Path $dllDir 'ClickUp.Client.dll')
```

Отримати задачі зі списку:

```powershell
$client = [ClickUp.Client.ClickUpClient]::FromEnvironment()

try {
    $tasks = $client.Tasks.GetAllTasksJson('<list_id>') | ConvertFrom-Json

    $tasks |
        Select-Object id, name, url, @{ Name = 'status'; Expression = { $_.status.status } }
}
finally {
    $client.Dispose()
}
```

Після build NuGet-залежності, включно з `Refit.dll`, копіюються в `ClickUp.Client\bin\Release\net8.0`. Саме тому в PowerShell потрібно завантажувати всі DLL з цієї папки, а потім `ClickUp.Client.dll`.

## Категорії API

Публічний API згрупований як у документації ClickUp:

| Властивість | Призначення |
| --- | --- |
| `$client.Users` | Поточний користувач і user endpoint-и |
| `$client.Workspaces` | Workspace/team endpoint-и ClickUp API v2 |
| `$client.Spaces` | Space endpoint-и |
| `$client.Folders` | Folder endpoint-и |
| `$client.Lists` | List endpoint-и |
| `$client.Tasks` | Task endpoint-и |
| `$client.Tags` | Tag endpoint-и |
| `$client.Comments` | Comment endpoint-и |
| `$client.Raw` | Виклики endpoint-ів, для яких ще немає typed wrapper |

Приклади:

```powershell
$client.Users.GetAuthorizedUserJson() | ConvertFrom-Json
$client.Workspaces.GetWorkspacesJson() | ConvertFrom-Json
$client.Spaces.GetSpacesJson('<workspace_or_team_id>') | ConvertFrom-Json
$client.Lists.GetFolderlessListsJson('<space_id>') | ConvertFrom-Json
$client.Tasks.GetTaskJson('<task_id>') | ConvertFrom-Json
$client.Tags.GetSpaceTagsJson('<space_id>') | ConvertFrom-Json
```

## Типові операції

Створити задачу:

```powershell
$taskBody = @{
    name = 'Review ClickUp client'
    description = 'Created from PowerShell through ClickUp.Client.'
    priority = 3
} | ConvertTo-Json -Depth 10

$task = $client.Tasks.CreateTaskJson('<list_id>', $taskBody) | ConvertFrom-Json
```

Оновити статус:

```powershell
$client.Tasks.SetTaskStatusJson('<task_id>', 'in progress') | Out-Null
```

Призначити виконавця і змінити статус:

```powershell
$client.Tasks.AssignTaskAndSetStatusJson('<task_id>', 'in progress', [long[]]@(12345678)) | Out-Null
```

Додати коментар:

```powershell
$comment = @'
Виконано:
- Оновлено через PowerShell script.

Перевірки:
- ClickUp API call completed.

Залишкові ризики:
- не виявлено.
'@

$client.Comments.AddTaskCommentJson('<task_id>', $comment, $false) | Out-Null
```

Працювати з тегами:

```powershell
$client.Tags.GetSpaceTagsJson('<space_id>') | ConvertFrom-Json
$client.Tags.AddTagToTaskJson('<task_id>', 'blocked') | Out-Null
$client.Tags.RemoveTagFromTaskJson('<task_id>', 'blocked') | Out-Null
```

Викликати endpoint, якого ще немає в категоріях:

```powershell
$client.Raw.RequestJson('GET', 'team') | ConvertFrom-Json
```

## Приклад скрипта

У репозиторії є готовий приклад:

```powershell
.\examples\List-ClickUpTasks.ps1 -ListId '<list_id>'
```

Скрипт завантажує DLL, створює `ClickUpClient` з `CLICKUP_API_TOKEN`, читає задачі зі списку і виводить коротку таблицю.

## Використання з C#

Для C#-застосунків можна зареєструвати клієнт через DI:

```csharp
using ClickUp.Client;

services.AddClickUpApiClient(
    apiToken: Environment.GetEnvironmentVariable("CLICKUP_API_TOKEN")
        ?? throw new InvalidOperationException("CLICKUP_API_TOKEN is not set."));
```

Після цього можна інжектити `ClickUpClient`:

```csharp
public sealed class ClickUpTaskReader
{
    private readonly ClickUpClient _client;

    public ClickUpTaskReader(ClickUpClient client)
    {
        _client = client;
    }

    public Task<string> GetTasksAsync(string listId)
    {
        return _client.Tasks.GetAllTasksJsonAsync(listId);
    }
}
```

## Тести

Тести знаходяться в окремому проєкті:

```text
ClickUp.Client.Tests\ClickUp.Client.Tests.csproj
```

Запустити всі тести:

```powershell
dotnet test .\ClickUp.Client.slnx -c Release
```

Запустити тільки test project:

```powershell
dotnet test .\ClickUp.Client.Tests\ClickUp.Client.Tests.csproj -c Release
```

Тести використовують fake `HttpMessageHandler`, тому:

- не потрібен `CLICKUP_API_TOKEN`;
- немає реальних HTTP-запитів до ClickUp;
- перевіряються URL-и, HTTP methods, headers, JSON body, retry і категорійний facade.

## Обробка помилок

HTTP-відповіді з неуспішним статусом перетворюються на `ClickUpApiException`. У винятку доступні:

- HTTP method;
- request URI;
- status code;
- response body.

Retry виконується для:

- `429 Too Many Requests`;
- `5xx` відповідей;
- тимчасових транспортних помилок, якщо запит не був скасований через `CancellationToken`.

## Як додати новий endpoint

1. Додати Refit-метод у відповідний interface в `ClickUp.Client\Abstractions`.
2. Додати PowerShell-friendly метод у відповідний клас з `ClickUp.Client\Categories`.
3. Якщо потрібен request body, додати DTO в `ClickUp.Client\Models` або приймати JSON-рядок і перетворювати його через `ParseJson`.
4. Додати тест у `ClickUp.Client.Tests`.
5. Запустити:

```powershell
dotnet test .\ClickUp.Client.slnx -c Release
```

## Сумісність

Старі facade-методи на `ClickUpClient`, наприклад `GetAllTasksJson()` або `AddTaskCommentJson()`, залишені для сумісності з уже написаними скриптами.

Для нового коду краще використовувати категорії:

```powershell
$client.Tasks.GetAllTasksJson('<list_id>')
$client.Comments.AddTaskCommentJson('<task_id>', $comment, $false)
```

## Безпека

- Не зберігайте `CLICKUP_API_TOKEN` у коді, README, коментарях або Git.
- Передавайте токен через змінні середовища або секрети CI.
- Не виводьте headers у лог без редагування `Authorization`.
