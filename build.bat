@echo off

echo *********
echo Restoring
echo *********
dotnet restore

echo ********
echo Building
echo ********
dotnet build

echo ***************
echo Initializing DB
echo ***************
dotnet run --init true
