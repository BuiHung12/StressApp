using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Bến Cảng Nhỏ — Harbor Village (30×30, centered at 60,0,0).
    /// 3-level terrain layout:
    ///   South (y=0):   Sandy Beach + Campfire Corner
    ///   Center (slope): Stone transition
    ///   North (y=0.3): Dock Boardwalk + Boat House + Lighthouse
    /// </summary>
    public static partial class FishingZoneBuilder
    {
        // Zone bounds: -15 to +15 in X and Z (local space)
        // Beach:  Z[-14, -2], y=0
        // Slope:  Z[-2, 2],  y transitions
        // Dock:   Z[2, 14],  y=0.3

        static readonly Color SandColor  = new(0.85f, 0.78f, 0.6f);
        static readonly Color DockWood   = new(0.45f, 0.32f, 0.2f);
        static readonly Color WetStone   = new(0.55f, 0.5f, 0.45f);
        static readonly Color WaterBlue  = new(0.15f, 0.4f, 0.65f, 0.8f);

        public static void Build()
        {
            var zone = new GameObject("FishingZone");
            zone.transform.position = new Vector3(60f, 0, 0f);

            // ═══ 3-Level Terrain ═══
            BuildTerrain(zone.transform);

            // ═══ Zone Lighting — sunset golden ═══
            ZoneFactory.CreateZoneLighting(zone.transform, new Color(1f, 0.85f, 0.6f), 1.8f, new Vector3(-0.3f, -1f, 0.2f));

            // ═══ South: Sandy Beach (left half) ═══
            BuildBeach(zone.transform);

            // ═══ South: Campfire Corner (right half) ═══
            BuildCampfire(zone.transform);

            // ═══ Center: Water Area ═══
            BuildWaterArea(zone.transform);

            // ═══ North: Dock Boardwalk (center-left) ═══
            BuildDock(zone.transform);

            // ═══ North-West: Boat House ═══
            BuildBoatHouse(zone.transform);

            // ═══ North-East: Lighthouse ═══
            BuildLighthouse(zone.transform);

            // ═══ Firefly particles ═══
            ZoneParticles.CreateFloatingParticles(
                zone.transform,
                new Color(1f, 0.9f, 0.4f, 0.8f),
                35,
                new Vector3(24f, 2f, 24f),
                "Fireflies");

            // ═══ Title ═══
            var title = new GameObject("FishingTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3.5f, -14f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Bến Cảng Nhỏ";
            tmp.fontSize = 6.4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ═══ Return Portal ═══
            ZoneFactory.CreatePortal("FishingReturnPortal", new Vector3(54f, 0.05f, 0), -90f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.2f, 0.5f, 0.95f, 0.7f),
                "Về Sảnh Chờ");
        }

        // ────────────────────────────────────────────────────────
        //  3-LEVEL TERRAIN & OCEAN SHORELINE
        // ────────────────────────────────────────────────────────
        private static void BuildTerrain(Transform parent)
        {
            // Sandy Beach Ground (South, Z[-15,0], y=0)
            var beach = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beach.name = "BeachGround";
            beach.transform.SetParent(parent, false);
            beach.transform.localPosition = new Vector3(0, -0.05f, -7.5f);
            beach.transform.localScale = new Vector3(30f, 0.1f, 15f);
            beach.GetComponent<Renderer>().material = ZoneFactory.CreateMat(SandColor);
            Object.Destroy(beach.GetComponent<Collider>());
            beach.AddComponent<BoxCollider>().size = Vector3.one;

            // Shoreline Stone Transition (Z[-1,1], y=0)
            var slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slope.name = "SlopeTransition";
            slope.transform.SetParent(parent, false);
            slope.transform.localPosition = new Vector3(0, -0.05f, 0);
            slope.transform.localScale = new Vector3(30f, 0.1f, 2f);
            slope.GetComponent<Renderer>().material = ZoneFactory.StoneMat(WetStone);
            Object.Destroy(slope.GetComponent<Collider>());
            slope.AddComponent<BoxCollider>().size = Vector3.one;
        }

        // ────────────────────────────────────────────────────────
        //  OCEAN BAY & SHORELINE (North: Z[0,14])
        // ────────────────────────────────────────────────────────
        private static void BuildWaterArea(Transform parent)
        {
            var area = new GameObject("WaterArea");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(0, 0, 7f);

            // Large Translucent Ocean Surface
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ocean.name = "OceanSurface";
            ocean.transform.SetParent(area.transform, false);
            ocean.transform.localPosition = new Vector3(0, 0.01f, 0);
            ocean.transform.localScale = new Vector3(30f, 0.02f, 14f);
            ocean.GetComponent<Renderer>().material = ZoneFactory.WaterMat(new Color(0.12f, 0.45f, 0.75f, 0.75f));
            Object.Destroy(ocean.GetComponent<Collider>());

            // Ocean bed below water
            var seabed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seabed.name = "OceanBed";
            seabed.transform.SetParent(area.transform, false);
            seabed.transform.localPosition = new Vector3(0, -0.3f, 0);
            seabed.transform.localScale = new Vector3(30f, 0.1f, 14f);
            seabed.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.18f, 0.35f, 0.45f));
            Object.Destroy(seabed.GetComponent<Collider>());

            ZoneParticles.CreateWaterRipples(area.transform, new Vector3(0, 0.02f, 0), 8f);

            // Shore rocks (fixed positions along water edge)
            var rocks = new (Vector3 pos, float size)[] {
                (new(-12f, 0.1f, -6.5f), 0.8f), (new(-9f, 0.12f, -6f), 0.6f),
                (new(9f, 0.1f, -6f), 0.7f),     (new(12f, 0.08f, -6.5f), 0.9f),
                (new(-4f, 0.08f, -6.2f), 0.45f),(new(4f, 0.1f, -6.2f), 0.5f),
            };
            foreach (var (pos, size) in rocks)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "ShoreRock";
                rock.transform.SetParent(area.transform, false);
                rock.transform.localPosition = pos;
                rock.transform.localScale = new Vector3(size, size * 0.5f, size * 0.8f);
                rock.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.42f, 0.38f, 0.35f));
                Object.Destroy(rock.GetComponent<Collider>());
            }

            // Lily pads & Floating Seaweed
            Vector3[] lilyPos = {
                new(-6f, 0.02f, -3f), new(-2f, 0.02f, -1f), new(3f, 0.02f, -2f),
                new(7f, 0.02f, -4f), new(-8f, 0.02f, 2f)
            };
            foreach (var lp in lilyPos)
            {
                var lily = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lily.name = "LilyPad";
                lily.transform.SetParent(area.transform, false);
                lily.transform.localPosition = lp;
                lily.transform.localScale = new Vector3(0.5f, 0.005f, 0.5f);
                lily.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.6f, 0.25f));
                Object.Destroy(lily.GetComponent<Collider>());
            }
        }

        // ────────────────────────────────────────────────────────
        //  SANDY BEACH (South-West: X[-14,-1], Z[-14,-2])
        // ────────────────────────────────────────────────────────
        private static void BuildBeach(Transform parent)
        {
            var area = new GameObject("Beach");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-7f, 0, -8f);

            // Area sign
            ZoneFactory.CreateAreaSign(area.transform, new Vector3(1.5f, 0.6f, 1.5f), "Bãi Cát");

            // Coastal Palm trees
            BuildPalmTree(area.transform, new Vector3(-5f, 0, 1f));
            BuildPalmTree(area.transform, new Vector3(4f, 0, 3f));
            BuildPalmTree(area.transform, new Vector3(5f, 0, -3f));

            // Beach Sun Umbrella & Lounger
            var umbrellaPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            umbrellaPole.transform.SetParent(area.transform, false);
            umbrellaPole.transform.localPosition = new Vector3(-1f, 1.0f, 0);
            umbrellaPole.transform.localScale = new Vector3(0.06f, 1.0f, 0.06f);
            umbrellaPole.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.4f, 0.3f));
            Object.Destroy(umbrellaPole.GetComponent<Collider>());

            var umbrellaTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            umbrellaTop.transform.SetParent(area.transform, false);
            umbrellaTop.transform.localPosition = new Vector3(-1f, 1.9f, 0);
            umbrellaTop.transform.localScale = new Vector3(2.2f, 0.05f, 2.2f);
            umbrellaTop.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.95f, 0.25f, 0.25f));
            Object.Destroy(umbrellaTop.GetComponent<Collider>());

            // Beach towel & Lounger
            var towel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            towel.name = "BeachTowel";
            towel.transform.SetParent(area.transform, false);
            towel.transform.localPosition = new Vector3(-1.2f, 0.02f, 0);
            towel.transform.localScale = new Vector3(1.4f, 0.02f, 0.8f);
            towel.transform.localRotation = Quaternion.Euler(0, 15f, 0);
            towel.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.6f, 0.9f));
            Object.Destroy(towel.GetComponent<Collider>());

            // Shells along beach
            Vector3[] shellPos = {
                new(-4f, 0.02f, -1f), new(-1f, 0.02f, 2.5f), new(1f, 0.02f, -0.5f),
                new(3f, 0.02f, 1.5f), new(5f, 0.02f, -1.5f)
            };
            foreach (var sp in shellPos)
            {
                var shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shell.name = "Shell";
                shell.transform.SetParent(area.transform, false);
                shell.transform.localPosition = sp;
                shell.transform.localScale = Vector3.one * 0.08f;
                shell.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.9f, 0.85f, 0.78f));
                Object.Destroy(shell.GetComponent<Collider>());
            }

            // Sandcastle
            BuildSandcastle(area.transform, new Vector3(2f, 0, -2f));
        }

        // ────────────────────────────────────────────────────────
        //  CAMPFIRE & BBQ CORNER (South-East: X[1,14], Z[-14,-2])
        // ────────────────────────────────────────────────────────
        private static void BuildCampfire(Transform parent)
        {
            var area = new GameObject("CampfireCorner");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(7f, 0, -8f);

            // Area sign
            ZoneFactory.CreateAreaSign(area.transform, new Vector3(-1.5f, 0.6f, 1.5f), "Lửa Trại BBQ");

            // Stone circle floor
            var fireFloor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fireFloor.name = "CampfireFloor";
            fireFloor.transform.SetParent(area.transform, false);
            fireFloor.transform.localPosition = new Vector3(0, 0.01f, 0);
            fireFloor.transform.localScale = new Vector3(4.2f, 0.01f, 4.2f);
            fireFloor.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.4f, 0.38f, 0.35f));
            Object.Destroy(fireFloor.GetComponent<Collider>());

            // Fire pit stones (ring of 8)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "FireStone";
                stone.transform.SetParent(area.transform, false);
                stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.6f, 0.08f, Mathf.Sin(angle) * 0.6f);
                stone.transform.localScale = new Vector3(0.22f, 0.16f, 0.22f);
                stone.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.35f, 0.33f, 0.3f));
                Object.Destroy(stone.GetComponent<Collider>());
            }

            // Glowing Fire core
            var fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fire.name = "FireCore";
            fire.transform.SetParent(area.transform, false);
            fire.transform.localPosition = new Vector3(0, 0.18f, 0);
            fire.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
            fire.GetComponent<Renderer>().material = ZoneFactory.CreateLitMat(new Color(1f, 0.5f, 0.1f, 0.9f), 0.9f, 0f);
            Object.Destroy(fire.GetComponent<Collider>());

            // Roasting fish on skewers over fire
            for (int i = 0; i < 2; i++)
            {
                float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                var skewer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                skewer.name = "FishSkewer";
                skewer.transform.SetParent(area.transform, false);
                skewer.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.25f, 0.3f, Mathf.Sin(angle) * 0.25f);
                skewer.transform.localScale = new Vector3(0.02f, 0.35f, 0.02f);
                skewer.transform.localRotation = Quaternion.Euler(45f, i * 90f, 0);
                skewer.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.25f, 0.1f));
                Object.Destroy(skewer.GetComponent<Collider>());

                var fish = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fish.transform.SetParent(skewer.transform, false);
                fish.transform.localPosition = new Vector3(0, 0.2f, 0);
                fish.transform.localScale = new Vector3(3f, 0.3f, 1.8f);
                fish.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.85f, 0.45f, 0.15f));
                Object.Destroy(fish.GetComponent<Collider>());
            }

            // Warm Fire Light
            var fireLight = new GameObject("FireLight");
            fireLight.transform.SetParent(area.transform, false);
            fireLight.transform.localPosition = new Vector3(0, 0.6f, 0);
            var light = fireLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.6f, 0.2f);
            light.range = 6f;
            light.intensity = 2.5f;

            ZoneParticles.CreateFireEmbers(area.transform, new Vector3(0, 0.2f, 0), new Color(1f, 0.6f, 0.1f));

            // Log seats around campfire
            for (int i = 0; i < 3; i++)
            {
                float angle = (i * 120f + 30f) * Mathf.Deg2Rad;
                float dist = 1.4f;
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.name = "LogSeat_Collider";
                log.transform.SetParent(area.transform, false);
                log.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, 0.15f, Mathf.Sin(angle) * dist);
                log.transform.localScale = new Vector3(0.28f, 0.16f, 0.28f);
                log.transform.localRotation = Quaternion.Euler(90f, i * 40f, 0);
                log.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.25f, 0.12f));
            }
        }

        // ────────────────────────────────────────────────────────
        //  DOCK BOARDWALK (North-Center: X[-6,6], Z[3,14])
        //  Center: (0, 0.3, 8)
        // ────────────────────────────────────────────────────────

}
}
