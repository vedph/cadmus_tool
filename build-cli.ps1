cd .\cadmus-tool

# Publish framework-dependent (any OS)
dotnet publish -c Release
compress-archive -path .\bin\Release\net10.0\publish\* -DestinationPath .\bin\Release\cadmus-tool-any.zip -Force

# Publish per-RID (framework-dependent)
dotnet publish -c Release -r win-x64 --self-contained false
dotnet publish -c Release -r linux-x64 --self-contained false
dotnet publish -c Release -r osx-x64 --self-contained false

# Zip each RID output
compress-archive -path .\bin\Release\net10.0\win-x64\publish\* -DestinationPath .\bin\Release\cadmus-tool-win-x64.zip -Force
compress-archive -path .\bin\Release\net10.0\linux-x64\publish\* -DestinationPath .\bin\Release\cadmus-tool-linux-x64.zip -Force
compress-archive -path .\bin\Release\net10.0\osx-x64\publish\* -DestinationPath .\bin\Release\cadmus-tool-osx-x64.zip -Force
