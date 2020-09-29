@echo off
echo UPDATE PLUGINS

set target=cadmus-tool\bin\Debug\netcoreapp3.1\plugins\

md %target%
del %target%*.* /q

xcopy ..\Cadmus\Cadmus.Parts\bin\Debug\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus\Cadmus.Lexicon.Parts\bin\Debug\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus\Cadmus.Philology.Parts\bin\Debug\netstandard2.0\*.dll %target% /y

xcopy ..\Cadmus\Cadmus.Seed.Parts\bin\Debug\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus\Cadmus.Seed.Philology.Parts\bin\Debug\netstandard2.0\*.dll %target% /y

pause
