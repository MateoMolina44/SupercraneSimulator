using SupercraneSimulator.Input;
using UnityEngine;

namespace SupercraneSimulator.Cameras
{
    /// <summary>
    /// Alterna entre la cámara de cabina y la externa con la tecla C o un botón de UI.
    /// </summary>
    public sealed class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CraneInput _input;
        [SerializeField] private Camera _cabinCamera;
        [SerializeField] private Camera _externalCamera;
        [SerializeField] private bool _startInCabin;

        private bool _inCabin;

        public bool InCabin => _inCabin;

        private void Start()
        {
            SetCabin(_startInCabin);
        }

        private void OnEnable()
        {
            _input.CameraPressed += Toggle;
        }

        private void OnDisable()
        {
            _input.CameraPressed -= Toggle;
        }

        public void Toggle()
        {
            SetCabin(!_inCabin);
        }

        public void Configure(CraneInput input, Camera cabin, Camera external)
        {
            _input = input;
            _cabinCamera = cabin;
            _externalCamera = external;
        }

        private void SetCabin(bool inCabin)
        {
            _inCabin = inCabin;
            _cabinCamera.enabled = inCabin;
            _externalCamera.enabled = !inCabin;

            // Un solo AudioListener activo.
            if (_cabinCamera.TryGetComponent(out AudioListener cabinListener))
            {
                cabinListener.enabled = inCabin;
            }

            if (_externalCamera.TryGetComponent(out AudioListener externalListener))
            {
                externalListener.enabled = !inCabin;
            }
        }
    }
}
