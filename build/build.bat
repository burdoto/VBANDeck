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
call build_exe.bat
cd "../build"

:copyExe
xcopy ..\VBANDeck\bin\Release\net5.0\win-x86 .\de.kaleidox.vbandeck.sdPlugin\ /S /Y
del /s /q "..\VBANDeck\bin\Release\net5.0\win-x86"

:craftPlugin
DistributionTool.exe de.kaleidox.vbandeck.sdPlugin .

:end
del /s /q "de.kaleidox.vbandeck.sdPlugin"
echo Done.
