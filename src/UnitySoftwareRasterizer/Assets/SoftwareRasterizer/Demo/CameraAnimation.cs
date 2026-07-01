using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SoftwareRasterizer.Demo
{
    public class CameraAnimation : MonoBehaviour
    {
        public Transform TargetPoint;
        public float Duration;

        public InputAction InputAction;

        private bool _isAnimating;

        private void OnEnable()
        {
            InputAction.Enable();
            InputAction.performed += OnPerformed;
        }

        private void OnDisable()
        {
            InputAction.performed -= OnPerformed;
            InputAction.Disable();
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            if (_isAnimating)
                return;

            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            _isAnimating = true;

            var startPosition = transform.position;
            var targetPosition = TargetPoint.position;

            Camera camera = GetComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 56.75f;
            

            var elapsed = 0f;
            while (elapsed < Duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / Duration);
                var eased = Mathf.SmoothStep(0f, 1f, t);

                transform.position = Vector3.Lerp(startPosition, targetPosition, eased);
                transform.LookAt(Vector3.zero);

                yield return null;
            }

            transform.position = targetPosition;
            transform.LookAt(Vector3.zero);

            _isAnimating = false;
        }
    }
}
