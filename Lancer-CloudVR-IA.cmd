@echo off
setlocal
where py.exe >nul 2>&1
if errorlevel 1 goto python
py -3 -X utf8 -B "%~dp0tools\cloudvr-rag\launch.py" %*
goto result

:python
where python.exe >nul 2>&1
if errorlevel 1 (
    echo Python 3.9 ou plus recent est requis pour le service CloudVR.
    pause
    exit /b 1
)
python -X utf8 -B "%~dp0tools\cloudvr-rag\launch.py" %*

:result
set "CLOUDVR_LAUNCH_EXIT=%ERRORLEVEL%"
if not "%CLOUDVR_LAUNCH_EXIT%"=="0" pause
exit /b %CLOUDVR_LAUNCH_EXIT%
