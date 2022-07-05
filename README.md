
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

# Requisite:
[.Net Core 3.1.7](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.7-windows-x64-installer)

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
	 - Unsubscribe from steammods.
	 - logging subscribed mods on folder.
 
# Road Map

<a href="https://trello.com/b/Zs2CHqSR/kenshi-mod-manager"> Click here </a>
    
# Conflict Checker
       Click on "Show conflicts", but beware it is slow if there's alot of mods, i did my best to optimize but there's alot of work to be done.
	   
# Index mods 
       this feature write the data from mod to JSON format

# Licence

Feel free to modify this project, don't forget to credit me :)

# Donation

<p align="center">
        <a href ="https://www.buymeacoffee.com/MMillerD">
         <img src="https://github.com/millerscout/Kenshi-Mod-Manager/raw/master/Donation.png" alt="Buy me a coffee" style="max-width:100%;height: 50px">
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
