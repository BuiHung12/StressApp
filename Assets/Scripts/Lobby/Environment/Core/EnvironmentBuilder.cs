using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Helper to build procedural 3D elements for the Waiting Lobby.
    /// </summary>
    public static class EnvironmentBuilder
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

        public static void CreateFountain3D()
        {
            var fountain = new GameObject("Fountain");

            // Outer Marble Plaza Rim
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "FountainPlaza";
            plaza.transform.SetParent(fountain.transform, false);
            plaza.transform.localPosition = new Vector3(0, 0.015f, 0);
            plaza.transform.localScale = new Vector3(5.2f, 0.02f, 5.2f);
            plaza.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.82f, 0.8f, 0.78f));
            Object.Destroy(plaza.GetComponent<Collider>());

            // Outer Basin Base Ring
            var baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseRing.name = "Base_Collider";
            baseRing.transform.SetParent(fountain.transform, false);
            baseRing.transform.localPosition = new Vector3(0, 0.28f, 0);
            baseRing.transform.localScale = new Vector3(4.0f, 0.28f, 4.0f);
            baseRing.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.32f, 0.35f, 0.42f));

            // Gold Trim Ring
            var goldTrim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goldTrim.name = "GoldTrim";
            goldTrim.transform.SetParent(fountain.transform, false);
            goldTrim.transform.localPosition = new Vector3(0, 0.43f, 0);
            goldTrim.transform.localScale = new Vector3(4.08f, 0.04f, 4.08f);
            goldTrim.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.92f, 0.76f, 0.28f));
            Object.Destroy(goldTrim.GetComponent<Collider>());

            // Main Crystal Pool Water
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "WaterPool";
            water.transform.SetParent(fountain.transform, false);
            water.transform.localPosition = new Vector3(0, 0.38f, 0);
            water.transform.localScale = new Vector3(3.7f, 0.06f, 3.7f);
            water.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.1f, 0.65f, 0.95f, 0.85f));
            Object.Destroy(water.GetComponent<Collider>());

            // Submerged LED Light Studs
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f * Mathf.Deg2Rad;
                var led = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                led.name = $"LED_{i}";
                led.transform.SetParent(fountain.transform, false);
                led.transform.localPosition = new Vector3(Mathf.Cos(ang) * 1.6f, 0.40f, Mathf.Sin(ang) * 1.6f);
                led.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
                led.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.95f, 1.0f));
                Object.Destroy(led.GetComponent<Collider>());
            }

            // Central Ornate Column
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(fountain.transform, false);
            pillar.transform.localPosition = new Vector3(0, 0.75f, 0);
            pillar.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            pillar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.78f, 0.76f, 0.74f));
            Object.Destroy(pillar.GetComponent<Collider>());

            // Middle Basin Bowl
            var midBowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            midBowl.name = "MiddleBowl";
            midBowl.transform.SetParent(fountain.transform, false);
            midBowl.transform.localPosition = new Vector3(0, 1.12f, 0);
            midBowl.transform.localScale = new Vector3(2.3f, 0.12f, 2.3f);
            midBowl.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.35f, 0.38f, 0.45f));
            Object.Destroy(midBowl.GetComponent<Collider>());

            var midWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            midWater.name = "MiddleWater";
            midWater.transform.SetParent(fountain.transform, false);
            midWater.transform.localPosition = new Vector3(0, 1.19f, 0);
            midWater.transform.localScale = new Vector3(2.15f, 0.04f, 2.15f);
            midWater.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.25f, 0.78f, 0.98f, 0.8f));
            Object.Destroy(midWater.GetComponent<Collider>());

            // Upper Column & Basin
            var topPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topPillar.name = "TopPillar";
            topPillar.transform.SetParent(fountain.transform, false);
            topPillar.transform.localPosition = new Vector3(0, 1.55f, 0);
            topPillar.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
            topPillar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.82f, 0.8f, 0.78f));
            Object.Destroy(topPillar.GetComponent<Collider>());

            var topBowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topBowl.name = "TopBowl";
            topBowl.transform.SetParent(fountain.transform, false);
            topBowl.transform.localPosition = new Vector3(0, 1.8f, 0);
            topBowl.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            topBowl.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.32f, 0.35f, 0.42f));
            Object.Destroy(topBowl.GetComponent<Collider>());

            // Glowing Crystal Orb Apex
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "GlowingOrbApex";
            orb.transform.SetParent(fountain.transform, false);
            orb.transform.localPosition = new Vector3(0, 2.05f, 0);
            orb.transform.localScale = Vector3.one * 0.45f;
            orb.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.4f, 0.92f, 1.0f));
            Object.Destroy(orb.GetComponent<Collider>());

            // Cascading Water Jets & Spray Droplets
            Vector3[] jets = {
                new(0, 2.3f, 0),
                new(0.35f, 2.15f, 0.2f), new(-0.35f, 2.15f, -0.2f),
                new(-0.2f, 2.18f, 0.35f), new(0.2f, 2.18f, -0.35f),
                new(0.7f, 1.5f, 0.4f), new(-0.7f, 1.5f, -0.4f),
                new(-0.4f, 1.5f, 0.7f), new(0.4f, 1.5f, -0.7f)
            };
            Color waterColor = new Color(0.6f, 0.92f, 1.0f, 0.9f);
            for (int i = 0; i < jets.Length; i++)
            {
                var jet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                jet.name = $"WaterJet_{i}";
                jet.transform.SetParent(fountain.transform, false);
                jet.transform.localPosition = jets[i];
                jet.transform.localScale = Vector3.one * (i == 0 ? 0.28f : 0.18f);
                jet.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(waterColor);
                Object.Destroy(jet.GetComponent<Collider>());
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

        public static void CreateFences(float lobbySize)
        {
            Color fenceColor = new Color(0.96f, 0.94f, 0.90f);
            float half = lobbySize * 0.45f;
            float step = 1.6f;

            for (float x = -half; x < half; x += step)
            {
                CreateFenceSegment(new Vector3(x, 0, half), new Vector3(x + step, 0, half), fenceColor);
                CreateFenceSegment(new Vector3(x, 0, -half), new Vector3(x + step, 0, -half), fenceColor);
            }
            for (float z = -half; z < half; z += step)
            {
                CreateFenceSegment(new Vector3(half, 0, z), new Vector3(half, 0, z + step), fenceColor, true);
                CreateFenceSegment(new Vector3(-half, 0, z), new Vector3(-half, 0, z + step), fenceColor, true);
            }
        }

        public static GameObject CreateFenceSegment(Vector3 start, Vector3 end, Color color, bool rotate90 = false)
        {
            var segment = new GameObject("FenceSegment");
            segment.transform.position = start;

            float length = Vector3.Distance(start, end);

            var fenceCollider = segment.AddComponent<BoxCollider>();
            fenceCollider.center = new Vector3(length * 0.5f, 0.5f, 0);
            fenceCollider.size = new Vector3(length, 1.0f, 0.2f);

            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(segment.transform, false);
            post.transform.localPosition = new Vector3(0, 0.5f, 0);
            post.transform.localScale = new Vector3(0.12f, 1.0f, 0.12f);
            post.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
            Object.Destroy(post.GetComponent<Collider>());

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "PostCap";
            cap.transform.SetParent(segment.transform, false);
            cap.transform.localPosition = new Vector3(0, 1.04f, 0);
            cap.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
            cap.transform.localRotation = Quaternion.Euler(45, 45, 0);
            cap.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color * 0.9f);
            Object.Destroy(cap.GetComponent<Collider>());

            for (int i = 0; i < 2; i++)
            {
                float y = i == 0 ? 0.3f : 0.7f;
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = $"Rail_{i}";
                rail.transform.SetParent(segment.transform, false);
                rail.transform.localPosition = new Vector3(length * 0.5f, y, 0);
                rail.transform.localScale = new Vector3(length, 0.06f, 0.05f);
                rail.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color * 0.95f);
                Object.Destroy(rail.GetComponent<Collider>());
            }

            int picketsCount = 3;
            for (int i = 1; i <= picketsCount; i++)
            {
                float t = (float)i / (picketsCount + 1);
                float x = length * t;

                var picket = new GameObject($"Picket_{i}");
                picket.transform.SetParent(segment.transform, false);
                picket.transform.localPosition = new Vector3(x, 0, 0);

                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(picket.transform, false);
                body.transform.localPosition = new Vector3(0, 0.45f, 0);
                body.transform.localScale = new Vector3(0.08f, 0.9f, 0.03f);
                body.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
                Object.Destroy(body.GetComponent<Collider>());

                var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tip.name = "Tip";
                tip.transform.SetParent(picket.transform, false);
                tip.transform.localPosition = new Vector3(0, 0.93f, 0);
                tip.transform.localScale = new Vector3(0.08f, 0.08f, 0.03f);
                tip.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                tip.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
                Object.Destroy(tip.GetComponent<Collider>());
            }

            if (rotate90)
            {
                segment.transform.rotation = Quaternion.Euler(0, 90f, 0);
            }

            return segment;
        }

        public static void CreateBuildings()
        {
            CreateBuilding("CafeShop", new Vector3(9, 0, 8),
                new Color(0.95f, 0.88f, 0.75f),
                new Color(0.82f, 0.28f, 0.22f),
                new Vector3(3.6f, 2.8f, 2.8f),
                "Bakery & Cafe", new Color(0.9f, 0.25f, 0.2f));

            CreateBuilding("ToolShop", new Vector3(-9, 0, 8),
                new Color(0.75f, 0.78f, 0.82f),
                new Color(0.22f, 0.48f, 0.75f),
                new Vector3(3.6f, 2.8f, 2.8f),
                "Gear & Tools", new Color(0.2f, 0.55f, 0.92f));

            CreateBuilding("MiloShop", new Vector3(-5, 0, -8),
                new Color(0.94f, 0.85f, 0.68f),
                new Color(0.22f, 0.65f, 0.28f),
                new Vector3(3.6f, 2.8f, 2.8f),
                "Fresh Market", new Color(0.2f, 0.75f, 0.35f));

            CreateBuilding("SweetShop", new Vector3(5, 0, -8),
                new Color(0.98f, 0.85f, 0.9f),
                new Color(0.92f, 0.48f, 0.68f),
                new Vector3(3.6f, 2.8f, 2.8f),
                "Sweet Shop", new Color(1.0f, 0.45f, 0.75f));
        }

        public static void CreateBuilding(string name, Vector3 pos, Color wallColor, Color roofColor, Vector3 size, string signText, Color stripeColor)
        {
            var building = new GameObject(name);

            // Foundation Plinth
            var plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plinth.name = "Plinth";
            plinth.transform.SetParent(building.transform, false);
            plinth.transform.localPosition = new Vector3(0, 0.1f, 0);
            plinth.transform.localScale = new Vector3(size.x + 0.3f, 0.2f, size.z + 0.3f);
            plinth.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.45f, 0.42f, 0.4f));
            Object.Destroy(plinth.GetComponent<Collider>());

            // Main Walls
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "Walls_Collider";
            walls.transform.SetParent(building.transform, false);
            walls.transform.localPosition = new Vector3(0, size.y * 0.5f + 0.1f, 0);
            walls.transform.localScale = size;
            walls.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(wallColor);

            // 2-Tiered Roof with Overhangs
            float roofThickness = 0.18f;
            float roofWidth = size.x * 0.62f;
            float roofLen = size.z + 0.5f;

            var roofLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofLeft.name = "RoofLeft";
            roofLeft.transform.SetParent(building.transform, false);
            roofLeft.transform.localPosition = new Vector3(-size.x * 0.26f, size.y + 0.42f, 0);
            roofLeft.transform.localScale = new Vector3(roofWidth, roofThickness, roofLen);
            roofLeft.transform.localRotation = Quaternion.Euler(0, 0, -25f);
            roofLeft.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(roofColor);
            Object.Destroy(roofLeft.GetComponent<Collider>());

            var roofRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofRight.name = "RoofRight";
            roofRight.transform.SetParent(building.transform, false);
            roofRight.transform.localPosition = new Vector3(size.x * 0.26f, size.y + 0.42f, 0);
            roofRight.transform.localScale = new Vector3(roofWidth, roofThickness, roofLen);
            roofRight.transform.localRotation = Quaternion.Euler(0, 0, 25f);
            roofRight.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(roofColor);
            Object.Destroy(roofRight.GetComponent<Collider>());

            // Corner Wooden Beams
            Color woodDark = new Color(0.32f, 0.2f, 0.12f);
            Vector3[] cornerOffsets = {
                new(-size.x * 0.5f, size.y * 0.5f + 0.1f, -size.z * 0.5f),
                new(size.x * 0.5f, size.y * 0.5f + 0.1f, -size.z * 0.5f),
                new(-size.x * 0.5f, size.y * 0.5f + 0.1f, size.z * 0.5f),
                new(size.x * 0.5f, size.y * 0.5f + 0.1f, size.z * 0.5f)
            };
            for (int i = 0; i < cornerOffsets.Length; i++)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = $"CornerPillar_{i}";
                pillar.transform.SetParent(building.transform, false);
                pillar.transform.localPosition = cornerOffsets[i];
                pillar.transform.localScale = new Vector3(0.18f, size.y, 0.18f);
                pillar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
                Object.Destroy(pillar.GetComponent<Collider>());
            }

            // Ornate Wooden Door
            var doorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorFrame.name = "DoorFrame";
            doorFrame.transform.SetParent(building.transform, false);
            doorFrame.transform.localPosition = new Vector3(0, 0.6f, -size.z * 0.5f - 0.02f);
            doorFrame.transform.localScale = new Vector3(0.72f, 1.2f, 0.04f);
            doorFrame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
            Object.Destroy(doorFrame.GetComponent<Collider>());

            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "DoorPanel";
            door.transform.SetParent(building.transform, false);
            door.transform.localPosition = new Vector3(0, 0.58f, -size.z * 0.5f - 0.03f);
            door.transform.localScale = new Vector3(0.62f, 1.12f, 0.03f);
            door.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.48f, 0.32f, 0.18f));
            Object.Destroy(door.GetComponent<Collider>());

            // Brass Door Handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "DoorHandle";
            handle.transform.SetParent(building.transform, false);
            handle.transform.localPosition = new Vector3(0.22f, 0.58f, -size.z * 0.5f - 0.05f);
            handle.transform.localScale = Vector3.one * 0.08f;
            handle.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.95f, 0.8f, 0.25f));
            Object.Destroy(handle.GetComponent<Collider>());

            // Double Windows with Flower Boxes
            for (int w = 0; w < 2; w++)
            {
                float xOff = w == 0 ? -size.x * 0.28f : size.x * 0.28f;

                var winFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winFrame.name = $"WindowFrame_{w}";
                winFrame.transform.SetParent(building.transform, false);
                winFrame.transform.localPosition = new Vector3(xOff, 1.45f, -size.z * 0.5f - 0.02f);
                winFrame.transform.localScale = new Vector3(0.65f, 0.65f, 0.04f);
                winFrame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
                Object.Destroy(winFrame.GetComponent<Collider>());

                var winGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winGlass.name = $"WindowGlass_{w}";
                winGlass.transform.SetParent(winFrame.transform, false);
                winGlass.transform.localPosition = new Vector3(0, 0, -0.1f);
                winGlass.transform.localScale = new Vector3(0.85f, 0.85f, 1.1f);
                winGlass.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.92f, 0.6f));
                Object.Destroy(winGlass.GetComponent<Collider>());

                // Flower Box under window
                var flowerBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flowerBox.name = $"FlowerBox_{w}";
                flowerBox.transform.SetParent(building.transform, false);
                flowerBox.transform.localPosition = new Vector3(xOff, 1.05f, -size.z * 0.5f - 0.08f);
                flowerBox.transform.localScale = new Vector3(0.75f, 0.16f, 0.22f);
                flowerBox.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
                Object.Destroy(flowerBox.GetComponent<Collider>());

                Color[] fCols = { new Color(1f, 0.2f, 0.4f), new Color(1f, 0.8f, 0.1f), new Color(0.3f, 0.85f, 1f) };
                for (int f = 0; f < 3; f++)
                {
                    var fl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    fl.name = $"Flower_{f}";
                    fl.transform.SetParent(flowerBox.transform, false);
                    fl.transform.localPosition = new Vector3(-0.25f + f * 0.25f, 0.55f, 0);
                    fl.transform.localScale = Vector3.one * 0.75f;
                    fl.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(fCols[f % fCols.Length]);
                    Object.Destroy(fl.GetComponent<Collider>());
                }
            }

            // Striped Canopy Awning over Entrance
            var awning = new GameObject("Awning");
            awning.transform.SetParent(building.transform, false);
            awning.transform.localPosition = new Vector3(0, size.y - 0.1f, -size.z * 0.5f - 0.45f);
            awning.transform.localRotation = Quaternion.Euler(-18f, 0, 0);

            float stripeWidth = (size.x + 0.4f) / 6f;
            for (int i = 0; i < 6; i++)
            {
                var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = $"Stripe_{i}";
                stripe.transform.SetParent(awning.transform, false);
                stripe.transform.localPosition = new Vector3(-size.x * 0.5f - 0.1f + (i + 0.5f) * stripeWidth, 0, 0);
                stripe.transform.localScale = new Vector3(stripeWidth, 0.06f, 0.9f);
                Color c = (i % 2 == 0) ? new Color(0.96f, 0.94f, 0.9f) : stripeColor;
                stripe.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(c);
                Object.Destroy(stripe.GetComponent<Collider>());
            }

            // Chimney with Puffy Smoke
            var chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chimney.name = "Chimney";
            chimney.transform.SetParent(building.transform, false);
            chimney.transform.localPosition = new Vector3(size.x * 0.35f, size.y + 0.9f, -size.z * 0.2f);
            chimney.transform.localScale = new Vector3(0.3f, 0.7f, 0.3f);
            chimney.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.38f, 0.35f, 0.32f));
            Object.Destroy(chimney.GetComponent<Collider>());

            for (int i = 0; i < 3; i++)
            {
                var smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                smoke.name = $"Smoke_{i}";
                smoke.transform.SetParent(building.transform, false);
                smoke.transform.localPosition = new Vector3(size.x * 0.35f + i * 0.08f, size.y + 1.4f + i * 0.18f, -size.z * 0.2f);
                smoke.transform.localScale = Vector3.one * (0.22f + i * 0.1f);
                smoke.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.92f, 0.95f, 0.7f));
                Object.Destroy(smoke.GetComponent<Collider>());
            }

            // Shop Signboard
            var signObj = new GameObject("Sign");
            signObj.transform.SetParent(building.transform, false);
            signObj.transform.localPosition = new Vector3(0, size.y + 1.0f, -size.z * 0.5f - 0.2f);
            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = $"<b>{signText}</b>";
            signTmp.fontSize = 4.2f;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = Color.white;

            var signBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBg.name = "SignBg";
            signBg.transform.SetParent(signObj.transform, false);
            signBg.transform.localPosition = new Vector3(0, 0, 0.02f);
            signBg.transform.localScale = new Vector3(2.2f, 0.7f, 0.06f);
            signBg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.18f, 0.15f, 0.12f));
            Object.Destroy(signBg.GetComponent<Collider>());

            var signBorder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBorder.name = "SignBorder";
            signBorder.transform.SetParent(signObj.transform, false);
            signBorder.transform.localPosition = new Vector3(0, 0, 0.01f);
            signBorder.transform.localScale = new Vector3(2.28f, 0.78f, 0.04f);
            signBorder.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.92f, 0.76f, 0.28f));
            Object.Destroy(signBorder.GetComponent<Collider>());
            signObj.AddComponent<BillboardText>();

            building.transform.position = pos;
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

        public static void CreatePortals(float lobbySize)
        {
            float pathEnd = lobbySize * 0.8f * 0.5f - 0.5f;

            CreatePortal("GardenPortal", new Vector3(0, 0, pathEnd), 0f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.15f, 0.85f, 0.3f, 0.7f),
                "Vườn Trên Cao");

            CreatePortal("PrisonPortal", new Vector3(0, 0, -pathEnd), 0f,
                new Color(0.25f, 0.25f, 0.3f),
                new Color(0.85f, 0.15f, 0.15f, 0.7f),
                "Nhà Tù");

            CreatePortal("FishingPortal", new Vector3(pathEnd, 0, 0), 90f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.2f, 0.5f, 0.95f, 0.7f),
                "Khu Câu Cá");

            CreatePortal("StudyPortal", new Vector3(-pathEnd, 0, 0), 90f,
                new Color(0.4f, 0.32f, 0.22f),
                new Color(0.95f, 0.75f, 0.15f, 0.7f),
                "Khu Học Tập");
        }

        public static void CreatePortal(string name, Vector3 pos, float rotY, Color ringColor, Color energyColor, string label)
        {
            var portal = new GameObject(name);
            portal.transform.position = pos;
            portal.transform.rotation = Quaternion.Euler(0, rotY, 0);

            float padRadius = 0.7f;

            var basePad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePad.name = "TeleportPad";
            basePad.transform.SetParent(portal.transform, false);
            basePad.transform.localPosition = new Vector3(0, 0.02f, 0);
            basePad.transform.localScale = new Vector3(padRadius * 2, 0.02f, padRadius * 2);
            basePad.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.12f, 0.14f, 0.18f));
            Object.DestroyImmediate(basePad.GetComponent<Collider>());

            var neonBorder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neonBorder.name = "NeonBorder";
            neonBorder.transform.SetParent(portal.transform, false);
            neonBorder.transform.localPosition = new Vector3(0, 0.03f, 0);
            neonBorder.transform.localScale = new Vector3(padRadius * 1.85f, 0.022f, padRadius * 1.85f);
            neonBorder.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(energyColor);
            Object.Destroy(neonBorder.GetComponent<Collider>());

            var innerPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerPlate.name = "InnerPlate";
            innerPlate.transform.SetParent(portal.transform, false);
            innerPlate.transform.localPosition = new Vector3(0, 0.04f, 0);
            innerPlate.transform.localScale = new Vector3(padRadius * 1.5f, 0.024f, padRadius * 1.5f);
            innerPlate.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.08f, 0.09f, 0.11f));
            Object.Destroy(innerPlate.GetComponent<Collider>());

            var coreGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coreGlow.name = "CoreGlow";
            coreGlow.transform.SetParent(portal.transform, false);
            coreGlow.transform.localPosition = new Vector3(0, 0.05f, 0);
            coreGlow.transform.localScale = new Vector3(padRadius * 1.1f, 0.026f, padRadius * 1.1f);
            Color intenseEnergy = new Color(energyColor.r * 1.2f, energyColor.g * 1.2f, energyColor.b * 1.2f, 0.9f);
            coreGlow.GetComponent<Renderer>().material = CharacterVisuals.CreateAdditiveMat(intenseEnergy);
            Object.Destroy(coreGlow.GetComponent<Collider>());

            var holoBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holoBeam.name = "HoloBeam";
            holoBeam.transform.SetParent(portal.transform, false);
            holoBeam.transform.localPosition = new Vector3(0, 0.6f, 0);
            holoBeam.transform.localScale = new Vector3(padRadius * 1.1f, 0.6f, padRadius * 1.1f);
            Color beamColor = new Color(energyColor.r, energyColor.g, energyColor.b, 0.22f);
            holoBeam.GetComponent<Renderer>().material = CharacterVisuals.CreateAdditiveMat(beamColor);
            Object.Destroy(holoBeam.GetComponent<Collider>());

            float dist = padRadius * 0.8f;
            Vector3[] beaconOffsets = {
                new(dist, 0.05f, dist),
                new(-dist, 0.05f, dist),
                new(dist, 0.05f, -dist),
                new(-dist, 0.05f, -dist)
            };

            foreach (var offset in beaconOffsets)
            {
                var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beacon.name = "BeaconStud";
                beacon.transform.SetParent(portal.transform, false);
                beacon.transform.localPosition = offset;
                beacon.transform.localScale = new Vector3(0.08f, 0.05f, 0.08f);
                beacon.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.25f, 0.28f, 0.32f));
                Object.Destroy(beacon.GetComponent<Collider>());

                var laserLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                laserLine.name = "LaserLine";
                laserLine.transform.SetParent(portal.transform, false);
                laserLine.transform.localPosition = offset + new Vector3(0, 0.5f, 0);
                laserLine.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);
                Color laserColor = new Color(energyColor.r * 1.5f, energyColor.g * 1.5f, energyColor.b * 1.5f, 0.4f);
                laserLine.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(laserColor);
                Object.Destroy(laserLine.GetComponent<Collider>());
            }

            var signObj = new GameObject("PortalSign");
            signObj.transform.SetParent(portal.transform, false);
            signObj.transform.localPosition = new Vector3(0, 1.8f, 0);

            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = label;
            signTmp.fontSize = 3.2f;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = Color.white;
            signObj.AddComponent<BillboardText>();

            var signBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBg.name = "SignBg";
            signBg.transform.SetParent(signObj.transform, false);
            signBg.transform.localPosition = new Vector3(0, 0, 0.01f);
            signBg.transform.localScale = new Vector3(1.6f, 0.45f, 0.005f);
            Color glassColor = new Color(energyColor.r * 0.2f, energyColor.g * 0.2f, energyColor.b * 0.2f, 0.45f);
            signBg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(glassColor);
            Object.Destroy(signBg.GetComponent<Collider>());

            var neonFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonFrame.name = "SignNeonFrame";
            neonFrame.transform.SetParent(signObj.transform, false);
            neonFrame.transform.localPosition = new Vector3(0, 0, 0.005f);
            neonFrame.transform.localScale = new Vector3(1.62f, 0.47f, 0.002f);
            neonFrame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(energyColor * 0.8f);
            Object.Destroy(neonFrame.GetComponent<Collider>());
        }

        public static GameObject CreateFlat(string name, Vector3 pos, Vector2 size, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            obj.transform.localScale = new Vector3(size.x, size.y, 1f);
            obj.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
            obj.isStatic = true;
            Object.Destroy(obj.GetComponent<Collider>());
            return obj;
        }

        public static GameObject CreateCircle(string name, Vector3 pos, float radius, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(radius * 2, 0.001f, radius * 2);
            obj.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
            obj.isStatic = true;
            Object.Destroy(obj.GetComponent<Collider>());
            return obj;
        }
    }
}
