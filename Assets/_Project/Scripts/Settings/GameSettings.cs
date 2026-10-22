using System;
using SupercraneSimulator.Cameras;
using SupercraneSimulator.Crane;
using SupercraneSimulator.Localization;
using UnityEngine;

namespace SupercraneSimulator.Settings
{
    /// <summary>
    /// Opciones del jugador. Se guardan con PlayerPrefs (en WebGL quedan en el navegador) y se
    /// aplican apenas cambian: paneles visibles, velocidad de cada joint, sensibilidad e idioma.
    /// </summary>
    public sealed class GameSettings : MonoBehaviour
    {
        public const float MinSpeed = 0.5f;
        public const float MaxSpeed = 2f;
        public const float MinSensitivity = 0.25f;
        public const float MaxSensitivity = 3f;

        private const string Prefix = "supercrane.";

        [Header("Dónde se aplica")]
        [SerializeField] private GameObject _onScreenControls;
        [SerializeField] private GameObject _telemetry;
        [SerializeField, Tooltip("En el orden de CraneAxis: pórtico, carro, izaje, spreader")]
        private DrivenJoint[] _joints;
        [SerializeField] private OrbitCamera _orbitCamera;

        private bool _showControls;
        private bool _showTelemetry;
        private float[] _speeds;
        private float _sensitivity;

        public event Action Changed;

        public bool ShowControls
        {
            get => _showControls;
            set { _showControls = value; SaveAndApply(); }
        }

        public bool ShowTelemetry
        {
            get => _showTelemetry;
            set { _showTelemetry = value; SaveAndApply(); }
        }

        public float Sensitivity
        {
            get => _sensitivity;
            set { _sensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity); SaveAndApply(); }
        }

        public Language Language
        {
            get => Strings.Current;
            set { Strings.Current = value; SaveAndApply(); }
        }

        private void Awake()
        {
            _speeds = new float[_joints.Length];
            Load();
            Apply();
        }

        /// <summary>Multiplicador de velocidad de un joint (1 = la velocidad de diseño).</summary>
        public float GetSpeed(int joint) => _speeds[joint];

        public void SetSpeed(int joint, float value)
        {
            _speeds[joint] = Mathf.Clamp(value, MinSpeed, MaxSpeed);
            SaveAndApply();
        }

        public void ResetToDefaults()
        {
            _showControls = true;
            _showTelemetry = true;
            _sensitivity = 1f;
            for (var i = 0; i < _speeds.Length; i++)
            {
                _speeds[i] = 1f;
            }

            Strings.Current = Language.Spanish;
            SaveAndApply();
        }

        public void Configure(GameObject onScreenControls, GameObject telemetry, DrivenJoint[] joints, OrbitCamera orbitCamera)
        {
            _onScreenControls = onScreenControls;
            _telemetry = telemetry;
            _joints = joints;
            _orbitCamera = orbitCamera;
        }

        private void Load()
        {
            _showControls = PlayerPrefs.GetInt(Prefix + "showControls", 1) == 1;
            _showTelemetry = PlayerPrefs.GetInt(Prefix + "showTelemetry", 1) == 1;
            _sensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + "sensitivity", 1f), MinSensitivity, MaxSensitivity);
            for (var i = 0; i < _speeds.Length; i++)
            {
                _speeds[i] = Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + "speed" + i, 1f), MinSpeed, MaxSpeed);
            }

            Strings.Current = (Language)PlayerPrefs.GetInt(Prefix + "language", (int)Language.Spanish);
        }

        private void SaveAndApply()
        {
            PlayerPrefs.SetInt(Prefix + "showControls", _showControls ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "showTelemetry", _showTelemetry ? 1 : 0);
            PlayerPrefs.SetFloat(Prefix + "sensitivity", _sensitivity);
            for (var i = 0; i < _speeds.Length; i++)
            {
                PlayerPrefs.SetFloat(Prefix + "speed" + i, _speeds[i]);
            }

            PlayerPrefs.SetInt(Prefix + "language", (int)Strings.Current);
            // En WebGL hay que guardar explícitamente para que quede en el navegador.
            PlayerPrefs.Save();
            Apply();
        }

        private void Apply()
        {
            _onScreenControls.SetActive(_showControls);
            _telemetry.SetActive(_showTelemetry);
            for (var i = 0; i < _joints.Length; i++)
            {
                _joints[i].SpeedMultiplier = _speeds[i];
            }

            _orbitCamera.Sensitivity = _sensitivity;
            Changed?.Invoke();
        }
    }
}
