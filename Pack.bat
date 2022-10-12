@echo off
echo BUILD Cadmus packages
del .\Cadmus.Cli.Core\bin\Debug\*.*nupkg

cd .\Cadmus.Cli.Core
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..
pause
