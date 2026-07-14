using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Billboard text — luôn hướng về camera.
    /// Gắn vào name tag trên đầu nhân vật.
    /// </summary>
    public class BillboardText : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            // Face the camera
            transform.rotation = _mainCamera.transform.rotation;
        }
    }
}
