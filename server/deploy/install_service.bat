@echo off
setlocal

set "SERVICE_NAME=lw.OpenCVDNN.PPOCR.HttpServer"
set "HOST=0.0.0.0"
set "PORT=8080"

if not "%~1"=="" set "PORT=%~1"
if not "%~2"=="" set "HOST=%~2"

set "PACKAGE_ROOT=%~dp0.."
for %%I in ("%PACKAGE_ROOT%") do set "PACKAGE_ROOT=%%~fI"
set "EXE_PATH=%PACKAGE_ROOT%\lw.OpenCVDNN.PPOCR.HttpServer.exe"

if not exist "%EXE_PATH%" (
    echo ERROR: exe not found: %EXE_PATH%
    pause
    exit /b 1
)

net session >nul 2>&1
if not "%errorlevel%"=="0" (
    echo ERROR: please run this bat as Administrator.
    pause
    exit /b 1
)

sc query "%SERVICE_NAME%" >nul 2>&1
if "%errorlevel%"=="0" (
    echo Service exists, stopping and deleting: %SERVICE_NAME%
    sc stop "%SERVICE_NAME%" >nul 2>&1
    timeout /t 2 /nobreak >nul
    sc delete "%SERVICE_NAME%"
    timeout /t 2 /nobreak >nul
)

echo Creating service: %SERVICE_NAME%
sc create "%SERVICE_NAME%" binPath= "\"%EXE_PATH%\" --service --host %HOST% --port %PORT%" DisplayName= "lw OpenCVDNN PPOCR HTTP Server" start= auto
if not "%errorlevel%"=="0" (
    echo ERROR: create service failed.
    pause
    exit /b 1
)

sc description "%SERVICE_NAME%" "OpenCV DNN PP-OCR HTTP service. Default port: %PORT%"

echo Starting service: %SERVICE_NAME%
sc start "%SERVICE_NAME%"
sc query "%SERVICE_NAME%"

echo.
echo Service installed. Open http://127.0.0.1:%PORT%/
pause
