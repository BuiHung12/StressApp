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

        private float _limbSpawnTimer = 0f;
        private float _starSpawnTimer = 0f;

        /// <summary>
        /// Kích hoạt trận đấm nhau dạng đám mây bụi giữa Player và Target.
        /// </summary>
        public static void Create(Transform player, Transform target, float duration = 1.5f)
        {
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
            _midPoint.y = 0.5f; // Raise slightly above ground
            transform.position = _midPoint;

            // Store and hide characters
            _originalPlayerScale = player.localScale;
            _originalTargetTransScale = target.localScale;

            player.localScale = Vector3.zero;
            target.localScale = Vector3.zero;

            // Create dust cloud puffs
            int puffCount = 6;
            _puffs = new Transform[puffCount];
            _puffSpeeds = new float[puffCount];
            _puffOffsets = new float[puffCount];
            _puffBaseScales = new Vector3[puffCount];

            Color cloudColor = new Color(0.9f, 0.9f, 0.9f, 0.95f); // Light grey dust

            for (int i = 0; i < puffCount; i++)
            {
                var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                puff.name = $"Puff_{i}";
                puff.transform.SetParent(transform, false);

                // Arrange puffs in a small bundle
                float angle = i * (360f / puffCount) * Mathf.Deg2Rad;
                float dist = Random.Range(0.2f, 0.5f);
                puff.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, Random.Range(-0.2f, 0.2f), Mathf.Sin(angle) * dist);

                float sizeX = Random.Range(1.2f, 1.8f);
                float sizeY = Random.Range(1.0f, 1.5f);
                puff.transform.localScale = new Vector3(sizeX, sizeY, sizeX);

                // Store info
                _puffs[i] = puff.transform;
                _puffBaseScales[i] = puff.transform.localScale;
                _puffSpeeds[i] = Random.Range(5f, 10f);
                _puffOffsets[i] = Random.Range(0f, 2f * Mathf.PI);

                // Unlit material for cartoon style
                puff.GetComponent<Renderer>().material = CreateUnlitMat(cloudColor);
                Destroy(puff.GetComponent<Collider>());
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

            // 1. Animate dust puffs (make them bubble and boil)
            for (int i = 0; i < _puffs.Length; i++)
            {
                if (_puffs[i] == null) continue;
                float sin = Mathf.Sin(Time.time * _puffSpeeds[i] + _puffOffsets[i]);
                float scaleFactor = 1f + sin * 0.15f;
                _puffs[i].localScale = _puffBaseScales[i] * scaleFactor;

                // Slowly rotate
                _puffs[i].Rotate(Vector3.up, 30f * Time.deltaTime);
            }

            // Slowly slide the whole cloud slightly left/right/up to feel alive
            transform.position = _midPoint + new Vector3(
                Mathf.Sin(Time.time * 15f) * 0.05f,
                Mathf.Cos(Time.time * 12f) * 0.03f,
                Mathf.Sin(Time.time * 9f) * 0.05f
            );

            // 2. Spawn limbs (fists/feet popping out)
            _limbSpawnTimer += Time.deltaTime;
            if (_limbSpawnTimer > 0.12f)
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

            // Random direction out of cloud
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Random.Range(-0.3f, 0.4f), Mathf.Sin(angle)).normalized;

            // Random limb type: Fist (sphere) or Foot (cube)
            bool isFist = Random.value > 0.5f;
            GameObject visual;

            if (isFist)
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Fist";
                visual.transform.localScale = Vector3.one * 0.4f;
                visual.GetComponent<Renderer>().material = CreateUnlitMat(new Color(1f, 0.85f, 0.7f)); // Skin color
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = "Foot";
                visual.transform.localScale = new Vector3(0.3f, 0.2f, 0.4f);
                visual.GetComponent<Renderer>().material = CreateUnlitMat(new Color(0.25f, 0.35f, 0.6f)); // Shoe/pant color
            }

            visual.transform.SetParent(limb.transform, false);
            Destroy(visual.GetComponent<Collider>());

            // Extend and retract animation
            var animator = limb.AddComponent<LimbAnimator>();
            animator.Setup(dir, Random.Range(0.6f, 1.2f), Random.Range(0.15f, 0.3f));
        }

        private void SpawnFightStar()
        {
            var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "Star";
            star.transform.SetParent(transform, false);
            star.transform.localPosition = Random.insideUnitSphere * 0.4f;
            star.transform.localScale = Vector3.one * 0.15f;

            Color[] colors = { Color.yellow, Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f) };
            star.GetComponent<Renderer>().material = CreateUnlitMat(colors[Random.Range(0, colors.Length)]);
            Destroy(star.GetComponent<Collider>());

            // Pop out and fall behavior
            var physics = star.AddComponent<StarPhysics>();
            physics.Setup(new Vector3(Random.Range(-3f, 3f), Random.Range(4f, 8f), Random.Range(-3f, 3f)));
        }

        private void FinishFight()
        {
            // Restore scales
            if (_playerTrans != null) _playerTrans.localScale = _originalPlayerScale;
            if (_targetTrans != null) _targetTrans.localScale = _originalTargetTransScale;

            // Apply swollen face state and knockback to target
            if (_targetTrans != null)
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

        private Material CreateUnlitMat(Color color)
        {
            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            mat.color = color;
            return mat;
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

        public void Setup(Vector3 dir, float maxDistance, float duration)
        {
            _dir = dir;
            _maxDistance = maxDistance;
            _duration = duration;
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
            transform.localRotation = Quaternion.LookRotation(_dir);
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
            transform.localPosition += _velocity * Time.deltaTime;
        }
    }
}
