using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Sacred Sky Garden Sanctuary - High Sanctuary, Sakura Tree, Gazebo, and Overlook
    /// </summary>
    public static partial class GardenZoneBuilder
    {
        private static void BuildHighSanctuary(Transform parent)
        {
            var hill = CreateIsland(
                parent,
                "HighSanctuary",
                new Vector3(0, 0f, 12f),
                5.5f,
                2.4f,
                GrassColor,
                DirtColor);

            BuildSacredTree(hill.transform, new Vector3(-2.4f, 0, -0.8f));
            CreateGardenPlot(hill.transform, new Vector3(-3.2f, 0.05f, -2.0f));
            CreateGardenPlot(hill.transform, new Vector3(-1.7f, 0.05f, -2.3f));
            BuildGazebo(hill.transform, new Vector3(2.7f, 0, 0.5f));
            BuildOverlook(hill.transform, new Vector3(0, 0, 4.2f));
            ScatterFlowers(hill.transform, 28);
        }

        private static void BuildSacredTree(Transform parent, Vector3 pos)
        {
            var root = new GameObject("SacredTree");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localPosition = new Vector3(0, 1.2f, 0);
            trunk.transform.localScale = new Vector3(.35f, 1.2f, .35f);
            trunk.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.42f, .28f, .2f));

            Vector3[] offsets = {
                new(-.8f, 2.7f, -.4f),
                new(.8f, 2.6f, .3f),
                new(0, 3.1f, .8f),
                new(0, 3.4f, 0),
                new(-.4f, 3.2f, .7f)
            };

            foreach (var p in offsets)
            {
                GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c.transform.SetParent(root.transform, false);
                c.transform.localPosition = p;
                c.transform.localScale = Vector3.one * Random.Range(1.4f, 2.2f);

                Color col = Color.Lerp(
                    new Color(1f, .75f, .85f),
                    new Color(1f, .93f, .65f),
                    Random.value);

                c.GetComponent<Renderer>().material = ZoneFactory.CreateMat(col);
                Object.Destroy(c.GetComponent<Collider>());
            }

            for (int i = 0; i < 12; i++)
            {
                Vector3 p = new Vector3(
                    Random.Range(-1.5f, 1.5f),
                    Random.Range(1.8f, 3.2f),
                    Random.Range(-1.5f, 1.5f));

                CreateHangingLantern(root.transform, p);
            }
        }

        private static void BuildGazebo(Transform parent, Vector3 pos)
        {
            var root = new GameObject("Gazebo");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.transform.SetParent(root.transform, false);
            floor.transform.localScale = new Vector3(2.8f, .08f, 2.8f);
            floor.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);

            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                Vector3 p = new Vector3(
                    Mathf.Cos(a) * 1.2f,
                    1f,
                    Mathf.Sin(a) * 1.2f);

                GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                col.transform.SetParent(root.transform, false);
                col.transform.localPosition = p;
                col.transform.localScale = new Vector3(.08f, 1f, .08f);
                col.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);
            }

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            roof.transform.SetParent(root.transform, false);
            roof.transform.localPosition = new Vector3(0, 2.25f, 0);
            roof.transform.localScale = new Vector3(2.8f, .8f, 2.8f);
            roof.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = new Vector3(0, 1.95f, 0);
            ring.transform.localScale = new Vector3(2.45f, .04f, 2.45f);
            ring.GetComponent<Renderer>().material = ZoneFactory.CreateMat(GoldColor);

            CreateTeaTable(root.transform);
        }

        private static void CreateTeaTable(Transform parent)
        {
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            table.transform.SetParent(parent, false);
            table.transform.localPosition = new Vector3(0, .35f, 0);
            table.transform.localScale = new Vector3(.55f, .35f, .55f);
            table.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);

            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2 / 3;
                GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                seat.transform.SetParent(parent, false);
                seat.transform.localPosition = new Vector3(Mathf.Cos(a) * .8f, .18f, Mathf.Sin(a) * .8f);
                seat.transform.localScale = new Vector3(.28f, .18f, .28f);
                seat.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);
            }
        }

        private static void BuildOverlook(Transform parent, Vector3 pos)
        {
            var root = new GameObject("Overlook");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;

            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(root.transform, false);
            deck.transform.localScale = new Vector3(2.6f, .12f, 2f);
            deck.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.72f, .52f, .34f));

            for (int i = 0; i < 5; i++)
            {
                float x = -1.2f + i * .6f;
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.transform.SetParent(root.transform, false);
                post.transform.localPosition = new Vector3(x, .45f, .9f);
                post.transform.localScale = new Vector3(.05f, .45f, .05f);
                post.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);
            }

            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.SetParent(root.transform, false);
            rail.transform.localPosition = new Vector3(0, .82f, .9f);
            rail.transform.localScale = new Vector3(2.45f, .05f, .05f);
            rail.GetComponent<Renderer>().material = ZoneFactory.CreateMat(GoldColor);

            CreateBench(root.transform, new Vector3(-.7f, .15f, -.35f));
            CreateBench(root.transform, new Vector3(.7f, .15f, -.35f));
        }

        private static void CreateBench(Transform parent, Vector3 pos)
        {
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(parent, false);
            seat.transform.localPosition = pos;
            seat.transform.localScale = new Vector3(.7f, .12f, .25f);
            seat.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.55f, .36f, .2f));

            for (int i = -1; i <= 1; i += 2)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(seat.transform, false);
                leg.transform.localPosition = new Vector3(i * .25f, -.6f, 0);
                leg.transform.localScale = new Vector3(.12f, .6f, .12f);
                leg.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.45f, .3f, .18f));
            }
        }

        private static void ScatterFlowers(Transform parent, int count)
        {
            Color[] cols = {
                new Color(1f, .7f, .8f),
                new Color(.65f, .55f, 1f),
                new Color(.5f, .8f, 1f),
                new Color(1f, .9f, .5f)
            };

            for (int i = 0; i < count; i++)
            {
                Vector2 p = Random.insideUnitCircle * 4f;
                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.transform.SetParent(parent, false);
                stem.transform.localPosition = new Vector3(p.x, .08f, p.y);
                stem.transform.localScale = new Vector3(.015f, .08f, .015f);
                stem.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.22f, .5f, .22f));

                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.transform.SetParent(parent, false);
                head.transform.localPosition = new Vector3(p.x, .18f, p.y);
                head.transform.localScale = Vector3.one * .12f;
                head.GetComponent<Renderer>().material = ZoneFactory.CreateMat(cols[Random.Range(0, cols.Length)]);
                Object.Destroy(head.GetComponent<Collider>());
            }
        }
    }
}
