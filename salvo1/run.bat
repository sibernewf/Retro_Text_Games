@echo off
setlocal
cd /d "%~dp0"

REM Build once so errors are visible
dotnet build -nologo
if errorlevel 1 (
  echo.
  echo Build failed. Press any key to close...
  pause >nul
  exit /b 1
)

REM Run without rebuilding, and keep the window open afterward
REM dotnet run --no-build
dotnet run -c Release
echo.
echo (Finished) Press any key to close...
pause >nul
