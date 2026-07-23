using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public static partial class StudyZoneBuilder
    {
        private static void BuildStonePath(Transform parent, Vector3 localPos, Vector3 scale)
        {
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = "StonePath";
            path.transform.SetParent(parent, false);
            path.transform.localPosition = localPos;
            path.transform.localScale = scale;
            path.GetComponent<Renderer>().material = ZoneFactory.StoneMat(PathColor);
            Object.Destroy(path.GetComponent<Collider>());
        }

        private static void BuildStoneBench(Transform parent, Vector3 localPos, float rotY)
        {
            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "StoneBench_Collider";
            bench.transform.SetParent(parent, false);
            bench.transform.localPosition = localPos;
            bench.transform.localScale = new Vector3(1.5f, 0.35f, 0.5f);
            bench.transform.localRotation = Quaternion.Euler(0, rotY, 0);
            bench.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.6f, 0.58f, 0.55f));
        }

        private static void BuildStudentDesk(Transform parent, Vector3 pos, float rotY, Color woodColor, Color legColor)
        {
            var desk = new GameObject("StudentDesk");
            desk.transform.SetParent(parent, false);
            desk.transform.localPosition = pos;
            desk.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Tabletop_Collider";
            top.transform.SetParent(desk.transform, false);
            top.transform.localPosition = new Vector3(0, 0.6f, 0);
            top.transform.localScale = new Vector3(1.5f, 0.06f, 0.7f);
            top.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            // Legs
            float[] lx = { -0.65f, 0.65f };
            float[] lz = { -0.28f, 0.28f };
            foreach (float xLeg in lx)
            {
                foreach (float zLeg in lz)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg";
                    leg.transform.SetParent(desk.transform, false);
                    leg.transform.localPosition = new Vector3(xLeg, 0.3f, zLeg);
                    leg.transform.localScale = new Vector3(0.05f, 0.3f, 0.05f);
                    leg.GetComponent<Renderer>().material = ZoneFactory.MetalMat(legColor);
                    Object.Destroy(leg.GetComponent<Collider>());
                }
            }

            // Student chair (facing North)
            var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chair.name = "ChairSeat_Collider";
            chair.transform.SetParent(desk.transform, false);
            chair.transform.localPosition = new Vector3(0, 0.35f, -0.65f);
            chair.transform.localScale = new Vector3(0.45f, 0.05f, 0.45f);
            chair.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 0.85f);

            var chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairBack.name = "ChairBack";
            chairBack.transform.SetParent(desk.transform, false);
            chairBack.transform.localPosition = new Vector3(0, 0.65f, -0.85f);
            chairBack.transform.localScale = new Vector3(0.45f, 0.5f, 0.05f);
            chairBack.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 0.85f);
            Object.Destroy(chairBack.GetComponent<Collider>());
        }

        private static void BuildBookshelf(Transform parent, Vector3 pos, float rotY, float height)
        {
            var shelf = new GameObject("Bookshelf");
            shelf.transform.SetParent(parent, false);
            shelf.transform.localPosition = pos;
            shelf.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            Color woodColor = new(0.45f, 0.3f, 0.15f);

            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back_Collider";
            back.transform.SetParent(shelf.transform, false);
            back.transform.localPosition = new Vector3(0, height * 0.5f, 0.25f);
            back.transform.localScale = new Vector3(1.8f, height, 0.06f);
            back.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            var sL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sL.name = "SideL";
            sL.transform.SetParent(shelf.transform, false);
            sL.transform.localPosition = new Vector3(-0.88f, height * 0.5f, 0);
            sL.transform.localScale = new Vector3(0.06f, height, 0.5f);
            sL.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);
            Object.Destroy(sL.GetComponent<Collider>());

            var sR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sR.name = "SideR";
            sR.transform.SetParent(shelf.transform, false);
            sR.transform.localPosition = new Vector3(0.88f, height * 0.5f, 0);
            sR.transform.localScale = new Vector3(0.06f, height, 0.5f);
            sR.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);
            Object.Destroy(sR.GetComponent<Collider>());

            int shelfCount = 4;
            for (int i = 0; i <= shelfCount; i++)
            {
                float h = i * (height / shelfCount);
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = "ShelfPlate";
                plate.transform.SetParent(shelf.transform, false);
                plate.transform.localPosition = new Vector3(0, h, 0);
                plate.transform.localScale = new Vector3(1.7f, 0.04f, 0.46f);
                plate.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);
                Object.Destroy(plate.GetComponent<Collider>());
            }

            Color[] bookColors = { Color.red, Color.blue, Color.yellow, Color.green, new(0.6f, 0.3f, 0.9f) };
            for (int s = 0; s < shelfCount; s++)
            {
                float sh = s * (height / shelfCount) + 0.06f;
                int bookCount = 6;
                for (int b = 0; b < bookCount; b++)
                {
                    float bh = 0.35f + (b % 3) * 0.04f;
                    var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    book.name = "Book";
                    book.transform.SetParent(shelf.transform, false);
                    book.transform.localPosition = new Vector3(-0.6f + b * 0.24f, sh + bh * 0.5f, 0);
                    book.transform.localScale = new Vector3(0.05f, bh, 0.3f);
                    book.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(bookColors[(s * 6 + b) % bookColors.Length]);
                    Object.Destroy(book.GetComponent<Collider>());
                }
            }
        }

        private static void BuildClock(Transform parent, Vector3 pos)
        {
            var clock = new GameObject("WallClock");
            clock.transform.SetParent(parent, false);
            clock.transform.localPosition = pos;

            var face = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            face.name = "ClockFace";
            face.transform.SetParent(clock.transform, false);
            face.transform.localScale = new Vector3(0.4f, 0.02f, 0.4f);
            face.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            face.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(Color.white);
            Object.Destroy(face.GetComponent<Collider>());

            var frame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frame.name = "ClockFrame";
            frame.transform.SetParent(clock.transform, false);
            frame.transform.localScale = new Vector3(0.44f, 0.025f, 0.44f);
            frame.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            frame.GetComponent<Renderer>().material = ZoneFactory.MetalMat(Color.black);
            Object.Destroy(frame.GetComponent<Collider>());
        }

        private static void BuildCourtyardTree(Transform parent, Vector3 pos)
        {
            var tree = new GameObject("CourtyardTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk_Collider";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0, 1.2f, 0);
            trunk.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));

            var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            leaves.transform.SetParent(tree.transform, false);
            leaves.transform.localPosition = new Vector3(0, 2.5f, 0);
            leaves.transform.localScale = new Vector3(2.5f, 2f, 2.5f);
            leaves.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.55f, 0.2f));
            Object.Destroy(leaves.GetComponent<Collider>());
        }

        private static void BuildDoorway(Transform parent, Vector3 localPos, float rotY, float width, float height, string label, Color frameColor)
        {
            var doorway = new GameObject("Doorway_" + label);
            doorway.transform.SetParent(parent, false);
            doorway.transform.localPosition = localPos;
            doorway.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            // Left post
            var left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "PostL";
            left.transform.SetParent(doorway.transform, false);
            left.transform.localPosition = new Vector3(-width * 0.5f, height * 0.5f, 0);
            left.transform.localScale = new Vector3(0.12f, height, 0.12f);
            left.GetComponent<Renderer>().material = ZoneFactory.WoodMat(frameColor);
            Object.Destroy(left.GetComponent<Collider>());

            // Right post
            var right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "PostR";
            right.transform.SetParent(doorway.transform, false);
            right.transform.localPosition = new Vector3(width * 0.5f, height * 0.5f, 0);
            right.transform.localScale = new Vector3(0.12f, height, 0.12f);
            right.GetComponent<Renderer>().material = ZoneFactory.WoodMat(frameColor);
            Object.Destroy(right.GetComponent<Collider>());

            // Lintel (top)
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Lintel";
            top.transform.SetParent(doorway.transform, false);
            top.transform.localPosition = new Vector3(0, height, 0);
            top.transform.localScale = new Vector3(width + 0.12f, 0.12f, 0.12f);
            top.GetComponent<Renderer>().material = ZoneFactory.WoodMat(frameColor);
            Object.Destroy(top.GetComponent<Collider>());

            // Sign above door
            if (!string.IsNullOrEmpty(label))
            {
                var sign = new GameObject("Sign");
                sign.transform.SetParent(doorway.transform, false);
                sign.transform.localPosition = new Vector3(0, height + 0.3f, 0);
                var tmp = sign.AddComponent<TextMeshPro>();
                tmp.text = label;
                tmp.fontSize = 1.4f;
                tmp.color = Color.white;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                sign.AddComponent<BillboardText>();
            }
        }
    }

    }
