using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Camera góc chéo isometric, orthographic.
    /// Theo dõi nhân vật mượt mà.
    /// </summary>
    public class LobbyCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Camera Settings")]
        [SerializeField] private float _angle = 35f;
        [SerializeField] private float _distance = 20f;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _orthoSize = 3.2f;

        [Header("Zoom")]
        [SerializeField] private float _minZoom = 2f;
        [SerializeField] private float _maxZoom = 8f;
        [SerializeField] private float _zoomSpeed = 3f;

        private Camera _cam;
        private Vector3 _offset;

        private void Start()
        {
            if (NetworkSetup.IsHeadlessServer())
            {
                enabled = false;
                return;
            }
            _cam = GetComponent<Camera>();

            if (_target == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null) _target = player.transform;
            }

            // Calculate offset based on angle
            CalculateOffset();

            _cam.orthographic = true;
            _cam.orthographicSize = _orthoSize;
        }

        private void CalculateOffset()
        {
            float rad = _angle * Mathf.Deg2Rad;
            _offset = new Vector3(0, _distance * Mathf.Sin(rad), -_distance * Mathf.Cos(rad));
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Check if target is player and is jailed
            var playerCtrl = _target.GetComponent<PlayerController>();
            bool isJailed = (playerCtrl != null && playerCtrl.IsJailed);

            // Zoom with scroll wheel (PC) or pinch-to-zoom (mobile)
            if (!isJailed)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");

                // === Pinch-to-zoom for mobile ===
                if (Input.touchCount == 2)
                {
                    Touch t0 = Input.GetTouch(0);
                    Touch t1 = Input.GetTouch(1);

                    // Ignore pinch if either touch is in the virtual joystick zone (bottom-left, e.g. < 400px)
                    bool t0Joy = t0.position.x < 400f && t0.position.y < 400f;
                    bool t1Joy = t1.position.x < 400f && t1.position.y < 400f;

                    if (!t0Joy && !t1Joy)
                    {
                        Vector2 t0Prev = t0.position - t0.deltaPosition;
                        Vector2 t1Prev = t1.position - t1.deltaPosition;

                        float prevDist = (t0Prev - t1Prev).magnitude;
                        float currDist = (t0.position - t1.position).magnitude;

                        float pinchDelta = (currDist - prevDist) * 0.01f;
                        scroll += pinchDelta;
                    }
                }

                if (scroll != 0f)
                {
                    _orthoSize -= scroll * _zoomSpeed;
                    _orthoSize = Mathf.Clamp(_orthoSize, _minZoom, _maxZoom);
                }
                // Smoothly lerp camera ortho size to target size
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _orthoSize, _smoothSpeed * Time.deltaTime);
            }
            else
            {
                // Smoothly zoom out to frame both prisoner and visiting bot
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, 4.5f, _smoothSpeed * Time.deltaTime);
            }

            // Decide focus position
            Vector3 focusPos = _target.position;
            if (isJailed)
            {
                // Shift focus to the midpoint between the cell and visiting desk (approx Z=-59.5f)
                focusPos = new Vector3(_target.position.x, 0f, -59.5f);
            }

            // Smooth follow focus point
            Vector3 desiredPos = focusPos + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, _smoothSpeed * Time.deltaTime);

            // Keep looking at target with isometric angle
            transform.rotation = Quaternion.Euler(_angle, 0f, 0f);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
