@echo off
setlocal
cd /d "%~dp0"

if "%SPTPath%"=="" set SPTPath=D:\SPT-5.0.0-47242-BE

echo ============================================================
echo  Building WeatherController.Client for SPT 5.0 (IL2CPP)
echo ============================================================
echo  SPTPath = %SPTPath%
echo.

if not exist "%SPTPath%\BepInEx\interop\Assembly-CSharp.dll" (
    echo WARNING: %SPTPath%\BepInEx\interop\Assembly-CSharp.dll not found.
    echo          Launch SPT once so BepInEx can generate the interop assemblies.
    echo.
)

dotnet build WeatherController.Client.csproj -c Release /p:SPTPath="%SPTPath%"
if errorlevel 1 (
    echo.
    echo BUILD FAILED.
    pause
    exit /b 1
)

echo.
echo BUILD SUCCEEDED.
echo Client plugin copied to %SPTPath%\BepInEx\plugins\WeatherController.Client\
pause