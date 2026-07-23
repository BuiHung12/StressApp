using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public static partial class GardenZoneBuilder
    {
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
