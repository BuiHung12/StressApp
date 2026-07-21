using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Vườn Bách Thảo — Botanical Garden (30×30, centered at 0,0,60).
    /// Grid layout: 4 corners + 1 center, connected by cross-shaped paths.
    ///
    ///   NW: Flower Meadow   NE: Zen Rock Garden
    ///   Center: Herb Spiral
    ///   SW: Vegetable Plots  SE: Fountain Garden
    /// </summary>
    public static class GardenZoneBuilder
    {
        // Zone bounds: -15 to +15 in both X and Z (local space)
        // Cross paths at X=0 (2.5 wide) and Z=0 (2.5 wide)
        // NW quadrant: X[-14,-1.5], Z[1.5,14]
        // NE quadrant: X[1.5,14],   Z[1.5,14]
        // SW quadrant: X[-14,-1.5], Z[-14,-1.5]
        // SE quadrant: X[1.5,14],   Z[-14,-1.5]
        // Center circle: around (0,0,0), radius ~3

        static readonly Color GrassGreen = new(0.18f, 0.42f, 0.15f);
        static readonly Color PathStone  = new(0.5f, 0.5f, 0.52f);
        static readonly Color WallStone  = new(0.55f, 0.52f, 0.48f);
        static readonly Color FenceWood  = new(0.5f, 0.38f, 0.22f);

        public static void Build(GameObject treePrefab = null, GameObject flowerPrefab = null)
        {
            var zone = new GameObject("GardenZone");
            zone.transform.position = new Vector3(0, 0, 60);

            // ═══ Ground ═══
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GardenGround";
            ground.transform.SetParent(zone.transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(30f, 0.1f, 30f);
            ground.GetComponent<Renderer>().material = ZoneFactory.CreateMat(GrassGreen);
            Object.Destroy(ground.GetComponent<Collider>());
            var walkCol = ground.AddComponent<BoxCollider>();
            walkCol.center = Vector3.zero;
            walkCol.size = Vector3.one;

            // ═══ Zone Lighting ═══
            ZoneFactory.CreateZoneLighting(zone.transform, new Color(1f, 0.95f, 0.8f), 0.85f, new Vector3(0.5f, -1f, 0.3f));

            // ═══ Perimeter Stone Wall ═══
            BuildPerimeterWall(zone.transform);

            // ═══ Cross-shaped Pathways ═══
            BuildCrossPaths(zone.transform);

            // ═══ NW: Flower Meadow ═══
            BuildFlowerMeadow(zone.transform);

            // ═══ NE: Zen Rock Garden ═══
            BuildZenGarden(zone.transform);

            // ═══ Center: Herb Spiral ═══
            BuildHerbSpiral(zone.transform);

            // ═══ SW: Vegetable Plots ═══
            BuildVegetablePlots(zone.transform);

            // ═══ SE: Fountain Garden ═══
            BuildFountainGarden(zone.transform);

            // ═══ Corner Trees ═══
            BuildCornerTrees(zone.transform, treePrefab);

            // ═══ Pollen Particles ═══
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

        // ────────────────────────────────────────────────────────
        //  PERIMETER WALL
        // ────────────────────────────────────────────────────────
        private static void BuildPerimeterWall(Transform parent)
        {
            float limit = 14.5f;
            float wallH = 1.0f;
            float wallThick = 0.4f;

            // North wall (with entrance gap at center)
            CreateWallSegment(parent, new Vector3(-7.5f, wallH * 0.5f, limit), new Vector3(13f, wallH, wallThick), 0f);
            CreateWallSegment(parent, new Vector3(7.5f, wallH * 0.5f, limit), new Vector3(13f, wallH, wallThick), 0f);

            // South wall (entrance gap at center for portal)
            CreateWallSegment(parent, new Vector3(-7.5f, wallH * 0.5f, -limit), new Vector3(13f, wallH, wallThick), 0f);
            CreateWallSegment(parent, new Vector3(7.5f, wallH * 0.5f, -limit), new Vector3(13f, wallH, wallThick), 0f);

            // East wall (solid)
            CreateWallSegment(parent, new Vector3(limit, wallH * 0.5f, 0), new Vector3(wallThick, wallH, 29f), 0f);

            // West wall (solid)
            CreateWallSegment(parent, new Vector3(-limit, wallH * 0.5f, 0), new Vector3(wallThick, wallH, 29f), 0f);

            // Entrance gates
            ZoneFactory.CreateGate(parent, new Vector3(0, 0, -limit), 0f, WallStone, 2f, 1.2f);
            ZoneFactory.CreateGate(parent, new Vector3(0, 0, limit), 0f, WallStone, 2f, 1.2f);
        }

        private static void CreateWallSegment(Transform parent, Vector3 pos, Vector3 scale, float rotY)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "PerimeterWall";
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = pos;
            wall.transform.localScale = scale;
            if (rotY != 0) wall.transform.localRotation = Quaternion.Euler(0, rotY, 0);
            wall.GetComponent<Renderer>().material = ZoneFactory.StoneMat(WallStone);
            Object.Destroy(wall.GetComponent<Collider>());
        }

        // ────────────────────────────────────────────────────────
        //  CROSS-SHAPED PATHWAYS (X=0 and Z=0, each 2.5 wide)
        // ────────────────────────────────────────────────────────
        private static void BuildCrossPaths(Transform parent)
        {
            // North-South path (X=0, full length)
            var nsPath = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nsPath.name = "PathNS";
            nsPath.transform.SetParent(parent, false);
            nsPath.transform.localPosition = new Vector3(0, 0.01f, 0);
            nsPath.transform.localScale = new Vector3(2.5f, 0.02f, 29f);
            nsPath.GetComponent<Renderer>().material = ZoneFactory.StoneMat(PathStone);
            Object.Destroy(nsPath.GetComponent<Collider>());

            // East-West path (Z=0, full length)
            var ewPath = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ewPath.name = "PathEW";
            ewPath.transform.SetParent(parent, false);
            ewPath.transform.localPosition = new Vector3(0, 0.01f, 0);
            ewPath.transform.localScale = new Vector3(29f, 0.02f, 2.5f);
            ewPath.GetComponent<Renderer>().material = ZoneFactory.StoneMat(PathStone);
            Object.Destroy(ewPath.GetComponent<Collider>());

            // Path border lines (subtle dark stone strips)
            Color borderColor = PathStone * 0.75f;
            float borderW = 0.12f;
            // NS path borders
            CreatePathBorder(parent, new Vector3(-1.25f, 0.015f, 0), new Vector3(borderW, 0.01f, 29f), borderColor);
            CreatePathBorder(parent, new Vector3(1.25f, 0.015f, 0), new Vector3(borderW, 0.01f, 29f), borderColor);
            // EW path borders
            CreatePathBorder(parent, new Vector3(0, 0.015f, -1.25f), new Vector3(29f, 0.01f, borderW), borderColor);
            CreatePathBorder(parent, new Vector3(0, 0.015f, 1.25f), new Vector3(29f, 0.01f, borderW), borderColor);
        }

        private static void CreatePathBorder(Transform parent, Vector3 pos, Vector3 scale, Color color)
        {
            var border = GameObject.CreatePrimitive(PrimitiveType.Cube);
            border.name = "PathBorder";
            border.transform.SetParent(parent, false);
            border.transform.localPosition = pos;
            border.transform.localScale = scale;
            border.GetComponent<Renderer>().material = ZoneFactory.StoneMat(color);
            Object.Destroy(border.GetComponent<Collider>());
        }

        // ────────────────────────────────────────────────────────
        //  NW: FLOWER MEADOW  (X[-14,-1.5], Z[1.5,14])
        //  Center: (-7.75, 0, 7.75), Size: 12.5 × 12.5
        // ────────────────────────────────────────────────────────
        private static void BuildFlowerMeadow(Transform parent)
        {
            var area = new GameObject("FlowerMeadow");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-7.75f, 0, 7.75f);

            // Darker grass floor
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero,
                new Vector3(12.5f, 0, 12.5f), new Color(0.28f, 0.58f, 0.18f), "MeadowFloor");

            // Low wooden fence on N, W, and part of E edges
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, 6f), 12f, 0f, FenceWood, 0.4f);    // North
            ZoneFactory.CreateLowFence(area.transform, new Vector3(-6f, 0, 0), 12f, 90f, FenceWood, 0.4f);  // West
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, -6f), 12f, 0f, FenceWood, 0.4f);   // South

            // Area sign
            ZoneFactory.CreateAreaSign(area.transform, new Vector3(0, 1.5f, -6.2f), "Hoa Viên");

            // Flower bush clusters — arranged in 3 rows × 3 columns
            Color[] flowerColors = {
                new(0.9f, 0.3f, 0.4f),   // Rose
                new(0.95f, 0.7f, 0.2f),  // Sunflower
                new(0.7f, 0.3f, 0.85f),  // Lavender
                new(0.95f, 0.5f, 0.7f),  // Pink
                new(0.3f, 0.6f, 0.9f),   // Bluebell
            };

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float x = -3.5f + col * 3.5f;
                    float z = -3f + row * 3f;
                    BuildFlowerBush(area.transform, new Vector3(x, 0, z),
                        flowerColors[(row * 3 + col) % flowerColors.Length]);
                }
            }

            // Garden bench
            BuildBench(area.transform, new Vector3(-4f, 0, 4f), 45f, new Color(0.55f, 0.38f, 0.22f));
        }

        // ────────────────────────────────────────────────────────
        //  NE: ZEN ROCK GARDEN  (X[1.5,14], Z[1.5,14])
        //  Center: (7.75, 0, 7.75), Size: 12.5 × 12.5
        // ────────────────────────────────────────────────────────
        private static void BuildZenGarden(Transform parent)
        {
            var area = new GameObject("ZenGarden");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(7.75f, 0, 7.75f);

            // Sand floor
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero,
                new Vector3(12.5f, 0, 12.5f), new Color(0.88f, 0.84f, 0.75f), "SandFloor");

            // Stone border on N, E edges
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, 6f), 12f, 0f, WallStone, 0.35f);  // North
            ZoneFactory.CreateLowFence(area.transform, new Vector3(6f, 0, 0), 12f, 90f, WallStone, 0.35f); // East
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, -6f), 12f, 0f, WallStone, 0.35f); // South

            // Area sign
            ZoneFactory.CreateAreaSign(area.transform, new Vector3(0, 1.5f, -6.2f), "Vườn Thiền");

            // Rake lines (parallel thin strips)
            for (float z = -4.5f; z <= 4.5f; z += 0.6f)
            {
                var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "RakeLine";
                line.transform.SetParent(area.transform, false);
                line.transform.localPosition = new Vector3(0, 0.035f, z);
                line.transform.localScale = new Vector3(10f, 0.005f, 0.15f);
                line.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.82f, 0.78f, 0.68f));
                Object.Destroy(line.GetComponent<Collider>());
            }

            // Decorative rocks — fixed asymmetric positions
            var rockData = new (Vector3 pos, Vector3 scale, float rotY)[] {
                (new(-2f, 0.25f, 2f),   new(1.2f, 0.5f, 0.9f), 15f),
                (new(2.5f, 0.35f, -1f), new(1.5f, 0.7f, 1.1f), -25f),
                (new(-1f, 0.15f, -3f),  new(0.6f, 0.3f, 0.5f), 40f),
                (new(3.5f, 0.2f, 3f),   new(0.8f, 0.4f, 0.7f), -10f),
                (new(0.5f, 0.12f, 4f),  new(0.4f, 0.25f, 0.35f), 55f),
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
                    new Color(0.42f, 0.38f, 0.36f));
            }

            // Bonsai trees (2)
            BuildBonsai(area.transform, new Vector3(-4f, 0, -1f));
            BuildBonsai(area.transform, new Vector3(1.5f, 0, -4f));
        }

        // ────────────────────────────────────────────────────────
        //  CENTER: HERB SPIRAL  (around 0,0,0)
        // ────────────────────────────────────────────────────────
        private static void BuildHerbSpiral(Transform parent)
        {
            var area = new GameObject("HerbSpiral");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = Vector3.zero; // exact center of zone

            // Circular stone paving under spiral
            var paving = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            paving.name = "SpiralPaving";
            paving.transform.SetParent(area.transform, false);
            paving.transform.localPosition = new Vector3(0, 0.015f, 0);
            paving.transform.localScale = new Vector3(4.5f, 0.01f, 4.5f);
            paving.GetComponent<Renderer>().material = ZoneFactory.StoneMat(PathStone * 0.9f);
            Object.Destroy(paving.GetComponent<Collider>());

            // Spiral stones — 1.5 turns
            float spiralRadius = 2.0f;
            int segments = 20;
            Color herbGreen = new(0.3f, 0.6f, 0.25f);

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 3f;
                float r = spiralRadius * (1f - t * 0.65f);
                float x = Mathf.Cos(angle) * r;
                float z = Mathf.Sin(angle) * r;
                float height = t * 0.5f;

                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = "SpiralStone";
                stone.transform.SetParent(area.transform, false);
                stone.transform.localPosition = new Vector3(x, height * 0.5f + 0.04f, z);
                stone.transform.localScale = new Vector3(0.4f, height + 0.08f, 0.4f);
                stone.transform.localRotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
                stone.GetComponent<Renderer>().material = ZoneFactory.StoneMat(
                    new Color(0.5f, 0.47f, 0.42f));
                Object.Destroy(stone.GetComponent<Collider>());

                // Herb at every other segment
                if (i % 2 == 0)
                {
                    float innerR = r - 0.35f;
                    var herb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    herb.name = "HerbPlant";
                    herb.transform.SetParent(area.transform, false);
                    herb.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * innerR, height + 0.12f, Mathf.Sin(angle) * innerR);
                    herb.transform.localScale = new Vector3(0.28f, 0.18f, 0.28f);
                    Color shade = Color.Lerp(herbGreen, new Color(0.5f, 0.7f, 0.2f), (i % 4) * 0.3f);
                    herb.GetComponent<Renderer>().material = ZoneFactory.CreateMat(shade);
                    Object.Destroy(herb.GetComponent<Collider>());
                }
            }

            // Center marker
            var center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.name = "SpiralCenter";
            center.transform.SetParent(area.transform, false);
            center.transform.localPosition = new Vector3(0, 0.35f, 0);
            center.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
            center.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.6f, 0.58f, 0.55f));
            Object.Destroy(center.GetComponent<Collider>());
        }

        // ────────────────────────────────────────────────────────
        //  SW: VEGETABLE PLOTS  (X[-14,-1.5], Z[-14,-1.5])
        //  Center: (-7.75, 0, -7.75), Size: 12.5 × 12.5
        // ────────────────────────────────────────────────────────
        private static void BuildVegetablePlots(Transform parent)
        {
            var area = new GameObject("VegetablePlots");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-7.75f, 0, -7.75f);

            // Rich fertile soil floor
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero,
                new Vector3(12.5f, 0, 12.5f), new Color(0.26f, 0.17f, 0.09f), "SoilFloor");

            // Cobblestone walking paths between rows
            for (float pathX = -5.2f; pathX <= 5.2f; pathX += 3.5f)
            {
                var pathTile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pathTile.name = "CobblePath_V";
                pathTile.transform.SetParent(area.transform, false);
                pathTile.transform.localPosition = new Vector3(pathX, 0.015f, 0);
                pathTile.transform.localScale = new Vector3(0.7f, 0.03f, 11.5f);
                pathTile.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.55f, 0.52f, 0.48f));
                Object.Destroy(pathTile.GetComponent<Collider>());
            }
            for (float pathZ = -4.5f; pathZ <= 4.5f; pathZ += 3.0f)
            {
                var pathTile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pathTile.name = "CobblePath_H";
                pathTile.transform.SetParent(area.transform, false);
                pathTile.transform.localPosition = new Vector3(0, 0.012f, pathZ);
                pathTile.transform.localScale = new Vector3(11.5f, 0.03f, 0.6f);
                pathTile.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.52f, 0.5f, 0.45f));
                Object.Destroy(pathTile.GetComponent<Collider>());
            }

            // Wooden fence borders
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, 6f), 12f, 0f, FenceWood, 0.4f);   // North
            ZoneFactory.CreateLowFence(area.transform, new Vector3(-6f, 0, 0), 12f, 90f, FenceWood, 0.4f); // West
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, -6f), 12f, 0f, FenceWood, 0.4f);  // South

            // Area sign
            ZoneFactory.CreateAreaSign(area.transform, new Vector3(0, 1.5f, 6.2f), "Vườn Rau Sinh Thái");

            // 4 Corner Wooden Posts with Fairy Lantern Lights
            Vector3[] lanternPosts = { new(-5.8f, 0, 5.8f), new(5.8f, 0, 5.8f), new(-5.8f, 0, -5.8f), new(5.8f, 0, -5.8f) };
            foreach (var lPos in lanternPosts)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "LanternPost";
                post.transform.SetParent(area.transform, false);
                post.transform.localPosition = lPos + new Vector3(0, 0.8f, 0);
                post.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
                post.GetComponent<Renderer>().material = ZoneFactory.WoodMat(FenceWood * 0.8f);
                Object.Destroy(post.GetComponent<Collider>());

                var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lamp.name = "LanternGlow";
                lamp.transform.SetParent(area.transform, false);
                lamp.transform.localPosition = lPos + new Vector3(0, 1.65f, 0);
                lamp.transform.localScale = Vector3.one * 0.28f;
                lamp.GetComponent<Renderer>().material = CharacterVisuals.CreateAdditiveMat(new Color(1f, 0.8f, 0.3f, 0.9f));
                Object.Destroy(lamp.GetComponent<Collider>());
            }

            // 3×3 grid of interactive GardenPlots
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float x = -3.5f + col * 3.5f;
                    float z = -3f + row * 3f;

                    // Interactive plot
                    var plot = new GameObject("GardenPlot");
                    plot.transform.SetParent(area.transform, false);
                    plot.transform.localPosition = new Vector3(x, 0.05f, z);
                    var gp = plot.AddComponent<GardenPlot>();
                    ZoneFactory.SetField(gp, "_growthDuration", 15f);
                    ZoneFactory.SetField(gp, "_harvestReward", 20);
                }
            }

            // Upgraded Scarecrow
            BuildScarecrow(area.transform, new Vector3(-5.2f, 0, -1.5f));

            // Upgraded Tool shed
            BuildToolShed(area.transform, new Vector3(4.8f, 0, -4.5f));

            // Wooden Crate Props with Harvested Produce
            Vector3[] cratePositions = { new(3.5f, 0, -4.2f), new(4.8f, 0, -3.0f) };
            foreach (var cPos in cratePositions)
            {
                var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = "HarvestCrate";
                crate.transform.SetParent(area.transform, false);
                crate.transform.localPosition = cPos + new Vector3(0, 0.25f, 0);
                crate.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
                crate.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.55f, 0.38f, 0.2f));
                Object.Destroy(crate.GetComponent<Collider>());

                var veg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                veg.transform.SetParent(crate.transform, false);
                veg.transform.localPosition = new Vector3(0, 0.55f, 0);
                veg.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
                veg.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.96f, 0.5f, 0.08f));
                Object.Destroy(veg.GetComponent<Collider>());
            }
        }

        // ────────────────────────────────────────────────────────
        //  SE: FOUNTAIN GARDEN  (X[1.5,14], Z[-14,-1.5])
        //  Center: (7.75, 0, -7.75), Size: 12.5 × 12.5
        // ────────────────────────────────────────────────────────
        private static void BuildFountainGarden(Transform parent)
        {
            var area = new GameObject("FountainGarden");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(7.75f, 0, -7.75f);

            // Smooth stone paving
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero,
                new Vector3(12.5f, 0, 12.5f), new Color(0.65f, 0.62f, 0.58f), "PavedFloor");

            // Low stone border (N, E, S)
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, 6f), 12f, 0f, WallStone, 0.3f);
            ZoneFactory.CreateLowFence(area.transform, new Vector3(6f, 0, 0), 12f, 90f, WallStone, 0.3f);
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, -6f), 12f, 0f, WallStone, 0.3f);

            // Area sign
            ZoneFactory.CreateAreaSign(area.transform, new Vector3(0, 1.5f, 6.2f), "Vườn Đài Phun");

            // Central fountain
            // Basin
            var basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.name = "FountainBasin";
            basin.transform.SetParent(area.transform, false);
            basin.transform.localPosition = new Vector3(0, 0.25f, 0);
            basin.transform.localScale = new Vector3(3f, 0.25f, 3f);
            basin.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.72f, 0.7f, 0.66f));

            // Water surface
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "FountainWater";
            water.transform.SetParent(area.transform, false);
            water.transform.localPosition = new Vector3(0, 0.38f, 0);
            water.transform.localScale = new Vector3(2.6f, 0.02f, 2.6f);
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
            bowl.transform.localPosition = new Vector3(0, 1.05f, 0);
            bowl.transform.localScale = new Vector3(0.6f, 0.25f, 0.6f);
            bowl.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.75f, 0.73f, 0.7f));
            Object.Destroy(bowl.GetComponent<Collider>());

            // Water ripples
            ZoneParticles.CreateWaterRipples(area.transform, new Vector3(0, 0.39f, 0), 1.2f);

            // 3 benches around fountain at 120° intervals
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f * Mathf.Deg2Rad;
                float dist = 3.8f;
                BuildBench(area.transform,
                    new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist),
                    -i * 120f + 90f,
                    new Color(0.5f, 0.35f, 0.2f));
            }

            // 4 flower pots at 90° intervals
            for (int i = 0; i < 4; i++)
            {
                float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                float dist = 4.5f;
                BuildFlowerPot(area.transform, new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist));
            }
        }

        // ────────────────────────────────────────────────────────
        //  CORNER TREES (4 corners of zone)
        // ────────────────────────────────────────────────────────
        private static void BuildCornerTrees(Transform parent, GameObject treePrefab)
        {
            Vector3[] positions = {
                new(-13f, 0, 13f), new(13f, 0, 13f),
                new(-13f, 0, -13f), new(13f, 0, -13f),
            };
            foreach (var pos in positions)
            {
                BuildProceduralTree(parent, pos);
            }
        }

        // ═══════════════════════ HELPER BUILDERS ═══════════════════════

        private static void BuildFlowerBush(Transform parent, Vector3 pos, Color flowerColor)
        {
            var bush = new GameObject("FlowerBush");
            bush.transform.SetParent(parent, false);
            bush.transform.localPosition = pos;

            float size = 0.5f;

            // Leaf base
            var leafBase = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leafBase.transform.SetParent(bush.transform, false);
            leafBase.transform.localPosition = new Vector3(0, size * 0.4f, 0);
            leafBase.transform.localScale = new Vector3(size * 1.2f, size * 0.6f, size * 1.2f);
            leafBase.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.25f, 0.55f, 0.2f));
            Object.Destroy(leafBase.GetComponent<Collider>());

            // Flower blooms (4 arranged in circle)
            for (int b = 0; b < 4; b++)
            {
                var bloom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bloom.transform.SetParent(bush.transform, false);
                float angle = b * 90f * Mathf.Deg2Rad;
                float r = size * 0.3f;
                bloom.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * r, size * 0.55f, Mathf.Sin(angle) * r);
                bloom.transform.localScale = Vector3.one * 0.12f;
                bloom.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(flowerColor);
                Object.Destroy(bloom.GetComponent<Collider>());
            }
        }

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

            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "BonsaiPot";
            pot.transform.SetParent(bonsai.transform, false);
            pot.transform.localPosition = new Vector3(0, 0.1f, 0);
            pot.transform.localScale = new Vector3(0.35f, 0.1f, 0.35f);
            pot.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.55f, 0.3f, 0.2f));
            Object.Destroy(pot.GetComponent<Collider>());

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(bonsai.transform, false);
            trunk.transform.localPosition = new Vector3(0, 0.35f, 0);
            trunk.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
            trunk.transform.localRotation = Quaternion.Euler(0, 0, 12f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));
            Object.Destroy(trunk.GetComponent<Collider>());

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

            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post_Collider";
            post.transform.SetParent(sc.transform, false);
            post.transform.localPosition = new Vector3(0, 0.7f, 0);
            post.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);
            post.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));

            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm";
            arm.transform.SetParent(sc.transform, false);
            arm.transform.localPosition = new Vector3(0, 1.1f, 0);
            arm.transform.localScale = new Vector3(0.8f, 0.06f, 0.06f);
            arm.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.38f, 0.25f, 0.12f));
            Object.Destroy(arm.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(sc.transform, false);
            head.transform.localPosition = new Vector3(0, 1.4f, 0);
            head.transform.localScale = Vector3.one * 0.25f;
            head.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.9f, 0.6f, 0.15f));
            Object.Destroy(head.GetComponent<Collider>());

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

            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "ShedWalls_Collider";
            walls.transform.SetParent(shed.transform, false);
            walls.transform.localPosition = new Vector3(0, 0.6f, 0);
            walls.transform.localScale = new Vector3(1.5f, 1.2f, 1.2f);
            walls.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.35f, 0.2f));

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

            var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flower.name = "Flower";
            flower.transform.SetParent(pot.transform, false);
            flower.transform.localPosition = new Vector3(0, 1.5f, 0);
            flower.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            Color[] colors = { new(0.9f, 0.4f, 0.5f), new(0.95f, 0.7f, 0.3f), new(0.8f, 0.3f, 0.8f) };
            flower.GetComponent<Renderer>().material = ZoneFactory.CreateMat(
                colors[Mathf.Abs(Mathf.RoundToInt(pos.x * 10f)) % colors.Length]);
            Object.Destroy(flower.GetComponent<Collider>());
        }

        private static void BuildProceduralTree(Transform parent, Vector3 pos)
        {
            var tree = new GameObject("ProceduralTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk_Collider";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = Vector3.up * 1.0f;
            trunk.transform.localScale = new Vector3(0.22f, 1.0f, 0.22f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));

            var c1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c1.transform.SetParent(tree.transform, false);
            c1.transform.localPosition = new Vector3(0, 2.2f, 0);
            c1.transform.localScale = new Vector3(1.8f, 1.2f, 1.8f);
            c1.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.55f, 0.18f));
            Object.Destroy(c1.GetComponent<Collider>());

            var c2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c2.transform.SetParent(tree.transform, false);
            c2.transform.localPosition = new Vector3(0.5f, 2.4f, 0.3f);
            c2.transform.localScale = new Vector3(1.2f, 0.9f, 1.2f);
            c2.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.25f, 0.6f, 0.2f));
            Object.Destroy(c2.GetComponent<Collider>());
        }
    }
}
