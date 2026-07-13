using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// UI quản lý tương tác trong sảnh chờ:
    /// - Nút Talk / Punch khi vào phạm vi
    /// - Dialogue box với typing effect
    /// - Hiệu ứng đấm
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

        private void Start()
        {
            _player = FindAnyObjectByType<PlayerController>();
            _mainCamera = Camera.main;

            // Hide UI initially
            _interactionPanel.SetActive(false);
            _dialoguePanel.SetActive(false);

            // Wire events
            _player.OnNearInteractable += ShowInteractionPrompt;
            _player.OnLeaveInteractable += HideInteractionPrompt;

            _talkButton.onClick.AddListener(OnTalkClicked);
            _punchButton.onClick.AddListener(OnPunchClicked);
            _dialogueNextButton.onClick.AddListener(OnDialogueAdvance);
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnNearInteractable -= ShowInteractionPrompt;
                _player.OnLeaveInteractable -= HideInteractionPrompt;
            }
        }

        private void Update()
        {
            // Update interaction prompt position (follow target in world space)
            if (_currentTarget != null && _interactionPanel.activeSelf)
            {
                UpdatePromptPosition();
            }

            // Advance dialogue with click/tap
            if (_dialogueActive && Input.GetMouseButtonDown(0))
            {
                // Only if not clicking a button
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    OnDialogueAdvance();
                }
            }
        }

        // ── Interaction Prompt ──────────────────────────────────

        private void ShowInteractionPrompt(IInteractable target)
        {
            if (_dialogueActive) return;

            _currentTarget = target;
            _interactionPanel.SetActive(true);

            _targetNameText.text = target.DisplayName;
            _talkButton.gameObject.SetActive(target.CanTalk);
            _punchButton.gameObject.SetActive(target.CanBePunched);

            UpdatePromptPosition();
        }

        private void HideInteractionPrompt()
        {
            _currentTarget = null;
            _interactionPanel.SetActive(false);
        }

        private void UpdatePromptPosition()
        {
            if (_currentTarget == null || _mainCamera == null) return;

            MonoBehaviour targetMB = _currentTarget as MonoBehaviour;
            if (targetMB == null) return;

            // Position just above the character's head
            Vector3 worldPos = targetMB.transform.position + Vector3.up * 5.5f;
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

            _dialogueNameText.text = _currentTarget.DisplayName;
            _dialoguePanel.SetActive(true);
            _interactionPanel.SetActive(false);

            ShowDialogueLine(_currentDialogueLines[0]);
        }

        private void ShowDialogueLine(string text)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _isTypingComplete = false;
            _dialogueHintText.text = "";
            _typingCoroutine = StartCoroutine(TypeDialogue(text));
        }

        private IEnumerator TypeDialogue(string text)
        {
            _dialogueContentText.text = "";

            for (int i = 0; i < text.Length; i++)
            {
                _dialogueContentText.text += text[i];
                yield return new WaitForSeconds(1f / _typingSpeed);
            }

            _isTypingComplete = true;
            _dialogueHintText.text = "Nhấn để tiếp tục...";
        }

        private void OnDialogueAdvance()
        {
            if (!_dialogueActive) return;

            if (!_isTypingComplete)
            {
                // Skip typing - show full text
                if (_typingCoroutine != null)
                    StopCoroutine(_typingCoroutine);
                _dialogueContentText.text = _currentDialogueLines[_currentLineIndex];
                _isTypingComplete = true;
                _dialogueHintText.text = "Nhấn để tiếp tục...";
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
            _dialoguePanel.SetActive(false);

            // Re-check if still near interactable
            var nearest = _player.GetNearestInteractable();
            if (nearest != null)
            {
                ShowInteractionPrompt(nearest);
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
    }
}
