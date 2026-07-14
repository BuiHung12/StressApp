using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Sacred Sky Garden Sanctuary - Procedural Island and Terrain generation
    /// </summary>
    public static partial class GardenZoneBuilder
    {
        static GameObject CreateIsland(
            GameObject parent,
            string name,
            Vector3 pos,
            float radius,
            float height)
        {
            GameObject island = new(name);
            island.transform.SetParent(parent.transform, false);
            island.transform.localPosition = pos;

            CreateGround(island, radius);
            CreateSoil(island, radius, height);
            CreateRockRing(island, radius, height);
            CreateRoots(island, radius, height);
            CreateGrassTufts(island, radius);

            return island;
        }

        static GameObject CreateIsland(
            Transform parent,
            string name,
            Vector3 pos,
            float radius,
            float height,
            Color grassCol,
            Color dirtCol)
        {
            GameObject island = new(name);
            island.transform.SetParent(parent, false);
            island.transform.localPosition = pos;

            // Custom CreateGround using passed grassCol
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.transform.SetParent(island.transform, false);
            top.transform.localScale = new Vector3(radius * 2f, .08f, radius * 2f);
            top.GetComponent<Renderer>().material = ZoneFactory.CreateMat(grassCol);
            Object.Destroy(top.GetComponent<CapsuleCollider>());
            BoxCollider box = top.AddComponent<BoxCollider>();
            box.size = new Vector3(radius * 1.7f, .2f, radius * 1.7f);

            for (int i = 0; i < 6; i++)
            {
                float a = i * 60 * Mathf.Deg2Rad;
                GameObject bump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bump.transform.SetParent(island.transform, false);
                bump.transform.localPosition = new Vector3(Mathf.Cos(a) * radius * .45f, 0, Mathf.Sin(a) * radius * .45f);
                float rr = Random.Range(.6f, 1.2f);
                bump.transform.localScale = new Vector3(rr, .07f, rr);
                bump.GetComponent<Renderer>().material = ZoneFactory.CreateMat(grassCol);
                Object.Destroy(bump.GetComponent<CapsuleCollider>());
            }

            // Custom CreateSoil using passed dirtCol
            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            soil.transform.SetParent(island.transform, false);
            soil.transform.localPosition = Vector3.down * (height * 0.5f + 0.04f);
            soil.transform.localScale = new Vector3(radius * 1.8f, height * 0.5f, radius * 1.8f);
            soil.GetComponent<Renderer>().material = ZoneFactory.CreateMat(dirtCol);
            Object.Destroy(soil.GetComponent<CapsuleCollider>());

            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = Random.Range(radius * .8f, radius * 1.05f);
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.transform.SetParent(island.transform, false);
                rock.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, Random.Range(-height * .6f, -.2f), Mathf.Sin(angle) * dist);
                rock.transform.localScale = new Vector3(Random.Range(.5f, 1.4f), Random.Range(.5f, 1.8f), Random.Range(.5f, 1.4f));
                rock.transform.localRotation = Random.rotation;
                rock.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Rock);
            }

            CreateRockRing(island, radius, height);
            CreateRoots(island, radius, height);
            CreateGrassTufts(island, radius);

            return island;
        }

        static void CreateGround(GameObject parent, float r)
        {
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.transform.SetParent(parent.transform, false);
            top.transform.localScale = new Vector3(r * 2f, .08f, r * 2f);
            top.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Grass);
            Object.Destroy(top.GetComponent<CapsuleCollider>());

            BoxCollider box = top.AddComponent<BoxCollider>();
            box.size = new Vector3(r * 1.7f, .2f, r * 1.7f);

            for (int i = 0; i < 6; i++)
            {
                float a = i * 60 * Mathf.Deg2Rad;
                GameObject bump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bump.transform.SetParent(parent.transform, false);
                bump.transform.localPosition = new Vector3(Mathf.Cos(a) * r * .45f, 0, Mathf.Sin(a) * r * .45f);
                float rr = Random.Range(.6f, 1.2f);
                bump.transform.localScale = new Vector3(rr, .07f, rr);
                bump.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Grass);
                Object.Destroy(bump.GetComponent<CapsuleCollider>());
            }
        }

        static void CreateSoil(GameObject parent, float r, float h)
        {
            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            soil.transform.SetParent(parent.transform, false);
            soil.transform.localPosition = Vector3.down * (h * 0.5f + 0.04f);
            soil.transform.localScale = new Vector3(r * 1.8f, h * 0.5f, r * 1.8f);
            soil.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Dirt);
            Object.Destroy(soil.GetComponent<CapsuleCollider>());

            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = Random.Range(r * .8f, r * 1.05f);
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.transform.SetParent(parent.transform, false);
                rock.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, Random.Range(-h * .6f, -.2f), Mathf.Sin(angle) * dist);
                rock.transform.localScale = new Vector3(Random.Range(.5f, 1.4f), Random.Range(.5f, 1.8f), Random.Range(.5f, 1.4f));
                rock.transform.localRotation = Random.rotation;
                rock.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Rock);
            }
        }

        static void CreateRockRing(GameObject parent, float r, float h)
        {
            for (int i = 0; i < 18; i++)
            {
                float angle = i * 20 * Mathf.Deg2Rad;
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.transform.SetParent(parent.transform, false);
                rock.transform.localPosition = new Vector3(Mathf.Cos(angle) * r * .95f, -.25f, Mathf.Sin(angle) * r * .95f);
                rock.transform.localScale = new Vector3(.5f + Random.value * .8f, .3f + Random.value * .7f, .5f + Random.value * .8f);
                rock.transform.localRotation = Quaternion.Euler(Random.Range(-15, 15), Random.Range(0, 360), Random.Range(-15, 15));
                rock.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Rock);
            }
        }

        static void CreateRoots(GameObject parent, float r, float h)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
                GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                root.transform.SetParent(parent.transform, false);
                root.transform.localPosition = new Vector3(Mathf.Cos(angle) * r * .85f, -.6f, Mathf.Sin(angle) * r * .85f);
                root.transform.localRotation = Quaternion.Euler(90, angle * Mathf.Rad2Deg, Random.Range(-30, 30));
                root.transform.localScale = new Vector3(.05f, Random.Range(.3f, .7f), .05f);
                root.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(.29f, .2f, .12f));
                Object.Destroy(root.GetComponent<CapsuleCollider>());
            }
        }

        static void CreateGrassTufts(GameObject parent, float r)
        {
            for (int i = 0; i < 35; i++)
            {
                float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
                float dist = Random.Range(.5f, r * .75f);
                Vector3 p = new Vector3(Mathf.Cos(angle) * dist, .03f, Mathf.Sin(angle) * dist);

                for (int j = 0; j < 4; j++)
                {
                    GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    blade.transform.SetParent(parent.transform, false);
                    blade.transform.localPosition = p + new Vector3(Random.Range(-.1f, .1f), .08f, Random.Range(-.1f, .1f));
                    blade.transform.localScale = new Vector3(.03f, Random.Range(.15f, .28f), .03f);
                    blade.transform.localRotation = Quaternion.Euler(Random.Range(-20, 20), Random.Range(0, 360), Random.Range(-10, 10));
                    blade.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Leaf);
                    Object.Destroy(blade.GetComponent<BoxCollider>());
                }
            }
        }
    }
}
