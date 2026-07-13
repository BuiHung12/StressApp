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

            // Zoom with scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                _orthoSize -= scroll * _zoomSpeed;
                _orthoSize = Mathf.Clamp(_orthoSize, _minZoom, _maxZoom);
                _cam.orthographicSize = _orthoSize;
            }

            // Smooth follow target
            Vector3 desiredPos = _target.position + _offset;
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
