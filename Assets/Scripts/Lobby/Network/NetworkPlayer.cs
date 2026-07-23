using UnityEngine;
using Mirror;

namespace RangerCity.Lobby
{
    /// <summary>
    /// NetworkPlayer — đồng bộ nhân vật qua mạng.
    /// Gắn kèm PlayerController trên cùng GameObject.
    /// Quản lý: position sync, animation sync, identity, customization.
    /// </summary>
    public partial class NetworkPlayer : NetworkBehaviour, IInteractable
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
        public int Gender = 0; // 0 = Male, 1 = Female

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
        private float _syncTimerLocal;
        private Vector3 _smoothVelocity;
        private bool _appearanceDirty = false;
        private Animator _animator;
        private float _lastMoveLogTime;

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

        // Tên kiểu tóc nam / nữ (dùng trong UI)
        public static readonly string[] MaleHairStyleNames = { "Tóc Ngắn", "Mohawk", "Afro", "Tóc Dựng", "Undercut", "Trọc" };
        public static readonly string[] FemaleHairStyleNames = { "Đuôi Ngựa", "Tóc Dài", "Tóc Búi", "Tóc Bob", "Bím Tóc", "Xoăn Ngắn" };

        // Tên kiểu áo nam / nữ
        public static readonly string[] MaleOutfitStyleNames = { "T-Shirt", "Vest", "Hoodie", "Ba Lỗ", "Áo Khoác" };
        public static readonly string[] FemaleOutfitStyleNames = { "Croptop", "Đầm", "Hoodie Nữ", "Hai Dây", "Áo Khoác Nữ" };

        // Tên kiểu quần nam / nữ
        public static readonly string[] MalePantsStyleNames = { "Jeans", "Shorts", "Cargo", "Joggers" };
        public static readonly string[] FemalePantsStyleNames = { "Skinny Jeans", "Váy Ngắn", "Shorts Nữ", "Leggings" };

        // Backward compatibility
        public static string[] HairStyleNames => MaleHairStyleNames;
        public static string[] OutfitStyleNames => MaleOutfitStyleNames;
        public static string[] PantsStyleNames => MalePantsStyleNames;

        // ── Lifecycle ──

        private void Awake()
        {
            transform.localScale = Vector3.one * 0.45f;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            _playerController = GetComponent<PlayerController>();
            if (_playerController != null)
                _playerController.enabled = true;

            // Update camera target to point to this spawned local player
            var lobbyCam = FindAnyObjectByType<LobbyCamera>();
            if (lobbyCam != null)
            {
                lobbyCam.SetTarget(transform);
            }

            // Đọc tên từ PlayerPrefs
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (string.IsNullOrEmpty(savedName))
            {
                string[] names = { "Ranger", "Scout", "Explorer", "Hero", "Knight" };
                savedName = names[Random.Range(0, names.Length)] + Random.Range(100, 999);
            }
            CmdSetDisplayName(savedName);

            // Đọc customization từ PlayerPrefs
            int gender = PlayerPrefs.GetInt("PlayerGender", 0);

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

            // Gửi tất cả customization lên server (thêm gender)
            CmdSetFullCustomization(gender, bodyCol, hairStyleIdx, hairCol, outfitIdx, pantsStyleIdx, pantsCol);

            // Gán giá trị cục bộ ngay lập tức để người chơi nhìn thấy nhân vật của mình thay đổi không cần chờ server sync
            Gender = gender;
            BodyColor = bodyCol;
            HairStyle = hairStyleIdx;
            HairColor = hairCol;
            OutfitStyle = outfitIdx;
            PantsStyle = pantsStyleIdx;
            PantsColor = pantsCol;
            _appearanceDirty = true;

            // Gửi Device ID
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            CmdSetDeviceId(deviceId);

            var lobbyUI = FindAnyObjectByType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.SetPlayer(_playerController);
            }

            Debug.Log($"[NetworkPlayer] Local player: {savedName}, Gender:{gender}, Hair:{hairStyleIdx}, Outfit:{outfitIdx}, Pants:{pantsStyleIdx}");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // Register in EntityRegistry on the server so that headless server has a valid _networkPlayers list
            EntityRegistry.RegisterNetworkPlayer(this);

            // Disable PlayerController on the server to prevent physics/portals queries running twice
            var pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!isLocalPlayer)
            {
                var pc = GetComponent<PlayerController>();
                if (pc != null) pc.enabled = false;
            }

            // Register in EntityRegistry for O(1) lookups
            EntityRegistry.RegisterNetworkPlayer(this);

            // Apply current appearance on join
            CharacterVisuals.ApplyCustomization(gameObject, Gender, HairStyle, HairColor, OutfitStyle, BodyColor, PantsStyle, PantsColor);
        }

        private void OnDestroy()
        {
            EntityRegistry.UnregisterNetworkPlayer(this);
        }

        private void Update()
        {
            if (netIdentity == null || netIdentity.netId == 0) return;

            // Cập nhật đồ họa nhân vật nếu có thay đổi từ SyncVar
            if (_appearanceDirty)
            {
                _appearanceDirty = false;
                CharacterVisuals.ApplyCustomization(gameObject, Gender, HairStyle, HairColor, OutfitStyle, BodyColor, PantsStyle, PantsColor);
            }

            // Broadcast NPC and FakePlayer positions from server
            // Use EntityRegistry instead of FindObjectsByType for O(1) check
            if (isServer && EntityRegistry.IsFirstServerPlayer(this))
            {
                _syncTimer += Time.deltaTime;
                if (_syncTimer >= _syncInterval)
                {
                    _syncTimer = 0f;
                    SyncNPCsToServer();
                }
            }

            if (isLocalPlayer && NetworkClient.active && NetworkClient.connection != null)
            {
                _syncTimerLocal += Time.deltaTime;
                if (_syncTimerLocal >= _syncInterval)
                {
                    _syncTimerLocal = 0f;
                    CmdSyncPosition(transform.position, transform.eulerAngles.y, IsMoving());
                }
            }
            else
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
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, _syncRotationY, 0), Time.deltaTime * 10f);
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

        private void UpdateRemoteAnimation()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (_animator != null)
            {
                _animator.SetFloat("Speed", _syncIsMoving ? 1f : 0f);
            }
        }


}
}
