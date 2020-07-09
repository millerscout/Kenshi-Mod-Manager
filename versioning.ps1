#inspiration from https://gist.github.com/kumichou/acefc48476957aad6b0c9abf203c304c
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True,Position=1)]
    [string]$bumpKind
)

function getVersion() {
	Invoke-Expression "git fetch | Write-Verbose"	
    $tag = Invoke-Expression "git describe --tags --always 2>&1"

    $tag = $tag.Split('-')[0]
    $tag = $tag -replace 'v', ''

    if ($tag -notmatch "\d+\.\d+") {
        $tag = '1.0'
    }

    Write-Verbose "Version found: $tag"
    return $tag
}

function bumpVersion($kind, $version) {
    $major, $minor = $version.split('.')

    switch ($kind) {
        "major" {
            $major = [int]$major + 1
        }
        "minor" {
            $minor = [int]$minor + 1
        }
    }

    return [string]::Format("{0}.{1}", $major, $minor)
}

function commitVersion($kind, $version) {
     Invoke-Expression "git add . 2>&1 | Write-Verbose"
     Invoke-Expression "git commit -m 'New Version: $version' 2>&1 | Write-Verbose"
     Invoke-Expression "git tag v$version 2>&1 | Write-Verbose"
}

function SetVersion ($file, $version) {
    Write-Verbose "Changing version in $file to $version"
    $fileObject = get-item $file

    $sr = new-object System.IO.StreamReader( $file, [System.Text.Encoding]::GetEncoding("utf-8") )
    $content = $sr.ReadToEnd()
    $sr.Close()

    $content = [Regex]::Replace($content, "<Version>[\s\S]*?<\/Version>", "<Version>"+$version+"</Version>");
	$content = [Regex]::Replace($content, "<FileVersion>[\s\S]*?<\/FileVersion>", "<FileVersion>"+$version+"</FileVersion>");
	$content = [Regex]::Replace($content, "<AssemblyVersion>[\s\S]*?<\/AssemblyVersion>", "<AssemblyVersion>"+$version+"</AssemblyVersion>");

    $sw = new-object System.IO.StreamWriter( $file, $false, [System.Text.Encoding]::GetEncoding("utf-8") )
    $sw.Write( $content )
    $sw.Close()
}

function setVersionInDir($dir, $version) {
    if ($version -eq "") {
        Write-Verbose "version not found"
        exit 1
    }

    # Set the Assembly version
    $info_files = Get-ChildItem $dir -Recurse -Include "KenshiModTool.csproj"

    foreach($file in $info_files)
    {
        Setversion $file $version
    }
}


$validCommands = @("major", "minor")

if ($bumpKind -eq '')
{
    Write-Output "Missing which number to bump up!"
    exit 1
}

if (-not $validCommands.Contains($bumpKind))
{
    Write-Output "Invalid command!"
    exit 1
}


$oldVersion = getVersion
$newVersion = bumpVersion $bumpKind $oldVersion

$dir = "KenshiModTool/."

setVersionInDir $dir $newVersion
commitVersion $bumpKind $newVersion

Write-Output $newVersion

dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\FullRelease-86x.pubxml
dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\FullRelease-x64.pubxml
dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\Standalone-x64.pubxml
dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\Standalone-x86.pubxml

xcopy "..\ModConflictManager\publish\x86\Mod Conflict Manager.exe" publish\Standalone-x86
xcopy "..\ModConflictManager\publish\x86\Mod Conflict Manager.exe" publish\FullRelease-x86
xcopy "..\ModConflictManager\publish\x64\Mod Conflict Manager.exe" publish\Standalone-x64
xcopy "..\ModConflictManager\publish\x64\Mod Conflict Manager.exe" publish\FullRelease-x64

Remove-Item "publish" -Include *.pdb -Recurse -force

7z a -tzip "publish\FullRelease-x64" -r "publish\FullRelease-x64"
7z a -tzip "publish\FullRelease-x86" -r "publish\FullRelease-x86"
7z a -tzip "publish\Standalone-x64" -r "publish\Standalone-x64"
7z a -tzip "publish\Standalone-x86" -r "publish\Standalone-x86"

$secret_key = Get-Content $env:APPDATA"..\..\..\.ssh\Token(Oauth)Kenshideploy"
$secret_key
$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("Authorization", $secret_key)
$headers.Add("Content-Type", "application/json")

$body = "{
`n  `"tag_name`": `"v$($newVersion)`",
`n  `"target_commitish`": `"master`",
`n  `"name`": `"v$($newVersion)`",
`n  `"body`": `"Text not avaiable Yet.`",
`n  `"draft`": false,
`n  `"prerelease`": false
`n}"

$response = Invoke-RestMethod 'https://api.github.com/repos/millerscout/Kenshi-Mod-Manager/releases
' -Method 'POST' -Headers $headers -Body $body
$id = $response.id

$dir = "C:\project\Kenshi-Mod-Manager\publish"
$info_files = Get-ChildItem $dir -Filter "*.zip"

foreach($file in $info_files)
{
    $name = $file | Select-object name | ForEach-Object {$_.Name}

    Clear-Variable headers 
    $headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
    $headers.Add("Authorization", $secret_key)
    $headers.Add("Content-Type", "application/zip")
    $body = ".\publish\$($name)"
    $path = "https://uploads.github.com/repos/millerscout/Kenshi-Mod-Manager/releases/$($id)/assets?name=$($name)"
    $response = Invoke-RestMethod $path -Method 'POST' -Headers $headers -Infile $body
    $response.id
}