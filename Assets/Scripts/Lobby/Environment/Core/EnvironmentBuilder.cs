using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Helper to build procedural 3D elements for the Waiting Lobby.
    /// </summary>
    public static partial class EnvironmentBuilder
    {
        public static void CreateTrees3D(GameObject treePrefab = null)
        {
            Vector3[] positions = {
                new(-10, 0, 10), new(-8, 0, 12), new(-12, 0, 8),
                new(10, 0, 10), new(12, 0, 8), new(8, 0, 12),
                new(-10, 0, -10), new(-12, 0, -8), new(-8, 0, -12),
                new(10, 0, -10), new(8, 0, -12), new(12, 0, -8),
                new(-5, 0, 13), new(5, 0, 13),
                new(-5, 0, -13), new(5, 0, -13),
                new(-13, 0, 3), new(-13, 0, -3), new(13, 0, 3),
            };

            Color[] greens = {
                new(0.18f, 0.54f, 0.20f),
                new(0.24f, 0.60f, 0.26f),
                new(0.15f, 0.48f, 0.18f),
            };

            foreach (var pos in positions)
            {
                float scale = Random.Range(0.85f, 1.25f);

                if (treePrefab != null)
                {
                    var treeInstance = Object.Instantiate(treePrefab);
                    treeInstance.name = "Tree";
                    treeInstance.transform.position = pos;
                    treeInstance.transform.localScale = Vector3.one * scale;

                    // Ensure there's a collider so player cannot walk through the trees
                    if (treeInstance.GetComponent<Collider>() == null && treeInstance.GetComponentInChildren<Collider>() == null)
                    {
                        var col = treeInstance.AddComponent<CapsuleCollider>();
                        col.center = new Vector3(0, 1.0f, 0);
                        col.radius = 0.25f;
                        col.height = 2.0f;
                    }
                    continue;
                }

                var tree = new GameObject("Tree");

                Color baseGreen = greens[Random.Range(0, greens.Length)];
                Color lightGreen = new Color(
                    Mathf.Min(baseGreen.r * 1.2f, 1f),
                    Mathf.Min(baseGreen.g * 1.2f, 1f),
                    Mathf.Min(baseGreen.b * 1.2f, 1f)
                );
                Color darkGreen = baseGreen * 0.8f;
                Color trunkColor = new Color(0.48f, 0.31f, 0.18f);

                // --- Trunk ---
                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk_Collider";
                trunk.transform.SetParent(tree.transform, false);
                trunk.transform.localPosition = new Vector3(0, 1.0f * scale, 0);
                trunk.transform.localScale = new Vector3(0.25f * scale, 1.0f * scale, 0.25f * scale);
                trunk.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(trunkColor);

                // --- Branches ---
                var leftBranch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leftBranch.name = "LeftBranch";
                leftBranch.transform.SetParent(tree.transform, false);
                leftBranch.transform.localPosition = new Vector3(-0.25f * scale, 1.3f * scale, 0);
                leftBranch.transform.localScale = new Vector3(0.10f * scale, 0.35f * scale, 0.10f * scale);
                leftBranch.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                leftBranch.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(trunkColor);
                Object.Destroy(leftBranch.GetComponent<Collider>());

                var rightBranch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rightBranch.name = "RightBranch";
                rightBranch.transform.SetParent(tree.transform, false);
                rightBranch.transform.localPosition = new Vector3(0.2f * scale, 1.5f * scale, 0.1f * scale);
                rightBranch.transform.localScale = new Vector3(0.08f * scale, 0.3f * scale, 0.08f * scale);
                rightBranch.transform.localRotation = Quaternion.Euler(-30f, 0, -45f);
                rightBranch.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(trunkColor);
                Object.Destroy(rightBranch.GetComponent<Collider>());

                // --- Canopy Layers ---
                var canopyMain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyMain.name = "CanopyMain";
                canopyMain.transform.SetParent(tree.transform, false);
                canopyMain.transform.localPosition = new Vector3(0, 2.3f * scale, 0);
                canopyMain.transform.localScale = new Vector3(1.8f * scale, 1.6f * scale, 1.8f * scale);
                canopyMain.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(baseGreen);
                Object.Destroy(canopyMain.GetComponent<Collider>());

                var canopyLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyLeft.name = "CanopyLeft";
                canopyLeft.transform.SetParent(tree.transform, false);
                canopyLeft.transform.localPosition = new Vector3(-0.75f * scale, 2.0f * scale, -0.1f * scale);
                canopyLeft.transform.localScale = new Vector3(1.2f * scale, 1.1f * scale, 1.2f * scale);
                canopyLeft.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(darkGreen);
                Object.Destroy(canopyLeft.GetComponent<Collider>());

                var canopyRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyRight.name = "CanopyRight";
                canopyRight.transform.SetParent(tree.transform, false);
                canopyRight.transform.localPosition = new Vector3(0.7f * scale, 2.2f * scale, 0.2f * scale);
                canopyRight.transform.localScale = new Vector3(1.1f * scale, 1.0f * scale, 1.1f * scale);
                canopyRight.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(darkGreen);
                Object.Destroy(canopyRight.GetComponent<Collider>());

                var canopyTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyTop.name = "CanopyTop";
                canopyTop.transform.SetParent(tree.transform, false);
                canopyTop.transform.localPosition = new Vector3(0.05f * scale, 2.9f * scale, -0.05f * scale);
                canopyTop.transform.localScale = new Vector3(1.3f * scale, 1.1f * scale, 1.3f * scale);
                canopyTop.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(lightGreen);
                Object.Destroy(canopyTop.GetComponent<Collider>());

                // --- Apples ---
                Vector3[] appleOffsets = {
                    new(-0.4f, 1.9f, 0.5f),
                    new(0.45f, 1.8f, -0.4f),
                    new(-0.2f, 2.6f, 0.6f),
                    new(0.55f, 2.4f, 0.1f)
                };
                Color appleColor = new Color(0.95f, 0.15f, 0.15f);

                for (int i = 0; i < appleOffsets.Length; i++)
                {
                    var apple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    apple.name = $"Apple_{i}";
                    apple.transform.SetParent(tree.transform, false);
                    apple.transform.localPosition = appleOffsets[i] * scale;
                    apple.transform.localScale = Vector3.one * 0.15f * scale;
                    apple.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(appleColor);
                    Object.Destroy(apple.GetComponent<Collider>());
                }

                // --- Shadow ---
                var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shadow.name = "Shadow";
                shadow.transform.SetParent(tree.transform, false);
                shadow.transform.localPosition = new Vector3(0.2f * scale, 0.005f, -0.2f * scale);
                shadow.transform.localScale = new Vector3(2.2f * scale, 0.005f, 2.2f * scale);
                shadow.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.15f, 0.35f, 0.1f, 0.35f));
                Object.Destroy(shadow.GetComponent<Collider>());

                tree.transform.position = pos;
            }
        }

        public static void CreateFlowers(GameObject flowerPrefab = null)
        {
            Color[] petalColors = {
                new(1.0f, 0.2f, 0.45f), // Rose Pink
                new(1.0f, 0.72f, 0.0f),  // Gold Marigold
                new(0.65f, 0.2f, 0.95f), // Lavender Purple
                new(0.0f, 0.85f, 1.0f),  // Electric Cyan
                new(1.0f, 0.35f, 0.15f), // Coral Sunset
            };

            Vector3[] patches = {
                new(-6.5f, 0, 3.5f), new(6.5f, 0, 3.5f), new(-6.5f, 0, -3.5f), new(6.5f, 0, -3.5f),
                new(-3.5f, 0, 6.5f), new(3.5f, 0, 6.5f), new(-3.5f, 0, -6.5f), new(3.5f, 0, -6.5f),
                new(-2.2f, 0, 2.2f), new(2.2f, 0, 2.2f), new(-2.2f, 0, -2.2f), new(2.2f, 0, -2.2f)
            };

            foreach (var patchPos in patches)
            {
                var bed = new GameObject("FlowerBed");
                bed.transform.position = patchPos;

                // Soil patch
                var soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                soil.name = "Soil";
                soil.transform.SetParent(bed.transform, false);
                soil.transform.localPosition = new Vector3(0, 0.015f, 0);
                soil.transform.localScale = new Vector3(1.8f, 0.02f, 1.8f);
                soil.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.28f, 0.18f, 0.12f));
                Object.Destroy(soil.GetComponent<Collider>());

                // Stone curb border
                var border = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                border.name = "StoneBorder";
                border.transform.SetParent(bed.transform, false);
                border.transform.localPosition = new Vector3(0, 0.025f, 0);
                border.transform.localScale = new Vector3(1.95f, 0.03f, 1.95f);
                border.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.55f, 0.52f, 0.5f));
                Object.Destroy(border.GetComponent<Collider>());

                for (int i = 0; i < 6; i++)
                {
                    Vector3 offset = new(Random.Range(-0.65f, 0.65f), 0, Random.Range(-0.65f, 0.65f));

                    if (flowerPrefab != null)
                    {
                        var flowerInstance = Object.Instantiate(flowerPrefab);
                        flowerInstance.name = "Flower";
                        flowerInstance.transform.position = patchPos + offset;
                        flowerInstance.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        continue;
                    }

                    Color c = petalColors[Random.Range(0, petalColors.Length)];
                    var flGroup = new GameObject($"Flower_{i}");
                    flGroup.transform.SetParent(bed.transform, false);
                    flGroup.transform.localPosition = offset;

                    // Stem & Leaves
                    var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stem.name = "Stem";
                    stem.transform.SetParent(flGroup.transform, false);
                    stem.transform.localPosition = new Vector3(0, 0.18f, 0);
                    stem.transform.localScale = new Vector3(0.04f, 0.18f, 0.04f);
                    stem.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.18f, 0.52f, 0.22f));
                    Object.Destroy(stem.GetComponent<Collider>());

                    var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    leaf.name = "Leaf";
                    leaf.transform.SetParent(flGroup.transform, false);
                    leaf.transform.localPosition = new Vector3(0.08f, 0.12f, 0);
                    leaf.transform.localScale = new Vector3(0.18f, 0.04f, 0.08f);
                    leaf.transform.localRotation = Quaternion.Euler(0, 0, 25f);
                    leaf.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.24f, 0.6f, 0.26f));
                    Object.Destroy(leaf.GetComponent<Collider>());

                    // Blossom Head
                    var center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    center.name = "Center";
                    center.transform.SetParent(flGroup.transform, false);
                    center.transform.localPosition = new Vector3(0, 0.38f, 0);
                    center.transform.localScale = Vector3.one * 0.14f;
                    center.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.9f, 0.2f));
                    Object.Destroy(center.GetComponent<Collider>());

                    // Petals
                    for (int p = 0; p < 5; p++)
                    {
                        float angle = p * 72f * Mathf.Deg2Rad;
                        var petal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        petal.name = $"Petal_{p}";
                        petal.transform.SetParent(flGroup.transform, false);
                        petal.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.11f, 0.38f, Mathf.Sin(angle) * 0.11f);
                        petal.transform.localScale = new Vector3(0.13f, 0.08f, 0.13f);
                        petal.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(c);
                        Object.Destroy(petal.GetComponent<Collider>());
                    }
                }
            }
        }

        public static void CreateBenches3D()
        {
            Vector3[] positions = { new(-4, 0, 5), new(4, 0, 5) };
            foreach (var pos in positions)
            {
                var bench = new GameObject("Bench");
                Color wood = new Color(0.55f, 0.36f, 0.25f);

                var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seat.name = "Seat_Collider";
                seat.transform.SetParent(bench.transform, false);
                seat.transform.localPosition = new Vector3(0, 0.35f, 0);
                seat.transform.localScale = new Vector3(1.2f, 0.08f, 0.4f);
                seat.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(wood);

                var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                back.name = "Back";
                back.transform.SetParent(bench.transform, false);
                back.transform.localPosition = new Vector3(0, 0.55f, -0.18f);
                back.transform.localScale = new Vector3(1.2f, 0.4f, 0.06f);
                back.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(wood);
                Object.Destroy(back.GetComponent<Collider>());

                for (int i = 0; i < 2; i++)
                {
                    float x = i == 0 ? -0.5f : 0.5f;
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = $"Leg_{i}";
                    leg.transform.SetParent(bench.transform, false);
                    leg.transform.localPosition = new Vector3(x, 0.17f, 0);
                    leg.transform.localScale = new Vector3(0.06f, 0.34f, 0.35f);
                    leg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.3f, 0.3f, 0.3f));
                    Object.Destroy(leg.GetComponent<Collider>());
                }

                var textObj = new GameObject("BenchText");
                textObj.transform.SetParent(bench.transform, false);
                textObj.transform.localPosition = new Vector3(0, 0.58f, -0.28f);
                var tmp = textObj.AddComponent<TextMeshPro>();
                tmp.text = "<color=#FF6699><b>I Love You</b></color>";
                tmp.fontSize = 1.8f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                textObj.AddComponent<BillboardText>();

                bench.transform.position = pos;
            }
        }

        public static void CreateStreetLamps()
        {
            Vector3[] positions = {
                new(-3.2f, 0, 3.2f), new(3.2f, 0, 3.2f),
                new(-3.2f, 0, -3.2f), new(3.2f, 0, -3.2f),
                new(-9.0f, 0, 2.0f), new(9.0f, 0, 2.0f),
                new(-2.0f, 0, 9.0f), new(2.0f, 0, -9.0f)
            };
            Color postColor = new Color(0.15f, 0.18f, 0.22f);
            Color goldAccent = new Color(0.92f, 0.76f, 0.28f);
            Color lampGlow = new Color(1.0f, 0.92f, 0.45f);

            foreach (var pos in positions)
            {
                var lamp = new GameObject("StreetLamp");

                // Pedestal Base
                var basePed = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                basePed.name = "BasePedestal";
                basePed.transform.SetParent(lamp.transform, false);
                basePed.transform.localPosition = new Vector3(0, 0.1f, 0);
                basePed.transform.localScale = new Vector3(0.28f, 0.1f, 0.28f);
                basePed.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(postColor);
                Object.Destroy(basePed.GetComponent<Collider>());

                // Main Fluted Post
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Post_Collider";
                post.transform.SetParent(lamp.transform, false);
                post.transform.localPosition = new Vector3(0, 0.9f, 0);
                post.transform.localScale = new Vector3(0.1f, 0.9f, 0.1f);
                post.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(postColor);

                // Gold Middle Molding Ring
                var goldRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                goldRing.name = "GoldRing";
                goldRing.transform.SetParent(lamp.transform, false);
                goldRing.transform.localPosition = new Vector3(0, 1.25f, 0);
                goldRing.transform.localScale = new Vector3(0.15f, 0.03f, 0.15f);
                goldRing.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(goldAccent);
                Object.Destroy(goldRing.GetComponent<Collider>());

                // Dual Arched Arms
                for (int side = 0; side < 2; side++)
                {
                    float xDir = side == 0 ? -0.22f : 0.22f;

                    var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    arm.name = $"Arm_{side}";
                    arm.transform.SetParent(lamp.transform, false);
                    arm.transform.localPosition = new Vector3(xDir, 1.72f, 0);
                    arm.transform.localScale = new Vector3(0.35f, 0.05f, 0.06f);
                    arm.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(postColor);
                    Object.Destroy(arm.GetComponent<Collider>());

                    // Ornate Lantern Housing
                    var lantern = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    lantern.name = $"Lantern_{side}";
                    lantern.transform.SetParent(lamp.transform, false);
                    lantern.transform.localPosition = new Vector3(xDir * 1.35f, 1.58f, 0);
                    lantern.transform.localScale = new Vector3(0.22f, 0.15f, 0.22f);
                    lantern.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(postColor);
                    Object.Destroy(lantern.GetComponent<Collider>());

                    // Glowing Bulb Inside
                    var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    bulb.name = $"Bulb_{side}";
                    bulb.transform.SetParent(lamp.transform, false);
                    bulb.transform.localPosition = new Vector3(xDir * 1.35f, 1.55f, 0);
                    bulb.transform.localScale = Vector3.one * 0.24f;
                    bulb.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(lampGlow);
                    Object.Destroy(bulb.GetComponent<Collider>());
                }

                // Top Finial Spike
                var finial = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                finial.name = "FinialSpike";
                finial.transform.SetParent(lamp.transform, false);
                finial.transform.localPosition = new Vector3(0, 1.85f, 0);
                finial.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
                finial.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(goldAccent);
                Object.Destroy(finial.GetComponent<Collider>());

                lamp.transform.position = pos;
            }
        }


}
}
