# Get all directories in the current location that contain 'plugin' (case-insensitive)
Push-Location ".."
$pluginDirs = Get-ChildItem -Directory | Where-Object { $_.Name -match 'plugin' }

foreach ($dir in $pluginDirs) {
    Write-Host "?? Building plugin in directory: $($dir.FullName)" -ForegroundColor Cyan

    Push-Location $dir.FullName

    try {
        dotnet build --configuration:Debug --target:Package -p:OutputDirectory="C:\Plugins\" -p:Version=0.0.0
    } catch {
        Write-Host "? Build failed in $($dir.Name): $_" -ForegroundColor Red
    }

    Pop-Location
}
