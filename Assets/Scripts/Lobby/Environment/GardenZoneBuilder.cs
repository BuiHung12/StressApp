using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Builds the Garden Zone (floating clouds, planting plots, golden tree).
    /// Redesigned from scratch as a Ghibli/Zelda-inspired Ancient Sky Sanctuary.
    /// Located at world Z = +60.
    /// </summary>
    public static class GardenZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("GardenZone");
            zone.transform.position = new Vector3(0, 0, 60f);

            var grassColor = new Color(0.28f, 0.58f, 0.18f);
            var stoneColor = new Color(0.6f, 0.6f, 0.62f);
            var woodColor = new Color(0.42f, 0.28f, 0.15f);
            var goldColor = new Color(1.0f, 0.85f, 0.15f);

            // ── 1. Sky Background (Clouds & Far Distant Ruins) ──
            CreateSkyBackground(zone);

            // ── 2. Island 1: Ruined Entrance Plaza (Z = -5.5f, Y = 0) ──
            var island1 = CreateSculptedRuinedIsland(zone, "EntrancePlaza", new Vector3(0, 0, -5.5f), 3.2f, 1.4f, grassColor);

            // Ancient Stone Arch wrapping the portal
            var entranceArch = new GameObject("EntranceArch");
            entranceArch.transform.SetParent(island1.transform, false);
            entranceArch.transform.localPosition = new Vector3(0, 0.05f, -2.6f);
            BuildRuinedArch(entranceArch);

            // Wishing Well Shrine
            var well = new GameObject("WishingWell");
            well.transform.SetParent(island1.transform, false);
            well.transform.localPosition = new Vector3(-2.2f, 0.05f, 1.2f);
            BuildWishingWell(well);

            // Cozy Reading Stole (Reading corner)
            CreateReadingStump(island1, new Vector3(2.0f, 0.05f, 1.4f));

            // Fairy lanterns
            CreateFairyLantern(island1, new Vector3(-2.4f, 0.05f, -1.8f));
            CreateFairyLantern(island1, new Vector3(2.4f, 0.05f, -1.8f));

            // Flower fields (clumpy groups)
            CreateClumpedFlowers(island1, new Vector3(-1.8f, 0.05f, -0.8f), new Color[] { new(0.96f, 0.45f, 0.62f), new(1.0f, 0.92f, 0.95f) }, 1.2f, 25);
            CreateClumpedFlowers(island1, new Vector3(1.8f, 0.05f, -0.6f), new Color[] { new(0.95f, 0.8f, 0.2f), new(0.9f, 0.5f, 0.1f) }, 1.2f, 25);

            // Foliage & Butterflies
            CreateFoliage(island1, new Vector3(2.0f, 0.05f, -0.2f));
            CreateFoliage(island1, new Vector3(-1.5f, 0.05f, -2.2f));
            CreateButterfliesAndGlow(island1, new Vector3(0, 0.6f, -1.0f));

            // ── 3. Island 2: Hanging Waterfall Terrace (Z = 0.2f, X = 3.5f, Y = 0.9f) ──
            var island2 = CreateSculptedRuinedIsland(zone, "WaterfallTerrace", new Vector3(3.5f, 0.9f, 0.2f), 3.8f, 1.6f, grassColor);

            // Double Cascade Waterfall
            CreateWaterfall(island2, new Vector3(-2.8f, 0.05f, -0.2f), 4.5f);

            // Cozy campfire site
            CreateCampfire(island2, new Vector3(0f, 0.05f, -1.8f));

            // Viewpoint Bench & Tea Table
            CreateRusticBench(island2, new Vector3(1.8f, 0.05f, 1.5f), -35f);
            var teaTable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            teaTable.name = "TeaTable";
            teaTable.transform.SetParent(island2.transform, false);
            teaTable.transform.localPosition = new Vector3(1.0f, 0.3f, 0.8f);
            teaTable.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            teaTable.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);

            var cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cup.name = "TeaCup";
            cup.transform.SetParent(teaTable.transform, false);
            cup.transform.localPosition = new Vector3(0, 0.55f, 0);
            cup.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            cup.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.9f, 0.95f));
            Object.Destroy(cup.GetComponent<Collider>());

            // Fairy Lantern
            CreateFairyLantern(island2, new Vector3(2.6f, 0.05f, -1.8f));

            // Stone steps path
            Vector3[] steps = { new(-1.2f, 0.02f, -1.5f), new(-0.5f, 0.02f, -0.8f), new(0.2f, 0.02f, -0.2f) };
            foreach (var step in steps)
            {
                var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = "PathSlab";
                slab.transform.SetParent(island2.transform, false);
                slab.transform.localPosition = step;
                slab.transform.localScale = new Vector3(0.7f, 0.02f, 0.5f);
                slab.transform.localRotation = Quaternion.Euler(0, Random.Range(-15f, 15f), 0);
                slab.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
                Object.Destroy(slab.GetComponent<Collider>());
            }

            // Flowers
            CreateClumpedFlowers(island2, new Vector3(2.4f, 0.05f, 0.5f), new Color[] { new(0.4f, 0.75f, 1.0f), new(0.9f, 0.95f, 0.95f) }, 1.4f, 30);
            CreateClumpedFlowers(island2, new Vector3(-1.8f, 0.05f, 1.5f), new Color[] { new(0.75f, 0.4f, 0.85f), new(0.95f, 0.5f, 0.7f) }, 1.2f, 25);
            CreateFoliage(island2, new Vector3(2.2f, 0.05f, -1.0f));

            // ── 4. Island 3: Sacred World Tree Shrine (Z = 5.5f, X = -1.2f, Y = 1.8f) ──
            var island3 = CreateSculptedRuinedIsland(zone, "WorldTreeShrine", new Vector3(-1.2f, 1.8f, 5.5f), 3.5f, 1.8f, grassColor);

            // Sacred Golden Tree wrapping ruined columns
            CreateSacredGoldenTree(island3, new Vector3(0, 0.05f, 1.2f));

            // Incense Altar Shrine
            CreateIncenseShrine(island3, new Vector3(1.8f, 0.05f, -1.2f));

            // Lanterns
            CreateFairyLantern(island3, new Vector3(-2.4f, 0.05f, -1.5f));
            CreateFairyLantern(island3, new Vector3(2.4f, 0.05f, -1.5f));

            // Flowers surrounding the tree base
            CreateClumpedFlowers(island3, new Vector3(-1.8f, 0.05f, 0.6f), new Color[] { goldColor, new(0.95f, 0.65f, 0.1f) }, 1.4f, 40);
            CreateClumpedFlowers(island3, new Vector3(1.8f, 0.05f, 0.6f), new Color[] { goldColor, new(1.0f, 0.95f, 0.5f) }, 1.4f, 40);

            CreateFoliage(island3, new Vector3(-2.2f, 0.05f, -0.8f));
            CreateFoliage(island3, new Vector3(2.2f, 0.05f, -0.8f));
            CreateButterfliesAndGlow(island3, new Vector3(0, 0.8f, 0.5f));

            // ── 5. Wooden Bridges ──
            CreateSuspensionBridge(zone, new Vector3(1.2f, 0.02f, -2.8f), new Vector3(2.2f, 0.92f, -1.4f), 10);
            CreateSuspensionBridge(zone, new Vector3(2.2f, 0.92f, 2.0f), new Vector3(0.0f, 1.82f, 3.8f), 10);

            // ── 6. Cloud Lock Barriers (Blocking the Bridges) ──
            var barrier2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier2.name = "Stair1_Barrier_Collider";
            barrier2.transform.SetParent(zone.transform, false);
            barrier2.transform.localPosition = new Vector3(1.7f, 0.5f, -2.1f);
            barrier2.transform.localScale = new Vector3(1.8f, 0.9f, 0.2f);
            barrier2.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
            var lock2 = barrier2.AddComponent<CloudLayer>();
            ZoneFactory.SetField(lock2, "_unlockCost", 10);
            ZoneFactory.SetField(lock2, "_barrierObj", barrier2);

            var barrier3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier3.name = "Stair2_Barrier_Collider";
            barrier3.transform.SetParent(zone.transform, false);
            barrier3.transform.localPosition = new Vector3(1.1f, 1.35f, 2.9f);
            barrier3.transform.localScale = new Vector3(1.8f, 0.9f, 0.2f);
            barrier3.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
            var lock3 = barrier3.AddComponent<CloudLayer>();
            ZoneFactory.SetField(lock3, "_unlockCost", 25);
            ZoneFactory.SetField(lock3, "_barrierObj", barrier3);

            // ── 7. Planting Plots ──
            CreatePlot(zone, new Vector3(-1.8f, 0.05f, -4.5f), 10f, 10);
            CreatePlot(zone, new Vector3(1.8f, 0.05f, -4.5f), 10f, 10);
            CreatePlot(zone, new Vector3(2.2f, 0.95f, 0.5f), 15f, 25);
            CreatePlot(zone, new Vector3(4.2f, 0.95f, -0.5f), 15f, 25);
            CreatePlot(zone, new Vector3(-2.2f, 1.85f, 4.2f), 20f, 50);
            CreatePlot(zone, new Vector3(0.2f, 1.85f, 4.2f), 20f, 50);

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

        private static GameObject CreateSculptedRuinedIsland(GameObject parent, string name, Vector3 localPos, float radius, float height, Color grassCol)
        {
            var island = new GameObject(name);
            island.transform.SetParent(parent.transform, false);
            island.transform.localPosition = localPos;

            var dirtCol = new Color(0.35f, 0.28f, 0.22f);
            var stoneColor = new Color(0.62f, 0.62f, 0.65f);

            // 1. Sculpting irregular, tiered terrain shape using multiple grass spheres & dirt cylinders
            int grassClusters = 16;
            for (int i = 0; i < grassClusters; i++)
            {
                float angle = (i * (360f / grassClusters) + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
                float dist = radius * Random.Range(0.2f, 0.75f);
                Vector3 offset = new Vector3(Mathf.Cos(angle) * dist, Random.Range(-0.06f, 0.08f), Mathf.Sin(angle) * dist);
                float rScale = radius * Random.Range(0.45f, 0.75f);

                var gm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gm.name = "GrassMound";
                gm.transform.SetParent(island.transform, false);
                gm.transform.localPosition = offset;
                gm.transform.localScale = new Vector3(rScale * 2f, 0.25f, rScale * 2f);
                gm.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(grassCol);
                Object.Destroy(gm.GetComponent<Collider>());

                var dm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dm.name = "DirtChunk_Collider";
                dm.transform.SetParent(island.transform, false);
                dm.transform.localPosition = offset + Vector3.down * (height * 0.5f);
                dm.transform.localScale = new Vector3(rScale * 1.9f, height * Random.Range(0.8f, 1.15f), rScale * 1.9f);
                dm.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(dirtCol);
            }

            // 2. Ruined stone platforms embedded in the grass
            int ruinsCount = Random.Range(3, 6);
            for (int i = 0; i < ruinsCount; i++)
            {
                var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = "RuinedSlab_Collider";
                slab.transform.SetParent(island.transform, false);
                slab.transform.localPosition = new Vector3(
                    Random.Range(-radius * 0.6f, radius * 0.6f),
                    0.02f,
                    Random.Range(-radius * 0.6f, radius * 0.6f)
                );
                slab.transform.localScale = new Vector3(
                    Random.Range(0.8f, 1.5f),
                    0.1f,
                    Random.Range(0.8f, 1.5f)
                );
                slab.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                slab.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor * 0.85f);
            }

            // 3. Exposed granite cliff rocks around edges to break cylinders
            int rockCliffs = Random.Range(8, 13);
            for (int i = 0; i < rockCliffs; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float rDist = radius * Random.Range(0.85f, 1.1f);
                Vector3 rockPos = new Vector3(Mathf.Cos(angle) * rDist, Random.Range(-height * 0.4f, 0.1f), Mathf.Sin(angle) * rDist);

                var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = "CliffRock_Collider";
                rock.transform.SetParent(island.transform, false);
                rock.transform.localPosition = rockPos;
                rock.transform.localScale = new Vector3(
                    Random.Range(0.7f, 1.4f),
                    Random.Range(0.6f, 1.6f),
                    Random.Range(0.7f, 1.4f)
                );
                rock.transform.localRotation = Quaternion.Euler(
                    Random.Range(-25f, 25f),
                    Random.Range(0f, 360f),
                    Random.Range(-25f, 25f)
                );
                rock.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
            }

            // 4. Roots hanging from below
            int rootCount = Random.Range(12, 18);
            for (int i = 0; i < rootCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = radius * Random.Range(0.15f, 0.8f);
                Vector3 rootPos = new Vector3(Mathf.Cos(angle) * dist, -height * Random.Range(0.8f, 1.2f), Mathf.Sin(angle) * dist);

                var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                root.name = "UnderRoot";
                root.transform.SetParent(island.transform, false);
                root.transform.localPosition = rootPos;
                float len = Random.Range(0.6f, 1.5f);
                root.transform.localScale = new Vector3(0.05f, len, 0.05f);
                root.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
                root.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(dirtCol * 0.65f);
                Object.Destroy(root.GetComponent<Collider>());
            }

            // 5. Ivy vines trailing down cliffs
            int vineCount = Random.Range(9, 14);
            for (int i = 0; i < vineCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = radius * 0.98f;
                Vector3 vinePos = new Vector3(Mathf.Cos(angle) * dist, -0.05f, Mathf.Sin(angle) * dist);

                var vine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                vine.name = "HangingIvy";
                vine.transform.SetParent(island.transform, false);
                vine.transform.localPosition = vinePos + Vector3.down * 0.4f;
                float len = Random.Range(0.45f, 0.9f);
                vine.transform.localScale = new Vector3(0.04f, len, 0.04f);
                vine.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.45f, 0.18f));
                Object.Destroy(vine.GetComponent<Collider>());
            }

            // 6. Tiny floating rocks nearby
            int floatRocks = Random.Range(3, 6);
            for (int i = 0; i < floatRocks; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float rDist = radius * Random.Range(1.3f, 1.7f);
                Vector3 fpos = new Vector3(Mathf.Cos(angle) * rDist, Random.Range(-0.8f, 0.6f), Mathf.Sin(angle) * rDist);

                var fr = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fr.name = "FloatingRock_Collider";
                fr.transform.SetParent(island.transform, false);
                fr.transform.localPosition = fpos;
                fr.transform.localScale = Vector3.one * Random.Range(0.25f, 0.55f);
                fr.transform.localRotation = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
                fr.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor * 0.95f);
            }

            // 7. Fallen leaves decoration
            int leavesCount = Random.Range(15, 25);
            for (int k = 0; k < leavesCount; k++)
            {
                float rx = Random.Range(-radius * 0.8f, radius * 0.8f);
                float rz = Random.Range(-radius * 0.8f, radius * 0.8f);
                if (new Vector2(rx, rz).magnitude <= radius)
                {
                    var leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leaf.name = "FallenLeaf";
                    leaf.transform.SetParent(island.transform, false);
                    leaf.transform.localPosition = new Vector3(rx, 0.03f, rz);
                    leaf.transform.localScale = new Vector3(0.06f, 0.005f, 0.08f);
                    leaf.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    Color leafColor = Random.Range(0, 2) == 0 ? new Color(1.0f, 0.8f, 0.2f) : new Color(0.9f, 0.5f, 0.1f);
                    leaf.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(leafColor);
                    Object.Destroy(leaf.GetComponent<Collider>());
                }
            }

            return island;
        }

        private static void CreateWaterfall(GameObject island, Vector3 localEdgePos, float dropHeight)
        {
            var waterParent = new GameObject("Waterfall");
            waterParent.transform.SetParent(island.transform, false);

            var stoneColor = new Color(0.58f, 0.58f, 0.6f);
            var waterColor = new Color(0.4f, 0.72f, 0.92f, 0.8f);

            // Double overlapping pond base
            var pond1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pond1.name = "PondWater1";
            pond1.transform.SetParent(waterParent.transform, false);
            pond1.transform.localPosition = localEdgePos + new Vector3(0, 0.02f, 0.4f);
            pond1.transform.localScale = new Vector3(1.3f, 0.01f, 1.3f);
            pond1.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(waterColor);
            Object.Destroy(pond1.GetComponent<Collider>());

            var pond2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pond2.name = "PondWater2";
            pond2.transform.SetParent(waterParent.transform, false);
            pond2.transform.localPosition = localEdgePos + new Vector3(0.3f, 0.02f, 0.6f);
            pond2.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);
            pond2.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(waterColor);
            Object.Destroy(pond2.GetComponent<Collider>());

            // Stone borders
            for (int i = 0; i < 9; i++)
            {
                float angle = (i * (360f / 9f)) * Mathf.Deg2Rad;
                var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = "PondRock_Collider";
                rock.transform.SetParent(waterParent.transform, false);
                rock.transform.localPosition = localEdgePos + new Vector3(Mathf.Cos(angle) * 0.8f, 0.08f, 0.4f + Mathf.Sin(angle) * 0.8f);
                rock.transform.localScale = new Vector3(
                    Random.Range(0.2f, 0.5f),
                    Random.Range(0.15f, 0.4f),
                    Random.Range(0.2f, 0.5f)
                );
                rock.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0, 360f), Random.Range(-10f, 10f));
                rock.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
            }

            // Wet shiny stones
            for (int i = 0; i < 4; i++)
            {
                var wetStone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wetStone.name = "WetStone";
                wetStone.transform.SetParent(waterParent.transform, false);
                wetStone.transform.localPosition = localEdgePos + new Vector3(Random.Range(-0.4f, 0.4f), 0.04f, 0.4f + Random.Range(-0.4f, 0.4f));
                wetStone.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);
                wetStone.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor * 0.72f);
                Object.Destroy(wetStone.GetComponent<Collider>());
            }

            // Lily pads
            for (int i = 0; i < 4; i++)
            {
                var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pad.name = "LilyPad";
                pad.transform.SetParent(waterParent.transform, false);
                pad.transform.localPosition = localEdgePos + new Vector3(Random.Range(-0.4f, 0.4f), 0.03f, 0.4f + Random.Range(-0.4f, 0.4f));
                pad.transform.localScale = new Vector3(0.18f, 0.005f, 0.18f);
                pad.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.18f, 0.48f, 0.22f));
                Object.Destroy(pad.GetComponent<Collider>());

                if (i == 0)
                {
                    var lotus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    lotus.name = "LotusFlower";
                    lotus.transform.SetParent(pad.transform, false);
                    lotus.transform.localPosition = new Vector3(0, 5.0f, 0);
                    lotus.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
                    lotus.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.96f, 0.5f, 0.75f));
                    Object.Destroy(lotus.GetComponent<Collider>());
                }
            }

            // Wooden dock
            var dock = new GameObject("PondDock");
            dock.transform.SetParent(waterParent.transform, false);
            dock.transform.localPosition = localEdgePos + new Vector3(0.6f, 0.03f, 0.1f);
            dock.transform.localRotation = Quaternion.Euler(0, 45f, 0);

            for (int p = 0; p < 4; p++)
            {
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = "DockPlank";
                plank.transform.SetParent(dock.transform, false);
                plank.transform.localPosition = new Vector3(0, 0, p * 0.16f);
                plank.transform.localScale = new Vector3(0.55f, 0.03f, 0.14f);
                plank.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.42f, 0.28f, 0.15f));
                Object.Destroy(plank.GetComponent<Collider>());
            }

            // Multi-sheet waterfall flow
            float[] offsets = { -0.2f, 0f, 0.2f };
            float[] widths = { 0.28f, 0.35f, 0.28f };
            float[] depths = { 0.04f, 0.06f, 0.04f };
            for (int s = 0; s < 3; s++)
            {
                var sheet = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sheet.name = "WaterFlowSheet";
                sheet.transform.SetParent(waterParent.transform, false);
                sheet.transform.localPosition = localEdgePos + new Vector3(offsets[s], -dropHeight * 0.5f, 0.05f + s * 0.01f);
                sheet.transform.localScale = new Vector3(widths[s], dropHeight, depths[s]);
                sheet.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.85f, 0.95f, 1.0f, 0.8f));
                Object.Destroy(sheet.GetComponent<Collider>());
            }

            // Foam splashes at the bottom
            var splashParent = new GameObject("SplashFoam");
            splashParent.transform.SetParent(waterParent.transform, false);
            splashParent.transform.localPosition = localEdgePos + new Vector3(0, -dropHeight, 0.05f);

            for (int i = 0; i < 7; i++)
            {
                var foam = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foam.name = "SplashFoamSphere";
                foam.transform.SetParent(splashParent.transform, false);
                foam.transform.localPosition = new Vector3(
                    Random.Range(-0.45f, 0.45f),
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f)
                );
                foam.transform.localScale = Vector3.one * Random.Range(0.18f, 0.42f);
                foam.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(Color.white);
                Object.Destroy(foam.GetComponent<Collider>());
            }
        }

        private static void CreateSuspensionBridge(GameObject zone, Vector3 start, Vector3 end, int plankCount)
        {
            var bridge = new GameObject("FantasyBridge");
            bridge.transform.SetParent(zone.transform, false);

            var woodColor = new Color(0.42f, 0.28f, 0.15f);
            var ropeColor = new Color(0.72f, 0.62f, 0.46f);

            // Compute midpoint with sag
            Vector3 mid = (start + end) * 0.5f + Vector3.down * 0.38f;

            // 1. Planks
            for (int i = 0; i < plankCount; i++)
            {
                float t = (float)i / (plankCount - 1);
                Vector3 pos = GetBezierPoint(start, mid, end, t);
                Vector3 tangent = GetBezierTangent(start, mid, end, t);

                pos += new Vector3(0, Random.Range(-0.015f, 0.015f), 0);
                float rotErrY = Random.Range(-4f, 4f);
                float rotErrX = Random.Range(-2f, 2f);
                Quaternion rot = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(rotErrX, 90f + rotErrY, 0);

                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = "BridgePlank_Collider";
                plank.transform.SetParent(bridge.transform, false);
                plank.transform.localPosition = pos;
                plank.transform.localScale = new Vector3(
                    Random.Range(1.4f, 1.75f),
                    0.06f,
                    Random.Range(0.22f, 0.28f)
                );
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

            // 3. Diagonal support beams
            var beamL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beamL.name = "SupportBeam";
            beamL.transform.SetParent(bridge.transform, false);
            beamL.transform.localPosition = start - rightOffset * 0.5f + Vector3.down * 0.3f;
            beamL.transform.localScale = new Vector3(0.06f, 0.4f, 0.06f);
            beamL.transform.localRotation = Quaternion.Euler(45f, 0, 0);
            beamL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor * 0.72f);
            Object.Destroy(beamL.GetComponent<Collider>());

            var beamR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beamR.name = "SupportBeam";
            beamR.transform.SetParent(bridge.transform, false);
            beamR.transform.localPosition = start + rightOffset * 0.5f + Vector3.down * 0.3f;
            beamR.transform.localScale = new Vector3(0.06f, 0.4f, 0.06f);
            beamR.transform.localRotation = Quaternion.Euler(-45f, 0, 0);
            beamR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor * 0.72f);
            Object.Destroy(beamR.GetComponent<Collider>());

            // 4. Double rope railings
            int ropeSegments = 16;
            for (int i = 0; i < ropeSegments; i++)
            {
                float t1 = (float)i / ropeSegments;
                float t2 = (float)(i + 1) / ropeSegments;

                Vector3 p1 = GetBezierPoint(start, mid, end, t1);
                Vector3 p2 = GetBezierPoint(start, mid, end, t2);

                // Handrail rope
                Vector3 lp1 = p1 - rightOffset + Vector3.up * 0.45f;
                Vector3 lp2 = p2 - rightOffset + Vector3.up * 0.45f;
                CreateRopeSegment(bridge, lp1, lp2, ropeColor);

                Vector3 rp1 = p1 + rightOffset + Vector3.up * 0.45f;
                Vector3 rp2 = p2 + rightOffset + Vector3.up * 0.45f;
                CreateRopeSegment(bridge, rp1, rp2, ropeColor);

                // Mid rope
                Vector3 mlp1 = p1 - rightOffset + Vector3.up * 0.22f;
                Vector3 mlp2 = p2 - rightOffset + Vector3.up * 0.22f;
                CreateRopeSegment(bridge, mlp1, mlp2, ropeColor);

                Vector3 mrp1 = p1 + rightOffset + Vector3.up * 0.22f;
                Vector3 mrp2 = p2 + rightOffset + Vector3.up * 0.22f;
                CreateRopeSegment(bridge, mrp1, mrp2, ropeColor);
            }

            // 5. Hanging lantern
            Vector3 midPoint = GetBezierPoint(start, mid, end, 0.5f);
            var bridgeLight = new GameObject("BridgeLantern");
            bridgeLight.transform.SetParent(bridge.transform, false);
            bridgeLight.transform.localPosition = midPoint - rightOffset + Vector3.up * 0.35f;

            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "LanternGlow";
            glow.transform.SetParent(bridgeLight.transform, false);
            glow.transform.localPosition = Vector3.down * 0.15f;
            glow.transform.localScale = Vector3.one * 0.15f;
            glow.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.88f, 0.5f));
            Object.Destroy(glow.GetComponent<Collider>());

            var lLight = bridgeLight.AddComponent<Light>();
            lLight.type = LightType.Point;
            lLight.color = new Color(1.0f, 0.85f, 0.5f);
            lLight.range = 3.5f;
            lLight.intensity = 1.2f;
        }

        private static void CreateRopeSegment(GameObject parent, Vector3 p1, Vector3 p2, Color color)
        {
            var segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = "BridgeRope";
            segment.transform.SetParent(parent.transform, false);

            Vector3 diff = p2 - p1;
            segment.transform.localPosition = p1 + diff * 0.5f;
            segment.transform.localScale = new Vector3(0.03f, diff.magnitude * 0.5f, 0.03f); // Cylinder height is 2
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

        private static void CreateSacredGoldenTree(GameObject parent, Vector3 localPos)
        {
            var goldTree = new GameObject("GhibliWorldTree");
            goldTree.transform.SetParent(parent.transform, false);
            goldTree.transform.localPosition = localPos;

            var trunkColor = new Color(0.38f, 0.25f, 0.12f);
            var goldLeaf = new Color(1.0f, 0.85f, 0.15f);
            var orangeLeaf = new Color(0.95f, 0.6f, 0.1f);
            var paleLeaf = new Color(1.0f, 0.95f, 0.45f);

            // 1. Ruined archway that the tree wraps around (Storytelling centerpiece)
            var archway = new GameObject("AncientArchway");
            archway.transform.SetParent(goldTree.transform, false);
            archway.transform.localPosition = Vector3.zero;

            var postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postL.transform.SetParent(archway.transform, false);
            postL.transform.localPosition = new Vector3(-0.45f, 0.8f, 0);
            postL.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
            postL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.55f, 0.55f, 0.58f));

            var postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postR.transform.SetParent(archway.transform, false);
            postR.transform.localPosition = new Vector3(0.45f, 0.8f, 0);
            postR.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
            postR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.55f, 0.55f, 0.58f));

            var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.transform.SetParent(archway.transform, false);
            lintel.transform.localPosition = new Vector3(0, 1.6f, 0);
            lintel.transform.localScale = new Vector3(1.1f, 0.12f, 0.18f);
            lintel.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.55f, 0.55f, 0.58f));

            // 2. Gnarled ancient trunk wrapping around the arches (24 segments)
            int trunkSegments = 24;
            for (int i = 0; i < trunkSegments; i++)
            {
                var tr = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tr.name = "TrunkSegment_Collider";
                tr.transform.SetParent(goldTree.transform, false);
                
                // Wrap math: spiral around the arch posts
                float wrapAngle = i * 0.4f;
                float tx = Mathf.Cos(wrapAngle) * 0.2f + (i < 12 ? -0.45f : 0.45f);
                float tz = Mathf.Sin(wrapAngle) * 0.12f;
                float ty = 0.1f + i * 0.11f;

                tr.transform.localPosition = new Vector3(tx, ty, tz);
                float scaleFac = 0.28f - i * 0.008f;
                tr.transform.localScale = new Vector3(scaleFac, 0.1f, scaleFac);
                tr.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
                tr.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(trunkColor);
            }

            // 3. Thick root flares spreading over archway base
            for (int i = 0; i < 5; i++)
            {
                float angle = (i * 72f) * Mathf.Deg2Rad;
                var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                root.name = "RootFlare";
                root.transform.SetParent(goldTree.transform, false);
                root.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.45f, 0.12f, Mathf.Sin(angle) * 0.45f);
                root.transform.localScale = new Vector3(0.12f, 0.22f, 0.12f);
                root.transform.localRotation = Quaternion.Euler(Mathf.Sin(angle) * 35f, 0f, -Mathf.Cos(angle) * 35f);
                root.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(trunkColor);
                Object.Destroy(root.GetComponent<Collider>());
            }

            // 4. Branch structures
            Vector3[] branchData = {
                new(-0.35f, 1.4f, 0.1f), new(0.3f, 1.6f, -0.2f), new(-0.1f, 1.7f, 0.4f),
                new(0.2f, 1.8f, 0.3f), new(-0.4f, 1.9f, -0.3f), new(0.05f, 2.1f, 0.5f)
            };
            Vector3[] branchScale = {
                new(0.12f, 0.45f, 0.12f), new(0.1f, 0.4f, 0.1f), new(0.1f, 0.4f, 0.1f),
                new(0.08f, 0.35f, 0.08f), new(0.08f, 0.35f, 0.08f), new(0.07f, 0.3f, 0.07f)
            };
            Quaternion[] branchRot = {
                Quaternion.Euler(0, 0, 50f), Quaternion.Euler(-45f, 0, -45f), Quaternion.Euler(45f, 45f, 0),
                Quaternion.Euler(30f, -30f, -30f), Quaternion.Euler(-30f, 60f, 50f), Quaternion.Euler(15f, 15f, 0)
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

            // 5. Layered canopy (32 spheres)
            var canopyData = new (Vector3 offset, float r, Color col)[] {
                (new(0, 2.3f, 0), 1.6f, goldLeaf),
                (new(-0.8f, 2.0f, 0.2f), 1.2f, orangeLeaf),
                (new(0.7f, 2.1f, -0.3f), 1.1f, orangeLeaf),
                (new(-0.2f, 2.2f, 0.8f), 1.1f, goldLeaf),
                (new(0.3f, 2.4f, 0.6f), 1.0f, paleLeaf),
                (new(-0.6f, 2.5f, -0.5f), 1.0f, paleLeaf),
                (new(0.1f, 2.9f, 0.1f), 1.3f, paleLeaf),
                (new(0.6f, 2.6f, -0.7f), 0.9f, orangeLeaf),
                (new(-0.7f, 2.7f, 0.7f), 0.9f, goldLeaf)
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

                for (int s = 0; s < 3; s++)
                {
                    var subL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    subL.name = "SubLeaves";
                    subL.transform.SetParent(leafGroup.transform, false);
                    subL.transform.localPosition = new Vector3(
                        Random.Range(-0.35f, 0.35f),
                        Random.Range(-0.35f, 0.35f),
                        Random.Range(-0.35f, 0.35f)
                    );
                    subL.transform.localScale = Vector3.one * Random.Range(0.4f, 0.6f);
                    subL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(col);
                    Object.Destroy(subL.GetComponent<Collider>());
                }
            }

            // 6. Hanging glowing fruits
            for (int i = 0; i < 8; i++)
            {
                var lightObj = new GameObject("MagicalTreeLight");
                lightObj.transform.SetParent(goldTree.transform, false);
                lightObj.transform.localPosition = new Vector3(
                    Random.Range(-1.4f, 1.4f),
                    Random.Range(1.1f, 2.4f),
                    Random.Range(-1.4f, 1.4f)
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

        private static void CreateRusticBench(GameObject parent, Vector3 localPos, float rotY)
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

        private static void CreateFairyLantern(GameObject parent, Vector3 localPos)
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

        private static void CreateClumpedFlowers(GameObject parent, Vector3 localCenter, Color[] colors, float radius, int count)
        {
            var field = new GameObject("FlowerField");
            field.transform.SetParent(parent.transform, false);
            field.transform.localPosition = localCenter;

            // Define 2 random sub-cluster centers
            Vector3 c1 = new Vector3(-radius * 0.22f, 0, -radius * 0.22f);
            Vector3 c2 = new Vector3(radius * 0.22f, 0, radius * 0.22f);

            for (int f = 0; f < count; f++)
            {
                Vector3 basePos = (Random.Range(0, 2) == 0) ? c1 : c2;
                float rDist = Random.Range(0f, radius * 0.35f);
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                float ox = basePos.x + Mathf.Cos(angle) * rDist;
                float oz = basePos.z + Mathf.Sin(angle) * rDist;
                float r = Random.Range(0.12f, 0.18f);

                var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "FlowerStem";
                stem.transform.SetParent(field.transform, false);
                stem.transform.localPosition = new Vector3(ox, 0.06f, oz);
                stem.transform.localScale = new Vector3(0.015f, 0.06f, 0.015f);
                stem.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.5f, 0.2f));
                Object.Destroy(stem.GetComponent<Collider>());

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "FlowerHead";
                head.transform.SetParent(field.transform, false);
                head.transform.localPosition = new Vector3(ox, 0.12f, oz);
                head.transform.localScale = Vector3.one * r;
                head.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(colors[Random.Range(0, colors.Length)]);
                Object.Destroy(head.GetComponent<Collider>());
            }
        }

        private static void CreateFoliage(GameObject parent, Vector3 localPos)
        {
            var decoratorObj = new GameObject("FoliageGroup");
            decoratorObj.transform.SetParent(parent.transform, false);
            decoratorObj.transform.localPosition = localPos;

            var bushColor = new Color(0.2f, 0.5f, 0.22f);
            var mushroomStemColor = new Color(0.9f, 0.9f, 0.85f);
            var mushroomCapColor = new Color(0.85f, 0.25f, 0.2f);

            // 1. Clumpy Ghibli bushes
            for (int i = 0; i < 4; i++)
            {
                PrimitiveType type = (i % 3 == 0) ? PrimitiveType.Sphere : ((i % 3 == 1) ? PrimitiveType.Capsule : PrimitiveType.Cube);
                var b = GameObject.CreatePrimitive(type);
                b.name = "BushPart";
                b.transform.SetParent(decoratorObj.transform, false);
                b.transform.localPosition = new Vector3(
                    (i - 1.5f) * 0.18f + Random.Range(-0.06f, 0.06f),
                    0.15f + Random.Range(0f, 0.08f),
                    Random.Range(-0.1f, 0.1f)
                );
                b.transform.localScale = new Vector3(
                    Random.Range(0.35f, 0.48f),
                    Random.Range(0.25f, 0.45f),
                    Random.Range(0.35f, 0.48f)
                );
                b.transform.localRotation = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
                b.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(bushColor);
                Object.Destroy(b.GetComponent<Collider>());
            }

            // 2. Ghibli Mushrooms
            int mushCount = Random.Range(3, 5);
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
                Color capCol = (m % 2 == 0) ? mushroomCapColor : new Color(0.15f, 0.65f, 0.85f); // red or glowing blue
                cap.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(capCol);
                Object.Destroy(cap.GetComponent<Collider>());
            }

            // 3. Fallen logs
            var twig = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            twig.name = "FallenTwig";
            twig.transform.SetParent(decoratorObj.transform, false);
            twig.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 0.02f, Random.Range(-0.4f, 0.4f));
            twig.transform.localScale = new Vector3(0.03f, 0.25f, 0.03f);
            twig.transform.localRotation = Quaternion.Euler(90f, Random.Range(0, 360f), 0);
            twig.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.45f, 0.35f, 0.25f));
            Object.Destroy(twig.GetComponent<Collider>());
        }

        private static void CreateButterfliesAndGlow(GameObject parent, Vector3 localPos)
        {
            var fxParent = new GameObject("MagicalFX");
            fxParent.transform.SetParent(parent.transform, false);
            fxParent.transform.localPosition = localPos;

            // Glowing dust particles
            int pCount = 10;
            for (int i = 0; i < pCount; i++)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.name = "MagicalParticle";
                p.transform.SetParent(fxParent.transform, false);
                p.transform.localPosition = new Vector3(
                    Random.Range(-1.6f, 1.6f),
                    0.4f + Random.Range(0f, 1.6f),
                    Random.Range(-1.6f, 1.6f)
                );
                p.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);
                p.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1f, 0.98f, 0.6f, 0.8f));
                Object.Destroy(p.GetComponent<Collider>());
            }

            // Butterflies
            Color[] bfColors = {
                new(0.4f, 0.7f, 1.0f), new(1.0f, 0.5f, 0.8f), new(1.0f, 0.85f, 0.3f)
            };
            int bfCount = 4;
            for (int b = 0; b < bfCount; b++)
            {
                var bf = new GameObject("Butterfly");
                bf.transform.SetParent(fxParent.transform, false);
                bf.transform.localPosition = new Vector3(
                    Random.Range(-1.2f, 1.2f),
                    0.6f + Random.Range(0f, 1.0f),
                    Random.Range(-1.2f, 1.2f)
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

            int spheres = 7;
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

        private static void CreateSkyBackground(GameObject zone)
        {
            var cloudsBelow = new GameObject("SeaOfClouds");
            cloudsBelow.transform.SetParent(zone.transform, false);

            var cloudBelowData = new (Vector3 pos, Vector3 scale)[] {
                (new(-8f, -4.5f, -6f), new(5.5f, 1.4f, 4.5f)),
                (new(8f, -4.8f, -3f), new(5.0f, 1.2f, 4.0f)),
                (new(-7f, -5.2f, 6f), new(6.0f, 1.6f, 5.0f)),
                (new(7f, -5.5f, 8f), new(6.5f, 1.8f, 5.5f)),
                (new(0f, -6.0f, 0f), new(8.5f, 2.0f, 7.5f)),
                (new(-3f, -4.2f, -8f), new(4.5f, 1.0f, 3.5f)),
                (new(3f, -4.2f, 8f), new(4.5f, 1.0f, 3.5f))
            };
            foreach (var (pos, scale) in cloudBelowData)
            {
                CreateCloudCluster(cloudsBelow, pos, scale);
            }
        }

        private static void CreateDistantIslands(GameObject zone)
        {
            var distantGroup = new GameObject("DistantScenery");
            distantGroup.transform.SetParent(zone.transform, false);

            // Distant Island L
            var di1 = CreateSculptedRuinedIsland(distantGroup, "DistantIslandL", new Vector3(-18f, 6f, 18f), 1.5f, 0.8f, new Color(0.32f, 0.58f, 0.22f));
            di1.transform.localScale = Vector3.one * 0.7f;
            var tTree = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tTree.name = "DistantGoldenTree";
            tTree.transform.SetParent(di1.transform, false);
            tTree.transform.localPosition = new Vector3(0, 0.6f, 0);
            tTree.transform.localScale = Vector3.one * 0.8f;
            tTree.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.85f, 0.15f));
            Object.Destroy(tTree.GetComponent<Collider>());

            // Distant Island R
            var di2 = CreateSculptedRuinedIsland(distantGroup, "DistantIslandR", new Vector3(18f, 8f, 22f), 1.2f, 0.6f, new Color(0.32f, 0.58f, 0.22f));
            di2.transform.localScale = Vector3.one * 0.6f;
        }

        private static void BuildRuinedArch(GameObject arch)
        {
            var stoneColor = new Color(0.6f, 0.6f, 0.62f);

            var archL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            archL.name = "ArchL_Collider";
            archL.transform.SetParent(arch.transform, false);
            archL.transform.localPosition = new Vector3(-1.1f, 1.1f, 0);
            archL.transform.localScale = new Vector3(0.12f, 1.1f, 0.12f);
            archL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);

            var archR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            archR.name = "ArchR_Collider";
            archR.transform.SetParent(arch.transform, false);
            archR.transform.localPosition = new Vector3(1.1f, 1.1f, 0);
            archR.transform.localScale = new Vector3(0.12f, 1.1f, 0.12f);
            archR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);

            var archTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archTop.name = "ArchTop";
            archTop.transform.SetParent(arch.transform, false);
            archTop.transform.localPosition = new Vector3(0, 2.2f, 0);
            archTop.transform.localScale = new Vector3(2.4f, 0.12f, 0.18f);
            archTop.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
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

        private static void CreatePathwaySign(GameObject parent, Vector3 localPos, string text)
        {
            var sign = new GameObject("PathwaySign");
            sign.transform.SetParent(parent.transform, false);
            sign.transform.localPosition = localPos;

            var woodColor = new Color(0.42f, 0.28f, 0.15f);

            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "SignPost_Collider";
            post.transform.SetParent(sign.transform, false);
            post.transform.localPosition = new Vector3(0, 0.4f, 0);
            post.transform.localScale = new Vector3(0.05f, 0.4f, 0.05f);
            post.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "SignBoard";
            board.transform.SetParent(sign.transform, false);
            board.transform.localPosition = new Vector3(0, 0.72f, 0);
            board.transform.localScale = new Vector3(0.6f, 0.22f, 0.05f);
            board.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
            board.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor * 0.8f);
            Object.Destroy(board.GetComponent<Collider>());

            var textObj = new GameObject("SignText");
            textObj.transform.SetParent(board.transform, false);
            textObj.transform.localPosition = new Vector3(0, 0, -0.026f);
            var tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 1.2f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateReadingStump(GameObject parent, Vector3 localPos)
        {
            var corner = new GameObject("ReadingStump");
            corner.transform.SetParent(parent.transform, false);
            corner.transform.localPosition = localPos;

            var woodColor = new Color(0.42f, 0.28f, 0.15f);

            // Stump table
            var table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            table.name = "StumpTable_Collider";
            table.transform.SetParent(corner.transform, false);
            table.transform.localPosition = new Vector3(0, 0.18f, 0);
            table.transform.localScale = new Vector3(0.45f, 0.18f, 0.45f);
            table.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            // Log stool
            var stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stool.name = "LogStool_Collider";
            stool.transform.SetParent(corner.transform, false);
            stool.transform.localPosition = new Vector3(0.45f, 0.14f, -0.1f);
            stool.transform.localScale = new Vector3(0.28f, 0.14f, 0.28f);
            stool.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor * 0.85f);

            // Open book on table
            var book = new GameObject("OpenBook");
            book.transform.SetParent(table.transform, false);
            book.transform.localPosition = new Vector3(0, 1.05f, 0);
            book.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            var pageL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pageL.transform.SetParent(book.transform, false);
            pageL.transform.localPosition = new Vector3(-0.1f, 0, 0);
            pageL.transform.localScale = new Vector3(0.2f, 0.02f, 0.28f);
            pageL.transform.localRotation = Quaternion.Euler(0, 0, 8f);
            pageL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(Color.white);
            Object.Destroy(pageL.GetComponent<Collider>());

            var pageR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pageR.transform.SetParent(book.transform, false);
            pageR.transform.localPosition = new Vector3(0.1f, 0, 0);
            pageR.transform.localScale = new Vector3(0.2f, 0.02f, 0.28f);
            pageR.transform.localRotation = Quaternion.Euler(0, 0, -8f);
            pageR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(Color.white);
            Object.Destroy(pageR.GetComponent<Collider>());
        }

        private static void CreateCampfire(GameObject parent, Vector3 localPos)
        {
            var camp = new GameObject("Campfire");
            camp.transform.SetParent(parent.transform, false);
            camp.transform.localPosition = localPos;

            var stoneColor = new Color(0.55f, 0.55f, 0.58f);

            // Stone ring
            for (int i = 0; i < 6; i++)
            {
                float angle = (i * 60f) * Mathf.Deg2Rad;
                var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "CampfireStone";
                stone.transform.SetParent(camp.transform, false);
                stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.35f, 0.04f, Mathf.Sin(angle) * 0.35f);
                stone.transform.localScale = Vector3.one * Random.Range(0.12f, 0.18f);
                stone.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
                Object.Destroy(stone.GetComponent<Collider>());
            }

            // Crossed logs
            float[] logRotations = { 30f, 120f };
            foreach (float r in logRotations)
            {
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.name = "CampfireLog";
                log.transform.SetParent(camp.transform, false);
                log.transform.localPosition = new Vector3(0, 0.03f, 0);
                log.transform.localScale = new Vector3(0.04f, 0.2f, 0.04f);
                log.transform.localRotation = Quaternion.Euler(90f, r, 0);
                log.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.4f, 0.26f, 0.12f));
                Object.Destroy(log.GetComponent<Collider>());
            }

            // Glowing flame sphere
            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "CampfireFlame";
            flame.transform.SetParent(camp.transform, false);
            flame.transform.localPosition = new Vector3(0, 0.15f, 0);
            flame.transform.localScale = Vector3.one * 0.18f;
            flame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.45f, 0.1f));
            Object.Destroy(flame.GetComponent<Collider>());

            var fLight = camp.AddComponent<Light>();
            fLight.type = LightType.Point;
            fLight.color = new Color(1.0f, 0.5f, 0.1f);
            fLight.range = 3.5f;
            fLight.intensity = 1.8f;
        }

        private static void CreateIncenseShrine(GameObject parent, Vector3 localPos)
        {
            var shrine = new GameObject("StoneShrine");
            shrine.transform.SetParent(parent.transform, false);
            shrine.transform.localPosition = localPos;

            var stoneColor = new Color(0.6f, 0.6f, 0.64f);

            // Altar slabs
            var slab1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab1.name = "AltarBase_Collider";
            slab1.transform.SetParent(shrine.transform, false);
            slab1.transform.localPosition = new Vector3(0, 0.15f, 0);
            slab1.transform.localScale = new Vector3(0.8f, 0.3f, 0.6f);
            slab1.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);

            var slab2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab2.name = "AltarTop";
            slab2.transform.SetParent(shrine.transform, false);
            slab2.transform.localPosition = new Vector3(0, 0.35f, 0);
            slab2.transform.localScale = new Vector3(0.6f, 0.1f, 0.45f);
            slab2.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor * 0.9f);
            Object.Destroy(slab2.GetComponent<Collider>());

            // Shrine box
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "ShrineBox";
            box.transform.SetParent(shrine.transform, false);
            box.transform.localPosition = new Vector3(0, 0.55f, 0);
            box.transform.localScale = new Vector3(0.35f, 0.3f, 0.3f);
            box.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor * 0.8f);
            Object.Destroy(box.GetComponent<Collider>());
        }
    }
}
