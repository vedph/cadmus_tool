Cadmus CLI Tool Plugins Root Folder
-----------------------------------

The plugins found here are used to get Cadmus factory providers for the CLI tool. A Cadmus factory provider plugin acts as a hub entry point for all the components to be packed in the CLI tool for a specific project.

Place your own plugins in a subfolder of this folder, naming each subfolder after the DLL plugin filename (usually Cadmus.PRJ.Services where PRJ is your project name).

For instance, plugin Cadmus.Tgr.Services.dll should be placed in a subfolder of this folder named Cadmus.Tgr.Services.
