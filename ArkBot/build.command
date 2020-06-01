msbuild ArkBot.sln /t:arkbot:rebuild /p:Configuration=Release /p:Platform=x64 /p:OutputPath="C:/Temp/ArkBot/dist/"


.NET Core 3.1
-----------------------------------------------------------------------------------------------------------------------------------------------
### NOTE: Can't use dotnet msbuild - fody packages will not get stripped from deps.json and it wont run if they are not found in the global package "cache" - use dotnet publish

dotnet publish ArkBot.sln /t:arkbot:rebuild /p:Configuration=Release /p:Platform=x64 /p:OutputPath="C:\Temp\ARK\ARK Bot Temp Directory\Release x64 core msbuild\arkbot"