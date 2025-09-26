@echo off
echo =====================================================
echo Chuhuiv Weather App v1.0.0 - Portable Release
echo =====================================================
echo.
echo Starting Chuhuiv Weather App...
echo.
echo If this is your first time running the app:
echo - The app will fetch current weather data from the internet
echo - Weather data will be cached for offline viewing
echo - Auto-refresh is enabled by default (every hour)
echo.
echo Press any key to start the application...
pause >nul

start "" "ChuhuivWeather.App.exe"

echo.
echo Weather app started successfully!
echo You can close this window now.
echo.
timeout /t 3 >nul