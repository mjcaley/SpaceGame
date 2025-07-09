set windows-shell := ["nu", "-c"]

default:
  just --list
 
[working-directory: 'SpaceGame.Assets']
build-shaders:
  rm -rf res/shaders
  mkdir res
  mkdir res/shaders
  slangc -g3 -entry vertexMain -o res/shaders/coloured-quad.vert.spv -capability spirv_1_0 -- coloured-quad.slang
  slangc -g3 -entry fragmentMain -o res/shaders/coloured-quad.frag.spv -capability spirv_1_0 -- coloured-quad.slang
  slangc -g3 -entry vertexMain -o res/shaders/coloured-quad.vert.hlsl -- coloured-quad.slang
  slangc -g3 -entry fragmentMain -o res/shaders/coloured-quad.frag.hlsl -- coloured-quad.slang
  
  slangc -g3 -entry main -o res/shaders/indexed-coloured-quad.vert.spv -capability spirv_1_0 -- indexed-coloured-quad.vert.slang
  slangc -g3 -entry main -o res/shaders/indexed-coloured-quad.frag.spv -capability spirv_1_0 -- indexed-coloured-quad.frag.slang
  slangc -g3 -entry main -o res/shaders/indexed-coloured-quad.vert.hlsl -- indexed-coloured-quad.vert.slang
  slangc -g3 -entry main -o res/shaders/indexed-coloured-quad.frag.hlsl -- indexed-coloured-quad.frag.slang

build: build-shaders
  dotnet build
