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
        public System.Action<float> OnJailStart;  // param = jail duration
        public System.Action OnJailEnd;
        public System.Action<int> OnCoinsChanged; // param = current coin count

        // Jail state
        private bool _isJailed;
        private float _jailTimer;
        private MonoBehaviour _jailVisitorTarget;
        private Vector3 _jailVisitorOrigPos;

        // Ranger Coins Economy
        private int _rangerCoins = 50;

        // Teleport cooldown
        private float _teleportCooldownTimer;

        public float InteractionRange => _interactionRange;
        public bool IsPunching => _isPunching;
        public bool IsJailed => _isJailed;
        public int RangerCoins => _rangerCoins;

        private void Start()
        {
            _mainCamera = Camera.main;
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Teleport cooldown countdown
            if (_teleportCooldownTimer > 0f) _teleportCooldownTimer -= Time.deltaTime;

            // Jail freeze
            if (_isJailed)
            {
                _jailTimer -= Time.deltaTime;
                if (_jailTimer <= 0f)
                {
                    ReleaseFromJail();
                }
                return; // Block all input while jailed
            }

            if (_isPunching) return;

            HandleKeyboardMovement();
            HandleClickMovement();
            HandlePunch();
            HandleInteractionKey();
            CheckPortals();
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

                // Move X
                Vector3 moveX = new Vector3(dir.x, 0, 0) * _moveSpeed * Time.deltaTime;
                Vector3 targetPosX = ClampToWorld(transform.position + moveX);
                if (IsValidPosition(targetPosX))
                {
                    transform.position = targetPosX;
                }

                // Move Z
                Vector3 moveZ = new Vector3(0, 0, dir.z) * _moveSpeed * Time.deltaTime;
                Vector3 targetPosZ = ClampToWorld(transform.position + moveZ);
                if (IsValidPosition(targetPosZ))
                {
                    transform.position = targetPosZ;
                }

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
                    Vector3 moveDir = dir.normalized;
                    
                    // Move X
                    Vector3 moveX = new Vector3(moveDir.x, 0, 0) * _moveSpeed * Time.deltaTime;
                    Vector3 targetPosX = ClampToWorld(transform.position + moveX);
                    bool movedX = false;
                    if (IsValidPosition(targetPosX))
                    {
                        transform.position = targetPosX;
                        movedX = true;
                    }

                    // Move Z
                    Vector3 moveZ = new Vector3(0, 0, moveDir.z) * _moveSpeed * Time.deltaTime;
                    Vector3 targetPosZ = ClampToWorld(transform.position + moveZ);
                    bool movedZ = false;
                    if (IsValidPosition(targetPosZ))
                    {
                        transform.position = targetPosZ;
                        movedZ = true;
                    }

                    _lastMoveDir = moveDir;

                    if (_animator) _animator.SetFloat(AnimSpeed, 1f);

                    // If we got completely blocked, cancel click-movement
                    if (!movedX && !movedZ)
                    {
                        _isClickMoving = false;
                        if (_animator) _animator.SetFloat(AnimSpeed, 0f);
                    }
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

            _punchCooldownTimer = _punchCooldown;
            _isPunching = true;
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
                
                // Store victim info for prison visit
                _jailVisitorTarget = closestTarget;
                _jailVisitorOrigPos = closestTarget.transform.position;

                // Send to jail after fight cloud ends
                Invoke(nameof(GoToJail), 1.6f);
            }

            Invoke(nameof(EndPunch), 0.35f);
        }

        private void GoToJail()
        {
            float jailDuration = 3f;
            // Teleport Attacker (Player) inside the jail cell (Z = -62)
            transform.position = new Vector3(0, 0, -62f);
            _isJailed = true;
            _jailTimer = jailDuration;
            _isClickMoving = false;

            // Teleport Victim (Bot/NPC) to the visiting area (Z = -56) facing South (Z-)
            if (_jailVisitorTarget != null)
            {
                _jailVisitorTarget.transform.position = new Vector3(0, 0, -56f);
                _jailVisitorTarget.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1f));

                // Disable behavior controllers so they stand still
                var fp = _jailVisitorTarget.GetComponent<FakePlayerController>();
                if (fp != null) fp.enabled = false;
                var npc = _jailVisitorTarget.GetComponent<NPCController>();
                if (npc != null) npc.enabled = false;
            }

            OnJailStart?.Invoke(jailDuration);
        }

        private void EndPunch() => _isPunching = false;

        private void ReleaseFromJail()
        {
            _isJailed = false;

            // Teleport Attacker (Player) back to Lobby near Prison Portal
            transform.position = new Vector3(0, 0, -9.5f);

            // Teleport Victim (Bot/NPC) back to their original position and re-enable movement
            if (_jailVisitorTarget != null)
            {
                _jailVisitorTarget.transform.position = _jailVisitorOrigPos;

                var fp = _jailVisitorTarget.GetComponent<FakePlayerController>();
                if (fp != null) fp.enabled = true;
                var npc = _jailVisitorTarget.GetComponent<NPCController>();
                if (npc != null) npc.enabled = true;

                _jailVisitorTarget = null;
            }

            _teleportCooldownTimer = 1.0f; // Prevent re-triggering portal immediately
            OnJailEnd?.Invoke();
        }

        public void AddCoins(int amount)
        {
            _rangerCoins = Mathf.Max(0, _rangerCoins + amount);
            OnCoinsChanged?.Invoke(_rangerCoins);
        }

        private void HandleInteractionKey()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Find nearest GardenPlot or CloudLayer
                float interactDist = 2.0f;
                
                // Check Garden Plots
                var plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
                GardenPlot closestPlot = null;
                float minPlotDist = interactDist;
                foreach (var plot in plots)
                {
                    float dist = Vector3.Distance(transform.position, plot.transform.position);
                    if (dist < minPlotDist)
                    {
                        minPlotDist = dist;
                        closestPlot = plot;
                    }
                }
                if (closestPlot != null)
                {
                    closestPlot.TryInteract(this);
                    return;
                }

                // Check Cloud Layers
                var clouds = FindObjectsByType<CloudLayer>(FindObjectsSortMode.None);
                CloudLayer closestCloud = null;
                float minCloudDist = interactDist;
                foreach (var cloud in clouds)
                {
                    float dist = Vector3.Distance(transform.position, cloud.transform.position);
                    if (dist < minCloudDist)
                    {
                        minCloudDist = dist;
                        closestCloud = cloud;
                    }
                }
                if (closestCloud != null)
                {
                    closestCloud.TryInteract(this);
                    return;
                }
            }
        }

        private void CheckPortals()
        {
            if (_teleportCooldownTimer > 0f) return;

            float portalRadius = 1.0f; // Bán kính nhạy hơn để kích hoạt mượt mà
            Vector3 currentPos = transform.position;

            // Tìm các cổng dịch chuyển tại sảnh
            GameObject gardenPortal = GameObject.Find("GardenPortal");
            GameObject prisonPortal = GameObject.Find("PrisonPortal");
            GameObject fishingPortal = GameObject.Find("FishingPortal");
            GameObject studyPortal = GameObject.Find("StudyPortal");

            // Tìm các cổng quay lại tại các Zone
            GameObject gardenRet = GameObject.Find("GardenReturnPortal");
            GameObject prisonRet = GameObject.Find("PrisonReturnPortal");
            GameObject fishingRet = GameObject.Find("FishingReturnPortal");
            GameObject studyRet = GameObject.Find("StudyReturnPortal");

            // Kiểm tra khoảng cách để dịch chuyển
            if (gardenPortal != null && Vector3.Distance(currentPos, gardenPortal.transform.position) < portalRadius)
            {
                Teleport(new Vector3(0, 0.05f, 54f));
            }
            else if (prisonPortal != null && Vector3.Distance(currentPos, prisonPortal.transform.position) < portalRadius)
            {
                Teleport(new Vector3(0, 0.05f, -54f));
            }
            else if (fishingPortal != null && Vector3.Distance(currentPos, fishingPortal.transform.position) < portalRadius)
            {
                Teleport(new Vector3(54f, 0.05f, 0));
            }
            else if (studyPortal != null && Vector3.Distance(currentPos, studyPortal.transform.position) < portalRadius)
            {
                Teleport(new Vector3(-54f, 0.05f, 0));
            }
            else if (gardenRet != null && Vector3.Distance(currentPos, gardenRet.transform.position) < portalRadius)
            {
                Teleport(new Vector3(0, 0.05f, 9.5f));
            }
            else if (prisonRet != null && Vector3.Distance(currentPos, prisonRet.transform.position) < portalRadius)
            {
                Teleport(new Vector3(0, 0.05f, -9.5f));
            }
            else if (fishingRet != null && Vector3.Distance(currentPos, fishingRet.transform.position) < portalRadius)
            {
                Teleport(new Vector3(9.5f, 0.05f, 0));
            }
            else if (studyRet != null && Vector3.Distance(currentPos, studyRet.transform.position) < portalRadius)
            {
                Teleport(new Vector3(-9.5f, 0.05f, 0));
            }
        }

        private void Teleport(Vector3 destination)
        {
            transform.position = destination;
            _isClickMoving = false;
            _teleportCooldownTimer = 1.0f; // 1 second cooldown
            
            // Trigger teleporter UI flash or visual feedback
            var flash = new GameObject("TeleportFlash");
            flash.transform.position = destination + Vector3.up * 0.5f;
            var light = flash.AddComponent<Light>();
            light.color = Color.cyan;
            light.range = 8f;
            light.intensity = 3f;
            Destroy(flash, 0.25f);
        }


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

        private bool IsValidPosition(Vector3 pos)
        {
            // Characters are capsule/spheres of size 1.2 units (radius 0.6)
            // Use radius 0.5f to allow walking close to walls but not clipping into them
            Collider[] hits = Physics.OverlapSphere(pos + Vector3.up * 0.5f, 0.45f);
            foreach (var hit in hits)
            {
                if (hit.transform.root == transform.root) continue;
                if (hit.isTrigger) continue;

                string name = hit.gameObject.name;
                if (name.Contains("Collider") || name.Contains("Obstacle") || name.Contains("Walls") ||
                    name.Contains("Tree") || name.Contains("Post") || name.Contains("Picket") ||
                    name.Contains("Seat") || name.Contains("Base") || name.Contains("Pillar") ||
                    name.Contains("Bowl") || name.Contains("Bench") || name.Contains("Fountain") ||
                    name.Contains("Fence") || name.Contains("House") || name.Contains("Shop"))
                {
                    return false;
                }
            }
            return true;
        }

        private Vector3 ClampToWorld(Vector3 pos)
        {
            // Dynamic Clamping based on current region
            float z = pos.z;
            float x = pos.x;

            if (z > 40f)
            {
                // Garden Zone (supports cloud heights and stairs)
                pos.x = Mathf.Clamp(x, -8f, 8f);
                pos.z = Mathf.Clamp(z, 52f, 68f);
            }
            else if (z < -40f)
            {
                // Prison Zone
                pos.x = Mathf.Clamp(x, -8f, 8f);
                pos.z = Mathf.Clamp(z, -68f, -52f);
            }
            else if (x > 40f)
            {
                // Fishing Zone
                pos.x = Mathf.Clamp(x, 52f, 68f);
                pos.z = Mathf.Clamp(z, -8f, 8f);
            }
            else if (x < -40f)
            {
                // Study Zone
                pos.x = Mathf.Clamp(x, -68f, -52f);
                pos.z = Mathf.Clamp(z, -8f, 8f);
            }
            else
            {
                // Main Lobby
                pos.x = Mathf.Clamp(x, _worldMinX, _worldMaxX);
                pos.z = Mathf.Clamp(z, _worldMinZ, _worldMaxZ);
            }

            // Keep vertical position flat relative to ground, but allow stair heights
            // We clamp y inside the movement code naturally, or let pos.y stay as is
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
