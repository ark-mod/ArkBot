![Web-app Interface](https://user-images.githubusercontent.com/408350/31540442-f0cb204c-b00b-11e7-8d40-f15b445cdcd2.png)

![Discord Bot Commands](https://user-images.githubusercontent.com/408350/31518648-405ee5f6-afa0-11e7-9c50-3dfd60ecdd7a.png)

## Introduction

An in-game companion app for players and Discord bot for server administrators.

The application monitors and extracts data from any number of configured local ARK servers and exposes this data through a Web App, Web API and Discord Bot.

It aims to provide important functions to players: dino listings, food-status, breeding info, statistics; and server admins: rcon-commands, server managing etc. It does not enable cheating or making available data that have a considerable impact on how the game is played.

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

To enable map clean-up from the companion app (web app) for administrators, install [ARK-Server-Beyond-API (forked by me)](https://github.com/tsebring/ARK-Server-Beyond-API) and [ARK Beyond API: Improved Commands (Plugin)](https://github.com/tsebring/ImprovedCommands).

## Documentation from Wiki

### How to setup?

* [Configuration](https://github.com/tsebring/ArkBot/wiki/Configuration)
* [Port Forwarding](https://github.com/tsebring/ArkBot/wiki/Port-Forwarding)

### How to use?

* [Companion App (Web App)](https://github.com/tsebring/ArkBot/wiki/Companion-App-(Web-App))
* [Web API](https://github.com/tsebring/ArkBot/wiki/Web-API)
* [Discord Bot](https://github.com/tsebring/ArkBot/wiki/Discord-Bot)

## Acknowledgements

Powered by ARK Savegame Toolkit .NET based on Qowyns work on [ark-tools](https://github.com/Qowyn/ark-tools).

Creature stat data is sourced from Cadons [ARK Smart Breeding](https://github.com/cadon/ARKStatsExtractor).

## Links

#### ARK Savegame Toolkit .NET

Library for reading ARK Survival Evolved savegame files in .NET

https://github.com/tsebring/ArkSavegameToolkitNet

#### ARK-Server-Beyond-API (forked by me)

Server-side ARK plugins.

https://github.com/tsebring/ARK-Server-Beyond-API

#### ARK Beyond API: Improved Commands (Plugin)

Used to facilitate map clean-up from the companion app (web app) for administrators.

https://github.com/tsebring/ImprovedCommands

#### ARK Beyond API: Cross Server Chat (Plugin)

Used for cross-server chat support.

https://github.com/tsebring/ArkCrossServerChat

#### ARK Beyond API: Imprinting Mod (Plugin)

Used for advance imprinting/cuddle support.

https://github.com/tsebring/ImprintingMod