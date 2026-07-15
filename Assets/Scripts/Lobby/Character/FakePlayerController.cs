using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Giả lập player khác trong sảnh — simple transform movement.
    /// </summary>
    public class FakePlayerController : MonoBehaviour, IInteractable, IPunchable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Player";
        [SerializeField] private string _avatarEmoji = "🧒";

        [Header("Greetings")]
        [SerializeField, TextArea(1, 3)]
        private string[] _greetings = new string[]
        {
            "Chào bạn! 👋",
            "Bạn mới vào à? 😊",
        };

        [Header("Behavior")]
        [SerializeField] private FakePlayerBehavior _behavior = FakePlayerBehavior.Wander;
        [SerializeField] private float _wanderRadius = 6f;
        [SerializeField] private float _moveSpeed = 1.8f;
        [SerializeField] private float _pauseMin = 2f;
        [SerializeField] private float _pauseMax = 5f;

        [Header("Punch Reaction")]
        [SerializeField] private float _hurtDuration = 1.5f;
        [SerializeField] private GameObject _swollenFaceEffect;
        [SerializeField] private GameObject _punchStarsEffect;

        // State
        private Vector3 _homePosition;
        private Vector3 _moveTarget;
        private float _moveTimer;
        private bool _isMovingAI;
        private bool _isHurt;
        private float _hurtTimer;
        private Vector3 _knockbackVelocity;

        // IInteractable
        public string DisplayName => _displayName;
        public string AvatarEmoji => _avatarEmoji;
        public bool CanTalk => true;
        public bool CanBePunched => true;
        public InteractableType Type => InteractableType.Player;

        private void Start()
        {
            _homePosition = transform.position;
            _moveTimer = Random.Range(_pauseMin, _pauseMax);
        }

        private void Update()
        {
            if (_isHurt)
            {
                UpdateHurt();
                return;
            }

            if (_behavior == FakePlayerBehavior.Wander)
                UpdateWander();
            // Idle = do nothing
        }

        private void UpdateWander()
        {
            if (_isMovingAI)
            {
                Vector3 dir = (_moveTarget - transform.position);
                dir.y = 0;

                if (dir.magnitude > 0.2f)
                {
                    Vector3 moveDir = dir.normalized;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 8f);
                    Vector3 nextPos = transform.position + moveDir * _moveSpeed * Time.deltaTime;
                    if (IsValidPosition(nextPos))
                    {
                        transform.position = nextPos;
                    }
                    else
                    {
                        // Blocked by obstacle, cancel current wander and choose another target later
                        Debug.Log($"[FakePlayerController] {_displayName} wandering blocked at: {transform.position}");
                        _isMovingAI = false;
                        _moveTimer = Random.Range(_pauseMin, _pauseMax);
                    }
                }
                else
                {
                    Debug.Log($"[FakePlayerController] {_displayName} finished wandering at: {transform.position}");
                    _isMovingAI = false;
                    _moveTimer = Random.Range(_pauseMin, _pauseMax);
                }
            }
            else
            {
                _moveTimer -= Time.deltaTime;
                if (_moveTimer <= 0f)
                {
                    Vector2 rnd = Random.insideUnitCircle * _wanderRadius;
                    _moveTarget = _homePosition + new Vector3(rnd.x, 0, rnd.y);
                    _isMovingAI = true;
                    Debug.Log($"[FakePlayerController] {_displayName} started wandering to: {_moveTarget}");
                }
            }
        }

        public string[] GetDialogueLines()
        {
            int idx = Random.Range(0, _greetings.Length);
            return new string[] { _greetings[idx] };
        }

        public void ReceivePunch(Vector3 knockDirection, float force)
        {
            if (_isHurt) return;
            _isHurt = true;
            _hurtTimer = _hurtDuration;
            _knockbackVelocity = knockDirection * force;
            _isMovingAI = false;

            if (_swollenFaceEffect != null) _swollenFaceEffect.SetActive(true);
            if (_punchStarsEffect != null) _punchStarsEffect.SetActive(true);
        }

        private void UpdateHurt()
        {
            _hurtTimer -= Time.deltaTime;
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

        private bool IsValidPosition(Vector3 pos)
        {
            Collider[] hits = Physics.OverlapSphere(pos + Vector3.up * 0.5f, 0.45f);
            foreach (var hit in hits)
            {
                if (hit.transform.root == transform.root) continue;
                if (hit.isTrigger) continue;

                string name = hit.gameObject.name;
                if (name.Contains("Collider") || name.Contains("Obstacle") || name.Contains("Walls") ||
                    name.Contains("Tree") || name.Contains("Post") || name.Contains("Picket") ||
                    name.Contains("Seat") || name.Contains("Base") || name.Contains("Pillar") ||
                    name.Contains("Bowl") || name.Contains("Bench") || name.Contains("Fountain") ||
                    name.Contains("Fence") || name.Contains("House") || name.Contains("Shop"))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public enum FakePlayerBehavior
    {
        Wander,
        Idle
    }
}
