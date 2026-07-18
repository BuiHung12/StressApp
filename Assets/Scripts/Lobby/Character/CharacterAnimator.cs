using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Animation đơn giản cho nhân vật: nhấp nhô khi di chuyển,
    /// tay vung vẩy khi đi. Gắn tự động bởi LobbySetup.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("Walk Animation")]
        [SerializeField] private float _bobSpeed = 8f;
        [SerializeField] private float _bobHeight = 0.08f;
        [SerializeField] private float _armSwingAngle = 25f;
        [SerializeField] private float _legSwingAngle = 20f;

        private Animator _animator;
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");

        private Transform _leftArm, _rightArm, _leftLeg, _rightLeg;
        private float _baseY;
        private Vector3 _lastPos;
        private bool _isMoving;

        private void Start()
        {
            _baseY = transform.localPosition.y;
            _lastPos = transform.position;
            _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                // Find limbs only for procedural animation
                _leftArm = transform.Find("LeftArm");
                _rightArm = transform.Find("RightArm");

                var legsContainer = transform.Find("LegsContainer");
                if (legsContainer != null)
                {
                    _leftLeg = legsContainer.Find("LeftLeg");
                    _rightLeg = legsContainer.Find("RightLeg");
                }
                else
                {
                    _leftLeg = transform.Find("LeftLeg");
                    _rightLeg = transform.Find("RightLeg");
                }
            }
        }

        private void LateUpdate()
        {
            Vector3 diff = transform.position - _lastPos;
            diff.y = 0f; // Ignore vertical bobbing for movement detection to prevent animation feedback loop
            float displacement = diff.magnitude;
            
            // Bỏ qua các sai số dịch chuyển siêu nhỏ (dưới 2 milimet) do nội suy mạng (SmoothDamp)
            if (displacement < 0.002f)
            {
                displacement = 0f;
            }

            float speed = displacement / Time.deltaTime;
            _isMoving = speed > 0.15f;
            _lastPos = transform.position;

            if (_animator != null)
            {
                _animator.SetFloat(AnimSpeed, _isMoving ? 1f : 0f);
                return; // Skip manual limb swinging when using Unity Animator
            }

            if (_isMoving)
            {
                float t = Time.time * _bobSpeed;

                // Bob up and down
                Vector3 pos = transform.localPosition;
                pos.y = _baseY + Mathf.Abs(Mathf.Sin(t)) * _bobHeight;
                transform.localPosition = pos;

                // Swing arms
                float swing = Mathf.Sin(t) * _armSwingAngle;
                if (_leftArm) _leftArm.localRotation = Quaternion.Euler(swing, 0, 0);
                if (_rightArm) _rightArm.localRotation = Quaternion.Euler(-swing, 0, 0);

                // Swing legs
                float legSwing = Mathf.Sin(t) * _legSwingAngle;
                if (_leftLeg) _leftLeg.localRotation = Quaternion.Euler(-legSwing, 0, 0);
                if (_rightLeg) _rightLeg.localRotation = Quaternion.Euler(legSwing, 0, 0);
            }
            else
            {
                // Dừng hành động bật nhảy và vung tay chân ngay lập tức!
                Vector3 pos = transform.localPosition;
                pos.y = _baseY;
                transform.localPosition = pos;

                if (_leftArm) _leftArm.localRotation = Quaternion.identity;
                if (_rightArm) _rightArm.localRotation = Quaternion.identity;
                if (_leftLeg) _leftLeg.localRotation = Quaternion.identity;
                if (_rightLeg) _rightLeg.localRotation = Quaternion.identity;
            }
        }
    }
}
