@echo off
goto :Release

:Debug
dotnet publish -c Debug -r win-x86

:Release
dotnet publish -c Release -r win-x86
