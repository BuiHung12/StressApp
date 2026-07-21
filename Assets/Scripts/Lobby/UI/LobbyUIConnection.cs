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
            panelImg.color = new Color(0.04f, 0.04f, 0.08f, 0.96f);

            // ── Main Card (glassmorphism) ──
            var card = CreatePanel(_connectionPanel.transform, "MainCard", Vector2.zero, new Vector2(1000, 650));
            var cardImg = card.GetComponent<Image>();
            cardImg.color = new Color(0.12f, 0.13f, 0.18f, 0.95f);

            var cardBorder = CreatePanel(card.transform, "CardBorder", Vector2.zero, new Vector2(1006, 656));
            cardBorder.transform.SetAsFirstSibling();
            cardBorder.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.85f, 0.25f);

            var titleBar = CreatePanel(card.transform, "TitleBar", new Vector2(0, 295), new Vector2(960, 48));
            titleBar.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.14f, 0.9f);
            MakeText(titleBar.transform, "TitleText", "RANGER CITY — TÙY CHỈNH NHÂN VẬT", 22,
                Vector2.zero, new Vector2(900, 40), TextAlignmentOptions.Center, new Color(0.5f, 0.85f, 1f));

            // ═══════════════════════════════════
            //  LEFT COLUMN — Character Preview
            // ═══════════════════════════════════
            var leftCol = CreatePanel(card.transform, "LeftCol", new Vector2(-250, -20), new Vector2(440, 510));
            leftCol.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.8f);

            MakeText(leftCol.transform, "PreviewLabel", "XEM TRƯỚC", 14,
                new Vector2(0, 230), new Vector2(400, 24), TextAlignmentOptions.Center, new Color(0.5f, 0.55f, 0.7f));

            CreatePreviewCharacter(leftCol.transform);

            MakeText(leftCol.transform, "NameLabel", "TÊN NHÂN VẬT", 14,
                new Vector2(0, -145), new Vector2(400, 20), TextAlignmentOptions.Center, new Color(0.5f, 0.55f, 0.7f));

            var nameInputObj = CreateInputFieldV2(leftCol.transform, savedName, "Nhập tên...",
                new Vector2(0, -180), new Vector2(320, 42));
            _nameInput = nameInputObj;
            
            // Cập nhật tên hiển thị trên đầu nhân vật thời gian thực khi đang gõ
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
            MakeText(leftCol.transform, "DeviceId", $"ID: {shortId}", 11,
                new Vector2(0, -225), new Vector2(400, 18), TextAlignmentOptions.Center, new Color(0.4f, 0.4f, 0.5f));

            // ═══════════════════════════════════
            //  RIGHT COLUMN — Customization Tabs
            // ═══════════════════════════════════
            var rightCol = CreatePanel(card.transform, "RightCol", new Vector2(220, 25), new Vector2(440, 420));
            rightCol.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.6f);

            MakeText(rightCol.transform, "GenderLabel", "GIỚI TÍNH", 13,
                new Vector2(0, 192), new Vector2(420, 18), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            var genderRow = CreatePanel(rightCol.transform, "GenderRow", new Vector2(0, 160), new Vector2(420, 36), false);
            genderRow.GetComponent<Image>().color = Color.clear;
            _genderButtons = new Image[2];
            string[] genderLabels = { "NAM", "NỮ" };
            for (int i = 0; i < 2; i++)
            {
                int genderIdx = i;
                var gBtn = CreatePanel(genderRow.transform, $"GenderBtn_{i}",
                    new Vector2(-100 + i * 200, 0), new Vector2(180, 32), true);
                _genderButtons[i] = gBtn.GetComponent<Image>();
                var btn = gBtn.AddComponent<Button>();
                btn.onClick.AddListener(() => { _selectedGender = genderIdx; RefreshGenderUI(); });
                MakeText(gBtn.transform, "Label", genderLabels[i], 14,
                    Vector2.zero, new Vector2(170, 28), TextAlignmentOptions.Center, Color.white);
            }

            var tabRow = CreatePanel(rightCol.transform, "TabRow", new Vector2(0, 105), new Vector2(420, 36));
            tabRow.GetComponent<Image>().color = Color.clear;
            _tabButtons = new Image[3];
            string[] tabLabels = { "TÓC", "ÁO", "QUẦN" };
            for (int i = 0; i < 3; i++)
            {
                int tabIdx = i;
                var tabBtn = CreatePanel(tabRow.transform, $"Tab_{i}",
                    new Vector2(-135 + i * 135, 0), new Vector2(130, 32), true);
                _tabButtons[i] = tabBtn.GetComponent<Image>();
                _tabButtons[i].color = i == 0 ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.18f, 0.18f, 0.24f, 0.8f);
                var btn = tabBtn.AddComponent<Button>();
                btn.onClick.AddListener(() => SwitchTab(tabIdx));
                MakeText(tabBtn.transform, "Label", tabLabels[i], 15,
                    Vector2.zero, new Vector2(120, 28), TextAlignmentOptions.Center, Color.white);
            }

            _tabHairContent = CreatePanel(rightCol.transform, "HairContent", new Vector2(0, -50), new Vector2(420, 270), false);
            _tabHairContent.GetComponent<Image>().color = Color.clear;
            BuildHairTab(_tabHairContent.transform);

            _tabOutfitContent = CreatePanel(rightCol.transform, "OutfitContent", new Vector2(0, -50), new Vector2(420, 270), false);
            _tabOutfitContent.GetComponent<Image>().color = Color.clear;
            BuildOutfitTab(_tabOutfitContent.transform);
            _tabOutfitContent.SetActive(false);

            _tabPantsContent = CreatePanel(rightCol.transform, "PantsContent", new Vector2(0, -50), new Vector2(420, 270), false);
            _tabPantsContent.GetComponent<Image>().color = Color.clear;
            BuildPantsTab(_tabPantsContent.transform);
            _tabPantsContent.SetActive(false);

            RefreshGenderUI();

            // ═══════════════════════════════════
            //  BOTTOM — Connection Buttons
            // ═══════════════════════════════════
            var bottomBar = CreatePanel(card.transform, "BottomBar", new Vector2(220, -235), new Vector2(440, 120), false);
            bottomBar.GetComponent<Image>().color = Color.clear;

            // Row 1: Inputs (at y = 35)
            var inputRow = CreatePanel(bottomBar.transform, "InputRow", new Vector2(0, 35), new Vector2(400, 42), false);
            inputRow.GetComponent<Image>().color = Color.clear;

            _addressInput = CreateInputFieldV2(inputRow.transform, "wool-delivery.gl.at.ply.gg", "",
                new Vector2(-50, 0), new Vector2(295, 36));

            _portInput = CreateInputFieldV2(inputRow.transform, "30645", "",
                new Vector2(152.5f, 0), new Vector2(95, 36));

            // Row 2: Buttons (at y = -25) - JOIN button is made massive and prominent, HOST is secondary
            CreateGradientButton(bottomBar.transform, "HostBtn", "HOST SERVER",
                new Color(0.18f, 0.19f, 0.24f), new Color(0.25f, 0.26f, 0.32f),
                new Vector2(-135, -25), new Vector2(130, 48), OnHostServerClicked, 13f);

            CreateGradientButton(bottomBar.transform, "JoinBtn", "JOIN",
                new Color(0.05f, 0.45f, 0.9f), new Color(0.0f, 0.75f, 1f),
                new Vector2(70, -25), new Vector2(260, 58), OnJoinServerClicked, 22f);
        }

        private void BuildHairTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU TÓC", 14,
                new Vector2(0, 115), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 70f;
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                int row = i / 3; int col = i % 3;
                float x = -130f + col * 130f;
                float yPos = y - row * 75f;

                var slot = CreateStyleSlot(parent, $"Hair_{i}", NetworkPlayer.MaleHairStyleNames[i], new Vector2(x, yPos), new Vector2(110, 66),
                    i == _selectedHairStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedHairStyle = idx; RefreshHairStyleHighlight(); UpdatePreview(); });
            }

            float cy = -75f;
            MakeText(parent, "HColorLabel", "MÀU TÓC", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.HairColorPalette, _selectedHairColor, cy - 20f,
                (idx) => { _selectedHairColor = idx; UpdatePreview(); }, "HairCol");
        }

        private void BuildOutfitTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU ÁO", 14,
                new Vector2(0, 115), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 70f;
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                int row = i / 3; int col = i % 3;
                float x = -130f + col * 130f;
                float yPos = y - row * 75f;

                var slot = CreateStyleSlot(parent, $"Outfit_{i}", NetworkPlayer.MaleOutfitStyleNames[i], new Vector2(x, yPos), new Vector2(110, 66),
                    i == _selectedOutfitStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedOutfitStyle = idx; RefreshOutfitStyleHighlight(); UpdatePreview(); });
            }

            float cy = -75f;
            MakeText(parent, "OColorLabel", "MÀU ÁO", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.BodyColorPalette, _selectedBodyColor, cy - 20f,
                (idx) => { _selectedBodyColor = idx; UpdatePreview(); }, "BodyCol");
        }

        private void BuildPantsTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU QUẦN", 14,
                new Vector2(0, 115), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 70f;
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                int col = i;
                float x = -145f + col * 97f;

                var slot = CreateStyleSlot(parent, $"Pants_{i}", NetworkPlayer.MalePantsStyleNames[i], new Vector2(x, y), new Vector2(85, 66),
                    i == _selectedPantsStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedPantsStyle = idx; RefreshPantsStyleHighlight(); UpdatePreview(); });
            }

            float cy = -75f;
            MakeText(parent, "PColorLabel", "MÀU QUẦN", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.PantsColorPalette, _selectedPantsColor, cy - 20f,
                (idx) => { _selectedPantsColor = idx; UpdatePreview(); }, "PantsCol");
        }

        private void BuildColorRow(Transform parent, Color[] palette, int selected, float yPos, System.Action<int> onSelect, string prefix)
        {
            float swatchSize = 34f;
            float spacing = 6f;
            float totalWidth = palette.Length * (swatchSize + spacing) - spacing;
            float startX = -totalWidth / 2f + swatchSize / 2f;

            for (int i = 0; i < palette.Length; i++)
            {
                int idx = i;
                var swatch = CreatePanel(parent, $"{prefix}_{i}", new Vector2(startX + i * (swatchSize + spacing), yPos), new Vector2(swatchSize, swatchSize));
                swatch.GetComponent<Image>().color = palette[i];
                var btn = swatch.AddComponent<Button>();
                btn.onClick.AddListener(() => onSelect(idx));

                if (i == selected)
                {
                    var ring = CreatePanel(swatch.transform, "Ring", Vector2.zero, new Vector2(swatchSize + 4, swatchSize + 4));
                    ring.transform.SetAsFirstSibling();
                    ring.GetComponent<Image>().color = Color.clear;
                    var outline = ring.AddComponent<Outline>();
                    outline.effectColor = Color.white;
                    outline.effectDistance = new Vector2(2, -2);
                }
            }
        }

        private GameObject CreateStyleSlot(Transform parent, string name, string label, Vector2 pos, Vector2 size, bool selected)
        {
            var slot = CreatePanel(parent, name, pos, size, true);
            var slotImg = slot.GetComponent<Image>();
            slotImg.color = selected ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
            slot.AddComponent<Button>();

            MakeText(slot.transform, "Label", label, 14, Vector2.zero, new Vector2(size.x - 10, size.y - 10), TextAlignmentOptions.Center, Color.white);
            return slot;
        }

        private void CreatePreviewCharacter(Transform parent)
        {
            _previewRT = new RenderTexture(512, 512, 16);
            _previewRT.Create();

            var rawObj = new GameObject("PreviewRawImage", typeof(RectTransform));
            rawObj.transform.SetParent(parent, false);
            var rawRt = rawObj.GetComponent<RectTransform>();
            rawRt.anchoredPosition = new Vector2(0, 30);
            rawRt.sizeDelta = new Vector2(300, 320);
            var rawImg = rawObj.AddComponent<RawImage>();
            rawImg.texture = _previewRT;
            rawImg.raycastTarget = false;

            _previewCharacter = new GameObject("PreviewChar");
            _previewCharacter.transform.position = new Vector3(200f, 0f, 200f);

            var camObj = new GameObject("PreviewCamera");
            camObj.transform.SetParent(_previewCharacter.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0.52f, 1.25f);
            camObj.transform.localRotation = Quaternion.Euler(6f, 180f, 0f);

            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRT;
            _previewCamera.depth = -5f;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.06f, 0.06f, 0.1f, 1f);
            _previewCamera.orthographic = true;
            _previewCamera.orthographicSize = 0.52f;

            var lightObj = new GameObject("PreviewLight");
            lightObj.transform.SetParent(camObj.transform, false);
            lightObj.transform.localPosition = new Vector3(0.5f, 0.5f, -0.5f);
            var previewLight = lightObj.AddComponent<Light>();
            previewLight.type = LightType.Point;
            previewLight.range = 5f;
            previewLight.intensity = 2f;
            previewLight.color = Color.white;

            var setup = FindAnyObjectByType<LobbySetup>();
            if (setup != null)
            {
                var charObj = CharacterVisuals.CreateCharacterTopDown("PreviewModel",
                    NetworkPlayer.BodyColorPalette[Mathf.Clamp(_selectedBodyColor, 0, NetworkPlayer.BodyColorPalette.Length - 1)],
                    new Color(1f, 0.88f, 0.7f));
                if (charObj != null)
                {
                    charObj.transform.SetParent(_previewCharacter.transform, false);
                    charObj.transform.localPosition = Vector3.zero;
                    charObj.transform.localRotation = Quaternion.identity;
                    charObj.transform.localScale = Vector3.one * 0.45f;

                    var anim = charObj.GetComponent<CharacterAnimator>();
                    if (anim != null) Destroy(anim);
                }
            }

            UpdatePreview();
            StartCoroutine(RotatePreview());
        }

        private IEnumerator RotatePreview()
        {
            while (_previewCharacter != null && _connectionPanel != null && _connectionPanel.activeSelf)
            {
                _previewRotation += 35f * Time.deltaTime;
                _previewCharacter.transform.rotation = Quaternion.Euler(0, _previewRotation, 0);
                yield return null;
            }
        }

        private void UpdatePreview()
        {
            if (_previewCharacter == null) return;

            var model = _previewCharacter.transform.Find("PreviewModel")?.gameObject;
            if (model == null) return;

            Color bodyColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(_selectedBodyColor, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
            Color hairColor = NetworkPlayer.HairColorPalette[Mathf.Clamp(_selectedHairColor, 0, NetworkPlayer.HairColorPalette.Length - 1)];
            Color pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(_selectedPantsColor, 0, NetworkPlayer.PantsColorPalette.Length - 1)];

            CharacterVisuals.ApplyCustomization(model, _selectedGender, _selectedHairStyle, hairColor, _selectedOutfitStyle, bodyColor, _selectedPantsStyle, pantsColor);
        }

        private void RefreshGenderUI()
        {
            PlayerPrefs.SetInt("PlayerGender", _selectedGender);
            PlayerPrefs.Save();

            for (int i = 0; i < 2; i++)
            {
                if (_genderButtons[i] != null)
                {
                    _genderButtons[i].color = i == _selectedGender
                        ? new Color(0.25f, 0.45f, 0.8f, 0.9f)
                        : new Color(0.18f, 0.18f, 0.24f, 0.8f);
                }
            }

            if (_tabHairContent != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    var slot = _tabHairContent.transform.Find($"Hair_{i}");
                    if (slot != null)
                    {
                        var tmp = slot.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                            tmp.text = _selectedGender == 0 ? NetworkPlayer.MaleHairStyleNames[i] : NetworkPlayer.FemaleHairStyleNames[i];
                    }
                }
            }

            if (_tabOutfitContent != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    var slot = _tabOutfitContent.transform.Find($"Outfit_{i}");
                    if (slot != null)
                    {
                        var tmp = slot.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                            tmp.text = _selectedGender == 0 ? NetworkPlayer.MaleOutfitStyleNames[i] : NetworkPlayer.FemaleOutfitStyleNames[i];
                    }
                }
            }

            if (_tabPantsContent != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    var slot = _tabPantsContent.transform.Find($"Pants_{i}");
                    if (slot != null)
                    {
                        var tmp = slot.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                            tmp.text = _selectedGender == 0 ? NetworkPlayer.MalePantsStyleNames[i] : NetworkPlayer.FemalePantsStyleNames[i];
                    }
                }
            }

            UpdatePreview();
        }

        private void SwitchTab(int tabIndex)
        {
            _activeTab = tabIndex;
            if (_tabHairContent != null) _tabHairContent.SetActive(tabIndex == 0);
            if (_tabOutfitContent != null) _tabOutfitContent.SetActive(tabIndex == 1);
            if (_tabPantsContent != null) _tabPantsContent.SetActive(tabIndex == 2);

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                if (_tabButtons[i] != null)
                    _tabButtons[i].color = i == tabIndex
                        ? new Color(0.25f, 0.45f, 0.8f, 0.9f)
                        : new Color(0.18f, 0.18f, 0.24f, 0.8f);
            }
        }

        private void RefreshHairStyleHighlight()
        {
            if (_tabHairContent == null) return;
            for (int i = 0; i < 6; i++)
            {
                var slot = _tabHairContent.transform.Find($"Hair_{i}");
                if (slot != null)
                {
                    var img = slot.GetComponent<Image>();
                    if (img != null)
                        img.color = i == _selectedHairStyle
                            ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
                }
            }
        }

        private void RefreshOutfitStyleHighlight()
        {
            if (_tabOutfitContent == null) return;
            for (int i = 0; i < 5; i++)
            {
                var slot = _tabOutfitContent.transform.Find($"Outfit_{i}");
                if (slot != null)
                {
                    var img = slot.GetComponent<Image>();
                    if (img != null)
                        img.color = i == _selectedOutfitStyle
                            ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
                }
            }
        }

        private void RefreshPantsStyleHighlight()
        {
            if (_tabPantsContent == null) return;
            for (int i = 0; i < 4; i++)
            {
                var slot = _tabPantsContent.transform.Find($"Pants_{i}");
                if (slot != null)
                {
                    var img = slot.GetComponent<Image>();
                    if (img != null)
                        img.color = i == _selectedPantsStyle
                            ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
                }
            }
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

            // Apply customization directly to local scene player
            var scenePlayer = GameObject.FindWithTag("Player");
            if (scenePlayer != null)
            {
                Color bodyColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(_selectedBodyColor, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
                Color hairColor = NetworkPlayer.HairColorPalette[Mathf.Clamp(_selectedHairColor, 0, NetworkPlayer.HairColorPalette.Length - 1)];
                Color pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(_selectedPantsColor, 0, NetworkPlayer.PantsColorPalette.Length - 1)];
                CharacterVisuals.ApplyCustomization(scenePlayer, _selectedGender, _selectedHairStyle, hairColor, _selectedOutfitStyle, bodyColor, _selectedPantsStyle, pantsColor);
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
            if (_previewCamera != null) Destroy(_previewCamera.gameObject);
            if (_previewRT != null)
            {
                _previewRT.Release();
                Destroy(_previewRT);
            }
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            return CreatePanel(parent, name, pos, size, false);
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size, bool raycastTarget)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.raycastTarget = raycastTarget;
            return obj;
        }

        private GameObject CreateRoundedPanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color, bool raycastTarget = false)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = raycastTarget;
            return obj;
        }

        private GameObject MakeText(Transform parent, string name, string text, float fontSize, Vector2 pos, Vector2 size, TextAlignmentOptions align, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return obj;
        }

        private TMP_InputField CreateInputFieldV2(Transform parent, string defaultText, string placeholder, Vector2 pos, Vector2 size)
        {
            var obj = new GameObject("Input", typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            img.type = Image.Type.Sliced;
            img.color = new Color(0.06f, 0.06f, 0.1f, 1f);
            img.raycastTarget = true;

            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(obj.transform, false);
            var trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 2); trt.offsetMax = new Vector2(-10, -2);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.enableWordWrapping = false;
            txt.raycastTarget = false;

            if (!string.IsNullOrEmpty(placeholder))
            {
                var phObj = new GameObject("Placeholder", typeof(RectTransform));
                phObj.transform.SetParent(obj.transform, false);
                var phRt = phObj.GetComponent<RectTransform>();
                phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
                phRt.offsetMin = new Vector2(10, 2); phRt.offsetMax = new Vector2(-10, -2);
                var phTxt = phObj.AddComponent<TextMeshProUGUI>();
                phTxt.text = placeholder;
                phTxt.fontSize = 14;
                phTxt.fontStyle = FontStyles.Italic;
                phTxt.color = new Color(0.4f, 0.4f, 0.5f);
                phTxt.raycastTarget = false;

                var input = obj.AddComponent<TMP_InputField>();
                input.textComponent = txt;
                input.placeholder = phTxt;
                input.text = defaultText;
                input.characterLimit = 20;
                return input;
            }
            else
            {
                var input = obj.AddComponent<TMP_InputField>();
                input.textComponent = txt;
                input.text = defaultText;
                input.characterLimit = 40;
                return input;
            }
        }

        private void CreateGradientButton(Transform parent, string name, string label, Color colorA, Color colorB, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action, float fontSize = 15f)
        {
            var btnObj = CreateRoundedPanel(parent, name, pos, size, Color.Lerp(colorA, colorB, 0.5f), true);
            var img = btnObj.GetComponent<Image>();
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            var colors = btn.colors;
            colors.highlightedColor = colorB;
            colors.pressedColor = colorA * 0.8f;
            colors.normalColor = Color.white;
            btn.colors = colors;

            MakeText(btnObj.transform, "Label", label, fontSize, Vector2.zero, size, TextAlignmentOptions.Center, Color.white);
        }

        private IEnumerator LoadFromDatabaseServerCoroutine()
        {
            if (_isSyncingWithServer) yield break;
            _isSyncingWithServer = true;

            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string url = $"{_apiBaseUrl}?deviceId={UnityWebRequest.EscapeURL(deviceId)}";

            Debug.Log($"[LobbyUIConnection] Querying database server for device: {deviceId}...");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 3; // 3 seconds timeout
                yield return webRequest.SendWebRequest();

                _isSyncingWithServer = false;

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string json = webRequest.downloadHandler.text;
                    Debug.Log($"[LobbyUIConnection] Database loaded: {json}");
                    try
                    {
                        ServerPlayerData serverData = JsonUtility.FromJson<ServerPlayerData>(json);
                        if (serverData != null)
                        {
                            _selectedGender = serverData.gender;
                            _selectedBodyColor = serverData.bodyColorIndex;
                            _selectedHairStyle = serverData.hairStyle;
                            _selectedHairColor = serverData.hairColorIndex;
                            _selectedOutfitStyle = serverData.outfitStyle;
                            _selectedPantsStyle = serverData.pantsStyle;
                            _selectedPantsColor = serverData.pantsColorIndex;

                            // Cache to PlayerPrefs
                            PlayerPrefs.SetString("PlayerName", serverData.name);
                            PlayerPrefs.SetInt("PlayerGender", _selectedGender);
                            PlayerPrefs.SetInt("PlayerColorIndex", _selectedBodyColor);
                            PlayerPrefs.SetInt("PlayerHairStyle", _selectedHairStyle);
                            PlayerPrefs.SetInt("PlayerHairColor", _selectedHairColor);
                            PlayerPrefs.SetInt("PlayerOutfitStyle", _selectedOutfitStyle);
                            PlayerPrefs.SetInt("PlayerPantsStyle", _selectedPantsStyle);
                            PlayerPrefs.SetInt("PlayerPantsColor", _selectedPantsColor);
                            PlayerPrefs.Save();

                            Debug.Log($"[LobbyUIConnection] Rebuilding UI with loaded data for player: {serverData.name}");
                            RebuildConnectionUI();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[LobbyUIConnection] Failed to parse player JSON: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[LobbyUIConnection] Database server unreachable or player not found. Using local PlayerPrefs fallback. Error: {webRequest.error}");
                }
            }
        }

        private IEnumerator SaveToDatabaseServerCoroutine()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string playerName = _nameInput != null ? _nameInput.text : "Ranger";

            ServerPlayerData data = new ServerPlayerData();
            data.deviceId = deviceId;
            data.name = playerName;
            data.gender = _selectedGender;
            data.bodyColorIndex = _selectedBodyColor;
            data.hairStyle = _selectedHairStyle;
            data.hairColorIndex = _selectedHairColor;
            data.outfitStyle = _selectedOutfitStyle;
            data.pantsStyle = _selectedPantsStyle;
            data.pantsColorIndex = _selectedPantsColor;

            string json = JsonUtility.ToJson(data);

            using (UnityWebRequest webRequest = new UnityWebRequest(_apiBaseUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = 3;

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[LobbyUIConnection] Successfully saved character configuration for device: {deviceId} to database server.");
                }
                else
                {
                    Debug.LogWarning($"[LobbyUIConnection] Failed to save character to database server (using local fallback). Error: {webRequest.error}");
                }
            }
        }

        private void RebuildConnectionUI()
        {
            if (_connectionPanel == null) return;

            bool wasActive = _connectionPanel.activeSelf;

            if (_previewCharacter != null) Destroy(_previewCharacter);
            if (_previewCamera != null) Destroy(_previewCamera.gameObject);
            if (_previewRT != null)
            {
                _previewRT.Release();
                Destroy(_previewRT);
            }
            if (_connectionPanel != null) Destroy(_connectionPanel);
            if (_noInternetOverlay != null) Destroy(_noInternetOverlay);
            _noInternetOverlay = null;

            CreateConnectionUI();

            if (_connectionPanel != null)
            {
                _connectionPanel.SetActive(wasActive);
            }
        }

        private void LoadDbConfig()
        {
            // Always write fresh config to ensure phone doesn't use stale cached values
            string path = Path.Combine(Application.persistentDataPath, "db_config.json");
            try
            {
                DbConfig config = new DbConfig();
                string json = JsonUtility.ToJson(config, true);
                File.WriteAllText(path, json);
                _apiBaseUrl = $"http://{config.apiHost}:{config.apiPort}/api/player";
                Debug.Log($"[LobbyUIConnection] Loaded DB Config: {_apiBaseUrl}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LobbyUIConnection] Failed to write db_config.json: {e.Message}");
            }
        }
    }

    [System.Serializable]
    public class ServerPlayerData
    {
        public string deviceId;
        public string name;
        public int gender;
        public int bodyColorIndex;
        public int hairStyle;
        public int hairColorIndex;
        public int outfitStyle;
        public int pantsStyle;
        public int pantsColorIndex;
    }

    [System.Serializable]
    public class DbConfig
    {
        public string apiHost = "bore.pub";
        public int apiPort = 57223;
    }
}
