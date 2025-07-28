set windows-shell := ["nu", "-c"]

default:
  just --list
 
generate-shader-assets:
  dotnet run --project SpaceGame.Build.Shaders/SpaceGame.Build.Shaders.csproj -- --src ./SpaceGame.Assets --dest ./SpaceGame.Assets/generated --namespace SpaceGame.Assets

build-projects:
  dotnet build SpaceGame.Build.sln

build-source:
  dotnet build SpaceGame.sln

build: build-projects build-source

run:
  dotnet run --project SpaceGame.Console/SpaceGame.Console.csproj
