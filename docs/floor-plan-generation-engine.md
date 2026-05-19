# Floor Plan Generation Engine

This document describes the MVP floor plan generation engine added under `FloorPlanGeneration*`. The engine is independent from the existing RGeoLib, notebook, sample, and Python API code; it is a small .NET library plus CLI that can later be wrapped by Grasshopper/Rhino components.

## Architecture

- `FloorPlanGeneration` targets `netstandard2.0` and contains the public `FloorPlanEngine`.
- `FloorPlanGeneration.Schema` defines JSON-serializable DTOs: `EngineInput`, `EngineOutput`, `LayoutVariant`, units, rooms, corridors, walls, openings, labels, diagnostics, metrics, and validation checks.
- `FloorPlanGeneration.Geometry` contains lightweight 2D primitives, polygon cleanup, predicate checks, and facade exposure helpers.
- `FloorPlanGeneration.Generation` contains deterministic seeded candidate generation, unit-mix selection, corridor strategy derivation, room templates, wall/opening/label output, and topology output.
- `FloorPlanGeneration.Topology` emits a simple graph of floorplate, fixed elements, corridor, unit, room, and facade relationships.
- `FloorPlanGeneration.Cli` is a `net8.0` command-line adapter using `System.Text.Json`.
- `FloorPlanGeneration.Tests` contains xUnit coverage for deterministic generation and failure paths.

The public orchestration path is:

1. Normalize missing optional input sections and defaults.
2. Clean the floorplate, holes, and fixed element polygons.
3. Stop on invalid geometry diagnostics such as self-intersection.
4. Generate deterministic corridor/unit/room candidates from the cleaned input.
5. Normalize generated output ordering and repair duplicate corridor connection labels.
6. Validate geometry containment, overlap, corridor width, doors, daylight requirements, and strict unit-mix constraints.
7. Score each variant and rank valid variants before invalid variants.

## Input Schema

The CLI accepts camelCase JSON matching `EngineInput`.

Main sections:

- `project`: `id`, `name`, `units`, `tolerance`, `seed`.
- `floorplate`: required `outer` polygon and optional `holes`.
- `fixedElements`: blocking or non-blocking polygons such as cores, stairs, shafts, and elevators.
- `access`: entry points, vertical core access points, optional corridor centerlines.
- `facade`: optional explicit daylight-capable segments. If omitted, the outer floorplate boundary is treated as daylight-capable.
- `program`: target unit types and room type rules.
- `rules`: minimum corridor width, room size, door width, daylight requirements, and minimum unit area.
- `generationSettings`: variant count, strictness, weighted variation, and scoring weights.

Polygon rings may be open or closed. The engine stores unique vertices internally and reports cleanup diagnostics when it removes duplicate, closing, or collinear points.

Example input:

```sh
samples/floor-plan-generation/rectangular-core-input.json
```

## Output Schema

`EngineOutput` contains:

- `projectId`
- `status`: `succeeded`, `partial`, or `failed`
- `variants`: ranked `LayoutVariant` results
- `diagnostics`: top-level cleanup, generation, validation, or CLI diagnostics

Each variant contains:

- `variantId`, `seed`, `status`
- `units`, each with type, polygon, area, rooms, facade length, and score
- `rooms`, duplicated at variant level for easier adapters
- `corridors`, with polygon, centerline, width, and connected ids
- `walls`, `doorsOpenings`, `labels`
- `metrics`: gross area, sellable area, corridor area, net-gross ratio, efficiency, unit-mix match, score
- `validation`: checks with severity, source id, and reason
- `topology`: graph nodes and edges for containment, corridor access, and facade exposure

## CLI Usage

Read from a file and write to a file:

```sh
dotnet run --project FloorPlanGeneration.Cli -- --input samples/floor-plan-generation/rectangular-core-input.json --output samples/floor-plan-generation/rectangular-core-output.json
```

Read from stdin and write to stdout:

```sh
dotnet run --project FloorPlanGeneration.Cli -- < samples/floor-plan-generation/rectangular-core-input.json
```

Exit codes:

- `0`: engine status is not `failed`
- `2`: engine returned failed JSON output
- `64`: CLI argument error

## Grasshopper Adapter Mapping

Recommended MVP Grasshopper components:

- `FP Engine Input`: builds `EngineInput` from floorplate curve, hole curves, core curves, facade curves, program tables, rule sliders, and seed.
- `FP Generate`: calls `FloorPlanEngine.Generate(input)` and returns output JSON plus typed variant objects.
- `FP Variant Picker`: sorts or selects a `LayoutVariant` by rank, score, or id.
- `FP Bake Variant`: maps units, rooms, corridors, walls, doors, and labels to Rhino geometry and layers.
- `FP Diagnostics`: displays top-level and selected-variant diagnostics grouped by severity and source id.
- `FP Topology Graph`: exposes `TopologyGraph.Nodes` and `TopologyGraph.Edges` for adjacency analysis or graph visualization.

Grasshopper geometry mapping:

- Closed planar boundary curve -> `floorplate.outer.points`
- Closed planar void curves -> `floorplate.holes`
- Core/stair/elevator/shaft curves -> `fixedElements`
- Corridor guide lines -> `access.corridorCenterlines`
- Door/core points -> `access.entryPoints` and `access.verticalCoreAccess`
- Facade sub-curves -> `facade.segments`
- Value lists/sliders/panels -> `program`, `rules`, and `generationSettings`

## Rhino Layers

Use these layer names when baking:

- `FP::Input::Floorplate`
- `FP::Input::Holes`
- `FP::Input::FixedElements`
- `FP::Input::Facade`
- `FP::Generated::Units`
- `FP::Generated::Rooms`
- `FP::Generated::Corridors`
- `FP::Generated::Walls`
- `FP::Generated::Doors`
- `FP::Generated::Labels`
- `FP::Diagnostics`
- `FP::Topology`

Labels produced by the engine already default to `FP::Generated::Labels`.

## Validation Guarantees

The MVP validates:

- Outer boundary has at least three points, finite coordinates, non-zero area, and no self-intersections.
- Holes and fixed elements are individually valid and contained by the floorplate.
- Blocking fixed elements and holes are avoided by corridors and units.
- Generated corridors meet minimum width and stay inside the floorplate.
- Generated units stay inside the floorplate, do not overlap holes/fixed elements/corridors/other units, and meet minimum unit area.
- Generated rooms reference existing units, stay inside their units, have positive area, and satisfy configured daylight requirements for bedroom/living room types.
- Generated units have door/opening connections.
- `generationSettings.strictness = "strict"` requires exact target unit counts when target counts are supplied.

Invalid outer-boundary geometry stops before candidate generation and returns failed diagnostics with no fake variants.

## MVP Limits

- Geometry operations are lightweight polygon predicates, not a full constructive solid geometry kernel.
- Candidate generation is deterministic and heuristic; it does not solve a global optimization problem.
- Unit rooms are template-based rectangular subdivisions.
- Complex non-orthogonal, highly concave, or multi-core buildings may produce partial or failed variants.
- Facade handling is segment-length based and does not model view quality, obstructions, fire separation, or code-specific egress.
- Validation is architectural sanity checking, not permit-ready building code compliance.
- The engine does not mutate existing RGeoLib, samples, notebooks, or Python API surfaces.

## BIM and IFC Next Steps

Recommended next integration steps:

1. Add stable GUIDs and external ids for units, rooms, walls, doors, and topology edges.
2. Add wall thickness offsets and room finish boundaries rather than centerline-only walls.
3. Map `UnitLayout` and `RoomLayout` to IFC spatial elements such as `IfcSpace` and zone/group objects.
4. Map `WallLayout` and `DoorOpening` to `IfcWall`/`IfcWallStandardCase` and `IfcDoor`.
5. Add storey/building/site metadata to `EngineInput.Project`.
6. Add an IFC export adapter separate from the core engine so the MVP library remains dependency-light.
7. Extend validation with egress distance, accessible clearances, shaft/fire separation, facade/window ratios, and local code profiles.
