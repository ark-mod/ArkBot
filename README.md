#### INSTALLATION INSTRUCTIONS
First time? Check out [Installation](https://github.com/ark-mod/ArkBot/blob/master/README.md#installation)

---

![Web-app Interface](https://user-images.githubusercontent.com/408350/31540442-f0cb204c-b00b-11e7-8d40-f15b445cdcd2.png)

![Discord Bot Commands](https://user-images.githubusercontent.com/408350/31518648-405ee5f6-afa0-11e7-9c50-3dfd60ecdd7a.png)

## Live demo

* ### [Companion App (Web App)](https://ark-mod.github.io/ArkBot)
  * [Server view](https://ark-mod.github.io/ArkBot/server/server1)
  * [Player view](https://ark-mod.github.io/ArkBot/player/10000000000001888)
  * [Admin view](https://ark-mod.github.io/ArkBot/admin/server1)

## Introduction

An in-game companion app for players and Discord bot for server administrators.

The application monitors and extracts data from any number of configured local ARK servers and exposes this data through a Web App, Web API and Discord Bot.

It aims to provide important functions to players: dino listings, food-status, breeding info, statistics; and server admins: rcon-commands, server managing etc. It does not enable cheating or making available data that have a considerable impact on how the game is played.


## Installation

* Download the latest pre-built binaries (see above).
* Perform configuration within the ArkBot program after opening by clicking on the configuration tab, completing all required fields.
* Important Dependency Note: 
ArkBot relies upon certain VS C++ runtimes installed. It requires [.NET 3.5 with service pack 1](https://www.microsoft.com/en-us/download/details.aspx?id=25150), [.NET 4.7](https://www.microsoft.com/en-us/download/details.aspx?id=55170), and [Microsoft Visual C++ Redistributable 2013](https://support.microsoft.com/en-us/help/4032938/update-for-visual-c-2013-redistributable-package). Please note: If you are running a windows server install, you must install the [.NET 3.5 and 4.7(or 4.8) from the server admin panel.](https://www.interserver.net/tips/kb/enable-net-framework-3-5-windows-server/)

This has been installed on WS 2012R2, WS 2016, WS 2019, Windows 10, Windows 11. Other platforms may not work. Try it at your own risk.

To enable map clean-up from the companion app (web app) for administrators, install [ARK-Server-API](https://arkserverapi.com/resources/ark-server-api.4/) and [ARK-Server-API: ArkBotHelper Plugin by WETBATMAN](https://gameservershub.com/forums/resources/ark-bot-helper.136/).


## Documentation from Wiki

### What does it do?

*  [More Information and Help](https://github.com/ark-mod/ArkBot/wiki)

### How to setup?

* [Getting Started](https://github.com/ark-mod/ArkBot/wiki/Getting-Started)
* [Configuration](https://github.com/ark-mod/ArkBot/wiki/Configuration)
* [Port Forwarding](https://github.com/ark-mod/ArkBot/wiki/Port-Forwarding)

### How to use?

* [Companion App (Web App)](https://github.com/ark-mod/ArkBot/wiki/Companion-App-(Web-App))
* [Web API](https://github.com/ark-mod/ArkBot/wiki/Web-API)
* [Discord Bot](https://github.com/ark-mod/ArkBot/wiki/Discord-Bot)

## Acknowledgements

Powered by ARK Savegame Toolkit .NET based on Qowyns work on [ark-tools](https://github.com/Qowyn/ark-tools).

Creature stat data is sourced from Cadons [ARK Smart Breeding](https://github.com/cadon/ARKStatsExtractor).

WETBATMAN for the Ark Server API: ArtBotHelper Plugin.

Knights GG Ark Cluster for extensive testing and support throughout the years [KNIGHTSGG](https://www.knightsggc.com).

## Links

#### ARK Savegame Toolkit .NET

Library for reading ARK Survival Evolved savegame files in .NET

https://github.com/ark-mod/ArkSavegameToolkitNet

#### ARK-Server-API 

Allows server-side ARK plugins.

https://gameservershub.com/forums/resources/ark-server-api.12/

#### ARK Server API: ArkBotHelper (Plugin by WETBATMAN)

Used to facilitate map clean-up from the companion app (web app) for administrators.

https://gameservershub.com/forums/resources/ark-bot-helper.136/
