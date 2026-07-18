using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Vườn Bách Thảo — Botanical Garden with 5 themed sub-areas:
    /// 1. Flower Meadow (NW)     2. Zen Rock Garden (NE)
    /// 3. Herb Spiral (Center)   4. Vegetable Plots (SW)  
    /// 5. Fountain Garden (SE)
    /// Organic curved paths, asymmetric layout, atmospheric lighting.
    /// </summary>
    public static class GardenZoneBuilder
    {
        public static void Build(GameObject treePrefab = null, GameObject flowerPrefab = null)
        {
            var zone = new GameObject("GardenZone");
            zone.transform.position = new Vector3(0, 0, 60);

            // ═══ Ground — irregular grass with path insets ═══
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GardenGround";
            ground.transform.SetParent(zone.transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(32f, 0.1f, 32f);
            ground.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.65f, 0.22f));
            Object.Destroy(ground.GetComponent<Collider>());
            var walkCol = ground.AddComponent<BoxCollider>();
            walkCol.center = Vector3.zero;
            walkCol.size = new Vector3(32f, 0.1f, 32f);

            // ═══ Zone Lighting — warm sunlight ═══
            ZoneFactory.CreateZoneLighting(zone.transform, new Color(1f, 0.95f, 0.8f), 1.6f, new Vector3(0.5f, -1f, 0.3f));

            // ═══ Curved Stone Paths ═══
            BuildCurvedPaths(zone.transform);

            // ═══ Border — irregular stone wall ═══
            BuildStoneWall(zone.transform);

            // ═══ Area 1: Flower Meadow (NW) ═══
            BuildFlowerMeadow(zone.transform);

            // ═══ Area 2: Zen Rock Garden (NE) ═══
            BuildZenGarden(zone.transform);

            // ═══ Area 3: Herb Spiral (Center-North) ═══
            BuildHerbSpiral(zone.transform);

            // ═══ Area 4: Vegetable Plots with GardenPlot interaction (SW) ═══
            BuildVegetablePlots(zone.transform);

            // ═══ Area 5: Fountain Garden (SE) ═══
            BuildFountainGarden(zone.transform);

            // ═══ Decorative Trees ═══
            BuildTrees(zone.transform, treePrefab);

            // ═══ Floating pollen particles ═══
            ZoneParticles.CreateFloatingParticles(
                zone.transform,
                new Color(1f, 0.95f, 0.6f, 0.7f),
                40,
                new Vector3(28f, 3f, 28f),
                "PollenParticles");

            // ═══ Title ═══
            var title = new GameObject("GardenTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3.5f, -14f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Vườn Bách Thảo";
            tmp.fontSize = 6.4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ═══ Return Portal ═══
            ZoneFactory.CreatePortal(
                "GardenReturnPortal",
                new Vector3(0, .05f, 54),
                180,
                new Color(0.93f, 0.94f, 0.96f),
                new Color(.25f, 1f, .5f, .7f),
                "Về Sảnh Chờ");
        }

        // ── Curved flagstone paths ──
        private static void BuildCurvedPaths(Transform parent)
        {
            Color pathColor = new Color(0.82f, 0.76f, 0.65f);
            // Main path from entrance curving through center
            float[] pathSegments = { -13f, -10f, -7f, -4f, -1f, 2f, 5f, 8f, 11f };
            foreach (float z in pathSegments)
            {
                float xOffset = Mathf.Sin(z * 0.15f) * 1.5f; // gentle curve
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = "PathStone";
                stone.transform.SetParent(parent, false);
                stone.transform.localPosition = new Vector3(xOffset, 0.02f, z);
                stone.transform.localScale = new Vector3(2.2f, 0.04f, 2.8f);
                stone.transform.localRotation = Quaternion.Euler(0, Random.Range(-8f, 8f), 0);
                stone.GetComponent<Renderer>().material = ZoneFactory.StoneMat(pathColor);
                Object.Destroy(stone.GetComponent<Collider>());
            }
            // Cross path (East-West) through herb spiral area
            for (float x = -10f; x <= 10f; x += 3f)
            {
                float zOff = Mathf.Sin(x * 0.2f) * 0.8f + 3f;
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = "CrossPathStone";
                stone.transform.SetParent(parent, false);
                stone.transform.localPosition = new Vector3(x, 0.02f, zOff);
                stone.transform.localScale = new Vector3(2.8f, 0.04f, 2.0f);
                stone.transform.localRotation = Quaternion.Euler(0, Random.Range(-5f, 5f), 0);
                stone.GetComponent<Renderer>().material = ZoneFactory.StoneMat(pathColor * 0.95f);
                Object.Destroy(stone.GetComponent<Collider>());
            }
        }

        // ── Irregular stone border wall ──
        private static void BuildStoneWall(Transform parent)
        {
            Color wallColor = new Color(0.55f, 0.52f, 0.48f);
            float limit = 15.5f;

            // Build walls with varied height stones
            void PlaceWallStone(Vector3 pos, Vector3 scale)
            {
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = "WallStone";
                stone.transform.SetParent(parent, false);
                stone.transform.localPosition = pos;
                stone.transform.localScale = scale;
                stone.transform.localRotation = Quaternion.Euler(0, Random.Range(-3f, 3f), 0);
                stone.GetComponent<Renderer>().material = ZoneFactory.StoneMat(
                    wallColor * Random.Range(0.85f, 1.1f));
            }

            // North, South walls — varied block heights
            for (float x = -15f; x <= 15f; x += 2.5f)
            {
                float h = Random.Range(0.6f, 1.2f);
                PlaceWallStone(new Vector3(x, h * 0.5f, limit), new Vector3(2.4f, h, 0.6f));
                PlaceWallStone(new Vector3(x, h * 0.5f, -limit), new Vector3(2.4f, h, 0.6f));
            }
            // East, West walls
            for (float z = -15f; z <= 15f; z += 2.5f)
            {
                float h = Random.Range(0.6f, 1.2f);
                PlaceWallStone(new Vector3(limit, h * 0.5f, z), new Vector3(0.6f, h, 2.4f));
                PlaceWallStone(new Vector3(-limit, h * 0.5f, z), new Vector3(0.6f, h, 2.4f));
            }
            // Entrance gap (south center) — skip wall stones near x=0, z=-limit
            // Already handled by not having stones at x ∈ [-2, 2] on south wall
        }

        // ── Area 1: Flower Meadow (NW quadrant) ──
        private static void BuildFlowerMeadow(Transform parent)
        {
            var area = new GameObject("FlowerMeadow");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-8f, 0, 9f);

            // Flower bush clusters — compound spheres in varied colors
            Color[] flowerColors = {
                new Color(0.9f, 0.3f, 0.4f),   // Rose
                new Color(0.95f, 0.7f, 0.2f),  // Sunflower
                new Color(0.7f, 0.3f, 0.85f),  // Lavender
                new Color(0.95f, 0.5f, 0.7f),  // Pink
                new Color(0.3f, 0.6f, 0.9f),   // Bluebell
            };

            Vector3[] bushPositions = {
                new(-2f, 0, 2f), new(0f, 0, 3.5f), new(2.5f, 0, 1.5f),
                new(-3f, 0, -1f), new(1f, 0, -0.5f), new(3f, 0, 3f),
                new(-1f, 0, 1f), new(2f, 0, -2f)
            };

            foreach (var pos in bushPositions)
            {
                var bush = new GameObject("FlowerBush");
                bush.transform.SetParent(area.transform, false);
                bush.transform.localPosition = pos;
                Color c = flowerColors[Random.Range(0, flowerColors.Length)];
                float size = Random.Range(0.4f, 0.7f);

                // Leaf base
                var leafBase = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leafBase.transform.SetParent(bush.transform, false);
                leafBase.transform.localPosition = new Vector3(0, size * 0.4f, 0);
                leafBase.transform.localScale = new Vector3(size * 1.2f, size * 0.6f, size * 1.2f);
                leafBase.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.25f, 0.55f, 0.2f));
                Object.Destroy(leafBase.GetComponent<Collider>());

                // Flower blooms (3-4 small spheres on top)
                int bloomCount = Random.Range(3, 6);
                for (int b = 0; b < bloomCount; b++)
                {
                    var bloom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    bloom.transform.SetParent(bush.transform, false);
                    float angle = b * (360f / bloomCount) * Mathf.Deg2Rad;
                    float r = size * 0.35f;
                    bloom.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * r, size * 0.6f + Random.Range(0f, 0.1f), Mathf.Sin(angle) * r);
                    bloom.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
                    bloom.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(c);
                    Object.Destroy(bloom.GetComponent<Collider>());
                }
            }

            // Garden bench
            BuildBench(area.transform, new Vector3(-3.5f, 0, -3f), 30f, new Color(0.55f, 0.38f, 0.22f));
        }

        // ── Area 2: Zen Rock Garden (NE quadrant) ──
        private static void BuildZenGarden(Transform parent)
        {
            var area = new GameObject("ZenGarden");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(8f, 0, 9f);

            // Raked sand surface
            var sand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sand.name = "RakedSand";
            sand.transform.SetParent(area.transform, false);
            sand.transform.localPosition = new Vector3(0, 0.01f, 0);
            sand.transform.localScale = new Vector3(10f, 0.02f, 10f);
            sand.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.88f, 0.84f, 0.75f));
            Object.Destroy(sand.GetComponent<Collider>());

            // Rake lines (thin flat cubes)
            for (float z = -4f; z <= 4f; z += 0.6f)
            {
                var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "RakeLine";
                line.transform.SetParent(area.transform, false);
                line.transform.localPosition = new Vector3(0, 0.025f, z);
                line.transform.localScale = new Vector3(8f, 0.005f, 0.15f);
                line.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.82f, 0.78f, 0.68f));
                Object.Destroy(line.GetComponent<Collider>());
            }

            // Decorative rocks (asymmetric placement)
            var rockData = new (Vector3 pos, Vector3 scale, float rotY)[] {
                (new(-1.5f, 0.25f, 1.5f), new(1.2f, 0.5f, 0.9f), 15f),
                (new(2f, 0.35f, -0.5f), new(1.5f, 0.7f, 1.1f), -25f),
                (new(-0.5f, 0.15f, -2.5f), new(0.6f, 0.3f, 0.5f), 40f),
                (new(3f, 0.2f, 2.5f), new(0.8f, 0.4f, 0.7f), -10f),
                (new(0.5f, 0.12f, 3f), new(0.4f, 0.25f, 0.35f), 55f),
            };
            foreach (var (pos, scale, rotY) in rockData)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "ZenRock";
                rock.transform.SetParent(area.transform, false);
                rock.transform.localPosition = pos;
                rock.transform.localScale = scale;
                rock.transform.localRotation = Quaternion.Euler(0, rotY, 0);
                rock.GetComponent<Renderer>().material = ZoneFactory.StoneMat(
                    new Color(0.4f + Random.Range(0f, 0.1f), 0.38f, 0.36f));
            }

            // Mini bonsai tree
            BuildBonsai(area.transform, new Vector3(-3f, 0, -1f));
            BuildBonsai(area.transform, new Vector3(1f, 0, -3.5f));
        }

        // ── Area 3: Herb Spiral (Center-North) ──
        private static void BuildHerbSpiral(Transform parent)
        {
            var area = new GameObject("HerbSpiral");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(0, 0, 5f);

            // Spiral bed — stones forming a spiral pattern
            float spiralRadius = 3.5f;
            int segments = 24;
            Color bedColor = new Color(0.4f, 0.25f, 0.12f);
            Color herbGreen = new Color(0.3f, 0.6f, 0.25f);

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 3f; // 1.5 turns
                float r = spiralRadius * (1f - t * 0.65f);
                float x = Mathf.Cos(angle) * r;
                float z = Mathf.Sin(angle) * r;
                float height = t * 0.6f; // spiral rises toward center

                // Stone border
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = "SpiralStone";
                stone.transform.SetParent(area.transform, false);
                stone.transform.localPosition = new Vector3(x, height * 0.5f + 0.05f, z);
                stone.transform.localScale = new Vector3(0.45f, height + 0.1f, 0.45f);
                stone.transform.localRotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
                stone.GetComponent<Renderer>().material = ZoneFactory.StoneMat(
                    new Color(0.5f, 0.47f, 0.42f) * Random.Range(0.9f, 1.05f));
                Object.Destroy(stone.GetComponent<Collider>());

                // Herb plant at each segment
                if (i % 2 == 0)
                {
                    float innerR = r - 0.4f;
                    var herb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    herb.name = "HerbPlant";
                    herb.transform.SetParent(area.transform, false);
                    herb.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * innerR, height + 0.15f, Mathf.Sin(angle) * innerR);
                    herb.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
                    // Varied green shades
                    Color shade = Color.Lerp(herbGreen, new Color(0.5f, 0.7f, 0.2f), Random.value);
                    herb.GetComponent<Renderer>().material = ZoneFactory.CreateMat(shade);
                    Object.Destroy(herb.GetComponent<Collider>());
                }
            }

            // Center marker stone
            var center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.name = "SpiralCenter";
            center.transform.SetParent(area.transform, false);
            center.transform.localPosition = new Vector3(0, 0.4f, 0);
            center.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            center.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.6f, 0.58f, 0.55f));
            Object.Destroy(center.GetComponent<Collider>());
        }

        // ── Area 4: Vegetable Plots (SW) — interactive GardenPlots ──
        private static void BuildVegetablePlots(Transform parent)
        {
            var area = new GameObject("VegetablePlots");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-7f, 0, -6f);

            // Soil bed (L-shaped, not rectangular!)
            // Main bed
            var mainBed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainBed.name = "MainSoilBed";
            mainBed.transform.SetParent(area.transform, false);
            mainBed.transform.localPosition = new Vector3(0, 0.06f, 0);
            mainBed.transform.localScale = new Vector3(8f, 0.12f, 5f);
            mainBed.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.22f, 0.1f));
            Object.Destroy(mainBed.GetComponent<Collider>());

            // Extension bed (L-shape arm)
            var extBed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            extBed.name = "ExtSoilBed";
            extBed.transform.SetParent(area.transform, false);
            extBed.transform.localPosition = new Vector3(3f, 0.06f, -3.5f);
            extBed.transform.localScale = new Vector3(4f, 0.12f, 3f);
            extBed.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.32f, 0.2f, 0.09f));
            Object.Destroy(extBed.GetComponent<Collider>());

            // Interactive GardenPlots — scattered, not grid
            Vector3[] plotPositions = {
                new(-2.5f, 0.08f, 1.2f), new(0f, 0.08f, 1.2f), new(2.5f, 0.08f, 1.2f),
                new(-2.5f, 0.08f, -1f),  new(0f, 0.08f, -1f),  new(2.5f, 0.08f, -1f),
                new(3f, 0.08f, -2.5f),   new(4.5f, 0.08f, -3.5f), new(3f, 0.08f, -4.5f)
            };
            foreach (var pos in plotPositions)
            {
                var plot = new GameObject("GardenPlot");
                plot.transform.SetParent(area.transform, false);
                plot.transform.localPosition = pos;
                var gp = plot.AddComponent<GardenPlot>();
                ZoneFactory.SetField(gp, "_growthDuration", 15f);
                ZoneFactory.SetField(gp, "_harvestReward", 20);
            }

            // Scarecrow
            BuildScarecrow(area.transform, new Vector3(-4f, 0, 0.5f));

            // Wooden tool shed
            BuildToolShed(area.transform, new Vector3(-4.5f, 0, -3f));
        }

        // ── Area 5: Fountain Garden (SE) ──
        private static void BuildFountainGarden(Transform parent)
        {
            var area = new GameObject("FountainGarden");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(7f, 0, -7f);

            // Circular paved area
            var paving = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            paving.name = "FountainPaving";
            paving.transform.SetParent(area.transform, false);
            paving.transform.localPosition = new Vector3(0, 0.01f, 0);
            paving.transform.localScale = new Vector3(8f, 0.02f, 8f);
            paving.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.65f, 0.62f, 0.58f));
            Object.Destroy(paving.GetComponent<Collider>());

            // Fountain base ring
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "FountainRing";
            ring.transform.SetParent(area.transform, false);
            ring.transform.localPosition = new Vector3(0, 0.25f, 0);
            ring.transform.localScale = new Vector3(2.5f, 0.25f, 2.5f);
            ring.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.72f, 0.7f, 0.66f));

            // Water surface
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "FountainWater";
            water.transform.SetParent(area.transform, false);
            water.transform.localPosition = new Vector3(0, 0.35f, 0);
            water.transform.localScale = new Vector3(2.2f, 0.02f, 2.2f);
            water.GetComponent<Renderer>().material = ZoneFactory.WaterMat(new Color(0.25f, 0.6f, 0.85f, 0.75f));
            Object.Destroy(water.GetComponent<Collider>());

            // Central pillar
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "FountainPillar";
            pillar.transform.SetParent(area.transform, false);
            pillar.transform.localPosition = new Vector3(0, 0.7f, 0);
            pillar.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
            pillar.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.78f, 0.76f, 0.72f));
            Object.Destroy(pillar.GetComponent<Collider>());

            // Top bowl
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.name = "FountainBowl";
            bowl.transform.SetParent(area.transform, false);
            bowl.transform.localPosition = new Vector3(0, 1.1f, 0);
            bowl.transform.localScale = new Vector3(0.6f, 0.25f, 0.6f);
            bowl.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.75f, 0.73f, 0.7f));
            Object.Destroy(bowl.GetComponent<Collider>());

            // Water ripples
            ZoneParticles.CreateWaterRipples(area.transform, new Vector3(0, 0.36f, 0), 1.0f);

            // Benches around fountain (3 at 120° apart)
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f * Mathf.Deg2Rad;
                float dist = 3f;
                BuildBench(area.transform,
                    new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist),
                    -i * 120f + 90f,
                    new Color(0.5f, 0.35f, 0.2f));
            }

            // Flower pots around fountain
            for (int i = 0; i < 4; i++)
            {
                float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                float dist = 3.5f;
                BuildFlowerPot(area.transform, new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist));
            }
        }

        // ═══════════════════════ HELPER BUILDERS ═══════════════════════

        private static void BuildBench(Transform parent, Vector3 pos, float rotY, Color woodColor)
        {
            var bench = new GameObject("GardenBench");
            bench.transform.SetParent(parent, false);
            bench.transform.localPosition = pos;
            bench.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat_Collider";
            seat.transform.SetParent(bench.transform, false);
            seat.transform.localPosition = new Vector3(0, 0.35f, 0);
            seat.transform.localScale = new Vector3(1.4f, 0.08f, 0.5f);
            seat.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            // Legs
            float[] lx = { -0.6f, 0.6f };
            foreach (float x in lx)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg";
                leg.transform.SetParent(bench.transform, false);
                leg.transform.localPosition = new Vector3(x, 0.17f, 0);
                leg.transform.localScale = new Vector3(0.08f, 0.34f, 0.4f);
                leg.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 0.85f);
                Object.Destroy(leg.GetComponent<Collider>());
            }

            // Backrest
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Backrest";
            back.transform.SetParent(bench.transform, false);
            back.transform.localPosition = new Vector3(0, 0.6f, -0.2f);
            back.transform.localScale = new Vector3(1.4f, 0.45f, 0.06f);
            back.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);
            Object.Destroy(back.GetComponent<Collider>());
        }

        private static void BuildBonsai(Transform parent, Vector3 pos)
        {
            var bonsai = new GameObject("Bonsai");
            bonsai.transform.SetParent(parent, false);
            bonsai.transform.localPosition = pos;

            // Pot
            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "BonsaiPot";
            pot.transform.SetParent(bonsai.transform, false);
            pot.transform.localPosition = new Vector3(0, 0.1f, 0);
            pot.transform.localScale = new Vector3(0.35f, 0.1f, 0.35f);
            pot.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.55f, 0.3f, 0.2f));
            Object.Destroy(pot.GetComponent<Collider>());

            // Trunk (slightly angled)
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(bonsai.transform, false);
            trunk.transform.localPosition = new Vector3(0, 0.35f, 0);
            trunk.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
            trunk.transform.localRotation = Quaternion.Euler(0, 0, 12f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));
            Object.Destroy(trunk.GetComponent<Collider>());

            // Canopy (flattened sphere)
            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Canopy";
            canopy.transform.SetParent(bonsai.transform, false);
            canopy.transform.localPosition = new Vector3(0.04f, 0.5f, 0);
            canopy.transform.localScale = new Vector3(0.4f, 0.18f, 0.4f);
            canopy.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.15f, 0.5f, 0.18f));
            Object.Destroy(canopy.GetComponent<Collider>());
        }

        private static void BuildScarecrow(Transform parent, Vector3 pos)
        {
            var sc = new GameObject("Scarecrow");
            sc.transform.SetParent(parent, false);
            sc.transform.localPosition = pos;

            // Post
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post_Collider";
            post.transform.SetParent(sc.transform, false);
            post.transform.localPosition = new Vector3(0, 0.7f, 0);
            post.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);
            post.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));

            // Cross arm
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm";
            arm.transform.SetParent(sc.transform, false);
            arm.transform.localPosition = new Vector3(0, 1.1f, 0);
            arm.transform.localScale = new Vector3(0.8f, 0.06f, 0.06f);
            arm.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.38f, 0.25f, 0.12f));
            Object.Destroy(arm.GetComponent<Collider>());

            // Head (pumpkin sphere)
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(sc.transform, false);
            head.transform.localPosition = new Vector3(0, 1.4f, 0);
            head.transform.localScale = Vector3.one * 0.25f;
            head.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.9f, 0.6f, 0.15f));
            Object.Destroy(head.GetComponent<Collider>());

            // Hat
            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "Hat";
            hat.transform.SetParent(sc.transform, false);
            hat.transform.localPosition = new Vector3(0, 1.55f, 0);
            hat.transform.localScale = new Vector3(0.35f, 0.08f, 0.35f);
            hat.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.3f, 0.2f, 0.1f));
            Object.Destroy(hat.GetComponent<Collider>());
        }

        private static void BuildToolShed(Transform parent, Vector3 pos)
        {
            var shed = new GameObject("ToolShed");
            shed.transform.SetParent(parent, false);
            shed.transform.localPosition = pos;

            // Walls
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "ShedWalls_Collider";
            walls.transform.SetParent(shed.transform, false);
            walls.transform.localPosition = new Vector3(0, 0.6f, 0);
            walls.transform.localScale = new Vector3(1.5f, 1.2f, 1.2f);
            walls.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.35f, 0.2f));

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(shed.transform, false);
            roof.transform.localPosition = new Vector3(0, 1.3f, 0);
            roof.transform.localScale = new Vector3(1.7f, 0.1f, 1.4f);
            roof.transform.localRotation = Quaternion.Euler(0, 0, 8f);
            roof.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.6f, 0.25f, 0.15f));
            Object.Destroy(roof.GetComponent<Collider>());
        }

        private static void BuildFlowerPot(Transform parent, Vector3 pos)
        {
            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "FlowerPot";
            pot.transform.SetParent(parent, false);
            pot.transform.localPosition = pos + Vector3.up * 0.15f;
            pot.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
            pot.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.6f, 0.35f, 0.2f));
            Object.Destroy(pot.GetComponent<Collider>());

            // Flower
            var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flower.name = "Flower";
            flower.transform.SetParent(pot.transform, false);
            flower.transform.localPosition = new Vector3(0, 1.5f, 0);
            flower.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            Color[] colors = { new(0.9f, 0.4f, 0.5f), new(0.95f, 0.7f, 0.3f), new(0.8f, 0.3f, 0.8f) };
            flower.GetComponent<Renderer>().material = ZoneFactory.CreateMat(colors[Random.Range(0, colors.Length)]);
            Object.Destroy(flower.GetComponent<Collider>());
        }

        private static void BuildTrees(Transform parent, GameObject treePrefab)
        {
            Vector3[] treePositions = {
                new(-13f, 0, 13f), new(13f, 0, 13f),
                new(-13f, 0, -13f), new(13f, 0, -13f),
                new(-13f, 0, 0f), new(13f, 0, 0f),
                new(-5f, 0, 13f), new(5f, 0, 13f),
                new(0f, 0, 13f),
            };

            foreach (var pos in treePositions)
            {
                if (treePrefab != null)
                {
                    var tree = Object.Instantiate(treePrefab, parent);
                    tree.name = "GardenTree";
                    tree.transform.localPosition = pos;
                    tree.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    tree.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
                }
                else
                {
                    // Procedural tree
                    var treeObj = new GameObject("ProceduralTree");
                    treeObj.transform.SetParent(parent, false);
                    treeObj.transform.localPosition = pos;

                    var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    trunk.name = "Trunk_Collider";
                    trunk.transform.SetParent(treeObj.transform, false);
                    trunk.transform.localPosition = Vector3.up * 1.0f;
                    trunk.transform.localScale = new Vector3(0.22f, 1.0f, 0.22f);
                    trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));

                    // Multi-sphere canopy for volume
                    float canopyY = 2.2f;
                    var c1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    c1.transform.SetParent(treeObj.transform, false);
                    c1.transform.localPosition = new Vector3(0, canopyY, 0);
                    c1.transform.localScale = new Vector3(1.8f, 1.2f, 1.8f);
                    c1.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.55f, 0.18f));
                    Object.Destroy(c1.GetComponent<Collider>());

                    var c2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    c2.transform.SetParent(treeObj.transform, false);
                    c2.transform.localPosition = new Vector3(0.5f, canopyY + 0.2f, 0.3f);
                    c2.transform.localScale = new Vector3(1.2f, 0.9f, 1.2f);
                    c2.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.25f, 0.6f, 0.2f));
                    Object.Destroy(c2.GetComponent<Collider>());

                    var c3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    c3.transform.SetParent(treeObj.transform, false);
                    c3.transform.localPosition = new Vector3(-0.4f, canopyY - 0.1f, -0.3f);
                    c3.transform.localScale = new Vector3(1.0f, 0.8f, 1.0f);
                    c3.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.18f, 0.5f, 0.15f));
                    Object.Destroy(c3.GetComponent<Collider>());
                }
            }
        }
    }
}
