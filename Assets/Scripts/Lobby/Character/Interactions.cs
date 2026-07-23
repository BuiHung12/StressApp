using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public partial class PlayerController : MonoBehaviour
    {
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


    }
    }
