using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Animation đơn giản cho nhân vật: nhấp nhô khi di chuyển,
    /// tay vung vẩy khi đi, chân lắc lư từ hông, đầu lắc nhẹ.
    /// Gắn tự động bởi LobbySetup.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("Walk Animation")]
        [SerializeField] private float _bobSpeed = 8f;
        [SerializeField] private float _bobHeight = 0.08f;
        [SerializeField] private float _armSwingAngle = 25f;
        [SerializeField] private float _legSwingAngle = 30f;
        [SerializeField] private float _headBobAngle = 3f;
        [SerializeField] private float _bodySwayAngle = 2.5f;

        private Animator _animator;
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");

        private Transform _leftArm, _rightArm;
        private Transform _leftHip, _rightHip;  // Hip pivots for leg swing
        private Transform _head;
        private Transform _torsoContainer;
        private float _baseY;
        private Vector3 _lastPos;
        private bool _isMoving;
        private float _animBlend;  // Smooth blend 0→1 for walk transitions

        private void Start()
        {
            _baseY = transform.localPosition.y;
            _lastPos = transform.position;
            _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                FindLimbs();
            }
        }

        private void FindLimbs()
        {
            _leftArm = transform.Find("LeftShoulderPivot");
            if (_leftArm == null) _leftArm = transform.Find("LeftArm");

            _rightArm = transform.Find("RightShoulderPivot");
            if (_rightArm == null) _rightArm = transform.Find("RightArm");

            _head = transform.Find("Head");
            _torsoContainer = transform.Find("TorsoContainer");

            var legsContainer = transform.Find("LegsContainer");
            if (legsContainer != null)
            {
                // Prefer hip pivots (new structure) for natural leg swing
                _leftHip = legsContainer.Find("LeftHip");
                _rightHip = legsContainer.Find("RightHip");

                // Fallback to direct leg search (old structure)
                if (_leftHip == null) _leftHip = legsContainer.Find("LeftLeg");
                if (_rightHip == null) _rightHip = legsContainer.Find("RightLeg");
            }
            else
            {
                _leftHip = transform.Find("LeftLeg");
                _rightHip = transform.Find("RightLeg");
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

            // Re-find limbs if they were destroyed/recreated by ApplyCustomization
            if (_leftHip == null || _rightHip == null || _leftArm == null || _rightArm == null)
            {
                FindLimbs();
            }

            // Smooth blend between idle (0) and walk (1) for fluid transitions
            float targetBlend = _isMoving ? 1f : 0f;
            _animBlend = Mathf.MoveTowards(_animBlend, targetBlend, Time.deltaTime * 8f);

            float t = Time.time * _bobSpeed;
            float blend = _animBlend;

            if (blend > 0.01f)
            {
                // === Body bob (up/down bounce) ===
                Vector3 pos = transform.localPosition;
                pos.y = _baseY + Mathf.Abs(Mathf.Sin(t)) * _bobHeight * blend;
                transform.localPosition = pos;

                // === Arm swing (opposite to legs) ===
                float armSwing = Mathf.Sin(t) * _armSwingAngle * blend;
                if (_leftArm) _leftArm.localRotation = Quaternion.Euler(armSwing, 0, 0);
                if (_rightArm) _rightArm.localRotation = Quaternion.Euler(-armSwing, 0, 0);

                // === Leg swing from hip pivots ===
                float legSwing = Mathf.Sin(t) * _legSwingAngle * blend;
                if (_leftHip) _leftHip.localRotation = Quaternion.Euler(-legSwing, 0, 0);
                if (_rightHip) _rightHip.localRotation = Quaternion.Euler(legSwing, 0, 0);

                // === Head bob (slight nod) ===
                float headBob = Mathf.Sin(t * 2f) * _headBobAngle * blend;
                if (_head) _head.localRotation = Quaternion.Euler(headBob, 0, 0);

                // === Body sway (slight tilt side to side) ===
                float sway = Mathf.Sin(t * 0.5f) * _bodySwayAngle * blend;
                if (_torsoContainer) _torsoContainer.localRotation = Quaternion.Euler(0, 0, sway);


            }
            else
            {
                // Idle: reset everything smoothly
                Vector3 pos = transform.localPosition;
                pos.y = _baseY;
                transform.localPosition = pos;

                if (_leftArm) _leftArm.localRotation = Quaternion.identity;
                if (_rightArm) _rightArm.localRotation = Quaternion.identity;
                if (_leftHip) _leftHip.localRotation = Quaternion.identity;
                if (_rightHip) _rightHip.localRotation = Quaternion.identity;
                if (_head) _head.localRotation = Quaternion.identity;
                if (_torsoContainer) _torsoContainer.localRotation = Quaternion.identity;
            }
        }
    }
}
