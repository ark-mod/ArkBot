# Project status last updated Sept 5th 2019

### Support
Support [Wiki](https://github.com/tsebring/ArkBot/wiki)


## DO NOT use GitHub as a support forum.  The issues section is reserved for just that, issues and error reporting.  If you need support with setup, have general questions, or something else not covered in the wiki, join our Discord using this [link](https://discord.gg/zDJcckF).  Be sure to read #welcome in the Discord on how to get support for ArkBot. 

Before requesting support from our Discord, please understand that ArkBot is designed in its current form to be run on the machine hosting your gameservers.  While methods exist to mirror/link files from a remote gameserver, arkbot is not designed with these workarounds in mind and as such are unsupported.  We will offer no support if you choose to go this route. 

#### Development
We have found a developer and we are currently working on bugfixes at the moment.  We currently have in our discord (link found above) a poll with what feature adds to work on next.  The top 4 suggestions will be worked on.  One of the first feature requests we will working on is adding support for any modmap server admins wish to utilize.  We will be making a user editable drag-and-drop folder for map images so all admins will need to do is place a map image in for it to work within arkbot.

#### Donate
If you wish to help support our costs for development, you can donate by simply typing "donate" (no quotes) in our discord in any channel.  Everything donated goes directly back into ArkBot.  Alternativly, you may click [here](https://donatebot.io/checkout/305511546417709058) without joining discord.

#### IMPORTANT NOTES
Important Dependency Note: 
ArkBot relies upon certain VS C++ runtimes installed. It requires [.NET 3.5 with service pack 1](https://www.microsoft.com/en-us/download/details.aspx?id=25150), [.NET 4.7](https://www.microsoft.com/en-us/download/details.aspx?id=55170), and [Microsoft Visual C++ Redistributable 2013](https://support.microsoft.com/en-us/help/4032938/update-for-visual-c-2013-redistributable-package). Please note: If you are running a windows server install, you must install the [.NET 3.5 and 4.7(or 4.8) from the server admin panel.](https://www.interserver.net/tips/kb/enable-net-framework-3-5-windows-server/)

This has been installed on WS 2012R2, WS 2016, WS 2019, and Windows 10. Other platforms may not work. Try it at your own risk. Questions? Join our discord and one of us will try to help as we can. Be sure to click the reaction to join the proper discord channels. 

#### INSTALLATION INSTRUCTIONS
First time? Check out [Installation](https://github.com/ark-mod/ArkBot/blob/master/README.md#installation)

#### Suggestions/improvments/bugs
Have a look at our [Roadmap](https://github.com/ark-mod/ArkBot/wiki/Roadmap)

If you think something is wrong with the project - help improve it. If you find a bug - help fix it. Don't know how to program? Start learning. This is the way of open source!

---

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

https://www.myget.org/F/arkbot-beta/api/v2/package/ArkBot

## Installation

* Download the latest pre-built binaries (see above).
* Perform configuration within the ArkBot program after opening by clicking on the configuration tab, completing all required fields.

To enable map clean-up from the companion app (web app) for administrators, install [ARK-Server-API](https://arkserverapi.com/resources/ark-server-api.4/) and [ARK-Server-API: ArkBotHelper Plugin by WETBATMAN](https://arkserverapi.com/resources/ark-bot-helper.142/).

## Documentation from Wiki

### What does it do?

*  [More Information](https://github.com/tsebring/ArkBot/wiki)

### How to setup?

* [Getting Started](https://github.com/tsebring/ArkBot/wiki/Getting-Started)
* [Configuration](https://github.com/tsebring/ArkBot/wiki/Configuration)
* [Port Forwarding](https://github.com/tsebring/ArkBot/wiki/Port-Forwarding)

### How to use?

* [Companion App (Web App)](https://github.com/tsebring/ArkBot/wiki/Companion-App-(Web-App))
* [Web API](https://github.com/tsebring/ArkBot/wiki/Web-API)
* [Discord Bot](https://github.com/tsebring/ArkBot/wiki/Discord-Bot) (currently not working properly and needs updating as of 9/5/2019)

## Acknowledgements

Powered by ARK Savegame Toolkit .NET based on Qowyns work on [ark-tools](https://github.com/Qowyn/ark-tools).

Creature stat data is sourced from Cadons [ARK Smart Breeding](https://github.com/cadon/ARKStatsExtractor).

## Links

#### ARK Savegame Toolkit .NET

Library for reading ARK Survival Evolved savegame files in .NET

https://github.com/tsebring/ArkSavegameToolkitNet

#### ARK-Server-API 

Allows server-side ARK plugins.

https://arkserverapi.com/resources/ark-server-api.4/

#### ARK Server API: ArkBotHelper (Plugin by WETBATMAN)

Used to facilitate map clean-up from the companion app (web app) for administrators.

https://arkserverapi.com/resources/ark-bot-helper.142/

#### ARK Beyond API: Imprinting Mod (Plugin)

Used for advance imprinting/cuddle support.

https://github.com/tsebring/ImprintingMod

#### ARK Beyond API: Modified Spawn Level Distribution (Plugin)

Used to change spawn level distribution on The Island and Scorched Earth (can be used on others as well) to match the official spawn level distribution on Ragnarok and The Center.

https://github.com/tsebring/ArkModifiedSpawnLevelDistribution
