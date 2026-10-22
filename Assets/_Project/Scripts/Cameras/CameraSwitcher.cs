using SupercraneSimulator.Input;
using UnityEngine;
using UnityEngine.UI;

namespace SupercraneSimulator.Cameras
{
    /// <summary>
    /// Recorre las cámaras en orden con la tecla C o un botón de UI. Solo una queda activa.
    /// </summary>
    public sealed class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CraneInput _input;
        [SerializeField] private Camera[] _cameras;
        [SerializeField] private string[] _names;
        [SerializeField, Tooltip("Opcional: muestra el nombre de la cámara activa")]
        private Text _label;
        [SerializeField, Min(0)] private int _startIndex;

        private int _index;

        public int Index => _index;

        private void Start()
        {
            Select(_startIndex);
        }

        private void OnEnable()
        {
            _input.CameraPressed += Next;
        }

        private void OnDisable()
        {
            _input.CameraPressed -= Next;
        }

        public void Next()
        {
            Select((_index + 1) % _cameras.Length);
        }

        public void Configure(CraneInput input, Text label, Camera[] cameras, string[] names)
        {
            _input = input;
            _label = label;
            _cameras = cameras;
            _names = names;
        }

        private void Select(int index)
        {
            _index = Mathf.Clamp(index, 0, _cameras.Length - 1);
            for (var i = 0; i < _cameras.Length; i++)
            {
                var active = i == _index;
                _cameras[i].enabled = active;

                // Un solo AudioListener activo.
                if (_cameras[i].TryGetComponent(out AudioListener listener))
                {
                    listener.enabled = active;
                }
            }

            if (_label != null && _names != null && _index < _names.Length)
            {
                _label.text = _names[_index];
            }
        }
    }
}
