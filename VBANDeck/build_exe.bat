@echo off
if [%1]=="-r" (
    goto :Release
) else (
    goto :Debug
)

:Debug
dotnet publish -c Debug -r win10-x64
dotnet publish -c Debug -r ubuntu.16.10-x64

:Release
dotnet publish -c Release -r win10-x64
dotnet publish -c Release -r ubuntu.16.10-x64
