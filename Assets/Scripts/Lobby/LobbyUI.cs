using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// UI quản lý tương tác trong sảnh chờ:
    /// - Nút Punch khi vào phạm vi (👊 trên đầu nhân vật)
    /// - Dialogue box với typing effect
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("Interaction Prompt")]
        [SerializeField] private GameObject _interactionPanel;
        [SerializeField] private Button _talkButton;
        [SerializeField] private Button _punchButton;
        [SerializeField] private TextMeshProUGUI _targetNameText;

        [Header("Dialogue Box")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _dialogueNameText;
        [SerializeField] private TextMeshProUGUI _dialogueContentText;
        [SerializeField] private Image _dialogueAvatarImage;
        [SerializeField] private TextMeshProUGUI _dialogueHintText;
        [SerializeField] private Button _dialogueNextButton;

        [Header("Punch Effect")]
        [SerializeField] private GameObject _punchEffectPrefab;
        [SerializeField] private Canvas _worldCanvas;

        [Header("Settings")]
        [SerializeField] private float _typingSpeed = 40f;

        private PlayerController _player;
        private Camera _mainCamera;
        private IInteractable _currentTarget;
        private bool _dialogueActive;
        private string[] _currentDialogueLines;
        private int _currentLineIndex;
        private Coroutine _typingCoroutine;
        private bool _isTypingComplete;

        // Jail UI (created dynamically)
        private GameObject _jailPanel;
        private TextMeshProUGUI _jailText;

        // Coins UI (created dynamically)
        private TextMeshProUGUI _coinText;

        // Emoji UI (created dynamically)
        private GameObject _emojiToggleButton;
        private GameObject _emojiPanel;
        private bool _emojiPanelOpen;

        // Connection UI (created dynamically)
        private GameObject _connectionPanel;
        private TMP_InputField _addressInput;
        private TMP_InputField _portInput;
        private TMP_InputField _nameInput;



        private void Start()
        {
            _player = FindAnyObjectByType<PlayerController>();
            _mainCamera = Camera.main;

            Debug.Log($"[LobbyUI] Start: player={_player != null}, cam={_mainCamera != null}, " +
                      $"interactionPanel={_interactionPanel != null}, punchBtn={_punchButton != null}, " +
                      $"talkBtn={_talkButton != null}, targetName={_targetNameText != null}, " +
                      $"dialoguePanel={_dialoguePanel != null}, dialogueNext={_dialogueNextButton != null}");

            // Hide UI initially (with null checks)
            if (_interactionPanel != null) _interactionPanel.SetActive(false);
            if (_dialoguePanel != null) _dialoguePanel.SetActive(false);

            // Wire events
            if (_player != null)
            {
                _player.OnNearInteractable += ShowInteractionPrompt;
                _player.OnLeaveInteractable += HideInteractionPrompt;
                _player.OnJailStart += ShowJailNotification;
                _player.OnJailEnd += HideJailNotification;
                _player.OnCoinsChanged += UpdateCoinUI;

                // Proactively check if player is already near someone
                var currentNear = _player.GetNearestInteractable();
                if (currentNear != null)
                {
                    ShowInteractionPrompt(currentNear);
                }

                // Create dynamic Coin UI in screen corner
                CreateCoinUI();

                // Create emoji picker UI
                CreateEmojiUI();
            }
            else
            {
                Debug.LogError("[LobbyUI] Player not found!");
            }

            if (_talkButton != null) _talkButton.onClick.AddListener(OnTalkClicked);
            if (_punchButton != null) _punchButton.onClick.AddListener(OnPunchClicked);
            if (_dialogueNextButton != null) _dialogueNextButton.onClick.AddListener(OnDialogueAdvance);

            // Create dynamic connection UI panel if not headless
            if (!NetworkSetup.IsHeadlessServer())
            {
                CreateConnectionUI();
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnNearInteractable -= ShowInteractionPrompt;
                _player.OnLeaveInteractable -= HideInteractionPrompt;
                _player.OnJailStart -= ShowJailNotification;
                _player.OnJailEnd -= HideJailNotification;
                _player.OnCoinsChanged -= UpdateCoinUI;
            }
        }

        private void Update()
        {
            // Update interaction prompt position (follow target in world space)
            if (_currentTarget != null && _interactionPanel != null && _interactionPanel.activeSelf)
            {
                UpdatePromptPosition();
            }

            // Advance dialogue with click/tap
            if (_dialogueActive && Input.GetMouseButtonDown(0))
            {
                var es = UnityEngine.EventSystems.EventSystem.current;
                if (es != null && !es.IsPointerOverGameObject())
                {
                    OnDialogueAdvance();
                }
            }
        }

        // ── Interaction Prompt ──────────────────────────────────

        private void ShowInteractionPrompt(IInteractable target)
        {
            if (_dialogueActive) return;
            if (_interactionPanel == null) return;

            _currentTarget = target;
            _interactionPanel.SetActive(true);

            if (_targetNameText != null) _targetNameText.text = target.DisplayName;
            if (_talkButton != null) _talkButton.gameObject.SetActive(target.CanTalk);
            if (_punchButton != null) _punchButton.gameObject.SetActive(target.CanBePunched);

            Debug.Log($"[LobbyUI] Show prompt for: {target.DisplayName}");
            UpdatePromptPosition();
        }

        private void HideInteractionPrompt()
        {
            _currentTarget = null;
            if (_interactionPanel != null) _interactionPanel.SetActive(false);
        }

        private void UpdatePromptPosition()
        {
            if (_currentTarget == null || _mainCamera == null || _interactionPanel == null) return;

            MonoBehaviour targetMB = _currentTarget as MonoBehaviour;
            if (targetMB == null) return;

            // Scale the height offset dynamically according to target character scale.
            // A local height of 2.4f is positioned cleanly above the head and the name tag.
            float heightOffset = 2.4f * targetMB.transform.localScale.y;
            Vector3 worldPos = targetMB.transform.position + Vector3.up * heightOffset;
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                _interactionPanel.transform.position = screenPos;
            }
        }

        // ── Talk ──────────────────────────────────

        private void OnTalkClicked()
        {
            if (_currentTarget == null) return;

            _currentDialogueLines = _currentTarget.GetDialogueLines();
            if (_currentDialogueLines == null || _currentDialogueLines.Length == 0) return;

            _currentLineIndex = 0;
            _dialogueActive = true;

            if (_dialogueNameText != null) _dialogueNameText.text = _currentTarget.DisplayName;
            if (_dialoguePanel != null) _dialoguePanel.SetActive(true);
            if (_interactionPanel != null) _interactionPanel.SetActive(false);

            ShowDialogueLine(_currentDialogueLines[0]);
        }

        private void ShowDialogueLine(string text)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _isTypingComplete = false;
            if (_dialogueHintText != null) _dialogueHintText.text = "";
            _typingCoroutine = StartCoroutine(TypeDialogue(text));
        }

        private IEnumerator TypeDialogue(string text)
        {
            if (_dialogueContentText != null) _dialogueContentText.text = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (_dialogueContentText != null) _dialogueContentText.text += text[i];
                yield return new WaitForSeconds(1f / _typingSpeed);
            }

            _isTypingComplete = true;
            if (_dialogueHintText != null) _dialogueHintText.text = "Nhấn để tiếp tục...";
        }

        private void OnDialogueAdvance()
        {
            if (!_dialogueActive) return;

            if (!_isTypingComplete)
            {
                // Skip typing - show full text
                if (_typingCoroutine != null)
                    StopCoroutine(_typingCoroutine);
                if (_dialogueContentText != null)
                    _dialogueContentText.text = _currentDialogueLines[_currentLineIndex];
                _isTypingComplete = true;
                if (_dialogueHintText != null) _dialogueHintText.text = "Nhấn để tiếp tục...";
                return;
            }

            _currentLineIndex++;
            if (_currentLineIndex < _currentDialogueLines.Length)
            {
                ShowDialogueLine(_currentDialogueLines[_currentLineIndex]);
            }
            else
            {
                CloseDialogue();
            }
        }

        private void CloseDialogue()
        {
            _dialogueActive = false;
            if (_dialoguePanel != null) _dialoguePanel.SetActive(false);

            // Re-check if still near interactable
            if (_player != null)
            {
                var nearest = _player.GetNearestInteractable();
                if (nearest != null)
                {
                    ShowInteractionPrompt(nearest);
                }
            }
        }

        // ── Punch ──────────────────────────────────

        private void OnPunchClicked()
        {
            if (_player == null) return;
            _player.ExecutePunch();
            HideInteractionPrompt();
        }

        /// <summary>
        /// Spawn punch effect tại vị trí world (gọi từ event).
        /// </summary>
        public void SpawnPunchEffect(Vector3 worldPosition)
        {
            if (_punchEffectPrefab == null || _worldCanvas == null) return;

            var effect = Instantiate(_punchEffectPrefab, _worldCanvas.transform);
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPosition + Vector3.up * 1.5f);
            effect.transform.position = screenPos;
            Destroy(effect, 1f);
        }

        // ── Jail Notification ────────────────────────────────

        private void ShowJailNotification(float duration)
        {
            // Create jail panel dynamically if it doesn't exist
            if (_jailPanel == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
                if (canvas == null) return;

                _jailPanel = new GameObject("JailPanel");
                _jailPanel.transform.SetParent(canvas.transform, false);

                var rt = _jailPanel.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(30f, -30f);
                rt.sizeDelta = new Vector2(90f, 90f);

                var bg = _jailPanel.AddComponent<Image>();
                bg.color = new Color(0.1f, 0.1f, 0.1f, 0.65f); // Soft dark transparent backing

                // Jail countdown text
                var textObj = new GameObject("JailText");
                textObj.transform.SetParent(_jailPanel.transform, false);
                var trt = textObj.AddComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = trt.offsetMax = Vector2.zero;

                _jailText = textObj.AddComponent<TextMeshProUGUI>();
                _jailText.fontSize = 42;
                _jailText.alignment = TextAlignmentOptions.Center;
                _jailText.color = new Color(0.9f, 0.2f, 0.2f); // Red countdown
                _jailText.fontStyle = FontStyles.Bold;
            }

            _jailText.text = $"{duration:0}";
            _jailPanel.SetActive(true);
            HideInteractionPrompt();

            // Start countdown coroutine
            StartCoroutine(JailCountdownCoroutine(duration));
        }

        private System.Collections.IEnumerator JailCountdownCoroutine(float duration)
        {
            float remaining = duration;
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                if (_jailText != null)
                    _jailText.text = $"{Mathf.Ceil(remaining):0}";
                yield return null;
            }
        }

        // ── Coins UI ─────────────────────────────────────────

        private void CreateCoinUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var coinObj = new GameObject("CoinUI");
            coinObj.transform.SetParent(canvas.transform, false);

            var rt = coinObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.8f, 0.9f);
            rt.anchorMax = new Vector2(0.98f, 0.98f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            _coinText = coinObj.AddComponent<TextMeshProUGUI>();
            _coinText.fontSize = 20;
            _coinText.alignment = TextAlignmentOptions.Right;
            _coinText.color = new Color(1f, 0.84f, 0f); // Gold color
            
            // Add a clean dropshadow effect
            var shadow = coinObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);

            UpdateCoinUI(_player.RangerCoins);
        }

        private void UpdateCoinUI(int coins)
        {
            if (_coinText != null)
            {
                _coinText.text = $"$ Coins: {coins}";
            }
        }

        private void HideJailNotification()
        {
            if (_jailPanel != null)
                _jailPanel.SetActive(false);
        }

        // ── Emoji Picker UI ──

        private void CreateEmojiUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Toggle button (bottom-left corner)
            _emojiToggleButton = new GameObject("EmojiToggleBtn");
            _emojiToggleButton.transform.SetParent(canvas.transform, false);

            var toggleRt = _emojiToggleButton.AddComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0.02f, 0.02f);
            toggleRt.anchorMax = new Vector2(0.08f, 0.08f);
            toggleRt.offsetMin = toggleRt.offsetMax = Vector2.zero;

            var toggleImg = _emojiToggleButton.AddComponent<Image>();
            toggleImg.color = new Color(0.2f, 0.2f, 0.3f, 0.85f);

            var toggleBtn = _emojiToggleButton.AddComponent<Button>();
            toggleBtn.onClick.AddListener(ToggleEmojiPanel);

            var toggleTextObj = new GameObject("Text");
            toggleTextObj.transform.SetParent(_emojiToggleButton.transform, false);
            var toggleTextRt = toggleTextObj.AddComponent<RectTransform>();
            toggleTextRt.anchorMin = Vector2.zero;
            toggleTextRt.anchorMax = Vector2.one;
            toggleTextRt.offsetMin = toggleTextRt.offsetMax = Vector2.zero;
            var toggleTxt = toggleTextObj.AddComponent<TextMeshProUGUI>();
            toggleTxt.text = "😀";
            toggleTxt.fontSize = 28;
            toggleTxt.alignment = TextAlignmentOptions.Center;

            // Emoji panel (grid of emoji buttons)
            _emojiPanel = new GameObject("EmojiPanel");
            _emojiPanel.transform.SetParent(canvas.transform, false);

            var panelRt = _emojiPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.02f, 0.1f);
            panelRt.anchorMax = new Vector2(0.28f, 0.35f);
            panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

            var panelImg = _emojiPanel.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.22f, 0.92f);

            // Grid layout
            var grid = _emojiPanel.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize = new Vector2(50, 50);
            grid.spacing = new Vector2(6, 6);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;

            // Create emoji buttons
            for (int i = 0; i < EmojiSystem.AvailableEmojis.Length; i++)
            {
                int index = i; // Capture for closure
                var emojiBtn = new GameObject($"Emoji_{i}");
                emojiBtn.transform.SetParent(_emojiPanel.transform, false);

                var btnImg = emojiBtn.AddComponent<Image>();
                btnImg.color = new Color(0.25f, 0.25f, 0.35f, 0.9f);

                var btn = emojiBtn.AddComponent<Button>();
                btn.onClick.AddListener(() => OnEmojiClicked(index));

                // Hover effect
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.4f, 0.4f, 0.6f, 1f);
                colors.pressedColor = new Color(0.5f, 0.5f, 0.8f, 1f);
                btn.colors = colors;

                var emojiTextObj = new GameObject("Text");
                emojiTextObj.transform.SetParent(emojiBtn.transform, false);
                var emojiTextRt = emojiTextObj.AddComponent<RectTransform>();
                emojiTextRt.anchorMin = Vector2.zero;
                emojiTextRt.anchorMax = Vector2.one;
                emojiTextRt.offsetMin = emojiTextRt.offsetMax = Vector2.zero;
                var emojiTxt = emojiTextObj.AddComponent<TextMeshProUGUI>();
                emojiTxt.text = EmojiSystem.AvailableEmojis[i];
                emojiTxt.fontSize = 28;
                emojiTxt.alignment = TextAlignmentOptions.Center;
            }

            _emojiPanel.SetActive(false); // Ẩn ban đầu
        }

        private void ToggleEmojiPanel()
        {
            _emojiPanelOpen = !_emojiPanelOpen;
            if (_emojiPanel != null)
                _emojiPanel.SetActive(_emojiPanelOpen);
        }

        private void OnEmojiClicked(int index)
        {
            // Tìm EmojiSystem trên player local
            if (_player != null)
            {
                var emojiSystem = _player.GetComponent<EmojiSystem>();
                if (emojiSystem != null)
                {
                    emojiSystem.ShowEmoji(index);
                }
                else
                {
                    Debug.LogWarning("[LobbyUI] EmojiSystem not found on player!");
                }
            }

            // Đóng panel sau khi chọn
            _emojiPanelOpen = false;
            if (_emojiPanel != null)
                _emojiPanel.SetActive(false);
        }

        // ══════════════════════════════════════════════════════
        //  CONNECTION & CHARACTER CUSTOMIZATION PANEL (Premium)
        // ══════════════════════════════════════════════════════

        // Customization state
        private int _selectedHairStyle = 0;
        private int _selectedHairColor = 0;
        private int _selectedOutfitStyle = 0;
        private int _selectedBodyColor = 0;
        private int _selectedPantsStyle = 0;
        private int _selectedPantsColor = 0;

        // Preview
        private GameObject _previewCharacter;
        private float _previewRotation = 0f;

        // Tab UI
        private int _activeTab = 0; // 0=hair, 1=outfit, 2=pants
        private GameObject _tabHairContent;
        private GameObject _tabOutfitContent;
        private GameObject _tabPantsContent;
        private Image[] _tabButtons;

        // Swatch highlight refs
        private GameObject _hairStyleIndicator;
        private GameObject _hairColorIndicator;
        private GameObject _outfitStyleIndicator;
        private GameObject _bodyColorIndicator;
        private GameObject _pantsStyleIndicator;
        private GameObject _pantsColorIndicator;

        private void CreateConnectionUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Load saved prefs
            _selectedBodyColor = PlayerPrefs.GetInt("PlayerColorIndex", 0);
            _selectedHairStyle = PlayerPrefs.GetInt("PlayerHairStyle", 0);
            _selectedHairColor = PlayerPrefs.GetInt("PlayerHairColor", 0);
            _selectedOutfitStyle = PlayerPrefs.GetInt("PlayerOutfitStyle", 0);
            _selectedPantsStyle = PlayerPrefs.GetInt("PlayerPantsStyle", 0);
            _selectedPantsColor = PlayerPrefs.GetInt("PlayerPantsColor", 0);
            string savedName = PlayerPrefs.GetString("PlayerName", "");

            // ── Full-screen overlay ──
            _connectionPanel = new GameObject("ConnectionPanel");
            _connectionPanel.transform.SetParent(canvas.transform, false);
            var panelRt = _connectionPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;
            var panelImg = _connectionPanel.AddComponent<Image>();
            panelImg.color = new Color(0.04f, 0.04f, 0.08f, 0.96f);

            // ── Main Card (glassmorphism) ──
            var card = CreatePanel(_connectionPanel.transform, "MainCard", Vector2.zero, new Vector2(1000, 650));
            var cardImg = card.GetComponent<Image>();
            cardImg.color = new Color(0.12f, 0.13f, 0.18f, 0.95f);

            // Subtle card border (outer glow simulation)
            var cardBorder = CreatePanel(card.transform, "CardBorder", Vector2.zero, new Vector2(1006, 656));
            cardBorder.transform.SetAsFirstSibling();
            cardBorder.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.85f, 0.25f);

            // ── Title bar ──
            var titleBar = CreatePanel(card.transform, "TitleBar", new Vector2(0, 295), new Vector2(960, 48));
            titleBar.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.14f, 0.9f);
            var titleTxt = MakeText(titleBar.transform, "TitleText", "✨ RANGER CITY — TÙY CHỈNH NHÂN VẬT", 22,
                Vector2.zero, new Vector2(900, 40), TextAlignmentOptions.Center, new Color(0.5f, 0.85f, 1f));

            // ═══════════════════════════════════
            //  LEFT COLUMN — Character Preview
            // ═══════════════════════════════════

            var leftCol = CreatePanel(card.transform, "LeftCol", new Vector2(-250, -20), new Vector2(440, 510));
            leftCol.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.8f);

            MakeText(leftCol.transform, "PreviewLabel", "XEM TRƯỚC", 14,
                new Vector2(0, 230), new Vector2(400, 24), TextAlignmentOptions.Center, new Color(0.5f, 0.55f, 0.7f));

            // Create preview character (3D object rendered by scene camera)
            CreatePreviewCharacter(leftCol.transform);

            // Name input under preview
            MakeText(leftCol.transform, "NameLabel", "TÊN NHÂN VẬT", 14,
                new Vector2(0, -145), new Vector2(400, 20), TextAlignmentOptions.Center, new Color(0.5f, 0.55f, 0.7f));

            var nameInputObj = CreateInputFieldV2(leftCol.transform, savedName, "Nhập tên...",
                new Vector2(0, -180), new Vector2(320, 42));
            _nameInput = nameInputObj;

            // Device ID
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string shortId = deviceId.Length > 10 ? deviceId.Substring(0, 10) + "..." : deviceId;
            MakeText(leftCol.transform, "DeviceId", $"ID: {shortId}", 11,
                new Vector2(0, -225), new Vector2(400, 18), TextAlignmentOptions.Center, new Color(0.4f, 0.4f, 0.5f));

            // ═══════════════════════════════════
            //  RIGHT COLUMN — Customization Tabs
            // ═══════════════════════════════════

            var rightCol = CreatePanel(card.transform, "RightCol", new Vector2(220, 25), new Vector2(440, 420));
            rightCol.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.6f);

            // Tab buttons row
            var tabRow = CreatePanel(rightCol.transform, "TabRow", new Vector2(0, 192), new Vector2(420, 42));
            tabRow.GetComponent<Image>().color = Color.clear;
            _tabButtons = new Image[3];
            string[] tabLabels = { "TÓC", "ÁO", "QUẦN" };
            for (int i = 0; i < 3; i++)
            {
                int tabIdx = i;
                var tabBtn = CreatePanel(tabRow.transform, $"Tab_{i}",
                    new Vector2(-135 + i * 135, 0), new Vector2(130, 36), true); // RaycastTarget = true
                _tabButtons[i] = tabBtn.GetComponent<Image>();
                _tabButtons[i].color = i == 0 ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.18f, 0.18f, 0.24f, 0.8f);
                var btn = tabBtn.AddComponent<Button>();
                btn.onClick.AddListener(() => SwitchTab(tabIdx));
                MakeText(tabBtn.transform, "Label", tabLabels[i], 16,
                    Vector2.zero, new Vector2(120, 32), TextAlignmentOptions.Center, Color.white);
            }

            // Tab contents (Container panels don't intercept raycasts)
            _tabHairContent = CreatePanel(rightCol.transform, "HairContent", new Vector2(0, 10), new Vector2(420, 320), false);
            _tabHairContent.GetComponent<Image>().color = Color.clear;
            BuildHairTab(_tabHairContent.transform);

            _tabOutfitContent = CreatePanel(rightCol.transform, "OutfitContent", new Vector2(0, 10), new Vector2(420, 320), false);
            _tabOutfitContent.GetComponent<Image>().color = Color.clear;
            BuildOutfitTab(_tabOutfitContent.transform);
            _tabOutfitContent.SetActive(false);

            _tabPantsContent = CreatePanel(rightCol.transform, "PantsContent", new Vector2(0, 10), new Vector2(420, 320), false);
            _tabPantsContent.GetComponent<Image>().color = Color.clear;
            BuildPantsTab(_tabPantsContent.transform);
            _tabPantsContent.SetActive(false);

            // ═══════════════════════════════════
            //  BOTTOM — Connection Buttons
            // ═══════════════════════════════════

            var bottomBar = CreatePanel(card.transform, "BottomBar", new Vector2(220, -235), new Vector2(440, 100), false);
            bottomBar.GetComponent<Image>().color = Color.clear;

            // Host button
            CreateGradientButton(bottomBar.transform, "HostBtn", "▶ HOST SERVER",
                new Color(0.15f, 0.55f, 0.3f), new Color(0.2f, 0.7f, 0.4f),
                new Vector2(0, 25), new Vector2(400, 46), OnHostServerClicked);

            // IP + Port + Join row
            var joinRow = CreatePanel(bottomBar.transform, "JoinRow", new Vector2(0, -25), new Vector2(400, 42), false);
            joinRow.GetComponent<Image>().color = Color.clear;

            _addressInput = CreateInputFieldV2(joinRow.transform, "wool-delivery.gl.at.ply.gg", "",
                new Vector2(-75, 0), new Vector2(220, 36));

            _portInput = CreateInputFieldV2(joinRow.transform, "30645", "",
                new Vector2(75, 0), new Vector2(65, 36));

            CreateGradientButton(joinRow.transform, "JoinBtn", "JOIN",
                new Color(0.2f, 0.4f, 0.75f), new Color(0.3f, 0.55f, 0.9f),
                new Vector2(160, 0), new Vector2(85, 36), OnJoinServerClicked);
        }

        // ── Tab Builders ──

        private void BuildHairTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU TÓC", 14,
                new Vector2(0, 140), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            // Style grid (2 rows x 3 cols)
            float y = 95f;
            for (int i = 0; i < NetworkPlayer.HairStyleNames.Length; i++)
            {
                int idx = i;
                int row = i / 3; int col = i % 3;
                float x = -130f + col * 130f;
                float yPos = y - row * 75f;

                var slot = CreateStyleSlot(parent, $"Hair_{i}", NetworkPlayer.HairStyleNames[i], new Vector2(x, yPos), new Vector2(110, 66),
                    i == _selectedHairStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedHairStyle = idx; RefreshHairStyleHighlight(); UpdatePreview(); });
            }

            // Color row
            y -= 155f;
            MakeText(parent, "HColorLabel", "MÀU TÓC", 14,
                new Vector2(0, y + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.HairColorPalette, _selectedHairColor, y - 20f,
                (idx) => { _selectedHairColor = idx; UpdatePreview(); }, "HairCol");
        }

        private void BuildOutfitTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU ÁO", 14,
                new Vector2(0, 140), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 95f;
            for (int i = 0; i < NetworkPlayer.OutfitStyleNames.Length; i++)
            {
                int idx = i;
                int row = i / 3; int col = i % 3;
                float x = -130f + col * 130f;
                float yPos = y - row * 75f;

                var slot = CreateStyleSlot(parent, $"Outfit_{i}", NetworkPlayer.OutfitStyleNames[i], new Vector2(x, yPos), new Vector2(110, 66),
                    i == _selectedOutfitStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedOutfitStyle = idx; RefreshOutfitStyleHighlight(); UpdatePreview(); });
            }

            float cy = -65f;
            MakeText(parent, "OColorLabel", "MÀU ÁO", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.BodyColorPalette, _selectedBodyColor, cy - 20f,
                (idx) => { _selectedBodyColor = idx; UpdatePreview(); }, "BodyCol");
        }

        private void BuildPantsTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU QUẦN", 14,
                new Vector2(0, 140), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 95f;
            for (int i = 0; i < NetworkPlayer.PantsStyleNames.Length; i++)
            {
                int idx = i;
                int col = i;
                float x = -145f + col * 97f;

                var slot = CreateStyleSlot(parent, $"Pants_{i}", NetworkPlayer.PantsStyleNames[i], new Vector2(x, y), new Vector2(85, 66),
                    i == _selectedPantsStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedPantsStyle = idx; RefreshPantsStyleHighlight(); UpdatePreview(); });
            }

            float cy = -45f;
            MakeText(parent, "PColorLabel", "MÀU QUẦN", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.PantsColorPalette, _selectedPantsColor, cy - 20f,
                (idx) => { _selectedPantsColor = idx; UpdatePreview(); }, "PantsCol");
        }

        // ── Color Row Builder ──

        private void BuildColorRow(Transform parent, Color[] palette, int selected, float yPos,
            System.Action<int> onSelect, string prefix)
        {
            float swatchSize = 34f;
            float spacing = 6f;
            float totalWidth = palette.Length * (swatchSize + spacing) - spacing;
            float startX = -totalWidth / 2f + swatchSize / 2f;

            for (int i = 0; i < palette.Length; i++)
            {
                int idx = i;
                var swatch = CreatePanel(parent, $"{prefix}_{i}",
                    new Vector2(startX + i * (swatchSize + spacing), yPos), new Vector2(swatchSize, swatchSize));
                swatch.GetComponent<Image>().color = palette[i];
                var btn = swatch.AddComponent<Button>();
                btn.onClick.AddListener(() => onSelect(idx));

                // Selection ring
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

        // ── Style Slot (label only for compatibility) ──

        private GameObject CreateStyleSlot(Transform parent, string name, string label,
            Vector2 pos, Vector2 size, bool selected)
        {
            var slot = CreatePanel(parent, name, pos, size, true); // RaycastTarget = true
            var slotImg = slot.GetComponent<Image>();
            slotImg.color = selected ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
            slot.AddComponent<Button>();

            // Clean bold styling label in slot center
            MakeText(slot.transform, "Label", label, 14,
                Vector2.zero, new Vector2(size.x - 10, size.y - 10), TextAlignmentOptions.Center, Color.white);

            return slot;
        }

        // ── Preview Character (RenderTexture 3D Cam) ──

        private Camera _previewCamera;
        private RenderTexture _previewRT;

        private void CreatePreviewCharacter(Transform parent)
        {
            // Create RenderTexture
            _previewRT = new RenderTexture(512, 512, 16);

            // RawImage to display target texture
            var rawObj = new GameObject("PreviewRawImage");
            rawObj.transform.SetParent(parent, false);
            var rawRt = rawObj.AddComponent<RectTransform>();
            rawRt.anchoredPosition = new Vector2(0, 30);
            rawRt.sizeDelta = new Vector2(300, 320);
            var rawImg = rawObj.AddComponent<RawImage>();
            rawImg.texture = _previewRT;
            rawImg.raycastTarget = false;

            // 3D container far away from play area
            _previewCharacter = new GameObject("PreviewChar");
            _previewCharacter.transform.position = new Vector3(200f, 0f, 200f);

            // Camera looking at the preview character
            var camObj = new GameObject("PreviewCamera");
            camObj.transform.SetParent(_previewCharacter.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0.52f, 1.25f);
            camObj.transform.localRotation = Quaternion.Euler(6f, 180f, 0f); // Look slightly down at torso

            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRT;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.06f, 0.06f, 0.1f, 1f);
            _previewCamera.orthographic = true;
            _previewCamera.orthographicSize = 0.52f;

            // Build mini character model
            var setup = FindAnyObjectByType<LobbySetup>();
            if (setup != null)
            {
                var method = typeof(LobbySetup).GetMethod("CreateCharacterTopDown",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    var charObj = method.Invoke(setup, new object[] { "PreviewModel",
                        NetworkPlayer.BodyColorPalette[Mathf.Clamp(_selectedBodyColor, 0, NetworkPlayer.BodyColorPalette.Length - 1)],
                        new Color(1f, 0.88f, 0.7f) }) as GameObject;
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
            }

            UpdatePreview();

            // Rotate preview character
            StartCoroutine(RotatePreview());
        }

        private System.Collections.IEnumerator RotatePreview()
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

            var model = _previewCharacter.transform.childCount > 0 ? _previewCharacter.transform.GetChild(0).gameObject : null;
            if (model == null) return;

            Color bodyColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(_selectedBodyColor, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
            Color hairColor = NetworkPlayer.HairColorPalette[Mathf.Clamp(_selectedHairColor, 0, NetworkPlayer.HairColorPalette.Length - 1)];
            Color pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(_selectedPantsColor, 0, NetworkPlayer.PantsColorPalette.Length - 1)];

            LobbySetup.ApplyCustomization(model, _selectedHairStyle, hairColor, _selectedOutfitStyle, bodyColor, _selectedPantsStyle, pantsColor);
        }

        // ── Tab Switching ──

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

        // ── Highlight Refreshers ──

        private void RefreshHairStyleHighlight()
        {
            if (_tabHairContent == null) return;
            for (int i = 0; i < NetworkPlayer.HairStyleNames.Length; i++)
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
            for (int i = 0; i < NetworkPlayer.OutfitStyleNames.Length; i++)
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
            for (int i = 0; i < NetworkPlayer.PantsStyleNames.Length; i++)
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

        // ── Save & Connect ──

        private void SavePlayerPrefs()
        {
            if (_nameInput != null)
                PlayerPrefs.SetString("PlayerName", _nameInput.text);
            PlayerPrefs.SetInt("PlayerColorIndex", _selectedBodyColor);
            PlayerPrefs.SetInt("PlayerHairStyle", _selectedHairStyle);
            PlayerPrefs.SetInt("PlayerHairColor", _selectedHairColor);
            PlayerPrefs.SetInt("PlayerOutfitStyle", _selectedOutfitStyle);
            PlayerPrefs.SetInt("PlayerPantsStyle", _selectedPantsStyle);
            PlayerPrefs.SetInt("PlayerPantsColor", _selectedPantsColor);
            PlayerPrefs.Save();
        }

        private void OnHostServerClicked()
        {
            SavePlayerPrefs();
            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
                setup.StartAsHost();
                CloseConnectionPanel();
            }
        }

        private void OnJoinServerClicked()
        {
            SavePlayerPrefs();
            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
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

        // ══════════════════════════════
        //  UI BUILDER HELPERS (Premium)
        // ══════════════════════════════

        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            return CreatePanel(parent, name, pos, size, false);
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size, bool raycastTarget)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.raycastTarget = raycastTarget;
            return obj;
        }

        private GameObject MakeText(Transform parent, string name, string text, float fontSize,
            Vector2 pos, Vector2 size, TextAlignmentOptions align, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false; // Never block raycast clicks
            return obj;
        }

        private TMP_InputField CreateInputFieldV2(Transform parent, string defaultText, string placeholder,
            Vector2 pos, Vector2 size)
        {
            var obj = new GameObject("Input");
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.color = new Color(0.06f, 0.06f, 0.1f, 1f);
            img.raycastTarget = true; // Clickable input

            // Text area
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 2); trt.offsetMax = new Vector2(-10, -2);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.enableWordWrapping = false;
            txt.raycastTarget = false;

            // Placeholder
            if (!string.IsNullOrEmpty(placeholder))
            {
                var phObj = new GameObject("Placeholder");
                phObj.transform.SetParent(obj.transform, false);
                var phRt = phObj.AddComponent<RectTransform>();
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

        private void CreateGradientButton(Transform parent, string name, string label,
            Color colorA, Color colorB, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var btnObj = CreatePanel(parent, name, pos, size, true); // RaycastTarget = true
            var img = btnObj.GetComponent<Image>();
            img.color = Color.Lerp(colorA, colorB, 0.5f);

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            var colors = btn.colors;
            colors.highlightedColor = colorB;
            colors.pressedColor = colorA * 0.8f;
            colors.normalColor = Color.white;
            btn.colors = colors;

            MakeText(btnObj.transform, "Label", label, 15,
                Vector2.zero, size, TextAlignmentOptions.Center, Color.white);
        }
    }
}
