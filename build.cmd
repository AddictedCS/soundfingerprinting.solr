@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

dotnet restore .\src\SoundFingerprinting.Solr.sln
dotnet test .\src\SoundFingerprinting.Solr.Tests\SoundFingerprinting.Solr.Tests.csproj -c %config%
dotnet pack .\src\SoundFingerprinting.Solr\SoundFingerprinting.Solr.csproj -c %config% -o ..\..\build