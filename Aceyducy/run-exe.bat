@echo off
setlocal
cd /d "%~dp0"

rem Find the csproj in this folder
for %%f in (*.csproj) do set "CSPROJ=%%~nxf"
if not defined CSPROJ (
  echo No .csproj found in this folder.
  pause
  exit /b 1
)
set "APPNAME=%CSPROJ:.csproj=%"

rem Pick a Windows RID
set "RID=win-x64"
if /i "%PROCESSOR_ARCHITECTURE%"=="ARM64" set "RID=win-arm64"

echo Publishing self-contained single-file for %RID%...
dotnet publish "%CSPROJ%" -c Release -r %RID% --self-contained true -p:PublishSingleFile=true -nologo
if errorlevel 1 (
  echo Publish failed.
  pause
  exit /b 1
)

rem Locate the published exe (don't assume net8/net9)
set "EXE="
for /f "delims=" %%p in ('dir /b /s "bin\Release\*\%RID%\publish\%APPNAME%.exe" 2^>nul') do set "EXE=%%~fp"
if not defined EXE (
  echo Could not find the published exe.
  echo Look under bin\Release\{tfm}\%RID%\publish\
  pause
  exit /b 1
)

echo.
echo Running: "%EXE%"
"%EXE%"
exit /b %ERRORLEVEL%
