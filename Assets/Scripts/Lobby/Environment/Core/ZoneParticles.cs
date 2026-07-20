using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Static helpers to create ParticleSystem effects via code for zone atmosphere.
    /// No external assets needed — uses built-in Default-Particle material.
    /// </summary>
    public static class ZoneParticles
    {
        /// <summary>
        /// Floating dust/pollen/firefly particles drifting in an area.
        /// </summary>
        public static ParticleSystem CreateFloatingParticles(
            Transform parent, Color color, int maxCount, Vector3 areaSize, string name = "FloatingParticles")
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = new Vector3(0, areaSize.y * 0.5f, 0);

            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 6f;
            main.startSpeed = 0.15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor = color;
            main.maxParticles = maxCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.02f; // slight upward drift

            var emission = ps.emission;
            emission.rateOverTime = maxCount / 4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = areaSize;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 0.5f),
                    new GradientColorKey(color, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.8f, 0.3f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.3f;
            noise.frequency = 0.5f;

            // Use built-in particle material
            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial(color);

            return ps;
        }

        /// <summary>
        /// Fire ember particles rising from a point.
        /// </summary>
        public static ParticleSystem CreateFireEmbers(Transform parent, Vector3 localPos, Color color)
        {
            var obj = new GameObject("FireEmbers");
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;

            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main.startColor = color;
            main.maxParticles = 30;
            main.gravityModifier = -0.15f; // rise up

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f),
                    new GradientColorKey(new Color(0.3f, 0.1f, 0f), 1f)
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.6f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 1.5f;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial(color);

            return ps;
        }

        /// <summary>
        /// Gentle water surface ripple particles (flat, on pond surface).
        /// </summary>
        public static ParticleSystem CreateWaterRipples(Transform parent, Vector3 localPos, float radius)
        {
            var obj = new GameObject("WaterRipples");
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;

            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 3f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = new Color(1f, 1f, 1f, 0.3f);
            main.maxParticles = 15;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation3D = false;

            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius * 0.8f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 1.5f)
            ));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.3f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial(new Color(0.7f, 0.85f, 1f, 0.4f));
            renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;

            return ps;
        }

        /// <summary>
        /// Smoke/haze that slowly drifts (prison, industrial).
        /// </summary>
        public static ParticleSystem CreateSmoke(Transform parent, Vector3 localPos, Color color)
        {
            var obj = new GameObject("Smoke");
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;

            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startColor = color;
            main.maxParticles = 20;
            main.gravityModifier = -0.03f;

            var emission = ps.emission;
            emission.rateOverTime = 3f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(1f, 1f)
            ));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color * 0.7f, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.4f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial(color);

            return ps;
        }

        // ── Helper ──

        private static Material GetParticleMaterial(Color tint)
        {
            // Use Default-Particle or Particles/Standard Unlit
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Particles/Alpha Blended");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("UI/Default");

            Material mat = null;
            try
            {
                if (shader != null)
                {
                    mat = new Material(shader);
                    mat.color = tint;
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive for glow
                    mat.renderQueue = 3100;
                }
                else
                {
                    Debug.LogWarning("[ZoneParticles] All particle shaders were stripped/null.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ZoneParticles] Failed to create particle material: {e.Message}");
            }
            return mat;
        }
    }
}
