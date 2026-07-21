using System.Collections.Generic;
using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Hiệu ứng đám mây bụi đấm nhau (Cartoon Fight Dust Cloud).
    /// Hút 2 nhân vật vào trong đám mây, tung bụi, tay chân đấm đá bay ra, 
    /// sau đó trả lại vị trí kèm theo mặt sưng tấy!
    /// </summary>
    public class FightCloudEffect : MonoBehaviour
    {
        private Transform _playerTrans;
        private Transform _targetTrans;
        private Vector3 _originalPlayerScale;
        private Vector3 _originalTargetTransScale;
        private Vector3 _midPoint;

        private float _duration = 1.5f;
        private float _elapsed = 0f;

        private Transform[] _puffs;
        private float[] _puffSpeeds;
        private float[] _puffOffsets;
        private Vector3[] _puffBaseScales;
        private float[] _puffOrbitAngles;
        private float[] _puffOrbitSpeeds;
        private float[] _puffOrbitRadii;
        private float[] _puffHeights;

        private float _limbSpawnTimer = 0f;
        private float _starSpawnTimer = 0f;

        /// <summary>
        /// Kiểm tra xem một nhân vật có đang trong trận đánh nhau nào không.
        /// </summary>
        public static bool IsInFight(Transform trans)
        {
            if (trans == null) return false;
            var effects = FindObjectsByType<FightCloudEffect>(FindObjectsSortMode.None);
            if (effects == null) return false;
            foreach (var effect in effects)
            {
                if (effect._playerTrans == trans || effect._targetTrans == trans)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Kích hoạt trận đấm nhau dạng đám mây bụi giữa Player và Target.
        /// </summary>
        public static void Create(Transform player, Transform target, float duration = 1.5f)
        {
            if (player == null || target == null) return;
            if (IsInFight(player) || IsInFight(target)) return;

            var go = new GameObject("FightCloud");
            var effect = go.AddComponent<FightCloudEffect>();
            effect.Initialize(player, target, duration);
        }

        private void Initialize(Transform player, Transform target, float duration)
        {
            _playerTrans = player;
            _targetTrans = target;
            _duration = duration;

            // Midpoint of the fight
            _midPoint = (player.position + target.position) * 0.5f;
            _midPoint.y = 0.4f; // Centered around character torso height
            transform.position = _midPoint;

            // Store and hide characters
            _originalPlayerScale = player.localScale;
            _originalTargetTransScale = target.localScale;

            player.localScale = Vector3.zero;
            target.localScale = Vector3.zero;

            // Create dust cloud puffs
            int puffCount = 14; // High puff count for dense cartoon whirlwind look
            _puffs = new Transform[puffCount];
            _puffSpeeds = new float[puffCount];
            _puffOffsets = new float[puffCount];
            _puffBaseScales = new Vector3[puffCount];
            _puffOrbitAngles = new float[puffCount];
            _puffOrbitSpeeds = new float[puffCount];
            _puffOrbitRadii = new float[puffCount];
            _puffHeights = new float[puffCount];

            Color cloudColor = new Color(0.93f, 0.93f, 0.93f, 0.96f); // Bright clean white dust
            var mat = CreateUnlitMat(cloudColor);

            for (int i = 0; i < puffCount; i++)
            {
                var puff = CreateVisualObject($"Puff_{i}", PrimitiveType.Sphere, mat);
                puff.transform.SetParent(transform, false);

                // Set up 2D orbital properties (in the local XY plane)
                _puffOrbitAngles[i] = i * (360f / puffCount) + Random.Range(-15f, 15f);
                _puffOrbitSpeeds[i] = Random.Range(180f, 360f) * (Random.value > 0.5f ? 1f : -1f); // Orbit clockwise or counterclockwise
                _puffOrbitRadii[i] = Random.Range(0.08f, 0.25f);
                _puffHeights[i] = Random.Range(-0.1f, 0.1f);

                // Highly irregular oval scales, but flattened on Z (thickness) to look like 2D flat disks
                float sizeX = Random.Range(0.4f, 0.9f);
                float sizeY = Random.Range(0.4f, 0.8f);
                float sizeZ = 0.001f; // Paper-thin flat disk
                puff.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);

                // Random 2D rotation (only around the Z axis)
                puff.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

                // Store info
                _puffs[i] = puff.transform;
                _puffBaseScales[i] = puff.transform.localScale;
                _puffSpeeds[i] = Random.Range(8f, 15f);
                _puffOffsets[i] = Random.Range(0f, 2f * Mathf.PI);
            }

            // Play camera shake via LobbyCamera
            var cam = Camera.main != null ? Camera.main.GetComponent<LobbyCamera>() : null;
            // (LobbyCamera follows automatically, but we can shake the whole game view slightly)
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;

            if (t >= 1.0f)
            {
                FinishFight();
                return;
            }

            // Keep the entire effect billboard facing the camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            // 1. Animate dust puffs (orbiting, bobbing, shifting on flat 2D plane)
            for (int i = 0; i < _puffs.Length; i++)
            {
                if (_puffs[i] == null) continue;

                // Orbit around the center in the local XY plane
                _puffOrbitAngles[i] += _puffOrbitSpeeds[i] * Time.deltaTime;
                float angleRad = _puffOrbitAngles[i] * Mathf.Deg2Rad;
                float r = _puffOrbitRadii[i];

                // Add slight Z offset to prevent depth fighting between layers
                _puffs[i].localPosition = new Vector3(
                    Mathf.Cos(angleRad) * r,
                    _puffHeights[i] + Mathf.Sin(Time.time * 6f + _puffOffsets[i]) * 0.03f,
                    i * 0.001f
                );

                // Pulsate scale only on X and Y (keeping Z flat)
                float sin = Mathf.Sin(Time.time * _puffSpeeds[i] + _puffOffsets[i]);
                float cos = Mathf.Cos(Time.time * (_puffSpeeds[i] * 0.9f) + _puffOffsets[i]);
                _puffs[i].localScale = new Vector3(
                    _puffBaseScales[i].x * (1f + sin * 0.22f),
                    _puffBaseScales[i].y * (1f + cos * 0.28f),
                    _puffBaseScales[i].z
                );

                // Rotate only around the local Z-axis (2D spin)
                _puffs[i].Rotate(Vector3.forward, 90f * Time.deltaTime);
            }

            // Slowly slide the whole cloud slightly left/right/up to feel alive
            transform.position = _midPoint + new Vector3(
                Mathf.Sin(Time.time * 15f) * 0.02f,
                Mathf.Cos(Time.time * 12f) * 0.01f,
                Mathf.Sin(Time.time * 9f) * 0.02f
            );

            // 2. Spawn limbs (white gloved arms and black legs thò thụt)
            _limbSpawnTimer += Time.deltaTime;
            if (_limbSpawnTimer > 0.09f) // Spawn slightly faster
            {
                _limbSpawnTimer = 0f;
                SpawnLimb();
            }

            // 3. Spawn stars popping out
            _starSpawnTimer += Time.deltaTime;
            if (_starSpawnTimer > 0.08f)
            {
                _starSpawnTimer = 0f;
                SpawnFightStar();
            }
        }

        private void SpawnLimb()
        {
            var limb = new GameObject("Limb");
            limb.transform.SetParent(transform, false);

            // Random direction restricted to the local XY plane
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f).normalized;

            // Random limb type: Fist (white-gloved hand) or Foot (dark leg with brown shoe)
            bool isFist = Random.value > 0.5f;

            if (isFist)
            {
                // Cartoon White-Gloved Arm and Hand
                var whiteMat = CreateUnlitMat(Color.white);
                
                // Arm/Sleeve (Cylinder)
                var arm = CreateVisualObject("Arm", PrimitiveType.Cylinder, whiteMat);
                arm.transform.SetParent(limb.transform, false);
                arm.transform.localScale = new Vector3(0.04f, 0.15f, 0.001f); // Flattened on Z
                arm.transform.localPosition = new Vector3(0, 0, 0.07f);
                arm.transform.localRotation = Quaternion.Euler(90f, 0, 0); // Align on Z axis
                
                // Hand (Sphere)
                var hand = CreateVisualObject("Hand", PrimitiveType.Sphere, whiteMat);
                hand.transform.SetParent(limb.transform, false);
                hand.transform.localScale = new Vector3(0.1f, 0.1f, 0.001f); // Flattened on Z
                hand.transform.localPosition = new Vector3(0, 0, 0.22f);
            }
            else
            {
                // Cartoon Black/Grey Leg and Foot (from reference image)
                var blackMat = CreateUnlitMat(new Color(0.12f, 0.12f, 0.12f));
                var brownMat = CreateUnlitMat(new Color(0.36f, 0.24f, 0.15f));
                
                // Leg (Cylinder)
                var leg = CreateVisualObject("Leg", PrimitiveType.Cylinder, blackMat);
                leg.transform.SetParent(limb.transform, false);
                leg.transform.localScale = new Vector3(0.03f, 0.15f, 0.001f); // Flattened on Z
                leg.transform.localPosition = new Vector3(0, 0, 0.07f);
                leg.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                
                // Shoe/Foot (Cube)
                var shoe = CreateVisualObject("Shoe", PrimitiveType.Cube, brownMat);
                shoe.transform.SetParent(limb.transform, false);
                shoe.transform.localScale = new Vector3(0.06f, 0.04f, 0.001f); // Flattened on Z
                shoe.transform.localPosition = new Vector3(0, 0, 0.22f);
            }

            // Extend and retract animation
            var animator = limb.AddComponent<LimbAnimator>();
            animator.Setup(dir, Random.Range(0.3f, 0.5f), Random.Range(0.12f, 0.22f));
        }

        private void SpawnFightStar()
        {
            Color[] colors = { 
                new Color(0.2f, 0.9f, 0.8f), // Teal/Cyan (like reference image)
                new Color(0.35f, 1f, 0.55f),  // Mint Green
                Color.yellow, 
                Color.white 
            };
            var mat = CreateUnlitMat(colors[Random.Range(0, colors.Length)]);

            // Create a compound 4-pointed star (+)
            var starContainer = new GameObject("Star");
            starContainer.transform.SetParent(transform, false);
            starContainer.transform.localPosition = Random.insideUnitSphere * 0.15f;

            // Horizontal bar
            var barH = CreateVisualObject("BarH", PrimitiveType.Cube, mat);
            barH.transform.SetParent(starContainer.transform, false);
            barH.transform.localScale = new Vector3(0.12f, 0.025f, 0.001f); // Flattened on Z

            // Vertical bar
            var barV = CreateVisualObject("BarV", PrimitiveType.Cube, mat);
            barV.transform.SetParent(starContainer.transform, false);
            barV.transform.localScale = new Vector3(0.025f, 0.12f, 0.001f); // Flattened on Z

            // Pop out and fall behavior (restricted to local XY plane)
            var physics = starContainer.AddComponent<StarPhysics>();
            physics.Setup(new Vector3(Random.Range(-1.8f, 1.8f), Random.Range(2.5f, 4.5f), 0f));

            // Add spinning animation
            starContainer.AddComponent<StarSpinner>().Setup(Random.Range(180f, 360f));
        }

        private void FinishFight()
        {
            // Restore scales (null-safe for disconnect during fight)
            if (_playerTrans != null) _playerTrans.localScale = _originalPlayerScale;
            if (_targetTrans != null) _targetTrans.localScale = _originalTargetTransScale;

            // Apply swollen face state and knockback to target
            if (_targetTrans != null && _playerTrans != null)
            {
                var npc = _targetTrans.GetComponent<NPCController>();
                var fp = _targetTrans.GetComponent<FakePlayerController>();

                Vector3 knockDir = (_targetTrans.position - _playerTrans.position).normalized;
                knockDir.y = 0;
                float force = 8f; // Strong knockback after cloud

                if (npc != null)
                {
                    // Directly enable effects and knockback
                    npc.ReceivePunch(knockDir, force);
                }
                else if (fp != null)
                {
                    fp.ReceivePunch(knockDir, force);
                }

                // Trigger comic text at finish
                ComicPunchEffect.SpawnAt(_targetTrans.position);
            }

            Destroy(gameObject);
        }

        // Static material cache to prevent memory leaks — materials created once, reused forever
        private static readonly Dictionary<Color, Material> _materialCache = new Dictionary<Color, Material>();

        private Material CreateUnlitMat(Color color)
        {
            if (_materialCache.TryGetValue(color, out var cached) && cached != null)
                return cached;

            var shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("UI/Default");

            Material mat = null;
            try
            {
                if (shader != null)
                {
                    mat = new Material(shader);
                    mat.color = color;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FightCloudEffect] Failed to create material: {e.Message}");
            }
            _materialCache[color] = mat;
            return mat;
        }

        // Static Mesh cache to avoid dynamic collider and mesh recreation crashes
        private static Mesh _sphereMesh;
        private static Mesh _cubeMesh;
        private static Mesh _cylinderMesh;

        private static void CacheMeshes()
        {
            if (_sphereMesh == null)
            {
                var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _sphereMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                Destroy(temp);
            }
            if (_cubeMesh == null)
            {
                var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _cubeMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                Destroy(temp);
            }
            if (_cylinderMesh == null)
            {
                var temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                _cylinderMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                Destroy(temp);
            }
        }

        private static GameObject CreateVisualObject(string name, PrimitiveType type, Material mat)
        {
            CacheMeshes();
            var go = new GameObject(name);
            var filter = go.AddComponent<MeshFilter>();
            if (type == PrimitiveType.Sphere) filter.sharedMesh = _sphereMesh;
            else if (type == PrimitiveType.Cube) filter.sharedMesh = _cubeMesh;
            else if (type == PrimitiveType.Cylinder) filter.sharedMesh = _cylinderMesh;

            var renderer = go.AddComponent<MeshRenderer>();
            if (mat != null) renderer.material = mat;
            return go;
        }
    }

    /// <summary>
    /// Helper to animate a single limb extending out of the cloud and retracting.
    /// </summary>
    internal class LimbAnimator : MonoBehaviour
    {
        private Vector3 _dir;
        private float _maxDistance;
        private float _duration;
        private float _elapsed = 0f;
        private float _angleDeg;

        public void Setup(Vector3 dir, float maxDistance, float duration)
        {
            _dir = dir;
            _maxDistance = maxDistance;
            _duration = duration;
            _angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;

            if (t >= 1.0f)
            {
                Destroy(gameObject);
                return;
            }

            // Sine wave projection (0 -> max -> 0)
            float dist = Mathf.Sin(t * Mathf.PI) * _maxDistance;
            transform.localPosition = _dir * dist;

            // Rotate purely in 2D around Z axis, pointing along _dir with a frantic shake
            float shakeAngle = Mathf.Sin(t * Mathf.PI * 7f) * 20f;
            transform.localRotation = Quaternion.Euler(0f, 0f, _angleDeg - 90f + shakeAngle);
        }
    }

    /// <summary>
    /// Helper to bounce a star out of the cloud.
    /// </summary>
    internal class StarPhysics : MonoBehaviour
    {
        private Vector3 _velocity;
        private float _gravity = -9.81f;
        private float _life = 0.6f;

        public void Setup(Vector3 force)
        {
            _velocity = force;
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0)
            {
                Destroy(gameObject);
                return;
            }

            _velocity.y += _gravity * Time.deltaTime;
            // Move strictly in local XY plane
            transform.localPosition += new Vector3(_velocity.x * Time.deltaTime, _velocity.y * Time.deltaTime, 0f);
        }
    }

    /// <summary>
    /// Helper to spin the 4-pointed stars.
    /// </summary>
    internal class StarSpinner : MonoBehaviour
    {
        private float _spinSpeed;
        public void Setup(float speed) { _spinSpeed = speed; }
        private void Update() { transform.Rotate(Vector3.forward, _spinSpeed * Time.deltaTime); }
    }
}
