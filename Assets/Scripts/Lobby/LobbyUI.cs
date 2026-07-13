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

            // Connection Panel (overlay background)
            _connectionPanel = new GameObject("ConnectionPanel");
            _connectionPanel.transform.SetParent(canvas.transform, false);

            var panelRt = _connectionPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

            var panelImg = _connectionPanel.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            // Card Container
            var card = new GameObject("Card");
            card.transform.SetParent(_connectionPanel.transform, false);
            var cardRt = card.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector3(450, 400);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.18f, 0.18f, 0.25f, 1f);

            // Title Text
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);
            var titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 140);
            titleRt.sizeDelta = new Vector2(400, 50);
            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "MULTIPLAYER CHOOSE MODE";
            titleTxt.fontSize = 24;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = Color.white;

            // Host Button
            var hostBtnObj = new GameObject("HostButton");
            hostBtnObj.transform.SetParent(card.transform, false);
            var hostRt = hostBtnObj.AddComponent<RectTransform>();
            hostRt.anchoredPosition = new Vector2(0, 70);
            hostRt.sizeDelta = new Vector2(300, 50);
            var hostImg = hostBtnObj.AddComponent<Image>();
            hostImg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
            var hostBtn = hostBtnObj.AddComponent<Button>();
            hostBtn.onClick.AddListener(OnHostServerClicked);

            var hostTextObj = new GameObject("Text");
            hostTextObj.transform.SetParent(hostBtnObj.transform, false);
            var hostTxtRt = hostTextObj.AddComponent<RectTransform>();
            hostTxtRt.anchorMin = Vector2.zero;
            hostTxtRt.anchorMax = Vector2.one;
            hostTxtRt.offsetMin = hostTxtRt.offsetMax = Vector2.zero;
            var hostTxt = hostTextObj.AddComponent<TextMeshProUGUI>();
            hostTxt.text = "Host Server (Cho máy chủ)";
            hostTxt.fontSize = 18;
            hostTxt.alignment = TextAlignmentOptions.Center;

            // Address Input
            var addressObj = new GameObject("AddressInput");
            addressObj.transform.SetParent(card.transform, false);
            var addrRt = addressObj.AddComponent<RectTransform>();
            addrRt.anchoredPosition = new Vector2(-60, -10);
            addrRt.sizeDelta = new Vector2(180, 40);
            var addrImg = addressObj.AddComponent<Image>();
            addrImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            var addrTextObj = new GameObject("TextArea");
            addrTextObj.transform.SetParent(addressObj.transform, false);
            var addrTextRt = addrTextObj.AddComponent<RectTransform>();
            addrTextRt.anchorMin = Vector2.zero;
            addrTextRt.anchorMax = Vector2.one;
            addrTextRt.offsetMin = new Vector2(8, 0);
            addrTextRt.offsetMax = new Vector2(-8, 0);

            var addrTxt = addrTextObj.AddComponent<TextMeshProUGUI>();
            addrTxt.fontSize = 16;
            addrTxt.alignment = TextAlignmentOptions.Left;
            addrTxt.color = Color.white;

            _addressInput = addressObj.AddComponent<TMP_InputField>();
            _addressInput.textComponent = addrTxt;
            _addressInput.text = "wool-delivery.gl.at.ply.gg";

            // Port Input
            var portObj = new GameObject("PortInput");
            portObj.transform.SetParent(card.transform, false);
            var portRt = portObj.AddComponent<RectTransform>();
            portRt.anchoredPosition = new Vector2(110, -10);
            portRt.sizeDelta = new Vector2(100, 40);
            var portImg = portObj.AddComponent<Image>();
            portImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            var portTextObj = new GameObject("TextArea");
            portTextObj.transform.SetParent(portObj.transform, false);
            var portTextRt = portTextObj.AddComponent<RectTransform>();
            portTextRt.anchorMin = Vector2.zero;
            portTextRt.anchorMax = Vector2.one;
            portTextRt.offsetMin = new Vector2(8, 0);
            portTextRt.offsetMax = new Vector2(-8, 0);

            var portTxt = portTextObj.AddComponent<TextMeshProUGUI>();
            portTxt.fontSize = 16;
            portTxt.alignment = TextAlignmentOptions.Left;
            portTxt.color = Color.white;

            _portInput = portObj.AddComponent<TMP_InputField>();
            _portInput.textComponent = portTxt;
            _portInput.text = "30645";

            // Labels
            CreateLabel(card.transform, "IP / Domain:", new Vector2(-60, 20));
            CreateLabel(card.transform, "Port:", new Vector2(110, 20));

            // Join Client Button
            var joinBtnObj = new GameObject("JoinButton");
            joinBtnObj.transform.SetParent(card.transform, false);
            var joinRt = joinBtnObj.AddComponent<RectTransform>();
            joinRt.anchoredPosition = new Vector2(0, -90);
            joinRt.sizeDelta = new Vector2(300, 50);
            var joinImg = joinBtnObj.AddComponent<Image>();
            joinImg.color = new Color(0.2f, 0.5f, 0.8f, 1f);
            var joinBtn = joinBtnObj.AddComponent<Button>();
            joinBtn.onClick.AddListener(OnJoinServerClicked);

            var joinTextObj = new GameObject("Text");
            joinTextObj.transform.SetParent(joinBtnObj.transform, false);
            var joinTxtRt = joinTextObj.AddComponent<RectTransform>();
            joinTxtRt.anchorMin = Vector2.zero;
            joinTxtRt.anchorMax = Vector2.one;
            joinTxtRt.offsetMin = joinTxtRt.offsetMax = Vector2.zero;
            var joinTxt = joinTextObj.AddComponent<TextMeshProUGUI>();
            joinTxt.text = "Join Lobby (Cho người chơi)";
            joinTxt.fontSize = 18;
            joinTxt.alignment = TextAlignmentOptions.Center;
        }

        private void CreateLabel(Transform parent, string text, Vector2 pos)
        {
            var labelObj = new GameObject("Label_" + text);
            labelObj.transform.SetParent(parent, false);
            var rt = labelObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(120, 20);
            var txt = labelObj.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = 14;
            txt.color = new Color(0.8f, 0.8f, 0.8f);
            txt.alignment = TextAlignmentOptions.Left;
        }

        private void OnHostServerClicked()
        {
            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
                setup.StartAsHost();
                if (_connectionPanel != null) _connectionPanel.SetActive(false);
            }
        }

        private void OnJoinServerClicked()
        {
            var setup = FindAnyObjectByType<NetworkSetup>();
            if (setup != null)
            {
                string address = _addressInput.text;
                string portStr = _portInput.text;

                if (ushort.TryParse(portStr, out ushort port))
                {
                    // Mirror uses transport to set port
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
