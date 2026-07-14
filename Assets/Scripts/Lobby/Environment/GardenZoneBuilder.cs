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

            // Ground (Grass, 16x16)
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
            stair1.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Color.white);

            var stair2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stair2.name = "CloudStair_2";
            stair2.transform.SetParent(zone.transform, false);
            stair2.transform.localPosition = new Vector3(0, 0.88f, 2f);
            stair2.transform.localScale = new Vector3(2f, 0.5f, 2f);
            stair2.transform.localRotation = Quaternion.Euler(14f, 0, 0);
            stair2.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Color.white);

            // ── Cloud Lock Barriers ──

            var barrier2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier2.name = "Stair1_Barrier_Collider";
            barrier2.transform.SetParent(zone.transform, false);
            barrier2.transform.localPosition = new Vector3(0, 0.6f, -1.8f);
            barrier2.transform.localScale = new Vector3(2f, 1f, 0.2f);
            barrier2.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
            var lock2 = barrier2.AddComponent<CloudLayer>();
            ZoneFactory.SetField(lock2, "_unlockCost", 10);
            ZoneFactory.SetField(lock2, "_barrierObj", barrier2);

            var barrier3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier3.name = "Stair2_Barrier_Collider";
            barrier3.transform.SetParent(zone.transform, false);
            barrier3.transform.localPosition = new Vector3(0, 1.2f, 1.4f);
            barrier3.transform.localScale = new Vector3(2f, 1f, 0.2f);
            barrier3.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.9f, 0.1f, 0.1f, 0.3f));
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

            // ── Golden Tree on Cloud 2 ──
            var goldTree = new GameObject("GoldenCloudTree");
            goldTree.transform.SetParent(zone.transform, false);
            goldTree.transform.localPosition = new Vector3(0, 0.6f, 0);

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk_Collider";
            trunk.transform.SetParent(goldTree.transform, false);
            trunk.transform.localPosition = new Vector3(0, 0.5f, 0);
            trunk.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.35f, 0.2f));

            var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.transform.SetParent(goldTree.transform, false);
            leaves.transform.localPosition = new Vector3(0, 1.1f, 0);
            leaves.transform.localScale = Vector3.one * 0.7f;
            leaves.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(1f, 0.85f, 0.2f));
            Object.Destroy(leaves.GetComponent<Collider>());

            // ── Title ──
            var title = new GameObject("GardenTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Khu Vuon Cua Ban";
            tmp.fontSize = 6f;
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
