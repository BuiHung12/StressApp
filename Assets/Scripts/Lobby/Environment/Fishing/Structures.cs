using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public static partial class FishingZoneBuilder
    {
        private static void BuildDock(Transform parent)
        {
            var area = new GameObject("DockBoardwalk");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(0, 0f, 8f);

            // Area sign
            ZoneFactory.CreateAreaSign(area.transform, new Vector3(-1.5f, 0.6f, 0f), "Cầu Tàu");

            // Plank details (8 planks, evenly spaced)
            for (int i = 0; i < 8; i++)
            {
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = "DockPlank";
                plank.transform.SetParent(area.transform, false);
                plank.transform.localPosition = new Vector3(i * 1.5f - 5.25f, 0.02f, 0);
                plank.transform.localScale = new Vector3(1.4f, 0.04f, 8f);
                plank.GetComponent<Renderer>().material = ZoneFactory.WoodMat(
                    new Color(0.48f, 0.35f, 0.22f));
                Object.Destroy(plank.GetComponent<Collider>());
            }

            // 3 Interactive Fishing Spots with fishing stools, rods, and bobbers
            Vector3[] spotPositions = { new(-4.5f, 0.05f, 2.5f), new(0f, 0.05f, 2.5f), new(4.5f, 0.05f, 2.5f) };
            foreach (var sPos in spotPositions)
            {
                var spotObj = new GameObject("FishingSpot");
                spotObj.transform.SetParent(area.transform, false);
                spotObj.transform.localPosition = sPos;
                spotObj.AddComponent<FishingSpot>();
            }

            // Rope coils and barrel on dock
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel_Collider";
            barrel.transform.SetParent(area.transform, false);
            barrel.transform.localPosition = new Vector3(-5f, 0.3f, -2f);
            barrel.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            barrel.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.32f, 0.15f));

            var bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucket.name = "BaitBucket_Collider";
            bucket.transform.SetParent(area.transform, false);
            bucket.transform.localPosition = new Vector3(-4f, 0.15f, 2f);
            bucket.transform.localScale = new Vector3(0.25f, 0.15f, 0.25f);
            bucket.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.45f, 0.45f, 0.48f));
        }

        // ────────────────────────────────────────────────────────
        //  BOAT HOUSE (North-West: X[-14,-7], Z[3,14])
        //  Center: (-10, 0.3, 9)
        // ────────────────────────────────────────────────────────
        private static void BuildBoatHouse(Transform parent)
        {
            var area = new GameObject("BoatHouse");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-10f, 0f, 9f);

            // Sub-area floor
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero,
                new Vector3(6f, 0, 6f), new Color(0.42f, 0.3f, 0.18f), "BoatHouseFloor");

            // Walls
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "BoatHouseWalls_Collider";
            walls.transform.SetParent(area.transform, false);
            walls.transform.localPosition = new Vector3(0, 0.8f, 0);
            walls.transform.localScale = new Vector3(3f, 1.6f, 2.5f);
            walls.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.35f, 0.2f));

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(area.transform, false);
            roof.transform.localPosition = new Vector3(0, 1.7f, 0);
            roof.transform.localScale = new Vector3(3.4f, 0.12f, 2.9f);
            roof.transform.localRotation = Quaternion.Euler(0, 0, 6f);
            roof.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.55f, 0.2f, 0.12f));
            Object.Destroy(roof.GetComponent<Collider>());

            // Sign
            var signObj = new GameObject("BaitSign");
            signObj.transform.SetParent(area.transform, false);
            signObj.transform.localPosition = new Vector3(0, 2f, -1.3f);
            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = "Cửa Hàng Mồi";
            signTmp.fontSize = 2f;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = new Color(0.2f, 0.15f, 0.1f);
            signObj.AddComponent<BillboardText>();
        }

        // ────────────────────────────────────────────────────────
        //  LIGHTHOUSE (North-East: X[7,14], Z[5,14])
        //  Center: (10, 0.3, 10)
        // ────────────────────────────────────────────────────────
        private static void BuildLighthouse(Transform parent)
        {
            var area = new GameObject("Lighthouse");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(10f, 0f, 10f);

            // Separate stone platform
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero,
                new Vector3(5f, 0, 5f), new Color(0.5f, 0.48f, 0.45f), "LighthouseBase");

            // Low fence around lighthouse
            ZoneFactory.CreateLowFence(area.transform, new Vector3(0, 0, 2.3f), 4.5f, 0f, WetStone, 0.4f);
            ZoneFactory.CreateLowFence(area.transform, new Vector3(2.3f, 0, 0), 4.5f, 90f, WetStone, 0.4f);

            // Foundation
            var foundation = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            foundation.name = "LHFoundation_Collider";
            foundation.transform.SetParent(area.transform, false);
            foundation.transform.localPosition = new Vector3(0, 0.1f, 0);
            foundation.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f);
            foundation.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.55f, 0.52f, 0.5f));

            // Tower tier 1
            var tier1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tier1.name = "LHTier1_Collider";
            tier1.transform.SetParent(area.transform, false);
            tier1.transform.localPosition = new Vector3(0, 1.2f, 0);
            tier1.transform.localScale = new Vector3(1.2f, 1.0f, 1.2f);
            tier1.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.88f, 0.85f, 0.8f));

            // Tower tier 2
            var tier2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tier2.name = "LHTier2";
            tier2.transform.SetParent(area.transform, false);
            tier2.transform.localPosition = new Vector3(0, 2.8f, 0);
            tier2.transform.localScale = new Vector3(0.9f, 0.8f, 0.9f);
            tier2.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.85f, 0.82f, 0.78f));
            Object.Destroy(tier2.GetComponent<Collider>());

            // Red stripe
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe.name = "RedStripe";
            stripe.transform.SetParent(area.transform, false);
            stripe.transform.localPosition = new Vector3(0, 2.3f, 0);
            stripe.transform.localScale = new Vector3(1.05f, 0.15f, 1.05f);
            stripe.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.85f, 0.2f, 0.15f));
            Object.Destroy(stripe.GetComponent<Collider>());

            // Observation deck
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            deck.name = "ObsDeck";
            deck.transform.SetParent(area.transform, false);
            deck.transform.localPosition = new Vector3(0, 3.6f, 0);
            deck.transform.localScale = new Vector3(1.4f, 0.05f, 1.4f);
            deck.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.65f, 0.63f, 0.6f));
            Object.Destroy(deck.GetComponent<Collider>());

            // Lantern room
            var lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lantern.name = "LanternRoom";
            lantern.transform.SetParent(area.transform, false);
            lantern.transform.localPosition = new Vector3(0, 4.0f, 0);
            lantern.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
            lantern.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(1f, 0.95f, 0.7f, 0.7f));
            Object.Destroy(lantern.GetComponent<Collider>());

            // Beacon light
            var beacon = new GameObject("BeaconLight");
            beacon.transform.SetParent(area.transform, false);
            beacon.transform.localPosition = new Vector3(0, 4.0f, 0);
            var bl = beacon.AddComponent<Light>();
            bl.type = LightType.Spot;
            bl.color = new Color(1f, 0.95f, 0.7f);
            bl.range = 20f;
            bl.intensity = 3f;
            bl.spotAngle = 50f;
            bl.transform.localRotation = Quaternion.Euler(15f, 0, 0);

            // Cap
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "LHCap";
            cap.transform.SetParent(area.transform, false);
            cap.transform.localPosition = new Vector3(0, 4.35f, 0);
            cap.transform.localScale = new Vector3(0.4f, 0.15f, 0.4f);
            cap.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.3f, 0.3f, 0.35f));
            Object.Destroy(cap.GetComponent<Collider>());
        }

        // ═══════════════════════ HELPER BUILDERS ═══════════════════════

        private static void BuildPalmTree(Transform parent, Vector3 pos)
        {
            var tree = new GameObject("PalmTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            // Trunk
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "PalmTrunk_Collider";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0, 1.2f, 0);
            trunk.transform.localScale = new Vector3(0.15f, 1.2f, 0.15f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.38f, 0.22f));

            // Fronds (5, evenly spaced)
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f * Mathf.Deg2Rad;
                var frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frond.name = "Frond";
                frond.transform.SetParent(tree.transform, false);
                frond.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.6f, 2.3f, Mathf.Sin(angle) * 0.6f);
                frond.transform.localScale = new Vector3(0.15f, 0.03f, 1.0f);
                frond.transform.localRotation = Quaternion.Euler(30f, angle * Mathf.Rad2Deg, 0);
                frond.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.22f, 0.6f, 0.2f));
                Object.Destroy(frond.GetComponent<Collider>());
            }

            // Coconuts (2)
            for (int i = 0; i < 2; i++)
            {
                var coconut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coconut.name = "Coconut";
                coconut.transform.SetParent(tree.transform, false);
                coconut.transform.localPosition = new Vector3(
                    (i == 0 ? -0.1f : 0.1f), 2.15f, (i == 0 ? 0.05f : -0.05f));
                coconut.transform.localScale = Vector3.one * 0.1f;
                coconut.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.35f, 0.15f));
                Object.Destroy(coconut.GetComponent<Collider>());
            }
        }

        private static void BuildSandcastle(Transform parent, Vector3 pos)
        {
            var castle = new GameObject("Sandcastle");
            castle.transform.SetParent(parent, false);
            castle.transform.localPosition = pos;

            Color sandColor = new(0.85f, 0.78f, 0.55f);

            // Base mound
            var mound = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mound.name = "Mound";
            mound.transform.SetParent(castle.transform, false);
            mound.transform.localPosition = new Vector3(0, 0.1f, 0);
            mound.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
            mound.GetComponent<Renderer>().material = ZoneFactory.CreateMat(sandColor);
            Object.Destroy(mound.GetComponent<Collider>());

            // 3 towers
            Vector3[] towerPos = { new(-0.15f, 0.25f, 0.1f), new(0.15f, 0.25f, 0.1f), new(0, 0.3f, -0.1f) };
            foreach (var tp in towerPos)
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = "Tower";
                tower.transform.SetParent(castle.transform, false);
                tower.transform.localPosition = tp;
                tower.transform.localScale = new Vector3(0.1f, 0.14f, 0.1f);
                tower.GetComponent<Renderer>().material = ZoneFactory.CreateMat(sandColor * 0.95f);
                Object.Destroy(tower.GetComponent<Collider>());
            }
        }
    }

    }
