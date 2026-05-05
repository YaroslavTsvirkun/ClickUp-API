param(
    [Parameter(Mandatory)]
    [string] $ListId,

    [string] $Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$targetFrameworks = if ($PSVersionTable.PSEdition -eq 'Desktop') {
    @('net48', 'netstandard2.0', 'net10.0', 'net9.0', 'net8.0')
}
else {
    @('net10.0', 'net9.0', 'net8.0', 'netstandard2.0', 'net48')
}
$dllDir = $targetFrameworks |
    ForEach-Object { Join-Path $repoRoot "ClickUp.Client\bin\$Configuration\$_" } |
    Where-Object { Test-Path -LiteralPath (Join-Path $_ 'ClickUp.Client.dll') } |
    Select-Object -First 1

if (-not $dllDir) {
    throw "Build the library first: dotnet build `"$repoRoot\ClickUp.Client\ClickUp.Client.csproj`" -c $Configuration"
}

$dllPath = Join-Path $dllDir 'ClickUp.Client.dll'

if (-not (Test-Path -LiteralPath $dllPath)) {
    throw "Build the library first: dotnet build `"$repoRoot\ClickUp.Client\ClickUp.Client.csproj`" -c $Configuration"
}

if (-not [Environment]::GetEnvironmentVariable('CLICKUP_API_TOKEN')) {
    throw 'Set CLICKUP_API_TOKEN before calling ClickUp.'
}

$loadedAssemblyNames = [AppDomain]::CurrentDomain.GetAssemblies() |
    ForEach-Object { $_.GetName().Name }

Get-ChildItem -LiteralPath $dllDir -Filter '*.dll' |
    Where-Object { $_.Name -ne 'ClickUp.Client.dll' } |
    Where-Object { $loadedAssemblyNames -notcontains [System.IO.Path]::GetFileNameWithoutExtension($_.Name) } |
    ForEach-Object {
        Add-Type -Path $_.FullName
    }

if ($loadedAssemblyNames -notcontains 'ClickUp.Client') {
    Add-Type -Path $dllPath
}

$client = [ClickUp.Client.ClickUpClient]::FromEnvironment()

try {
    $tasks = $client.Tasks.GetAllTasksJson($ListId) | ConvertFrom-Json

    $tasks |
        Select-Object id, name, url, @{ Name = 'status'; Expression = { $_.status.status } }
}
finally {
    $client.Dispose()
}
