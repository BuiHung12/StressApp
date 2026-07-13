using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Builds the Fishing Zone (sandy ground, water pond, wooden pier, barrel, rod).
    /// Located at world X = +60.
    /// </summary>
    public static class FishingZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("FishingZone");
            zone.transform.position = new Vector3(60f, 0, 0f);

            // Ground (Sand yellow, 16x16)
            var ground = ZoneFactory.CreateFlat("FishingGround", Vector3.zero, new Vector2(16f, 16f),
                new Color(0.92f, 0.82f, 0.62f));
            ground.transform.SetParent(zone.transform, false);

            // Water pond (flat cylinder)
            var pond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pond.name = "WaterPond";
            pond.transform.SetParent(zone.transform, false);
            pond.transform.localPosition = new Vector3(0, 0.01f, 0f);
            pond.transform.localScale = new Vector3(12f, 0.01f, 12f);
            pond.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.55f, 0.9f, 0.85f));
            Object.Destroy(pond.GetComponent<Collider>());

            // Wooden pier
            var pier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pier.name = "WoodenPier";
            pier.transform.SetParent(zone.transform, false);
            pier.transform.localPosition = new Vector3(0, 0.06f, -4f);
            pier.transform.localScale = new Vector3(2f, 0.1f, 5f);
            pier.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.55f, 0.38f, 0.22f));

            // Support pillars
            for (int i = 0; i < 4; i++)
            {
                float px = (i % 2 == 0) ? -0.9f : 0.9f;
                float pz = (i < 2) ? -2.5f : -5f;
                var pil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pil.name = "PierSupport_Collider";
                pil.transform.SetParent(zone.transform, false);
                pil.transform.localPosition = new Vector3(px, 0.03f, pz);
                pil.transform.localScale = new Vector3(0.12f, 0.03f, 0.12f);
                pil.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.25f, 0.15f));
            }

            // Fish barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "FishBarrel_Collider";
            barrel.transform.SetParent(zone.transform, false);
            barrel.transform.localPosition = new Vector3(-0.6f, 0.3f, -2f);
            barrel.transform.localScale = new Vector3(0.35f, 0.3f, 0.35f);
            barrel.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.35f, 0.2f));

            // Fishing rod
            var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rod.name = "FishingRod";
            rod.transform.SetParent(zone.transform, false);
            rod.transform.localPosition = new Vector3(0.6f, 0.4f, -2.8f);
            rod.transform.localScale = new Vector3(0.04f, 0.8f, 0.04f);
            rod.transform.localRotation = Quaternion.Euler(-45f, 45f, 0f);
            rod.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.8f, 0.7f, 0.5f));
            Object.Destroy(rod.GetComponent<Collider>());

            // ── Title ──
            var title = new GameObject("FishingTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Ho Cau Ca Thu Gian";
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ── Return Portal ──
            ZoneFactory.CreatePortal("FishingReturnPortal", new Vector3(54f, 0.05f, 0), -90f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.2f, 0.5f, 0.95f, 0.7f),
                "Ve Sanh Cho");
        }
    }
}
