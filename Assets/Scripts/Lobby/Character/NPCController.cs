using UnityEngine;
using Mirror;


namespace RangerCity.Lobby
{
    /// <summary>
    /// NPC trong sảnh chờ — đi lại tự do khắp sảnh và các zone.
    /// NPC tù (wanderRadius nhỏ) chỉ quanh quẩn tại chỗ.
    /// NPC tự do chọn điểm đến ngẫu nhiên trên toàn bản đồ, teleport qua portal khi cần.
    /// </summary>
    public class NPCController : MonoBehaviour, IInteractable, IPunchable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "NPC";
        [SerializeField] private string _avatarEmoji = "🧑";
        [SerializeField] private InteractableType _type = InteractableType.NPC;

        [Header("Dialogue")]
        [SerializeField, TextArea(2, 4)]
        private string[] _dialogueLines = new string[]
        {
            "Xin chào! 👋",
            "Hôm nay đẹp trời nhỉ! ☀️",
        };

        [Header("Wandering AI")]
        [SerializeField] private float _wanderRadius = 4f;
        [SerializeField] private float _wanderPauseMin = 2f;
        [SerializeField] private float _wanderPauseMax = 5f;
        [SerializeField] private float _moveSpeed = 1.2f;

        [Header("Punch Reaction")]
        [SerializeField] private float _hurtDuration = 1.5f;
        [SerializeField] private GameObject _swollenFaceEffect;
        [SerializeField] private GameObject _punchStarsEffect;

        // State
        private Vector3 _homePosition;
        private Vector3 _wanderTarget;
        private float _wanderTimer;
        private bool _isWandering;
        private bool _isHurt;
        private float _hurtTimer;
        private Vector3 _knockbackVelocity;
        private bool _isJailedNPC; // NPC bị nhốt (wanderRadius <= 1)
        private float _saveTimer = 0f;

        // Sync variables for client
        private Vector3 _syncPosition;
        private Quaternion _syncRotation;
        private bool _hasSyncData;
        private Vector3 _smoothVelocity;

        // IInteractable
        public string DisplayName => _displayName;
        public string AvatarEmoji => _avatarEmoji;
        public bool CanTalk => true;
        public bool CanBePunched => true;
        public InteractableType Type => _type;

        public bool IsHurt => _isHurt;

        // ── Zone definitions ──
        // Lobby:   X[-14,14],  Z[-14,14]
        // Garden:  X[-14,14],  Z[46,74]    portal at ~(0,0,10) → teleport to (0,0.05,56)
        // Prison:  X[-14,14],  Z[-74,-46]  (NPC không vào đây)
        // Fishing: X[46,74],   Z[-14,14]   portal at ~(10,0,0) → teleport to (56,0.05,0)
        // Study:   X[-74,-46], Z[-14,14]   portal at ~(-10,0,0) → teleport to (-60,0.05,-12)

        private static readonly Vector4[] _zoneAreas = new Vector4[]
        {
            new Vector4(-13f, -13f, 13f, 13f),   // Lobby (minX, minZ, maxX, maxZ)
            new Vector4(-13f, 47f, 13f, 73f),     // Garden
            new Vector4(47f, -13f, 73f, 13f),     // Fishing
            new Vector4(-73f, -13f, -47f, 13f),   // Study
        };

        // Zone teleport entry points (where NPC appears when entering a zone)
        private static readonly Vector3[] _zoneEntryPoints = new Vector3[]
        {
            new Vector3(0, 0.03f, 0),              // Lobby center
            new Vector3(0, 0.03f, 56f),             // Garden entry
            new Vector3(56f, 0.03f, 0),             // Fishing entry
            new Vector3(-60f, 0.03f, -12f),         // Study entry
        };

        private void Start()
        {
            _homePosition = transform.position;
            _wanderTimer = Random.Range(_wanderPauseMin, _wanderPauseMax);
            _isJailedNPC = _wanderRadius <= 1f;

            // Load saved NPC position if exists
            if (PlayerPrefs.HasKey($"NPC_{_displayName}_X"))
            {
                float x = PlayerPrefs.GetFloat($"NPC_{_displayName}_X");
                float y = PlayerPrefs.GetFloat($"NPC_{_displayName}_Y");
                float z = PlayerPrefs.GetFloat($"NPC_{_displayName}_Z");
                transform.position = new Vector3(x, y, z);
                if (!_isJailedNPC)
                {
                    _homePosition = transform.position;
                }
            }

            EntityRegistry.RegisterNPC(this);
        }

        private void SavePositionToPrefs()
        {
            if (NetworkServer.active || !NetworkClient.active)
            {
                PlayerPrefs.SetFloat($"NPC_{_displayName}_X", transform.position.x);
                PlayerPrefs.SetFloat($"NPC_{_displayName}_Y", transform.position.y);
                PlayerPrefs.SetFloat($"NPC_{_displayName}_Z", transform.position.z);
                PlayerPrefs.Save();
            }
        }

        private void OnDestroy()
        {
            EntityRegistry.UnregisterNPC(this);
        }

        private void Update()
        {
            // Simulate NPC AI on server, or locally if offline/disconnected
            if (NetworkServer.active || !NetworkClient.active)
            {
                if (_isHurt)
                {
                    UpdateHurt();
                    return;
                }
                UpdateWander();

                // Save NPC position periodically during wandering on server
                if (_isWandering)
                {
                    _saveTimer += Time.deltaTime;
                    if (_saveTimer >= 1.0f)
                    {
                        _saveTimer = 0f;
                        SavePositionToPrefs();
                    }
                }
            }
            else
            {
                // Clients interpolate position and rotation
                if (_hasSyncData)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, _syncPosition, ref _smoothVelocity, 0.1f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, _syncRotation, Time.deltaTime * 10f);
                }
            }
        }

        public void SetSyncData(Vector3 position, float rotationY, bool isHurt)
        {
            _syncPosition = position;
            _syncRotation = Quaternion.Euler(0, rotationY, 0);
            _hasSyncData = true;

            // Apply hurt effects visually on clients
            if (_isHurt != isHurt)
            {
                _isHurt = isHurt;
                if (_swollenFaceEffect != null) _swollenFaceEffect.SetActive(isHurt);
                if (_punchStarsEffect != null) _punchStarsEffect.SetActive(isHurt);
            }
        }

        // ── Wandering ──

        private void UpdateWander()
        {
            if (_isWandering)
            {
                Vector3 dir = (_wanderTarget - transform.position);
                dir.y = 0;

                if (dir.magnitude > 0.4f)
                {
                    Vector3 moveDir = dir.normalized;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 8f);
                    Vector3 nextPos = transform.position + moveDir * _moveSpeed * Time.deltaTime;

                    if (IsValidPosition(nextPos))
                    {
                        transform.position = nextPos;
                    }
                    else
                    {
                        // Blocked by obstacle — pick new destination
                        _isWandering = false;
                        _wanderTimer = Random.Range(0.5f, 1.5f);
                    }
                }
                else
                {
                    _isWandering = false;
                    _wanderTimer = Random.Range(_wanderPauseMin, _wanderPauseMax);
                }
            }
            else
            {
                _wanderTimer -= Time.deltaTime;
                if (_wanderTimer <= 0f)
                {
                    PickNextDestination();
                }
            }
        }

        private void PickNextDestination()
        {
            if (_isJailedNPC)
            {
                // Jailed NPC: wander within small radius of home position
                Vector2 rnd = Random.insideUnitCircle * _wanderRadius;
                _wanderTarget = _homePosition + new Vector3(rnd.x, 0, rnd.y);
                _wanderTarget.y = 0.03f;
                _isWandering = true;
                return;
            }

            // Free NPC: always wander within the main lobby (zone 0)
            int targetZone = 0; 
            int currentZone = GetCurrentZone();

            Vector4 area = _zoneAreas[targetZone];
            Vector3 target = new Vector3(
                Random.Range(area.x + 1f, area.z - 1f),
                0.03f,
                Random.Range(area.y + 1f, area.w - 1f)
            );

            if (targetZone != currentZone)
            {
                // Teleport back to main lobby
                transform.position = _zoneEntryPoints[targetZone];
                _homePosition = transform.position;
            }

            _wanderTarget = target;
            _isWandering = true;
        }

        private int GetCurrentZone()
        {
            Vector3 pos = transform.position;
            for (int i = 0; i < _zoneAreas.Length; i++)
            {
                Vector4 a = _zoneAreas[i];
                if (pos.x >= a.x && pos.x <= a.z && pos.z >= a.y && pos.z <= a.w)
                    return i;
            }
            return 0; // Default to lobby
        }

        // ── Dialogue ──

        public string[] GetDialogueLines() => _dialogueLines;

        // ── Punch Reaction ──

        public void ReceivePunch(Vector3 knockDirection, float force)
        {
            if (_isHurt) return;

            _isHurt = true;
            _hurtTimer = _hurtDuration;
            _knockbackVelocity = knockDirection * force;
            _isWandering = false;

            if (_swollenFaceEffect != null) _swollenFaceEffect.SetActive(true);
            if (_punchStarsEffect != null) _punchStarsEffect.SetActive(true);
        }

        private void UpdateHurt()
        {
            _hurtTimer -= Time.deltaTime;

            // Knockback
            if (_knockbackVelocity.sqrMagnitude > 0.1f)
            {
                transform.position += _knockbackVelocity * Time.deltaTime;
                _knockbackVelocity *= 0.88f;
            }

            if (_hurtTimer <= 0f)
            {
                _isHurt = false;
                if (_swollenFaceEffect != null) _swollenFaceEffect.SetActive(false);
                if (_punchStarsEffect != null) _punchStarsEffect.SetActive(false);
            }
        }

        private bool IsValidPosition(Vector3 pos)
        {
            return CollisionUtils.IsValidPosition(pos, transform.root);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Vector3 center = Application.isPlaying ? _homePosition : transform.position;
            Gizmos.DrawWireSphere(center, _wanderRadius);
        }
    }
}
