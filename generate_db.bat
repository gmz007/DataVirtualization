@echo off
echo Checking for Python installation...

python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Python is not installed or not in PATH
    echo Please install Python and try again
    pause
    exit /b 1
)

echo Creating virtual environment...
if not exist "venv" (
    python -m venv venv
    if %errorlevel% neq 0 (
        echo Failed to create virtual environment
        pause
        exit /b 1
    )
)

echo Activating virtual environment...
call venv\Scripts\activate
if %errorlevel% neq 0 (
    echo Failed to activate virtual environment
    pause
    exit /b 1
)

echo Installing faker module in virtual environment...
pip install faker
if %errorlevel% neq 0 (
    echo Failed to install faker module
    call venv\Scripts\deactivate
    pause
    exit /b 1
)

echo Running database generation script...
python generate_db.py
if %errorlevel% neq 0 (
    echo Error running the database generation script
    call venv\Scripts\deactivate
    pause
    exit /b 1
)

echo Deactivating virtual environment...
call venv\Scripts\deactivate

echo Database generation completed successfully
pause