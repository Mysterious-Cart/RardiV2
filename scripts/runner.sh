param(
    [switch]$WithUrls
)

$ErrorActionPreference = 'Stop'
$solutionRoot = Resolve-Path "$PSScriptRoot\.."

$services = @(
    @{ Name = 'SecurityService';  Project = 'SecurityService\SecurityService.csproj'; Url = 'http://localhost:7001' },
    @{ Name = 'CustomerService';  Project = 'CustomerService\CustomerService.csproj'; Url = 'http://localhost:7002' },
    @{ Name = 'InventoryService'; Project = 'InventoryService\InventoryService.csproj'; Url = 'http://localhost:7003' },
    @{ Name = 'PaymentService';   Project = 'PaymentService\PaymentService.csproj'; Url = 'http://localhost:7004' }
)

foreach ($svc in $services) {
    $projectPath = Join-Path $solutionRoot $svc.Project
    if (!(Test-Path $projectPath)) {
        Write-Warning "Project not found: $($svc.Project)"
        continue
    }

    $cmd = "Set-Location `"$solutionRoot`"; dotnet run --project `"$projectPath`""
    if ($WithUrls -and $svc.Url) {
        $cmd += " --urls $($svc.Url)"
    }

    Start-Process powershell.exe -ArgumentList '-NoExit', '-Command', $cmd
    Write-Host "Launching $($svc.Name)…"
}