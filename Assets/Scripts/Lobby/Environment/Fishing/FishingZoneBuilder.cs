using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Bến Cảng Nhỏ — Harbor Village with 5 sub-areas:
    /// 1. Sandy Beach (S)          2. Campfire Corner (SE)
    /// 3. Water Area (Center)      4. Dock Boardwalk (N)
    /// 5. Lighthouse (NE)
    /// Multi-level terrain, warm sunset lighting.
    /// Located at world X = +60.
    /// </summary>
    public static class FishingZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("FishingZone");
            zone.transform.position = new Vector3(60f, 0, 0f);

            // ═══ Multi-level ground ═══
            BuildTerrain(zone.transform);

            // ═══ Zone Lighting — sunset golden ═══
            ZoneFactory.CreateZoneLighting(zone.transform, new Color(1f, 0.85f, 0.6f), 1.8f, new Vector3(-0.3f, -1f, 0.2f));

            // ═══ Area 1: Sandy Beach (South) ═══
            BuildBeach(zone.transform);

            // ═══ Area 2: Campfire Corner (SE) ═══
            BuildCampfire(zone.transform);

            // ═══ Area 3: Water Area (Center) ═══
            BuildWaterArea(zone.transform);

            // ═══ Area 4: Dock Boardwalk (North) ═══
            BuildDock(zone.transform);

            // ═══ Area 5: Lighthouse (NE) ═══
            BuildLighthouse(zone.transform);

            // ═══ Boat House (NW) ═══
            BuildBoatHouse(zone.transform);

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
            title.transform.localPosition = new Vector3(0, 3.5f, -5f);
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

        // ── Multi-level terrain ──
        private static void BuildTerrain(Transform parent)
        {
            // Beach level (South, y=0)
            var beachGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beachGround.name = "BeachGround";
            beachGround.transform.SetParent(parent, false);
            beachGround.transform.localPosition = new Vector3(0, -0.05f, -8f);
            beachGround.transform.localScale = new Vector3(28f, 0.1f, 10f);
            beachGround.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.85f, 0.78f, 0.6f));
            Object.Destroy(beachGround.GetComponent<Collider>());
            var bc = beachGround.AddComponent<BoxCollider>();
            bc.size = Vector3.one;

            // Dock level (North, y=0.3 — raised platform)
            var dockGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dockGround.name = "DockGround";
            dockGround.transform.SetParent(parent, false);
            dockGround.transform.localPosition = new Vector3(0, 0.1f, 8f);
            dockGround.transform.localScale = new Vector3(28f, 0.3f, 12f);
            dockGround.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.45f, 0.32f, 0.2f));
            Object.Destroy(dockGround.GetComponent<Collider>());
            var dc = dockGround.AddComponent<BoxCollider>();
            dc.size = Vector3.one;

            // Transition slope
            var slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slope.name = "Slope";
            slope.transform.SetParent(parent, false);
            slope.transform.localPosition = new Vector3(0, 0.05f, 1f);
            slope.transform.localScale = new Vector3(28f, 0.15f, 4f);
            slope.transform.localRotation = Quaternion.Euler(5f, 0, 0);
            slope.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.55f, 0.5f, 0.45f));
            Object.Destroy(slope.GetComponent<Collider>());
            slope.AddComponent<BoxCollider>();
        }

        // ── Area 1: Sandy Beach ──
        private static void BuildBeach(Transform parent)
        {
            var area = new GameObject("Beach");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-5f, 0, -9f);

            // Palm trees (compound shapes)
            BuildPalmTree(area.transform, new Vector3(-3f, 0, 0));
            BuildPalmTree(area.transform, new Vector3(3f, 0, 1f));
            BuildPalmTree(area.transform, new Vector3(8f, 0, -1.5f));

            // Beach towel
            var towel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            towel.name = "BeachTowel";
            towel.transform.SetParent(area.transform, false);
            towel.transform.localPosition = new Vector3(0, 0.02f, 0.5f);
            towel.transform.localScale = new Vector3(1.2f, 0.02f, 0.7f);
            towel.transform.localRotation = Quaternion.Euler(0, 15f, 0);
            towel.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.9f, 0.35f, 0.35f));
            Object.Destroy(towel.GetComponent<Collider>());

            // Shells (small spheres)
            for (int i = 0; i < 6; i++)
            {
                var shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shell.name = "Shell";
                shell.transform.SetParent(area.transform, false);
                shell.transform.localPosition = new Vector3(
                    Random.Range(-5f, 5f), 0.02f, Random.Range(-2f, 2f));
                shell.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);
                shell.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(
                    new Color(0.9f + Random.Range(-0.1f, 0f), 0.85f + Random.Range(-0.1f, 0f), 0.75f));
                Object.Destroy(shell.GetComponent<Collider>());
            }

            // Sandcastle
            BuildSandcastle(area.transform, new Vector3(5f, 0, 0));
        }

        // ── Area 2: Campfire Corner ──
        private static void BuildCampfire(Transform parent)
        {
            var area = new GameObject("CampfireCorner");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(8f, 0, -9f);

            // Fire pit stones (ring)
            Color stoneColor = new Color(0.35f, 0.33f, 0.3f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "FireStone";
                stone.transform.SetParent(area.transform, false);
                stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.5f, 0.08f, Mathf.Sin(angle) * 0.5f);
                stone.transform.localScale = new Vector3(0.2f, 0.15f, 0.2f);
                stone.GetComponent<Renderer>().material = ZoneFactory.StoneMat(stoneColor * Random.Range(0.85f, 1.1f));
                Object.Destroy(stone.GetComponent<Collider>());
            }

            // Fire (glowing orange-red core)
            var fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fire.name = "FireCore";
            fire.transform.SetParent(area.transform, false);
            fire.transform.localPosition = new Vector3(0, 0.15f, 0);
            fire.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            fire.GetComponent<Renderer>().material = ZoneFactory.CreateLitMat(new Color(1f, 0.5f, 0.1f, 0.9f), 0.9f, 0f);
            Object.Destroy(fire.GetComponent<Collider>());

            // Fire light
            var fireLight = new GameObject("FireLight");
            fireLight.transform.SetParent(area.transform, false);
            fireLight.transform.localPosition = new Vector3(0, 0.5f, 0);
            var light = fireLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.6f, 0.2f);
            light.range = 5f;
            light.intensity = 2f;

            // Ember particles
            ZoneParticles.CreateFireEmbers(area.transform, new Vector3(0, 0.2f, 0), new Color(1f, 0.6f, 0.1f));

            // Log seats (3 around fire)
            for (int i = 0; i < 3; i++)
            {
                float angle = (i * 120f + 30f) * Mathf.Deg2Rad;
                float dist = 1.2f;
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.name = "LogSeat_Collider";
                log.transform.SetParent(area.transform, false);
                log.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, 0.15f, Mathf.Sin(angle) * dist);
                log.transform.localScale = new Vector3(0.25f, 0.15f, 0.25f);
                log.transform.localRotation = Quaternion.Euler(90f, i * 40f, 0);
                log.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.4f, 0.25f, 0.12f));
            }

            // Marshmallow sticks (thin cylinders)
            for (int i = 0; i < 2; i++)
            {
                var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick.name = "MarshmallowStick";
                stick.transform.SetParent(area.transform, false);
                stick.transform.localPosition = new Vector3(-0.5f + i * 0.4f, 0.3f, 0.6f);
                stick.transform.localScale = new Vector3(0.02f, 0.4f, 0.02f);
                stick.transform.localRotation = Quaternion.Euler(30f, i * 20f, 0);
                stick.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.35f, 0.15f));
                Object.Destroy(stick.GetComponent<Collider>());
            }
        }

        // ── Area 3: Water Area (irregular pond) ──
        private static void BuildWaterArea(Transform parent)
        {
            var area = new GameObject("WaterArea");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(0, 0, 0);

            // Main water body (overlapping cylinders for irregular shape)
            var water1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water1.name = "Water1";
            water1.transform.SetParent(area.transform, false);
            water1.transform.localPosition = new Vector3(-2f, -0.05f, -1f);
            water1.transform.localScale = new Vector3(8f, 0.02f, 6f);
            water1.GetComponent<Renderer>().material = ZoneFactory.WaterMat(new Color(0.15f, 0.4f, 0.65f, 0.8f));
            Object.Destroy(water1.GetComponent<Collider>());

            var water2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water2.name = "Water2";
            water2.transform.SetParent(area.transform, false);
            water2.transform.localPosition = new Vector3(3f, -0.05f, 0.5f);
            water2.transform.localScale = new Vector3(6f, 0.02f, 5f);
            water2.GetComponent<Renderer>().material = ZoneFactory.WaterMat(new Color(0.12f, 0.35f, 0.6f, 0.78f));
            Object.Destroy(water2.GetComponent<Collider>());

            // Water ripples
            ZoneParticles.CreateWaterRipples(area.transform, new Vector3(0, -0.02f, 0), 4f);

            // Rocky shore edges
            BuildRockyShore(area.transform);

            // Lily pads
            Vector3[] lilyPositions = {
                new(-3f, 0f, -1.5f), new(-1f, 0f, 0.5f), new(2f, 0f, -0.5f),
                new(4f, 0f, 1f), new(-2f, 0f, 0.8f)
            };
            foreach (var pos in lilyPositions)
            {
                var lily = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lily.name = "LilyPad";
                lily.transform.SetParent(area.transform, false);
                lily.transform.localPosition = pos;
                lily.transform.localScale = new Vector3(0.4f, 0.005f, 0.4f);
                lily.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                lily.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.6f, 0.25f));
                Object.Destroy(lily.GetComponent<Collider>());

                // Small flower on some lily pads
                if (Random.value > 0.4f)
                {
                    var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    flower.name = "LilyFlower";
                    flower.transform.SetParent(lily.transform, false);
                    flower.transform.localPosition = new Vector3(0.3f, 0.5f, 0);
                    flower.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
                    flower.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.95f, 0.8f, 0.85f));
                    Object.Destroy(flower.GetComponent<Collider>());
                }
            }

            // Wooden posts in water
            float[] postXs = { -5f, -1f, 3f, 6f };
            foreach (float px in postXs)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "WaterPost";
                post.transform.SetParent(area.transform, false);
                post.transform.localPosition = new Vector3(px, 0.3f, Random.Range(-1f, 1f));
                post.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
                post.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.35f, 0.22f, 0.1f));
                Object.Destroy(post.GetComponent<Collider>());
            }

            // Floating buoys (2)
            var buoy1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            buoy1.name = "Buoy";
            buoy1.transform.SetParent(area.transform, false);
            buoy1.transform.localPosition = new Vector3(-4f, 0.1f, 0);
            buoy1.transform.localScale = Vector3.one * 0.2f;
            buoy1.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.9f, 0.2f, 0.15f));
            Object.Destroy(buoy1.GetComponent<Collider>());

            var buoy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            buoy2.name = "Buoy2";
            buoy2.transform.SetParent(area.transform, false);
            buoy2.transform.localPosition = new Vector3(5f, 0.1f, 0.5f);
            buoy2.transform.localScale = Vector3.one * 0.2f;
            buoy2.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.9f, 0.9f, 0.2f));
            Object.Destroy(buoy2.GetComponent<Collider>());
        }

        // ── Area 4: Dock Boardwalk ──
        private static void BuildDock(Transform parent)
        {
            var area = new GameObject("DockBoardwalk");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-3f, 0.25f, 8f);

            // Plank details on the dock
            for (int i = 0; i < 8; i++)
            {
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = "DockPlank";
                plank.transform.SetParent(area.transform, false);
                plank.transform.localPosition = new Vector3(i * 1.5f - 5f, 0.02f, 0);
                plank.transform.localScale = new Vector3(1.4f, 0.04f, 4f);
                plank.GetComponent<Renderer>().material = ZoneFactory.WoodMat(
                    new Color(0.48f + Random.Range(-0.05f, 0.05f), 0.35f, 0.22f));
                Object.Destroy(plank.GetComponent<Collider>());
            }

            // Fishing spots (3 with stools)
            for (int i = 0; i < 3; i++)
            {
                float x = i * 4f - 4f;
                // Stool
                var stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stool.name = "FishingStool_Collider";
                stool.transform.SetParent(area.transform, false);
                stool.transform.localPosition = new Vector3(x, 0.15f, -1.2f);
                stool.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
                stool.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.42f, 0.3f, 0.18f));

                // Fishing rod (angled cylinder)
                var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rod.name = "FishingRod";
                rod.transform.SetParent(area.transform, false);
                rod.transform.localPosition = new Vector3(x + 0.2f, 0.6f, -0.8f);
                rod.transform.localScale = new Vector3(0.02f, 0.6f, 0.02f);
                rod.transform.localRotation = Quaternion.Euler(30f, 0, 10f);
                rod.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.38f, 0.2f));
                Object.Destroy(rod.GetComponent<Collider>());
            }

            // Rope coils (2)
            for (int i = 0; i < 2; i++)
            {
                var rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rope.name = "RopeCoil";
                rope.transform.SetParent(area.transform, false);
                rope.transform.localPosition = new Vector3(6f + i * 1.2f, 0.12f, 1f);
                rope.transform.localScale = new Vector3(0.3f, 0.08f, 0.3f);
                rope.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.7f, 0.6f, 0.4f));
                Object.Destroy(rope.GetComponent<Collider>());
            }

            // Bait bucket
            var bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucket.name = "BaitBucket_Collider";
            bucket.transform.SetParent(area.transform, false);
            bucket.transform.localPosition = new Vector3(-6f, 0.15f, 0.5f);
            bucket.transform.localScale = new Vector3(0.25f, 0.15f, 0.25f);
            bucket.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.45f, 0.45f, 0.48f));

            // Barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel_Collider";
            barrel.transform.SetParent(area.transform, false);
            barrel.transform.localPosition = new Vector3(-7f, 0.3f, -1f);
            barrel.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            barrel.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.32f, 0.15f));

            // Net hanging (flat plane)
            var net = GameObject.CreatePrimitive(PrimitiveType.Quad);
            net.name = "FishingNet";
            net.transform.SetParent(area.transform, false);
            net.transform.localPosition = new Vector3(8f, 1.2f, 0.5f);
            net.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            net.transform.localRotation = Quaternion.Euler(0, -20f, 0);
            net.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.6f, 0.55f, 0.4f, 0.6f));
            Object.Destroy(net.GetComponent<Collider>());
        }

        // ── Area 5: Lighthouse ──
        private static void BuildLighthouse(Transform parent)
        {
            var lh = new GameObject("Lighthouse");
            lh.transform.SetParent(parent, false);
            lh.transform.localPosition = new Vector3(10f, 0.25f, 10f);

            // Foundation
            var foundation = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            foundation.name = "LHFoundation_Collider";
            foundation.transform.SetParent(lh.transform, false);
            foundation.transform.localPosition = new Vector3(0, 0.1f, 0);
            foundation.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f);
            foundation.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.55f, 0.52f, 0.5f));

            // Tower body (3 tiers, narrowing)
            var tier1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tier1.name = "LHTier1_Collider";
            tier1.transform.SetParent(lh.transform, false);
            tier1.transform.localPosition = new Vector3(0, 1.2f, 0);
            tier1.transform.localScale = new Vector3(1.2f, 1.0f, 1.2f);
            tier1.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.88f, 0.85f, 0.8f));

            var tier2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tier2.name = "LHTier2";
            tier2.transform.SetParent(lh.transform, false);
            tier2.transform.localPosition = new Vector3(0, 2.8f, 0);
            tier2.transform.localScale = new Vector3(0.9f, 0.8f, 0.9f);
            tier2.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.85f, 0.82f, 0.78f));
            Object.Destroy(tier2.GetComponent<Collider>());

            // Red stripe band
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe.name = "RedStripe";
            stripe.transform.SetParent(lh.transform, false);
            stripe.transform.localPosition = new Vector3(0, 2.3f, 0);
            stripe.transform.localScale = new Vector3(1.05f, 0.15f, 1.05f);
            stripe.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.85f, 0.2f, 0.15f));
            Object.Destroy(stripe.GetComponent<Collider>());

            // Observation deck
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            deck.name = "ObsDeck";
            deck.transform.SetParent(lh.transform, false);
            deck.transform.localPosition = new Vector3(0, 3.6f, 0);
            deck.transform.localScale = new Vector3(1.4f, 0.05f, 1.4f);
            deck.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.65f, 0.63f, 0.6f));
            Object.Destroy(deck.GetComponent<Collider>());

            // Lantern room (glass dome)
            var lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lantern.name = "LanternRoom";
            lantern.transform.SetParent(lh.transform, false);
            lantern.transform.localPosition = new Vector3(0, 4.0f, 0);
            lantern.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
            lantern.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(1f, 0.95f, 0.7f, 0.7f));
            Object.Destroy(lantern.GetComponent<Collider>());

            // Beacon light
            var beacon = new GameObject("BeaconLight");
            beacon.transform.SetParent(lh.transform, false);
            beacon.transform.localPosition = new Vector3(0, 4.0f, 0);
            var bl = beacon.AddComponent<Light>();
            bl.type = LightType.Spot;
            bl.color = new Color(1f, 0.95f, 0.7f);
            bl.range = 20f;
            bl.intensity = 3f;
            bl.spotAngle = 50f;
            bl.transform.localRotation = Quaternion.Euler(15f, 0, 0);

            // Top cap
            // Cone not available — use cylinder as cap
            var capObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            capObj.name = "LHCap";
            capObj.transform.SetParent(lh.transform, false);
            capObj.transform.localPosition = new Vector3(0, 4.35f, 0);
            capObj.transform.localScale = new Vector3(0.4f, 0.15f, 0.4f);
            capObj.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.3f, 0.3f, 0.35f));
            Object.Destroy(capObj.GetComponent<Collider>());
        }

        // ── Boat House (NW) ──
        private static void BuildBoatHouse(Transform parent)
        {
            var bh = new GameObject("BoatHouse");
            bh.transform.SetParent(parent, false);
            bh.transform.localPosition = new Vector3(-10f, 0.25f, 9f);

            // Walls
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "BoatHouseWalls_Collider";
            walls.transform.SetParent(bh.transform, false);
            walls.transform.localPosition = new Vector3(0, 0.7f, 0);
            walls.transform.localScale = new Vector3(3f, 1.4f, 2.5f);
            walls.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.35f, 0.2f));

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(bh.transform, false);
            roof.transform.localPosition = new Vector3(0, 1.55f, 0);
            roof.transform.localScale = new Vector3(3.4f, 0.12f, 2.9f);
            roof.transform.localRotation = Quaternion.Euler(0, 0, 6f);
            roof.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.55f, 0.2f, 0.12f));
            Object.Destroy(roof.GetComponent<Collider>());

            // Sign "Bait Shop"
            var signObj = new GameObject("BaitSign");
            signObj.transform.SetParent(bh.transform, false);
            signObj.transform.localPosition = new Vector3(0, 1.8f, -1.3f);
            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = "Cửa Hàng Mồi";
            signTmp.fontSize = 2f;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = new Color(0.2f, 0.15f, 0.1f);
            signObj.AddComponent<BillboardText>();
        }

        // ═══════════════════════ HELPER BUILDERS ═══════════════════════

        private static void BuildPalmTree(Transform parent, Vector3 pos)
        {
            var tree = new GameObject("PalmTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            // Curved trunk (slightly angled cylinder)
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "PalmTrunk_Collider";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0, 1.2f, 0);
            trunk.transform.localScale = new Vector3(0.15f, 1.2f, 0.15f);
            trunk.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-8f, 8f));
            trunk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.38f, 0.22f));

            // Fronds (4-5 elongated flat spheres radiating out)
            int frondCount = Random.Range(4, 6);
            for (int i = 0; i < frondCount; i++)
            {
                float angle = (i * 360f / frondCount + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
                var frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frond.name = "Frond";
                frond.transform.SetParent(tree.transform, false);
                frond.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.6f, 2.3f, Mathf.Sin(angle) * 0.6f);
                frond.transform.localScale = new Vector3(0.15f, 0.03f, 1.0f);
                frond.transform.localRotation = Quaternion.Euler(
                    30f, angle * Mathf.Rad2Deg, 0);
                frond.GetComponent<Renderer>().material = ZoneFactory.CreateMat(
                    new Color(0.2f + Random.Range(0f, 0.1f), 0.6f, 0.2f));
                Object.Destroy(frond.GetComponent<Collider>());
            }

            // Coconuts
            for (int i = 0; i < 2; i++)
            {
                var coconut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coconut.name = "Coconut";
                coconut.transform.SetParent(tree.transform, false);
                coconut.transform.localPosition = new Vector3(
                    Random.Range(-0.15f, 0.15f), 2.15f, Random.Range(-0.15f, 0.15f));
                coconut.transform.localScale = Vector3.one * 0.1f;
                coconut.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.35f, 0.15f));
                Object.Destroy(coconut.GetComponent<Collider>());
            }
        }

        private static void BuildSandcastle(Transform parent, Vector3 pos)
        {
            var castle = new GameObject("Sandcastle");
            castle.transform.SetParent(parent, false);
            castle.transform.localPosition = pos;

            Color sandColor = new Color(0.85f, 0.78f, 0.55f);

            // Base mound
            var mound = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mound.name = "Mound";
            mound.transform.SetParent(castle.transform, false);
            mound.transform.localPosition = new Vector3(0, 0.1f, 0);
            mound.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
            mound.GetComponent<Renderer>().material = ZoneFactory.CreateMat(sandColor);
            Object.Destroy(mound.GetComponent<Collider>());

            // Towers (3 small cylinders)
            Vector3[] towerPos = { new(-0.15f, 0.25f, 0.1f), new(0.15f, 0.25f, 0.1f), new(0, 0.3f, -0.1f) };
            foreach (var tp in towerPos)
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = "Tower";
                tower.transform.SetParent(castle.transform, false);
                tower.transform.localPosition = tp;
                tower.transform.localScale = new Vector3(0.1f, 0.12f + Random.Range(0f, 0.05f), 0.1f);
                tower.GetComponent<Renderer>().material = ZoneFactory.CreateMat(sandColor * 0.95f);
                Object.Destroy(tower.GetComponent<Collider>());
            }
        }

        private static void BuildRockyShore(Transform parent)
        {
            var rockData = new (Vector3 pos, float size)[] {
                (new(-6f, 0.1f, -2f), 0.5f), (new(-5f, 0.15f, 1f), 0.7f),
                (new(6f, 0.12f, -1.5f), 0.6f), (new(7f, 0.1f, 1f), 0.4f),
                (new(-3f, 0.08f, -3f), 0.35f), (new(4f, 0.08f, -3f), 0.3f),
                (new(-7f, 0.1f, 0f), 0.45f), (new(8f, 0.12f, -0.5f), 0.55f),
            };
            foreach (var (pos, size) in rockData)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "ShoreRock";
                rock.transform.SetParent(parent, false);
                rock.transform.localPosition = pos;
                rock.transform.localScale = new Vector3(size, size * 0.5f, size * 0.8f);
                rock.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                rock.GetComponent<Renderer>().material = ZoneFactory.StoneMat(
                    new Color(0.4f + Random.Range(0f, 0.1f), 0.38f, 0.35f));
                Object.Destroy(rock.GetComponent<Collider>());
            }
        }
    }
}
