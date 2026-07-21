using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Static factory helpers shared by all zone builders.
    /// </summary>
    public static class ZoneFactory
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

        public static GameObject CreateFenceSegment(Vector3 start, Vector3 end, Color color, bool rotate90 = false)
        {
            var segment = new GameObject("FenceSegment");
            segment.transform.position = start;
            float length = Vector3.Distance(start, end);
            if (rotate90) segment.transform.rotation = Quaternion.Euler(0, 90, 0);

            var fenceCollider = segment.AddComponent<BoxCollider>();
            fenceCollider.center = new Vector3(length * 0.5f, 0.5f, 0);
            fenceCollider.size = new Vector3(length, 1.0f, 0.2f);

            float postHeight = 0.9f;
            float postWidth = 0.06f;

            // Left post
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(segment.transform, false);
            post.transform.localPosition = new Vector3(0, postHeight * 0.5f, 0);
            post.transform.localScale = new Vector3(postWidth, postHeight, postWidth);
            post.GetComponent<Renderer>().material = CreateMat(color);
            Object.Destroy(post.GetComponent<Collider>());

            // Cap on left post
            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Cap";
            cap.transform.SetParent(segment.transform, false);
            cap.transform.localPosition = new Vector3(0, postHeight + 0.015f, 0);
            cap.transform.localScale = Vector3.one * (postWidth * 1.4f);
            cap.GetComponent<Renderer>().material = CreateMat(color * 0.95f);
            Object.Destroy(cap.GetComponent<Collider>());

            // Horizontal rails
            float[] railHeights = { 0.25f, 0.55f };
            foreach (float rh in railHeights)
            {
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = "Rail";
                rail.transform.SetParent(segment.transform, false);
                rail.transform.localPosition = new Vector3(length * 0.5f, rh, 0);
                rail.transform.localScale = new Vector3(length, 0.04f, 0.03f);
                rail.GetComponent<Renderer>().material = CreateMat(color);
                Object.Destroy(rail.GetComponent<Collider>());
            }

            // Pickets
            int picketCount = Mathf.Max(2, Mathf.FloorToInt(length / 0.18f));
            float spacing = length / picketCount;
            for (int i = 1; i < picketCount; i++)
            {
                var picket = new GameObject("Picket");
                picket.transform.SetParent(segment.transform, false);
                picket.transform.localPosition = new Vector3(i * spacing, 0, 0);

                float picketH = 0.72f;
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(picket.transform, false);
                body.transform.localPosition = new Vector3(0, picketH * 0.5f, 0);
                body.transform.localScale = new Vector3(0.04f, picketH, 0.02f);
                body.GetComponent<Renderer>().material = CreateMat(color);
                Object.Destroy(body.GetComponent<Collider>());

                var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tip.name = "Tip";
                tip.transform.SetParent(picket.transform, false);
                tip.transform.localPosition = new Vector3(0, picketH + 0.04f, 0);
                tip.transform.localScale = new Vector3(0.04f, 0.08f, 0.02f);
                tip.transform.localRotation = Quaternion.Euler(0, 0, 45);
                tip.GetComponent<Renderer>().material = CreateMat(color);
                Object.Destroy(tip.GetComponent<Collider>());
            }

            // Right end post
            var postEnd = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postEnd.name = "PostEnd";
            postEnd.transform.SetParent(segment.transform, false);
            postEnd.transform.localPosition = new Vector3(length, postHeight * 0.5f, 0);
            postEnd.transform.localScale = new Vector3(postWidth, postHeight, postWidth);
            postEnd.GetComponent<Renderer>().material = CreateMat(color);
            Object.Destroy(postEnd.GetComponent<Collider>());

            var capEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            capEnd.name = "CapEnd";
            capEnd.transform.SetParent(segment.transform, false);
            capEnd.transform.localPosition = new Vector3(length, postHeight + 0.015f, 0);
            capEnd.transform.localScale = Vector3.one * (postWidth * 1.4f);
            capEnd.GetComponent<Renderer>().material = CreateMat(color * 0.95f);
            Object.Destroy(capEnd.GetComponent<Collider>());

            return segment;
        }

        // ── Teleportation Portal ──

        public static void CreatePortal(string name, Vector3 pos, float rotY,
            Color ringColor, Color energyColor, string label)
        {
            var portal = new GameObject(name);
            portal.transform.position = pos;
            portal.transform.rotation = Quaternion.Euler(0, rotY, 0);

            float padRadius = 0.7f;

            // 1. Base Ring
            var basePad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePad.name = "TeleportPad";
            basePad.transform.SetParent(portal.transform, false);
            basePad.transform.localPosition = new Vector3(0, 0.02f, 0);
            basePad.transform.localScale = new Vector3(padRadius * 2, 0.02f, padRadius * 2);
            basePad.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.12f, 0.14f, 0.18f));
            Object.DestroyImmediate(basePad.GetComponent<Collider>());

            // 2. Neon Border
            var neonBorder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neonBorder.name = "NeonBorder";
            neonBorder.transform.SetParent(portal.transform, false);
            neonBorder.transform.localPosition = new Vector3(0, 0.03f, 0);
            neonBorder.transform.localScale = new Vector3(padRadius * 1.85f, 0.022f, padRadius * 1.85f);
            neonBorder.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(energyColor);
            Object.Destroy(neonBorder.GetComponent<Collider>());

            // 3. Inner Plate
            var innerPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerPlate.name = "InnerPlate";
            innerPlate.transform.SetParent(portal.transform, false);
            innerPlate.transform.localPosition = new Vector3(0, 0.04f, 0);
            innerPlate.transform.localScale = new Vector3(padRadius * 1.5f, 0.024f, padRadius * 1.5f);
            innerPlate.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.08f, 0.09f, 0.11f));
            Object.Destroy(innerPlate.GetComponent<Collider>());

            // 4. Core Glow
            var coreGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coreGlow.name = "CoreGlow";
            coreGlow.transform.SetParent(portal.transform, false);
            coreGlow.transform.localPosition = new Vector3(0, 0.05f, 0);
            coreGlow.transform.localScale = new Vector3(padRadius * 1.1f, 0.026f, padRadius * 1.1f);
            Color intenseEnergy = new Color(energyColor.r * 1.2f, energyColor.g * 1.2f, energyColor.b * 1.2f, 0.9f);
            coreGlow.GetComponent<Renderer>().material = CharacterVisuals.CreateAdditiveMat(intenseEnergy);
            Object.Destroy(coreGlow.GetComponent<Collider>());

            // 5. Holo Beam
            var holoBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holoBeam.name = "HoloBeam";
            holoBeam.transform.SetParent(portal.transform, false);
            holoBeam.transform.localPosition = new Vector3(0, 0.6f, 0);
            holoBeam.transform.localScale = new Vector3(padRadius * 1.1f, 0.6f, padRadius * 1.1f);
            Color beamColor = new Color(energyColor.r, energyColor.g, energyColor.b, 0.22f);
            holoBeam.GetComponent<Renderer>().material = CharacterVisuals.CreateAdditiveMat(beamColor);
            Object.Destroy(holoBeam.GetComponent<Collider>());

            // 6. Corner Beacons
            float dist = padRadius * 0.8f;
            Vector3[] beaconOffsets = {
                new(dist, 0.05f, dist), new(-dist, 0.05f, dist),
                new(dist, 0.05f, -dist), new(-dist, 0.05f, -dist)
            };
            foreach (var offset in beaconOffsets)
            {
                var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beacon.name = "BeaconStud";
                beacon.transform.SetParent(portal.transform, false);
                beacon.transform.localPosition = offset;
                beacon.transform.localScale = new Vector3(0.08f, 0.05f, 0.08f);
                beacon.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.25f, 0.28f, 0.32f));
                Object.Destroy(beacon.GetComponent<Collider>());

                var laserLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                laserLine.name = "LaserLine";
                laserLine.transform.SetParent(portal.transform, false);
                laserLine.transform.localPosition = offset + new Vector3(0, 0.5f, 0);
                laserLine.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);
                Color laserColor = new Color(energyColor.r * 1.5f, energyColor.g * 1.5f, energyColor.b * 1.5f, 0.4f);
                laserLine.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(laserColor);
                Object.Destroy(laserLine.GetComponent<Collider>());
            }

            // 7. Floating Sign (Skip if label is empty)
            if (!string.IsNullOrEmpty(label))
            {
                var signObj = new GameObject("PortalSign");
                signObj.transform.SetParent(portal.transform, false);
                signObj.transform.localPosition = new Vector3(0, 1.8f, 0);

                var signTmp = signObj.AddComponent<TextMeshPro>();
                signTmp.text = label;
                signTmp.fontSize = 3.2f;
                signTmp.alignment = TextAlignmentOptions.Center;
                signTmp.color = Color.white;
                signObj.AddComponent<BillboardText>();

                var signBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                signBg.name = "SignBg";
                signBg.transform.SetParent(signObj.transform, false);
                signBg.transform.localPosition = new Vector3(0, 0, 0.01f);
                signBg.transform.localScale = new Vector3(1.6f, 0.45f, 0.005f);
                Color glassColor = new Color(energyColor.r * 0.2f, energyColor.g * 0.2f, energyColor.b * 0.2f, 0.45f);
                signBg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(glassColor);
                Object.Destroy(signBg.GetComponent<Collider>());

                var neonFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                neonFrame.name = "SignNeonFrame";
                neonFrame.transform.SetParent(signObj.transform, false);
                neonFrame.transform.localPosition = new Vector3(0, 0, 0.005f);
                neonFrame.transform.localScale = new Vector3(1.62f, 0.47f, 0.002f);
                neonFrame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(energyColor * 0.8f);
                Object.Destroy(neonFrame.GetComponent<Collider>());
            }
        }

        // ── Cloud Visual ──

        public static void CreateCloudVisual(GameObject parent, float radius)
        {
            Color cloudColor = new Color(1f, 1f, 1f, 0.95f);
            float thickness = 0.18f;

            var c1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c1.transform.SetParent(parent.transform, false);
            c1.transform.localPosition = Vector3.zero;
            c1.transform.localScale = new Vector3(radius * 2, thickness, radius * 2);
            c1.GetComponent<Renderer>().material = CreateMat(cloudColor);

            var c2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c2.transform.SetParent(parent.transform, false);
            c2.transform.localPosition = new Vector3(radius * 0.3f, 0.01f, radius * 0.2f);
            c2.transform.localScale = new Vector3(radius * 1.2f, thickness * 1.1f, radius * 1.2f);
            c2.GetComponent<Renderer>().material = CreateMat(cloudColor);

            var c3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c3.transform.SetParent(parent.transform, false);
            c3.transform.localPosition = new Vector3(-radius * 0.3f, 0.01f, -radius * 0.2f);
            c3.transform.localScale = new Vector3(radius * 1.2f, thickness * 1.1f, radius * 1.2f);
            c3.GetComponent<Renderer>().material = CreateMat(cloudColor);
        }

        // ── Sub-Area Floor ──

        /// <summary>
        /// Creates a distinct floor slab for a sub-area, parented to the zone.
        /// Slightly raised above ground to visually separate areas.
        /// </summary>
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
        public static void CreateLowFence(Transform parent, Vector3 center, float length, float directionAngle, Color color, float height = 0.5f)
        {
            var fence = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fence.name = "LowFence";
            fence.transform.SetParent(parent, false);
            fence.transform.localPosition = center + Vector3.up * (height * 0.5f);
            fence.transform.localRotation = Quaternion.Euler(0, directionAngle, 0);
            fence.transform.localScale = new Vector3(length, height, 0.15f);
            fence.GetComponent<Renderer>().material = StoneMat(color);
            Object.Destroy(fence.GetComponent<Collider>());
        }

        // ── Area Sign ──

        /// <summary>
        /// Creates a compact 3D wooden signboard with short wooden legs, planted inside a sub-area.
        /// </summary>
        public static void CreateAreaSign(Transform parent, Vector3 localPos, string text, Color textColor = default)
        {
            if (textColor == default) textColor = new Color(0.95f, 0.95f, 0.95f);

            var signHolder = new GameObject("AreaSign_" + text.Replace(" ", ""));
            signHolder.transform.SetParent(parent, false);
            signHolder.transform.localPosition = localPos;

            Color woodDark = new Color(0.38f, 0.24f, 0.12f);
            Color boardWood = new Color(0.24f, 0.15f, 0.08f);

            // 1. Two Short Wooden Legs planted into the ground
            float postHeight = 0.55f;
            float postSpacing = 0.65f;
            Vector3[] postPositions = { new Vector3(-postSpacing * 0.5f, -postHeight * 0.5f, 0), new Vector3(postSpacing * 0.5f, -postHeight * 0.5f, 0) };
            foreach (var pPos in postPositions)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "SignPost";
                post.transform.SetParent(signHolder.transform, false);
                post.transform.localPosition = pPos;
                post.transform.localScale = new Vector3(0.05f, postHeight * 0.5f, 0.05f);
                post.GetComponent<Renderer>().material = WoodMat(woodDark);
                Object.Destroy(post.GetComponent<Collider>());
            }

            // 2. Small Wooden Board Frame
            var boardFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardFrame.name = "BoardFrame";
            boardFrame.transform.SetParent(signHolder.transform, false);
            boardFrame.transform.localPosition = Vector3.zero;
            boardFrame.transform.localScale = new Vector3(1.15f, 0.42f, 0.05f);
            boardFrame.GetComponent<Renderer>().material = WoodMat(woodDark);
            Object.Destroy(boardFrame.GetComponent<Collider>());

            // 3. Inner Dark Wooden Plate
            var innerPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            innerPlate.name = "InnerPlate";
            innerPlate.transform.SetParent(signHolder.transform, false);
            innerPlate.transform.localPosition = new Vector3(0, 0, -0.008f);
            innerPlate.transform.localScale = new Vector3(1.08f, 0.35f, 0.052f);
            innerPlate.GetComponent<Renderer>().material = WoodMat(boardWood);
            Object.Destroy(innerPlate.GetComponent<Collider>());

            // 4. Text on Front
            var textObj = new GameObject("SignText");
            textObj.transform.SetParent(signHolder.transform, false);
            textObj.transform.localPosition = new Vector3(0, 0, -0.04f);

            var tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 1.6f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;

            signHolder.AddComponent<BillboardText>();
        }

        // ── Gate (opening in a wall/fence) ──

        /// <summary>
        /// Creates two short pillars with a gap between them, forming a gate.
        /// </summary>
        public static void CreateGate(Transform parent, Vector3 localPos, float rotY, Color color, float gapWidth = 1.5f, float height = 1.2f)
        {
            var gate = new GameObject("Gate");
            gate.transform.SetParent(parent, false);
            gate.transform.localPosition = localPos;
            gate.transform.localRotation = Quaternion.Euler(0, rotY, 0);

            // Left pillar
            var pL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pL.name = "GatePillarL";
            pL.transform.SetParent(gate.transform, false);
            pL.transform.localPosition = new Vector3(-gapWidth * 0.5f - 0.15f, height * 0.5f, 0);
            pL.transform.localScale = new Vector3(0.3f, height, 0.3f);
            pL.GetComponent<Renderer>().material = StoneMat(color);
            Object.Destroy(pL.GetComponent<Collider>());

            // Right pillar
            var pR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pR.name = "GatePillarR";
            pR.transform.SetParent(gate.transform, false);
            pR.transform.localPosition = new Vector3(gapWidth * 0.5f + 0.15f, height * 0.5f, 0);
            pR.transform.localScale = new Vector3(0.3f, height, 0.3f);
            pR.GetComponent<Renderer>().material = StoneMat(color);
            Object.Destroy(pR.GetComponent<Collider>());

            // Lintel (horizontal beam above gate)
            var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "GateLintel";
            lintel.transform.SetParent(gate.transform, false);
            lintel.transform.localPosition = new Vector3(0, height + 0.08f, 0);
            lintel.transform.localScale = new Vector3(gapWidth + 0.6f, 0.15f, 0.35f);
            lintel.GetComponent<Renderer>().material = StoneMat(color * 0.9f);
            Object.Destroy(lintel.GetComponent<Collider>());
        }

        // ── Reflection helper ──

        public static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(target, value);
        }
    }
}
