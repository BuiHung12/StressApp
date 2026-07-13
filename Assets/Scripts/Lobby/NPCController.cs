using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// NPC trong sảnh chờ — đi lại tự do (simple transform movement).
    /// Nói chuyện, bị đấm hiện mặt sưng.
    /// </summary>
    public class NPCController : MonoBehaviour, IInteractable, IPunchable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "NPC";
        [SerializeField] private string _avatarEmoji = "🧑";
        [SerializeField] private InteractableType _type = InteractableType.NPC;

        [Header("Dialogue")]
        [SerializeField, TextArea(2, 4)]
        private string[] _dialogueLines = new string[]
        {
            "Xin chào! 👋",
            "Hôm nay đẹp trời nhỉ! ☀️",
        };

        [Header("Wandering AI")]
        [SerializeField] private float _wanderRadius = 4f;
        [SerializeField] private float _wanderPauseMin = 2f;
        [SerializeField] private float _wanderPauseMax = 5f;
        [SerializeField] private float _moveSpeed = 1.2f;

        [Header("Punch Reaction")]
        [SerializeField] private float _hurtDuration = 1.5f;
        [SerializeField] private GameObject _swollenFaceEffect;
        [SerializeField] private GameObject _punchStarsEffect;

        // State
        private Vector3 _homePosition;
        private Vector3 _wanderTarget;
        private float _wanderTimer;
        private bool _isWandering;
        private bool _isHurt;
        private float _hurtTimer;
        private Vector3 _knockbackVelocity;

        // IInteractable
        public string DisplayName => _displayName;
        public string AvatarEmoji => _avatarEmoji;
        public bool CanTalk => true;
        public bool CanBePunched => true;
        public InteractableType Type => _type;

        private void Start()
        {
            _homePosition = transform.position;
            _wanderTimer = Random.Range(_wanderPauseMin, _wanderPauseMax);
        }

        private void Update()
        {
            if (_isHurt)
            {
                UpdateHurt();
                return;
            }
            UpdateWander();
        }

        // ── Wandering ──

        private void UpdateWander()
        {
            if (_isWandering)
            {
                Vector3 dir = (_wanderTarget - transform.position);
                dir.y = 0;

                if (dir.magnitude > 0.2f)
                {
                    transform.position += dir.normalized * _moveSpeed * Time.deltaTime;
                }
                else
                {
                    _isWandering = false;
                    _wanderTimer = Random.Range(_wanderPauseMin, _wanderPauseMax);
                }
            }
            else
            {
                _wanderTimer -= Time.deltaTime;
                if (_wanderTimer <= 0f)
                {
                    // Pick random target near home
                    Vector2 rnd = Random.insideUnitCircle * _wanderRadius;
                    _wanderTarget = _homePosition + new Vector3(rnd.x, 0, rnd.y);
                    _isWandering = true;
                }
            }
        }

        // ── Dialogue ──

        public string[] GetDialogueLines() => _dialogueLines;

        // ── Punch Reaction ──

        public void ReceivePunch(Vector3 knockDirection, float force)
        {
            if (_isHurt) return;

            _isHurt = true;
            _hurtTimer = _hurtDuration;
            _knockbackVelocity = knockDirection * force;
            _isWandering = false;

            if (_swollenFaceEffect != null) _swollenFaceEffect.SetActive(true);
            if (_punchStarsEffect != null) _punchStarsEffect.SetActive(true);
        }

        private void UpdateHurt()
        {
            _hurtTimer -= Time.deltaTime;

            // Knockback
            if (_knockbackVelocity.sqrMagnitude > 0.1f)
            {
                transform.position += _knockbackVelocity * Time.deltaTime;
                _knockbackVelocity *= 0.88f;
            }

            if (_hurtTimer <= 0f)
            {
                _isHurt = false;
                if (_swollenFaceEffect != null) _swollenFaceEffect.SetActive(false);
                if (_punchStarsEffect != null) _punchStarsEffect.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Vector3 center = Application.isPlaying ? _homePosition : transform.position;
            Gizmos.DrawWireSphere(center, _wanderRadius);
        }
    }
}
