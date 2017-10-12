# ARK Survival Evolved Companion App / Discord Bot

![Web-app Interface](https://cloud.githubusercontent.com/assets/408350/26307865/044f849a-3ef7-11e7-930f-9ac829e58d45.png)

![Discord Bot Commands](https://cloud.githubusercontent.com/assets/408350/25876839/0a91380c-3520-11e7-9172-3a7707cd4c56.png)

## Introduction

An in-game companion app for players and Discord bot for server administrators.

The application monitors and extracts data from any number of configured local ARK servers and exposes this data through a Web App, Web API and Discord Bot.

It aims to provide important functions to players: dino listings, food-status, breeding info, statistics; and server admins: rcon-commands, server managing etc. It does not enable cheating or making available data that have a considerable impact on how the game is played.

Previously the application utilized a modified version of https://github.com/Qowyn/ark-tools to extract data from savegame-files. It has since been replaced by a faster and more configurable .NET-library developed in conjunction with this application based on Qowyns work on ark-tools.

The application also utilizes creature stat data sourced from Cadons excellent ARK Smart Breeding application (https://github.com/cadon/ARKStatsExtractor).

## Latest release
### Stable

https://github.com/tsebring/ArkBot/releases

### Pre-release built from latest sources
Open as zip-archive or change extension to .zip, binaries are located under tools/.

https://www.myget.org/F/tsebring/api/v2/package/ArkDiscordBot

## Installation
**For questions/problems: open a GitHub issue or contact me on Discord (Tobias#5051).**

* Download the latest pre-built binaries (see above).
* Copy defaultconfig.json and name it config.json.
* Open config.json in a text editor and go through each setting and change according to your environment (settings are documented below).

## Documentation

**All config settings have descriptions that can be found in:**
https://github.com/tsebring/ArkBot/blob/master/ArkBot/Config.cs.

Multiple servers and clusters are configured as separate elements in the configuration. There is a legacy server/cluster configuration in the root configuration that is actively being phased out. It should always be a carbon copy of your primary server/cluster configuration.

Keys are unique identifiers used to identify each server and cluster. They must be unique, not contain spaces or any special characters and ideally be short, descriptive and easy to remember and type. 

Keys are in some cases used to indicate a relation between two or more configuration elements: i.e. each server link to a cluster by referencing a particular cluster key.

Many Discord Bot commands and Web API features make use of these unique keys. Occasionally it is part of the user interaction as is the case with the !food `<server key>` or !admin `<server key>` ... commands.

When utilizing server management features, including start-, stop-, restart- and update-server commands, the server key is used to identify a particular running process. The server key configured for a particular instance must be appended to the ShooterGameServer.exe parameter list as "-serverkey=yourserverinstancekey". An example of this is found in the defaultconfig.json file. Do not forget to append this parameter even when manually starting a server.

### Main configuration

**arktoolsExecutablePath**

Relative/Absolute path of the ark-tools executable. ArkDiscordBot currently uses a custom built version of ark-tools that expose additional data.


**jsonOutputDirPath**

The absolute path of a directory where temporary json-files are to be extracted.


**tempFileOutputDirPath**

Absolute path to a directory where temporary binary files are stored (generated images etc.)


**botToken**

Discord Bot Token from https://discordapp.com/developers/.


**steamOpenIdRelyingServiceListenPrefix**

The address the local OpenID web service will listen to.


**steamOpenIdRedirectUri**

The external address to which Steam OpenID will redirect the user after successful or failed authentication. Must be accessible externally. Forward port through router/firewall and allow it through Windows Firewall. Can be different from the local port if port forwarding is used.


**googleApiKey**

Google API-key from https://console.developers.google.com/. Used for URL Shortener API with the Steam OpenID links.


**steamApiKey**

Steam API-key from http://steamcommunity.com/dev/. Used to request user information about linked steam users.


### Server instance configuration

**saveFilePath**

The absolute path of a savegame-file (.ark) to watch for changes and extract data from.


### Cluster instance configuration

**savePath**

The absolute path of the directory where cluster-files are stored. Cluster-files are extracted as part of the server update process triggered by savegame-file (.ark) to watchers.

## Web App

n in-game companion app built on top of the Web API and implemented in Angular (https://angular.io/). 

Features server status, server details, online player listing, per server player-/tribe listings, individual player profile with character- and creature information, including food-status, mating cooldowns, baby age and cuddle timers, breeding info, generator status, crop status, tribe logs and more. 

Url: `webAppListenPrefix`
Admin url: `webAppListenPrefix`/admin/`serverKey`

## Web API

RESTful API for accessing exported ARK Server save data via HTTP in JSON- or XML-format. A SignalR hub push server update notifications to connected clients in real-time.

The prebuilt web-app included in this release is by default configured to call the web api on 127.0.0.1:60001. If you want to use another port for the web api you will need to reflect this change in environment.prod.ts and rebuild the web-app dist manually using `ng build --prod --bh /`.

### Endpoints (base path is configured in `webApiListenPrefix`)

/api/map/`mapName`: ARK topographic maps for (TheIsland, TheCenter, ScorchedEarth_P and Ragnarok) sourced from ARK Survival Evolved Wiki (http://ark.gamepedia.com).

/api/player/`steamId`: Player data for player identified by `steamId` from each configured server instance.

/api/server/`serverKey`: Player and tribe listing from each configured server instance.

/api/adminserver/`serverKey`: Player and tribe listing with additional creature and structure counts from each configured server instance.

/api/structures/`serverKey`: Clustered structure data used to show the location of tribes/structures in the ARK.

With [ARK-Server-Beyond-API](https://github.com/tsebring/ARK-Server-Beyond-API) and [ImprovedCommands](https://github.com/tsebring/ImprovedCommands) administrators may remotely destroy and clean-up old structures and tamed creatures in the ARK.

/api/servers: User-, access control- and server status information including active players and statistics for each configured server instance.

/api/administer/`...`: Rcon and other commands exposed through the web app.

/api/authentication/`...`: Authenticate players using Steam.

/signalr/ (hub name `ServerUpdateHub`): Server update notifications using SignalR.