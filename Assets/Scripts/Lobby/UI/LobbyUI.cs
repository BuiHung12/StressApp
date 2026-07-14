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
    public partial class LobbyUI : MonoBehaviour
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

            if (_interactionPanel != null) _interactionPanel.SetActive(false);
            if (_dialoguePanel != null) _dialoguePanel.SetActive(false);

            if (_player != null)
            {
                _player.OnNearInteractable += ShowInteractionPrompt;
                _player.OnLeaveInteractable += HideInteractionPrompt;
                _player.OnJailStart += ShowJailNotification;
                _player.OnJailEnd += HideJailNotification;
                _player.OnCoinsChanged += UpdateCoinUI;

                var currentNear = _player.GetNearestInteractable();
                if (currentNear != null)
                {
                    ShowInteractionPrompt(currentNear);
                }

                CreateCoinUI();
                CreateEmojiUI();
            }
            else
            {
                Debug.LogError("[LobbyUI] Player not found!");
            }

            if (_talkButton != null) _talkButton.onClick.AddListener(OnTalkClicked);
            if (_punchButton != null) _punchButton.onClick.AddListener(OnPunchClicked);
            if (_dialogueNextButton != null) _dialogueNextButton.onClick.AddListener(OnDialogueAdvance);

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

        public void SetPlayer(PlayerController player)
        {
            if (_player != null)
            {
                _player.OnNearInteractable -= ShowInteractionPrompt;
                _player.OnLeaveInteractable -= HideInteractionPrompt;
                _player.OnJailStart -= ShowJailNotification;
                _player.OnJailEnd -= HideJailNotification;
                _player.OnCoinsChanged -= UpdateCoinUI;
            }

            _player = player;

            if (_player != null)
            {
                _player.OnNearInteractable += ShowInteractionPrompt;
                _player.OnLeaveInteractable += HideInteractionPrompt;
                _player.OnJailStart += ShowJailNotification;
                _player.OnJailEnd += HideJailNotification;
                _player.OnCoinsChanged += UpdateCoinUI;

                UpdateCoinUI(_player.Coins);
            }
        }

        private void Update()
        {
            if (_currentTarget != null && _interactionPanel != null && _interactionPanel.activeSelf)
            {
                UpdatePromptPosition();
            }

            if (_dialogueActive && Input.GetMouseButtonDown(0))
            {
                var es = UnityEngine.EventSystems.EventSystem.current;
                if (es != null && !es.IsPointerOverGameObject())
                {
                    OnDialogueAdvance();
                }
            }
        }

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

            float heightOffset = 2.4f * targetMB.transform.localScale.y;
            Vector3 worldPos = targetMB.transform.position + Vector3.up * heightOffset;
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                _interactionPanel.transform.position = screenPos;
            }
        }

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

            if (_player != null)
            {
                var nearest = _player.GetNearestInteractable();
                if (nearest != null)
                {
                    ShowInteractionPrompt(nearest);
                }
            }
        }

        private void OnPunchClicked()
        {
            if (_player == null) return;
            _player.ExecutePunch();
            HideInteractionPrompt();
        }

        public void SpawnPunchEffect(Vector3 worldPosition)
        {
            if (_punchEffectPrefab == null || _worldCanvas == null) return;

            var effect = Instantiate(_punchEffectPrefab, _worldCanvas.transform);
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPosition + Vector3.up * 1.5f);
            effect.transform.position = screenPos;
            Destroy(effect, 1f);
        }

        private void ShowJailNotification(float duration)
        {
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
                bg.color = new Color(0.1f, 0.1f, 0.1f, 0.65f);

                var textObj = new GameObject("JailText");
                textObj.transform.SetParent(_jailPanel.transform, false);
                var trt = textObj.AddComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = trt.offsetMax = Vector2.zero;

                _jailText = textObj.AddComponent<TextMeshProUGUI>();
                _jailText.fontSize = 42;
                _jailText.alignment = TextAlignmentOptions.Center;
                _jailText.color = new Color(0.9f, 0.2f, 0.2f);
                _jailText.fontStyle = FontStyles.Bold;
            }

            _jailText.text = $"{duration:0}";
            _jailPanel.SetActive(true);
            HideInteractionPrompt();

            StartCoroutine(JailCountdownCoroutine(duration));
        }

        private IEnumerator JailCountdownCoroutine(float duration)
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
            _coinText.color = new Color(1f, 0.84f, 0f);
            
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

        private void CreateEmojiUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

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

            _emojiPanel = new GameObject("EmojiPanel");
            _emojiPanel.transform.SetParent(canvas.transform, false);

            var panelRt = _emojiPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.02f, 0.1f);
            panelRt.anchorMax = new Vector2(0.28f, 0.35f);
            panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

            var panelImg = _emojiPanel.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.22f, 0.92f);

            var grid = _emojiPanel.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize = new Vector2(50, 50);
            grid.spacing = new Vector2(6, 6);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;

            for (int i = 0; i < EmojiSystem.AvailableEmojis.Length; i++)
            {
                int index = i;
                var emojiBtn = new GameObject($"Emoji_{i}");
                emojiBtn.transform.SetParent(_emojiPanel.transform, false);

                var btnImg = emojiBtn.AddComponent<Image>();
                btnImg.color = new Color(0.25f, 0.25f, 0.35f, 0.9f);

                var btn = emojiBtn.AddComponent<Button>();
                btn.onClick.AddListener(() => OnEmojiClicked(index));

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

            _emojiPanel.SetActive(false);
        }

        private void ToggleEmojiPanel()
        {
            _emojiPanelOpen = !_emojiPanelOpen;
            if (_emojiPanel != null)
                _emojiPanel.SetActive(_emojiPanelOpen);
        }

        private void OnEmojiClicked(int index)
        {
            if (_player != null)
            {
                var emojiSystem = _player.GetComponent<EmojiSystem>();
                if (emojiSystem != null)
                {
                    emojiSystem.ShowEmoji(index);
                }
            }

            _emojiPanelOpen = false;
            if (_emojiPanel != null)
                _emojiPanel.SetActive(false);
        }
    }
}
