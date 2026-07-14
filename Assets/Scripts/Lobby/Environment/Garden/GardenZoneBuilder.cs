using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Sacred Sky Garden Sanctuary - Main Build Controller
    /// </summary>
    public static partial class GardenZoneBuilder
    {
        // ── Color Definitions ──
        private static readonly Color Grass = new(0.28f,0.67f,0.29f);
        private static readonly Color Dirt = new(0.39f,0.29f,0.19f);
        private static readonly Color Rock = new(0.55f,0.56f,0.60f);
        private static readonly Color Marble = new(0.93f,0.94f,0.96f);
        private static readonly Color Gold = new(0.96f,0.82f,0.25f);
        private static readonly Color Water = new(0.18f,0.72f,0.92f,0.75f);
        private static readonly Color Leaf = new(0.23f,0.64f,0.25f);

        // Color aliases for compatibility
        private static Color GrassColor => Grass;
        private static Color DirtColor => Dirt;
        private static Color MarbleColor => Marble;
        private static Color GoldColor => Gold;

        public static void Build()
        {
            GameObject zone = new GameObject("GardenZone");
            zone.transform.position = new Vector3(0,0,60);

            BuildCloudSea(zone.transform);

            CreateFloatingIslands(zone.transform);

            GameObject entrance =
                CreateIsland(
                    zone.transform,
                    "EntranceIsland",
                    new Vector3(0,0,-4),
                    3.4f,
                    1.5f,
                    GrassColor,
                    DirtColor);

            BuildEntranceGate(entrance.transform);

            CreatePortalArea(entrance.transform);

            CreateLantern(
                entrance.transform,
                new Vector3(-1.5f,.3f,0));

            GameObject meadow =
                CreateIsland(
                    zone.transform,
                    "FlowerMeadow",
                    new Vector3(-3.5f,0f,3),
                    3.9f,
                    1.6f,
                    GrassColor,
                    DirtColor);

            ScatterFlowers(
                meadow.transform,
                40);

            CreateGardenPlot(
                meadow.transform,
                new Vector3(-1f,.05f,-.5f));

            CreateGardenPlot(
                meadow.transform,
                new Vector3(1.2f,.05f,.7f));

            GameObject pond =
                CreateIsland(
                    zone.transform,
                    "PondIsland",
                    new Vector3(3.5f,0f,3),
                    3.8f,
                    1.8f,
                    GrassColor,
                    DirtColor);

            BuildPond(pond.transform);

            BuildWaterfall(
                pond.transform,
                new Vector3(2.8f,0,0),
                4.8f);

            BuildMeditationArea(
                pond.transform);

            BuildCurvedBridge(
                zone.transform,
                new Vector3(0,.05f,-2f),
                new Vector3(-3,.05f,2.2f));

            BuildCurvedBridge(
                zone.transform,
                new Vector3(-.4f,.05f,3),
                new Vector3(3,.05f,3));

            BuildStoneSteps(
                zone.transform,
                new Vector3(.2f,.05f,6.5f),
                new Vector3(0,.05f,10));

            BuildHighSanctuary(
                zone.transform);

            CreateFireflies(
                zone.transform,
                50);

            CreateTitle(zone.transform);
        }

        private static void CreateTitle(Transform parent)
        {
            GameObject obj=new GameObject("GardenTitle");
            obj.transform.SetParent(parent,false);

            obj.transform.localPosition=
                new Vector3(0,6,10);

            TextMeshPro text=
                obj.AddComponent<TextMeshPro>();

            text.text="Sacred Sky Garden";
            text.fontSize=5;
            text.alignment=
                TextAlignmentOptions.Center;

            obj.AddComponent<BillboardText>();
        }

        private static void BuildEntranceGate(Transform parent)
        {
            GameObject gate=new GameObject("Gate");
            gate.transform.SetParent(parent,false);

            GameObject left=
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            left.transform.SetParent(gate.transform,false);
            left.transform.localPosition=
                new Vector3(-1.1f,1,0);
            left.transform.localScale=
                new Vector3(.16f,1,.16f);
            left.GetComponent<Renderer>().material=
                ZoneFactory.CreateMat(MarbleColor);

            GameObject right=
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            right.transform.SetParent(gate.transform,false);
            right.transform.localPosition=
                new Vector3(1.1f,1,0);
            right.transform.localScale=
                new Vector3(.16f,1,.16f);
            right.GetComponent<Renderer>().material=
                ZoneFactory.CreateMat(MarbleColor);

            GameObject top=
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            top.transform.SetParent(gate.transform,false);
            top.transform.localPosition=
                new Vector3(0,2,0);
            top.transform.localScale=
                new Vector3(2.5f,.15f,.25f);

            top.GetComponent<Renderer>().material=
                ZoneFactory.CreateMat(GoldColor);
        }

        private static void BuildStoneSteps(
            Transform parent,
            Vector3 start,
            Vector3 end)
        {
            const int steps=7;

            for(int i=0;i<steps;i++)
            {
                float t=i/(float)(steps-1);

                Vector3 p=Vector3.Lerp(start,end,t);

                p.x+=Mathf.Sin(t*Mathf.PI)*.45f;

                GameObject s=
                    GameObject.CreatePrimitive(
                        PrimitiveType.Cube);

                s.transform.SetParent(parent,false);
                s.transform.localPosition=p;

                s.transform.localScale=
                    new Vector3(
                        1.6f,
                        .12f,
                        .7f);

                s.GetComponent<Renderer>().material=
                    ZoneFactory.CreateMat(
                        MarbleColor);
            }
        }

        private static void BuildMeditationArea(
            Transform parent)
        {
            GameObject root=
                new GameObject("Meditation");

            root.transform.SetParent(parent,false);
            root.transform.localPosition=
                new Vector3(1.6f,0,1.6f);

            CreateBench(
                root.transform,
                Vector3.zero);

            GameObject altar=
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            altar.transform.SetParent(root.transform,false);
            altar.transform.localPosition=
                new Vector3(-.8f,.25f,-.6f);

            altar.transform.localScale=
                new Vector3(.3f,.25f,.3f);

            altar.GetComponent<Renderer>().material=
                ZoneFactory.CreateMat(
                    MarbleColor);

            Light l=
                altar.AddComponent<Light>();

            l.type=LightType.Point;
            l.range=3;
            l.intensity=1.4f;
            l.color=new Color(
                1,
                .85f,
                .5f);
        }

        private static void BuildPond(
            Transform parent)
        {
            GameObject water=
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            water.transform.SetParent(parent,false);

            water.transform.localPosition=
                new Vector3(-.4f,.02f,-.3f);

            water.transform.localScale=
                new Vector3(
                    2.2f,
                    .02f,
                    2.2f);

            water.GetComponent<Renderer>().material=
                ZoneFactory.CreateMat(
                    new Color(
                        .35f,
                        .8f,
                        1f,
                        .8f));

            Object.Destroy(
                water.GetComponent<Collider>());

            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI*2/8;

                GameObject rock=
                    GameObject.CreatePrimitive(
                        PrimitiveType.Cube);

                rock.transform.SetParent(parent,false);

                rock.transform.localPosition=
                    new Vector3(
                        Mathf.Cos(a)*1.2f-.4f,
                        .05f,
                        Mathf.Sin(a)*1.2f-.3f);

                rock.transform.localScale=
                    new Vector3(
                        .55f,
                        .12f,
                        .35f);

                rock.GetComponent<Renderer>().material=
                    ZoneFactory.CreateMat(
                        new Color(
                            .6f,
                            .6f,
                            .63f));
            }
        }
    }
}
