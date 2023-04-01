#inspiration from https://gist.github.com/kumichou/acefc48476957aad6b0c9abf203c304c
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True,Position=1)]
    [string]$bumpKind
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

function UpdateVersionForUpdater($versionName){
    $zipName = $versionName;
    if($versionName -eq 'selfcontained'){ $zipName = "FullRelease"}
	echo "https://github.com/millerscout/Kenshi-Mod-Manager/releases/download/v$($newVersion)/$($zipName).zip"
    if(checkStatus("https://github.com/millerscout/Kenshi-Mod-Manager/releases/download/v$($newVersion)/$($zipName).zip")==1){
        $xml = Get-Content -Path "updatelist-$($versionName).xml"
        $xml = $xml -replace "<!-- NEXT VERSION -->", "<!-- NEXT VERSION -->`n<item>
    <version>$($newVersion).0.0</version>
    <url>https://github.com/millerscout/Kenshi-Mod-Manager/releases/download/v$($newVersion)/$($zipName).zip</url>
    <changelog>https://github.com/millerscout/Kenshi-Mod-Manager/releases/tag/v$($newVersion)</changelog> 
</item>"
        
    Set-Content -Path "updatelist-$($versionName).xml" -Value $xml -Force

    }
}

UpdateVersionForUpdater("standalone")
UpdateVersionForUpdater("selfcontained")

Invoke-Expression "git add .  | Write-Verbose"
Invoke-Expression "git commit -m 'updatelist Bump.' | Write-Verbose"
Invoke-Expression "git push | Write-Verbose"