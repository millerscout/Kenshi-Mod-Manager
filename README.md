
# Kenshi Mod Manager by MillerScout

<p>This mod was created to organize mods and the auto-sort is based on this 
       <a href="https://steamcommunity.com/sharedfiles/filedetails/?id=1850250979" target="_blank">Post</a>.
</p>
<p align="center">
       <a href="https://discord.gg/g7F6aHw">
              <img width="120 "src="https://raw.githubusercontent.com/millerscout/Kenshi-Mod-Manager/assets/Releases/GenesisGuild/GMG.jpg" target="_blank">
       </a>
</p>
<p>This project is being Part of the <a href="https://discord.gg/g7F6aHw">Genesis Modding Guild</a>, you can look up for support there at #kmm-tool</p>


# How to Use?

<p>
       <a href ="https://www.youtube.com/watch?v=OT-n6t6FZb0">
         Found on Youtube from hekheler<a/> this video is a better explanation that i couldn't write properly :)
</p>
<p>Hope he don't mind sharing his video :x</p>

# Requisite:
[3.1 NetCore](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-aspnetcore-3.1.3-windows-x64-installer) or [latest](https://dotnet.microsoft.com/download/dotnet-core/current)

<p align="center">
       <img src="../assets/Releases/v1.6/print.png?raw=true">
</p>

# Features

     - Faster way to organize mods. 
     - shortcut to go to Mod's Page either (nexus/steam workshop)
     - Show Categories (Steam)
     - Toggle Active (Context Menu)
     - Active Mod (Context Menu)
     - Deactive Mod (Context Menu)
     - save profile mods (current Active and mod order)
     - load profile.
     - Search through mods using Name or SubscriptionId(steam).
     - Basic dependency detection (dependency order is not checked yet).
     - List deep changes from mods and list them for modders (alpha)
 
# Road Map

<a href="https://trello.com/b/Zs2CHqSR/kenshi-mod-manager"> Click here </a>
    
# Conflict Checker
       if you're willing to test this feature, you'll need to manually configure the config.json.
       
       on your config.json

add the line

``` json
  "ConflictAnalyzerPath": "Mod Conflict Manager.exe"
```

your config file should look like this:
``` javascript
{
  "gamePath": " {Your game path.} ",
  "SteamModsPath": " {YourSteam Folder if applicable} ",
  "SteamPageUrl": "https://steamcommunity.com/sharedfiles/filedetails/?id=",
  "NexusPageUrl": "https://www.nexusmods.com/kenshi/search/?gsearch=",
  "ConflictAnalyzerPath": "Mod Conflict Manager.exe"
}
```


# Known Bugs
       - when you trying to open and nothing happens, try to delete the "config.json" file.

# Licence

Few free to modify this project, don't forget to credit me :)

# Donation

<p align="center">
        <a href ="https://www.buymeacoffee.com/gR79MHU">
         <img src="https://github.com/millerscout/Kenshi-Mod-Manager/raw/master/Donation.png" alt="Buy me a coffee" style="max-width:100%;">
     </a>
</p>

# How generate builds (this is a reminder for me)

for minor version
``` 
 $ build-all.bat minor
 $ push
```

for major version
``` 
 $ build-all.bat major
 $ push
```
