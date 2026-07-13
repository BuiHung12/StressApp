using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Tự động tạo toàn bộ sảnh chờ 2D top-down khi scene load.
    /// Không cần NavMesh — dùng simple transform movement.
    ///
    /// CÁCH DÙNG: Gắn script này vào 1 empty GameObject rồi nhấn Play.
    /// </summary>
    public class LobbySetup : MonoBehaviour
    {
        [Header("Lobby Settings")]
        [SerializeField] private float _lobbySize = 30f;

        [Header("Character Scale")]
        [SerializeField] private float _characterScale = 3.0f;

        [Header("Colors")]
        [SerializeField] private Color _grassColor = new Color(0.45f, 0.78f, 0.22f);
        [SerializeField] private Color _pathColor = new Color(0.92f, 0.87f, 0.78f);

        private void Awake()
        {
            // Xóa light mặc định (camera sẽ được reuse)
            var defaultLight = FindAnyObjectByType<Light>();
            if (defaultLight != null) Destroy(defaultLight.gameObject);

            CreateEnvironment();
            CreateLighting();
            var player = CreatePlayer();
            CreateNPCs();
            CreateFakePlayers();
            CreateCamera(player.transform);
            CreateUI();

            Debug.Log("🎮 Ranger City Lobby (2D Top-Down) loaded!");
            Debug.Log("📋 WASD to move, Space to punch, Click to move");
        }

        // ── Environment (flat 2D top-down) ──

        private void CreateEnvironment()
        {
            // Grass ground
            var ground = CreateFlat("Ground", Vector3.zero, new Vector2(_lobbySize, _lobbySize), _grassColor);

            // Paths (cross shape, raised slightly)
            float pathLen = _lobbySize * 0.8f;
            CreateFlat("Path_H", new Vector3(0, 0.01f, 0), new Vector2(pathLen, 2f), _pathColor);
            CreateFlat("Path_V", new Vector3(0, 0.01f, 0), new Vector2(2f, pathLen), _pathColor);

            // Detailed curb/borders flanking the pathways
            Color curbColor = new Color(0.35f, 0.35f, 0.35f);
            CreateFlat("Curb_H_Top", new Vector3(0, 0.012f, 1.05f), new Vector2(pathLen, 0.1f), curbColor);
            CreateFlat("Curb_H_Bot", new Vector3(0, 0.012f, -1.05f), new Vector2(pathLen, 0.1f), curbColor);
            CreateFlat("Curb_V_Left", new Vector3(-1.05f, 0.012f, 0), new Vector2(0.1f, pathLen), curbColor);
            CreateFlat("Curb_V_Right", new Vector3(1.05f, 0.012f, 0), new Vector2(0.1f, pathLen), curbColor);

            // Central plaza (stone circle)
            CreateCircle("Plaza", new Vector3(0, 0.02f, 0), 4f, new Color(0.78f, 0.74f, 0.72f));

            // 3D Trees (trunk + canopy sphere)
            CreateTrees3D();

            // Flower patches
            CreateFlowers();

            // Fountain center (3D)
            CreateFountain3D();

            // Benches (3D)
            CreateBenches3D();

            // Fences
            CreateFences();

            // Small buildings/stalls
            CreateBuildings();

            // Street Lamps (3D)
            CreateStreetLamps();

            // Zone Gates at the 4 path endpoints
            CreateGates();
        }

        private void CreateTrees3D()
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
                trunk.GetComponent<Renderer>().material = CreateMat(trunkColor);
                // Keep trunk collider for blockages!

                // --- Branches ---
                // Left branch
                var leftBranch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leftBranch.name = "LeftBranch";
                leftBranch.transform.SetParent(tree.transform, false);
                leftBranch.transform.localPosition = new Vector3(-0.25f * scale, 1.3f * scale, 0);
                leftBranch.transform.localScale = new Vector3(0.10f * scale, 0.35f * scale, 0.10f * scale);
                leftBranch.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                leftBranch.GetComponent<Renderer>().material = CreateMat(trunkColor);
                Destroy(leftBranch.GetComponent<Collider>());

                // Right branch
                var rightBranch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rightBranch.name = "RightBranch";
                rightBranch.transform.SetParent(tree.transform, false);
                rightBranch.transform.localPosition = new Vector3(0.2f * scale, 1.5f * scale, 0.1f * scale);
                rightBranch.transform.localScale = new Vector3(0.08f * scale, 0.3f * scale, 0.08f * scale);
                rightBranch.transform.localRotation = Quaternion.Euler(-30f, 0, -45f);
                rightBranch.GetComponent<Renderer>().material = CreateMat(trunkColor);
                Destroy(rightBranch.GetComponent<Collider>());

                // --- Canopy Layers (Overlapping Spheres) ---
                // 1. Center / Main Canopy
                var canopyMain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyMain.name = "CanopyMain";
                canopyMain.transform.SetParent(tree.transform, false);
                canopyMain.transform.localPosition = new Vector3(0, 2.3f * scale, 0);
                canopyMain.transform.localScale = new Vector3(1.8f * scale, 1.6f * scale, 1.8f * scale);
                canopyMain.GetComponent<Renderer>().material = CreateMat(baseGreen);
                Destroy(canopyMain.GetComponent<Collider>());

                // 2. Left Puff
                var canopyLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyLeft.name = "CanopyLeft";
                canopyLeft.transform.SetParent(tree.transform, false);
                canopyLeft.transform.localPosition = new Vector3(-0.75f * scale, 2.0f * scale, -0.1f * scale);
                canopyLeft.transform.localScale = new Vector3(1.2f * scale, 1.1f * scale, 1.2f * scale);
                canopyLeft.GetComponent<Renderer>().material = CreateMat(darkGreen);
                Destroy(canopyLeft.GetComponent<Collider>());

                // 3. Right Puff
                var canopyRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyRight.name = "CanopyRight";
                canopyRight.transform.SetParent(tree.transform, false);
                canopyRight.transform.localPosition = new Vector3(0.7f * scale, 2.2f * scale, 0.2f * scale);
                canopyRight.transform.localScale = new Vector3(1.1f * scale, 1.0f * scale, 1.1f * scale);
                canopyRight.GetComponent<Renderer>().material = CreateMat(darkGreen);
                Destroy(canopyRight.GetComponent<Collider>());

                // 4. Top Puff
                var canopyTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopyTop.name = "CanopyTop";
                canopyTop.transform.SetParent(tree.transform, false);
                canopyTop.transform.localPosition = new Vector3(0.05f * scale, 2.9f * scale, -0.05f * scale);
                canopyTop.transform.localScale = new Vector3(1.3f * scale, 1.1f * scale, 1.3f * scale);
                canopyTop.GetComponent<Renderer>().material = CreateMat(lightGreen);
                Destroy(canopyTop.GetComponent<Collider>());

                // --- Apples / Fruits ---
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
                    apple.GetComponent<Renderer>().material = CreateMat(appleColor);
                    Destroy(apple.GetComponent<Collider>());
                }

                // --- Shadow ---
                var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shadow.name = "Shadow";
                shadow.transform.SetParent(tree.transform, false);
                shadow.transform.localPosition = new Vector3(0.2f * scale, 0.005f, -0.2f * scale);
                shadow.transform.localScale = new Vector3(2.2f * scale, 0.005f, 2.2f * scale);
                shadow.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.35f, 0.1f, 0.35f)); // Soft dark green transparent shadow
                Destroy(shadow.GetComponent<Collider>());

                tree.transform.position = pos;
            }
        }

        private void CreateFlowers()
        {
            Color[] colors = {
                new(0.91f, 0.12f, 0.39f),
                new(1f, 0.6f, 0f),
                new(0.61f, 0.15f, 0.69f),
                new(0.13f, 0.59f, 0.95f),
                new(0.99f, 0.84f, 0.21f),
            };

            Vector3[] patches = {
                new(-6, 0, 3), new(6, 0, 3), new(-6, 0, -3), new(6, 0, -3),
                new(-3, 0, 7), new(3, 0, 7), new(-3, 0, -7), new(3, 0, -7),
            };

            foreach (var patchPos in patches)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 offset = new(Random.Range(-0.6f, 0.6f), 0, Random.Range(-0.6f, 0.6f));
                    Color c = colors[Random.Range(0, colors.Length)];

                    // Flower stem
                    var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stem.name = "FlowerStem";
                    stem.transform.position = patchPos + offset + new Vector3(0, 0.15f, 0);
                    stem.transform.localScale = new Vector3(0.03f, 0.15f, 0.03f);
                    stem.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 0.55f, 0.2f));
                    Destroy(stem.GetComponent<Collider>());

                    // Flower head
                    var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    head.name = "FlowerHead";
                    head.transform.position = patchPos + offset + new Vector3(0, 0.35f, 0);
                    head.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
                    head.GetComponent<Renderer>().material = CreateMat(c);
                    Destroy(head.GetComponent<Collider>());
                }
            }
        }

        private void CreateFountain3D()
        {
            var fountain = new GameObject("Fountain");

            // 1. Plaza under fountain
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "FountainPlaza";
            plaza.transform.SetParent(fountain.transform, false);
            plaza.transform.localPosition = new Vector3(0, 0.01f, 0);
            plaza.transform.localScale = new Vector3(4.5f, 0.02f, 4.5f);
            plaza.GetComponent<Renderer>().material = CreateMat(new Color(0.6f, 0.58f, 0.56f));
            Destroy(plaza.GetComponent<Collider>());

            // 2. Base ring (stone outer wall) - KEEP COLLIDER
            var baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseRing.name = "Base_Collider";
            baseRing.transform.SetParent(fountain.transform, false);
            baseRing.transform.localPosition = new Vector3(0, 0.25f, 0);
            baseRing.transform.localScale = new Vector3(3.4f, 0.25f, 3.4f);
            baseRing.GetComponent<Renderer>().material = CreateMat(new Color(0.68f, 0.65f, 0.62f));
            // Keep collider!

            // 3. Water pool inside basin
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "WaterPool";
            water.transform.SetParent(fountain.transform, false);
            water.transform.localPosition = new Vector3(0, 0.35f, 0);
            water.transform.localScale = new Vector3(3.1f, 0.05f, 3.1f);
            water.GetComponent<Renderer>().material = CreateMat(new Color(0.18f, 0.64f, 0.85f, 0.8f));
            Destroy(water.GetComponent<Collider>());

            // 4. Center pillar
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(fountain.transform, false);
            pillar.transform.localPosition = new Vector3(0, 0.7f, 0);
            pillar.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
            pillar.GetComponent<Renderer>().material = CreateMat(new Color(0.72f, 0.69f, 0.66f));
            Destroy(pillar.GetComponent<Collider>());

            // 5. Middle bowl
            var midBowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            midBowl.name = "MiddleBowl";
            midBowl.transform.SetParent(fountain.transform, false);
            midBowl.transform.localPosition = new Vector3(0, 1.0f, 0);
            midBowl.transform.localScale = new Vector3(1.8f, 0.25f, 1.8f);
            midBowl.GetComponent<Renderer>().material = CreateMat(new Color(0.68f, 0.65f, 0.62f));
            Destroy(midBowl.GetComponent<Collider>());

            // 6. Middle water layer (overflow cascade)
            var midWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            midWater.name = "MiddleWater";
            midWater.transform.SetParent(fountain.transform, false);
            midWater.transform.localPosition = new Vector3(0, 0.85f, 0);
            midWater.transform.localScale = new Vector3(1.7f, 0.15f, 1.7f);
            midWater.GetComponent<Renderer>().material = CreateMat(new Color(0.35f, 0.78f, 0.94f, 0.7f));
            Destroy(midWater.GetComponent<Collider>());

            // 7. Top pillar
            var topPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topPillar.name = "TopPillar";
            topPillar.transform.SetParent(fountain.transform, false);
            topPillar.transform.localPosition = new Vector3(0, 1.3f, 0);
            topPillar.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            topPillar.GetComponent<Renderer>().material = CreateMat(new Color(0.75f, 0.72f, 0.69f));
            Destroy(topPillar.GetComponent<Collider>());

            // 8. Top bowl
            var topBowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            topBowl.name = "TopBowl";
            topBowl.transform.SetParent(fountain.transform, false);
            topBowl.transform.localPosition = new Vector3(0, 1.5f, 0);
            topBowl.transform.localScale = new Vector3(0.9f, 0.2f, 0.9f);
            topBowl.GetComponent<Renderer>().material = CreateMat(new Color(0.7f, 0.67f, 0.64f));
            Destroy(topBowl.GetComponent<Collider>());

            // 9. Splashing water jets (small spheres)
            Vector3[] jets = {
                new(0, 1.7f, 0),
                new(0.2f, 1.65f, 0.1f),
                new(-0.2f, 1.65f, -0.1f),
                new(-0.1f, 1.68f, 0.2f),
                new(0.1f, 1.68f, -0.2f)
            };
            Color waterColor = new Color(0.5f, 0.85f, 1.0f, 0.9f);
            for (int i = 0; i < jets.Length; i++)
            {
                var jet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                jet.name = $"WaterJet_{i}";
                jet.transform.SetParent(fountain.transform, false);
                jet.transform.localPosition = jets[i];
                jet.transform.localScale = Vector3.one * 0.15f;
                jet.GetComponent<Renderer>().material = CreateMat(waterColor);
                Destroy(jet.GetComponent<Collider>());
            }
        }

        private void CreateBenches3D()
        {
            Vector3[] positions = { new(-4, 0, 5), new(4, 0, 5), new(-4, 0, -5), new(4, 0, -5) };
            foreach (var pos in positions)
            {
                var bench = new GameObject("Bench");
                Color wood = new Color(0.55f, 0.36f, 0.25f);

                // Seat (KEEP COLLIDER)
                var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seat.name = "Seat_Collider";
                seat.transform.SetParent(bench.transform, false);
                seat.transform.localPosition = new Vector3(0, 0.35f, 0);
                seat.transform.localScale = new Vector3(1.2f, 0.08f, 0.4f);
                seat.GetComponent<Renderer>().material = CreateMat(wood);
                // Keep collider!

                // Back
                var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                back.name = "Back";
                back.transform.SetParent(bench.transform, false);
                back.transform.localPosition = new Vector3(0, 0.55f, -0.18f);
                back.transform.localScale = new Vector3(1.2f, 0.4f, 0.06f);
                back.GetComponent<Renderer>().material = CreateMat(wood);
                Destroy(back.GetComponent<Collider>());

                // Legs
                for (int i = 0; i < 2; i++)
                {
                    float x = i == 0 ? -0.5f : 0.5f;
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = $"Leg_{i}";
                    leg.transform.SetParent(bench.transform, false);
                    leg.transform.localPosition = new Vector3(x, 0.17f, 0);
                    leg.transform.localScale = new Vector3(0.06f, 0.34f, 0.35f);
                    leg.GetComponent<Renderer>().material = CreateMat(new Color(0.3f, 0.3f, 0.3f));
                    Destroy(leg.GetComponent<Collider>());
                }

                bench.transform.position = pos;
            }
        }

        private void CreateFences()
        {
            Color fenceColor = new Color(0.96f, 0.94f, 0.90f); // Warm creamy white
            float half = _lobbySize * 0.45f;
            float step = 1.6f;
            float gateGap = 2.0f; // skip fence segments within this range of center (gate opening)

            // Fences along the 4 edges (skip segments at gate openings)
            // Horizontal fences (North z=half, South z=-half) — gate at x≈0
            for (float x = -half; x < half; x += step)
            {
                float mid = x + step * 0.5f;
                if (Mathf.Abs(mid) < gateGap) continue; // skip for gate opening
                CreateFenceSegment(new Vector3(x, 0, half), new Vector3(x + step, 0, half), fenceColor);
                CreateFenceSegment(new Vector3(x, 0, -half), new Vector3(x + step, 0, -half), fenceColor);
            }
            // Vertical fences (East x=half, West x=-half) — gate at z≈0
            for (float z = -half; z < half; z += step)
            {
                float mid = z + step * 0.5f;
                if (Mathf.Abs(mid) < gateGap) continue; // skip for gate opening
                CreateFenceSegment(new Vector3(half, 0, z), new Vector3(half, 0, z + step), fenceColor, rotate90: true);
                CreateFenceSegment(new Vector3(-half, 0, z), new Vector3(-half, 0, z + step), fenceColor, rotate90: true);
            }
        }

        private void CreateFenceSegment(Vector3 start, Vector3 end, Color color, bool rotate90 = false)
        {
            var segment = new GameObject("FenceSegment");
            segment.transform.position = start;

            float length = Vector3.Distance(start, end);

            // Add block collider to the entire fence segment (so characters cannot pass through!)
            var fenceCollider = segment.AddComponent<BoxCollider>();
            fenceCollider.center = new Vector3(length * 0.5f, 0.5f, 0);
            fenceCollider.size = new Vector3(length, 1.0f, 0.2f);

            // Main posts at start and end
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(segment.transform, false);
            post.transform.localPosition = new Vector3(0, 0.5f, 0);
            post.transform.localScale = new Vector3(0.12f, 1.0f, 0.12f);
            post.GetComponent<Renderer>().material = CreateMat(color);
            Destroy(post.GetComponent<Collider>());

            // Post cap (small diamond on top of the post)
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "PostCap";
            cap.transform.SetParent(segment.transform, false);
            cap.transform.localPosition = new Vector3(0, 1.04f, 0);
            cap.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
            cap.transform.localRotation = Quaternion.Euler(45, 45, 0);
            cap.GetComponent<Renderer>().material = CreateMat(color * 0.9f);
            Destroy(cap.GetComponent<Collider>());

            // Top and Bottom horizontal rails
            for (int i = 0; i < 2; i++)
            {
                float y = i == 0 ? 0.3f : 0.7f;
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = $"Rail_{i}";
                rail.transform.SetParent(segment.transform, false);
                rail.transform.localPosition = new Vector3(length * 0.5f, y, 0);
                rail.transform.localScale = new Vector3(length, 0.06f, 0.05f);
                rail.GetComponent<Renderer>().material = CreateMat(color * 0.95f);
                Destroy(rail.GetComponent<Collider>());
            }

            // Vertical pickets (3 pickets between main posts)
            int picketsCount = 3;
            for (int i = 1; i <= picketsCount; i++)
            {
                float t = (float)i / (picketsCount + 1);
                float x = length * t;

                var picket = new GameObject($"Picket_{i}");
                picket.transform.SetParent(segment.transform, false);
                picket.transform.localPosition = new Vector3(x, 0, 0);

                // Picket body
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(picket.transform, false);
                body.transform.localPosition = new Vector3(0, 0.45f, 0);
                body.transform.localScale = new Vector3(0.08f, 0.9f, 0.03f);
                body.GetComponent<Renderer>().material = CreateMat(color);
                Destroy(body.GetComponent<Collider>());

                // Pointy top (rotated cube to make it look like a fence picket tip)
                var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tip.name = "Tip";
                tip.transform.SetParent(picket.transform, false);
                tip.transform.localPosition = new Vector3(0, 0.93f, 0);
                tip.transform.localScale = new Vector3(0.08f, 0.08f, 0.03f);
                tip.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                tip.GetComponent<Renderer>().material = CreateMat(color);
                Destroy(tip.GetComponent<Collider>());
            }

            if (rotate90)
            {
                segment.transform.rotation = Quaternion.Euler(0, 90f, 0);
            }
        }

        private void CreateBuildings()
        {
            // 1. Cafe / Coffee Shop (top-right area)
            CreateBuilding("CafeShop", new Vector3(9, 0, 8),
                new Color(0.9f, 0.76f, 0.58f), // warm wood wall
                new Color(0.76f, 0.22f, 0.18f), // terracotta roof
                new Vector3(3.2f, 2.5f, 2.5f),
                "☕ Cafe", new Color(1.0f, 0.2f, 0.2f)); // red stripes

            // 2. Tool / Blacksmith Shop (top-left area)
            CreateBuilding("ToolShop", new Vector3(-9, 0, 8),
                new Color(0.7f, 0.72f, 0.75f), // stone walls
                new Color(0.2f, 0.42f, 0.65f), // dark roof
                new Vector3(3.2f, 2.5f, 2.5f),
                "🛠️ Tools", new Color(0.2f, 0.5f, 0.9f)); // blue stripes

            // 3. Milo's general/groceries Shop (bottom-left area)
            CreateBuilding("MiloShop", new Vector3(-5, 0, -8),
                new Color(0.92f, 0.8f, 0.6f), // orange-wood wall
                new Color(0.25f, 0.6f, 0.22f), // green roof
                new Vector3(3.2f, 2.5f, 2.5f),
                "🍎 Market", new Color(0.2f, 0.7f, 0.3f)); // green stripes

            // 4. Bakery / Sweet Shop (bottom-right area)
            CreateBuilding("SweetShop", new Vector3(5, 0, -8),
                new Color(0.96f, 0.8f, 0.85f), // soft pink wall
                new Color(0.85f, 0.45f, 0.6f), // deep pink roof
                new Vector3(3.2f, 2.5f, 2.5f),
                "🍰 Sweets", new Color(1.0f, 0.5f, 0.75f)); // pink stripes
        }

        private void CreateBuilding(string name, Vector3 pos, Color wallColor, Color roofColor, Vector3 size, string signText, Color stripeColor)
        {
            var building = new GameObject(name);

            // Walls (KEEP COLLIDER)
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "Walls_Collider";
            walls.transform.SetParent(building.transform, false);
            walls.transform.localPosition = new Vector3(0, size.y * 0.5f, 0);
            walls.transform.localScale = size;
            walls.GetComponent<Renderer>().material = CreateMat(wallColor);
            // Keep collider!

            // Peaked roof (/ \ shape using two slanted panels)
            float roofThickness = 0.15f;
            float roofWidth = size.x * 0.6f;
            float roofLen = size.z + 0.4f;

            var roofLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofLeft.name = "RoofLeft";
            roofLeft.transform.SetParent(building.transform, false);
            roofLeft.transform.localPosition = new Vector3(-size.x * 0.26f, size.y + 0.35f, 0);
            roofLeft.transform.localScale = new Vector3(roofWidth, roofThickness, roofLen);
            roofLeft.transform.localRotation = Quaternion.Euler(0, 0, -22f); // angle up
            roofLeft.GetComponent<Renderer>().material = CreateMat(roofColor);
            Destroy(roofLeft.GetComponent<Collider>());

            var roofRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofRight.name = "RoofRight";
            roofRight.transform.SetParent(building.transform, false);
            roofRight.transform.localPosition = new Vector3(size.x * 0.26f, size.y + 0.35f, 0);
            roofRight.transform.localScale = new Vector3(roofWidth, roofThickness, roofLen);
            roofRight.transform.localRotation = Quaternion.Euler(0, 0, 22f); // angle up
            roofRight.GetComponent<Renderer>().material = CreateMat(roofColor);
            Destroy(roofRight.GetComponent<Collider>());

            // Timber-framed corner columns for classic cottage detail
            Color woodDark = new Color(0.35f, 0.22f, 0.14f);
            Vector3[] cornerOffsets = {
                new(-size.x * 0.5f, size.y * 0.5f, -size.z * 0.5f),
                new(size.x * 0.5f, size.y * 0.5f, -size.z * 0.5f),
                new(-size.x * 0.5f, size.y * 0.5f, size.z * 0.5f),
                new(size.x * 0.5f, size.y * 0.5f, size.z * 0.5f)
            };
            for (int i = 0; i < cornerOffsets.Length; i++)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = $"CornerPillar_{i}";
                pillar.transform.SetParent(building.transform, false);
                pillar.transform.localPosition = cornerOffsets[i];
                pillar.transform.localScale = new Vector3(0.16f, size.y, 0.16f);
                pillar.GetComponent<Renderer>().material = CreateMat(woodDark);
                Destroy(pillar.GetComponent<Collider>());
            }

            // Door (facing SOUTH, negative Z - facing the camera!)
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.SetParent(building.transform, false);
            door.transform.localPosition = new Vector3(0, 0.5f, -size.z * 0.5f - 0.01f);
            door.transform.localScale = new Vector3(0.6f, 1f, 0.02f);
            door.GetComponent<Renderer>().material = CreateMat(woodDark);
            Destroy(door.GetComponent<Collider>());

            // Front Window (facing SOUTH)
            var winFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            winFrame.name = "WindowFrame";
            winFrame.transform.SetParent(building.transform, false);
            winFrame.transform.localPosition = new Vector3(size.x * 0.25f, 1.2f, -size.z * 0.5f - 0.01f);
            winFrame.transform.localScale = new Vector3(0.5f, 0.5f, 0.03f);
            winFrame.GetComponent<Renderer>().material = CreateMat(woodDark);
            Destroy(winFrame.GetComponent<Collider>());

            var winGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            winGlass.name = "WindowGlass";
            winGlass.transform.SetParent(winFrame.transform, false);
            winGlass.transform.localPosition = new Vector3(0, 0, -0.2f);
            winGlass.transform.localScale = new Vector3(0.8f, 0.8f, 1.2f);
            winGlass.GetComponent<Renderer>().material = CreateMat(new Color(0.6f, 0.85f, 0.95f));
            Destroy(winGlass.GetComponent<Collider>());

            // Striped Canvas Awning in the front (tilted forward toward negative Z)
            var awning = new GameObject("Awning");
            awning.transform.SetParent(building.transform, false);
            awning.transform.localPosition = new Vector3(0, size.y - 0.2f, -size.z * 0.5f - 0.4f);
            awning.transform.localRotation = Quaternion.Euler(-15f, 0, 0); // tilted forward towards south

            // Draw stripes: 5 thin stripes horizontally next to each other
            float stripeWidth = (size.x + 0.2f) / 5f;
            for (int i = 0; i < 5; i++)
            {
                var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = $"Stripe_{i}";
                stripe.transform.SetParent(awning.transform, false);
                stripe.transform.localPosition = new Vector3(-size.x * 0.5f + (i + 0.5f) * stripeWidth, 0, 0);
                stripe.transform.localScale = new Vector3(stripeWidth, 0.05f, 0.8f);
                Color c = (i % 2 == 0) ? Color.white : stripeColor;
                stripe.GetComponent<Renderer>().material = CreateMat(c);
                Destroy(stripe.GetComponent<Collider>());
            }

            // Chimney pipe on the roof
            var chimney = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chimney.name = "Chimney";
            chimney.transform.SetParent(building.transform, false);
            chimney.transform.localPosition = new Vector3(size.x * 0.35f, size.y + 0.8f, -size.z * 0.2f);
            chimney.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            chimney.GetComponent<Renderer>().material = CreateMat(new Color(0.3f, 0.3f, 0.3f));
            Destroy(chimney.GetComponent<Collider>());

            // Tiny smoke puffs
            for (int i = 0; i < 3; i++)
            {
                var smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                smoke.name = $"Smoke_{i}";
                smoke.transform.SetParent(building.transform, false);
                smoke.transform.localPosition = new Vector3(size.x * 0.35f + i * 0.05f, size.y + 1.3f + i * 0.15f, -size.z * 0.2f);
                smoke.transform.localScale = Vector3.one * (0.15f + i * 0.08f);
                smoke.GetComponent<Renderer>().material = CreateMat(new Color(0.85f, 0.85f, 0.85f, 0.6f)); // semi-transparent gray
                Destroy(smoke.GetComponent<Collider>());
            }

            // Shop counter table (a wooden table in front, facing south)
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter_Collider";
            counter.transform.SetParent(building.transform, false);
            counter.transform.localPosition = new Vector3(-size.x * 0.25f, 0.35f, -size.z * 0.5f - 0.8f);
            counter.transform.localScale = new Vector3(1.2f, 0.7f, 0.4f);
            counter.GetComponent<Renderer>().material = CreateMat(new Color(0.55f, 0.36f, 0.24f));
            // Keep counter collider so players can't clip into the display table!

            // Add apples/oranges crates if it is Milo's shop (Market)
            if (name.Contains("Milo"))
            {
                // Red apples box
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = "ApplesBox";
                box.transform.SetParent(building.transform, false);
                box.transform.localPosition = new Vector3(-size.x * 0.25f, 0.75f, -size.z * 0.5f - 0.8f);
                box.transform.localScale = new Vector3(0.5f, 0.15f, 0.3f);
                box.GetComponent<Renderer>().material = CreateMat(new Color(0.4f, 0.25f, 0.15f));
                Destroy(box.GetComponent<Collider>());

                // Apple spheres inside
                for (int j = 0; j < 4; j++)
                {
                    var apple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    apple.name = "Apple";
                    apple.transform.SetParent(box.transform, false);
                    apple.transform.localPosition = new Vector3(-0.3f + j * 0.2f, 0.5f, Random.Range(-0.2f, 0.2f));
                    apple.transform.localScale = Vector3.one * 0.4f;
                    apple.GetComponent<Renderer>().material = CreateMat(Color.red);
                    Destroy(apple.GetComponent<Collider>());
                }
            }

            // Crate/Barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel_Collider";
            barrel.transform.SetParent(building.transform, false);
            barrel.transform.localPosition = new Vector3(size.x * 0.3f, 0.4f, -size.z * 0.5f - 0.8f);
            barrel.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
            barrel.GetComponent<Renderer>().material = CreateMat(new Color(0.48f, 0.33f, 0.2f));
            // Keep barrel collider!

            // Shop sign (emoji text above counter)
            var signObj = new GameObject("Sign");
            signObj.transform.SetParent(building.transform, false);
            signObj.transform.localPosition = new Vector3(-size.x * 0.25f, 1.8f, -size.z * 0.5f - 0.8f);
            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = signText;
            signTmp.fontSize = 4;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = Color.white;
            // Add background box for sign
            var signBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBg.name = "SignBg";
            signBg.transform.SetParent(signObj.transform, false);
            signBg.transform.localPosition = new Vector3(0, 0, -0.01f);
            signBg.transform.localScale = new Vector3(1.6f, 0.6f, 0.05f);
            signBg.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.12f, 0.1f));
            Destroy(signBg.GetComponent<Collider>());
            // Add billboard script to sign so it faces camera
            signObj.AddComponent<BillboardText>();

            // Shadow
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shadow.name = "Shadow";
            shadow.transform.SetParent(building.transform, false);
            shadow.transform.localPosition = new Vector3(0.2f, 0.005f, -0.2f);
            shadow.transform.localScale = new Vector3(size.x + 0.2f, 0.01f, size.z + 0.2f);
            shadow.GetComponent<Renderer>().material = CreateMat(new Color(0, 0, 0, 0.15f));
            Destroy(shadow.GetComponent<Collider>());

            building.transform.position = pos;
        }

        private void CreateStreetLamps()
        {
            Vector3[] positions = {
                new(-2.5f, 0, 2.5f), new(2.5f, 0, 2.5f),
                new(-2.5f, 0, -2.5f), new(2.5f, 0, -2.5f),
                new(-8.5f, 0, 1.5f), new(8.5f, 0, 1.5f),
                new(-1.5f, 0, 8.5f), new(1.5f, 0, -8.5f)
            };
            Color postColor = new Color(0.2f, 0.22f, 0.25f);
            Color lampColor = new Color(1.0f, 0.95f, 0.4f);

            foreach (var pos in positions)
            {
                var lamp = new GameObject("StreetLamp");

                // Post (KEEP COLLIDER)
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Post_Collider";
                post.transform.SetParent(lamp.transform, false);
                post.transform.localPosition = new Vector3(0, 0.8f, 0);
                post.transform.localScale = new Vector3(0.08f, 0.8f, 0.08f);
                post.GetComponent<Renderer>().material = CreateMat(postColor);
                // Keep collider!

                // Arm
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arm.name = "Arm";
                arm.transform.SetParent(lamp.transform, false);
                // Extend arm toward the paths slightly
                float armOffset = pos.x < 0 ? 0.2f : -0.2f;
                if (Mathf.Abs(pos.z) > Mathf.Abs(pos.x)) armOffset = pos.z < 0 ? 0.2f : -0.2f;
                arm.transform.localPosition = new Vector3(Mathf.Abs(pos.x) > Mathf.Abs(pos.z) ? armOffset : 0, 1.5f, Mathf.Abs(pos.z) > Mathf.Abs(pos.x) ? armOffset : 0);
                arm.transform.localScale = new Vector3(0.4f, 0.06f, 0.08f);
                arm.GetComponent<Renderer>().material = CreateMat(postColor);
                Destroy(arm.GetComponent<Collider>());

                // Bulb/Glass lantern
                var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bulb.name = "Bulb";
                bulb.transform.SetParent(lamp.transform, false);
                bulb.transform.localPosition = arm.transform.localPosition + new Vector3(0, -0.15f, 0);
                bulb.transform.localScale = Vector3.one * 0.22f;
                bulb.GetComponent<Renderer>().material = CreateMat(lampColor);
                Destroy(bulb.GetComponent<Collider>());

                lamp.transform.position = pos;
            }
        }

        // ── Zone Gates ──

        private void CreateGates()
        {
            float half = _lobbySize * 0.45f;

            // North gate → Garden (green)
            CreateGate("GardenGate", new Vector3(0, 0, half), 0f,
                new Color(0.2f, 0.6f, 0.25f), // green pillars
                new Color(0.35f, 0.75f, 0.3f), // green accent
                "🌿 Vườn Trên Cao");

            // South gate → Prison (dark gray)
            CreateGate("PrisonGate", new Vector3(0, 0, -half), 180f,
                new Color(0.3f, 0.3f, 0.35f), // dark stone
                new Color(0.5f, 0.15f, 0.15f), // red accent
                "🔒 Nhà Tù");

            // East gate → Fishing (blue)
            CreateGate("FishingGate", new Vector3(half, 0, 0), 90f,
                new Color(0.25f, 0.45f, 0.7f), // blue stone
                new Color(0.3f, 0.7f, 0.9f), // cyan accent
                "🎣 Khu Câu Cá");

            // West gate → Study (warm brown)
            CreateGate("StudyGate", new Vector3(-half, 0, 0), -90f,
                new Color(0.55f, 0.38f, 0.22f), // warm wood
                new Color(0.85f, 0.65f, 0.3f), // gold accent
                "📚 Khu Học Tập");
        }

        private void CreateGate(string name, Vector3 pos, float rotY, Color pillarColor, Color accentColor, string label)
        {
            var gate = new GameObject(name);
            gate.transform.position = pos;
            gate.transform.rotation = Quaternion.Euler(0, rotY, 0);

            float pillarSpacing = 1.3f; // half-width of opening
            float pillarHeight = 3.5f;

            // Left pillar (KEEP COLLIDER)
            var leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftPillar.name = "Pillar_Collider";
            leftPillar.transform.SetParent(gate.transform, false);
            leftPillar.transform.localPosition = new Vector3(-pillarSpacing, pillarHeight * 0.5f, 0);
            leftPillar.transform.localScale = new Vector3(0.35f, pillarHeight * 0.5f, 0.35f);
            leftPillar.GetComponent<Renderer>().material = CreateMat(pillarColor);
            // Keep collider!

            // Right pillar (KEEP COLLIDER)
            var rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightPillar.name = "Pillar_Collider";
            rightPillar.transform.SetParent(gate.transform, false);
            rightPillar.transform.localPosition = new Vector3(pillarSpacing, pillarHeight * 0.5f, 0);
            rightPillar.transform.localScale = new Vector3(0.35f, pillarHeight * 0.5f, 0.35f);
            rightPillar.GetComponent<Renderer>().material = CreateMat(pillarColor);
            // Keep collider!

            // Pillar caps (decorative spheres on top)
            for (int i = 0; i < 2; i++)
            {
                float px = i == 0 ? -pillarSpacing : pillarSpacing;
                var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cap.name = "PillarCap";
                cap.transform.SetParent(gate.transform, false);
                cap.transform.localPosition = new Vector3(px, pillarHeight + 0.15f, 0);
                cap.transform.localScale = Vector3.one * 0.4f;
                cap.GetComponent<Renderer>().material = CreateMat(accentColor);
                Destroy(cap.GetComponent<Collider>());
            }

            // Crossbeam (top horizontal bar connecting pillars)
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Crossbeam";
            beam.transform.SetParent(gate.transform, false);
            beam.transform.localPosition = new Vector3(0, pillarHeight - 0.1f, 0);
            beam.transform.localScale = new Vector3(pillarSpacing * 2 + 0.35f, 0.3f, 0.3f);
            beam.GetComponent<Renderer>().material = CreateMat(pillarColor * 0.9f);
            Destroy(beam.GetComponent<Collider>());

            // Arch detail (smaller decorative bar below crossbeam)
            var archDetail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archDetail.name = "ArchDetail";
            archDetail.transform.SetParent(gate.transform, false);
            archDetail.transform.localPosition = new Vector3(0, pillarHeight - 0.45f, 0);
            archDetail.transform.localScale = new Vector3(pillarSpacing * 2 - 0.2f, 0.12f, 0.15f);
            archDetail.GetComponent<Renderer>().material = CreateMat(accentColor);
            Destroy(archDetail.GetComponent<Collider>());

            // Lanterns hanging from crossbeam (2 lanterns)
            for (int i = 0; i < 2; i++)
            {
                float lx = i == 0 ? -0.7f : 0.7f;

                // Lantern chain
                var chain = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chain.name = "Chain";
                chain.transform.SetParent(gate.transform, false);
                chain.transform.localPosition = new Vector3(lx, pillarHeight - 0.55f, 0);
                chain.transform.localScale = new Vector3(0.03f, 0.3f, 0.03f);
                chain.GetComponent<Renderer>().material = CreateMat(new Color(0.25f, 0.25f, 0.25f));
                Destroy(chain.GetComponent<Collider>());

                // Lantern body (cube)
                var lanternBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lanternBody.name = "LanternBody";
                lanternBody.transform.SetParent(gate.transform, false);
                lanternBody.transform.localPosition = new Vector3(lx, pillarHeight - 0.85f, 0);
                lanternBody.transform.localScale = new Vector3(0.2f, 0.25f, 0.2f);
                lanternBody.GetComponent<Renderer>().material = CreateMat(accentColor * 0.8f);
                Destroy(lanternBody.GetComponent<Collider>());

                // Lantern glow (sphere inside)
                var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                glow.name = "LanternGlow";
                glow.transform.SetParent(gate.transform, false);
                glow.transform.localPosition = new Vector3(lx, pillarHeight - 0.85f, 0);
                glow.transform.localScale = Vector3.one * 0.18f;
                glow.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.95f, 0.5f, 0.9f));
                Destroy(glow.GetComponent<Collider>());
            }

            // Sign board (TextMeshPro billboard above crossbeam)
            var signObj = new GameObject("GateSign");
            signObj.transform.SetParent(gate.transform, false);
            signObj.transform.localPosition = new Vector3(0, pillarHeight + 0.6f, 0);
            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = label;
            signTmp.fontSize = 5;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = Color.white;
            signObj.AddComponent<BillboardText>();

            // Sign background plank
            var signBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBg.name = "SignBg";
            signBg.transform.SetParent(signObj.transform, false);
            signBg.transform.localPosition = new Vector3(0, 0, 0.02f);
            signBg.transform.localScale = new Vector3(2.8f, 0.7f, 0.06f);
            signBg.GetComponent<Renderer>().material = CreateMat(pillarColor * 0.6f);
            Destroy(signBg.GetComponent<Collider>());
        }

        // ── Lighting ──

        private void CreateLighting()
        {
            // Simple ambient light - Unlit shader doesn't need directional light
            // but we keep one for any Lit objects
            var sunObj = new GameObject("Sun");
            var sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.98f, 0.92f);
            sun.intensity = 1.0f;
            sun.shadows = LightShadows.None;
            sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.9f, 0.92f, 0.95f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.fog = false;
        }

        // ── Player ──

        private GameObject CreatePlayer()
        {
            var player = CreateCharacterTopDown("Player", new Color(0.26f, 0.65f, 0.96f),
                new Color(1f, 0.88f, 0.7f));
            player.transform.position = new Vector3(0, 0, -3);
            player.AddComponent<PlayerController>();
            player.tag = "Player";
            return player;
        }

        // ── NPCs ──

        private void CreateNPCs()
        {
            CreateNPC("Chief Rosa", "👩‍✈️", new Vector3(-5, 0, 4),
                new Color(0.81f, 0.58f, 0.86f),
                new[] {
                    "Chào mừng Junior Ranger! 🌟",
                    "Tôi là Chief Rosa, người hướng dẫn.",
                    "Hãy khám phá sảnh chờ nhé! 💪",
                });

            CreateNPC("Sr. Stoplight", "🚦", new Vector3(5, 0, 4),
                new Color(0.4f, 0.73f, 0.42f),
                new[] {
                    "Xin chào! Tôi là Señor Stoplight! 🚦",
                    "Đèn đỏ = DỪNG, đèn xanh = ĐI! 🔴🟢",
                    "Kiên nhẫn là chìa khóa! ⏰",
                });

            CreateNPC("Milo", "🧑‍🍳", new Vector3(-3, 0, -6),
                new Color(1f, 0.72f, 0.3f),
                new[] {
                    "Chào! Tôi là Milo, chủ cửa hàng! 🛒",
                    "Quay lại khi có Ranger Coins nhé! 💰",
                });
        }

        private void CreateNPC(string name, string emoji, Vector3 pos,
            Color bodyColor, string[] dialogues)
        {
            var npc = CreateCharacterTopDown(name, bodyColor, new Color(1f, 0.88f, 0.7f));
            npc.transform.position = pos;

            var ctrl = npc.AddComponent<NPCController>();
            SetField(ctrl, "_displayName", name);
            SetField(ctrl, "_avatarEmoji", emoji);
            SetField(ctrl, "_dialogueLines", dialogues);
            SetField(ctrl, "_moveSpeed", 0.3f);        // NPC di chuyển rất chậm (strolling)
            SetField(ctrl, "_wanderPauseMin", 5f);      // Dừng lại nghỉ lâu hơn
            SetField(ctrl, "_wanderPauseMax", 10f);
            SetField(ctrl, "_wanderRadius", 3f);

            AddSwollenFaceEffect(npc);
            AddNameTag(npc, name);
        }

        // ── Fake Players ──

        private void CreateFakePlayers()
        {
            var data = new (string name, Vector3 pos, Color color, string[] greetings)[] {
                ("Luna", new(4, 0, -2), new(0.94f, 0.33f, 0.31f),
                    new[] { "Chào! Mình là Luna! 🌙", "Sảnh này vui lắm! 🎉" }),
                ("Kai", new(-4, 0, 0), new(0.4f, 0.73f, 0.42f),
                    new[] { "Yo! 🏃", "Đừng đấm mình nha! 😅" }),
                ("Sakura", new(7, 0, -5), new(0.49f, 0.34f, 0.76f),
                    new[] { "Konnichiwa! 🌸", "Mình thích sảnh này! 🎨" }),
                ("Tí", new(-7, 0, -5), new(1f, 0.54f, 0.4f),
                    new[] { "Ê bạn! 👋", "Tìm được bí mật chưa? 🕵️" }),
                ("Mochi", new(0, 0, 8), new(1f, 0.65f, 0.15f),
                    new[] { "Zzz... mình đang nghỉ! 😴", "Ồ xin lỗi! 😊" }),
                ("Rex", new(8, 0, 2), new(0.36f, 0.42f, 0.75f),
                    new[] { "Hey! Bạn cũng Ranger hả? 💪", "Đã xong 50 nhiệm vụ! 🏆" }),
            };

            foreach (var (fpName, pos, color, greetings) in data)
            {
                var fp = CreateCharacterTopDown(fpName, color, new Color(1f, 0.88f, 0.7f));
                fp.transform.position = pos;

                var ctrl = fp.AddComponent<FakePlayerController>();
                SetField(ctrl, "_displayName", fpName);
                SetField(ctrl, "_greetings", greetings);
                SetField(ctrl, "_moveSpeed", 0.35f);       // Fake player đi chậm dạo mát
                SetField(ctrl, "_pauseMin", 5f);           // Dừng lâu hơn
                SetField(ctrl, "_pauseMax", 10f);
                SetField(ctrl, "_wanderRadius", 4f);

                AddSwollenFaceEffect(fp);
                AddNameTag(fp, fpName);
            }
        }

        // ── Camera (top-down orthographic) ──

        private void CreateCamera(Transform player)
        {
            // Reuse existing Main Camera if available
            GameObject camObj;
            Camera cam;

            if (Camera.main != null)
            {
                camObj = Camera.main.gameObject;
                cam = Camera.main;
                // Remove old AudioListener if duplicated
            }
            else
            {
                camObj = new GameObject("LobbyCamera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                if (camObj.GetComponent<AudioListener>() == null)
                    camObj.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.4f, 0.7f, 0.25f);
            cam.orthographic = true;
            cam.orthographicSize = 3.2f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // Isometric angle like Hay Day (~35 degrees)
            float angle = 35f;
            float distance = 20f;
            float radAngle = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(0, distance * Mathf.Sin(radAngle), -distance * Mathf.Cos(radAngle));
            camObj.transform.position = player.position + offset;
            camObj.transform.rotation = Quaternion.Euler(angle, 0f, 0f);

            // Add lobby camera follow script
            var lobbyCamera = camObj.GetComponent<LobbyCamera>();
            if (lobbyCamera == null)
                lobbyCamera = camObj.AddComponent<LobbyCamera>();
            lobbyCamera.SetTarget(player);
        }

        // ── UI ──

        private void CreateUI()
        {
            // Canvas
            var canvasObj = new GameObject("LobbyCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Interaction prompt
            var interactionPanel = CreateInteractionPanel(canvasObj.transform);
            var dialoguePanel = CreateDialoguePanel(canvasObj.transform);

            // Wire LobbyUI
            var ui = canvasObj.AddComponent<LobbyUI>();
            SetField(ui, "_interactionPanel", interactionPanel);
            SetField(ui, "_dialoguePanel", dialoguePanel);
            SetField(ui, "_talkButton", interactionPanel.transform.Find("TalkButton").GetComponent<Button>());
            SetField(ui, "_punchButton", interactionPanel.transform.Find("PunchButton").GetComponent<Button>());
            SetField(ui, "_targetNameText", interactionPanel.transform.Find("TargetName").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueNameText", dialoguePanel.transform.Find("NameText").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueContentText", dialoguePanel.transform.Find("ContentText").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueHintText", dialoguePanel.transform.Find("HintText").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueNextButton", dialoguePanel.transform.Find("NextButton").GetComponent<Button>());
        }

        private GameObject CreateInteractionPanel(Transform parent)
        {
            // Root panel (transparent container)
            var panel = new GameObject("InteractionPanel");
            panel.transform.SetParent(parent, false);
            var panelRT = panel.AddComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(54, 54);

            // Punch button (transparent background to let only the fist icon be visible and clickable)
            var punchBtnObj = CreateUIButton("PunchButton", panel.transform, Vector2.zero,
                "", Color.clear, new Vector2(54, 54));
            
            // Procedural fist icon inside the button
            var fistIcon = new GameObject("FistIcon");
            fistIcon.transform.SetParent(punchBtnObj.transform, false);
            var fistRT = fistIcon.AddComponent<RectTransform>();
            fistRT.sizeDelta = new Vector2(50, 50);
            var fistImg = fistIcon.AddComponent<Image>();
            fistImg.sprite = CreateFistSprite();
            fistImg.color = Color.white;

            // Dummy components to satisfy LobbyUI.cs references
            var talkBtnObj = CreateUIButton("TalkButton", panel.transform, new Vector2(9999f, 9999f), 
                "DummyTalk", Color.clear, Vector2.one);
            talkBtnObj.SetActive(false);

            var nameObj = new GameObject("TargetName");
            nameObj.transform.SetParent(panel.transform, false);
            nameObj.transform.localPosition = new Vector3(9999f, 9999f, 0);
            var nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "";

            panel.SetActive(false);
            return panel;
        }

        private Sprite CreateFistSprite()
        {
            string customPath = System.IO.Path.Combine(Application.dataPath, "Textures/fist.png");
            if (System.IO.File.Exists(customPath))
            {
                try
                {
                    byte[] data = System.IO.File.ReadAllBytes(customPath);
                    Texture2D customTex = new Texture2D(2, 2);
                    if (customTex.LoadImage(data))
                    {
                        Color[] pixels = customTex.GetPixels();
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            if (pixels[i].r > 0.99f && pixels[i].g > 0.99f && pixels[i].b > 0.99f)
                            {
                                pixels[i] = new Color(0, 0, 0, 0);
                            }
                        }
                        customTex.SetPixels(pixels);
                        customTex.Apply();

                        customTex.filterMode = FilterMode.Bilinear;
                        return Sprite.Create(
                            customTex,
                            new Rect(0, 0, customTex.width, customTex.height),
                            new Vector2(0.5f, 0.5f),
                            customTex.width
                        );
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to load custom fist image: {ex.Message}");
                }
            }

            const int w = 32;
            const int h = 32;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color clear = new Color(0, 0, 0, 0);
            Color outline = new Color(0.55f, 0.22f, 0.00f);
            Color fill = new Color(1.00f, 0.78f, 0.12f);
            Color highlight = new Color(1.00f, 0.92f, 0.35f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            string[] mask =
            {
                "................................", // 0
                "................................", // 1
                "................................", // 2
                "................................", // 3
                // Knuckle tips outline
                "......OOO...OO...OO...OO...OOO..", // 4
                // Knuckle sides and fills
                ".....OFFFO.OFFO.OFFO.OFFO.OFFFO.", // 5
                // Knuckle creases starting to merge
                "....OFFFFOFFFFOFFFFOFFFFOFFFFO..", // 6
                "...OFFFFFFOFFFFOFFFFOFFFFFFFO...", // 7
                // Palm main body
                "...OFFFFFFFFFFFFFFFFFFFFFFFFO...", // 8
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 9
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 10
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 11
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 12
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 13
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 14
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 15
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 16
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..", // 17
                "...OFFFFFFFFFFFFFFFFFFFFFFFFO...", // 18
                "...OFFFFFFFFFFFFFFFFFFFFFFFFO...", // 19
                "....OFFFFFFFFFFFFFFFFFFFFFFO....", // 20
                ".....OFFFFFFFFFFFFFFFFFFFFO.....", // 21
                "......OFFFFFFFFFFFFFFFFFFO......", // 22
                ".......OFFFFFFFFFFFFFFFFO.......", // 23
                ".........OFFFFFFFFFFFFO.........", // 24
                "...........OFFFFFFFFO...........", // 25
                ".............OOOOOO.............", // 26
                "................................", // 27
                "................................", // 28
                "................................", // 29
                "................................", // 30
                "................................"  // 31
            };

            int startX = 0;
            int startY = 0;

            for (int row = 0; row < mask.Length; row++)
            {
                string line = mask[row];

                for (int col = 0; col < line.Length; col++)
                {
                    int px = startX + col;
                    int py = startY + row;

                    if (px < 0 || px >= w || py < 0 || py >= h)
                        continue;

                    char c = line[col];

                    switch (c)
                    {
                        case 'O':
                            tex.SetPixel(px, py, outline);
                            break;

                        case 'F':
                        {
                            float t = (float)row / mask.Length;

                            tex.SetPixel(
                                px,
                                py,
                                Color.Lerp(fill, highlight, t)
                            );
                            break;
                        }
                    }
                }
            }

            tex.Apply();

            return Sprite.Create(
                tex,
                new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f),
                32f
            );
        }

        private void DrawLine(
            Texture2D tex,
            int x1,
            int y1,
            int x2,
            int y2,
            Color color)
        {
            int dx = Mathf.Abs(x2 - x1);
            int dy = Mathf.Abs(y2 - y1);

            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;

            int err = dx - dy;

            while (true)
            {
                tex.SetPixel(x1, y1, color);

                if (x1 == x2 && y1 == y2)
                    break;

                int e2 = err * 2;

                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        private GameObject CreateDialoguePanel(Transform parent)
        {
            var panel = new GameObject("DialoguePanel");
            panel.transform.SetParent(parent, false);

            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.02f);
            rt.anchorMax = new Vector2(0.9f, 0.22f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.16f, 0.92f);

            CreateTMPText("NameText", panel.transform, Vector2.zero, "NPC", 20,
                TextAlignmentOptions.TopLeft, new Color(0.39f, 0.71f, 1f),
                new Vector2(0.05f, 0.7f), new Vector2(0.95f, 0.95f));

            CreateTMPText("ContentText", panel.transform, Vector2.zero, "", 15,
                TextAlignmentOptions.TopLeft, Color.white,
                new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.7f));

            CreateTMPText("HintText", panel.transform, Vector2.zero, "", 12,
                TextAlignmentOptions.BottomRight, new Color(0.62f, 0.66f, 0.78f),
                new Vector2(0.5f, 0.02f), new Vector2(0.95f, 0.2f));

            // Invisible clickable area
            var nextBtn = new GameObject("NextButton");
            nextBtn.transform.SetParent(panel.transform, false);
            var nrt = nextBtn.AddComponent<RectTransform>();
            nrt.anchorMin = Vector2.zero; nrt.anchorMax = Vector2.one;
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            nextBtn.AddComponent<Button>();
            nextBtn.AddComponent<Image>().color = Color.clear;

            panel.SetActive(false);
            return panel;
        }

        // ── Helpers ──

        private GameObject CreateCharacterTopDown(string charName, Color bodyColor, Color skinColor)
        {
            var character = new GameObject(charName);

            // === TORSO (capsule - main body) ===
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(character.transform);
            torso.transform.localPosition = new Vector3(0, 0.7f, 0);
            torso.transform.localScale = new Vector3(0.4f, 0.45f, 0.25f);
            torso.GetComponent<Renderer>().material = CreateMat(bodyColor);
            Destroy(torso.GetComponent<Collider>());

            // === HEAD (sphere) ===
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(character.transform);
            head.transform.localPosition = new Vector3(0, 1.35f, 0);
            head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            head.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(head.GetComponent<Collider>());

            // === HAIR (half sphere on top) ===
            var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(character.transform);
            hair.transform.localPosition = new Vector3(0, 1.48f, -0.02f);
            hair.transform.localScale = new Vector3(0.42f, 0.25f, 0.42f);
            hair.GetComponent<Renderer>().material = CreateMat(bodyColor * 0.5f);
            Destroy(hair.GetComponent<Collider>());

            // === LEFT ARM ===
            var leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(character.transform);
            leftArm.transform.localPosition = new Vector3(-0.28f, 0.72f, 0);
            leftArm.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            leftArm.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(leftArm.GetComponent<Collider>());

            // === RIGHT ARM ===
            var rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(character.transform);
            rightArm.transform.localPosition = new Vector3(0.28f, 0.72f, 0);
            rightArm.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            rightArm.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(rightArm.GetComponent<Collider>());

            // === LEFT LEG ===
            var leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(character.transform);
            leftLeg.transform.localPosition = new Vector3(-0.1f, 0.22f, 0);
            leftLeg.transform.localScale = new Vector3(0.14f, 0.25f, 0.14f);
            leftLeg.GetComponent<Renderer>().material = CreateMat(new Color(0.25f, 0.35f, 0.55f)); // Pants
            Destroy(leftLeg.GetComponent<Collider>());

            // === RIGHT LEG ===
            var rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(character.transform);
            rightLeg.transform.localPosition = new Vector3(0.1f, 0.22f, 0);
            rightLeg.transform.localScale = new Vector3(0.14f, 0.25f, 0.14f);
            rightLeg.GetComponent<Renderer>().material = CreateMat(new Color(0.25f, 0.35f, 0.55f)); // Pants
            Destroy(rightLeg.GetComponent<Collider>());

            // === SHOES ===
            var leftShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftShoe.name = "LeftShoe";
            leftShoe.transform.SetParent(character.transform);
            leftShoe.transform.localPosition = new Vector3(-0.1f, 0.04f, 0.04f);
            leftShoe.transform.localScale = new Vector3(0.13f, 0.08f, 0.2f);
            leftShoe.GetComponent<Renderer>().material = CreateMat(new Color(0.35f, 0.2f, 0.15f));
            Destroy(leftShoe.GetComponent<Collider>());

            var rightShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightShoe.name = "RightShoe";
            rightShoe.transform.SetParent(character.transform);
            rightShoe.transform.localPosition = new Vector3(0.1f, 0.04f, 0.04f);
            rightShoe.transform.localScale = new Vector3(0.13f, 0.08f, 0.2f);
            rightShoe.GetComponent<Renderer>().material = CreateMat(new Color(0.35f, 0.2f, 0.15f));
            Destroy(rightShoe.GetComponent<Collider>());

            // === SHADOW (flat circle on ground) ===
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "Shadow";
            shadow.transform.SetParent(character.transform);
            shadow.transform.localPosition = new Vector3(0.05f, 0.005f, -0.05f);
            shadow.transform.localScale = new Vector3(0.5f, 0.005f, 0.5f);
            shadow.GetComponent<Renderer>().material = CreateMat(new Color(0, 0, 0, 0.2f));
            Destroy(shadow.GetComponent<Collider>());

            character.transform.localScale = Vector3.one * _characterScale;
            character.AddComponent<CharacterAnimator>();
            return character;
        }

        private void AddSwollenFaceEffect(GameObject character)
        {
            // === Swollen Face (big red cheeks + cross eyes) ===
            var swollen = new GameObject("SwollenFace");
            swollen.transform.SetParent(character.transform, false);
            swollen.transform.localPosition = new Vector3(0, 1.2f, 0.15f);

            // Left cheek (big, red, puffy)
            var leftCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftCheek.name = "LeftCheek";
            leftCheek.transform.SetParent(swollen.transform, false);
            leftCheek.transform.localPosition = new Vector3(-0.18f, 0, 0.08f);
            leftCheek.transform.localScale = new Vector3(0.25f, 0.2f, 0.15f);
            leftCheek.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.2f, 0.2f));
            Destroy(leftCheek.GetComponent<Collider>());

            // Right cheek (bigger! asymmetric = funnier)
            var rightCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightCheek.name = "RightCheek";
            rightCheek.transform.SetParent(swollen.transform, false);
            rightCheek.transform.localPosition = new Vector3(0.2f, -0.03f, 0.1f);
            rightCheek.transform.localScale = new Vector3(0.3f, 0.25f, 0.18f);
            rightCheek.GetComponent<Renderer>().material = CreateMat(new Color(0.95f, 0.15f, 0.15f));
            Destroy(rightCheek.GetComponent<Collider>());

            // Bump on head
            var bump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bump.name = "HeadBump";
            bump.transform.SetParent(swollen.transform, false);
            bump.transform.localPosition = new Vector3(0.05f, 0.25f, 0);
            bump.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
            bump.GetComponent<Renderer>().material = CreateMat(new Color(0.9f, 0.4f, 0.5f));
            Destroy(bump.GetComponent<Collider>());

            // === Stars spinning around head ===
            var stars = new GameObject("PunchStars");
            stars.transform.SetParent(character.transform, false);
            stars.transform.localPosition = new Vector3(0, 1.7f, 0);

            Color[] starColors = {
                new(1f, 0.84f, 0f),     // Gold
                new(1f, 0.4f, 0.4f),    // Red
                new(0.3f, 0.85f, 1f),   // Cyan
                new(0.4f, 1f, 0.4f),    // Green
                new(1f, 0.65f, 0f),     // Orange
            };

            for (int i = 0; i < 5; i++)
            {
                var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.name = $"Star_{i}";
                star.transform.SetParent(stars.transform, false);
                star.transform.localScale = Vector3.one * 0.1f;
                star.GetComponent<Renderer>().material = CreateMat(starColors[i % starColors.Length]);
                Destroy(star.GetComponent<Collider>());
            }
            stars.AddComponent<SwollenFaceEffect>();

            // Wire references
            var npc = character.GetComponent<NPCController>();
            if (npc != null)
            {
                SetField(npc, "_swollenFaceEffect", swollen);
                SetField(npc, "_punchStarsEffect", stars);
            }
            var fp = character.GetComponent<FakePlayerController>();
            if (fp != null)
            {
                SetField(fp, "_swollenFaceEffect", swollen);
                SetField(fp, "_punchStarsEffect", stars);
            }

            swollen.SetActive(false);
            stars.SetActive(false);
        }

        private void AddNameTag(GameObject character, string nameText)
        {
            var tag = new GameObject("NameTag");
            tag.transform.SetParent(character.transform, false);
            tag.transform.localPosition = new Vector3(0, 1.8f, 0);

            var tmp = tag.AddComponent<TextMeshPro>();
            tmp.text = nameText;
            tmp.fontSize = 4;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 1);

            // Billboard script to always face camera
            tag.AddComponent<BillboardText>();
        }

        // ── Geometry Helpers ──

        private GameObject CreateFlat(string name, Vector3 pos, Vector2 size, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Flat on ground
            obj.transform.localScale = new Vector3(size.x, size.y, 1f);
            obj.GetComponent<Renderer>().material = CreateMat(color);
            obj.isStatic = true;
            Destroy(obj.GetComponent<Collider>());
            return obj;
        }

        private GameObject CreateCircle(string name, Vector3 pos, float radius, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(radius * 2, 0.001f, radius * 2);
            obj.GetComponent<Renderer>().material = CreateMat(color);
            obj.isStatic = true;
            Destroy(obj.GetComponent<Collider>());
            return obj;
        }

        private Material CreateMat(Color color)
        {
            // Unlit/Color = flat cartoony colors (no shading, no lighting)
            // This gives a Hay Day / cartoon look
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.color = color;

            // For transparent colors
            if (color.a < 1f)
            {
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            return mat;
        }

        private void CreateTMPText(string name, Transform parent, Vector2 pos,
            string text, float size, TextAlignmentOptions align, Color color,
            Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = color;

            var rt = tmp.GetComponent<RectTransform>();
            if (anchorMin.HasValue && anchorMax.HasValue)
            {
                rt.anchorMin = anchorMin.Value;
                rt.anchorMax = anchorMax.Value;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchoredPosition = pos;
                rt.sizeDelta = new Vector2(200, 30);
            }
        }

        private GameObject CreateUIButton(string name, Transform parent, Vector2 pos,
            string label, Color bgColor, Vector2 size)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            btnObj.AddComponent<Image>().color = bgColor;
            btnObj.AddComponent<Button>();

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 15;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            var lrt = tmp.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            return btnObj;
        }

        private void SetField<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(obj, value);
        }
    }
}
