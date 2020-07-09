@RD /S /Q "publish"
powershell -ExecutionPolicy ByPass -File versioning.ps1 %1 -verbose


dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\FullRelease-86x.pubxml
dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\FullRelease-x64.pubxml
dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\Standalone-x64.pubxml
dotnet publish -p:PublishProfile=KenshiModTool\Properties\PublishProfiles\Standalone-x86.pubxml

xcopy "..\ModConflictManager\publish\x86\Mod Conflict Manager.exe" publish\Standalone-x86
xcopy "..\ModConflictManager\publish\x86\Mod Conflict Manager.exe" publish\FullRelease-x86
xcopy "..\ModConflictManager\publish\x64\Mod Conflict Manager.exe" publish\Standalone-x64
xcopy "..\ModConflictManager\publish\x64\Mod Conflict Manager.exe" publish\FullRelease-x64

del "publish\*.pdb" /s /f /q

cd publish

7z a -tzip FullRelease-x64 -r FullRelease-x64
7z a -tzip FullRelease-x86 -r FullRelease-x86
7z a -tzip Standalone-x64 -r Standalone-x64
7z a -tzip Standalone-x86 -r Standalone-x86