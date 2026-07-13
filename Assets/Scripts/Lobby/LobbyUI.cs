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
        private int _selectedColorIndex = 0;
        private Image[] _colorSwatches;
        private GameObject _colorIndicator;

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

        // ── Connection Panel UI ──

        private void CreateConnectionUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Load saved preferences
            _selectedColorIndex = PlayerPrefs.GetInt("PlayerColorIndex", 0);
            string savedName = PlayerPrefs.GetString("PlayerName", "");

            // Connection Panel (full-screen overlay)
            _connectionPanel = new GameObject("ConnectionPanel");
            _connectionPanel.transform.SetParent(canvas.transform, false);

            var panelRt = _connectionPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

            var panelImg = _connectionPanel.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.14f, 0.97f);

            // Card Container (bigger to fit customization)
            var card = new GameObject("Card");
            card.transform.SetParent(_connectionPanel.transform, false);
            var cardRt = card.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(480, 580);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.16f, 0.16f, 0.22f, 1f);

            float y = 250f; // Start from top

            // ── Title ──
            var titleObj = CreateUIText(card.transform, "RANGER CITY LOBBY", 26, new Vector2(0, y), new Vector2(420, 40));
            titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.85f, 1f);
            y -= 50f;

            // ── Tên nhân vật ──
            CreateUIText(card.transform, "Tên nhân vật:", 15, new Vector2(-120, y), new Vector2(200, 24), TextAlignmentOptions.Left, new Color(0.75f, 0.75f, 0.85f));
            y -= 30f;

            var nameObj = new GameObject("NameInput");
            nameObj.transform.SetParent(card.transform, false);
            var nameRt = nameObj.AddComponent<RectTransform>();
            nameRt.anchoredPosition = new Vector2(0, y);
            nameRt.sizeDelta = new Vector2(380, 40);
            var nameImg = nameObj.AddComponent<Image>();
            nameImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            var nameTextArea = new GameObject("Text");
            nameTextArea.transform.SetParent(nameObj.transform, false);
            var nrt = nameTextArea.AddComponent<RectTransform>();
            nrt.anchorMin = Vector2.zero;
            nrt.anchorMax = Vector2.one;
            nrt.offsetMin = new Vector2(10, 0);
            nrt.offsetMax = new Vector2(-10, 0);
            var nameTxt = nameTextArea.AddComponent<TextMeshProUGUI>();
            nameTxt.fontSize = 18;
            nameTxt.color = Color.white;

            // Placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(nameObj.transform, false);
            var phRt = placeholderObj.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(10, 0);
            phRt.offsetMax = new Vector2(-10, 0);
            var phTxt = placeholderObj.AddComponent<TextMeshProUGUI>();
            phTxt.text = "Nhập tên của bạn...";
            phTxt.fontSize = 18;
            phTxt.fontStyle = FontStyles.Italic;
            phTxt.color = new Color(0.5f, 0.5f, 0.55f);

            _nameInput = nameObj.AddComponent<TMP_InputField>();
            _nameInput.textComponent = nameTxt;
            _nameInput.placeholder = phTxt;
            _nameInput.text = savedName;
            _nameInput.characterLimit = 16;

            y -= 45f;

            // ── Chọn màu nhân vật ──
            CreateUIText(card.transform, "Chọn màu nhân vật:", 15, new Vector2(-100, y), new Vector2(240, 24), TextAlignmentOptions.Left, new Color(0.75f, 0.75f, 0.85f));
            y -= 35f;

            // Color swatch grid
            var colorGrid = new GameObject("ColorGrid");
            colorGrid.transform.SetParent(card.transform, false);
            var gridRt = colorGrid.AddComponent<RectTransform>();
            gridRt.anchoredPosition = new Vector2(0, y);
            gridRt.sizeDelta = new Vector2(380, 45);

            _colorSwatches = new Image[NetworkPlayer.ColorPalette.Length];
            float swatchSize = 36f;
            float spacing = 2f;
            float totalWidth = NetworkPlayer.ColorPalette.Length * (swatchSize + spacing) - spacing;
            float startX = -totalWidth / 2f + swatchSize / 2f;

            for (int i = 0; i < NetworkPlayer.ColorPalette.Length; i++)
            {
                int idx = i;
                var swatch = new GameObject($"Color_{i}");
                swatch.transform.SetParent(colorGrid.transform, false);
                var swRt = swatch.AddComponent<RectTransform>();
                swRt.anchoredPosition = new Vector2(startX + i * (swatchSize + spacing), 0);
                swRt.sizeDelta = new Vector2(swatchSize, swatchSize);

                var swImg = swatch.AddComponent<Image>();
                swImg.color = NetworkPlayer.ColorPalette[i];
                _colorSwatches[i] = swImg;

                var swBtn = swatch.AddComponent<Button>();
                swBtn.onClick.AddListener(() => SelectColor(idx));
            }

            // Selection indicator border
            _colorIndicator = new GameObject("Indicator");
            _colorIndicator.transform.SetParent(colorGrid.transform, false);
            var indRt = _colorIndicator.AddComponent<RectTransform>();
            indRt.sizeDelta = new Vector2(swatchSize + 6, swatchSize + 6);
            var indImg = _colorIndicator.AddComponent<Image>();
            indImg.color = Color.clear;
            var outline = _colorIndicator.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3, -3);

            // Position indicator on default color
            UpdateColorIndicator();

            y -= 50f;

            // ── Device ID ──
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string shortId = deviceId.Length > 12 ? deviceId.Substring(0, 12) + "..." : deviceId;
            CreateUIText(card.transform, $"ID Thiết bị: {shortId}", 12, new Vector2(0, y), new Vector2(380, 20), TextAlignmentOptions.Center, new Color(0.5f, 0.5f, 0.6f));

            y -= 40f;

            // ── Separator ──
            var sep = new GameObject("Separator");
            sep.transform.SetParent(card.transform, false);
            var sepRt = sep.AddComponent<RectTransform>();
            sepRt.anchoredPosition = new Vector2(0, y);
            sepRt.sizeDelta = new Vector2(380, 1);
            var sepImg = sep.AddComponent<Image>();
            sepImg.color = new Color(0.3f, 0.3f, 0.4f);

            y -= 30f;

            // ── Host Button ──
            CreateActionButton(card.transform, "▶ Host Server (Cho máy chủ)", new Color(0.18f, 0.62f, 0.34f), new Vector2(0, y), OnHostServerClicked);

            y -= 60f;

            // ── IP / Port Inputs ──
            CreateUIText(card.transform, "IP / Domain:", 13, new Vector2(-80, y + 18), new Vector2(160, 20), TextAlignmentOptions.Left, new Color(0.65f, 0.65f, 0.75f));
            CreateUIText(card.transform, "Port:", 13, new Vector2(125, y + 18), new Vector2(80, 20), TextAlignmentOptions.Left, new Color(0.65f, 0.65f, 0.75f));

            // Address
            _addressInput = CreateInputField(card.transform, "wool-delivery.gl.at.ply.gg", new Vector2(-55, y), new Vector2(230, 36));

            // Port
            _portInput = CreateInputField(card.transform, "30645", new Vector2(140, y), new Vector2(80, 36));

            y -= 50f;

            // ── Join Button ──
            CreateActionButton(card.transform, "🌐 Join Lobby (Cho người chơi)", new Color(0.2f, 0.48f, 0.82f), new Vector2(0, y), OnJoinServerClicked);
        }

        // ── Helper: Create text element ──
        private GameObject CreateUIText(Transform parent, string text, float fontSize, Vector2 pos, Vector2 size,
            TextAlignmentOptions align = TextAlignmentOptions.Center, Color? color = null)
        {
            var obj = new GameObject("Text_" + text.Substring(0, Mathf.Min(10, text.Length)));
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color ?? Color.white;
            return obj;
        }

        // ── Helper: Create input field ──
        private TMP_InputField CreateInputField(Transform parent, string defaultText, Vector2 pos, Vector2 size)
        {
            var obj = new GameObject("Input");
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.08f, 0.12f, 1f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(8, 0);
            trt.offsetMax = new Vector2(-8, 0);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 14;
            txt.color = Color.white;

            var input = obj.AddComponent<TMP_InputField>();
            input.textComponent = txt;
            input.text = defaultText;
            return input;
        }

        // ── Helper: Create action button ──
        private void CreateActionButton(Transform parent, string text, Color bgColor, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            var btnObj = new GameObject("Btn_" + text.Substring(0, Mathf.Min(8, text.Length)));
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(340, 48);
            var img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            // Hover effects
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private void SelectColor(int index)
        {
            _selectedColorIndex = index;
            UpdateColorIndicator();
        }

        private void UpdateColorIndicator()
        {
            if (_colorIndicator == null || _colorSwatches == null) return;
            if (_selectedColorIndex >= 0 && _selectedColorIndex < _colorSwatches.Length)
            {
                _colorIndicator.transform.position = _colorSwatches[_selectedColorIndex].transform.position;
            }
        }

        private void SavePlayerPrefs()
        {
            // Lưu tên và màu vào PlayerPrefs để NetworkPlayer đọc khi connect
            if (_nameInput != null)
                PlayerPrefs.SetString("PlayerName", _nameInput.text);
            PlayerPrefs.SetInt("PlayerColorIndex", _selectedColorIndex);
            PlayerPrefs.Save();
        }

        private void OnHostServerClicked()
        {
            SavePlayerPrefs();
            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
                setup.StartAsHost();
                if (_connectionPanel != null) _connectionPanel.SetActive(false);
            }
        }

        private void OnJoinServerClicked()
        {
            SavePlayerPrefs();
            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
                string address = _addressInput.text;
                string portStr = _portInput.text;

                if (ushort.TryParse(portStr, out ushort port))
                {
                    var transport = Mirror.NetworkManager.singleton.transport as kcp2k.KcpTransport;
                    if (transport != null)
                    {
                        transport.port = port;
                    }
                }

                setup.StartAsClient(address);
                if (_connectionPanel != null) _connectionPanel.SetActive(false);
            }
        }
    }
}
