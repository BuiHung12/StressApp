using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Điều khiển nhân vật chính — 2D top-down, không cần NavMesh.
    /// Di chuyển bằng WASD/Arrow keys hoặc click chuột.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 4f;

        [Header("Punch")]
        [SerializeField] private float _punchRange = 2.2f;
        [SerializeField] private float _punchCooldown = 0.5f;
        [SerializeField] private float _punchKnockbackForce = 6f;

        [Header("Interaction")]
        [SerializeField] private float _interactionRange = 2.5f;

        [Header("World Bounds")]
        [SerializeField] private float _worldMinX = -14f;
        [SerializeField] private float _worldMaxX = 14f;
        [SerializeField] private float _worldMinZ = -14f;
        [SerializeField] private float _worldMaxZ = 14f;

        // State
        private Camera _mainCamera;
        private Vector3 _moveTarget;
        private bool _isClickMoving;
        private float _punchCooldownTimer;
        private bool _isPunching;
        private IInteractable _nearestInteractable;
        private Vector3 _lastMoveDir = Vector3.forward;

        // Animator
        private Animator _animator;
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimPunch = Animator.StringToHash("Punch");

        // Events
        public System.Action<IInteractable> OnNearInteractable;
        public System.Action OnLeaveInteractable;
        public System.Action OnPunchHit;

        public float InteractionRange => _interactionRange;
        public bool IsPunching => _isPunching;

        private void Start()
        {
            _mainCamera = Camera.main;
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_isPunching) return;

            HandleKeyboardMovement();
            HandleClickMovement();
            HandlePunch();
            DetectNearbyInteractables();
        }

        // ── Keyboard Movement (WASD / Arrows) ──

        private void HandleKeyboardMovement()
        {
            float h = 0f, v = 0f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

            Vector3 dir = new Vector3(h, 0f, v).normalized;

            if (dir.sqrMagnitude > 0.01f)
            {
                _isClickMoving = false; // Cancel click-to-move
                _lastMoveDir = dir;

                Vector3 newPos = transform.position + dir * _moveSpeed * Time.deltaTime;
                newPos = ClampToWorld(newPos);
                transform.position = newPos;

                if (_animator) _animator.SetFloat(AnimSpeed, 1f);
            }
            else if (!_isClickMoving)
            {
                if (_animator) _animator.SetFloat(AnimSpeed, 0f);
            }
        }

        // ── Click-to-Move ──

        private void HandleClickMovement()
        {
            if (Input.GetMouseButtonDown(0) && _mainCamera != null)
            {
                // Ignore if over UI
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

                if (groundPlane.Raycast(ray, out float dist))
                {
                    _moveTarget = ray.GetPoint(dist);
                    _moveTarget.y = 0f;
                    _isClickMoving = true;
                }
            }

            if (_isClickMoving)
            {
                Vector3 dir = (_moveTarget - transform.position);
                dir.y = 0f;

                if (dir.magnitude > 0.15f)
                {
                    Vector3 move = dir.normalized * _moveSpeed * Time.deltaTime;
                    Vector3 newPos = ClampToWorld(transform.position + move);
                    transform.position = newPos;
                    _lastMoveDir = dir.normalized;

                    if (_animator) _animator.SetFloat(AnimSpeed, 1f);
                }
                else
                {
                    _isClickMoving = false;
                    if (_animator) _animator.SetFloat(AnimSpeed, 0f);
                }
            }
        }

        // ── Punch ──

        private void HandlePunch()
        {
            _punchCooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && _punchCooldownTimer <= 0f)
            {
                ExecutePunch();
            }
        }

        public void ExecutePunch()
        {
            if (_isPunching || _punchCooldownTimer > 0f) return;

            _isPunching = true;
            _punchCooldownTimer = _punchCooldown;
            _isClickMoving = false;

            if (_animator) _animator.SetTrigger(AnimPunch);

            // Find nearest target in range
            MonoBehaviour closestTarget = null;
            float closestDist = _punchRange;

            var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.gameObject == gameObject) continue;
                var punchable = obj as IPunchable;
                if (punchable == null) continue;

                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = obj;
                }
            }

            if (closestTarget != null)
            {
                // Trigger Cartoon Fight Cloud!
                FightCloudEffect.Create(transform, closestTarget.transform, 1.5f);
                OnPunchHit?.Invoke();
            }

            Invoke(nameof(EndPunch), 0.35f);
        }

        private void EndPunch() => _isPunching = false;

        // ── Interaction Detection ──

        private void DetectNearbyInteractables()
        {
            IInteractable closest = null;
            float closestDist = _interactionRange;

            var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.gameObject == gameObject) continue;
                var interactable = obj as IInteractable;
                if (interactable == null) continue;

                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }

            if (closest != _nearestInteractable)
            {
                if (_nearestInteractable != null) OnLeaveInteractable?.Invoke();
                _nearestInteractable = closest;
                if (closest != null)
                {
                    Debug.Log($"[Player] Near: {closest.DisplayName} dist={closestDist:F1} range={_interactionRange}");
                    OnNearInteractable?.Invoke(closest);
                }
            }
        }

        public IInteractable GetNearestInteractable() => _nearestInteractable;

        private Vector3 ClampToWorld(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, _worldMinX, _worldMaxX);
            pos.z = Mathf.Clamp(pos.z, _worldMinZ, _worldMaxZ);
            pos.y = 0f;
            return pos;
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
