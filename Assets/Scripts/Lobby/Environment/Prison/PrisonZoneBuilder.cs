using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Nhà Giam Ranger City — Prison compound with 6 sub-areas:
    /// 1. Cell Block (NE)          2. Exercise Yard (SW)
    /// 3. Mess Hall (SE)           4. Guard Tower (NW)
    /// 5. Warden Office (W)        6. Solitary Confinement (S)
    /// L-shaped layout, harsh lighting, oppressive atmosphere.
    /// Located at world Z = -60.
    /// </summary>
    public static class PrisonZoneBuilder
    {
        public static void Build()
        {
            var zone = new GameObject("PrisonZone");
            zone.transform.position = new Vector3(0, 0, -60f);

            // ═══ Ground ═══
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "PrisonGround";
            ground.transform.SetParent(zone.transform, false);
            ground.transform.localPosition = new Vector3(0, -0.05f, 0);
            ground.transform.localScale = new Vector3(30f, 0.1f, 30f);
            ground.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.32f, 0.3f, 0.28f));
            Object.Destroy(ground.GetComponent<Collider>());
            ground.AddComponent<BoxCollider>();

            // Dirty floor patches
            for (int i = 0; i < 5; i++)
            {
                var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                patch.name = "DirtyPatch";
                patch.transform.SetParent(zone.transform, false);
                patch.transform.localPosition = new Vector3(
                    Random.Range(-10f, 10f), 0.01f, Random.Range(-10f, 10f));
                patch.transform.localScale = new Vector3(
                    Random.Range(1.5f, 3f), 0.005f, Random.Range(1.5f, 3f));
                patch.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                patch.GetComponent<Renderer>().material = ZoneFactory.StoneMat(
                    new Color(0.25f + Random.Range(0f, 0.05f), 0.22f, 0.2f));
                Object.Destroy(patch.GetComponent<Collider>());
            }

            // ═══ Zone Lighting — harsh cold ═══
            ZoneFactory.CreateZoneLighting(zone.transform, new Color(0.7f, 0.75f, 0.9f), 1.2f, new Vector3(0, -1f, 0));

            // ═══ Perimeter Walls — high concrete with razor wire ═══
            BuildPerimeterWalls(zone.transform);

            // ═══ Area 1: Cell Block (NE) ═══
            BuildCellBlock(zone.transform);

            // ═══ Area 2: Exercise Yard (SW) ═══
            BuildExerciseYard(zone.transform);

            // ═══ Area 3: Mess Hall (SE) ═══
            BuildMessHall(zone.transform);

            // ═══ Area 4: Guard Tower (NW) ═══
            BuildGuardTower(zone.transform);

            // ═══ Area 5: Warden Office (W) ═══
            BuildWardenOffice(zone.transform);

            // ═══ Area 6: Solitary Confinement (S) ═══
            BuildSolitary(zone.transform);

            // ═══ Smoke/haze particles ═══
            ZoneParticles.CreateSmoke(zone.transform, new Vector3(0, 0.5f, 0),
                new Color(0.4f, 0.4f, 0.45f, 0.3f));

            // ═══ Title ═══
            var title = new GameObject("PrisonTitle");
            title.transform.SetParent(zone.transform, false);
            title.transform.localPosition = new Vector3(0, 4f, -14f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.text = "Nhà Giam Ranger City";
            tmp.fontSize = 5.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.9f, 0.2f, 0.15f);
            tmp.alignment = TextAlignmentOptions.Center;
            title.AddComponent<BillboardText>();

            // ═══ Return Portal ═══
            ZoneFactory.CreatePortal("PrisonReturnPortal", new Vector3(0, 0.05f, -54f), 0f,
                new Color(0.25f, 0.25f, 0.3f),
                new Color(0.85f, 0.15f, 0.15f, 0.7f),
                "Về Sảnh Chờ");
        }

        // ── Perimeter Walls with razor wire ──
        private static void BuildPerimeterWalls(Transform parent)
        {
            Color wallColor = new Color(0.22f, 0.22f, 0.24f);
            float limit = 14.5f;
            float wallH = 3.5f;

            // 4 walls
            BuildWall(parent, new Vector3(0, wallH * 0.5f, limit), new Vector3(29f, wallH, 0.5f), wallColor);
            BuildWall(parent, new Vector3(0, wallH * 0.5f, -limit), new Vector3(29f, wallH, 0.5f), wallColor);
            BuildWall(parent, new Vector3(limit, wallH * 0.5f, 0), new Vector3(0.5f, wallH, 29f), wallColor);
            BuildWall(parent, new Vector3(-limit, wallH * 0.5f, 0), new Vector3(0.5f, wallH, 29f), wallColor);

            // Razor wire on top of walls (spiky spheres)
            for (float x = -13f; x <= 13f; x += 1.5f)
            {
                BuildRazorWire(parent, new Vector3(x, wallH + 0.1f, limit));
                BuildRazorWire(parent, new Vector3(x, wallH + 0.1f, -limit));
            }
            for (float z = -13f; z <= 13f; z += 1.5f)
            {
                BuildRazorWire(parent, new Vector3(limit, wallH + 0.1f, z));
                BuildRazorWire(parent, new Vector3(-limit, wallH + 0.1f, z));
            }

            // Warning signs on walls
            BuildWarningSign(parent, new Vector3(5f, 2f, -14.3f), "⚠ KHU VỰC CẤM");
            BuildWarningSign(parent, new Vector3(-5f, 2f, -14.3f), "🔇 IM LẶNG!");
        }

        // ── Area 1: Cell Block (6 varied cells) ──
        private static void BuildCellBlock(Transform parent)
        {
            var area = new GameObject("CellBlock");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(5f, 0, 7f);

            Color barColor = new Color(0.25f, 0.25f, 0.28f);

            // 6 cells in 2 rows of 3
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int cellIdx = row * 3 + col;
                    float x = col * 3.2f - 3.2f;
                    float z = row * 4f - 2f;

                    var cell = new GameObject($"Cell_{cellIdx}");
                    cell.transform.SetParent(area.transform, false);
                    cell.transform.localPosition = new Vector3(x, 0, z);

                    // Back wall
                    var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    back.name = "CellBack_Collider";
                    back.transform.SetParent(cell.transform, false);
                    back.transform.localPosition = new Vector3(0, 1.2f, 1.8f);
                    back.transform.localScale = new Vector3(3f, 2.4f, 0.15f);
                    back.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.2f, 0.2f, 0.22f));

                    // Side walls
                    var sideL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sideL.name = "CellSideL_Collider";
                    sideL.transform.SetParent(cell.transform, false);
                    sideL.transform.localPosition = new Vector3(-1.45f, 1.2f, 0.9f);
                    sideL.transform.localScale = new Vector3(0.1f, 2.4f, 1.8f);
                    sideL.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.2f, 0.2f, 0.22f));

                    // Iron bars (front of cell)
                    for (float bx = -1.2f; bx <= 1.2f; bx += 0.25f)
                    {
                        var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        bar.name = "Bar";
                        bar.transform.SetParent(cell.transform, false);
                        bar.transform.localPosition = new Vector3(bx, 1.2f, 0);
                        bar.transform.localScale = new Vector3(0.05f, 1.2f, 0.05f);
                        bar.GetComponent<Renderer>().material = ZoneFactory.MetalMat(barColor);
                        Object.Destroy(bar.GetComponent<Collider>());
                    }

                    // Bed
                    var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bed.name = "Bed_Collider";
                    bed.transform.SetParent(cell.transform, false);
                    bed.transform.localPosition = new Vector3(-0.8f, 0.2f, 1.2f);
                    bed.transform.localScale = new Vector3(1.2f, 0.2f, 0.6f);
                    bed.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.45f, 0.35f, 0.25f));

                    // Pillow
                    var pillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pillow.name = "Pillow";
                    pillow.transform.SetParent(cell.transform, false);
                    pillow.transform.localPosition = new Vector3(-0.8f, 0.32f, 1.45f);
                    pillow.transform.localScale = new Vector3(0.35f, 0.08f, 0.2f);
                    pillow.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.6f, 0.58f, 0.55f));
                    Object.Destroy(pillow.GetComponent<Collider>());

                    // Toilet
                    var toilet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    toilet.name = "Toilet";
                    toilet.transform.SetParent(cell.transform, false);
                    toilet.transform.localPosition = new Vector3(0.8f, 0.2f, 1.4f);
                    toilet.transform.localScale = new Vector3(0.22f, 0.2f, 0.22f);
                    toilet.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.85f, 0.85f, 0.88f));
                    Object.Destroy(toilet.GetComponent<Collider>());

                    // Unique cell items (variation!)
                    switch (cellIdx)
                    {
                        case 0: // Tally marks on wall
                            var tally = new GameObject("TallyMarks");
                            tally.transform.SetParent(cell.transform, false);
                            tally.transform.localPosition = new Vector3(0.5f, 1.5f, 1.75f);
                            var tallyTmp = tally.AddComponent<TextMeshPro>();
                            tallyTmp.text = "IIII IIII III";
                            tallyTmp.fontSize = 1.5f;
                            tallyTmp.color = new Color(0.7f, 0.65f, 0.6f);
                            tallyTmp.alignment = TextAlignmentOptions.Center;
                            break;
                        case 1: // Books stack
                            for (int b = 0; b < 3; b++)
                            {
                                var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                book.name = "Book";
                                book.transform.SetParent(cell.transform, false);
                                book.transform.localPosition = new Vector3(0.5f, 0.05f + b * 0.06f, 0.8f);
                                book.transform.localScale = new Vector3(0.2f, 0.05f, 0.15f);
                                book.transform.localRotation = Quaternion.Euler(0, b * 15f, 0);
                                Color[] bColors = { new(0.7f, 0.2f, 0.2f), new(0.2f, 0.3f, 0.7f), new(0.6f, 0.6f, 0.2f) };
                                book.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(bColors[b]);
                                Object.Destroy(book.GetComponent<Collider>());
                            }
                            break;
                        case 3: // Photo on wall
                            var photo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            photo.name = "Photo";
                            photo.transform.SetParent(cell.transform, false);
                            photo.transform.localPosition = new Vector3(-0.2f, 1.5f, 1.75f);
                            photo.transform.localScale = new Vector3(0.2f, 0.15f, 0.005f);
                            photo.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.8f, 0.75f, 0.7f));
                            Object.Destroy(photo.GetComponent<Collider>());
                            break;
                        case 5: // Chains on wall
                            for (int c = 0; c < 3; c++)
                            {
                                var chain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                                chain.name = "Chain";
                                chain.transform.SetParent(cell.transform, false);
                                chain.transform.localPosition = new Vector3(-0.5f + c * 0.3f, 1.8f, 1.73f);
                                chain.transform.localScale = new Vector3(0.03f, 0.3f, 0.03f);
                                chain.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.35f, 0.33f, 0.3f));
                                Object.Destroy(chain.GetComponent<Collider>());
                            }
                            break;
                    }

                    // Ceiling light per cell
                    var cellLight = new GameObject("CellLight");
                    cellLight.transform.SetParent(cell.transform, false);
                    cellLight.transform.localPosition = new Vector3(0, 2.2f, 0.9f);
                    var cl = cellLight.AddComponent<Light>();
                    cl.type = LightType.Point;
                    cl.color = new Color(0.9f, 0.85f, 0.7f);
                    cl.range = 4f;
                    cl.intensity = 0.8f;
                }
            }
        }

        // ── Area 2: Exercise Yard ──
        private static void BuildExerciseYard(Transform parent)
        {
            var area = new GameObject("ExerciseYard");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-7f, 0, -3f);

            // Yard surface (slightly different concrete)
            var yard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            yard.name = "YardSurface";
            yard.transform.SetParent(area.transform, false);
            yard.transform.localPosition = new Vector3(0, 0.01f, 0);
            yard.transform.localScale = new Vector3(12f, 0.02f, 10f);
            yard.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.38f, 0.36f, 0.33f));
            Object.Destroy(yard.GetComponent<Collider>());

            // Running track markings (white lines)
            for (int i = 0; i < 4; i++)
            {
                float z = -3f + i * 2f;
                var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "TrackLine";
                line.transform.SetParent(area.transform, false);
                line.transform.localPosition = new Vector3(0, 0.02f, z);
                line.transform.localScale = new Vector3(10f, 0.005f, 0.05f);
                line.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.8f, 0.78f, 0.75f));
                Object.Destroy(line.GetComponent<Collider>());
            }

            // Bench press
            var benchFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchFrame.name = "BenchPress_Collider";
            benchFrame.transform.SetParent(area.transform, false);
            benchFrame.transform.localPosition = new Vector3(-3f, 0.2f, 2f);
            benchFrame.transform.localScale = new Vector3(1.2f, 0.08f, 0.4f);
            benchFrame.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.3f, 0.28f, 0.25f));

            // Barbell
            var barbell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barbell.name = "Barbell";
            barbell.transform.SetParent(area.transform, false);
            barbell.transform.localPosition = new Vector3(-3f, 0.6f, 2f);
            barbell.transform.localScale = new Vector3(0.03f, 0.5f, 0.03f);
            barbell.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            barbell.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.4f, 0.4f, 0.42f));
            Object.Destroy(barbell.GetComponent<Collider>());

            // Weight plates
            for (int s = 0; s < 2; s++)
            {
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plate.name = "WeightPlate";
                plate.transform.SetParent(area.transform, false);
                plate.transform.localPosition = new Vector3(-3f + (s == 0 ? -0.55f : 0.55f), 0.6f, 2f);
                plate.transform.localScale = new Vector3(0.15f, 0.03f, 0.15f);
                plate.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                plate.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.2f, 0.2f, 0.22f));
                Object.Destroy(plate.GetComponent<Collider>());
            }

            // Pull-up bar
            var pullUp = new GameObject("PullUpBar");
            pullUp.transform.SetParent(area.transform, false);
            pullUp.transform.localPosition = new Vector3(2f, 0, 3f);

            // Posts
            var postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postL.name = "PostL_Collider";
            postL.transform.SetParent(pullUp.transform, false);
            postL.transform.localPosition = new Vector3(-0.5f, 1.1f, 0);
            postL.transform.localScale = new Vector3(0.06f, 1.1f, 0.06f);
            postL.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.35f, 0.35f, 0.38f));

            var postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postR.name = "PostR_Collider";
            postR.transform.SetParent(pullUp.transform, false);
            postR.transform.localPosition = new Vector3(0.5f, 1.1f, 0);
            postR.transform.localScale = new Vector3(0.06f, 1.1f, 0.06f);
            postR.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.35f, 0.35f, 0.38f));

            // Cross bar
            var crossBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crossBar.name = "CrossBar";
            crossBar.transform.SetParent(pullUp.transform, false);
            crossBar.transform.localPosition = new Vector3(0, 2.15f, 0);
            crossBar.transform.localScale = new Vector3(0.04f, 0.5f, 0.04f);
            crossBar.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            crossBar.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.4f, 0.4f, 0.42f));
            Object.Destroy(crossBar.GetComponent<Collider>());

            // Basketball hoop
            BuildBasketballHoop(area.transform, new Vector3(4f, 0, -2f));
        }

        // ── Area 3: Mess Hall ──
        private static void BuildMessHall(Transform parent)
        {
            var area = new GameObject("MessHall");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(5f, 0, -7f);

            // Partial roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "MessRoof";
            roof.transform.SetParent(area.transform, false);
            roof.transform.localPosition = new Vector3(0, 2.8f, 0);
            roof.transform.localScale = new Vector3(10f, 0.1f, 8f);
            roof.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.3f, 0.28f, 0.26f));
            Object.Destroy(roof.GetComponent<Collider>());

            // Long tables (3)
            for (int i = 0; i < 3; i++)
            {
                float z = -2f + i * 2.5f;

                var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
                table.name = "MessTable_Collider";
                table.transform.SetParent(area.transform, false);
                table.transform.localPosition = new Vector3(0, 0.4f, z);
                table.transform.localScale = new Vector3(6f, 0.06f, 0.8f);
                table.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.5f, 0.48f, 0.46f));

                // Bench seats (2 per table)
                for (int s = 0; s < 2; s++)
                {
                    float bz = z + (s == 0 ? -0.6f : 0.6f);
                    var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bench.name = "Bench_Collider";
                    bench.transform.SetParent(area.transform, false);
                    bench.transform.localPosition = new Vector3(0, 0.2f, bz);
                    bench.transform.localScale = new Vector3(5.5f, 0.04f, 0.35f);
                    bench.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.45f, 0.43f, 0.4f));
                }

                // Food trays on table
                for (int t = 0; t < 4; t++)
                {
                    var tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tray.name = "FoodTray";
                    tray.transform.SetParent(area.transform, false);
                    tray.transform.localPosition = new Vector3(-2f + t * 1.5f, 0.45f, z);
                    tray.transform.localScale = new Vector3(0.3f, 0.02f, 0.2f);
                    tray.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.7f, 0.68f, 0.65f));
                    Object.Destroy(tray.GetComponent<Collider>());
                }
            }

            // Kitchen counter (back wall)
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "KitchenCounter_Collider";
            counter.transform.SetParent(area.transform, false);
            counter.transform.localPosition = new Vector3(0, 0.5f, 3.5f);
            counter.transform.localScale = new Vector3(8f, 1.0f, 0.8f);
            counter.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.55f, 0.53f, 0.5f));

            // Fluorescent light
            var messLight = new GameObject("MessLight");
            messLight.transform.SetParent(area.transform, false);
            messLight.transform.localPosition = new Vector3(0, 2.5f, 0);
            var ml = messLight.AddComponent<Light>();
            ml.type = LightType.Point;
            ml.color = new Color(0.85f, 0.9f, 1f);
            ml.range = 8f;
            ml.intensity = 1.5f;
        }

        // ── Area 4: Guard Tower ──
        private static void BuildGuardTower(Transform parent)
        {
            var tower = new GameObject("GuardTower");
            tower.transform.SetParent(parent, false);
            tower.transform.localPosition = new Vector3(-12f, 0, 12f);

            // Tower legs (4)
            float legDist = 0.8f;
            Vector3[] legPositions = {
                new(-legDist, 2f, -legDist), new(legDist, 2f, -legDist),
                new(-legDist, 2f, legDist), new(legDist, 2f, legDist)
            };
            foreach (var pos in legPositions)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "TowerLeg_Collider";
                leg.transform.SetParent(tower.transform, false);
                leg.transform.localPosition = pos;
                leg.transform.localScale = new Vector3(0.12f, 2f, 0.12f);
                leg.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.3f, 0.3f, 0.32f));
            }

            // Platform
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "TowerPlatform_Collider";
            platform.transform.SetParent(tower.transform, false);
            platform.transform.localPosition = new Vector3(0, 4f, 0);
            platform.transform.localScale = new Vector3(2.5f, 0.15f, 2.5f);
            platform.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.45f, 0.32f, 0.2f));

            // Cabin walls (half-height)
            var cabinN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinN.name = "CabinN";
            cabinN.transform.SetParent(tower.transform, false);
            cabinN.transform.localPosition = new Vector3(0, 4.6f, 1.1f);
            cabinN.transform.localScale = new Vector3(2.3f, 1.0f, 0.1f);
            cabinN.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.42f, 0.3f, 0.18f));
            Object.Destroy(cabinN.GetComponent<Collider>());

            var cabinS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinS.name = "CabinS";
            cabinS.transform.SetParent(tower.transform, false);
            cabinS.transform.localPosition = new Vector3(0, 4.6f, -1.1f);
            cabinS.transform.localScale = new Vector3(2.3f, 1.0f, 0.1f);
            cabinS.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.42f, 0.3f, 0.18f));
            Object.Destroy(cabinS.GetComponent<Collider>());

            // Cabin roof
            var cabinRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinRoof.name = "CabinRoof";
            cabinRoof.transform.SetParent(tower.transform, false);
            cabinRoof.transform.localPosition = new Vector3(0, 5.2f, 0);
            cabinRoof.transform.localScale = new Vector3(2.7f, 0.1f, 2.7f);
            cabinRoof.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.3f, 0.28f, 0.25f));
            Object.Destroy(cabinRoof.GetComponent<Collider>());

            // Searchlight (spot light)
            var searchlight = new GameObject("Searchlight");
            searchlight.transform.SetParent(tower.transform, false);
            searchlight.transform.localPosition = new Vector3(0, 5f, 0);
            var sl = searchlight.AddComponent<Light>();
            sl.type = LightType.Spot;
            sl.color = new Color(1f, 0.95f, 0.8f);
            sl.range = 25f;
            sl.intensity = 3f;
            sl.spotAngle = 35f;
            searchlight.transform.localRotation = Quaternion.Euler(45f, 30f, 0);

            // Alarm bell
            var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "AlarmBell";
            bell.transform.SetParent(tower.transform, false);
            bell.transform.localPosition = new Vector3(0, 5.35f, 0);
            bell.transform.localScale = new Vector3(0.2f, 0.15f, 0.2f);
            bell.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.7f, 0.2f, 0.15f));
            Object.Destroy(bell.GetComponent<Collider>());
        }

        // ── Area 5: Warden Office ──
        private static void BuildWardenOffice(Transform parent)
        {
            var area = new GameObject("WardenOffice");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(-9f, 0, 2f);

            // Room walls
            var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "OfficeBack_Collider";
            backWall.transform.SetParent(area.transform, false);
            backWall.transform.localPosition = new Vector3(0, 1.2f, 2.5f);
            backWall.transform.localScale = new Vector3(5f, 2.4f, 0.15f);
            backWall.GetComponent<Renderer>().material = ZoneFactory.StoneMat(new Color(0.35f, 0.33f, 0.3f));

            // Desk
            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "WardenDesk_Collider";
            desk.transform.SetParent(area.transform, false);
            desk.transform.localPosition = new Vector3(0, 0.4f, 1.5f);
            desk.transform.localScale = new Vector3(2f, 0.08f, 0.8f);
            desk.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.35f, 0.22f, 0.12f));

            // Desk drawers
            var drawer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            drawer.name = "Drawer";
            drawer.transform.SetParent(area.transform, false);
            drawer.transform.localPosition = new Vector3(0.6f, 0.25f, 1.5f);
            drawer.transform.localScale = new Vector3(0.5f, 0.35f, 0.7f);
            drawer.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.3f, 0.2f, 0.1f));
            Object.Destroy(drawer.GetComponent<Collider>());

            // Chair
            var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chair.name = "WardenChair_Collider";
            chair.transform.SetParent(area.transform, false);
            chair.transform.localPosition = new Vector3(0, 0.35f, 0.5f);
            chair.transform.localScale = new Vector3(0.55f, 0.05f, 0.55f);
            chair.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.18f, 0.15f));

            // Safe
            var safe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            safe.name = "Safe_Collider";
            safe.transform.SetParent(area.transform, false);
            safe.transform.localPosition = new Vector3(-2f, 0.35f, 2f);
            safe.transform.localScale = new Vector3(0.6f, 0.7f, 0.5f);
            safe.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.25f, 0.25f, 0.28f));

            // Wanted poster board
            var wanted = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wanted.name = "WantedBoard";
            wanted.transform.SetParent(area.transform, false);
            wanted.transform.localPosition = new Vector3(1.5f, 1.5f, 2.42f);
            wanted.transform.localScale = new Vector3(1.5f, 1.2f, 0.02f);
            wanted.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.5f, 0.38f, 0.22f));
            Object.Destroy(wanted.GetComponent<Collider>());

            var wantedText = new GameObject("WantedText");
            wantedText.transform.SetParent(wanted.transform, false);
            wantedText.transform.localPosition = new Vector3(0, 0, -0.6f);
            var wtmp = wantedText.AddComponent<TextMeshPro>();
            wtmp.text = "🚨 TRUY NÃ\n━━━━━━━━\n👤 Tên A\n👤 Tên B\n👤 Tên C";
            wtmp.fontSize = 1.2f;
            wtmp.color = new Color(0.15f, 0.1f, 0.08f);
            wtmp.alignment = TextAlignmentOptions.Center;

            // Filing cabinet
            var cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinet.name = "FilingCabinet_Collider";
            cabinet.transform.SetParent(area.transform, false);
            cabinet.transform.localPosition = new Vector3(2f, 0.5f, 2f);
            cabinet.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);
            cabinet.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.4f, 0.4f, 0.42f));
        }

        // ── Area 6: Solitary Confinement ──
        private static void BuildSolitary(Transform parent)
        {
            var area = new GameObject("SolitaryConfinement");
            area.transform.SetParent(parent, false);
            area.transform.localPosition = new Vector3(8f, 0, -12f);

            // Tiny dark room
            Color darkWall = new Color(0.15f, 0.15f, 0.17f);

            var walls = new[] {
                (new Vector3(0, 1f, 1.5f), new Vector3(3f, 2f, 0.15f)),    // back
                (new Vector3(-1.4f, 1f, 0), new Vector3(0.15f, 2f, 3f)),   // left
                (new Vector3(1.4f, 1f, 0), new Vector3(0.15f, 2f, 3f)),    // right
            };
            foreach (var (pos, scale) in walls)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "SolitaryWall_Collider";
                wall.transform.SetParent(area.transform, false);
                wall.transform.localPosition = pos;
                wall.transform.localScale = scale;
                wall.GetComponent<Renderer>().material = ZoneFactory.StoneMat(darkWall);
            }

            // Heavy door
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "HeavyDoor_Collider";
            door.transform.SetParent(area.transform, false);
            door.transform.localPosition = new Vector3(0, 1f, -1.4f);
            door.transform.localScale = new Vector3(2.6f, 2f, 0.2f);
            door.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.25f, 0.22f, 0.2f));

            // Small window slit in door
            var slit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slit.name = "WindowSlit";
            slit.transform.SetParent(area.transform, false);
            slit.transform.localPosition = new Vector3(0, 1.5f, -1.28f);
            slit.transform.localScale = new Vector3(0.5f, 0.12f, 0.05f);
            slit.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.35f, 0.33f, 0.3f));
            Object.Destroy(slit.GetComponent<Collider>());

            // Dim light
            var dimLight = new GameObject("DimLight");
            dimLight.transform.SetParent(area.transform, false);
            dimLight.transform.localPosition = new Vector3(0, 1.8f, 0);
            var dl = dimLight.AddComponent<Light>();
            dl.type = LightType.Point;
            dl.color = new Color(0.7f, 0.6f, 0.4f);
            dl.range = 3f;
            dl.intensity = 0.4f;

            // Sign
            var sign = new GameObject("SolitarySign");
            sign.transform.SetParent(area.transform, false);
            sign.transform.localPosition = new Vector3(0, 2.2f, -1.5f);
            var stmp = sign.AddComponent<TextMeshPro>();
            stmp.text = "BIỆT GIAM";
            stmp.fontSize = 2f;
            stmp.color = new Color(0.9f, 0.2f, 0.15f);
            stmp.alignment = TextAlignmentOptions.Center;
            sign.AddComponent<BillboardText>();
        }

        // ═══════════════════════ HELPER BUILDERS ═══════════════════════

        private static void BuildWall(Transform parent, Vector3 pos, Vector3 scale, Color color)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "PerimeterWall_Collider";
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material = ZoneFactory.StoneMat(color);
        }

        private static void BuildRazorWire(Transform parent, Vector3 pos)
        {
            var wire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wire.name = "RazorWire";
            wire.transform.SetParent(parent, false);
            wire.transform.localPosition = pos;
            wire.transform.localScale = new Vector3(0.6f, 0.2f, 0.15f);
            wire.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            wire.GetComponent<Renderer>().material = ZoneFactory.MetalMat(
                new Color(0.3f + Random.Range(0f, 0.05f), 0.3f, 0.32f));
            Object.Destroy(wire.GetComponent<Collider>());
        }

        private static void BuildWarningSign(Transform parent, Vector3 pos, string text)
        {
            var sign = new GameObject("WarningSign");
            sign.transform.SetParent(parent, false);
            sign.transform.localPosition = pos;

            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "SignBg";
            bg.transform.SetParent(sign.transform, false);
            bg.transform.localScale = new Vector3(1.5f, 0.5f, 0.02f);
            bg.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.9f, 0.8f, 0.15f));
            Object.Destroy(bg.GetComponent<Collider>());

            var tmp = sign.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 1.5f;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildBasketballHoop(Transform parent, Vector3 pos)
        {
            var hoop = new GameObject("BasketballHoop");
            hoop.transform.SetParent(parent, false);
            hoop.transform.localPosition = pos;

            // Backboard
            var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backboard.name = "Backboard_Collider";
            backboard.transform.SetParent(hoop.transform, false);
            backboard.transform.localPosition = new Vector3(0, 2.2f, 0);
            backboard.transform.localScale = new Vector3(1.2f, 0.8f, 0.05f);
            backboard.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.9f, 0.9f, 0.92f));

            // Pole
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole_Collider";
            pole.transform.SetParent(hoop.transform, false);
            pole.transform.localPosition = new Vector3(0, 1.2f, 0.2f);
            pole.transform.localScale = new Vector3(0.08f, 1.2f, 0.08f);
            pole.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.35f, 0.35f, 0.38f));

            // Rim
            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Rim";
            rim.transform.SetParent(hoop.transform, false);
            rim.transform.localPosition = new Vector3(0, 2.0f, -0.3f);
            rim.transform.localScale = new Vector3(0.35f, 0.015f, 0.35f);
            rim.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.8f, 0.4f, 0.15f));
            Object.Destroy(rim.GetComponent<Collider>());
        }
    }
}
