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
            _animator = GetComponent<Animator>();

            _gardenPortal = GameObject.Find("GardenPortal");
            _prisonPortal = GameObject.Find("PrisonPortal");
            _fishingPortal = GameObject.Find("FishingPortal");
            _studyPortal = GameObject.Find("StudyPortal");
            _gardenRet = GameObject.Find("GardenReturnPortal");
            _prisonRet = GameObject.Find("PrisonReturnPortal");
            _fishingRet = GameObject.Find("FishingReturnPortal");
            _studyRet = GameObject.Find("StudyReturnPortal");
        }

        private void Update()
        {
            if (_teleportCooldownTimer > 0f) _teleportCooldownTimer -= Time.deltaTime;

            if (_isJailed)
            {
                _jailTimer -= Time.deltaTime;
                if (_jailTimer <= 0f) ReleaseFromJail();
                return;
            }

            if (_isPunching) return;

            HandleKeyboardMovement();
            HandleClickMovement();
            HandlePunch();
            HandleInteractionKey();
            CheckPortals();
            DetectNearbyInteractables();
        }

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
                _isClickMoving = false;
                _lastMoveDir = dir;

                Vector3 targetPosX = ClampToWorld(transform.position + new Vector3(dir.x, 0, 0) * _moveSpeed * Time.deltaTime);
                if (IsValidPosition(targetPosX)) transform.position = targetPosX;

                Vector3 targetPosZ = ClampToWorld(transform.position + new Vector3(0, 0, dir.z) * _moveSpeed * Time.deltaTime);
                if (IsValidPosition(targetPosZ)) transform.position = targetPosZ;

                if (_animator) _animator.SetFloat(AnimSpeed, 1f);
            }
            else if (!_isClickMoving)
            {
                if (_animator) _animator.SetFloat(AnimSpeed, 0f);
            }
        }

        private void HandleClickMovement()
        {
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
                    bool moved = false;

                    Vector3 targetPosX = ClampToWorld(transform.position + new Vector3(moveDir.x, 0, 0) * _moveSpeed * Time.deltaTime);
                    if (IsValidPosition(targetPosX)) { transform.position = targetPosX; moved = true; }

                    Vector3 targetPosZ = ClampToWorld(transform.position + new Vector3(0, 0, moveDir.z) * _moveSpeed * Time.deltaTime);
                    if (IsValidPosition(targetPosZ)) { transform.position = targetPosZ; moved = true; }

                    _lastMoveDir = moveDir;
                    if (_animator) _animator.SetFloat(AnimSpeed, 1f);

                    if (!moved)
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

        private void HandlePunch()
        {
            _punchCooldownTimer -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space) && _punchCooldownTimer <= 0f) ExecutePunch();
        }

        public void ExecutePunch()
        {
            if (_isPunching || _punchCooldownTimer > 0f) return;

            _punchCooldownTimer = _punchCooldown;
            _isPunching = true;
            _isClickMoving = false;
            if (_animator) _animator.SetTrigger(AnimPunch);

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
                FightCloudEffect.Create(transform, closestTarget.transform, 1.5f);
                OnPunchHit?.Invoke();
                Invoke(nameof(GoToJail), 1.6f);
            }
            Invoke(nameof(EndPunch), 0.35f);
        }

        private void GoToJail()
        {
            float jailDuration = 15f;
            transform.position = new Vector3(2f, 0.05f, -62f);
            _isJailed = true;
            _jailTimer = jailDuration;
            _isClickMoving = false;
            OnJailStart?.Invoke(jailDuration);
        }

        private void EndPunch() => _isPunching = false;

        private void ReleaseFromJail()
        {
            _isJailed = false;
            var lobbyPortal = _prisonPortal ?? GameObject.Find("PrisonPortal");
            Vector3 dest = lobbyPortal != null ? lobbyPortal.transform.position + new Vector3(0, 0, 1.2f) : new Vector3(0, 0.05f, -9.5f);
            dest.y = 0.05f;
            transform.position = dest;
            _teleportCooldownTimer = 1.0f;
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
                float interactDist = 2.0f;
                var plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
                GardenPlot closestPlot = null;
                float minPlotDist = interactDist;
                foreach (var plot in plots)
                {
                    float dist = Vector3.Distance(transform.position, plot.transform.position);
                    if (dist < minPlotDist) { minPlotDist = dist; closestPlot = plot; }
                }
                if (closestPlot != null) { closestPlot.TryInteract(this); return; }

                var clouds = FindObjectsByType<CloudLayer>(FindObjectsSortMode.None);
                CloudLayer closestCloud = null;
                float minCloudDist = interactDist;
                foreach (var cloud in clouds)
                {
                    float dist = Vector3.Distance(transform.position, cloud.transform.position);
                    if (dist < minCloudDist) { minCloudDist = dist; closestCloud = cloud; }
                }
                if (closestCloud != null) { closestCloud.TryInteract(this); return; }
            }
        }

        private void CheckPortals()
        {
            if (_teleportCooldownTimer > 0f) return;

            float portalRadius = 1.0f;
            Vector3 currentPos = transform.position;

            var gP = _gardenPortal ?? GameObject.Find("GardenPortal");
            var pP = _prisonPortal ?? GameObject.Find("PrisonPortal");
            var fP = _fishingPortal ?? GameObject.Find("FishingPortal");
            var sP = _studyPortal ?? GameObject.Find("StudyPortal");

            var gR = _gardenRet ?? GameObject.Find("GardenReturnPortal");
            var pR = _prisonRet ?? GameObject.Find("PrisonReturnPortal");
            var fR = _fishingRet ?? GameObject.Find("FishingReturnPortal");
            var sR = _studyRet ?? GameObject.Find("StudyReturnPortal");

            if (gP != null && Vector3.Distance(currentPos, gP.transform.position) < portalRadius) Teleport(new Vector3(0, 0.05f, 56f));
            else if (pP != null && Vector3.Distance(currentPos, pP.transform.position) < portalRadius) Teleport(new Vector3(0, 0.05f, -56f));
            else if (fP != null && Vector3.Distance(currentPos, fP.transform.position) < portalRadius) Teleport(new Vector3(56f, 0.05f, 0));
            else if (sP != null && Vector3.Distance(currentPos, sP.transform.position) < portalRadius) Teleport(new Vector3(-56f, 0.05f, 0));
            else if (gR != null && Vector3.Distance(currentPos, gR.transform.position) < portalRadius) Teleport(gP != null ? gP.transform.position + new Vector3(0, 0, -1.2f) : new Vector3(0, 0.05f, 9.5f));
            else if (pR != null && Vector3.Distance(currentPos, pR.transform.position) < portalRadius) Teleport(pP != null ? pP.transform.position + new Vector3(0, 0, 1.2f) : new Vector3(0, 0.05f, -9.5f));
            else if (fR != null && Vector3.Distance(currentPos, fR.transform.position) < portalRadius) Teleport(fP != null ? fP.transform.position + new Vector3(-1.2f, 0, 0) : new Vector3(9.5f, 0.05f, 0));
            else if (sR != null && Vector3.Distance(currentPos, sR.transform.position) < portalRadius) Teleport(sP != null ? sP.transform.position + new Vector3(1.2f, 0, 0) : new Vector3(-9.5f, 0.05f, 0));
        }

        private void Teleport(Vector3 destination)
        {
            transform.position = destination;
            _isClickMoving = false;
            _teleportCooldownTimer = 1.0f;
            
            var flash = new GameObject("TeleportFlash");
            flash.transform.position = destination + Vector3.up * 0.5f;
            var light = flash.AddComponent<Light>();
            light.color = Color.cyan;
            light.range = 8f;
            light.intensity = 3f;
            Destroy(flash, 0.25f);
        }

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
                if (dist < closestDist) { closestDist = dist; closest = interactable; }
            }

            if (closest != _nearestInteractable)
            {
                if (_nearestInteractable != null) OnLeaveInteractable?.Invoke();
                _nearestInteractable = closest;
                if (closest != null) OnNearInteractable?.Invoke(closest);
            }
        }

        public IInteractable GetNearestInteractable() => _nearestInteractable;

        private bool IsValidPosition(Vector3 pos)
        {
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
            float z = pos.z;
            float x = pos.x;

            if (z > 40f) { pos.x = Mathf.Clamp(x, -8f, 8f); pos.z = Mathf.Clamp(z, 52f, 68f); }
            else if (z < -40f) { pos.x = Mathf.Clamp(x, -8f, 8f); pos.z = Mathf.Clamp(z, -68f, -52f); }
            else if (x > 40f) { pos.x = Mathf.Clamp(x, 52f, 68f); pos.z = Mathf.Clamp(z, -8f, 8f); }
            else if (x < -40f) { pos.x = Mathf.Clamp(x, -68f, -52f); pos.z = Mathf.Clamp(z, -8f, 8f); }
            else { pos.x = Mathf.Clamp(x, _worldMinX, _worldMaxX); pos.z = Mathf.Clamp(z, _worldMinZ, _worldMaxZ); }

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
