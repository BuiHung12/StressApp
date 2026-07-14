using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Hiệu ứng mặt sưng buồn cười khi bị đấm:
    /// - Má sưng to (sphere đỏ bên má)
    /// - Mắt xoáy (X X) 
    /// - Sao quay quanh đầu
    /// - Nhân vật rung lắc
    /// </summary>
    public class SwollenFaceEffect : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private float _starRotateSpeed = 250f;
        [SerializeField] private float _starRadius = 0.6f;
        [SerializeField] private float _shakeIntensity = 0.05f;
        [SerializeField] private float _shakeSpeed = 25f;

        private Transform[] _stars;
        private Vector3 _initialLocalPos;

        private void Awake()
        {
            _initialLocalPos = transform.localPosition;

            // Find star children
            _stars = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                _stars[i] = transform.GetChild(i);
            }
        }

        private void OnEnable()
        {
            // Spawn comic effect
            ComicPunchEffect.SpawnAt(transform.position);
        }

        private void LateUpdate()
        {
            // Billboard face camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            // Shake the character
            Vector3 shake = new Vector3(
                Mathf.Sin(Time.time * _shakeSpeed) * _shakeIntensity,
                Mathf.Sin(Time.time * _shakeSpeed * 1.3f) * _shakeIntensity * 0.5f,
                0
            );
            transform.localPosition = _initialLocalPos + shake;

            // Rotate stars around head
            if (_stars != null)
            {
                for (int i = 0; i < _stars.Length; i++)
                {
                    if (_stars[i] == null) continue;
                    float angle = Time.time * _starRotateSpeed + i * (360f / _stars.Length);
                    float rad = angle * Mathf.Deg2Rad;
                    Vector3 starPos = new Vector3(
                        Mathf.Cos(rad) * _starRadius,
                        0.3f + Mathf.Sin(rad * 2f) * 0.15f,
                        Mathf.Sin(rad) * _starRadius
                    );
                    _stars[i].localPosition = starPos;
                    // Spin the star itself
                    _stars[i].Rotate(0, _starRotateSpeed * 2f * Time.deltaTime, 0);
                }
            }
        }
    }
}
