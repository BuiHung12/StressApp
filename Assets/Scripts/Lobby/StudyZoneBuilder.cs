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

            // Ground (Wood panel, 16x16)
            var ground = ZoneFactory.CreateFlat("StudyGround", Vector3.zero, new Vector2(16f, 16f),
                new Color(0.72f, 0.58f, 0.42f));
            ground.transform.SetParent(zone.transform, false);

            // Desks & seats (3 rows)
            for (int i = 0; i < 3; i++)
            {
                float dz = 3f - i * 2.8f;

                var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                desk.name = "StudyDesk_Collider";
                desk.transform.SetParent(zone.transform, false);
                desk.transform.localPosition = new Vector3(1.5f, 0.35f, dz);
                desk.transform.localScale = new Vector3(1.8f, 0.7f, 0.6f);
                desk.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.35f, 0.22f));

                var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seat.name = "StudySeat_Collider";
                seat.transform.SetParent(zone.transform, false);
                seat.transform.localPosition = new Vector3(2.8f, 0.25f, dz);
                seat.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                seat.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.22f, 0.15f));
            }

            // Teacher desk
            var teacherDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            teacherDesk.name = "TeacherDesk_Collider";
            teacherDesk.transform.SetParent(zone.transform, false);
            teacherDesk.transform.localPosition = new Vector3(-2.8f, 0.35f, 0);
            teacherDesk.transform.localScale = new Vector3(2f, 0.7f, 0.8f);
            teacherDesk.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.45f, 0.3f, 0.18f));

            // Blackboard stands
            var frameL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frameL.name = "BoardStand_Collider";
            frameL.transform.SetParent(zone.transform, false);
            frameL.transform.localPosition = new Vector3(-5f, 1f, -1.5f);
            frameL.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            frameL.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.2f, 0.2f));

            var frameR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frameR.name = "BoardStand_Collider";
            frameR.transform.SetParent(zone.transform, false);
            frameR.transform.localPosition = new Vector3(-5f, 1f, 1.5f);
            frameR.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            frameR.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.2f, 0.2f));

            // Blackboard panel
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Blackboard_Collider";
            board.transform.SetParent(zone.transform, false);
            board.transform.localPosition = new Vector3(-5f, 1.6f, 0f);
            board.transform.localScale = new Vector3(0.1f, 1.4f, 2.8f);
            board.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.12f, 0.25f, 0.18f));

            // Bookshelf
            var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "Bookshelf_Collider";
            shelf.transform.SetParent(zone.transform, false);
            shelf.transform.localPosition = new Vector3(0f, 1.2f, 7f);
            shelf.transform.localScale = new Vector3(4f, 2.4f, 0.8f);
            shelf.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.35f, 0.2f));

            // Books on shelf
            Color[] bookColors = { Color.red, Color.blue, Color.yellow, Color.green };
            for (int k = 0; k < 6; k++)
            {
                var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                book.name = "Book";
                book.transform.SetParent(shelf.transform, false);
                book.transform.localPosition = new Vector3(-0.35f + k * 0.15f, 0.1f, 0);
                book.transform.localScale = new Vector3(0.08f, 0.35f, 0.6f);
                book.GetComponent<Renderer>().material = ZoneFactory.CreateMat(bookColors[k % bookColors.Length]);
                Object.Destroy(book.GetComponent<Collider>());
            }

            // ── Title ──
            var title = new GameObject("StudyTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Hoc Vien Tri Thuc";
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ── Return Portal ──
            ZoneFactory.CreatePortal("StudyReturnPortal", new Vector3(-54f, 0.05f, 0), 90f,
                new Color(0.4f, 0.32f, 0.22f),
                new Color(0.95f, 0.75f, 0.15f, 0.7f),
                "Ve Sanh Cho");
        }
    }
}
