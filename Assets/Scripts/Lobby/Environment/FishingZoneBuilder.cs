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
            pond.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.15f, 0.45f, 0.8f, 0.88f));
            Object.Destroy(pond.GetComponent<Collider>());

            // ── Wooden Pier ──
            var pier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pier.name = "WoodenPier";
            pier.transform.SetParent(zone.transform, false);
            pier.transform.localPosition = new Vector3(0, 0.06f, -3.8f);
            pier.transform.localScale = new Vector3(2.2f, 0.1f, 4.4f);
            pier.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.48f, 0.32f, 0.18f));

            // Support pillars
            for (int i = 0; i < 4; i++)
            {
                float px = (i % 2 == 0) ? -1.0f : 1.0f;
                float pz = (i < 2) ? -2.2f : -5.4f;
                var pil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pil.name = "PierSupport_Collider";
                pil.transform.SetParent(zone.transform, false);
                pil.transform.localPosition = new Vector3(px, 0.03f, pz);
                pil.transform.localScale = new Vector3(0.15f, 0.03f, 0.15f);
                pil.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.3f, 0.2f, 0.1f));
            }

            // Wooden Pier Railings
            float[] railZ = { -5.4f, -3.8f, -2.2f };
            foreach (float rz in railZ)
            {
                // Left posts
                var postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                postL.name = "RailPostL";
                postL.transform.SetParent(zone.transform, false);
                postL.transform.localPosition = new Vector3(-1.05f, 0.5f, rz);
                postL.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f);
                postL.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.23f, 0.12f));
                Object.Destroy(postL.GetComponent<Collider>());

                // Right posts
                var postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                postR.name = "RailPostR";
                postR.transform.SetParent(zone.transform, false);
                postR.transform.localPosition = new Vector3(1.05f, 0.5f, rz);
                postR.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f);
                postR.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.23f, 0.12f));
                Object.Destroy(postR.GetComponent<Collider>());
            }
            // Horizontal rails (Left & Right sides)
            float[] railHeights = { 0.45f, 0.85f };
            foreach (float rh in railHeights)
            {
                var sideRailL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sideRailL.name = "SideRailL";
                sideRailL.transform.SetParent(zone.transform, false);
                sideRailL.transform.localPosition = new Vector3(-1.05f, rh, -3.8f);
                sideRailL.transform.localScale = new Vector3(0.05f, 0.05f, 3.2f);
                sideRailL.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.23f, 0.12f));
                Object.Destroy(sideRailL.GetComponent<Collider>());

                var sideRailR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sideRailR.name = "SideRailR";
                sideRailR.transform.SetParent(zone.transform, false);
                sideRailR.transform.localPosition = new Vector3(1.05f, rh, -3.8f);
                sideRailR.transform.localScale = new Vector3(0.05f, 0.05f, 3.2f);
                sideRailR.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.23f, 0.12f));
                Object.Destroy(sideRailR.GetComponent<Collider>());
            }

            // Fisherman's Bench on the Pier
            var bench = new GameObject("FishermanBench");
            bench.transform.SetParent(zone.transform, false);
            bench.transform.localPosition = new Vector3(0.7f, 0.11f, -4.2f);
            bench.transform.localRotation = Quaternion.Euler(0, -90, 0);

            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat_Collider";
            seat.transform.SetParent(bench.transform, false);
            seat.transform.localPosition = new Vector3(0, 0.2f, 0);
            seat.transform.localScale = new Vector3(1.2f, 0.08f, 0.4f);
            seat.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.4f, 0.25f, 0.15f));

            var legL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legL.name = "LegL";
            legL.transform.SetParent(bench.transform, false);
            legL.transform.localPosition = new Vector3(-0.5f, 0.1f, 0);
            legL.transform.localScale = new Vector3(0.08f, 0.2f, 0.3f);
            legL.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.25f, 0.15f, 0.1f));

            var legR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legR.name = "LegR";
            legR.transform.SetParent(bench.transform, false);
            legR.transform.localPosition = new Vector3(0.5f, 0.1f, 0);
            legR.transform.localScale = new Vector3(0.08f, 0.2f, 0.3f);
            legR.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.25f, 0.15f, 0.1f));

            // Fish barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "FishBarrel_Collider";
            barrel.transform.SetParent(zone.transform, false);
            barrel.transform.localPosition = new Vector3(-0.7f, 0.3f, -2.2f);
            barrel.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            barrel.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.5f, 0.33f, 0.18f));

            // Fishing rod
            var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rod.name = "FishingRod";
            rod.transform.SetParent(zone.transform, false);
            rod.transform.localPosition = new Vector3(0.7f, 0.45f, -2.6f);
            rod.transform.localScale = new Vector3(0.04f, 0.9f, 0.04f);
            rod.transform.localRotation = Quaternion.Euler(-50f, 35f, 0f);
            rod.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.75f, 0.65f, 0.45f));
            Object.Destroy(rod.GetComponent<Collider>());

            // ── Warm Street Lamp ──
            var lampPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lampPost.name = "LampPost_Collider";
            lampPost.transform.SetParent(zone.transform, false);
            lampPost.transform.localPosition = new Vector3(-1.4f, 1.1f, -5.6f);
            lampPost.transform.localScale = new Vector3(0.08f, 1.1f, 0.08f);
            lampPost.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.18f, 0.2f, 0.22f));

            var lampArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lampArm.name = "LampArm";
            lampArm.transform.SetParent(zone.transform, false);
            lampArm.transform.localPosition = new Vector3(-1.2f, 2.15f, -5.6f);
            lampArm.transform.localScale = new Vector3(0.4f, 0.06f, 0.06f);
            lampArm.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.18f, 0.2f, 0.22f));
            Object.Destroy(lampArm.GetComponent<Collider>());

            var lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lantern.name = "Lantern";
            lantern.transform.SetParent(zone.transform, false);
            lantern.transform.localPosition = new Vector3(-1.0f, 2.0f, -5.6f);
            lantern.transform.localScale = new Vector3(0.18f, 0.24f, 0.18f);
            lantern.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(1f, 0.92f, 0.68f));
            Object.Destroy(lantern.GetComponent<Collider>());

            // Lamp light glow
            var lampLightObj = new GameObject("StreetLampLight");
            lampLightObj.transform.SetParent(lantern.transform, false);
            lampLightObj.transform.localPosition = Vector3.zero;
            var lampLight = lampLightObj.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.color = new Color(1f, 0.9f, 0.65f);
            lampLight.range = 7f;
            lampLight.intensity = 1.8f;

            // ── Cozy Campfire on the Sand ──
            var campfire = new GameObject("CozyCampfire");
            campfire.transform.SetParent(zone.transform, false);
            campfire.transform.localPosition = new Vector3(-5.2f, 0.02f, -5.2f);

            // Ring of stones
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                float r = 0.5f;
                var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "CampfireStone";
                stone.transform.SetParent(campfire.transform, false);
                stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * r, 0.06f, Mathf.Sin(angle) * r);
                stone.transform.localScale = new Vector3(0.2f, 0.12f, 0.2f);
                stone.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.35f, 0.35f, 0.37f));
                Object.Destroy(stone.GetComponent<Collider>());
            }

            // Crossed wooden logs
            for (int i = 0; i < 3; i++)
            {
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.name = "CampfireLog";
                log.transform.SetParent(campfire.transform, false);
                log.transform.localPosition = new Vector3(0, 0.08f, 0);
                log.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
                log.transform.localRotation = Quaternion.Euler(90f, i * 60f, 15f);
                log.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.32f, 0.2f, 0.1f));
                Object.Destroy(log.GetComponent<Collider>());
            }

            // Gilded fire glow core
            var fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fire.name = "CampfireGlow";
            fire.transform.SetParent(campfire.transform, false);
            fire.transform.localPosition = new Vector3(0, 0.25f, 0);
            fire.transform.localScale = new Vector3(0.32f, 0.42f, 0.32f);
            fire.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(1.0f, 0.42f, 0.0f, 0.85f));
            Object.Destroy(fire.GetComponent<Collider>());

            var campfireLightObj = new GameObject("CampfireLight");
            campfireLightObj.transform.SetParent(fire.transform, false);
            campfireLightObj.transform.localPosition = Vector3.zero;
            var fireLight = campfireLightObj.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1.0f, 0.55f, 0.1f);
            fireLight.range = 6.2f;
            fireLight.intensity = 2.4f;

            // ── Decorative Shore Rocks ──
            var rocksData = new (Vector3 pos, Vector3 scale)[]
            {
                (new(4.5f, 0.2f, 3.8f), new(1.4f, 0.7f, 1.2f)),
                (new(-4.2f, 0.15f, 4.0f), new(1.1f, 0.5f, 1.1f)),
                (new(-5.4f, 0.3f, -2.2f), new(1.6f, 0.9f, 1.4f)),
                (new(5.2f, 0.15f, -2.5f), new(0.9f, 0.5f, 0.9f)),
                (new(0f, 0.1f, 5.7f), new(1.2f, 0.4f, 1.2f))
            };
            foreach (var (pos, scale) in rocksData)
            {
                var rk = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rk.name = "ShoreRock_Collider";
                rk.transform.SetParent(zone.transform, false);
                rk.transform.localPosition = pos;
                rk.transform.localScale = scale;
                rk.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.38f, 0.38f, 0.4f));
            }

            // ── Lily Pads and Lotuses ──
            var liliesData = new (Vector3 pos, float size, bool hasLotus)[]
            {
                (new(2.4f, 0.02f, 1.6f), 0.9f, true),
                (new(-3.4f, 0.02f, 0.6f), 0.75f, false),
                (new(-2.0f, 0.02f, 3.4f), 0.85f, true),
                (new(0.5f, 0.02f, -2.0f), 0.7f, false),
                (new(-1.2f, 0.02f, -1.8f), 0.8f, false)
            };
            foreach (var (pos, size, hasLotus) in liliesData)
            {
                var lily = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lily.name = "LilyPad";
                lily.transform.SetParent(zone.transform, false);
                lily.transform.localPosition = pos;
                lily.transform.localScale = new Vector3(size, 0.005f, size);
                lily.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.18f, 0.46f, 0.22f));
                Object.Destroy(lily.GetComponent<Collider>());

                if (hasLotus)
                {
                    var lotus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    lotus.name = "LotusFlower";
                    lotus.transform.SetParent(lily.transform, false);
                    lotus.transform.localPosition = new Vector3(0, 4.0f, 0); // local to flat scale
                    lotus.transform.localScale = new Vector3(0.3f, 12f, 0.3f);
                    lotus.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.96f, 0.45f, 0.62f));
                    Object.Destroy(lotus.GetComponent<Collider>());
                }
            }

            // ── Cattails / Reeds Clusters ──
            var reedRoots = new Vector3[]
            {
                new(-5.0f, 0, -3.2f),
                new(5.0f, 0, 2.6f),
                (new(-2.6f, 0, 5.1f))
            };
            foreach (var root in reedRoots)
            {
                var cluster = new GameObject("ReedCluster");
                cluster.transform.SetParent(zone.transform, false);
                cluster.transform.localPosition = root;

                int stalks = Random.Range(3, 5);
                for (int s = 0; s < stalks; s++)
                {
                    float ox = (s - stalks * 0.5f) * 0.15f;
                    float oz = Random.Range(-0.1f, 0.1f);
                    float h = Random.Range(1.1f, 1.5f);

                    var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stem.name = "ReedStem";
                    stem.transform.SetParent(cluster.transform, false);
                    stem.transform.localPosition = new Vector3(ox, h * 0.5f, oz);
                    stem.transform.localScale = new Vector3(0.04f, h * 0.5f, 0.04f);
                    stem.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                    stem.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.24f, 0.46f, 0.22f));
                    Object.Destroy(stem.GetComponent<Collider>());

                    var top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    top.name = "ReedTop";
                    top.transform.SetParent(stem.transform, false);
                    top.transform.localPosition = new Vector3(0, 0.9f, 0); // on top of stem
                    top.transform.localScale = new Vector3(1.6f, 0.12f, 1.6f);
                    top.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.42f, 0.26f, 0.15f));
                    Object.Destroy(top.GetComponent<Collider>());
                }
            }

            // ── Title ──
            var title = new GameObject("FishingTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 3.4f, -5f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Khu Cau Ca Thu Gian";
            tmp.fontSize = 6.4f;
            tmp.fontStyle = FontStyles.Bold;
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
