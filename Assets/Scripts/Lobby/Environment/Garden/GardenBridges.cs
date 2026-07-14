using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Sacred Sky Garden Sanctuary - Bridges and Stairways
    /// </summary>
    public static partial class GardenZoneBuilder
    {
        private static void BuildCurvedBridge(Transform parent, Vector3 start, Vector3 end)
        {
            GameObject root = new GameObject("SkyBridge");
            root.transform.SetParent(parent, false);

            const int pieces = 14;
            for (int i = 0; i < pieces; i++)
            {
                float t = i / (float)(pieces - 1);
                Vector3 p = Vector3.Lerp(start, end, t);

                p.y += Mathf.Sin(t * Mathf.PI) * 0.45f;
                p.x += Mathf.Sin(t * Mathf.PI * 1.2f) * 0.8f;

                GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.transform.SetParent(root.transform, false);
                slab.transform.localPosition = p;
                slab.transform.localScale = new Vector3(1.15f, .12f, .75f);

                slab.transform.localRotation = Quaternion.Euler(0, Mathf.Sin(t * 5f) * 18f, 0);
                slab.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);

                if (i < pieces - 1)
                {
                    CreateBridgeRail(root.transform, p, 1);
                    CreateBridgeRail(root.transform, p, -1);
                }
            }
        }

        private static void CreateBridgeRail(Transform parent, Vector3 center, int side)
        {
            Vector3 offset = new Vector3(side * .55f, .45f, 0);

            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.transform.SetParent(parent, false);
            post.transform.localPosition = center + offset;
            post.transform.localScale = new Vector3(.035f, .35f, .035f);
            post.GetComponent<Renderer>().material = ZoneFactory.CreateMat(MarbleColor);

            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.transform.SetParent(post.transform, false);
            cap.transform.localPosition = new Vector3(0, 1.05f, 0);
            cap.transform.localScale = Vector3.one * 1.5f;
            cap.GetComponent<Renderer>().material = ZoneFactory.CreateMat(GoldColor);
        }
    }
}
