param (
    [int[]]$Ports = @(5189, 5173)
)

Write-Host "Searching for processes holding ports: $($Ports -join ', ')..." -ForegroundColor Cyan

foreach ($Port in $Ports) {
    # Find process IDs listening on the port
    $Connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue

    if ($Connections) {
        foreach ($Conn in $Connections) {
            $PIDToKill = $Conn.OwningProcess
            try {
                $Process = Get-Process -Id $PIDToKill -ErrorAction SilentlyContinue
                if ($Process) {
                    Write-Host "Killing process $($Process.ProcessName) (PID: $PIDToKill) on port $Port..." -ForegroundColor Yellow
                    Stop-Process -Id $PIDToKill -Force
                    Write-Host "Successfully killed PID $PIDToKill." -ForegroundColor Green
                }
            } catch {
                Write-Host "Failed to kill process $PIDToKill on port $Port. Please run as Administrator." -ForegroundColor Red
            }
        }
    } else {
        Write-Host "No process found listening on port $Port." -ForegroundColor DarkGray
    }
}
