using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Builds the Study Zone (wooden floor, desks, chairs, blackboard, bookshelf).
    /// Located at world X = -60.
    /// </summary>
    public static class StudyZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("StudyZone");
            zone.transform.position = new Vector3(-60f, 0, 0f);

            var woodColor = new Color(0.48f, 0.32f, 0.18f);
            var legColor = new Color(0.2f, 0.2f, 0.2f);
            var seatColor = new Color(0.38f, 0.24f, 0.15f);

            // Ground (Wood panel, 16x16)
            var ground = ZoneFactory.CreateFlat("StudyGround", Vector3.zero, new Vector2(16f, 16f),
                new Color(0.7f, 0.54f, 0.38f));
            ground.transform.SetParent(zone.transform, false);

            // ── Structural Academy Pillars ──
            Vector3[] pillarPositions = {
                new(-7.6f, 1.5f, -7.6f), new(-7.6f, 1.5f, 7.6f),
                new(7.6f, 1.5f, -7.6f), new(7.6f, 1.5f, 7.6f)
            };
            foreach (var pos in pillarPositions)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "AcademyPillar_Collider";
                pillar.transform.SetParent(zone.transform, false);
                pillar.transform.localPosition = pos;
                pillar.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
                pillar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.85f, 0.85f, 0.88f));
            }

            // Half-walls connecting pillars (North, South, West)
            var wallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallN.name = "AcademyWallN_Collider";
            wallN.transform.SetParent(zone.transform, false);
            wallN.transform.localPosition = new Vector3(0, 0.4f, 7.6f);
            wallN.transform.localScale = new Vector3(14.8f, 0.8f, 0.15f);
            wallN.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.78f, 0.64f, 0.5f));

            var wallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallS.name = "AcademyWallS_Collider";
            wallS.transform.SetParent(zone.transform, false);
            wallS.transform.localPosition = new Vector3(0, 0.4f, -7.6f);
            wallS.transform.localScale = new Vector3(14.8f, 0.8f, 0.15f);
            wallS.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.78f, 0.64f, 0.5f));

            var wallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW.name = "AcademyWallW_Collider";
            wallW.transform.SetParent(zone.transform, false);
            wallW.transform.localPosition = new Vector3(-7.6f, 0.4f, 0f);
            wallW.transform.localScale = new Vector3(0.15f, 0.8f, 14.8f);
            wallW.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.78f, 0.64f, 0.5f));

            // ── Desks & Seats (Grid of 3 Rows x 2 Columns) ──
            float[] rowsX = { 0.5f, 3.2f, 5.9f };
            float[] colsZ = { -2.4f, 2.4f };

            foreach (float rx in rowsX)
            {
                foreach (float cz in colsZ)
                {
                    // Student Desk
                    var desk = new GameObject("StudentDesk");
                    desk.transform.SetParent(zone.transform, false);
                    desk.transform.localPosition = new Vector3(rx, 0, cz);

                    var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    top.name = "Tabletop_Collider";
                    top.transform.SetParent(desk.transform, false);
                    top.transform.localPosition = new Vector3(0, 0.65f, 0);
                    top.transform.localScale = new Vector3(1.6f, 0.08f, 0.7f);
                    top.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

                    float[] lx = { -0.7f, 0.7f };
                    float[] lz = { -0.28f, 0.28f };
                    foreach (float xLeg in lx)
                    {
                        foreach (float zLeg in lz)
                        {
                            var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            leg.name = "Leg";
                            leg.transform.SetParent(desk.transform, false);
                            leg.transform.localPosition = new Vector3(xLeg, 0.3f, zLeg);
                            leg.transform.localScale = new Vector3(0.06f, 0.3f, 0.06f);
                            leg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(legColor);
                            Object.Destroy(leg.GetComponent<Collider>());
                        }
                    }

                    // Student Chair (facing West towards blackboard)
                    var chair = new GameObject("StudentChair");
                    chair.transform.SetParent(zone.transform, false);
                    chair.transform.localPosition = new Vector3(rx + 1.1f, 0, cz);

                    var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    seat.name = "Seat_Collider";
                    seat.transform.SetParent(chair.transform, false);
                    seat.transform.localPosition = new Vector3(0, 0.38f, 0);
                    seat.transform.localScale = new Vector3(0.5f, 0.06f, 0.5f);
                    seat.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(seatColor);

                    float[] clx = { -0.2f, 0.2f };
                    float[] clz = { -0.2f, 0.2f };
                    foreach (float cx in clx)
                    {
                        foreach (float czLeg in clz)
                        {
                            var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            leg.name = "ChairLeg";
                            leg.transform.SetParent(chair.transform, false);
                            leg.transform.localPosition = new Vector3(cx, 0.18f, czLeg);
                            leg.transform.localScale = new Vector3(0.04f, 0.18f, 0.04f);
                            leg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(legColor);
                            Object.Destroy(leg.GetComponent<Collider>());
                        }
                    }

                    // Backrest (on East side of the chair)
                    var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    back.name = "Backrest";
                    back.transform.SetParent(chair.transform, false);
                    back.transform.localPosition = new Vector3(0.22f, 0.65f, 0);
                    back.transform.localScale = new Vector3(0.06f, 0.5f, 0.45f);
                    back.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(seatColor);
                    Object.Destroy(back.GetComponent<Collider>());
                }
            }

            // ── Teacher's Area ──
            var teacherDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            teacherDesk.name = "TeacherDesk_Collider";
            teacherDesk.transform.SetParent(zone.transform, false);
            teacherDesk.transform.localPosition = new Vector3(-3.4f, 0.35f, 0);
            teacherDesk.transform.localScale = new Vector3(2.0f, 0.7f, 0.8f);
            teacherDesk.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var teacherChair = new GameObject("TeacherChair");
            teacherChair.transform.SetParent(zone.transform, false);
            teacherChair.transform.localPosition = new Vector3(-4.8f, 0, 0);

            var tseat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tseat.name = "Seat_Collider";
            tseat.transform.SetParent(teacherChair.transform, false);
            tseat.transform.localPosition = new Vector3(0, 0.38f, 0);
            tseat.transform.localScale = new Vector3(0.55f, 0.06f, 0.55f);
            tseat.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(seatColor);

            var tback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tback.name = "Backrest";
            tback.transform.SetParent(teacherChair.transform, false);
            tback.transform.localPosition = new Vector3(-0.25f, 0.7f, 0);
            tback.transform.localScale = new Vector3(0.06f, 0.6f, 0.5f);
            tback.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(seatColor);
            Object.Destroy(tback.GetComponent<Collider>());

            // ── Blackboard Stands & Panel ──
            var frameL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frameL.name = "BoardStand_Collider";
            frameL.transform.SetParent(zone.transform, false);
            frameL.transform.localPosition = new Vector3(-6.2f, 1f, -1.7f);
            frameL.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            frameL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.2f, 0.2f));

            var frameR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frameR.name = "BoardStand_Collider";
            frameR.transform.SetParent(zone.transform, false);
            frameR.transform.localPosition = new Vector3(-6.2f, 1f, 1.7f);
            frameR.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            frameR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.2f, 0.2f));

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Blackboard_Collider";
            board.transform.SetParent(zone.transform, false);
            board.transform.localPosition = new Vector3(-6.2f, 1.6f, 0f);
            board.transform.localScale = new Vector3(0.12f, 1.5f, 3.4f);
            board.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.1f, 0.22f, 0.15f));

            // Blackboard text
            var boardTextObj = new GameObject("BoardText");
            boardTextObj.transform.SetParent(board.transform, false);
            boardTextObj.transform.localPosition = new Vector3(0.07f, 0, 0); // slightly in front
            boardTextObj.transform.localRotation = Quaternion.Euler(0, 90, 0);
            var btmp = boardTextObj.AddComponent<TextMeshPro>();
            btmp.text = "Ranger Academy\n2 + 2 = 5?\nE = mc2";
            btmp.fontSize = 2.4f;
            btmp.fontStyle = FontStyles.Bold;
            btmp.color = Color.white;
            btmp.alignment = TextAlignmentOptions.Center;

            // ── Twin Bookshelves loaded with books ──
            BuildBookshelf(zone, new Vector3(0f, 0f, 6.4f), 0f);
            BuildBookshelf(zone, new Vector3(0f, 0f, -6.4f), 180f);

            // ── Classroom Globes ──
            var globeStand = new GameObject("GlobeStand");
            globeStand.transform.SetParent(zone.transform, false);
            globeStand.transform.localPosition = new Vector3(-6.0f, 0, 6.0f);

            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "GlobeTable_Collider";
            table.transform.SetParent(globeStand.transform, false);
            table.transform.localPosition = new Vector3(0, 0.35f, 0);
            table.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            table.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var globeBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            globeBase.name = "GlobeBase";
            globeBase.transform.SetParent(globeStand.transform, false);
            globeBase.transform.localPosition = new Vector3(0, 0.72f, 0);
            globeBase.transform.localScale = new Vector3(0.2f, 0.02f, 0.2f);
            globeBase.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(legColor);
            Object.Destroy(globeBase.GetComponent<Collider>());

            var globeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            globeSphere.name = "GlobeSphere";
            globeSphere.transform.SetParent(globeStand.transform, false);
            globeSphere.transform.localPosition = new Vector3(0, 0.95f, 0);
            globeSphere.transform.localScale = Vector3.one * 0.35f;
            globeSphere.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.5f, 0.9f));
            Object.Destroy(globeSphere.GetComponent<Collider>());

            // ── Title ──
            var title = new GameObject("StudyTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3.4f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Hoc Vien Tri Thuc";
            tmp.fontSize = 6.4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ── Return Portal ──
            ZoneFactory.CreatePortal("StudyReturnPortal", new Vector3(-54f, 0.05f, 0), 90f,
                new Color(0.4f, 0.32f, 0.22f),
                new Color(0.95f, 0.75f, 0.15f, 0.7f),
                "Ve Sanh Cho");
        }

        private static void BuildBookshelf(GameObject parent, Vector3 localPos, float rotY)
        {
            var shelf = new GameObject("Bookshelf");
            shelf.transform.SetParent(parent.transform, false);
            shelf.transform.localPosition = localPos;
            shelf.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            var woodColor = new Color(0.48f, 0.32f, 0.18f);

            // Back panel
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "ShelfBack_Collider";
            back.transform.SetParent(shelf.transform, false);
            back.transform.localPosition = new Vector3(0, 1.2f, 0.35f);
            back.transform.localScale = new Vector3(3.6f, 2.4f, 0.1f);
            back.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            // Side panels
            var sideL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideL.name = "ShelfSideL_Collider";
            sideL.transform.SetParent(shelf.transform, false);
            sideL.transform.localPosition = new Vector3(-1.75f, 1.2f, 0);
            sideL.transform.localScale = new Vector3(0.1f, 2.4f, 0.8f);
            sideL.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            var sideR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideR.name = "ShelfSideR_Collider";
            sideR.transform.SetParent(shelf.transform, false);
            sideR.transform.localPosition = new Vector3(1.75f, 1.2f, 0);
            sideR.transform.localScale = new Vector3(0.1f, 2.4f, 0.8f);
            sideR.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);

            // Shelves (Bottom, Middle, Top)
            float[] heights = { 0.05f, 0.85f, 1.65f, 2.35f };
            foreach (float h in heights)
            {
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = "ShelfPlate_Collider";
                plate.transform.SetParent(shelf.transform, false);
                plate.transform.localPosition = new Vector3(0, h, 0);
                plate.transform.localScale = new Vector3(3.4f, 0.08f, 0.78f);
                plate.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodColor);
            }

            // Spawn books on shelves
            Color[] bookColors = {
                new(0.85f, 0.2f, 0.2f), new(0.2f, 0.4f, 0.85f),
                new(0.9f, 0.8f, 0.2f), new(0.2f, 0.7f, 0.3f),
                new(0.8f, 0.4f, 0.8f), new(0.15f, 0.15f, 0.18f)
            };
            float[] shelfHeights = { 0.1f, 0.9f, 1.7f };
            for (int s = 0; s < 3; s++)
            {
                float sh = shelfHeights[s];
                int bookCount = 12;
                float startX = -1.5f;
                float stepX = 3.0f / bookCount;

                for (int b = 0; b < bookCount; b++)
                {
                    float bh = Random.Range(0.4f, 0.6f);
                    float bw = Random.Range(0.06f, 0.12f);
                    float bd = Random.Range(0.45f, 0.55f);

                    var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    book.name = "Book";
                    book.transform.SetParent(shelf.transform, false);
                    book.transform.localPosition = new Vector3(startX + b * stepX, sh + bh * 0.5f, -0.05f);
                    book.transform.localScale = new Vector3(bw, bh, bd);
                    book.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(bookColors[Random.Range(0, bookColors.Length)]);
                    Object.Destroy(book.GetComponent<Collider>());
                }
            }
        }
    }
}
