# ARK Survival Evolved Discord bot

A Discord bot for ARK Survival Evolved utilizing https://github.com/arktools to extract data from savegame-files on your server.


## Configuration in config.json
**"saveFilePath": "\<absolute .ark savegame path\>"**

Absolute path to the savegame-file (.ark) to watch for changes and extract data from.


**"clusterSavePath": "\<absolute directory path where cluster savegame-files are stored\>"**

Absolute path to the directory where cluster savegame-files are stored.


**"arktoolsExecutablePath": "Tools\\ark-tools\\ark-tools.exe"**

Relative/Absolute path to the ark-tools executable. ArkDiscordBot currently uses a custom build version that expose some additional data.


**"jsonOutputDirPath": "\<absolute directory path\>"**

Absolute path to a directory where temporary json files are to be extracted.


**"tempFileOutputDirPath": "\<absolute directory path\>"**

Absolute path to a directory where temporary binary files are stored (generated images etc.)


**"botToken": "\<discord bot api token\>"**

Discord Bot Token from https://discordapp.com/developers/.


**"steamOpenIdRelyingServiceListenPrefix":  "http://+:\<port\>/openid/"**

Which adress the local OpenID webservice will listen at.


**"steamOpenIdRedirectUri": "http://\<ip/domain\>:\<port\>/openid/"**

The external address to which Steam OpenID will redierect the user after successful or failed authentication. Must be accessible externaly. Forward port through router/firewall and allow it through Windows Firewall. Can be different from the local port if port forwarding is used.


**"googleApiKey": "\<google api key\>"**

Google API-key from https://console.developers.google.com/. Used for URL Shortener API with the Steam OpenID links.


**"steamApiKey": "\<steam api key\>"**

Steam API-key from http://steamcommunity.com/dev/. Used to request user information about linked steam users.