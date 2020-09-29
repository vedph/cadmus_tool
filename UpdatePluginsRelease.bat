@echo off
echo UPDATE PLUGINS

set target=cadmus-tool\bin\Release\netcoreapp3.1\plugins\

md %target%
del %target%*.* /q

xcopy ..\Cadmus2\Cadmus.Parts\bin\Release\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus2\Cadmus.Lexicon.Parts\bin\Release\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus2\Cadmus.Philology.Parts\bin\Release\netstandard2.0\*.dll %target% /y

xcopy ..\Cadmus\Cadmus.Seed.Parts\bin\Release\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus\Cadmus.Seed.Philology.Parts\bin\Release\netstandard2.0\*.dll %target% /y

pause
