using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Học Viện Tri Thức — Ranger Academy (30×30, centered at -60,0,0).
    /// Standard School Layout:
    ///   - Left half: Large classroom with rows of desks facing a large blackboard at the front.
    ///   - Right half: Cozy Library with bookshelves and a reading table.
    ///   - South: Open courtyard with a stone pathway, tree, benches, and fountain.
    /// No roof, so it's fully visible to the top-down camera.
    /// </summary>
    public static class StudyZoneBuilder
    {
        static readonly Color WallColor       = new(0.85f, 0.82f, 0.78f);
        static readonly Color ClassroomFloor  = new(0.72f, 0.58f, 0.42f); // Warm wood
        static readonly Color LibraryFloor    = new(0.48f, 0.22f, 0.18f); // Red carpet
        static readonly Color CourtyardGrass  = new(0.28f, 0.52f, 0.24f); // Green grass
        static readonly Color PathColor       = new(0.55f, 0.53f, 0.5f);  // Stone grey

        public static void Build()
        {
            var zone = new GameObject("StudyZone");
            zone.transform.position = new Vector3(-60f, 0, 0f);

            // ═══ Zone Base (dark stone margin) ═══
            var basePad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basePad.name = "StudyZoneBase";
            basePad.transform.SetParent(zone.transform, false);
            basePad.transform.localPosition = new Vector3(0, -0.08f, 0f);
            basePad.transform.localScale = new Vector3(28f, 0.1f, 30f);
            basePad.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.2f, 0.2f, 0.22f));
            Object.Destroy(basePad.GetComponent<Collider>());
            basePad.AddComponent<BoxCollider>().size = Vector3.one;

            // ═══ Zone Lighting — warm and cozy study glow ═══
            ZoneFactory.CreateZoneLighting(zone.transform, new Color(0.98f, 0.96f, 0.9f), 1.1f, new Vector3(0.1f, -1f, 0.2f));

            // ═══ Structural Walls ═══
            BuildWalls(zone.transform);

            // ═══ Left: Main Classroom ═══
            BuildClassroomArea(zone.transform);

            // ═══ Right: Library Wing ═══
            BuildLibraryArea(zone.transform);

            // ═══ South: Courtyard ═══
            BuildCourtyardArea(zone.transform);

            // ═══ Dust particles ═══
            ZoneParticles.CreateFloatingParticles(zone.transform, new Color(1f, 0.95f, 0.8f, 0.4f), 20, new Vector3(24f, 3f, 24f), "DustMotes");

            // ═══ Title ═══
            var title = new GameObject("StudyTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 4f, -14f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Học Viện Tri Thức";
            tmp.fontSize = 5.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.15f, 0.45f, 0.8f);
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ═══ Return Portal ═══
            ZoneFactory.CreatePortal("StudyReturnPortal", new Vector3(-54f, 0.05f, -12f), 90f,
                new Color(0.4f, 0.32f, 0.22f),
                new Color(0.95f, 0.75f, 0.15f, 0.7f),
                "Về Sảnh Chờ");
        }

        // ────────────────────────────────────────────────────────
        //  STRUCTURAL WALLS (Building outline, open roof)
        // ────────────────────────────────────────────────────────
        private static void BuildWalls(Transform parent)
        {
            float wallH = 2.4f;
            float lowWallH = 0.8f;
            Color doorFrameColor = new Color(0.4f, 0.25f, 0.15f);

            // North Wall (back) - keep tall since it's at the back
            CreateWall(parent, new Vector3(0, wallH * 0.5f, 14f), new Vector3(26f, wallH, 0.2f));

            // West Wall - low
            CreateWall(parent, new Vector3(-13f, lowWallH * 0.5f, 6f), new Vector3(0.2f, lowWallH, 16f));

            // East Wall - low
            CreateWall(parent, new Vector3(13f, lowWallH * 0.5f, 6f), new Vector3(0.2f, lowWallH, 16f));

            // Internal Divider Wall (Classroom | Library) - split into 2 segments for door gap at Z = 4.5f
            CreateWall(parent, new Vector3(2f, lowWallH * 0.5f, 10f), new Vector3(0.2f, lowWallH, 8f));
            CreateWall(parent, new Vector3(2f, lowWallH * 0.5f, 0.5f), new Vector3(0.2f, lowWallH, 5f));

            // South Front Walls (with doorway openings at X = -5f and X = 7f)
            CreateWall(parent, new Vector3(-9.5f, lowWallH * 0.5f, -2f), new Vector3(7f, lowWallH, 0.2f)); // Classroom left
            CreateWall(parent, new Vector3(-1.5f, lowWallH * 0.5f, -2f), new Vector3(5f, lowWallH, 0.2f)); // Classroom right / center
            CreateWall(parent, new Vector3(4.0f, lowWallH * 0.5f, -2f), new Vector3(4f, lowWallH, 0.2f));  // Library left
            CreateWall(parent, new Vector3(10.5f, lowWallH * 0.5f, -2f), new Vector3(5f, lowWallH, 0.2f));  // Library right

            // ═══ Labeled Doorway Frames ═══
            // 1. Classroom South Door
            BuildDoorway(parent, new Vector3(-5f, 0f, -2f), 0f, 1.4f, 1.8f, "LỚP HỌC", doorFrameColor);

            // 2. Library South Door
            BuildDoorway(parent, new Vector3(7f, 0f, -2f), 0f, 1.4f, 1.8f, "THƯ VIỆN", doorFrameColor);

            // 3. Connecting Door (between Classroom and Library)
            BuildDoorway(parent, new Vector3(2f, 0f, 4.5f), 90f, 1.4f, 1.8f, "LỐI ĐI CHUNG", doorFrameColor);
        }

        private static void CreateWall(Transform parent, Vector3 pos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "AcademyWall_Collider";
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material = ZoneFactory.StoneMat(WallColor);
        }

        // ────────────────────────────────────────────────────────
        //  LEFT AREA: CLASSROOM (X[-13, 2], Z[-2, 14])
        // ────────────────────────────────────────────────────────
        private static void BuildClassroomArea(Transform parent)
        {
            var area = new GameObject("ClassroomArea");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-5.5f, 0, 6f);

            // Floor
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero, new Vector3(15f, 0.1f, 16f), ClassroomFloor, "ClassroomFloor");

            ZoneFactory.CreateAreaSign(area.transform, new Vector3(0f, 0.6f, -0.2f), "Lớp Học");

            Color woodColor = new(0.48f, 0.32f, 0.18f);
            Color legColor = new(0.2f, 0.2f, 0.2f);

            // Large blackboard on the North wall
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Blackboard_Collider";
            board.transform.SetParent(area.transform, false);
            board.transform.localPosition = new Vector3(0, 1.4f, 7.8f);
            board.transform.localScale = new Vector3(6f, 1.5f, 0.08f);
            board.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.08f, 0.18f, 0.12f));

            var boardText = new GameObject("BoardText");
            boardText.transform.SetParent(board.transform, false);
            boardText.transform.localPosition = new Vector3(0, 0, -0.6f);
            var btmp = boardText.AddComponent<TextMeshPro>();
            btmp.text = "Ranger Academy\n----------\n[1] Bài 1: Bảo Vệ Rừng\n[2] Bài 2: Toán Sinh Tồn\n[3] Bài 3: Đọc Bản Đồ";
            btmp.fontSize = 1.6f;
            btmp.color = Color.white;
            btmp.alignment = TextAlignmentOptions.Center;

            // Teacher desk & podium at the front
            var podium = GameObject.CreatePrimitive(PrimitiveType.Cube);
            podium.name = "TeacherPodium_Collider";
            podium.transform.SetParent(area.transform, false);
            podium.transform.localPosition = new Vector3(0, 0.08f, 5.2f);
            podium.transform.localScale = new Vector3(3.5f, 0.16f, 1.8f);
            podium.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 1.1f);

            var tDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tDesk.name = "TeacherDesk_Collider";
            tDesk.transform.SetParent(area.transform, false);
            tDesk.transform.localPosition = new Vector3(0, 0.45f, 5.2f);
            tDesk.transform.localScale = new Vector3(2f, 0.6f, 0.8f);
            tDesk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            // Teacher chair
            var tChair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tChair.name = "TeacherChair_Collider";
            tChair.transform.SetParent(area.transform, false);
            tChair.transform.localPosition = new Vector3(0, 0.35f, 6.2f);
            tChair.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            tChair.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 0.8f);

            // Clock above blackboard
            BuildClock(area.transform, new Vector3(0, 2.4f, 7.85f));

            // Student desks in neat rows (3 rows × 2 columns = 6 desks) facing North
            float[] rowsZ = { -4f, -1f, 2f };
            float[] colsX = { -3.2f, 3.2f };

            foreach (float rz in rowsZ)
            {
                foreach (float cx in colsX)
                {
                    BuildStudentDesk(area.transform, new Vector3(cx, 0, rz), 0f, woodColor, legColor);
                }
            }

            // Light sources (hanging lamps)
            float[] lightsZ = { -2f, 3f };
            foreach (float lz in lightsZ)
            {
                var pendant = new GameObject("ClassroomLight");
                pendant.transform.SetParent(area.transform, false);
                pendant.transform.localPosition = new Vector3(0, 2.3f, lz);

                var pl = pendant.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(1f, 0.95f, 0.85f);
                pl.range = 7f;
                pl.intensity = 1.0f;
            }
        }

        // ────────────────────────────────────────────────────────
        //  RIGHT AREA: LIBRARY (X[2, 13], Z[-2, 14])
        // ────────────────────────────────────────────────────────
        private static void BuildLibraryArea(Transform parent)
        {
            var area = new GameObject("LibraryArea");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(7.5f, 0, 6f);

            // Floor
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero, new Vector3(11f, 0.1f, 16f), LibraryFloor, "LibraryFloor");

            ZoneFactory.CreateAreaSign(area.transform, new Vector3(0f, 0.6f, 0f), "Thư Viện");

            Color woodColor = new(0.45f, 0.3f, 0.15f);

            // Bookshelves along East and North walls
            BuildBookshelf(area.transform, new Vector3(4.2f, 0, 5f), -90f, 2.2f);
            BuildBookshelf(area.transform, new Vector3(4.2f, 0, 1.5f), -90f, 2.2f);
            BuildBookshelf(area.transform, new Vector3(4.2f, 0, -2f), -90f, 2.2f);
            BuildBookshelf(area.transform, new Vector3(0f, 0, 7.2f), 0f, 2.2f);

            // Long study table in the center
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "LibraryTable_Collider";
            table.transform.SetParent(area.transform, false);
            table.transform.localPosition = new Vector3(-1.2f, 0.45f, 0f);
            table.transform.localScale = new Vector3(3.2f, 0.08f, 1.2f);
            table.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            // Table legs
            float[] lx = { -1.4f, 1.4f };
            float[] lz = { -0.5f, 0.5f };
            foreach (float x in lx)
            {
                foreach (float z in lz)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg";
                    leg.transform.SetParent(table.transform, false);
                    leg.transform.localPosition = new Vector3(x / 3.2f, -5f, z);
                    leg.transform.localScale = new Vector3(0.06f / 3.2f, 5f, 0.06f);
                    leg.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.2f, 0.2f, 0.22f));
                    Object.Destroy(leg.GetComponent<Collider>());
                }
            }

            // Study chairs (4)
            Vector3[] chairPositions = {
                new(-2.2f, 0.35f, 0.4f), new(-0.2f, 0.35f, 0.4f),
                new(-2.2f, 0.35f, -0.4f), new(-0.2f, 0.35f, -0.4f)
            };
            foreach (var pos in chairPositions)
            {
                var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.name = "StudyChair_Collider";
                chair.transform.SetParent(area.transform, false);
                chair.transform.localPosition = pos;
                chair.transform.localScale = new Vector3(0.45f, 0.4f, 0.45f);
                chair.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 0.8f);
            }

            // Globe on small stand
            var globeStand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            globeStand.name = "GlobeStand_Collider";
            globeStand.transform.SetParent(area.transform, false);
            globeStand.transform.localPosition = new Vector3(-3.5f, 0.3f, 5f);
            globeStand.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            globeStand.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            var globeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            globeSphere.name = "Globe";
            globeSphere.transform.SetParent(area.transform, false);
            globeSphere.transform.localPosition = new Vector3(-3.5f, 0.7f, 5f);
            globeSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            globeSphere.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.2f, 0.5f, 0.8f));
            Object.Destroy(globeSphere.GetComponent<Collider>());
        }

        // ────────────────────────────────────────────────────────
        //  SOUTH AREA: COURTYARD (X[-13, 13], Z[-14, -2])
        // ────────────────────────────────────────────────────────
        private static void BuildCourtyardArea(Transform parent)
        {
            var area = new GameObject("CourtyardArea");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(0, 0, -8f);

            // Grass floor
            ZoneFactory.CreateSubAreaFloor(area.transform, Vector3.zero, new Vector3(26f, 0.1f, 12f), CourtyardGrass, "CourtyardFloor");

            ZoneFactory.CreateAreaSign(area.transform, new Vector3(-1.2f, 0.6f, 0.5f), "Sân Trường");

            // Pathways from south edges to school doors
            BuildStonePath(area.transform, new Vector3(-5.5f, 0, 0), new Vector3(2f, 0.02f, 12f)); // Path to classroom door
            BuildStonePath(area.transform, new Vector3(7.5f, 0, 0), new Vector3(2f, 0.02f, 12f));  // Path to library door
            BuildStonePath(area.transform, new Vector3(1f, 0, -4f), new Vector3(15f, 0.02f, 2f));   // Horizontal connecting path

            // Large central tree with circular bench
            BuildCourtyardTree(area.transform, new Vector3(1f, 0, 1f));

            // Benches along paths
            BuildStoneBench(area.transform, new Vector3(-9f, 0.2f, -4f), 0f);
            BuildStoneBench(area.transform, new Vector3(11f, 0.2f, -4f), 0f);

            // Small water fountain in the grass
            var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountain.name = "CourtyardFountain";
            fountain.transform.SetParent(area.transform, false);
            fountain.transform.localPosition = new Vector3(-9f, 0.15f, 2f);
            fountain.transform.localScale = new Vector3(1.2f, 0.15f, 1.2f);
            fountain.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.65f, 0.63f, 0.6f));
            Object.Destroy(fountain.GetComponent<Collider>());

            var fountainWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountainWater.name = "FountainWater";
            fountainWater.transform.SetParent(area.transform, false);
            fountainWater.transform.localPosition = new Vector3(-9f, 0.22f, 2f);
            fountainWater.transform.localScale = new Vector3(1.0f, 0.01f, 1.0f);
            fountainWater.GetComponent<Renderer>().material = ZoneFactory.WaterMat(new Color(0.3f, 0.6f, 0.85f, 0.7f));
            Object.Destroy(fountainWater.GetComponent<Collider>());
        }

        // ═══════════════════════ HELPER BUILDERS ═══════════════════════

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
