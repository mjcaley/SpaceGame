default:
  just --list
 
[working-directory: 'SpaceGame.Assets']
build-shaders:
  rm -rf res/shaders
  mkdir res/shaders
  slangc -g3 -entry vertexMain -o res/shaders/coloured-quad.vert.spv -capability spirv_1_0 -- coloured-quad.slang
  slangc -g3 -entry fragmentMain -o res/shaders/coloured-quad.frag.spv -capability spirv_1_0 -- coloured-quad.slang
  slangc -g3 -entry vertexMain -o res/shaders/coloured-quad.vert.hlsl -- coloured-quad.slang
  slangc -g3 -entry fragmentMain -o res/shaders/coloured-quad.frag.hlsl -- coloured-quad.slang

build: build-shaders
  dotnet build
