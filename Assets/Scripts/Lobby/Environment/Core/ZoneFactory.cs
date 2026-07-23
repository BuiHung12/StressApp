using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Static factory helpers shared by all zone builders.
    /// </summary>
    public static partial class ZoneFactory
    {
        // ── Materials ──

        // Cache key that includes color + material properties
        private struct MatKey : System.IEquatable<MatKey>
        {
            public Color color;
            public float smoothness;
            public float metallic;

            public MatKey(Color c, float s, float m) { color = c; smoothness = s; metallic = m; }
            public bool Equals(MatKey other) => color.Equals(other.color) && smoothness == other.smoothness && metallic == other.metallic;
            public override bool Equals(object obj) => obj is MatKey k && Equals(k);
            public override int GetHashCode() => color.GetHashCode() ^ (smoothness.GetHashCode() << 8) ^ (metallic.GetHashCode() << 16);
        }

        private static readonly System.Collections.Generic.Dictionary<MatKey, Material> _matCache
            = new System.Collections.Generic.Dictionary<MatKey, Material>();

        /// <summary>
        /// Create a lit material with default low smoothness (matte).
        /// Uses URP/Lit for shadows and depth — NOT Unlit/Color.
        /// </summary>
        public static Material CreateMat(Color color)
        {
            return CreateLitMat(color, 0.1f, 0.0f);
        }

        /// <summary>
        /// Create a lit material with custom smoothness and metallic values.
        /// </summary>
        /// <param name="smoothness">0 = rough/matte, 1 = mirror-like</param>
        /// <param name="metallic">0 = non-metal, 1 = full metal</param>
        public static Material CreateLitMat(Color color, float smoothness, float metallic)
        {
            var key = new MatKey(color, smoothness, metallic);
            if (_matCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            // Prefer URP/Lit for shadows + specular, fallback to Standard, then Unlit
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            var mat = new Material(shader);
            mat.color = color;

            // Set PBR properties
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", smoothness);
            else if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", smoothness);

            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", metallic);

            // Transparency support
            if (color.a < 1f)
            {
                // URP transparency
                if (mat.HasProperty("_Surface"))
                {
                    mat.SetFloat("_Surface", 1); // 1 = Transparent
                    mat.SetFloat("_Blend", 0);   // 0 = Alpha
                }
                // Standard shader transparency
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            _matCache[key] = mat;
            return mat;
        }

        // ── Material Presets ──

        /// <summary>Polished wood — desks, shelves, fences</summary>
        public static Material WoodMat(Color color) => CreateLitMat(color, 0.35f, 0.0f);

        /// <summary>Metal — bars, lamp posts, rails</summary>
        public static Material MetalMat(Color color) => CreateLitMat(color, 0.6f, 0.8f);

        /// <summary>Stone/concrete — walls, rocks, ground</summary>
        public static Material StoneMat(Color color) => CreateLitMat(color, 0.05f, 0.0f);

        /// <summary>Water — ponds, fountains</summary>
        public static Material WaterMat(Color color) => CreateLitMat(color, 0.9f, 0.0f);

        /// <summary>Glossy — books, tiles, glass</summary>
        public static Material GlossyMat(Color color) => CreateLitMat(color, 0.7f, 0.1f);

        // ── Zone Lighting ──

        /// <summary>
        /// Creates directional fill light + point fill for a zone.
        /// </summary>
        public static void CreateZoneLighting(Transform parent, Color lightColor, float intensity, Vector3 direction)
        {
            // Main directional-like fill (using spot at distance for zone-local effect)
            var mainLight = new GameObject("ZoneFillLight");
            mainLight.transform.SetParent(parent, false);
            mainLight.transform.localPosition = new Vector3(0, 8f, 0);
            mainLight.transform.localRotation = Quaternion.LookRotation(direction);
            var light = mainLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColor;
            light.range = 30f;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.6f;

            // Secondary low fill to soften dark side
            var fillLight = new GameObject("ZoneAmbientFill");
            fillLight.transform.SetParent(parent, false);
            fillLight.transform.localPosition = new Vector3(0, 4f, 0);
            var fill = fillLight.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = Color.Lerp(lightColor, Color.white, 0.5f);
            fill.range = 25f;
            fill.intensity = intensity * 0.3f;
            fill.shadows = LightShadows.None;
        }

        // ── Flat Quad on Ground ──

        public static GameObject CreateFlat(string name, Vector3 pos, Vector2 size, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            obj.transform.localScale = new Vector3(size.x, size.y, 1f);
            obj.GetComponent<Renderer>().material = CreateMat(color);
            obj.isStatic = true;
            Object.Destroy(obj.GetComponent<Collider>());
            return obj;
        }

        // ── Fence Segment ──

        public static GameObject CreateSubAreaFloor(Transform parent, Vector3 localPos, Vector3 size, Color color, string name = "SubFloor")
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = name;
            floor.transform.SetParent(parent, false);
            floor.transform.localPosition = localPos + new Vector3(0, 0.02f, 0);
            floor.transform.localScale = new Vector3(size.x, 0.04f, size.z);
            floor.GetComponent<Renderer>().material = StoneMat(color);
            Object.Destroy(floor.GetComponent<Collider>());
            return floor;
        }

        // ── Low Fence (border for sub-areas) ──

        /// <summary>
        /// Creates a low fence/wall along one edge. Direction: 0=North(+Z), 90=East(+X), 180=South(-Z), 270=West(-X).
        /// </summary>

}
}
