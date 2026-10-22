using SupercraneSimulator.Localization;
using SupercraneSimulator.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace SupercraneSimulator.UI
{
    /// <summary>
    /// Panel de configuración dentro de la pausa: toggles, sliders e idioma.
    /// Cada cambio se manda a <see cref="GameSettings"/>, que lo guarda y lo aplica.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.2f, 0.45f, 0.75f, 0.95f);
        private static readonly Color NormalColor = new Color(0.15f, 0.17f, 0.2f, 0.85f);

        [SerializeField] private GameSettings _settings;
        [SerializeField] private Toggle _controlsToggle;
        [SerializeField] private Toggle _telemetryToggle;
        [SerializeField, Tooltip("En el orden de CraneAxis")] private Slider[] _speedSliders;
        [SerializeField] private Text[] _speedValues;
        [SerializeField] private Slider _sensitivitySlider;
        [SerializeField] private Text _sensitivityValue;
        [SerializeField] private Button _spanishButton;
        [SerializeField] private Button _englishButton;
        [SerializeField] private Button _defaultsButton;

        private void Awake()
        {
            _controlsToggle.onValueChanged.AddListener(value => _settings.ShowControls = value);
            _telemetryToggle.onValueChanged.AddListener(value => _settings.ShowTelemetry = value);
            for (var i = 0; i < _speedSliders.Length; i++)
            {
                var joint = i;
                _speedSliders[i].minValue = GameSettings.MinSpeed;
                _speedSliders[i].maxValue = GameSettings.MaxSpeed;
                _speedSliders[i].onValueChanged.AddListener(value => _settings.SetSpeed(joint, value));
            }

            _sensitivitySlider.minValue = GameSettings.MinSensitivity;
            _sensitivitySlider.maxValue = GameSettings.MaxSensitivity;
            _sensitivitySlider.onValueChanged.AddListener(value => _settings.Sensitivity = value);
            _spanishButton.onClick.AddListener(() => _settings.Language = Language.Spanish);
            _englishButton.onClick.AddListener(() => _settings.Language = Language.English);
            _defaultsButton.onClick.AddListener(_settings.ResetToDefaults);
        }

        private void OnEnable()
        {
            _settings.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            _settings.Changed -= Refresh;
        }

        public void Configure(GameSettings settings, Toggle controls, Toggle telemetry, Slider[] speeds, Text[] speedValues,
            Slider sensitivity, Text sensitivityValue, Button spanish, Button english, Button defaults)
        {
            _settings = settings;
            _controlsToggle = controls;
            _telemetryToggle = telemetry;
            _speedSliders = speeds;
            _speedValues = speedValues;
            _sensitivitySlider = sensitivity;
            _sensitivityValue = sensitivityValue;
            _spanishButton = spanish;
            _englishButton = english;
            _defaultsButton = defaults;
        }

        private void Refresh()
        {
            // Sin notificar, para no volver a disparar el guardado.
            _controlsToggle.SetIsOnWithoutNotify(_settings.ShowControls);
            _telemetryToggle.SetIsOnWithoutNotify(_settings.ShowTelemetry);
            for (var i = 0; i < _speedSliders.Length; i++)
            {
                var speed = _settings.GetSpeed(i);
                _speedSliders[i].SetValueWithoutNotify(speed);
                _speedValues[i].text = "×" + Strings.Number(speed, "0.00");
            }

            _sensitivitySlider.SetValueWithoutNotify(_settings.Sensitivity);
            _sensitivityValue.text = "×" + Strings.Number(_settings.Sensitivity, "0.00");

            var english = _settings.Language == Language.English;
            _spanishButton.targetGraphic.color = english ? NormalColor : SelectedColor;
            _englishButton.targetGraphic.color = english ? SelectedColor : NormalColor;
        }
    }
}
