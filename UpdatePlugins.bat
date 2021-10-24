@echo off
REM This batch is used in my workstation to collect plugins and profiles
REM from several Cadmus solutions.
echo UPDATE PLUGINS
set target=.\cadmus-tool\bin\Debug\net5.0\plugins

md %target%
del %target%\*.* /q

xcopy ..\Mqdq\CadmusMqdq\Cadmus.Cli.Plugin.Mqdq\bin\Debug\net5.0\*.* %target%\Cadmus.Cli.Plugin.Mqdq\ /y
xcopy ..\Mqdq\CadmusMqdqApi\CadmusMqdqApi\wwwroot\seed-profile.json %target%\Cadmus.Cli.Plugin.Mqdq\ /y
pause
xcopy ..\Pura\CadmusPura\Cadmus.Cli.Plugin.Pura\bin\Debug\net5.0\*.* %target%\Cadmus.Cli.Plugin.Pura\ /y
xcopy ..\Pura\CadmusPuraApi\CadmusPuraApi\wwwroot\seed-profile.json %target%\Cadmus.Cli.Plugin.Pura\ /y
pause
