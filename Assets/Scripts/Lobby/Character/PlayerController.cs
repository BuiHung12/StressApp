using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Điều khiển nhân vật chính — 2D top-down, không cần NavMesh.
    /// Di chuyển bằng WASD/Arrow keys hoặc click chuột.
    /// </summary>
    public partial class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 4f;

        [Header("Punch")]
        [SerializeField] private float _punchRange = 2.2f;
        [SerializeField] private float _punchCooldown = 0.5f;

        [Header("Interaction")]
        [SerializeField] private float _interactionRange = 2.5f;

        [Header("World Bounds")]
        [SerializeField] private float _worldMinX = -14f;
        [SerializeField] private float _worldMaxX = 14f;
        [SerializeField] private float _worldMinZ = -14f;
        [SerializeField] private float _worldMaxZ = 14f;

        private Camera _mainCamera;
        private Vector3 _moveTarget;
        private bool _isClickMoving;
        private bool _wasKeyboardMoving;
        private float _punchCooldownTimer;
        private bool _isPunching;
        private IInteractable _nearestInteractable;
        private Vector3 _lastMoveDir = Vector3.forward;

        private Animator _animator;
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimPunch = Animator.StringToHash("Punch");

        // Events
        public System.Action<IInteractable> OnNearInteractable;
        public System.Action OnLeaveInteractable;
        public System.Action OnPunchHit;
        public System.Action<float> OnJailStart;
        public System.Action OnJailEnd;
        public System.Action<int> OnCoinsChanged;

        private bool _isJailed;
        private float _jailTimer;
        private int _rangerCoins = 50;
        private float _teleportCooldownTimer;
        private float _saveTimer = 0f;
        private bool _isStunned;
        private Vector3 _knockbackVelocity;

        public float InteractionRange => _interactionRange;
        public bool IsPunching => _isPunching;
        public bool IsJailed => _isJailed;
        public int RangerCoins => _rangerCoins;

        // Cached portals
        private GameObject _gardenPortal, _prisonPortal, _fishingPortal, _studyPortal;
        private GameObject _gardenRet, _prisonRet, _fishingRet, _studyRet;

        private void Start()
        {
            _mainCamera = Camera.main;
            _animator = GetComponentInChildren<Animator>();

            _gardenPortal = GameObject.Find("GardenPortal");
            _prisonPortal = GameObject.Find("PrisonPortal");
            _fishingPortal = GameObject.Find("FishingPortal");
            _studyPortal = GameObject.Find("StudyPortal");
            _gardenRet = GameObject.Find("GardenReturnPortal");
            _prisonRet = GameObject.Find("PrisonReturnPortal");
            _fishingRet = GameObject.Find("FishingReturnPortal");
            _studyRet = GameObject.Find("StudyReturnPortal");

            // Load saved player position if exists
            if (PlayerPrefs.HasKey("PlayerPosX"))
            {
                float x = PlayerPrefs.GetFloat("PlayerPosX");
                float y = PlayerPrefs.GetFloat("PlayerPosY");
                float z = PlayerPrefs.GetFloat("PlayerPosZ");
                transform.position = new Vector3(x, y, z);
            }
        }

        private void SavePositionToPrefs()
        {
            PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
            PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);
            PlayerPrefs.SetFloat("PlayerPosZ", transform.position.z);
            PlayerPrefs.Save();
        }

        private void Update()
        {
            if (_teleportCooldownTimer > 0f) _teleportCooldownTimer -= Time.deltaTime;
            if (_punchCooldownTimer > 0f) _punchCooldownTimer -= Time.deltaTime;

            if (_isJailed)
            {
                _jailTimer -= Time.deltaTime;
                if (_jailTimer <= 0f) ReleaseFromJail();
                return;
            }

            if (_isStunned)
            {
                if (_knockbackVelocity.sqrMagnitude > 0.1f)
                {
                    Vector3 nextPos = transform.position + _knockbackVelocity * Time.deltaTime;
                    nextPos = ClampToWorld(nextPos);
                    nextPos = ResolveCollisions(nextPos);
                    transform.position = nextPos;
                    _knockbackVelocity *= 0.85f;
                }
                return;
            }

            if (_isPunching) return;

            HandleKeyboardMovement();
            HandleClickMovement();
            HandlePunch();
            HandleInteractionKey();
            CheckPortals();
            DetectNearbyInteractables();

            // Save player position periodically during movement
            if (_isClickMoving || _wasKeyboardMoving)
            {
                _saveTimer += Time.deltaTime;
                if (_saveTimer >= 1.0f)
                {
                    _saveTimer = 0f;
                    SavePositionToPrefs();
                }
            }
        }

        private void HandleKeyboardMovement()
        {
            float h = 0f, v = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

            // === Mobile joystick input ===
            if (VirtualJoystick.Instance != null)
            {
                Vector2 joyDir = VirtualJoystick.Instance.Direction;
                if (joyDir.sqrMagnitude > 0.01f)
                {
                    h = joyDir.x;
                    v = joyDir.y;
                }
            }

            Vector3 dir = new Vector3(h, 0f, v).normalized;
            if (dir.sqrMagnitude > 0.01f)
            {
                _wasKeyboardMoving = true;
                _isClickMoving = false;
                _lastMoveDir = dir;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 12f);

                Vector3 targetPos = transform.position + dir * _moveSpeed * Time.deltaTime;
                targetPos = ResolveCollisions(targetPos);
                transform.position = ClampToWorld(targetPos);

                if (_animator) _animator.SetFloat(AnimSpeed, 1f);
            }
            else if (!_isClickMoving)
            {
                _wasKeyboardMoving = false;
                if (_animator) _animator.SetFloat(AnimSpeed, 0f);
            }
            else
            {
                _wasKeyboardMoving = false;
            }
        }

        private void HandleClickMovement()
        {
            // On mobile platforms, only use virtual joystick — disable click/tap-to-move
            if (Application.isMobilePlatform) return;

            if (Input.GetMouseButtonDown(0) && _mainCamera != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out float dist))
                {
                    _moveTarget = ray.GetPoint(dist);
                    _moveTarget.y = 0.03f;
                    _isClickMoving = true;
                    Debug.Log($"[PlayerController] Click to move started. Target destination: {_moveTarget}");
                }
            }

            if (_isClickMoving)
            {
                Vector3 dir = (_moveTarget - transform.position);
                dir.y = 0f;
                float dist = dir.magnitude;

                if (dist > 0.15f)
                {
                    float step = _moveSpeed * Time.deltaTime;
                    if (dist <= step)
                    {
                        Vector3 finalPos = ClampToWorld(_moveTarget);
                        finalPos = ResolveCollisions(finalPos);
                        transform.position = finalPos;
                        Debug.Log($"[PlayerController] Click to move complete. Snapped to destination: {finalPos}");
                        _isClickMoving = false;
                        if (_animator) _animator.SetFloat(AnimSpeed, 0f);
                    }
                    else
                    {
                        Vector3 moveDir = dir.normalized;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 12f);
                        
                        Vector3 prevPos = transform.position;
                        Vector3 targetPos = transform.position + moveDir * step;
                        targetPos = ResolveCollisions(targetPos);
                        transform.position = ClampToWorld(targetPos);

                        _lastMoveDir = moveDir;
                        if (_animator) _animator.SetFloat(AnimSpeed, 1f);

                        // Stuck detection (moved less than 10% of step)
                        if (Vector3.Distance(prevPos, transform.position) < step * 0.1f)
                        {
                            Debug.Log($"[PlayerController] Click to move stopped. Character is blocked at: {transform.position}");
                            _isClickMoving = false;
                            if (_animator) _animator.SetFloat(AnimSpeed, 0f);
                        }
                    }
                }
                else
                {
                    Debug.Log($"[PlayerController] Click to move complete. Reached target range: {transform.position}");
                    _isClickMoving = false;
                    if (_animator) _animator.SetFloat(AnimSpeed, 0f);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _punchRange);
        }
    }
}
