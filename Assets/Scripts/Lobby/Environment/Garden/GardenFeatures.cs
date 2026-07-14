using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Sacred Sky Garden Sanctuary - Waterfall, Cloud sea, Portal, Lanterns, and Plots
    /// </summary>
    public static partial class GardenZoneBuilder
    {
        private static void BuildWaterfall(Transform parent, Vector3 pos, float height)
        {
            GameObject root = new GameObject("Waterfall");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;

            GameObject sheet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sheet.transform.SetParent(root.transform, false);
            sheet.transform.localPosition = new Vector3(0, -height * .5f, 0);
            sheet.transform.localScale = new Vector3(.8f, height, .06f);
            sheet.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.45f, .8f, 1f, .75f));
            Object.Destroy(sheet.GetComponent<Collider>());

            for (int i = 0; i < 12; i++)
            {
                GameObject foam = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foam.transform.SetParent(root.transform, false);
                foam.transform.localPosition = new Vector3(
                    Random.Range(-.45f, .45f),
                    -height + Random.Range(-.15f, .15f),
                    Random.Range(-.35f, .35f));
                foam.transform.localScale = Vector3.one * Random.Range(.18f, .35f);
                foam.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Color.white);
                Object.Destroy(foam.GetComponent<Collider>());
            }
        }

        private static void BuildCloudSea(Transform parent)
        {
            GameObject clouds = new GameObject("CloudSea");
            clouds.transform.SetParent(parent, false);

            for (int i = 0; i < 60; i++)
            {
                Vector3 p = new Vector3(
                    Random.Range(-28f, 28f),
                    Random.Range(-5.5f, -3f),
                    Random.Range(-18f, 30f));
                float r = Random.Range(1.2f, 3.8f);

                GameObject group = new GameObject("Cloud");
                group.transform.SetParent(clouds.transform, false);
                group.transform.localPosition = p;

                ZoneFactory.CreateCloudVisual(group, r);
            }
        }

        private static void CreateFloatingIslands(Transform parent)
        {
            Vector3[] pos = {
                new(-18, 6, 16),
                new(18, 8, 20),
                new(-22, 10, -6),
                new(24, 7, 8),
                new(0, 12, 28)
            };

            foreach (var p in pos)
            {
                GameObject island = CreateIsland(
                    parent,
                    "BackgroundIsland",
                    p,
                    Random.Range(1.6f, 2.8f),
                    Random.Range(.8f, 1.5f),
                    GrassColor,
                    DirtColor);

                GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tree.transform.SetParent(island.transform, false);
                tree.transform.localPosition = new Vector3(0, .7f, 0);
                tree.transform.localScale = Vector3.one * 1.1f;
                tree.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(1f, .82f, .3f));
                Object.Destroy(tree.GetComponent<Collider>());
            }
        }

        private static void CreatePortalArea(Transform parent)
        {
            ZoneFactory.CreatePortal(
                "GardenReturnPortal",
                new Vector3(0, .05f, 54),
                180,
                MarbleColor,
                new Color(.25f, 1f, .5f, .7f),
                "Return");

            for (int i = 0; i < 8; i++)
            {
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.transform.SetParent(parent, false);

                float a = i * Mathf.PI * 2f / 8f;
                orb.transform.localPosition = new Vector3(
                    Mathf.Cos(a) * .7f,
                    .4f,
                    Mathf.Sin(a) * .7f - 6f);
                orb.transform.localScale = Vector3.one * .12f;
                orb.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.6f, 1f, .8f));
                Object.Destroy(orb.GetComponent<Collider>());
            }
        }

        private static void CreateHangingLantern(Transform parent, Vector3 pos)
        {
            GameObject root = new GameObject("Lantern");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;

            GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rope.transform.SetParent(root.transform, false);
            rope.transform.localScale = new Vector3(.01f, .12f, .01f);
            rope.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Color.black);

            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.transform.SetParent(root.transform, false);
            lamp.transform.localPosition = new Vector3(0, -.16f, 0);
            lamp.transform.localScale = Vector3.one * .14f;
            lamp.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(1f, .95f, .55f));
            Object.Destroy(lamp.GetComponent<Collider>());

            Light l = root.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 3.5f;
            l.intensity = 1.3f;
            l.color = new Color(1f, .9f, .6f);
        }

        static void CreateLantern(Transform parent, Vector3 pos)
        {
            GameObject lamp = new("Lantern");
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = pos;

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.SetParent(lamp.transform, false);
            pole.transform.localPosition = new Vector3(0, .8f, 0);
            pole.transform.localScale = new Vector3(.05f, .8f, .05f);
            pole.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(lamp.transform, false);
            cube.transform.localPosition = new Vector3(0, 1.7f, 0);
            cube.transform.localScale = Vector3.one * .18f;
            cube.GetComponent<Renderer>().material = ZoneFactory.CreateMat(GoldColor);

            Light l = lamp.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 5;
            l.intensity = 2;
            l.color = new Color(1f, .9f, .7f);
        }

        static void CreateGardenBed(Transform parent, Vector3 pos)
        {
            GameObject bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.transform.SetParent(parent, false);
            bed.transform.localPosition = pos;
            bed.transform.localScale = new Vector3(.9f, .18f, .9f);
            bed.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.42f, .28f, .15f));

            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soil.transform.SetParent(bed.transform, false);
            soil.transform.localPosition = new Vector3(0, .55f, 0);
            soil.transform.localScale = new Vector3(.88f, .25f, .88f);
            soil.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.22f, .16f, .1f));

            GameObject plot = new("GardenPlot");
            plot.transform.SetParent(parent, false);
            plot.transform.localPosition = pos;

            GardenPlot gp = plot.AddComponent<GardenPlot>();
            ZoneFactory.SetField(gp, "_growthDuration", 15f);
            ZoneFactory.SetField(gp, "_harvestReward", 20);
        }

        static void CreateGardenPlot(Transform parent, Vector3 pos)
        {
            CreateGardenBed(parent, pos);
        }

        private static void CreateFireflies(Transform parent, int count)
        {
            GameObject root = new GameObject("Fireflies");
            root.transform.SetParent(parent, false);

            for (int i = 0; i < count; i++)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                g.transform.SetParent(root.transform, false);
                g.transform.localPosition = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(.3f, 2f),
                    Random.Range(-5f, 5f));
                g.transform.localScale = Vector3.one * .05f;
                g.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(1f, .95f, .45f));
                Object.Destroy(g.GetComponent<Collider>());
            }
        }
    }
}
