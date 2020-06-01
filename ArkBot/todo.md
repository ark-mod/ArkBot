Notes
-------------------------------------------------------------------------------
Commented out code marked with //TODO [.NET Core] and can be filtered out in the tasks view.



Minor things
-------------------------------------------------------------------------------
- Web App
  - ~~Update SignalR to work again. Need to update the node package [npm install @microsoft/signalr] (https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr-typescript-webpack?view=aspnetcore-3.1&tabs=visual-studio)~~
  - ~~Restore notification manager~~
  - ~~Make sure access control works as intended (probably not enabled server-side anymore since AccessControlAuthorizationFilter is commented out)~~
  - ~~Restore SSL renewal~~
  - Restore exception logging for web api
  - ~~Remove web api endpoint configuration options (web app and web api now hosted together)~~
  - ~~Remove web api port handling from angular code~~
  - ~~Restore SSL redirects and the redirect endpoint configurations~~
  - ~~"Logged in as" at the top-right in the web app has no name anymore. It's null in the api so likely not getting it from steam like before.~~
- WPF
  - Restore TaskbarIcon (not built for .NET Core)
- Discord
  - Restore linksteam command
  - Restore/fix IBarebonesSteamOpenId
  - Remove debug.database command
  - Test database creation/migrations
  - Add setting for database connection string
  - Migrate linksteam database from MSSQL CE 4 to MSSQL (?)
- RCON
  - RCON does not work after switching to third party querymaster .net core port.
  - Change from async in SteamManager.cs#L57
- Restore the test project
- Remove Discord.VotingManager and other unused code
- WebApp/dist is not copied on build due to BeforeBuild target being overriden by Microsoft.Common.CurrentVersion.targets


Bugs
-------------------------------------------------------------------------------
- WPF
  - Access control settings do not save in the configuration editor (tried with pages - player)


Old Bugs
-------------------------------------------------------------------------------
- WPF
  - netsh delete does not delete all sslcerts for our application id
  - multiple certs get added to the cert store for different settings
  - if no steam api key is configured web app will not show username (except that login still works)


Volume Shadow Copy (VSS)
-------------------------------------------------------------------------------
Not related to the .NET Core upgrade but one option to avoid file conflicts with ARK would be to sync the save folder to a secondary location and to target that folder with ARK Bot.
This could be done with a third party backup software, or with the .NET Wrapper of VSS from alphaleonis/AlphaVSS on the link below.
https://github.com/alphaleonis/AlphaVSS-Samples/blob/develop/src/VssBackup/Examples.cs


Extended WPF Toolkit
-------------------------------------------------------------------------------
Currently relying on an unofficial port of the library that lets us use it in .NET Core
v4.0 will include official multitargeting support for .NET Core 3 (beginning of February 2020 for Plus users - community users one version behind)



Moving all third party files to /bin directory
-------------------------------------------------------------------------------
There is no current alternative to probing privatePath in .NET Core.

runtimeOptions.additionalProbingPaths in runtimeconfig.template.json rather than
<runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <probing privatePath="lib;libs" />
    </assemblyBinding>
</runtime>

var settings = new CefSettings()
{
    BrowserSubprocessPath = "bin\\CefSharp.BrowserSubprocess.exe",
};

.csproj
<PropertyGroup>
    <CefSharpTargetDir>$(TargetDir)\bin</CefSharpTargetDir>
</PropertyGroup>

This will change the output directory of all packages etc.
<ItemDefinitionGroup>
    <ReferenceCopyLocalPaths>
        <DestinationSubDirectory>bin\</DestinationSubDirectory>
    </ReferenceCopyLocalPaths>
</ItemDefinitionGroup>

There is no current alternative to probing privatePath in .NET Core
https://github.com/dotnet/cli/issues/11713


https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/default-probing
AppDomain.CurrentDomain.AssemblyResolve