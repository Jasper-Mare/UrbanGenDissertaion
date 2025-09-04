# UrbanGenDissertaion

This is the repository for my university undergrad dissertation project.
This project is/was being developed in my 3rd year at the University Of Exeter from 2025 to 2026.

## The topic proposal went as follows:

Procedural generation is a powerful tool in games to speed up asset creation or even create novel virtual worlds around the player at run time. Many procedural generation aproaches for dynamically creating urban environments are defined by systems of square road grids, so the resulting cities miss out on the unique charm of densely interconnected urban areas and freeform pedestrian spaces. In this project I will research and develop techniques for creating virtual cities that capture these elements.

See central Manchester around Castlefield and the old town of Cordoba, Spain for examples of charming densely connected urban areas.

I imagine the aproach may involve splines and curves surrounded by bounding volumes so elements can grow while avoiding clipping through their neighbours and deciding when to for intersections with them. Buildings can be modeled as volumes filling in the space unused by the elements.

The program will be built in a game engine so I can focus on the core project without wasting time on the surrounding infrastructure.

The feature goals are:

Minimum Product: 
* A program that can generate a freeform streetplan that includes pedestrian pathways, roads, and buildings. 
* The most minimum version could be a 2d plan like a map.

Important features:
* 3d generation with basic boxy buildings.
* Generate meshes for the city that can be exported.
* Support for overlapping elements ie support for bridges, viaducts, tunnels.
* Rail, Rivers, Parks, Plazas/Courtyards,

Stretch Goals:
* Terrain that isn't flat (i.e. hills).
* Deterministic generation tiles (so am unlimited area can be generated a tile at a time).
* Support for pre-modeled buildings (so an environment artist could provide a building).
* Improved generation of buildings (give buildings details beyond basic boxes).
* Good texturing (ie dynamic road intersection textures, detailed building apearances)
