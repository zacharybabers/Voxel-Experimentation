Voxel Experimentation

My goal with this project is to create a (basic) voxel terrain "engine" in unity. 

Features I want to mess with and eventually implement: voxel rendering (of course), atlas based texturing, different block types, terrain generation with noise, biome generation, shrubbery/tree generation, water, grass, weather, working infinite chunk streaming, LOD implementation for larger view distance, village/town and structure generation.

I don't intend on adding terrain manipulation (à la Minecraft), although it is possible I will end up giving it a try. My reasoning for this omission is that the ideas that I currently have for game extensions of this engine wouldn't involve building or destroying terrain.

For mesh generation my plan is to use a simple (and currently single-threaded, might attempt multithreading through jobs/burst if the performance is too bad but I have never worked with them so that's a big if) culling algorithm to draw chunks as a single mesh.

Additionally I'll be doing all of this from scratch w/ unity for rendering except importing a library for noise. (https://github.com/Auburn/FastNoiseLite)


Feature Checklist:

Voxel Rendering - done

Atlas Based Texturing - done

Different Block Types - done

Terrain Generation w/ Noise - done but needs rework

Biome Generation - wip

Shrubbery/Tree Generation

Water

Grass

Weather

Infinite Chunk Streaming - done

Village/Town Generation

LOD Implementation - done
