@echo off
REM This batch is used in my workstation to collect plugins and profiles
REM from several Cadmus solutions.
REM Please notice that when compiling the libraries to load as plugins,
REM you must ensure to include all their dependencies. The safest way is
REM publishing the library into a directory and then use this directory as
REM the source in this batch.
echo UPDATE PLUGINS
set target=.\cadmus-tool\bin\Debug\net8.0\plugins

md %target%
del %target%\*.* /q
REM Itinera
xcopy ..\Itinera\CadmusItinera\Cadmus.Itinera.Services\bin\Debug\net8.0\publish\*.* %target%\Cadmus.Itinera.Services\ /y
REM Pura
xcopy ..\Pura\CadmusPuraApi\CadmusPuraApi\wwwroot\seed-profile.json %target%\Cadmus.Pura.Services\ /y
xcopy ..\Tgr\CadmusTgr\Cadmus.Seed.Tgr.Parts\bin\Debug\net8.0\*.* %target%\Cadmus.Pura.Services\ /y
xcopy ..\Pura\CadmusPura\Cadmus.Pura.Services\bin\Debug\net8.0\*.* %target%\Cadmus.Pura.Services\ /y
pause
REM Renovella
xcopy ..\Renovella\CadmusRenovellaApi\CadmusRenovellaApi\wwwroot\seed-profile.json %target%\Cadmus.Renovella.Services\ /y
xcopy ..\CadmusBricks\Cadmus.Refs.Bricks\bin\Debug\net8.0\*.* %target%\Cadmus.Renovella.Services\ /y
xcopy ..\Renovella\CadmusRenovella\Cadmus.Renovella.Services\bin\Debug\net8.0\*.* %target%\Cadmus.Renovella.Services\ /y
pause
REM Tgr
xcopy ..\Tgr\CadmusTgrApi\CadmusTgrApi\wwwroot\seed-profile.json %target%\Cadmus.Tgr.Services\ /y
xcopy ..\CadmusBricks\Cadmus.Refs.Bricks\bin\Debug\net8.0\*.* %target%\Cadmus.Tgr.Services\ /y
xcopy ..\Tgr\CadmusTgr\Cadmus.Tgr.Services\bin\Debug\net8.0\*.* %target%\Cadmus.Tgr.Services\ /y
REM VeLA
xcopy ..\Vela\CadmusVelaApi\CadmusVelaApi\wwwroot\seed-profile.json %target%\Cadmus.Vela.Services\ /y
xcopy ..\CadmusBricks\Cadmus.Refs.Bricks\bin\Debug\net8.0\*.* %target%\Cadmus.Vela.Services\ /y
xcopy ..\Vela\CadmusVela\Cadmus.Vela.Services\bin\Debug\net8.0\*.* %target%\Cadmus.Vela.Services\ /y

pause
