@echo off

goto :cleanup
goto :copyTree
goto :buildExe
goto :copyExe
goto :craftPlugin
goto :end

:cleanup
del /s /q "de.kaleidox.vbandeck.streamDeckPlugin"
del /s /q "de.kaleidox.vbandeck.sdPlugin"

:copyTree
xcopy ..\tree .\de.kaleidox.vbandeck.sdPlugin\ /S /Y

:buildExe
echo Building executables ...
cd "../VBANDeck"
call build_exe.bat %1
cd "../build"

:copyExe
copy "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Release\netcoreapp2.2\win-x86\VBANDeck.exe" "../build/de.kaleidox.vbandeck.sdPlugin/"
del /s /q "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Release\netcoreapp2.2\ubuntu.16.10-x64"
del /s /q "A:\Workspaces\VBAN-StreamDeck-Plugin\VBANDeck\bin\Release\netcoreapp2.2\win-x86"

:craftPlugin
DistributionTool.exe de.kaleidox.vbandeck.sdPlugin .

:end
del /s /q "de.kaleidox.vbandeck.sdPlugin"
echo Done.
