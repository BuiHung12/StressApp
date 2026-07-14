using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Builds the Garden Zone (floating clouds, planting plots, golden tree).
    /// Located at world Z = +60.
    /// </summary>
    public static class GardenZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("GardenZone");
            zone.transform.position = new Vector3(0, 0, 60f);

            var woodColor = new Color(0.48f, 0.32f, 0.18f);
            var stoneColor = new Color(0.72f, 0.72f, 0.75f);
            var greenLeaves = new Color(0.18f, 0.54f, 0.2f);

            // Ground (Grass green, 16x16)
            var ground = ZoneFactory.CreateFlat("GardenGround", Vector3.zero, new Vector2(16f, 16f),
                new Color(0.35f, 0.68f, 0.18f));
            ground.transform.SetParent(zone.transform, false);

            // White fences around perimeter
            Color whiteColor = new Color(0.95f, 0.95f, 0.95f);
            float limit = 8f, step = 1.6f;

            for (float x = -limit; x < limit; x += step)
            {
                ZoneFactory.CreateFenceSegment(new Vector3(x, 0, limit),
                    new Vector3(x + step, 0, limit), whiteColor).transform.SetParent(zone.transform, false);
                ZoneFactory.CreateFenceSegment(new Vector3(x, 0, -limit),
                    new Vector3(x + step, 0, -limit), whiteColor).transform.SetParent(zone.transform, false);
            }
            for (float z = -limit; z < limit; z += step)
            {
                ZoneFactory.CreateFenceSegment(new Vector3(limit, 0, z),
                    new Vector3(limit, 0, z + step), whiteColor, true).transform.SetParent(zone.transform, false);
                ZoneFactory.CreateFenceSegment(new Vector3(-limit, 0, z),
                    new Vector3(-limit, 0, z + step), whiteColor, true).transform.SetParent(zone.transform, false);
            }

            // ── Decorative Entrance Archway ──
            var arch = new GameObject("GardenArch");
            arch.transform.SetParent(zone.transform, false);
            arch.transform.localPosition = new Vector3(0, 0, -5.2f);

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

            // Arch Flowers (Pink and red spheres climbing)
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

            // ── Stone Pathways Slabs ──
            var pathSlabs = new Vector3[] {
                new(0, 0.005f, -3.5f), new(0, 0.005f, -2.0f), new(0, 0.005f, -0.5f)
            };
            foreach (var pos in pathSlabs)
            {
                var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = "PathSlab";
                slab.transform.SetParent(zone.transform, false);
                slab.transform.localPosition = pos;
                slab.transform.localScale = new Vector3(0.9f, 0.01f, 0.6f);
                slab.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);
                Object.Destroy(slab.GetComponent<Collider>());
            }

            // ── Cozy Wishing Well ──
            var well = new GameObject("WishingWell");
            well.transform.SetParent(zone.transform, false);
            well.transform.localPosition = new Vector3(-5.2f, 0, 4.8f);

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

            // ── Garden Bench & Tea Table ──
            var restArea = new GameObject("GardenRestArea");
            restArea.transform.SetParent(zone.transform, false);
            restArea.transform.localPosition = new Vector3(4.8f, 0, 2.5f);

            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Bench_Collider";
            bench.transform.SetParent(restArea.transform, false);
            bench.transform.localPosition = new Vector3(0, 0.22f, 0);
            bench.transform.localScale = new Vector3(1.4f, 0.08f, 0.5f);
            bench.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var benchBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchBack.name = "BenchBack";
            benchBack.transform.SetParent(restArea.transform, false);
            benchBack.transform.localPosition = new Vector3(0, 0.65f, 0.22f);
            benchBack.transform.localScale = new Vector3(1.4f, 0.45f, 0.06f);
            benchBack.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);
            Object.Destroy(benchBack.GetComponent<Collider>());

            var table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            table.name = "GardenTable_Collider";
            table.transform.SetParent(restArea.transform, false);
            table.transform.localPosition = new Vector3(-1.1f, 0.35f, -0.4f);
            table.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
            table.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(stoneColor);

            // ── Magical Fairy Lanterns ──
            var lampPositions = new Vector3[] { new(-4.5f, 0, -4.5f), new(4.5f, 0, -4.5f) };
            foreach (var lpos in lampPositions)
            {
                var lampPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lampPost.name = "LanternPost_Collider";
                lampPost.transform.SetParent(zone.transform, false);
                lampPost.transform.localPosition = lpos + new Vector3(0, 0.9f, 0);
                lampPost.transform.localScale = new Vector3(0.06f, 0.9f, 0.06f);
                lampPost.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.2f, 0.2f));

                var lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lantern.name = "FairyLantern";
                lantern.transform.SetParent(zone.transform, false);
                lantern.transform.localPosition = lpos + new Vector3(0, 1.8f, 0);
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

            // ── Decorative Flower Patches ──
            var flowersData = new (Vector3 root, Color color)[] {
                (new(-6.2f, 0.01f, -5.8f), new(0.9f, 0.2f, 0.2f)),
                (new(6.2f, 0.01f, -5.8f), new(0.95f, 0.8f, 0.2f)),
                (new(-6.4f, 0.01f, 1.8f), new(0.95f, 0.95f, 0.98f)),
                (new(6.4f, 0.01f, -1.8f), new(0.72f, 0.3f, 0.85f))
            };
            foreach (var (root, fcolor) in flowersData)
            {
                for (int f = 0; f < 5; f++)
                {
                    float ox = (f - 2.5f) * 0.18f + Random.Range(-0.06f, 0.06f);
                    float oz = Random.Range(-0.2f, 0.2f);
                    float r = Random.Range(0.12f, 0.2f);

                    var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    flower.name = "GardenFlower";
                    flower.transform.SetParent(zone.transform, false);
                    flower.transform.localPosition = root + new Vector3(ox, r * 0.5f, oz);
                    flower.transform.localScale = Vector3.one * r;
                    flower.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(fcolor);
                    Object.Destroy(flower.GetComponent<Collider>());
                }
            }

            // ── Floating Clouds ──

            // Cloud Layer 2 (Medium, y=0.6)
            var cloudL2 = new GameObject("Cloud_L2");
            cloudL2.transform.SetParent(zone.transform, false);
            cloudL2.transform.localPosition = new Vector3(0, 0.6f, 0);
            ZoneFactory.CreateCloudVisual(cloudL2, 2.8f);

            // Cloud Layer 3 (High, y=1.2)
            var cloudL3 = new GameObject("Cloud_L3");
            cloudL3.transform.SetParent(zone.transform, false);
            cloudL3.transform.localPosition = new Vector3(0, 1.2f, 4f);
            ZoneFactory.CreateCloudVisual(cloudL3, 2.4f);

            // Cloud stairs (ramps)
            var stair1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stair1.name = "CloudStair_1";
            stair1.transform.SetParent(zone.transform, false);
            stair1.transform.localPosition = new Vector3(0, 0.28f, -2.5f);
            stair1.transform.localScale = new Vector3(2f, 0.5f, 2.5f);
            stair1.transform.localRotation = Quaternion.Euler(14f, 0, 0);
            stair1.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(Color.white);

            var stair2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stair2.name = "CloudStair_2";
            stair2.transform.SetParent(zone.transform, false);
            stair2.transform.localPosition = new Vector3(0, 0.88f, 2f);
            stair2.transform.localScale = new Vector3(2f, 0.5f, 2f);
            stair2.transform.localRotation = Quaternion.Euler(14f, 0, 0);
            stair2.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(Color.white);

            // ── Cloud Lock Barriers ──
            var barrier2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier2.name = "Stair1_Barrier_Collider";
            barrier2.transform.SetParent(zone.transform, false);
            barrier2.transform.localPosition = new Vector3(0, 0.6f, -1.8f);
            barrier2.transform.localScale = new Vector3(2f, 1f, 0.2f);
            barrier2.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
            var lock2 = barrier2.AddComponent<CloudLayer>();
            ZoneFactory.SetField(lock2, "_unlockCost", 10);
            ZoneFactory.SetField(lock2, "_barrierObj", barrier2);

            var barrier3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier3.name = "Stair2_Barrier_Collider";
            barrier3.transform.SetParent(zone.transform, false);
            barrier3.transform.localPosition = new Vector3(0, 1.2f, 1.4f);
            barrier3.transform.localScale = new Vector3(2f, 1f, 0.2f);
            barrier3.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
            var lock3 = barrier3.AddComponent<CloudLayer>();
            ZoneFactory.SetField(lock3, "_unlockCost", 25);
            ZoneFactory.SetField(lock3, "_barrierObj", barrier3);

            // ── Planting Plots ──
            CreatePlot(zone, new Vector3(-2f, 0.05f, -4f), 10f, 10);
            CreatePlot(zone, new Vector3(2f, 0.05f, -4f), 10f, 10);
            CreatePlot(zone, new Vector3(-1.5f, 0.65f, 0f), 15f, 25);
            CreatePlot(zone, new Vector3(1.5f, 0.65f, 0f), 15f, 25);
            CreatePlot(zone, new Vector3(-1.2f, 1.25f, 4f), 20f, 50);
            CreatePlot(zone, new Vector3(1.2f, 1.25f, 4f), 20f, 50);

            // ── Magnificent Golden Canopy Tree on Cloud 2 ──
            var goldTree = new GameObject("GoldenCloudTree");
            goldTree.transform.SetParent(zone.transform, false);
            goldTree.transform.localPosition = new Vector3(0, 0.6f, 0);

            var trunkBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunkBase.name = "Trunk_Collider";
            trunkBase.transform.SetParent(goldTree.transform, false);
            trunkBase.transform.localPosition = new Vector3(0, 0.6f, 0);
            trunkBase.transform.localScale = new Vector3(0.24f, 0.6f, 0.24f);
            trunkBase.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.42f, 0.28f, 0.15f));

            // Branches
            Vector3[] branchData = {
                new(-0.25f, 1.1f, 0), new(0.22f, 1.25f, 0.1f), new(0f, 1.2f, -0.22f)
            };
            Vector3[] branchScale = {
                new(0.08f, 0.35f, 0.08f), new(0.06f, 0.32f, 0.06f), new(0.06f, 0.32f, 0.06f)
            };
            Quaternion[] branchRot = {
                Quaternion.Euler(0, 0, 45f), Quaternion.Euler(-30f, 0, -45f), Quaternion.Euler(45f, 30f, 0)
            };
            for (int i = 0; i < 3; i++)
            {
                var br = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                br.name = "TreeBranch";
                br.transform.SetParent(goldTree.transform, false);
                br.transform.localPosition = branchData[i];
                br.transform.localScale = branchScale[i];
                br.transform.localRotation = branchRot[i];
                br.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.42f, 0.28f, 0.15f));
                Object.Destroy(br.GetComponent<Collider>());
            }

            // Canopy layers (overlapping spheres of gold, orange, and pale yellow)
            Color goldLeaf = new Color(1.0f, 0.85f, 0.2f);
            Color orangeLeaf = new Color(0.95f, 0.65f, 0.15f);
            Color paleLeaf = new Color(1.0f, 0.92f, 0.5f);

            var canopyCenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopyCenter.transform.SetParent(goldTree.transform, false);
            canopyCenter.transform.localPosition = new Vector3(0, 1.7f, 0);
            canopyCenter.transform.localScale = Vector3.one * 1.2f;
            canopyCenter.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(goldLeaf);
            Object.Destroy(canopyCenter.GetComponent<Collider>());

            var canopyL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopyL.transform.SetParent(goldTree.transform, false);
            canopyL.transform.localPosition = new Vector3(-0.65f, 1.4f, -0.1f);
            canopyL.transform.localScale = Vector3.one * 0.85f;
            canopyL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(orangeLeaf);
            Object.Destroy(canopyL.GetComponent<Collider>());

            var canopyR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopyR.transform.SetParent(goldTree.transform, false);
            canopyR.transform.localPosition = new Vector3(0.6f, 1.5f, 0.15f);
            canopyR.transform.localScale = Vector3.one * 0.8f;
            canopyR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(orangeLeaf);
            Object.Destroy(canopyR.GetComponent<Collider>());

            var canopyTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopyTop.transform.SetParent(goldTree.transform, false);
            canopyTop.transform.localPosition = new Vector3(0.05f, 2.15f, -0.05f);
            canopyTop.transform.localScale = Vector3.one * 0.95f;
            canopyTop.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(paleLeaf);
            Object.Destroy(canopyTop.GetComponent<Collider>());

            // ── Title ──
            var title = new GameObject("GardenTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3.4f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Khu Vuon Cua Ban";
            tmp.fontSize = 6.4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ── Return Portal ──
            ZoneFactory.CreatePortal("GardenReturnPortal", new Vector3(0, 0.05f, 54f), 180f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.15f, 0.85f, 0.3f, 0.7f),
                "Ve Sanh Cho");
        }

        private static void CreatePlot(GameObject parent, Vector3 localPos, float duration, int reward)
        {
            var plotObj = new GameObject("PlanterPlot");
            plotObj.transform.SetParent(parent.transform, false);
            plotObj.transform.localPosition = localPos;
            var plot = plotObj.AddComponent<GardenPlot>();
            ZoneFactory.SetField(plot, "_growthDuration", duration);
            ZoneFactory.SetField(plot, "_harvestReward", reward);
        }
    }
}
