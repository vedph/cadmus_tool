@echo off
REM This batch is used in my workstation to collect plugins from several Cadmus solutions.
echo UPDATE PLUGINS

set target=.\cadmus-tool\bin\Debug\net5.0\plugins\

md %target%
del %target%*.* /q

xcopy .\Cadmus.Cli.Plugin.Mqdq\bin\Debug\netstandard2.1\*.* %target%Cadmus.Cli.Plugin.Mqdq\ /y

pause
