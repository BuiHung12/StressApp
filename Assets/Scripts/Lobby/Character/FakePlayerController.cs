using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Giả lập player khác trong sảnh — simple transform movement.
    /// </summary>
    public class FakePlayerController : MonoBehaviour, IInteractable, IPunchable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Player";
        [SerializeField] private string _avatarEmoji = "🧒";

        [Header("Greetings")]
        [SerializeField, TextArea(1, 3)]
        private string[] _greetings = new string[]
        {
            "Chào bạn! 👋",
            "Bạn mới vào à? 😊",
        };

        [Header("Behavior")]
        [SerializeField] private FakePlayerBehavior _behavior = FakePlayerBehavior.Wander;
        [SerializeField] private float _wanderRadius = 6f;
        [SerializeField] private float _moveSpeed = 1.8f;
        [SerializeField] private float _pauseMin = 2f;
        [SerializeField] private float _pauseMax = 5f;

        [Header("Punch Reaction")]
        [SerializeField] private float _hurtDuration = 1.5f;
        [SerializeField] private GameObject _swollenFaceEffect;
        [SerializeField] private GameObject _punchStarsEffect;

        // State
        private Vector3 _homePosition;
        private Vector3 _moveTarget;
        private float _moveTimer;
        private bool _isMovingAI;
        private bool _isHurt;
        private float _hurtTimer;
        private Vector3 _knockbackVelocity;

        private float _saveTimer = 0f;
        // IInteractable
        public string DisplayName => _displayName;
        public string AvatarEmoji => _avatarEmoji;
        public bool CanTalk => true;
        public bool CanBePunched => true;
        public InteractableType Type => InteractableType.Player;

        private void Start()
        {
            _homePosition = transform.position;
            _moveTimer = Random.Range(_pauseMin, _pauseMax);

            if (PlayerPrefs.HasKey($"FakePlayer_{_displayName}_X"))
            {
                float x = PlayerPrefs.GetFloat($"FakePlayer_{_displayName}_X");
                float y = PlayerPrefs.GetFloat($"FakePlayer_{_displayName}_Y");
                float z = PlayerPrefs.GetFloat($"FakePlayer_{_displayName}_Z");
                Vector3 loadedPos = new Vector3(x, y, z);
                if (IsValidPosition(loadedPos))
                {
                    transform.position = loadedPos;
                    _homePosition = transform.position;
                }
                else
                {
                    Debug.LogWarning($"[FakePlayerController] Saved position {loadedPos} for {_displayName} is inside an obstacle. Restoring default safe position {_homePosition}");
                }
            }

            _hurtDuration = 5f;
            EntityRegistry.RegisterFakePlayer(this);
        }

        private void SavePositionToPrefs()
        {
            if (Mirror.NetworkServer.active || !Mirror.NetworkClient.active)
            {
                PlayerPrefs.SetFloat($"FakePlayer_{_displayName}_X", transform.position.x);
                PlayerPrefs.SetFloat($"FakePlayer_{_displayName}_Y", transform.position.y);
                PlayerPrefs.SetFloat($"FakePlayer_{_displayName}_Z", transform.position.z);
                PlayerPrefs.Save();
            }
        }

        private void OnDestroy()
        {
            EntityRegistry.UnregisterFakePlayer(this);
        }

        // Sync variables for client interpolation
        private Vector3 _syncPosition;
        private Quaternion _syncRotation;
        private bool _hasSyncData;
        private Vector3 _smoothVelocity;

        /// <summary>
        /// Nhận dữ liệu vị trí/xoay từ server qua RPC.
        /// Client sẽ interpolate thay vì chạy AI riêng.
        /// </summary>
        public void SetSyncData(Vector3 position, float rotationY)
        {
            _syncPosition = position;
            _syncRotation = Quaternion.Euler(0, rotationY, 0);
            _hasSyncData = true;
        }

        private void Update()
        {
            // Server (or offline) runs AI, clients interpolate
            if (Mirror.NetworkServer.active || !Mirror.NetworkClient.active)
            {
                if (_isHurt)
                {
                    UpdateHurt();
                    return;
                }

                if (_behavior == FakePlayerBehavior.Wander)
                    UpdateWander();
                // Idle = do nothing

                // Save FakePlayer position periodically during wandering on server
                if (_isMovingAI)
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
                // Client: interpolate synced position/rotation
                if (_hasSyncData)
                {
                    float dist = Vector3.Distance(transform.position, _syncPosition);
                    if (dist > 4f)
                    {
                        transform.position = _syncPosition;
                        _smoothVelocity = Vector3.zero;
                    }
                    else
                    {
                        transform.position = Vector3.SmoothDamp(transform.position, _syncPosition, ref _smoothVelocity, 0.1f);
                    }
                    transform.rotation = Quaternion.Slerp(transform.rotation, _syncRotation, Time.deltaTime * 10f);
                }
            }
        }

        // ── Zone definitions (shared with NPCController) ──
        private static readonly Vector4[] _zoneAreas = new Vector4[]
        {
            new Vector4(-13f, -13f, 13f, 13f),   // Lobby
            new Vector4(-13f, 47f, 13f, 73f),     // Garden
            new Vector4(47f, -13f, 73f, 13f),     // Fishing
            new Vector4(-73f, -13f, -47f, 13f),   // Study
        };

        private static readonly Vector3[] _zoneEntryPoints = new Vector3[]
        {
            new Vector3(0, 0.03f, 0),
            new Vector3(0, 0.03f, 56f),
            new Vector3(56f, 0.03f, 0),
            new Vector3(-60f, 0.03f, -12f),
        };

        private void UpdateWander()
        {
            if (_isMovingAI)
            {
                Vector3 dir = (_moveTarget - transform.position);
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
                        // Blocked — pick new destination soon
                        _isMovingAI = false;
                        _moveTimer = Random.Range(0.5f, 1.5f);
                    }
                }
                else
                {
                    _isMovingAI = false;
                    _moveTimer = Random.Range(_pauseMin, _pauseMax);
                }
            }
            else
            {
                _moveTimer -= Time.deltaTime;
                if (_moveTimer <= 0f)
                {
                    PickNextDestination();
                }
            }
        }

        private void PickNextDestination()
        {
            // Pick a random zone, then random point inside it
            int targetZone = Random.Range(0, _zoneAreas.Length);
            int currentZone = GetCurrentZone();

            Vector4 area = _zoneAreas[targetZone];
            Vector3 target = new Vector3(
                Random.Range(area.x + 1f, area.z - 1f),
                0.03f,
                Random.Range(area.y + 1f, area.w - 1f)
            );

            if (targetZone != currentZone)
            {
                // Teleport to the target zone entry point
                transform.position = _zoneEntryPoints[targetZone];
                _homePosition = transform.position;
            }

            _moveTarget = target;
            _isMovingAI = true;
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
            return 0;
        }

        public string[] GetDialogueLines()
        {
            int idx = Random.Range(0, _greetings.Length);
            return new string[] { _greetings[idx] };
        }

        public void ReceivePunch(Vector3 knockDirection, float force)
        {
            if (_isHurt) return;
            _isHurt = true;
            _hurtTimer = _hurtDuration;
            _knockbackVelocity = knockDirection * force;
            _isMovingAI = false;

            if (_swollenFaceEffect != null) _swollenFaceEffect.SetActive(true);
            if (_punchStarsEffect != null) _punchStarsEffect.SetActive(true);
        }

        private void UpdateHurt()
        {
            _hurtTimer -= Time.deltaTime;
            if (_knockbackVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 nextPos = transform.position + _knockbackVelocity * Time.deltaTime;
                if (IsValidPosition(nextPos))
                {
                    transform.position = nextPos;
                }
                else
                {
                    _knockbackVelocity = Vector3.zero;
                }
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
    }

    public enum FakePlayerBehavior
    {
        Wander,
        Idle
    }
}
