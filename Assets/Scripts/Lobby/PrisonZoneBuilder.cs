using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Builds the Prison Zone (concrete ground, dark walls, iron bars, cells, visiting area).
    /// Located at world Z = -60.
    /// </summary>
    public static class PrisonZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("PrisonZone");
            zone.transform.position = new Vector3(0, 0, -60f);

            // Ground (Concrete gray, 16x16)
            var ground = ZoneFactory.CreateFlat("PrisonGround", Vector3.zero, new Vector2(16f, 16f),
                new Color(0.3f, 0.3f, 0.33f));
            ground.transform.SetParent(zone.transform, false);

            // High dark brick walls
            Color wallColor = new Color(0.18f, 0.18f, 0.2f);
            float limit = 8f;

            // North wall
            var wallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallN.name = "WallN_Collider";
            wallN.transform.SetParent(zone.transform, false);
            wallN.transform.localPosition = new Vector3(0, 1.5f, limit);
            wallN.transform.localScale = new Vector3(16f, 3f, 0.5f);
            wallN.GetComponent<Renderer>().material = ZoneFactory.CreateMat(wallColor);

            // South wall
            var wallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallS.name = "WallS_Collider";
            wallS.transform.SetParent(zone.transform, false);
            wallS.transform.localPosition = new Vector3(0, 1.5f, -limit);
            wallS.transform.localScale = new Vector3(16f, 3f, 0.5f);
            wallS.GetComponent<Renderer>().material = ZoneFactory.CreateMat(wallColor);

            // East wall
            var wallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE.name = "WallE_Collider";
            wallE.transform.SetParent(zone.transform, false);
            wallE.transform.localPosition = new Vector3(limit, 1.5f, 0);
            wallE.transform.localScale = new Vector3(0.5f, 3f, 16f);
            wallE.GetComponent<Renderer>().material = ZoneFactory.CreateMat(wallColor);

            // West wall
            var wallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW.name = "WallW_Collider";
            wallW.transform.SetParent(zone.transform, false);
            wallW.transform.localPosition = new Vector3(-limit, 1.5f, 0);
            wallW.transform.localScale = new Vector3(0.5f, 3f, 16f);
            wallW.GetComponent<Renderer>().material = ZoneFactory.CreateMat(wallColor);

            // ── Iron Bars Partition (local z = 0) ──
            Color barsColor = new Color(0.2f, 0.2f, 0.22f);
            for (float x = -6f; x <= 6f; x += 0.4f)
            {
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bar.name = "JailBar_Collider";
                bar.transform.SetParent(zone.transform, false);
                bar.transform.localPosition = new Vector3(x, 1.5f, 0);
                bar.transform.localScale = new Vector3(0.06f, 1.5f, 0.06f);
                bar.GetComponent<Renderer>().material = ZoneFactory.CreateMat(barsColor);
            }

            // Cell dividers
            var div1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            div1.name = "CellDivider_Collider";
            div1.transform.SetParent(zone.transform, false);
            div1.transform.localPosition = new Vector3(-2f, 1.5f, -2f);
            div1.transform.localScale = new Vector3(0.2f, 3f, 4f);
            div1.GetComponent<Renderer>().material = ZoneFactory.CreateMat(wallColor * 0.8f);

            var div2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            div2.name = "CellDivider_Collider";
            div2.transform.SetParent(zone.transform, false);
            div2.transform.localPosition = new Vector3(2f, 1.5f, -2f);
            div2.transform.localScale = new Vector3(0.2f, 3f, 4f);
            div2.GetComponent<Renderer>().material = ZoneFactory.CreateMat(wallColor * 0.8f);

            // Prison bed
            var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "PrisonBed_Collider";
            bed.transform.SetParent(zone.transform, false);
            bed.transform.localPosition = new Vector3(0, 0.25f, -3f);
            bed.transform.localScale = new Vector3(1.8f, 0.3f, 0.8f);
            bed.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.35f, 0.2f));

            // ── Visiting Area (desk in front of bars) ──
            var visitDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visitDesk.name = "VisitingDesk_Collider";
            visitDesk.transform.SetParent(zone.transform, false);
            visitDesk.transform.localPosition = new Vector3(0, 0.45f, 0.5f);
            visitDesk.transform.localScale = new Vector3(6f, 0.9f, 0.6f);
            visitDesk.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.4f, 0.25f, 0.15f));

            // ── Title ──
            var title = new GameObject("PrisonTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Nha Tu Thanh Pho";
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ── Return Portal ──
            ZoneFactory.CreatePortal("PrisonReturnPortal", new Vector3(0, 0.05f, -66f), 0f,
                new Color(0.25f, 0.25f, 0.3f),
                new Color(0.85f, 0.15f, 0.15f, 0.7f),
                "Ve Sanh Cho");
        }
    }
}
