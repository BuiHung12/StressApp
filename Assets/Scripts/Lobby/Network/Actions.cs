using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public partial class NetworkPlayer : NetworkBehaviour, IInteractable
    {
        // ── Commands (Client → Server) ──

        [Command]
        private void CmdSyncPosition(Vector3 position, float rotationY, bool isMoving)
        {
            // if (_syncIsMoving != isMoving)
            // {
            //     Debug.Log($"[NetworkPlayer] {DisplayName} (connId={connIdStr}) {(isMoving ? "STARTED moving" : "STOPPED moving")} at position {position}");
            // }
            // else if (isMoving && Time.time - _lastMoveLogTime >= 2.0f)
            // {
            //     _lastMoveLogTime = Time.time;
            //     Debug.Log($"[NetworkPlayer] {DisplayName} (connId={connIdStr}) moving at position {position}");
            // }

            _syncPosition = position;
            _syncRotationY = rotationY;
            _syncIsMoving = isMoving;
            transform.position = position;
        }

        [Command]
        private void CmdSetDisplayName(string name) { DisplayName = name; }

        [Command]
        private void CmdSetDeviceId(string id) { DeviceId = id; }

        [Command]
        public void CmdSetFullCustomization(int gender, Color bodyColor, int hairStyle, Color hairColor, int outfitStyle, int pantsStyle, Color pantsColor)
        {
            Gender = gender;
            BodyColor = bodyColor;
            HairStyle = hairStyle;
            HairColor = hairColor;
            OutfitStyle = outfitStyle;
            PantsStyle = pantsStyle;
            PantsColor = pantsColor;
        }

        // ── Punch sync ──

        [Command]
        public void CmdExecutePunch(Vector3 position, Vector3 direction, int targetType, uint targetNetId, string targetName)
        {
            RpcOnPunchExecuted(netIdentity, position, direction, targetType, targetNetId, targetName);

            // Server-side reactions
            if (targetType == 1 && targetNetId != 0)
            {
                if (NetworkServer.spawned.TryGetValue(targetNetId, out NetworkIdentity identity))
                {
                    var targetNp = identity.GetComponent<NetworkPlayer>();
                    if (targetNp != null)
                    {
                        Invoke(nameof(ServerSendToJail), 1.6f);
                    }
                }
            }
            else if (targetType == 2 && !string.IsNullOrEmpty(targetName))
            {
                var npc = EntityRegistry.GetNPC(targetName);
                if (npc != null)
                {
                    Vector3 knockDir = (npc.transform.position - position).normalized;
                    knockDir.y = 0;
                    npc.ReceivePunch(knockDir, 8f);
                }
            }
            else if (targetType == 3 && !string.IsNullOrEmpty(targetName))
            {
                var fp = EntityRegistry.GetFakePlayer(targetName);
                if (fp != null)
                {
                    Vector3 knockDir = (fp.transform.position - position).normalized;
                    knockDir.y = 0;
                    fp.ReceivePunch(knockDir, 8f);
                }
            }
        }

        private void ServerSendToJail()
        {
            transform.position = new Vector3(2f, 0.03f, -62f);
            RpcSendToJail(15f);
        }

        [ClientRpc]
        private void RpcSendToJail(float duration)
        {
            if (isLocalPlayer)
            {
                var pc = GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.GoToJail();
                }
            }
        }

        [ClientRpc]
        private void RpcOnPunchExecuted(NetworkIdentity puncher, Vector3 position, Vector3 direction, int targetType, uint targetNetId, string targetName)
        {
            if (puncher == null) return;

            if (!isLocalPlayer)
            {
                var anim = GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("Punch");
                }
            }

            Transform targetTrans = null;
            if (targetType == 1 && targetNetId != 0)
            {
                if (NetworkClient.spawned.TryGetValue(targetNetId, out NetworkIdentity identity))
                {
                    targetTrans = identity.transform;
                }
            }
            else if (targetType == 2 && !string.IsNullOrEmpty(targetName))
            {
                var npc = EntityRegistry.GetNPC(targetName);
                if (npc != null) targetTrans = npc.transform;
            }
            else if (targetType == 3 && !string.IsNullOrEmpty(targetName))
            {
                var fp = EntityRegistry.GetFakePlayer(targetName);
                if (fp != null) targetTrans = fp.transform;
            }

            if (targetTrans != null)
            {
                FightCloudEffect.Create(puncher.transform, targetTrans, 1.5f);

                if (targetType == 1 && targetNetId != 0)
                {
                    if (NetworkClient.spawned.TryGetValue(targetNetId, out NetworkIdentity identity))
                    {
                        var targetNp = identity.GetComponent<NetworkPlayer>();
                        if (targetNp != null && targetNp.isLocalPlayer)
                        {
                            var pc = targetNp.GetComponent<PlayerController>();
                            if (pc != null)
                            {
                                pc.Stun(position, 5f);
                            }
                        }
                    }
                }
                else if (targetType == 2 && !string.IsNullOrEmpty(targetName))
                {
                    var npc = EntityRegistry.GetNPC(targetName);
                    if (npc != null && npc.CanBePunched)
                    {
                        Vector3 knockDir = (npc.transform.position - position).normalized;
                        knockDir.y = 0;
                        npc.ReceivePunch(knockDir, 8f);
                    }
                }
                else if (targetType == 3 && !string.IsNullOrEmpty(targetName))
                {
                    var fp = EntityRegistry.GetFakePlayer(targetName);
                    if (fp != null)
                    {
                        Vector3 knockDir = (fp.transform.position - position).normalized;
                        knockDir.y = 0;
                        fp.ReceivePunch(knockDir, 8f);
                    }
                }
            }
        }

        // ── SyncVar Hooks ──

        private void OnDisplayNameChanged(string oldName, string newName)
        {
            var nameTag = transform.Find("NameTag");
            if (nameTag != null)
            {
                var tmp = nameTag.GetComponentInChildren<TMPro.TextMeshPro>();
                if (tmp != null) tmp.text = newName;
            }
            gameObject.name = newName;
        }

        // Generic appearance hook — any customization SyncVar change triggers full re-apply
        private void OnAppearanceChanged(int oldVal, int newVal)
        {
            _appearanceDirty = true;
        }

        private void OnAppearanceChanged(Color oldVal, Color newVal)
        {
            _appearanceDirty = true;
        }

        // ── NPC & FakePlayer Position/State Synchronizer ──

        private void SyncNPCsToServer()
        {
            // === Sync NPCs ===
            var npcs = EntityRegistry.AllNPCs;
            if (npcs.Count > 0)
            {
                string[] names = new string[npcs.Count];
                Vector3[] positions = new Vector3[npcs.Count];
                float[] rotationsY = new float[npcs.Count];
                bool[] isHurts = new bool[npcs.Count];

                for (int i = 0; i < npcs.Count; i++)
                {
                    names[i] = npcs[i].DisplayName;
                    positions[i] = npcs[i].transform.position;
                    rotationsY[i] = npcs[i].transform.eulerAngles.y;
                    isHurts[i] = npcs[i].IsHurt;
                }

                RpcSyncNPCs(names, positions, rotationsY, isHurts);
            }

            // === Sync FakePlayers ===
            var fakePlayers = EntityRegistry.AllFakePlayers;
            if (fakePlayers.Count > 0)
            {
                string[] fpNames = new string[fakePlayers.Count];
                Vector3[] fpPositions = new Vector3[fakePlayers.Count];
                float[] fpRotationsY = new float[fakePlayers.Count];

                for (int i = 0; i < fakePlayers.Count; i++)
                {
                    fpNames[i] = fakePlayers[i].DisplayName;
                    fpPositions[i] = fakePlayers[i].transform.position;
                    fpRotationsY[i] = fakePlayers[i].transform.eulerAngles.y;
                }

                RpcSyncFakePlayers(fpNames, fpPositions, fpRotationsY);
            }
        }

        [ClientRpc]
        private void RpcSyncNPCs(string[] names, Vector3[] positions, float[] rotationsY, bool[] isHurts)
        {
            if (isServer) return;

            for (int i = 0; i < names.Length; i++)
            {
                // O(1) lookup via EntityRegistry instead of O(n) GameObject.Find
                var npcCtrl = EntityRegistry.GetNPC(names[i]);
                if (npcCtrl != null)
                {
                    npcCtrl.SetSyncData(positions[i], rotationsY[i], isHurts[i]);
                }
            }
        }

        [ClientRpc]
        private void RpcSyncFakePlayers(string[] names, Vector3[] positions, float[] rotationsY)
        {
            if (isServer) return;

            for (int i = 0; i < names.Length; i++)
            {
                var fp = EntityRegistry.GetFakePlayer(names[i]);
                if (fp != null)
                {
                    fp.SetSyncData(positions[i], rotationsY[i]);
                }
            }
        }

        // ── GardenPlot Sync ──

        [Command]
        public void CmdPlantSeed(int plotIndex)
        {
            RpcPlantSeed(plotIndex);
        }

        [ClientRpc]
        private void RpcPlantSeed(int plotIndex)
        {
            var plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
            if (plotIndex >= 0 && plotIndex < plots.Length)
            {
                plots[plotIndex].ForceSetState(PlotState.Growing);
            }
        }

        [Command]
        public void CmdHarvestPlot(int plotIndex)
        {
            RpcHarvestPlot(plotIndex);
        }

        [ClientRpc]
        private void RpcHarvestPlot(int plotIndex)
        {
            var plots = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
            if (plotIndex >= 0 && plotIndex < plots.Length)
            {
                plots[plotIndex].ForceSetState(PlotState.Empty);
            }
        }

        // ── FishingSpot Sync ──

        [Command]
        public void CmdCastFish(int spotIndex)
        {
            RpcCastFish(spotIndex);
        }

        [ClientRpc]
        private void RpcCastFish(int spotIndex)
        {
            var spots = FindObjectsByType<FishingSpot>(FindObjectsSortMode.None);
            if (spotIndex >= 0 && spotIndex < spots.Length)
            {
                spots[spotIndex].ForceSetState(FishingState.Waiting);
            }
        }

        [Command]
        public void CmdCatchFish(int spotIndex)
        {
            RpcCatchFish(spotIndex);
        }

        [ClientRpc]
        private void RpcCatchFish(int spotIndex)
        {
            var spots = FindObjectsByType<FishingSpot>(FindObjectsSortMode.None);
            if (spotIndex >= 0 && spotIndex < spots.Length)
            {
                spots[spotIndex].ForceSetState(FishingState.Idle);
            }
        }

        // ── CloudLayer Unlock Sync ──

        [Command]
        public void CmdUnlockCloud(int cloudIndex)
        {
            RpcUnlockCloud(cloudIndex);
        }

        [ClientRpc]
        private void RpcUnlockCloud(int cloudIndex)
        {
            var clouds = FindObjectsByType<CloudLayer>(FindObjectsSortMode.None);
            if (cloudIndex >= 0 && cloudIndex < clouds.Length)
            {
                clouds[cloudIndex].ForceUnlock();
            }
        }
        
        // ── IInteractable Implementation ──
        string IInteractable.DisplayName => DisplayName;
        public string AvatarEmoji => Gender == 0 ? "👦" : "👧";
        public bool CanTalk => false;
        public bool CanBePunched => true;
        public InteractableType Type => InteractableType.Player;
        public string[] GetDialogueLines() => new string[] { "..." };
    }

    }
