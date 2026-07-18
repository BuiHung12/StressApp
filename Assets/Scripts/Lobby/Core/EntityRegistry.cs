using System.Collections.Generic;
using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Static registry cho tất cả NPC và FakePlayer trong scene.
    /// Thay thế FindObjectsByType và GameObject.Find — O(1) lookup thay vì O(n) scan.
    /// NPCs/FakePlayers đăng ký tại Start(), hủy đăng ký tại OnDestroy().
    /// </summary>
    public static class EntityRegistry
    {
        // ── NPC Registry ──
        private static readonly Dictionary<string, NPCController> _npcs = new Dictionary<string, NPCController>();
        private static readonly List<NPCController> _npcList = new List<NPCController>();

        // ── FakePlayer Registry ──
        private static readonly Dictionary<string, FakePlayerController> _fakePlayers = new Dictionary<string, FakePlayerController>();
        private static readonly List<FakePlayerController> _fakePlayerList = new List<FakePlayerController>();

        // ── NetworkPlayer Registry ──
        private static readonly List<NetworkPlayer> _networkPlayers = new List<NetworkPlayer>();
        private static NetworkPlayer _firstServerPlayer;

        // ═══════════════════════════════════
        //  NPC Registration
        // ═══════════════════════════════════

        public static void RegisterNPC(NPCController npc)
        {
            if (npc == null) return;
            string key = npc.DisplayName;
            if (!_npcs.ContainsKey(key))
            {
                _npcs[key] = npc;
                _npcList.Add(npc);
            }
        }

        public static void UnregisterNPC(NPCController npc)
        {
            if (npc == null) return;
            string key = npc.DisplayName;
            _npcs.Remove(key);
            _npcList.Remove(npc);
        }

        /// <summary>
        /// O(1) lookup by display name. Dùng cho RPC sync thay vì GameObject.Find.
        /// </summary>
        public static NPCController GetNPC(string displayName)
        {
            _npcs.TryGetValue(displayName, out var npc);
            return npc;
        }

        /// <summary>
        /// Trả về list hiện tại (không allocate mới). Dùng thay FindObjectsByType.
        /// CẢNH BÁO: Không modify list này bên ngoài!
        /// </summary>
        public static IReadOnlyList<NPCController> AllNPCs => _npcList;

        // ═══════════════════════════════════
        //  FakePlayer Registration
        // ═══════════════════════════════════

        public static void RegisterFakePlayer(FakePlayerController fp)
        {
            if (fp == null) return;
            string key = fp.DisplayName;
            if (!_fakePlayers.ContainsKey(key))
            {
                _fakePlayers[key] = fp;
                _fakePlayerList.Add(fp);
            }
        }

        public static void UnregisterFakePlayer(FakePlayerController fp)
        {
            if (fp == null) return;
            string key = fp.DisplayName;
            _fakePlayers.Remove(key);
            _fakePlayerList.Remove(fp);
        }

        public static FakePlayerController GetFakePlayer(string displayName)
        {
            _fakePlayers.TryGetValue(displayName, out var fp);
            return fp;
        }

        public static IReadOnlyList<FakePlayerController> AllFakePlayers => _fakePlayerList;

        // ═══════════════════════════════════
        //  NetworkPlayer Registration
        // ═══════════════════════════════════

        public static void RegisterNetworkPlayer(NetworkPlayer np)
        {
            if (np == null || _networkPlayers.Contains(np)) return;
            _networkPlayers.Add(np);

            // First registered server player becomes the "primary" sync broadcaster
            if (_firstServerPlayer == null && np.isServer)
            {
                _firstServerPlayer = np;
            }
        }

        public static void UnregisterNetworkPlayer(NetworkPlayer np)
        {
            _networkPlayers.Remove(np);
            if (_firstServerPlayer == np)
            {
                _firstServerPlayer = null;
                // Reassign if there are other server players
                foreach (var p in _networkPlayers)
                {
                    if (p != null && p.isServer)
                    {
                        _firstServerPlayer = p;
                        break;
                    }
                }
            }
        }

        public static bool IsFirstServerPlayer(NetworkPlayer np)
        {
            return _firstServerPlayer == np;
        }

        public static IReadOnlyList<NetworkPlayer> AllNetworkPlayers => _networkPlayers;

        // ═══════════════════════════════════
        //  Cleanup (call on scene unload)
        // ═══════════════════════════════════

        public static void ClearAll()
        {
            _npcs.Clear();
            _npcList.Clear();
            _fakePlayers.Clear();
            _fakePlayerList.Clear();
            _networkPlayers.Clear();
            _firstServerPlayer = null;
        }
    }
}
