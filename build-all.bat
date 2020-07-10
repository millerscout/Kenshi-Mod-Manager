@RD /S /Q "publish"

powershell -ExecutionPolicy ByPass -File versioning.ps1 %1 -verbose

