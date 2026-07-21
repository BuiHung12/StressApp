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

            try
            {
                if (_animator) _animator.SetTrigger(AnimPunch);

                MonoBehaviour closestTarget = null;
                float closestDist = _punchRange;

                // Find closest NPC (O(n) on registry, not full scene scan)
                var npcs = EntityRegistry.AllNPCs;
                foreach (var npc in npcs)
                {
                    float dist = Vector3.Distance(transform.position, npc.transform.position);
                    if (dist < closestDist) { closestDist = dist; closestTarget = npc; }
                }

                // Find closest Fake Player
                var fakePlayers = EntityRegistry.AllFakePlayers;
                foreach (var fp in fakePlayers)
                {
                    float dist = Vector3.Distance(transform.position, fp.transform.position);
                    if (dist < closestDist) { closestDist = dist; closestTarget = fp; }
                }

                // Find closest actual Player (other than self)
                var players = EntityRegistry.AllNetworkPlayers;
                foreach (var p in players)
                {
                    if (p == null || p.gameObject == this.gameObject) continue;
                    float dist = Vector3.Distance(transform.position, p.transform.position);
                    if (dist < closestDist) { closestDist = dist; closestTarget = p; }
                }

                if (closestTarget != null)
                {
                    OnPunchHit?.Invoke();

                    var localNp = GetComponent<NetworkPlayer>();
                    if (localNp != null)
                    {
                        int targetType = 0;
                        uint targetNetId = 0;
                        string targetName = "";

                        if (closestTarget is NetworkPlayer npTarget)
                        {
                            targetType = 1;
                            var identity = npTarget.GetComponent<Mirror.NetworkIdentity>();
                            if (identity != null) targetNetId = identity.netId;
                        }
                        else if (closestTarget is NPCController npcTarget)
                        {
                            targetType = 2;
                            targetName = npcTarget.DisplayName;
                        }
                        else if (closestTarget is FakePlayerController fpTarget)
                        {
                            targetType = 3;
                            targetName = fpTarget.DisplayName;
                        }

                        localNp.CmdExecutePunch(transform.position, _lastMoveDir, targetType, targetNetId, targetName);
                    }
                    else
                    {
                        FightCloudEffect.Create(transform, closestTarget.transform, 1.5f);
                        if (closestTarget is NPCController)
                        {
                            Debug.Log("[PlayerController] Punched NPC. No jail penalty.");
                        }
                        else
                        {
                            Debug.Log("[PlayerController] Punched player/fake player! Sending to jail immediately.");
                            Invoke(nameof(GoToJail), 1.6f);
                        }
                    }
                }
            }
            finally
            {
                Invoke(nameof(EndPunch), 0.35f);
            }
        }

        public void GoToJail()
        {
            float jailDuration = 15f;
            Debug.Log($"[PlayerController] Player sent to jail at (2, 0.03, -62) for {jailDuration}s.");
            transform.position = new Vector3(2f, 0.03f, -62f);
            _isJailed = true;
            _jailTimer = jailDuration;
            _isClickMoving = false;
            SavePositionToPrefs();
            OnJailStart?.Invoke(jailDuration);
        }

        public void Stun(Vector3 puncherPos, float duration)
        {
            if (_isStunned) return;

            _isStunned = true;
            _isClickMoving = false;

            // Calculate knockback direction
            Vector3 knockDir = (transform.position - puncherPos).normalized;
            knockDir.y = 0;
            if (knockDir.sqrMagnitude < 0.001f) knockDir = Vector3.back;
            _knockbackVelocity = knockDir * 6f; // Initial push speed

            StartCoroutine(StunCoroutine(duration));
        }

        private System.Collections.IEnumerator StunCoroutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (elapsed < duration * 0.7f)
                {
                    // Wobble or spin the player visually
                    transform.Rotate(Vector3.up, 360f * Time.deltaTime * 2f);
                }
                yield return null;
            }

            _isStunned = false;
            _knockbackVelocity = Vector3.zero;
        }

        private void EndPunch() => _isPunching = false;

        private void ReleaseFromJail()
        {
            _isJailed = false;
            var lobbyPortal = _prisonPortal ?? GameObject.Find("PrisonPortal");
            Vector3 dest = lobbyPortal != null ? lobbyPortal.transform.position + new Vector3(0, 0, 1.2f) : new Vector3(0, 0.03f, -9.5f);
            dest.y = 0.03f;
            Debug.Log($"[PlayerController] Player released from jail. Moving to: {dest}");
            transform.position = dest;
            _teleportCooldownTimer = 1.0f;
            SavePositionToPrefs();
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
                ExecuteInteraction();
            }
        }

        /// <summary>
        /// Thực hiện tương tác với đối tượng gần nhất.
        /// Gọi bởi phím E (PC) hoặc nút Interact (mobile).
        /// </summary>
        /// <summary>
        /// Thực hiện tương tác với đối tượng gần nhất.
        /// Gọi bởi phím E (PC) hoặc nút Interact (mobile).
        /// </summary>
        public void ExecuteInteraction()
        {
            float interactDist = 3.5f;

            var spots = FindObjectsByType<FishingSpot>(FindObjectsSortMode.None);
            FishingSpot closestSpot = null;
            float minSpotDist = interactDist;
            foreach (var spot in spots)
            {
                float dist = Vector3.Distance(transform.position, spot.transform.position);
                if (dist < minSpotDist) { minSpotDist = dist; closestSpot = spot; }
            }
            if (closestSpot != null) { closestSpot.TryInteract(this); return; }

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

            // Nếu không có spot/plot/cloud, thử talk với NPC gần nhất
            if (_nearestInteractable != null)
            {
                var lobbyUI = FindAnyObjectByType<LobbyUI>();
                if (lobbyUI != null)
                {
                    lobbyUI.StartDialogue(_nearestInteractable);
                }
            }
        }

        public bool HasAnyNearbyInteractable()
        {
            if (_nearestInteractable != null) return true;

            float checkDist = 3.5f;

            var spots = FindObjectsByType<FishingSpot>(FindObjectsSortMode.None);
            foreach (var s in spots)
            {
                if (Vector3.Distance(transform.position, s.transform.position) < checkDist) return true;
            }

            var plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
            foreach (var p in plots)
            {
                if (Vector3.Distance(transform.position, p.transform.position) < checkDist) return true;
            }

            var clouds = FindObjectsByType<CloudLayer>(FindObjectsSortMode.None);
            foreach (var c in clouds)
            {
                if (Vector3.Distance(transform.position, c.transform.position) < checkDist) return true;
            }

            return false;
        }

        public string GetInteractionLabel()
        {
            float checkDist = 3.5f;

            var spots = FindObjectsByType<FishingSpot>(FindObjectsSortMode.None);
            FishingSpot closestSpot = null;
            float minSpotDist = checkDist;
            foreach (var spot in spots)
            {
                float dist = Vector3.Distance(transform.position, spot.transform.position);
                if (dist < minSpotDist) { minSpotDist = dist; closestSpot = spot; }
            }
            if (closestSpot != null)
            {
                if (closestSpot.State == FishingState.Idle) return "CÂU CÁ";
                if (closestSpot.State == FishingState.Biting) return "GIẬT CẦN!";
                return "ĐANG ĐỢI";
            }

            var plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
            GardenPlot closestPlot = null;
            float minPlotDist = checkDist;
            foreach (var plot in plots)
            {
                float dist = Vector3.Distance(transform.position, plot.transform.position);
                if (dist < minPlotDist) { minPlotDist = dist; closestPlot = plot; }
            }
            if (closestPlot != null)
            {
                if (closestPlot.State == PlotState.Empty) return "GIEO HẠT";
                if (closestPlot.State == PlotState.Ripe) return "THU HOẠCH";
                return "ĐANG LỚN";
            }

            var clouds = FindObjectsByType<CloudLayer>(FindObjectsSortMode.None);
            foreach (var c in clouds)
            {
                if (Vector3.Distance(transform.position, c.transform.position) < checkDist) return "NHẢY MÂY";
            }

            if (_nearestInteractable != null) return "TRÒ CHUYỆN";

            return "TƯƠNG TÁC";
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
            else if (sP != null && Vector3.Distance(currentPos, sP.transform.position) < portalRadius) Teleport(new Vector3(-60f, 0.05f, -12f));
            else if (gR != null && Vector3.Distance(currentPos, gR.transform.position) < portalRadius) Teleport(gP != null ? gP.transform.position + new Vector3(0, 0, -1.2f) : new Vector3(0, 0.05f, 9.5f));
            else if (pR != null && Vector3.Distance(currentPos, pR.transform.position) < portalRadius) Teleport(pP != null ? pP.transform.position + new Vector3(0, 0, 1.2f) : new Vector3(0, 0.05f, -9.5f));
            else if (fR != null && Vector3.Distance(currentPos, fR.transform.position) < portalRadius) Teleport(fP != null ? fP.transform.position + new Vector3(-1.2f, 0, 0) : new Vector3(9.5f, 0.05f, 0));
            else if (sR != null && Vector3.Distance(currentPos, sR.transform.position) < portalRadius) Teleport(sP != null ? sP.transform.position + new Vector3(1.2f, 0, 0) : new Vector3(-9.5f, 0.05f, 0));
        }

        private void Teleport(Vector3 destination)
        {
            Debug.Log($"[PlayerController] Teleporting player to: {destination}");
            transform.position = destination;
            _isClickMoving = false;
            _teleportCooldownTimer = 1.0f;
            SavePositionToPrefs();
        }

        private void DetectNearbyInteractables()
        {
            IInteractable closest = null;
            float closestDist = _interactionRange;

            var npcs = EntityRegistry.AllNPCs;
            foreach (var npc in npcs)
            {
                float dist = Vector3.Distance(transform.position, npc.transform.position);
                if (dist < closestDist) { closestDist = dist; closest = npc; }
            }

            var fakePlayers = EntityRegistry.AllFakePlayers;
            foreach (var fp in fakePlayers)
            {
                float dist = Vector3.Distance(transform.position, fp.transform.position);
                if (dist < closestDist) { closestDist = dist; closest = fp; }
            }

            var players = EntityRegistry.AllNetworkPlayers;
            foreach (var p in players)
            {
                if (p == null || p.gameObject == this.gameObject) continue;
                float dist = Vector3.Distance(transform.position, p.transform.position);
                if (dist < closestDist) { closestDist = dist; closest = p; }
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
            return CollisionUtils.IsValidPosition(pos, transform.root);
        }

        private Vector3 ClampToWorld(Vector3 pos)
        {
            float z = pos.z;
            float x = pos.x;

            if (z > 40f) { pos.x = Mathf.Clamp(x, -14f, 14f); pos.z = Mathf.Clamp(z, 46f, 74f); }
            else if (z < -40f) { pos.x = Mathf.Clamp(x, -14f, 14f); pos.z = Mathf.Clamp(z, -74f, -46f); }
            else if (x > 40f) { pos.x = Mathf.Clamp(x, 46f, 74f); pos.z = Mathf.Clamp(z, -14f, 14f); }
            else if (x < -40f) { pos.x = Mathf.Clamp(x, -74f, -46f); pos.z = Mathf.Clamp(z, -14f, 14f); }
            else { pos.x = Mathf.Clamp(x, _worldMinX, _worldMaxX); pos.z = Mathf.Clamp(z, _worldMinZ, _worldMaxZ); }

            return pos;
        }

        private Vector3 ResolveCollisions(Vector3 pos)
        {
            float playerRadius = 0.4f; // slightly smaller than player capsule model to prevent hard stops
            
            // Iterate 3 times to handle corners or multi-collision areas
            for (int iteration = 0; iteration < 3; iteration++)
            {
                Collider[] hits = Physics.OverlapSphere(pos + Vector3.up * 0.5f, playerRadius + 0.05f);
                bool resolvedAny = false;
                
                foreach (var hit in hits)
                {
                    if (!IsObstacle(hit)) continue;

                    Vector3 closestPoint = hit.ClosestPoint(pos + Vector3.up * 0.5f);
                    closestPoint.y = pos.y;

                    Vector3 toPlayer = pos - closestPoint;
                    toPlayer.y = 0;
                    float dist = toPlayer.magnitude;

                    if (dist < playerRadius)
                    {
                        float overlap = playerRadius - dist;
                        Vector3 pushDir = dist > 0.001f ? toPlayer.normalized : Vector3.forward;
                        pos += pushDir * overlap;
                        resolvedAny = true;
                    }
                }
                
                if (!resolvedAny) break;
            }
            
            return pos;
        }

        private bool IsObstacle(Collider hit)
        {
            if (hit.transform.root == transform.root) return false;
            if (hit.isTrigger) return false;

            string name = hit.gameObject.name;
            return name.Contains("Collider") || name.Contains("Obstacle") || name.Contains("Walls") ||
                   name.Contains("Tree") || name.Contains("Post") || name.Contains("Picket") ||
                   name.Contains("Seat") || name.Contains("Base") || name.Contains("Pillar") ||
                   name.Contains("Bowl") || name.Contains("Bench") || name.Contains("Fountain") ||
                   name.Contains("Fence") || name.Contains("House") || name.Contains("Shop");
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
