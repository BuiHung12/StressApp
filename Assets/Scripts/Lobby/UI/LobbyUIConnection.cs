using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

namespace RangerCity.Lobby
{
    public partial class LobbyUI : MonoBehaviour
    {
        // Customization state
        private int _selectedGender = 0; // 0 = Nam, 1 = Nữ
        private int _selectedHairStyle = 0;
        private int _selectedHairColor = 0;
        private int _selectedOutfitStyle = 0;
        private int _selectedBodyColor = 0;
        private int _selectedPantsStyle = 0;
        private int _selectedPantsColor = 0;

        // Preview
        private GameObject _previewCharacter;
        private Camera _previewCamera;
        private RenderTexture _previewRT;
        private float _previewRotation;

        // Tab UI
        private int _activeTab = 0; // 0=hair, 1=outfit, 2=pants
        private GameObject _tabHairContent;
        private GameObject _tabOutfitContent;
        private GameObject _tabPantsContent;
        private Image[] _tabButtons;
        private Image[] _genderButtons;

        // Centralized DB Integration
        private string _apiBaseUrl = "http://bore.pub:57223/api/player";
        private bool _isSyncingWithServer = false;
        private bool _hasLoadedFromDatabase = false;
        private bool _isConnecting = false;

        private void CreateConnectionUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Load DB config file
            LoadDbConfig();

            // Only trigger database load once
            if (!_hasLoadedFromDatabase)
            {
                _hasLoadedFromDatabase = true;
                StartCoroutine(LoadFromDatabaseServerCoroutine());
            }

            // Load saved prefs
            _selectedGender = PlayerPrefs.GetInt("PlayerGender", 0);
            _selectedBodyColor = PlayerPrefs.GetInt("PlayerColorIndex", 0);
            _selectedHairStyle = PlayerPrefs.GetInt("PlayerHairStyle", 0);
            _selectedHairColor = PlayerPrefs.GetInt("PlayerHairColor", 0);
            _selectedOutfitStyle = PlayerPrefs.GetInt("PlayerOutfitStyle", 0);
            _selectedPantsStyle = PlayerPrefs.GetInt("PlayerPantsStyle", 0);
            _selectedPantsColor = PlayerPrefs.GetInt("PlayerPantsColor", 0);
            string savedName = PlayerPrefs.GetString("PlayerName", "");

            // ── Create No Internet Overlay if not exists ──
            if (_noInternetOverlay == null)
            {
                _noInternetOverlay = new GameObject("NoInternetOverlay", typeof(RectTransform));
                _noInternetOverlay.transform.SetParent(canvas.transform, false);
                var overlayRt = _noInternetOverlay.GetComponent<RectTransform>();
                overlayRt.anchorMin = Vector2.zero; overlayRt.anchorMax = Vector2.one;
                overlayRt.offsetMin = overlayRt.offsetMax = Vector2.zero;
                
                var overlayImg = _noInternetOverlay.AddComponent<Image>();
                overlayImg.color = new Color(0f, 0f, 0.05f, 0.75f); // translucent dark blue wash overlay
                
                // Border panel first (renders behind Card panel)
                var warnBorder = CreateRoundedPanel(_noInternetOverlay.transform, "CardBorder", Vector2.zero, new Vector2(858, 458), new Color(1f, 0.65f, 0.15f, 0.8f));
                
                // Card panel second (renders in front of Border panel)
                var warnCard = CreateRoundedPanel(_noInternetOverlay.transform, "WarnCard", Vector2.zero, new Vector2(850, 450), new Color(0.1f, 0.12f, 0.18f, 0.98f));

                // Add 3D drop shadow to card
                var shadow = warnCard.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
                shadow.effectDistance = new Vector2(4, -4);

                // Warning Texts (Children of warnCard)
                MakeText(warnCard.transform, "Title", "[!] KHÔNG CÓ KẾT NỐI INTERNET", 32,
                    new Vector2(0, 130), new Vector2(780, 50), TextAlignmentOptions.Center, new Color(1f, 0.65f, 0.15f));

                MakeText(warnCard.transform, "Content", "Vui lòng kiểm tra lại đường truyền Wifi hoặc dữ liệu di động 3G/4G trên thiết bị của bạn để tiếp tục.", 22,
                    new Vector2(0, 15), new Vector2(780, 150), TextAlignmentOptions.Center, new Color(0.9f, 0.9f, 0.95f));

                // Reconnection loading line (Rounded)
                var progressBg = CreateRoundedPanel(warnCard.transform, "ProgressBg", new Vector2(0, -100), new Vector2(600, 12), new Color(0.05f, 0.06f, 0.1f, 1f));
                var progressFill = CreateRoundedPanel(progressBg.transform, "ProgressFill", Vector2.zero, new Vector2(600, 12), new Color(1f, 0.65f, 0.15f, 0.9f));

                MakeText(warnCard.transform, "Status", "Đang tự động kết nối lại...", 18,
                    new Vector2(0, -140), new Vector2(780, 30), TextAlignmentOptions.Center, new Color(0.5f, 0.75f, 1f));
                
                // Initially active if internet was not verified yet, or handled by SetInternetState
                _noInternetOverlay.SetActive(!_isInternetAvailable);
            }

            // ── Full-screen overlay ──
            _connectionPanel = new GameObject("ConnectionPanel", typeof(RectTransform));
            _connectionPanel.transform.SetParent(canvas.transform, false);
            var panelRt = _connectionPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;
            var panelImg = _connectionPanel.AddComponent<Image>();
            panelImg.color = new Color(0.04f, 0.05f, 0.09f, 0.96f);

            // ── Main Card (glassmorphism dashboard) ──
            var card = CreatePanel(_connectionPanel.transform, "MainCard", Vector2.zero, new Vector2(1040, 650));
            var cardImg = card.GetComponent<Image>();
            cardImg.color = new Color(0.08f, 0.1f, 0.15f, 0.97f);

            var cardBorder = CreatePanel(card.transform, "CardBorder", Vector2.zero, new Vector2(1046, 656));
            cardBorder.transform.SetAsFirstSibling();
            cardBorder.GetComponent<Image>().color = new Color(0.02f, 0.65f, 0.95f, 0.5f);

            // ── Top Header Bar ──
            var titleBar = CreatePanel(card.transform, "TitleBar", new Vector2(0, 295), new Vector2(1000, 48));
            titleBar.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.11f, 0.95f);

            // Left Logo Badge
            var badge = CreatePanel(titleBar.transform, "Badge", new Vector2(-410, 0), new Vector2(130, 32));
            badge.GetComponent<Image>().color = new Color(0.02f, 0.52f, 0.85f, 0.9f);
            MakeText(badge.transform, "BadgeTxt", "RANGER CITY", 13, Vector2.zero, new Vector2(120, 28), TextAlignmentOptions.Center, Color.white);

            // Title
            MakeText(titleBar.transform, "TitleText", "TÙY CHỈNH NHÂN VẬT 3D", 22,
                new Vector2(0, 0), new Vector2(500, 40), TextAlignmentOptions.Center, new Color(0.4f, 0.88f, 1f));

            // Server Online Status Badge
            var statusBadge = CreatePanel(titleBar.transform, "StatusBadge", new Vector2(400, 0), new Vector2(140, 32));
            statusBadge.GetComponent<Image>().color = new Color(0.06f, 0.35f, 0.22f, 0.9f);
            MakeText(statusBadge.transform, "StatusTxt", "SERVER ONLINE", 12, Vector2.zero, new Vector2(130, 28), TextAlignmentOptions.Center, new Color(0.4f, 1f, 0.7f));

            // ═══════════════════════════════════
            //  LEFT COLUMN — 3D Showcase Stage
            // ═══════════════════════════════════
            var leftCol = CreatePanel(card.transform, "LeftCol", new Vector2(-260, -20), new Vector2(460, 510));
            leftCol.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.92f);

            var leftBorder = CreatePanel(leftCol.transform, "LeftBorder", Vector2.zero, new Vector2(464, 514));
            leftBorder.transform.SetAsFirstSibling();
            leftBorder.GetComponent<Image>().color = new Color(0.02f, 0.52f, 0.85f, 0.3f);

            MakeText(leftCol.transform, "PreviewLabel", "SAN XEM TRUOC 3D", 14,
                new Vector2(0, 230), new Vector2(420, 24), TextAlignmentOptions.Center, new Color(0.4f, 0.75f, 1f));

            CreatePreviewCharacter(leftCol.transform);

            MakeText(leftCol.transform, "NameLabel", "TEN NHAN VAT", 14,
                new Vector2(0, -145), new Vector2(420, 20), TextAlignmentOptions.Center, new Color(0.55f, 0.65f, 0.8f));

            var nameInputObj = CreateInputFieldV2(leftCol.transform, savedName, "Nhap ten nhan vat...",
                new Vector2(0, -180), new Vector2(340, 42));
            _nameInput = nameInputObj;
            
            _nameInput.onValueChanged.AddListener((val) => {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    var nameTag = player.transform.Find("NameTag");
                    if (nameTag != null)
                    {
                        var tmp = nameTag.GetComponentInChildren<TMPro.TextMeshPro>();
                        if (tmp != null) tmp.text = string.IsNullOrEmpty(val) ? "Player" : val;
                    }
                }
            });

            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string shortId = deviceId.Length > 10 ? deviceId.Substring(0, 10) + "..." : deviceId;
            MakeText(leftCol.transform, "DeviceId", $"ID THIET BI: {shortId}", 11,
                new Vector2(0, -225), new Vector2(420, 18), TextAlignmentOptions.Center, new Color(0.4f, 0.45f, 0.55f));

            // ═══════════════════════════════════
            //  RIGHT COLUMN — Customization Suite
            // ═══════════════════════════════════
            var rightCol = CreatePanel(card.transform, "RightCol", new Vector2(230, 55), new Vector2(460, 360));
            rightCol.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.11f, 0.8f);

            var rightBorder = CreatePanel(rightCol.transform, "RightBorder", Vector2.zero, new Vector2(464, 364));
            rightBorder.transform.SetAsFirstSibling();
            rightBorder.GetComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f, 0.4f);

            MakeText(rightCol.transform, "GenderLabel", "GIOI TINH", 13,
                new Vector2(0, 158), new Vector2(440, 18), TextAlignmentOptions.Left, new Color(0.4f, 0.75f, 1f));

            var genderRow = CreatePanel(rightCol.transform, "GenderRow", new Vector2(0, 126), new Vector2(440, 34), false);
            genderRow.GetComponent<Image>().color = Color.clear;
            _genderButtons = new Image[2];
            string[] genderLabels = { "NAM", "NU" };
            for (int i = 0; i < 2; i++)
            {
                int genderIdx = i;
                var gBtn = CreatePanel(genderRow.transform, $"GenderBtn_{i}",
                    new Vector2(-105 + i * 210, 0), new Vector2(190, 32), true);
                _genderButtons[i] = gBtn.GetComponent<Image>();
                var btn = gBtn.AddComponent<Button>();
                btn.onClick.AddListener(() => { _selectedGender = genderIdx; RefreshGenderUI(); });
                MakeText(gBtn.transform, "Label", genderLabels[i], 14,
                    Vector2.zero, new Vector2(180, 28), TextAlignmentOptions.Center, Color.white);
            }

            MakeText(rightCol.transform, "HairHeader", "TÙY CHỈNH TÓC", 13,
                new Vector2(0, 82), new Vector2(440, 18), TextAlignmentOptions.Left, new Color(0.4f, 0.75f, 1f));

            _tabHairContent = CreatePanel(rightCol.transform, "HairContent", new Vector2(0, -60), new Vector2(440, 260), false);
            _tabHairContent.GetComponent<Image>().color = Color.clear;
            BuildHairTab(_tabHairContent.transform);

            RefreshGenderUI();

            // ═══════════════════════════════════
            //  BOTTOM — Connection Action Bar
            // ═══════════════════════════════════
            var bottomBar = CreatePanel(card.transform, "BottomBar", new Vector2(230, -200), new Vector2(460, 110), false);
            bottomBar.GetComponent<Image>().color = Color.clear;

            var inputRow = CreatePanel(bottomBar.transform, "InputRow", new Vector2(0, 28), new Vector2(440, 36), false);
            inputRow.GetComponent<Image>().color = Color.clear;

            _addressInput = CreateInputFieldV2(inputRow.transform, "wool-delivery.gl.at.ply.gg", "",
                new Vector2(-60, 0), new Vector2(300, 34));

            _portInput = CreateInputFieldV2(inputRow.transform, "30645", "",
                new Vector2(162.5f, 0), new Vector2(95, 34));

            CreateGradientButton(bottomBar.transform, "HostBtn", "OFFLINE",
                new Color(0.14f, 0.15f, 0.22f), new Color(0.2f, 0.22f, 0.3f),
                new Vector2(-140, -24), new Vector2(135, 48), OnHostServerClicked, 13f);

            CreateGradientButton(bottomBar.transform, "JoinBtn", "VAO GAME",
                new Color(0.02f, 0.52f, 0.95f), new Color(0.0f, 0.82f, 1f),
                new Vector2(70, -24), new Vector2(275, 56), OnJoinServerClicked, 20f);
        }

        private void SavePlayerPrefs()
        {
            if (_nameInput != null)
            {
                string newName = _nameInput.text;
                PlayerPrefs.SetString("PlayerName", newName);
                
                // Cập nhật ngay lập tức tên hiển thị trên đầu nhân vật local trong scene
                var localPlayer = GameObject.FindWithTag("Player");
                if (localPlayer != null)
                {
                    var nameTag = localPlayer.transform.Find("NameTag");
                    if (nameTag != null)
                    {
                        var tmp = nameTag.GetComponentInChildren<TMPro.TextMeshPro>();
                        if (tmp != null) tmp.text = string.IsNullOrEmpty(newName) ? "Player" : newName;
                    }
                }
            }
            PlayerPrefs.SetInt("PlayerGender", _selectedGender);
            PlayerPrefs.SetInt("PlayerColorIndex", _selectedBodyColor);
            PlayerPrefs.SetInt("PlayerHairStyle", _selectedHairStyle);
            PlayerPrefs.SetInt("PlayerHairColor", _selectedHairColor);
            PlayerPrefs.SetInt("PlayerOutfitStyle", _selectedOutfitStyle);
            PlayerPrefs.SetInt("PlayerPantsStyle", _selectedPantsStyle);
            PlayerPrefs.SetInt("PlayerPantsColor", _selectedPantsColor);
            PlayerPrefs.Save();

            // Apply customization & starter items from inventory
            var scenePlayer = GameObject.FindWithTag("Player");
            if (scenePlayer != null)
            {
                var inv = scenePlayer.GetComponent<PlayerInventory>();
                if (inv == null) inv = scenePlayer.AddComponent<PlayerInventory>();
                inv.LoadInventory(_selectedGender);
            }
        }

        private void OnHostServerClicked()
        {
            StartCoroutine(HostServerAfterSaveCoroutine());
        }

        private IEnumerator HostServerAfterSaveCoroutine()
        {
            if (_isConnecting) yield break;
            _isConnecting = true;

            SavePlayerPrefs();

            // Wait for database sync to finish
            yield return StartCoroutine(SaveToDatabaseServerCoroutine());

            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
                setup.Stop();
                yield return null; // Wait 1 frame for cleanup
                setup.StartAsHost();
                CloseConnectionPanel();
            }
            _isConnecting = false;
        }

        private void OnJoinServerClicked()
        {
            StartCoroutine(JoinServerAfterSaveCoroutine());
        }

        private IEnumerator JoinServerAfterSaveCoroutine()
        {
            if (_isConnecting) yield break;
            _isConnecting = true;

            SavePlayerPrefs();

            // Wait for database sync to finish
            yield return StartCoroutine(SaveToDatabaseServerCoroutine());

            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
                setup.Stop();
                yield return null; // Wait 1 frame for cleanup

                string address = _addressInput != null ? _addressInput.text : "localhost";
                string portStr = _portInput != null ? _portInput.text : "7777";

                if (ushort.TryParse(portStr, out ushort port))
                {
                    var transport = Mirror.NetworkManager.singleton.transport as kcp2k.KcpTransport;
                    if (transport != null) transport.port = port;
                }

                setup.StartAsClient(address);
                CloseConnectionPanel();
            }
            _isConnecting = false;
        }

        private void CloseConnectionPanel()
        {
            if (_connectionPanel != null) _connectionPanel.SetActive(false);
            if (_previewCharacter != null) Destroy(_previewCharacter);
            if (_previewCamera != null)
            {
                _previewCamera.targetTexture = null;
                Destroy(_previewCamera.gameObject);
                _previewCamera = null;
            }
            if (_previewRT != null)
            {
                if (RenderTexture.active == _previewRT) RenderTexture.active = null;
                _previewRT.Release();
                Destroy(_previewRT);
                _previewRT = null;
            }
        }


}
}
