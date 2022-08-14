@echo off
REM This batch is used in my workstation to collect plugins and profiles
REM from several Cadmus solutions.
echo UPDATE PLUGINS
set target=.\cadmus-tool\bin\Debug\net6.0\plugins

md %target%
del %target%\*.* /q
REM Mqdq
xcopy ..\Mqdq\CadmusMqdq\Cadmus.Cli.Plugin.Mqdq\bin\Debug\net6.0\*.* %target%\Cadmus.Cli.Plugin.Mqdq\ /y
xcopy ..\Mqdq\CadmusMqdqApi\CadmusMqdqApi\wwwroot\seed-profile.json %target%\Cadmus.Cli.Plugin.Mqdq\ /y
pause
REM Pura
xcopy ..\Pura\CadmusPura\Cadmus.Cli.Plugin.Pura\bin\Debug\net6.0\*.* %target%\Cadmus.Cli.Plugin.Pura\ /y
xcopy ..\Pura\CadmusPuraApi\CadmusPuraApi\wwwroot\seed-profile.json %target%\Cadmus.Cli.Plugin.Pura\ /y
xcopy ..\Tgr\CadmusTgr\Cadmus.Seed.Tgr.Parts\bin\Debug\net6.0\ %target%\Cadmus.Cli.Plugin.Pura\ /y
pause
REM Renovella
xcopy ..\CadmusBricks\Cadmus.Refs.Bricks\bin\Debug\net6.0\*.* %target%\Cadmus.Cli.Plugin.Renovella\ /y
xcopy ..\Renovella\CadmusRenovella\Cadmus.Cli.Plugin.Renovella\bin\Debug\net6.0\*.* %target%\Cadmus.Cli.Plugin.Renovella\ /y
xcopy ..\Renovella\CadmusRenovellaApi\CadmusRenovellaApi\wwwroot\seed-profile.json %target%\Cadmus.Cli.Plugin.Renovella\ /y
pause
REM Tgr
xcopy ..\CadmusBricks\Cadmus.Refs.Bricks\bin\Debug\net6.0\*.* %target%\Cadmus.Cli.Plugin.Tgr\ /y
xcopy ..\Tgr\CadmusTgr\Cadmus.Cli.Plugin.Tgr\bin\Debug\net6.0\*.* %target%\Cadmus.Cli.Plugin.Tgr\ /y
xcopy ..\Tgr\CadmusTgrApi\CadmusTgrApi\wwwroot\seed-profile.json %target%\Cadmus.Cli.Plugin.Tgr\ /y
pause
