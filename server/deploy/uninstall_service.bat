@echo off
setlocal

set "SERVICE_NAME=lw.OpenCVDNN.PPOCR.HttpServer"

net session >nul 2>&1
if not "%errorlevel%"=="0" (
    echo ERROR: please run this bat as Administrator.
    pause
    exit /b 1
)

sc query "%SERVICE_NAME%" >nul 2>&1
if not "%errorlevel%"=="0" (
    echo Service not found: %SERVICE_NAME%
    pause
    exit /b 0
)

echo Stopping service: %SERVICE_NAME%
sc stop "%SERVICE_NAME%" >nul 2>&1
timeout /t 2 /nobreak >nul

echo Deleting service: %SERVICE_NAME%
sc delete "%SERVICE_NAME%"

echo.
echo Service deleted.
pause
