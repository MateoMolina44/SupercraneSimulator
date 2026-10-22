using UnityEngine;
using UnityEngine.InputSystem;

namespace SupercraneSimulator.Cameras
{
    /// <summary>
    /// Cámara externa que orbita alrededor de un punto que sigue al pórtico.
    /// Clic derecho y arrastrar para orbitar, rueda para acercar o alejar.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class OrbitCamera : MonoBehaviour
    {
        [SerializeField, Tooltip("Objeto a seguir (el pórtico)")]
        private Transform _target;
        [SerializeField, Tooltip("Punto de órbita relativo al objeto seguido")]
        private Vector3 _pivotOffset = new Vector3(0f, 8f, 4f);

        [Header("Sensibilidad")]
        [SerializeField, Min(0f), Tooltip("Grados por píxel de arrastre")]
        private float _orbitSpeed = 0.25f;
        [SerializeField, Min(0f), Tooltip("Metros por paso de la rueda")]
        private float _zoomSpeed = 2.5f;

        [Header("Límites")]
        [SerializeField] private float _minPitch = 5f;
        [SerializeField] private float _maxPitch = 80f;
        [SerializeField] private float _minDistance = 12f;
        [SerializeField] private float _maxDistance = 90f;
        [SerializeField, Min(0.01f), Tooltip("Segundos que tarda en seguir al pórtico")]
        private float _followTime = 0.3f;

        private Camera _camera;
        private Vector3 _pivot;
        private Vector3 _pivotVelocity;
        private float _yaw;
        private float _pitch;
        private float _distance;

        /// <summary>Multiplicador de la sensibilidad, para la configuración.</summary>
        public float Sensitivity { get; set; } = 1f;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _pivot = _target.TransformPoint(_pivotOffset);

            // Arranca desde la pose con la que se armó la escena.
            var offset = transform.position - _pivot;
            _distance = Mathf.Clamp(offset.magnitude, _minDistance, _maxDistance);
            var angles = Quaternion.LookRotation(-offset).eulerAngles;
            _yaw = angles.y;
            _pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, angles.x), _minPitch, _maxPitch);
        }

        private void LateUpdate()
        {
            if (_camera.enabled && Time.timeScale > 0f)
            {
                ReadMouse();
            }

            _pivot = Vector3.SmoothDamp(_pivot, _target.TransformPoint(_pivotOffset), ref _pivotVelocity, _followTime,
                Mathf.Infinity, Time.unscaledDeltaTime);
            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.SetPositionAndRotation(_pivot - rotation * Vector3.forward * _distance, rotation);
        }

        public void Configure(Transform target, Vector3 pivotOffset)
        {
            _target = target;
            _pivotOffset = pivotOffset;
        }

        private void ReadMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.rightButton.isPressed)
            {
                var delta = mouse.delta.ReadValue() * (_orbitSpeed * Sensitivity);
                _yaw += delta.x;
                _pitch = Mathf.Clamp(_pitch - delta.y, _minPitch, _maxPitch);
            }

            // La rueda devuelve 120 por paso en Windows y valores chicos en WebGL: solo importa el signo.
            var scroll = mouse.scroll.ReadValue().y;
            if (!Mathf.Approximately(scroll, 0f))
            {
                _distance = Mathf.Clamp(_distance - Mathf.Sign(scroll) * _zoomSpeed * Sensitivity, _minDistance, _maxDistance);
            }
        }
    }
}
