using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Học Viện Tri Thức — Knowledge Academy with 5 sub-areas:
    /// 1. Main Classroom (W-center) — U-shape desks
    /// 2. Library Wing (NW)         — tall bookshelves, reading nook
    /// 3. Lab Room (NE)             — experiment tables
    /// 4. Art Corner (E)            — easel, sculptures
    /// 5. Courtyard (S)             — open air, fountain, tree
    /// Located at world X = -60.
    /// </summary>
    public static class StudyZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("StudyZone");
            zone.transform.position = new Vector3(-60f, 0, 0f);

            // ═══ Ground — split indoor/outdoor ═══
            // Indoor floor (tiled wood)
            var indoorFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indoorFloor.name = "IndoorFloor";
            indoorFloor.transform.SetParent(zone.transform, false);
            indoorFloor.transform.localPosition = new Vector3(0, -0.05f, 4f);
            indoorFloor.transform.localScale = new Vector3(28f, 0.1f, 20f);
            indoorFloor.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.65f, 0.5f, 0.35f));
            Object.Destroy(indoorFloor.GetComponent<Collider>());
            indoorFloor.AddComponent<BoxCollider>();

            // Outdoor courtyard (stone)
            var courtyard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            courtyard.name = "CourtyardFloor";
            courtyard.transform.SetParent(zone.transform, false);
            courtyard.transform.localPosition = new Vector3(0, -0.05f, -10f);
            courtyard.transform.localScale = new Vector3(28f, 0.1f, 12f);
            courtyard.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.6f, 0.58f, 0.55f));
            Object.Destroy(courtyard.GetComponent<Collider>());
            courtyard.AddComponent<BoxCollider>();

            // ═══ Zone Lighting — cool fluorescent ═══
            ZoneFactory.CreateZoneLighting(zone.transform, new Color(0.9f, 0.92f, 1f), 1.5f, new Vector3(0, -1f, 0.3f));

            // ═══ Building Structure — walls, roof, archways ═══
            BuildStructure(zone.transform);

            // ═══ Area 1: Main Classroom (W-center) ═══
            BuildClassroom(zone.transform);

            // ═══ Area 2: Library Wing (NW) ═══
            BuildLibrary(zone.transform);

            // ═══ Area 3: Lab Room (NE) ═══
            BuildLabRoom(zone.transform);

            // ═══ Area 4: Art Corner (E) ═══
            BuildArtCorner(zone.transform);

            // ═══ Area 5: Courtyard (S) ═══
            BuildCourtyard(zone.transform);

            // ═══ Dust mote particles ═══
            ZoneParticles.CreateFloatingParticles(
                zone.transform,
                new Color(1f, 0.98f, 0.9f, 0.5f),
                30,
                new Vector3(24f, 3f, 16f),
                "DustMotes");

            // ═══ Title ═══
            var title = new GameObject("StudyTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 4.5f, -14f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Học Viện Tri Thức";
            tmp.fontSize = 6.4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ═══ Return Portal ═══
            ZoneFactory.CreatePortal("StudyReturnPortal", new Vector3(-54f, 0.05f, 0), 90f,
                new Color(0.4f, 0.32f, 0.22f),
                new Color(0.95f, 0.75f, 0.15f, 0.7f),
                "Về Sảnh Chờ");
        }

        // ── Building Structure ──
        private static void BuildStructure(Transform parent)
        {
            Color wallColor = new Color(0.88f, 0.85f, 0.78f);
            Color pillarColor = new Color(0.82f, 0.8f, 0.76f);

            // Corner pillars (8 structural pillars)
            Vector3[] pillarPos = {
                new(-13f, 1.5f, 14f), new(-13f, 1.5f, 6f), new(-13f, 1.5f, -2f),
                new(13f, 1.5f, 14f), new(13f, 1.5f, 6f), new(13f, 1.5f, -2f),
                new(-4f, 1.5f, 14f), new(4f, 1.5f, 14f)
            };
            foreach (var pos in pillarPos)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "AcademyPillar_Collider";
                pillar.transform.SetParent(parent, false);
                pillar.transform.localPosition = pos;
                pillar.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
                pillar.GetComponent<Renderer>().material = ZoneFactory.StoneMat(pillarColor);
            }

            // North wall (back of building)
            var wallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallN.name = "WallN_Collider";
            wallN.transform.SetParent(parent, false);
            wallN.transform.localPosition = new Vector3(0, 1.5f, 14f);
            wallN.transform.localScale = new Vector3(26f, 3f, 0.2f);
            wallN.GetComponent<Renderer>().material = ZoneFactory.StoneMat(wallColor);

            // West wall (with window frames)
            var wallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW.name = "WallW_Collider";
            wallW.transform.SetParent(parent, false);
            wallW.transform.localPosition = new Vector3(-13f, 1.5f, 6f);
            wallW.transform.localScale = new Vector3(0.2f, 3f, 16f);
            wallW.GetComponent<Renderer>().material = ZoneFactory.StoneMat(wallColor);

            // East wall (partial — open to courtyard)
            var wallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE.name = "WallE_Collider";
            wallE.transform.SetParent(parent, false);
            wallE.transform.localPosition = new Vector3(13f, 1.5f, 8f);
            wallE.transform.localScale = new Vector3(0.2f, 3f, 12f);
            wallE.GetComponent<Renderer>().material = ZoneFactory.StoneMat(wallColor);

            // Partial roof over indoor (flat cube ceiling)
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(parent, false);
            roof.transform.localPosition = new Vector3(0, 3.1f, 6f);
            roof.transform.localScale = new Vector3(26f, 0.15f, 16f);
            roof.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.75f, 0.55f, 0.35f));
            Object.Destroy(roof.GetComponent<Collider>());

            // Divider wall between classroom & library
            var divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = "Divider_Collider";
            divider.transform.SetParent(parent, false);
            divider.transform.localPosition = new Vector3(-4f, 1.0f, 10f);
            divider.transform.localScale = new Vector3(0.15f, 2.0f, 8f);
            divider.GetComponent<Renderer>().material = ZoneFactory.StoneMat(wallColor * 0.95f);

            // Divider between classroom & lab
            var divider2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider2.name = "Divider2_Collider";
            divider2.transform.SetParent(parent, false);
            divider2.transform.localPosition = new Vector3(4f, 1.0f, 10f);
            divider2.transform.localScale = new Vector3(0.15f, 2.0f, 8f);
            divider2.GetComponent<Renderer>().material = ZoneFactory.StoneMat(wallColor * 0.95f);

            // Window frames on west wall
            for (int i = 0; i < 3; i++)
            {
                float z = 2f + i * 5f;
                var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frame.name = "WindowFrame";
                frame.transform.SetParent(parent, false);
                frame.transform.localPosition = new Vector3(-12.85f, 2.0f, z);
                frame.transform.localScale = new Vector3(0.08f, 1.2f, 1.5f);
                frame.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.55f, 0.4f, 0.25f));
                Object.Destroy(frame.GetComponent<Collider>());
            }

            // Hanging pendant lights
            float[] lightXs = { -8f, 0f, 8f };
            foreach (float lx in lightXs)
            {
                var pendant = new GameObject("PendantLight");
                pendant.transform.SetParent(parent, false);
                pendant.transform.localPosition = new Vector3(lx, 2.6f, 8f);

                // Wire
                var wire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wire.name = "Wire";
                wire.transform.SetParent(pendant.transform, false);
                wire.transform.localPosition = new Vector3(0, 0.2f, 0);
                wire.transform.localScale = new Vector3(0.015f, 0.2f, 0.015f);
                wire.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.2f, 0.2f, 0.2f));
                Object.Destroy(wire.GetComponent<Collider>());

                // Shade
                var shade = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shade.name = "Shade";
                shade.transform.SetParent(pendant.transform, false);
                shade.transform.localPosition = Vector3.zero;
                shade.transform.localScale = new Vector3(0.35f, 0.18f, 0.35f);
                shade.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.9f, 0.85f, 0.7f, 0.8f));
                Object.Destroy(shade.GetComponent<Collider>());

                // Light source
                var light = pendant.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.95f, 0.85f);
                light.range = 6f;
                light.intensity = 1.2f;
            }
        }

        // ── Area 1: Main Classroom — U-shape desks ──
        private static void BuildClassroom(Transform parent)
        {
            var area = new GameObject("Classroom");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(0, 0, 6f);

            Color woodColor = new Color(0.48f, 0.32f, 0.18f);
            Color legColor = new Color(0.2f, 0.2f, 0.2f);

            // U-shape desk arrangement (not grid!)
            Vector3[] deskPositions = {
                // Left arm of U
                new(-2.5f, 0, 2f), new(-2.5f, 0, 0f), new(-2.5f, 0, -2f),
                // Bottom of U
                new(-1f, 0, -3f), new(1f, 0, -3f),
                // Right arm of U
                new(2.5f, 0, 2f), new(2.5f, 0, 0f), new(2.5f, 0, -2f),
            };
            float[] deskRotations = {
                90f, 90f, 90f,
                0f, 0f,
                -90f, -90f, -90f,
            };

            for (int i = 0; i < deskPositions.Length; i++)
            {
                BuildStudentDesk(area.transform, deskPositions[i], deskRotations[i], woodColor, legColor);
            }

            // Teacher podium (raised platform)
            var podium = GameObject.CreatePrimitive(PrimitiveType.Cube);
            podium.name = "Podium_Collider";
            podium.transform.SetParent(area.transform, false);
            podium.transform.localPosition = new Vector3(0, 0.08f, 5f);
            podium.transform.localScale = new Vector3(3f, 0.16f, 2f);
            podium.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 1.1f);

            // Teacher desk on podium
            var teacherDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            teacherDesk.name = "TeacherDesk_Collider";
            teacherDesk.transform.SetParent(area.transform, false);
            teacherDesk.transform.localPosition = new Vector3(0, 0.45f, 5.5f);
            teacherDesk.transform.localScale = new Vector3(2.0f, 0.6f, 0.8f);
            teacherDesk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            // Blackboard (on wall behind teacher)
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Blackboard_Collider";
            board.transform.SetParent(area.transform, false);
            board.transform.localPosition = new Vector3(0, 1.6f, 7.8f);
            board.transform.localScale = new Vector3(3.5f, 1.5f, 0.1f);
            board.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.08f, 0.2f, 0.12f));

            // Blackboard text
            var boardText = new GameObject("BoardText");
            boardText.transform.SetParent(board.transform, false);
            boardText.transform.localPosition = new Vector3(0, 0, -0.06f);
            var btmp = boardText.AddComponent<TextMeshPro>();
            btmp.text = "Ranger Academy\n━━━━━━━━━━\n🌳 Bài 1: Bảo Vệ Rừng\n📐 Bài 2: Toán Sinh Tồn\n🗺️ Bài 3: Đọc Bản Đồ";
            btmp.fontSize = 1.8f;
            btmp.color = Color.white;
            btmp.alignment = TextAlignmentOptions.Left;

            // Clock on wall
            BuildClock(area.transform, new Vector3(3f, 2.5f, 7.85f));
        }

        // ── Area 2: Library Wing ──
        private static void BuildLibrary(Transform parent)
        {
            var area = new GameObject("LibraryWing");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-8f, 0, 10f);

            // Tall bookshelves forming corridor
            BuildBookshelf(area.transform, new Vector3(-3f, 0, 0), 0f, 2.2f);
            BuildBookshelf(area.transform, new Vector3(0f, 0, 0), 0f, 2.2f);
            BuildBookshelf(area.transform, new Vector3(-3f, 0, 3.5f), 0f, 1.8f);

            // Reading nook — armchair + lamp
            var chairSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairSeat.name = "ArmchairSeat_Collider";
            chairSeat.transform.SetParent(area.transform, false);
            chairSeat.transform.localPosition = new Vector3(2f, 0.25f, 3f);
            chairSeat.transform.localScale = new Vector3(0.7f, 0.08f, 0.7f);
            chairSeat.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.2f, 0.15f));

            var chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chairBack.name = "Backrest";
            chairBack.transform.SetParent(area.transform, false);
            chairBack.transform.localPosition = new Vector3(2f, 0.5f, 3.3f);
            chairBack.transform.localScale = new Vector3(0.7f, 0.5f, 0.08f);
            chairBack.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.2f, 0.15f));
            Object.Destroy(chairBack.GetComponent<Collider>());

            // Armrests
            var armL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armL.name = "ArmL";
            armL.transform.SetParent(area.transform, false);
            armL.transform.localPosition = new Vector3(1.65f, 0.4f, 3.15f);
            armL.transform.localScale = new Vector3(0.06f, 0.25f, 0.5f);
            armL.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.2f, 0.15f));
            Object.Destroy(armL.GetComponent<Collider>());

            var armR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armR.name = "ArmR";
            armR.transform.SetParent(area.transform, false);
            armR.transform.localPosition = new Vector3(2.35f, 0.4f, 3.15f);
            armR.transform.localScale = new Vector3(0.06f, 0.25f, 0.5f);
            armR.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.2f, 0.15f));
            Object.Destroy(armR.GetComponent<Collider>());

            // Reading lamp
            BuildDeskLamp(area.transform, new Vector3(3f, 0, 3f));
        }

        // ── Area 3: Lab Room ──
        private static void BuildLabRoom(Transform parent)
        {
            var area = new GameObject("LabRoom");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(8f, 0, 10f);

            // Lab tables (2 long tables)
            for (int i = 0; i < 2; i++)
            {
                float z = i * 3.5f;
                var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
                table.name = "LabTable_Collider";
                table.transform.SetParent(area.transform, false);
                table.transform.localPosition = new Vector3(0, 0.45f, z);
                table.transform.localScale = new Vector3(3.5f, 0.08f, 1.0f);
                table.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.7f, 0.7f, 0.72f));

                // Table legs
                float[] lx = { -1.6f, 1.6f };
                float[] lz = { -0.4f, 0.4f };
                foreach (float x in lx)
                {
                    foreach (float zl in lz)
                    {
                        var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        leg.name = "Leg";
                        leg.transform.SetParent(table.transform, false);
                        leg.transform.localPosition = new Vector3(x / 3.5f, -2.5f, zl);
                        leg.transform.localScale = new Vector3(0.05f / 3.5f, 2.5f, 0.05f);
                        leg.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.35f, 0.35f, 0.38f));
                        Object.Destroy(leg.GetComponent<Collider>());
                    }
                }

                // Test tube rack
                var rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rack.name = "TestTubeRack";
                rack.transform.SetParent(area.transform, false);
                rack.transform.localPosition = new Vector3(-0.8f, 0.55f, z);
                rack.transform.localScale = new Vector3(0.4f, 0.06f, 0.15f);
                rack.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.35f, 0.2f));
                Object.Destroy(rack.GetComponent<Collider>());

                // Test tubes (colored cylinders)
                Color[] tubeColors = { new(0.3f, 0.7f, 0.3f), new(0.7f, 0.3f, 0.3f), new(0.3f, 0.5f, 0.8f) };
                for (int t = 0; t < 3; t++)
                {
                    var tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tube.name = "TestTube";
                    tube.transform.SetParent(area.transform, false);
                    tube.transform.localPosition = new Vector3(-0.9f + t * 0.1f, 0.65f, z);
                    tube.transform.localScale = new Vector3(0.02f, 0.08f, 0.02f);
                    tube.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(tubeColors[t]);
                    Object.Destroy(tube.GetComponent<Collider>());
                }

                // Microscope (compound shape)
                if (i == 0)
                {
                    BuildMicroscope(area.transform, new Vector3(1f, 0.5f, z));
                }
            }

            // Periodic table poster (flat quad)
            var poster = GameObject.CreatePrimitive(PrimitiveType.Cube);
            poster.name = "PeriodicTable";
            poster.transform.SetParent(area.transform, false);
            poster.transform.localPosition = new Vector3(0, 1.8f, 3.9f);
            poster.transform.localScale = new Vector3(2.5f, 1.5f, 0.02f);
            poster.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.95f, 0.95f, 0.9f));
            Object.Destroy(poster.GetComponent<Collider>());

            var posterText = new GameObject("PosterText");
            posterText.transform.SetParent(poster.transform, false);
            posterText.transform.localPosition = new Vector3(0, 0, -0.6f);
            var ptmp = posterText.AddComponent<TextMeshPro>();
            ptmp.text = "BẢNG TUẦN HOÀN\nH He Li Be B C N O F Ne";
            ptmp.fontSize = 1.5f;
            ptmp.color = Color.black;
            ptmp.alignment = TextAlignmentOptions.Center;
        }

        // ── Area 4: Art Corner ──
        private static void BuildArtCorner(Transform parent)
        {
            var area = new GameObject("ArtCorner");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(10f, 0, 2f);

            // Easel
            var easelLegs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            easelLegs.name = "EaselFrame_Collider";
            easelLegs.transform.SetParent(area.transform, false);
            easelLegs.transform.localPosition = new Vector3(0, 0.8f, 0);
            easelLegs.transform.localScale = new Vector3(0.06f, 1.6f, 0.6f);
            easelLegs.transform.localRotation = Quaternion.Euler(10f, 0, 0);
            easelLegs.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.35f, 0.2f));

            // Canvas
            var canvas = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canvas.name = "Canvas";
            canvas.transform.SetParent(area.transform, false);
            canvas.transform.localPosition = new Vector3(0, 1.3f, 0.15f);
            canvas.transform.localScale = new Vector3(0.8f, 0.6f, 0.02f);
            canvas.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.95f, 0.92f, 0.85f));
            Object.Destroy(canvas.GetComponent<Collider>());

            // Paint palette on floor
            var palette = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            palette.name = "Palette";
            palette.transform.SetParent(area.transform, false);
            palette.transform.localPosition = new Vector3(0.8f, 0.35f, 0.3f);
            palette.transform.localScale = new Vector3(0.3f, 0.01f, 0.2f);
            palette.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.6f, 0.45f, 0.3f));
            Object.Destroy(palette.GetComponent<Collider>());

            // Paint blobs on palette
            Color[] paints = { Color.red, Color.blue, new(1f, 0.8f, 0f), Color.green };
            for (int i = 0; i < paints.Length; i++)
            {
                var blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blob.name = "PaintBlob";
                blob.transform.SetParent(palette.transform, false);
                float angle = i * 90f * Mathf.Deg2Rad;
                blob.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.3f, 0.3f, Mathf.Sin(angle) * 0.3f);
                blob.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
                blob.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(paints[i]);
                Object.Destroy(blob.GetComponent<Collider>());
            }

            // Small sculptures (abstract shapes)
            var sculpt1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sculpt1.name = "Sculpture1";
            sculpt1.transform.SetParent(area.transform, false);
            sculpt1.transform.localPosition = new Vector3(-1.5f, 0.3f, -1f);
            sculpt1.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f);
            sculpt1.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.85f, 0.85f, 0.88f));
            Object.Destroy(sculpt1.GetComponent<Collider>());

            var sculpt2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sculpt2.name = "Sculpture2";
            sculpt2.transform.SetParent(area.transform, false);
            sculpt2.transform.localPosition = new Vector3(-1f, 0.25f, 1.5f);
            sculpt2.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            sculpt2.transform.localRotation = Quaternion.Euler(0, 45f, 0);
            sculpt2.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.3f, 0.5f, 0.8f));
            Object.Destroy(sculpt2.GetComponent<Collider>());
        }

        // ── Area 5: Courtyard ──
        private static void BuildCourtyard(Transform parent)
        {
            var area = new GameObject("Courtyard");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(0, 0, -10f);

            // Large central tree
            BuildCourtyardTree(area.transform, new Vector3(0, 0, 0));

            // Stone benches (4 around tree)
            for (int i = 0; i < 4; i++)
            {
                float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                float dist = 3.5f;
                var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bench.name = "StoneBench_Collider";
                bench.transform.SetParent(area.transform, false);
                bench.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, 0.2f, Mathf.Sin(angle) * dist);
                bench.transform.localScale = new Vector3(1.5f, 0.4f, 0.5f);
                bench.transform.localRotation = Quaternion.Euler(0, -i * 90f, 0);
                bench.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.6f, 0.58f, 0.55f));
            }

            // Small fountain
            var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountain.name = "CourtyardFountain";
            fountain.transform.SetParent(area.transform, false);
            fountain.transform.localPosition = new Vector3(-6f, 0.15f, 0);
            fountain.transform.localScale = new Vector3(1.2f, 0.15f, 1.2f);
            fountain.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.65f, 0.63f, 0.6f));
            Object.Destroy(fountain.GetComponent<Collider>());

            var fountainWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountainWater.name = "FountainWater";
            fountainWater.transform.SetParent(area.transform, false);
            fountainWater.transform.localPosition = new Vector3(-6f, 0.22f, 0);
            fountainWater.transform.localScale = new Vector3(1.0f, 0.01f, 1.0f);
            fountainWater.GetComponent<Renderer>().material = ZoneFactory.WaterMat(new Color(0.3f, 0.6f, 0.85f, 0.7f));
            Object.Destroy(fountainWater.GetComponent<Collider>());

            // Bulletin board
            var bulletin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bulletin.name = "BulletinBoard_Collider";
            bulletin.transform.SetParent(area.transform, false);
            bulletin.transform.localPosition = new Vector3(7f, 1.0f, 2f);
            bulletin.transform.localScale = new Vector3(2f, 1.5f, 0.1f);
            bulletin.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.55f, 0.4f, 0.25f));

            var boardText = new GameObject("BulletinText");
            boardText.transform.SetParent(bulletin.transform, false);
            boardText.transform.localPosition = new Vector3(0, 0, -0.6f);
            var btmp = boardText.AddComponent<TextMeshPro>();
            btmp.text = "📌 THÔNG BÁO\n━━━━━━━━━\n🎉 Lễ tốt nghiệp\n📝 Đăng ký khóa mới\n🏆 BXH: Top Rangers";
            btmp.fontSize = 1.2f;
            btmp.color = new Color(0.15f, 0.12f, 0.1f);
            btmp.alignment = TextAlignmentOptions.Left;
        }

        // ═══════════════════════ HELPER BUILDERS ═══════════════════════

        private static void BuildStudentDesk(Transform parent, Vector3 pos, float rotY,
            Color woodColor, Color legColor)
        {
            var desk = new GameObject("StudentDesk");
            desk.transform.SetParent(parent, false);
            desk.transform.localPosition = pos;
            desk.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Tabletop_Collider";
            top.transform.SetParent(desk.transform, false);
            top.transform.localPosition = new Vector3(0, 0.65f, 0);
            top.transform.localScale = new Vector3(1.2f, 0.06f, 0.6f);
            top.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            float[] lx = { -0.5f, 0.5f };
            float[] lz = { -0.25f, 0.25f };
            foreach (float x in lx)
            {
                foreach (float z in lz)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg";
                    leg.transform.SetParent(desk.transform, false);
                    leg.transform.localPosition = new Vector3(x, 0.3f, z);
                    leg.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);
                    leg.GetComponent<Renderer>().material = ZoneFactory.MetalMat(legColor);
                    Object.Destroy(leg.GetComponent<Collider>());
                }
            }

            // Chair
            var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chair.name = "Chair_Collider";
            chair.transform.SetParent(desk.transform, false);
            chair.transform.localPosition = new Vector3(0, 0.35f, -0.6f);
            chair.transform.localScale = new Vector3(0.45f, 0.04f, 0.45f);
            chair.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor * 0.9f);
        }

        private static void BuildBookshelf(Transform parent, Vector3 pos, float rotY, float height)
        {
            var shelf = new GameObject("Bookshelf");
            shelf.transform.SetParent(parent, false);
            shelf.transform.localPosition = pos;
            shelf.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            Color woodColor = new Color(0.48f, 0.32f, 0.18f);

            // Back panel
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back_Collider";
            back.transform.SetParent(shelf.transform, false);
            back.transform.localPosition = new Vector3(0, height * 0.5f, 0.3f);
            back.transform.localScale = new Vector3(2f, height, 0.06f);
            back.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);

            // Sides
            var sL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sL.name = "SideL";
            sL.transform.SetParent(shelf.transform, false);
            sL.transform.localPosition = new Vector3(-0.95f, height * 0.5f, 0);
            sL.transform.localScale = new Vector3(0.06f, height, 0.6f);
            sL.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);
            Object.Destroy(sL.GetComponent<Collider>());

            var sR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sR.name = "SideR";
            sR.transform.SetParent(shelf.transform, false);
            sR.transform.localPosition = new Vector3(0.95f, height * 0.5f, 0);
            sR.transform.localScale = new Vector3(0.06f, height, 0.6f);
            sR.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);
            Object.Destroy(sR.GetComponent<Collider>());

            // Shelves
            int shelfCount = Mathf.FloorToInt(height / 0.6f);
            for (int i = 0; i <= shelfCount; i++)
            {
                float h = i * (height / shelfCount);
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = "ShelfPlate";
                plate.transform.SetParent(shelf.transform, false);
                plate.transform.localPosition = new Vector3(0, h, 0);
                plate.transform.localScale = new Vector3(1.8f, 0.05f, 0.55f);
                plate.GetComponent<Renderer>().material = ZoneFactory.WoodMat(woodColor);
                Object.Destroy(plate.GetComponent<Collider>());
            }

            // Books on shelves
            Color[] bookColors = {
                new(0.85f, 0.2f, 0.2f), new(0.2f, 0.4f, 0.85f),
                new(0.9f, 0.8f, 0.2f), new(0.2f, 0.7f, 0.3f),
                new(0.8f, 0.4f, 0.8f), new(0.15f, 0.15f, 0.18f)
            };
            for (int s = 0; s < shelfCount; s++)
            {
                float sh = s * (height / shelfCount) + 0.06f;
                int bookCount = Random.Range(6, 10);
                float startX = -0.8f;
                float stepX = 1.6f / bookCount;

                for (int b = 0; b < bookCount; b++)
                {
                    float bh = Random.Range(0.3f, 0.5f);
                    var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    book.name = "Book";
                    book.transform.SetParent(shelf.transform, false);
                    book.transform.localPosition = new Vector3(startX + b * stepX, sh + bh * 0.5f, 0);
                    book.transform.localScale = new Vector3(Random.Range(0.04f, 0.08f), bh, Random.Range(0.3f, 0.4f));
                    book.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(bookColors[Random.Range(0, bookColors.Length)]);
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
            face.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.95f, 0.95f, 0.9f));
            Object.Destroy(face.GetComponent<Collider>());

            var frame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frame.name = "ClockFrame";
            frame.transform.SetParent(clock.transform, false);
            frame.transform.localScale = new Vector3(0.44f, 0.025f, 0.44f);
            frame.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            frame.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.3f, 0.25f, 0.2f));
            Object.Destroy(frame.GetComponent<Collider>());
        }

        private static void BuildMicroscope(Transform parent, Vector3 pos)
        {
            var micro = new GameObject("Microscope");
            micro.transform.SetParent(parent, false);
            micro.transform.localPosition = pos;

            var baseM = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseM.name = "MicroBase";
            baseM.transform.SetParent(micro.transform, false);
            baseM.transform.localPosition = new Vector3(0, 0.02f, 0);
            baseM.transform.localScale = new Vector3(0.15f, 0.04f, 0.2f);
            baseM.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.2f, 0.2f, 0.22f));
            Object.Destroy(baseM.GetComponent<Collider>());

            var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "MicroArm";
            arm.transform.SetParent(micro.transform, false);
            arm.transform.localPosition = new Vector3(0, 0.15f, 0.05f);
            arm.transform.localScale = new Vector3(0.03f, 0.12f, 0.03f);
            arm.transform.localRotation = Quaternion.Euler(15f, 0, 0);
            arm.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.25f, 0.25f, 0.28f));
            Object.Destroy(arm.GetComponent<Collider>());

            var eyepiece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            eyepiece.name = "Eyepiece";
            eyepiece.transform.SetParent(micro.transform, false);
            eyepiece.transform.localPosition = new Vector3(0, 0.28f, 0.08f);
            eyepiece.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            eyepiece.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.15f, 0.15f, 0.18f));
            Object.Destroy(eyepiece.GetComponent<Collider>());
        }

        private static void BuildDeskLamp(Transform parent, Vector3 pos)
        {
            var lamp = new GameObject("DeskLamp");
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = pos;

            var baseL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseL.name = "LampBase";
            baseL.transform.SetParent(lamp.transform, false);
            baseL.transform.localPosition = new Vector3(0, 0.02f, 0);
            baseL.transform.localScale = new Vector3(0.15f, 0.02f, 0.15f);
            baseL.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.25f, 0.25f, 0.28f));
            Object.Destroy(baseL.GetComponent<Collider>());

            var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "LampArm";
            arm.transform.SetParent(lamp.transform, false);
            arm.transform.localPosition = new Vector3(0, 0.25f, 0);
            arm.transform.localScale = new Vector3(0.02f, 0.25f, 0.02f);
            arm.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.2f, 0.2f, 0.22f));
            Object.Destroy(arm.GetComponent<Collider>());

            var shade = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shade.name = "LampShade";
            shade.transform.SetParent(lamp.transform, false);
            shade.transform.localPosition = new Vector3(0, 0.5f, 0);
            shade.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
            shade.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.85f, 0.75f, 0.5f));
            Object.Destroy(shade.GetComponent<Collider>());

            // Light
            var light = lamp.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.92f, 0.7f);
            light.range = 3f;
            light.intensity = 1f;
        }

        private static void BuildCourtyardTree(Transform parent, Vector3 pos)
        {
            var tree = new GameObject("CourtyardTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            // Thick trunk
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk_Collider";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0, 1.2f, 0);
            trunk.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.28f, 0.15f));

            // Large spreading canopy (4 overlapping spheres)
            var c1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c1.transform.SetParent(tree.transform, false);
            c1.transform.localPosition = new Vector3(0, 2.8f, 0);
            c1.transform.localScale = new Vector3(3f, 1.5f, 3f);
            c1.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.55f, 0.18f));
            Object.Destroy(c1.GetComponent<Collider>());

            var c2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c2.transform.SetParent(tree.transform, false);
            c2.transform.localPosition = new Vector3(0.8f, 3.0f, 0.5f);
            c2.transform.localScale = new Vector3(2f, 1.2f, 2f);
            c2.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.25f, 0.6f, 0.2f));
            Object.Destroy(c2.GetComponent<Collider>());

            var c3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c3.transform.SetParent(tree.transform, false);
            c3.transform.localPosition = new Vector3(-0.7f, 2.7f, -0.4f);
            c3.transform.localScale = new Vector3(2.2f, 1.3f, 2.2f);
            c3.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.18f, 0.5f, 0.15f));
            Object.Destroy(c3.GetComponent<Collider>());

            // Ground shadow disc
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "TreeShadow";
            shadow.transform.SetParent(tree.transform, false);
            shadow.transform.localPosition = new Vector3(0.3f, 0.01f, 0.2f);
            shadow.transform.localScale = new Vector3(3.5f, 0.005f, 3.5f);
            shadow.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.15f, 0.35f, 0.1f, 0.3f));
            Object.Destroy(shadow.GetComponent<Collider>());
        }
    }
}
