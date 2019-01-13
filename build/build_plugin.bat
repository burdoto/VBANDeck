@echo off

goto :cleanup
goto :buildExe
if [%1]=="-r" (
    goto :copyRelease
) else (
    goto :copyDebug
)
goto :craftPlugin
goto :end

:cleanup
del /s /q "de.kaleidox.vbandeck.streamDeckPlugin"
del /s /q "de.kaleidox.vbandeck.sdPlugin"
copy "../tree/" "./de.kaleidox.vbandeck.sdPlugin/"

:buildExe
echo Building executables ...
cd "../VBANDeck"
call build_exe.bat %1

:copyRelease
copy "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Release\netcoreapp2.2\win10-x64\VBANDeck.exe" "../build/de.kaleidox.vbandeck.sdPlugin/"
del /s /q "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Release\netcoreapp2.2\ubuntu.16.10-x64"
del /s /q "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Release\netcoreapp2.2\win10-x64"
cd "../build"

:copyDebug
copy "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Debug\netcoreapp2.2\win10-x64\VBANDeck.exe" "../build/de.kaleidox.vbandeck.sdPlugin/"
del /s /q "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Debug\netcoreapp2.2\ubuntu.16.10-x64"
del /s /q "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Debug\netcoreapp2.2\win10-x64"
cd "../build"

:craftPlugin
DistributionTool.exe de.kaleidox.vbandeck.sdPlugin .

:end
echo Done.
