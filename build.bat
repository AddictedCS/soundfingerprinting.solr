@echo off

dotnet clean "./src/SoundFingerprinting.Solr.sln"
dotnet restore "./src/SoundFingerprinting.Solr.sln" --packages "./packages"
dotnet build "./src/SoundFingerprinting.Solr.sln" --configuration=Release --no-restore
dotnet test "./src/SoundFingerprinting.Solr.Tests/SoundFingerprinting.Solr.Tests.csproj" --configuration=Release --no-restore --no-build
dotnet pack "./src/SoundFingerprinting.Solr/SoundFingerprinting.Solr.csproj" --configuration=Release --no-restore --no-build --output "../../artifacts"