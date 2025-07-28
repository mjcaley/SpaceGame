set windows-shell := ["nu", "-c"]

default:
  just --list
 
build-projects:
  dotnet build SpaceGame.Build.sln

build-source:
  dotnet build SpaceGame.sln

build: build-projects build-source
