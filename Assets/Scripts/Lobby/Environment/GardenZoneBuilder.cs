using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Builds the Garden Zone (floating clouds, planting plots, golden tree).
    /// Redesigned from scratch to feel like a fantasy sky sanctuary inspired by Sky: COTL & Ghibli.
    /// Located at world Z = +60.
    /// </summary>
    public static class GardenZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("GardenZone");
            zone.transform.position = new Vector3(0, 0, 60f);

            var grassColor = new Color(0.32f, 0.65f, 0.22f);
            var goldColor = new Color(1.0f, 0.85f, 0.15f);

            // ── 1. Background Cloud Sea (Y = -5 to -8) ──
            CreateBackgroundCloudSea(zone);

            // ── 2. Island 1: Entrance Sanctuary (Z = -5.0f, Y = 0) ──
            var island1 = CreateSkyIsland(zone, "EntranceIsland", new Vector3(0, 0, -5.0f), 3.0f, 1.2f, grassColor);
            
            // Entrance Archway
            var arch = new GameObject("EntranceArch");
            arch.transform.SetParent(island1.transform, false);
            arch.transform.localPosition = new Vector3(0, 0.02f, -2.5f);
            BuildFloralArch(arch);

            // Wishing Well
            var well = new GameObject("WishingWell");
            well.transform.SetParent(island1.transform, false);
            well.transform.localPosition = new Vector3(-1.8f, 0.02f, 1.5f);
            BuildWishingWell(well);

            // Lanterns
            CreateLantern(island1, new Vector3(-2.2f, 0.02f, -2.0f));
            CreateLantern(island1, new Vector3(2.2f, 0.02f, -2.0f));

            // Decorations & Flowers
            CreateFlowerPatch(island1, new Vector3(-2.0f, 0.02f, -0.5f), new Color(0.95f, 0.45f, 0.65f), 5);
            CreateFlowerPatch(island1, new Vector3(2.0f, 0.02f, -1.0f), new Color(0.95f, 0.8f, 0.2f), 5);
            CreateBushesAndMushrooms(island1, new Vector3(1.8f, 0.02f, 1.5f));
            CreateButterfliesAndParticles(island1, new Vector3(0, 0.5f, 0));

            // ── 3. Island 2: Garden Terrace (Z = 0.5f, X = 3.2f, Y = 0.8f) ──
            var island2 = CreateSkyIsland(zone, "GardenTerrace", new Vector3(3.2f, 0.8f, 0.5f), 3.5f, 1.4f, grassColor);

            // Waterfall pouring off the left edge (X = -2.8f)
            CreateWaterfall(island2, new Vector3(-2.6f, 0.02f, 0f), 4.2f);

            // Cozy Bench & Tea Table
            CreateBench(island2, new Vector3(1.6f, 0.02f, 1.2f), -45f);
            var table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            table.name = "Table";
            table.transform.SetParent(island2.transform, false);
            table.transform.localPosition = new Vector3(0.9f, 0.3f, 0.5f);
            table.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
            table.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.7f, 0.7f, 0.72f));

            // Lantern on Island 2
            CreateLantern(island2, new Vector3(2.4f, 0.02f, -2.0f));

            // Flower Patches & Foliage
            CreateFlowerPatch(island2, new Vector3(2.2f, 0.02f, 0.5f), new Color(0.4f, 0.75f, 1.0f), 6);
            CreateFlowerPatch(island2, new Vector3(-1.8f, 0.02f, 1.8f), new Color(0.9f, 0.95f, 0.95f), 4);
            CreateBushesAndMushrooms(island2, new Vector3(2.2f, 0.02f, -1.2f));

            // ── 4. Island 3: World Tree Sanctuary (Z = 5.2f, X = -1.2f, Y = 1.6f) ──
            var island3 = CreateSkyIsland(zone, "WorldTreeSanctuary", new Vector3(-1.2f, 1.6f, 5.2f), 3.2f, 1.6f, grassColor);

            // Giant Ancient Golden Tree
            CreateGoldenTree(island3, new Vector3(0, 0.02f, 1.2f));

            // Extra Cloud cluster behind tree
            var cloudBack = new GameObject("CanopyCloudCluster");
            cloudBack.transform.SetParent(island3.transform, false);
            cloudBack.transform.localPosition = new Vector3(0.5f, 2.2f, 2.5f);
            CreateCloudCluster(cloudBack, Vector3.zero, new Vector3(2.5f, 1.8f, 2.0f));

            // Lanterns
            CreateLantern(island3, new Vector3(-2.2f, 0.02f, -1.8f));
            CreateLantern(island3, new Vector3(2.2f, 0.02f, -1.8f));

            // Flowers, Foliage & Butterflies
            CreateFlowerPatch(island3, new Vector3(-2.0f, 0.02f, 0.8f), goldColor, 7);
            CreateBushesAndMushrooms(island3, new Vector3(-2.2f, 0.02f, -0.6f));
            CreateBushesAndMushrooms(island3, new Vector3(2.2f, 0.02f, 0.4f));
            CreateButterfliesAndParticles(island3, new Vector3(0, 0.8f, 1.0f));

            // ── 5. Wooden Bridges ──
            // Bridge 1 connects Island 1 (local center (0,0,-5) -> edge is roughly X=1.2, Z=-2.5) 
            // to Island 2 (local center (3.2, 0.8, 0.5) -> edge is roughly X=1.6, Z=-1.8)
            CreateFantasyBridge(zone, new Vector3(1.2f, 0.02f, -2.6f), new Vector3(2.0f, 0.82f, -1.5f), 10);

            // Bridge 2 connects Island 2 (local center (3.2, 0.8, 0.5) -> edge is roughly X=1.6, Z=2.2)
            // to Island 3 (local center (-1.2, 1.6, 5.2) -> edge is roughly X=0.8, Z=3.6)
            CreateFantasyBridge(zone, new Vector3(2.0f, 0.82f, 2.0f), new Vector3(0.0f, 1.62f, 3.8f), 10);

            // ── 6. Cloud Lock Barriers (Blocking the Bridges) ──
            var barrier2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier2.name = "Stair1_Barrier_Collider";
            barrier2.transform.SetParent(zone.transform, false);
            barrier2.transform.localPosition = new Vector3(1.6f, 0.5f, -2.0f);
            barrier2.transform.localScale = new Vector3(1.8f, 0.9f, 0.2f);
            barrier2.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
            var lock2 = barrier2.AddComponent<CloudLayer>();
            ZoneFactory.SetField(lock2, "_unlockCost", 10);
            ZoneFactory.SetField(lock2, "_barrierObj", barrier2);

            var barrier3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier3.name = "Stair2_Barrier_Collider";
            barrier3.transform.SetParent(zone.transform, false);
            barrier3.transform.localPosition = new Vector3(1.0f, 1.3f, 2.9f);
            barrier3.transform.localScale = new Vector3(1.8f, 0.9f, 0.2f);
            barrier3.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
            var lock3 = barrier3.AddComponent<CloudLayer>();
            ZoneFactory.SetField(lock3, "_unlockCost", 25);
            ZoneFactory.SetField(lock3, "_barrierObj", barrier3);

            // ── 7. Planting Plots (Aesthetic wood-lined farming beds) ──
            // Island 1 (Y = 0)
            CreatePlot(zone, new Vector3(-1.8f, 0.05f, -4.5f), 10f, 10);
            CreatePlot(zone, new Vector3(1.8f, 0.05f, -4.5f), 10f, 10);
            // Island 2 (Y = 0.8)
            CreatePlot(zone, new Vector3(2.2f, 0.85f, 0.5f), 15f, 25);
            CreatePlot(zone, new Vector3(4.2f, 0.85f, -0.5f), 15f, 25);
            // Island 3 (Y = 1.6)
            CreatePlot(zone, new Vector3(-2.2f, 1.65f, 4.2f), 20f, 50);
            CreatePlot(zone, new Vector3(0.2f, 1.65f, 4.2f), 20f, 50);

            // ── 8. Title Billboard ──
            var title = new GameObject("GardenTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3.6f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Khu Vuon Cua Ban";
            tmp.fontSize = 6.4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ── 9. Return Portal ──
            ZoneFactory.CreatePortal("GardenReturnPortal", new Vector3(0, 0.05f, 54f), 180f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.15f, 0.85f, 0.3f, 0.7f),
                "Ve Sanh Cho");
        }

        private static void CreatePlot(GameObject parent, Vector3 localPos, float duration, int reward)
        {
            // Planting plot wrap frame (wooden box)
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "PlanterFrame_Collider";
            frame.transform.SetParent(parent.transform, false);
            frame.transform.localPosition = localPos + Vector3.up * 0.01f;
            frame.transform.localScale = new Vector3(0.68f, 0.06f, 0.68f);
            frame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.48f, 0.32f, 0.18f));

            var plotObj = new GameObject("PlanterPlot");
            plotObj.transform.SetParent(parent.transform, false);
            plotObj.transform.localPosition = localPos;
            var plot = plotObj.AddComponent<GardenPlot>();
            ZoneFactory.SetField(plot, "_growthDuration", duration);
            ZoneFactory.SetField(plot, "_harvestReward", reward);
        }

        private static GameObject CreateSkyIsland(GameObject parent, string name, Vector3 localPos, float radius, float height, Color grassCol)
        {
            var island = new GameObject(name);
            island.transform.SetParent(parent.transform, false);
            island.transform.localPosition = localPos;

            var dirtCol = new Color(0.35f, 0.28f, 0.22f);
            var stoneColor = new Color(0.68f, 0.68f, 0.7f);

            // 1. Main grass cylinder and dirt base cylinder
            var mainGrass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainGrass.name = "IslandGrassMain";
            mainGrass.transform.SetParent(island.transform, false);
            mainGrass.transform.localPosition = Vector3.zero;
            mainGrass.transform.localScale = new Vector3(radius * 2, 0.05f, radius * 2);
            mainGrass.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(grassCol);

            var mainDirt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainDirt.name = "IslandDirtMain_Collider";
            mainDirt.transform.SetParent(island.transform, false);
            mainDirt.transform.localPosition = new Vector3(0, -height * 0.5f, 0);
            mainDirt.transform.localScale = new Vector3(radius * 2, height, radius * 2);
            mainDirt.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(dirtCol);

            // 2. Overlapping sub-islands to build an irregular natural silhouette
            int subParts = 5;
            for (int i = 0; i < subParts; i++)
            {
                float angle = (i * (360f / subParts) + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
                float dist = radius * Random.Range(0.4f, 0.65f);
                Vector3 subOffset = new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
                float subRadius = radius * Random.Range(0.45f, 0.68f);
                float subHeight = height * Random.Range(0.8f, 1.15f);

                var subGrass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                subGrass.name = "IslandGrassSub";
                subGrass.transform.SetParent(island.transform, false);
                subGrass.transform.localPosition = subOffset + new Vector3(0, Random.Range(-0.02f, 0.02f), 0);
                subGrass.transform.localScale = new Vector3(subRadius * 2, 0.05f, subRadius * 2);
                subGrass.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(grassCol);
                Object.Destroy(subGrass.GetComponent<Collider>());

                var subDirt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                subDirt.name = "IslandDirtSub_Collider";
                subDirt.transform.SetParent(island.transform, false);
                subDirt.transform.localPosition = subOffset + new Vector3(0, -subHeight * 0.5f, 0);
                subDirt.transform.localScale = new Vector3(subRadius * 2, subHeight, subRadius * 2);
                subDirt.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(dirtCol);
            }

            // 3. Exposed cliff rocks around the edges
            int rockCount = Random.Range(5, 9);
            for (int i = 0; i < rockCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float rDist = radius * Random.Range(0.8f, 1.05f);
                Vector3 rockPos = new Vector3(Mathf.Cos(angle) * rDist, Random.Range(-0.15f, 0.15f), Mathf.Sin(angle) * rDist);

                var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = "ExposedRock_Collider";
                rock.transform.SetParent(island.transform, false);
                rock.transform.localPosition = rockPos;
                rock.transform.localScale = new Vector3(
                    Random.Range(0.4f, 0.85f),
                    Random.Range(0.4f, 1.3f),
                    Random.Range(0.4f, 0.85f)
                );
                rock.transform.localRotation = Quaternion.Euler(
                    Random.Range(-22f, 22f),
                    Random.Range(0f, 360f),
                    Random.Range(-22f, 22f)
                );
                rock.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
            }

            // 4. Roots hanging from below
            int rootCount = Random.Range(8, 14);
            for (int i = 0; i < rootCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = radius * Random.Range(0.15f, 0.75f);
                Vector3 rootPos = new Vector3(Mathf.Cos(angle) * dist, -height * Random.Range(0.7f, 1.1f), Mathf.Sin(angle) * dist);

                var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                root.name = "HangingRoot";
                root.transform.SetParent(island.transform, false);
                root.transform.localPosition = rootPos;
                float rootLen = Random.Range(0.45f, 1.3f);
                root.transform.localScale = new Vector3(0.06f, rootLen, 0.06f);
                root.transform.localRotation = Quaternion.Euler(
                    Random.Range(-15f, 15f),
                    0,
                    Random.Range(-15f, 15f)
                );
                root.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(dirtCol * 0.72f);
                Object.Destroy(root.GetComponent<Collider>());
            }

            // 5. Ivy vines trailing down cliffs
            int vineCount = Random.Range(6, 10);
            for (int i = 0; i < vineCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = radius * 0.98f;
                Vector3 vinePos = new Vector3(Mathf.Cos(angle) * dist, -0.1f, Mathf.Sin(angle) * dist);

                var vine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                vine.name = "HangingVine";
                vine.transform.SetParent(island.transform, false);
                vine.transform.localPosition = vinePos + Vector3.down * 0.35f;
                float vineLen = Random.Range(0.35f, 0.75f);
                vine.transform.localScale = new Vector3(0.04f, vineLen, 0.04f);
                vine.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.44f, 0.18f));
                Object.Destroy(vine.GetComponent<Collider>());
            }

            return island;
        }

        private static void CreateWaterfall(GameObject island, Vector3 localEdgePos, float dropHeight)
        {
            var waterParent = new GameObject("Waterfall");
            waterParent.transform.SetParent(island.transform, false);

            var stoneColor = new Color(0.58f, 0.58f, 0.6f);
            var waterColor = new Color(0.45f, 0.78f, 0.95f, 0.8f);

            // Pond base
            var pond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pond.name = "PondWater";
            pond.transform.SetParent(waterParent.transform, false);
            pond.transform.localPosition = localEdgePos + new Vector3(0, 0.02f, 0.4f);
            pond.transform.localScale = new Vector3(1.3f, 0.01f, 1.3f);
            pond.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(waterColor);
            Object.Destroy(pond.GetComponent<Collider>());

            // Surrounding stone border
            for (int i = 0; i < 7; i++)
            {
                float angle = (i * (360f / 7f)) * Mathf.Deg2Rad;
                var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = "PondRock_Collider";
                rock.transform.SetParent(waterParent.transform, false);
                rock.transform.localPosition = localEdgePos + new Vector3(Mathf.Cos(angle) * 0.75f, 0.08f, 0.4f + Mathf.Sin(angle) * 0.75f);
                rock.transform.localScale = new Vector3(
                    Random.Range(0.2f, 0.45f),
                    Random.Range(0.15f, 0.35f),
                    Random.Range(0.2f, 0.45f)
                );
                rock.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0, 360f), Random.Range(-10f, 10f));
                rock.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
            }

            // Lily pads
            for (int i = 0; i < 3; i++)
            {
                var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pad.name = "LilyPad";
                pad.transform.SetParent(waterParent.transform, false);
                pad.transform.localPosition = localEdgePos + new Vector3(Random.Range(-0.3f, 0.3f), 0.03f, 0.4f + Random.Range(-0.3f, 0.3f));
                pad.transform.localScale = new Vector3(0.18f, 0.005f, 0.18f);
                pad.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.18f, 0.48f, 0.22f));
                Object.Destroy(pad.GetComponent<Collider>());
            }

            // Waterfall stream sheet
            var flow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flow.name = "WaterFlow";
            flow.transform.SetParent(waterParent.transform, false);
            flow.transform.localPosition = localEdgePos + new Vector3(0, -dropHeight * 0.5f, 0.05f);
            flow.transform.localScale = new Vector3(0.85f, dropHeight, 0.06f);
            flow.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.85f, 0.95f, 1.0f, 0.85f));
            Object.Destroy(flow.GetComponent<Collider>());

            // Foam splashes
            var splashParent = new GameObject("SplashFoam");
            splashParent.transform.SetParent(waterParent.transform, false);
            splashParent.transform.localPosition = localEdgePos + new Vector3(0, -dropHeight, 0.05f);

            for (int i = 0; i < 6; i++)
            {
                var foam = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foam.name = "SplashFoamSphere";
                foam.transform.SetParent(splashParent.transform, false);
                foam.transform.localPosition = new Vector3(
                    Random.Range(-0.4f, 0.4f),
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f)
                );
                foam.transform.localScale = Vector3.one * Random.Range(0.2f, 0.38f);
                foam.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(Color.white);
                Object.Destroy(foam.GetComponent<Collider>());
            }
        }

        private static void CreateFantasyBridge(GameObject zone, Vector3 start, Vector3 end, int plankCount)
        {
            var bridge = new GameObject("FantasyBridge");
            bridge.transform.SetParent(zone.transform, false);

            var woodColor = new Color(0.42f, 0.28f, 0.15f);
            var ropeColor = new Color(0.72f, 0.62f, 0.46f);

            // Compute midpoint with sag
            Vector3 mid = (start + end) * 0.5f + Vector3.down * 0.35f;

            // 1. Planks
            for (int i = 0; i < plankCount; i++)
            {
                float t = (float)i / (plankCount - 1);
                Vector3 pos = GetBezierPoint(start, mid, end, t);
                Vector3 tangent = GetBezierTangent(start, mid, end, t);
                Quaternion rot = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(0, 90, 0);

                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = "BridgePlank_Collider";
                plank.transform.SetParent(bridge.transform, false);
                plank.transform.localPosition = pos;
                plank.transform.localScale = new Vector3(1.6f, 0.06f, 0.26f);
                plank.transform.localRotation = rot;
                plank.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);
            }

            // 2. Support posts
            Vector3 rightOffset = Quaternion.Euler(0, 90, 0) * (end - start).normalized * 0.8f;
            Vector3[] postPositions = {
                start + rightOffset, start - rightOffset,
                end + rightOffset, end - rightOffset
            };
            foreach (var ppos in postPositions)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "BridgePost_Collider";
                post.transform.SetParent(bridge.transform, false);
                post.transform.localPosition = ppos + Vector3.up * 0.45f;
                post.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
                post.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor * 0.78f);
            }

            // 3. Rope railings
            int ropeSegments = 16;
            for (int i = 0; i < ropeSegments; i++)
            {
                float t1 = (float)i / ropeSegments;
                float t2 = (float)(i + 1) / ropeSegments;

                Vector3 p1 = GetBezierPoint(start, mid, end, t1);
                Vector3 p2 = GetBezierPoint(start, mid, end, t2);

                // Left rope
                Vector3 lp1 = p1 - rightOffset + Vector3.up * 0.45f;
                Vector3 lp2 = p2 - rightOffset + Vector3.up * 0.45f;
                CreateRopeSegment(bridge, lp1, lp2, ropeColor);

                // Right rope
                Vector3 rp1 = p1 + rightOffset + Vector3.up * 0.45f;
                Vector3 rp2 = p2 + rightOffset + Vector3.up * 0.45f;
                CreateRopeSegment(bridge, rp1, rp2, ropeColor);
            }
        }

        private static void CreateRopeSegment(GameObject parent, Vector3 p1, Vector3 p2, Color color)
        {
            var segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = "BridgeRope";
            segment.transform.SetParent(parent.transform, false);

            Vector3 diff = p2 - p1;
            segment.transform.localPosition = p1 + diff * 0.5f;
            segment.transform.localScale = new Vector3(0.03f, diff.magnitude * 0.5f, 0.03f); // Cylinder has height 2
            segment.transform.localRotation = Quaternion.FromToRotation(Vector3.up, diff.normalized);

            segment.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
            Object.Destroy(segment.GetComponent<Collider>());
        }

        private static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        private static Vector3 GetBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return (2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1)).normalized;
        }

        private static void CreateGoldenTree(GameObject parent, Vector3 localPos)
        {
            var goldTree = new GameObject("GhibliGoldenTree");
            goldTree.transform.SetParent(parent.transform, false);
            goldTree.transform.localPosition = localPos;

            var trunkColor = new Color(0.4f, 0.26f, 0.12f);
            var goldLeaf = new Color(1.0f, 0.85f, 0.15f);
            var orangeLeaf = new Color(0.95f, 0.6f, 0.1f);
            var paleLeaf = new Color(1.0f, 0.95f, 0.45f);

            // Gnarled trunk segment overlap
            int trunkSegments = 6;
            for (int i = 0; i < trunkSegments; i++)
            {
                var tr = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tr.name = "TrunkSegment_Collider";
                tr.transform.SetParent(goldTree.transform, false);
                tr.transform.localPosition = new Vector3(
                    Random.Range(-0.06f, 0.06f),
                    0.25f + i * 0.35f,
                    Random.Range(-0.06f, 0.06f)
                );
                tr.transform.localScale = new Vector3(0.42f - i * 0.05f, 0.22f, 0.42f - i * 0.05f);
                tr.transform.localRotation = Quaternion.Euler(Random.Range(-6f, 6f), Random.Range(0f, 360f), Random.Range(-6f, 6f));
                tr.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(trunkColor);
            }

            // Gnarled branches
            Vector3[] branchData = {
                new(-0.35f, 1.4f, 0.1f), new(0.3f, 1.6f, -0.2f), new(-0.1f, 1.7f, 0.4f),
                new(0.2f, 1.8f, 0.3f), new(-0.4f, 1.9f, -0.3f)
            };
            Vector3[] branchScale = {
                new(0.12f, 0.45f, 0.12f), new(0.1f, 0.4f, 0.1f), new(0.1f, 0.4f, 0.1f),
                new(0.08f, 0.35f, 0.08f), new(0.08f, 0.35f, 0.08f)
            };
            Quaternion[] branchRot = {
                Quaternion.Euler(0, 0, 50f), Quaternion.Euler(-45f, 0, -45f), Quaternion.Euler(45f, 45f, 0),
                Quaternion.Euler(30f, -30f, -30f), Quaternion.Euler(-30f, 60f, 50f)
            };
            for (int i = 0; i < branchData.Length; i++)
            {
                var br = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                br.name = "TreeBranch";
                br.transform.SetParent(goldTree.transform, false);
                br.transform.localPosition = branchData[i];
                br.transform.localScale = branchScale[i];
                br.transform.localRotation = branchRot[i];
                br.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(trunkColor);
                Object.Destroy(br.GetComponent<Collider>());
            }

            // Canopy layered foliage spheres
            var canopyData = new (Vector3 offset, float r, Color col)[] {
                (new(0, 2.2f, 0), 1.6f, goldLeaf),
                (new(-0.8f, 1.9f, 0.2f), 1.2f, orangeLeaf),
                (new(0.7f, 2.0f, -0.3f), 1.1f, orangeLeaf),
                (new(-0.2f, 2.1f, 0.8f), 1.1f, goldLeaf),
                (new(0.3f, 2.3f, 0.6f), 1.0f, paleLeaf),
                (new(-0.6f, 2.4f, -0.5f), 1.0f, paleLeaf),
                (new(0.1f, 2.8f, 0.1f), 1.3f, paleLeaf)
            };
            foreach (var (offset, r, col) in canopyData)
            {
                var leafGroup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leafGroup.name = "CanopyLeaves";
                leafGroup.transform.SetParent(goldTree.transform, false);
                leafGroup.transform.localPosition = offset;
                leafGroup.transform.localScale = Vector3.one * r;
                leafGroup.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(col);
                Object.Destroy(leafGroup.GetComponent<Collider>());

                // Small foliage noise spheres
                for (int s = 0; s < 3; s++)
                {
                    var subL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    subL.name = "SubLeaves";
                    subL.transform.SetParent(leafGroup.transform, false);
                    subL.transform.localPosition = new Vector3(
                        Random.Range(-0.3f, 0.3f),
                        Random.Range(-0.3f, 0.3f),
                        Random.Range(-0.3f, 0.3f)
                    );
                    subL.transform.localScale = Vector3.one * Random.Range(0.4f, 0.6f);
                    subL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(col);
                    Object.Destroy(subL.GetComponent<Collider>());
                }
            }

            // Hanging golden fairy lanterns (glowing tree fruits)
            for (int i = 0; i < 6; i++)
            {
                var lightObj = new GameObject("MagicalTreeLight");
                lightObj.transform.SetParent(goldTree.transform, false);
                lightObj.transform.localPosition = new Vector3(
                    Random.Range(-1.3f, 1.3f),
                    Random.Range(1.1f, 2.4f),
                    Random.Range(-1.3f, 1.3f)
                );

                var glowBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                glowBall.name = "GlowSphere";
                glowBall.transform.SetParent(lightObj.transform, false);
                glowBall.transform.localPosition = Vector3.zero;
                glowBall.transform.localScale = Vector3.one * 0.12f;
                glowBall.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.95f, 0.5f));
                Object.Destroy(glowBall.GetComponent<Collider>());

                var pointLight = lightObj.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.color = new Color(1.0f, 0.85f, 0.4f);
                pointLight.range = 3.8f;
                pointLight.intensity = 1.3f;
            }
        }

        private static void CreateBench(GameObject parent, Vector3 localPos, float rotY)
        {
            var benchGroup = new GameObject("RusticBench");
            benchGroup.transform.SetParent(parent.transform, false);
            benchGroup.transform.localPosition = localPos;
            benchGroup.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            var woodColor = new Color(0.42f, 0.28f, 0.15f);
            var legColor = new Color(0.2f, 0.2f, 0.2f);

            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "BenchSeat_Collider";
            seat.transform.SetParent(benchGroup.transform, false);
            seat.transform.localPosition = new Vector3(0, 0.25f, 0);
            seat.transform.localScale = new Vector3(1.3f, 0.08f, 0.45f);
            seat.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "BenchBackrest";
            back.transform.SetParent(benchGroup.transform, false);
            back.transform.localPosition = new Vector3(0, 0.65f, 0.2f);
            back.transform.localScale = new Vector3(1.3f, 0.45f, 0.06f);
            back.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);
            Object.Destroy(back.GetComponent<Collider>());

            float[] lx = { -0.55f, 0.55f };
            float[] lz = { -0.18f, 0.18f };
            foreach (float x in lx)
            {
                foreach (float z in lz)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "BenchLeg";
                    leg.transform.SetParent(benchGroup.transform, false);
                    leg.transform.localPosition = new Vector3(x, 0.12f, z);
                    leg.transform.localScale = new Vector3(0.05f, 0.12f, 0.05f);
                    leg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(legColor);
                    Object.Destroy(leg.GetComponent<Collider>());
                }
            }
        }

        private static void CreateLantern(GameObject parent, Vector3 localPos)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "LanternPost_Collider";
            post.transform.SetParent(parent.transform, false);
            post.transform.localPosition = localPos + new Vector3(0, 0.9f, 0);
            post.transform.localScale = new Vector3(0.06f, 0.9f, 0.06f);
            post.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.22f, 0.22f, 0.25f));

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "LanternCap";
            cap.transform.SetParent(parent.transform, false);
            cap.transform.localPosition = localPos + new Vector3(0, 1.85f, 0);
            cap.transform.localScale = new Vector3(0.28f, 0.06f, 0.28f);
            cap.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.22f, 0.22f, 0.25f));
            Object.Destroy(cap.GetComponent<Collider>());

            var lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lantern.name = "FairyLantern";
            lantern.transform.SetParent(parent.transform, false);
            lantern.transform.localPosition = localPos + new Vector3(0, 1.68f, 0);
            lantern.transform.localScale = Vector3.one * 0.22f;
            lantern.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.88f, 0.5f));
            Object.Destroy(lantern.GetComponent<Collider>());

            var lampLightObj = new GameObject("FairyLight");
            lampLightObj.transform.SetParent(lantern.transform, false);
            lampLightObj.transform.localPosition = Vector3.zero;
            var lampLight = lampLightObj.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.color = new Color(1.0f, 0.85f, 0.5f);
            lampLight.range = 5.5f;
            lampLight.intensity = 1.6f;
        }

        private static void CreateFlowerPatch(GameObject parent, Vector3 localPos, Color color, int count)
        {
            var patch = new GameObject("FlowerPatch");
            patch.transform.SetParent(parent.transform, false);
            patch.transform.localPosition = localPos;

            for (int f = 0; f < count; f++)
            {
                float ox = Random.Range(-0.35f, 0.35f);
                float oz = Random.Range(-0.35f, 0.35f);
                float r = Random.Range(0.12f, 0.18f);

                var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "FlowerStem";
                stem.transform.SetParent(patch.transform, false);
                stem.transform.localPosition = new Vector3(ox, 0.06f, oz);
                stem.transform.localScale = new Vector3(0.015f, 0.06f, 0.015f);
                stem.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.5f, 0.2f));
                Object.Destroy(stem.GetComponent<Collider>());

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "FlowerHead";
                head.transform.SetParent(patch.transform, false);
                head.transform.localPosition = new Vector3(ox, 0.12f, oz);
                head.transform.localScale = Vector3.one * r;
                head.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
                Object.Destroy(head.GetComponent<Collider>());
            }
        }

        private static void CreateBushesAndMushrooms(GameObject parent, Vector3 localPos)
        {
            var decoratorObj = new GameObject("FoliageGroup");
            decoratorObj.transform.SetParent(parent.transform, false);
            decoratorObj.transform.localPosition = localPos;

            var bushColor = new Color(0.2f, 0.5f, 0.22f);
            var mushroomStemColor = new Color(0.9f, 0.9f, 0.85f);
            var mushroomCapColor = new Color(0.85f, 0.25f, 0.2f);

            // Bush (3 overlapping spheres)
            for (int i = 0; i < 3; i++)
            {
                var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                b.name = "BushSphere";
                b.transform.SetParent(decoratorObj.transform, false);
                b.transform.localPosition = new Vector3(
                    (i - 1) * 0.2f + Random.Range(-0.05f, 0.05f),
                    0.15f + Random.Range(0f, 0.1f),
                    Random.Range(-0.1f, 0.1f)
                );
                b.transform.localScale = Vector3.one * Random.Range(0.35f, 0.5f);
                b.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(bushColor);
                Object.Destroy(b.GetComponent<Collider>());
            }

            // Mushrooms (2-3 mushrooms)
            int mushCount = Random.Range(2, 4);
            for (int m = 0; m < mushCount; m++)
            {
                float mx = 0.3f + m * 0.2f + Random.Range(-0.05f, 0.05f);
                float mz = Random.Range(-0.2f, 0.2f);

                var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "MushroomStem";
                stem.transform.SetParent(decoratorObj.transform, false);
                stem.transform.localPosition = new Vector3(mx, 0.05f, mz);
                stem.transform.localScale = new Vector3(0.03f, 0.05f, 0.03f);
                stem.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(mushroomStemColor);
                Object.Destroy(stem.GetComponent<Collider>());

                var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cap.name = "MushroomCap";
                cap.transform.SetParent(decoratorObj.transform, false);
                cap.transform.localPosition = new Vector3(mx, 0.11f, mz);
                cap.transform.localScale = new Vector3(0.12f, 0.06f, 0.12f);
                cap.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(mushroomCapColor);
                Object.Destroy(cap.GetComponent<Collider>());
            }
        }

        private static void CreateButterfliesAndParticles(GameObject parent, Vector3 localPos)
        {
            var fxParent = new GameObject("MagicalFX");
            fxParent.transform.SetParent(parent.transform, false);
            fxParent.transform.localPosition = localPos;

            // Spawn floating glowing particles
            int pCount = 5;
            for (int i = 0; i < pCount; i++)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.name = "MagicalParticle";
                p.transform.SetParent(fxParent.transform, false);
                p.transform.localPosition = new Vector3(
                    Random.Range(-1f, 1f),
                    0.5f + Random.Range(0f, 1.2f),
                    Random.Range(-1f, 1f)
                );
                p.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);
                p.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1f, 0.98f, 0.6f, 0.8f));
                Object.Destroy(p.GetComponent<Collider>());
            }

            // Spawn butterflies (low-poly double cubes)
            Color[] bfColors = {
                new(0.4f, 0.7f, 1.0f), new(1.0f, 0.5f, 0.8f), new(1.0f, 0.85f, 0.3f)
            };
            int bfCount = 3;
            for (int b = 0; b < bfCount; b++)
            {
                var bf = new GameObject("Butterfly");
                bf.transform.SetParent(fxParent.transform, false);
                bf.transform.localPosition = new Vector3(
                    Random.Range(-0.8f, 0.8f),
                    0.6f + Random.Range(0f, 0.8f),
                    Random.Range(-0.8f, 0.8f)
                );

                var wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wingL.name = "WingL";
                wingL.transform.SetParent(bf.transform, false);
                wingL.transform.localPosition = new Vector3(-0.04f, 0, 0);
                wingL.transform.localScale = new Vector3(0.06f, 0.01f, 0.06f);
                wingL.transform.localRotation = Quaternion.Euler(0, 0, 30f);
                wingL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(bfColors[b % bfColors.Length]);
                Object.Destroy(wingL.GetComponent<Collider>());

                var wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wingR.name = "WingR";
                wingR.transform.SetParent(bf.transform, false);
                wingR.transform.localPosition = new Vector3(0.04f, 0, 0);
                wingR.transform.localScale = new Vector3(0.06f, 0.01f, 0.06f);
                wingR.transform.localRotation = Quaternion.Euler(0, 0, -30f);
                wingR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(bfColors[b % bfColors.Length]);
                Object.Destroy(wingR.GetComponent<Collider>());
            }
        }

        private static void CreateCloudCluster(GameObject parent, Vector3 localPos, Vector3 scale)
        {
            var cluster = new GameObject("CloudCluster");
            cluster.transform.SetParent(parent.transform, false);
            cluster.transform.localPosition = localPos;

            int spheres = 6;
            for (int i = 0; i < spheres; i++)
            {
                var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                s.name = "CloudSphere";
                s.transform.SetParent(cluster.transform, false);
                s.transform.localPosition = new Vector3(
                    Random.Range(-0.4f, 0.4f) * scale.x,
                    Random.Range(-0.15f, 0.15f) * scale.y,
                    Random.Range(-0.4f, 0.4f) * scale.z
                );
                s.transform.localScale = new Vector3(
                    scale.x * Random.Range(0.7f, 1.25f),
                    scale.y * Random.Range(0.8f, 1.15f),
                    scale.z * Random.Range(0.7f, 1.25f)
                );
                s.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.96f, 0.96f, 0.98f, 0.95f));
                Object.Destroy(s.GetComponent<Collider>());
            }
        }

        private static void CreateBackgroundCloudSea(GameObject zone)
        {
            var cloudsBelow = new GameObject("SeaOfClouds");
            cloudsBelow.transform.SetParent(zone.transform, false);

            var cloudBelowData = new (Vector3 pos, Vector3 scale)[] {
                (new(-8f, -4.5f, -6f), new(5f, 1.4f, 4f)),
                (new(8f, -4.8f, -3f), new(4.5f, 1.2f, 3.5f)),
                (new(-7f, -5.2f, 6f), new(5.5f, 1.6f, 4.5f)),
                (new(7f, -5.5f, 8f), new(6f, 1.8f, 5f)),
                (new(0f, -6.0f, 0f), new(8f, 2.0f, 7f)),
                (new(-3f, -4.2f, -8f), new(4f, 1.0f, 3.2f)),
                (new(3f, -4.2f, 8f), new(4f, 1.0f, 3.2f))
            };
            foreach (var (pos, scale) in cloudBelowData)
            {
                CreateCloudCluster(cloudsBelow, pos, scale);
            }
        }

        private static void BuildFloralArch(GameObject arch)
        {
            var woodColor = new Color(0.48f, 0.32f, 0.18f);

            var archL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            archL.name = "ArchL_Collider";
            archL.transform.SetParent(arch.transform, false);
            archL.transform.localPosition = new Vector3(-1.1f, 1.1f, 0);
            archL.transform.localScale = new Vector3(0.08f, 1.1f, 0.08f);
            archL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var archR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            archR.name = "ArchR_Collider";
            archR.transform.SetParent(arch.transform, false);
            archR.transform.localPosition = new Vector3(1.1f, 1.1f, 0);
            archR.transform.localScale = new Vector3(0.08f, 1.1f, 0.08f);
            archR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var archTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archTop.name = "ArchTop";
            archTop.transform.SetParent(arch.transform, false);
            archTop.transform.localPosition = new Vector3(0, 2.2f, 0);
            archTop.transform.localScale = new Vector3(2.3f, 0.08f, 0.15f);
            archTop.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);
            Object.Destroy(archTop.GetComponent<Collider>());

            Color[] flowerColors = { new(0.96f, 0.45f, 0.62f), new(0.9f, 0.2f, 0.2f) };
            for (int i = 0; i < 6; i++)
            {
                float fy = 0.5f + i * 0.35f;
                var flL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flL.name = "ArchFlowerL";
                flL.transform.SetParent(arch.transform, false);
                flL.transform.localPosition = new Vector3(-1.1f + Random.Range(-0.05f, 0.05f), fy, Random.Range(-0.05f, 0.05f));
                flL.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);
                flL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(flowerColors[i % 2]);
                Object.Destroy(flL.GetComponent<Collider>());

                var flR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flR.name = "ArchFlowerR";
                flR.transform.SetParent(arch.transform, false);
                flR.transform.localPosition = new Vector3(1.1f + Random.Range(-0.05f, 0.05f), fy, Random.Range(-0.05f, 0.05f));
                flR.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);
                flR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(flowerColors[i % 2]);
                Object.Destroy(flR.GetComponent<Collider>());
            }
        }

        private static void BuildWishingWell(GameObject well)
        {
            var woodColor = new Color(0.48f, 0.32f, 0.18f);
            var stoneColor = new Color(0.72f, 0.72f, 0.75f);

            var wellBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wellBase.name = "WellBase_Collider";
            wellBase.transform.SetParent(well.transform, false);
            wellBase.transform.localPosition = new Vector3(0, 0.4f, 0);
            wellBase.transform.localScale = new Vector3(1.2f, 0.4f, 1.2f);
            wellBase.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor * 0.9f);

            var wellWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wellWater.name = "WellWater";
            wellWater.transform.SetParent(well.transform, false);
            wellWater.transform.localPosition = new Vector3(0, 0.78f, 0);
            wellWater.transform.localScale = new Vector3(1.05f, 0.01f, 1.05f);
            wellWater.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.55f, 0.9f, 0.85f));
            Object.Destroy(wellWater.GetComponent<Collider>());

            var pillarL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillarL.name = "WellPillarL";
            pillarL.transform.SetParent(well.transform, false);
            pillarL.transform.localPosition = new Vector3(-0.48f, 1.1f, 0);
            pillarL.transform.localScale = new Vector3(0.06f, 0.7f, 0.06f);
            pillarL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);
            Object.Destroy(pillarL.GetComponent<Collider>());

            var pillarR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillarR.name = "WellPillarR";
            pillarR.transform.SetParent(well.transform, false);
            pillarR.transform.localPosition = new Vector3(0.48f, 1.1f, 0);
            pillarR.transform.localScale = new Vector3(0.06f, 0.7f, 0.06f);
            pillarR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);
            Object.Destroy(pillarR.GetComponent<Collider>());

            var roofL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofL.name = "WellRoofL";
            roofL.transform.SetParent(well.transform, false);
            roofL.transform.localPosition = new Vector3(-0.35f, 1.9f, 0);
            roofL.transform.localScale = new Vector3(0.8f, 0.06f, 1.2f);
            roofL.transform.localRotation = Quaternion.Euler(0, 0, 30f);
            roofL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor * 0.8f);
            Object.Destroy(roofL.GetComponent<Collider>());

            var roofR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofR.name = "WellRoofR";
            roofR.transform.SetParent(well.transform, false);
            roofR.transform.localPosition = new Vector3(0.35f, 1.9f, 0);
            roofR.transform.localScale = new Vector3(0.8f, 0.06f, 1.2f);
            roofR.transform.localRotation = Quaternion.Euler(0, 0, -30f);
            roofR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor * 0.8f);
            Object.Destroy(roofR.GetComponent<Collider>());
        }
    }
}
