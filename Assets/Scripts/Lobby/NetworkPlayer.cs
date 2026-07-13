using UnityEngine;
using Mirror;

namespace RangerCity.Lobby
{
    /// <summary>
    /// NetworkPlayer — đồng bộ nhân vật qua mạng.
    /// Gắn kèm PlayerController trên cùng GameObject.
    /// Quản lý: position sync, animation sync, identity, customization.
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        [Header("Sync Settings")]
        [SerializeField] private float _syncInterval = 0.05f; // 20 updates/sec

        // ── Identity ──
        [SyncVar(hook = nameof(OnDisplayNameChanged))]
        public string DisplayName = "Player";

        [SyncVar]
        public string DeviceId = "";

        // ── Customization SyncVars ──
        [SyncVar(hook = nameof(OnAppearanceChanged))]
        public int HairStyle = 0;

        [SyncVar(hook = nameof(OnAppearanceChanged))]
        public Color HairColor = new Color(0.18f, 0.12f, 0.08f);

        [SyncVar(hook = nameof(OnAppearanceChanged))]
        public int OutfitStyle = 0;

        [SyncVar(hook = nameof(OnAppearanceChanged))]
        public Color BodyColor = new Color(0.26f, 0.65f, 0.96f);

        [SyncVar(hook = nameof(OnAppearanceChanged))]
        public int PantsStyle = 0;

        [SyncVar(hook = nameof(OnAppearanceChanged))]
        public Color PantsColor = new Color(0.25f, 0.35f, 0.55f);

        // Sync vị trí mượt mà
        [SyncVar] private Vector3 _syncPosition;
        [SyncVar] private float _syncRotationY;
        [SyncVar] private bool _syncIsMoving;

        private PlayerController _playerController;
        private float _syncTimer;
        private Vector3 _smoothVelocity;

        // ── Preset Palettes ──

        public static readonly Color[] BodyColorPalette = new Color[]
        {
            new Color(0.26f, 0.65f, 0.96f),  // Xanh dương
            new Color(0.94f, 0.33f, 0.31f),  // Đỏ
            new Color(0.4f, 0.73f, 0.42f),   // Xanh lá
            new Color(1f, 0.72f, 0.3f),      // Cam
            new Color(0.49f, 0.34f, 0.76f),  // Tím
            new Color(1f, 0.84f, 0f),        // Vàng
            new Color(0.36f, 0.42f, 0.75f),  // Chàm
            new Color(0.94f, 0.47f, 0.76f),  // Hồng
            new Color(0.3f, 0.8f, 0.8f),     // Cyan
            new Color(0.85f, 0.85f, 0.85f),  // Trắng bạc
        };

        // Giữ backward compatibility
        public static Color[] ColorPalette => BodyColorPalette;

        public static readonly Color[] HairColorPalette = new Color[]
        {
            new Color(0.18f, 0.12f, 0.08f),  // Đen
            new Color(0.45f, 0.28f, 0.12f),  // Nâu đậm
            new Color(0.68f, 0.45f, 0.2f),   // Nâu vàng
            new Color(0.92f, 0.78f, 0.4f),   // Vàng
            new Color(0.82f, 0.32f, 0.18f),  // Đỏ gạch
            new Color(0.55f, 0.55f, 0.6f),   // Xám bạc
            new Color(0.3f, 0.5f, 0.85f),    // Xanh dương
            new Color(0.75f, 0.3f, 0.65f),   // Tím hồng
        };

        public static readonly Color[] PantsColorPalette = new Color[]
        {
            new Color(0.25f, 0.35f, 0.55f),  // Jeans xanh
            new Color(0.15f, 0.15f, 0.18f),  // Đen
            new Color(0.55f, 0.42f, 0.3f),   // Khaki
            new Color(0.4f, 0.25f, 0.2f),    // Nâu
            new Color(0.35f, 0.45f, 0.35f),  // Xanh rêu
            new Color(0.75f, 0.75f, 0.78f),  // Xám nhạt
        };

        // Tên kiểu tóc (dùng trong UI)
        public static readonly string[] HairStyleNames = { "Ngắn", "Dài", "Mohawk", "Afro", "Ponytail", "Trọc" };
        public static readonly string[] HairStyleIcons = { "💇", "💁", "🦔", "🌀", "🎀", "🥚" };

        // Tên kiểu áo
        public static readonly string[] OutfitStyleNames = { "T-Shirt", "Vest", "Hoodie", "Tank Top", "Jacket" };
        public static readonly string[] OutfitStyleIcons = { "👕", "🎽", "🧥", "🩳", "🧱" };

        // Tên kiểu quần
        public static readonly string[] PantsStyleNames = { "Jeans", "Shorts", "Cargo", "Skirt" };
        public static readonly string[] PantsStyleIcons = { "👖", "🩳", "🪖", "👗" };

        // ── Lifecycle ──

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            _playerController = GetComponent<PlayerController>();
            if (_playerController != null)
                _playerController.enabled = true;

            // Đọc tên từ PlayerPrefs
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (string.IsNullOrEmpty(savedName))
            {
                string[] names = { "Ranger", "Scout", "Explorer", "Hero", "Knight" };
                savedName = names[Random.Range(0, names.Length)] + Random.Range(100, 999);
            }
            CmdSetDisplayName(savedName);

            // Đọc customization từ PlayerPrefs
            int bodyColorIdx = PlayerPrefs.GetInt("PlayerColorIndex", 0);
            Color bodyCol = (bodyColorIdx >= 0 && bodyColorIdx < BodyColorPalette.Length)
                ? BodyColorPalette[bodyColorIdx]
                : Color.HSVToRGB(Random.value, 0.6f, 0.9f);

            int hairStyleIdx = PlayerPrefs.GetInt("PlayerHairStyle", 0);
            int hairColorIdx = PlayerPrefs.GetInt("PlayerHairColor", 0);
            Color hairCol = (hairColorIdx >= 0 && hairColorIdx < HairColorPalette.Length)
                ? HairColorPalette[hairColorIdx]
                : HairColorPalette[0];

            int outfitIdx = PlayerPrefs.GetInt("PlayerOutfitStyle", 0);

            int pantsStyleIdx = PlayerPrefs.GetInt("PlayerPantsStyle", 0);
            int pantsColorIdx = PlayerPrefs.GetInt("PlayerPantsColor", 0);
            Color pantsCol = (pantsColorIdx >= 0 && pantsColorIdx < PantsColorPalette.Length)
                ? PantsColorPalette[pantsColorIdx]
                : PantsColorPalette[0];

            // Gửi tất cả customization lên server
            CmdSetFullCustomization(bodyCol, hairStyleIdx, hairCol, outfitIdx, pantsStyleIdx, pantsCol);

            // Gửi Device ID
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            CmdSetDeviceId(deviceId);

            Debug.Log($"[NetworkPlayer] Local player: {savedName}, Hair:{hairStyleIdx}, Outfit:{outfitIdx}, Pants:{pantsStyleIdx}");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!isLocalPlayer)
            {
                var pc = GetComponent<PlayerController>();
                if (pc != null) pc.enabled = false;
            }

            // Apply current appearance on join (hooks chỉ fire khi giá trị thay đổi)
            LobbySetup.ApplyCustomization(gameObject, HairStyle, HairColor, OutfitStyle, BodyColor, PantsStyle, PantsColor);
        }

        private void Update()
        {
            if (netIdentity == null || netIdentity.netId == 0) return;

            if (isLocalPlayer)
            {
                _syncTimer += Time.deltaTime;
                if (_syncTimer >= _syncInterval)
                {
                    _syncTimer = 0f;
                    CmdSyncPosition(transform.position, transform.eulerAngles.y, IsMoving());

                    // If we are the Host (Server), also broadcast the positions of all NPCs!
                    if (isServer)
                    {
                        SyncNPCsToServer();
                    }
                }
            }
            else
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position, _syncPosition, ref _smoothVelocity, 0.1f);
                UpdateRemoteAnimation();
            }
        }

        private bool IsMoving()
        {
            if (_playerController != null)
            {
                float speed = (transform.position - _syncPosition).magnitude / _syncInterval;
                return speed > 0.1f;
            }
            return false;
        }

        private void UpdateRemoteAnimation() { }

        // ── Commands (Client → Server) ──

        [Command]
        private void CmdSyncPosition(Vector3 position, float rotationY, bool isMoving)
        {
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
        private void CmdSetFullCustomization(Color bodyColor, int hairStyle, Color hairColor, int outfitStyle, int pantsStyle, Color pantsColor)
        {
            BodyColor = bodyColor;
            HairStyle = hairStyle;
            HairColor = hairColor;
            OutfitStyle = outfitStyle;
            PantsStyle = pantsStyle;
            PantsColor = pantsColor;
        }

        // ── Punch sync ──

        [Command]
        public void CmdPunch(Vector3 direction)
        {
            RpcShowPunchEffect(direction);
        }

        [ClientRpc]
        private void RpcShowPunchEffect(Vector3 direction)
        {
            Debug.Log($"[NetworkPlayer] {DisplayName} punched!");
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
            LobbySetup.ApplyCustomization(gameObject, HairStyle, HairColor, OutfitStyle, BodyColor, PantsStyle, PantsColor);
        }

        private void OnAppearanceChanged(Color oldVal, Color newVal)
        {
            LobbySetup.ApplyCustomization(gameObject, HairStyle, HairColor, OutfitStyle, BodyColor, PantsStyle, PantsColor);
        }

        // ── NPC Position/State Synchronizer ──

        private void SyncNPCsToServer()
        {
            var npcs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
            if (npcs == null || npcs.Length == 0) return;

            string[] names = new string[npcs.Length];
            Vector3[] positions = new Vector3[npcs.Length];
            float[] rotationsY = new float[npcs.Length];
            bool[] isHurts = new bool[npcs.Length];

            for (int i = 0; i < npcs.Length; i++)
            {
                names[i] = npcs[i].DisplayName;
                positions[i] = npcs[i].transform.position;
                rotationsY[i] = npcs[i].transform.eulerAngles.y;
                isHurts[i] = npcs[i].IsHurt;
            }

            RpcSyncNPCs(names, positions, rotationsY, isHurts);
        }

        [ClientRpc]
        private void RpcSyncNPCs(string[] names, Vector3[] positions, float[] rotationsY, bool[] isHurts)
        {
            // Clients (excluding Server/Host, who already runs the simulation locally) apply sync data
            if (isServer) return;

            for (int i = 0; i < names.Length; i++)
            {
                var npcObj = GameObject.Find(names[i]);
                if (npcObj != null)
                {
                    var npcCtrl = npcObj.GetComponent<NPCController>();
                    if (npcCtrl != null)
                    {
                        npcCtrl.SetSyncData(positions[i], rotationsY[i], isHurts[i]);
                    }
                }
            }
        }
    }
}
