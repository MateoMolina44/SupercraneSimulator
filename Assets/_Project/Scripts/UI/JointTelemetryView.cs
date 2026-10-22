using System.Text;
using SupercraneSimulator.Crane;
using UnityEngine;
using UnityEngine.UI;

namespace SupercraneSimulator.UI
{
    /// <summary>
    /// Tabla de telemetría: target pedido y valor real leído de jointPosition, por joint.
    /// Usa una columna de texto por dato porque la fuente no es monoespaciada.
    /// </summary>
    public sealed class JointTelemetryView : MonoBehaviour
    {
        [SerializeField] private CraneController _crane;
        [SerializeField] private Text _labelsText;
        [SerializeField] private Text _targetsText;
        [SerializeField] private Text _actualsText;

        private readonly StringBuilder _labels = new StringBuilder(128);
        private readonly StringBuilder _targets = new StringBuilder(128);
        private readonly StringBuilder _actuals = new StringBuilder(128);

        private void LateUpdate()
        {
            var joints = _crane.Joints;
            if (joints == null)
            {
                return;
            }

            _labels.Clear().AppendLine("<b>Joint</b>");
            _targets.Clear().AppendLine("<b>Target</b>");
            _actuals.Clear().AppendLine("<b>Real</b>");
            foreach (var joint in joints)
            {
                _labels.AppendLine(joint.Label);
                _targets.AppendLine(Format(joint.Target, joint.Unit));
                _actuals.AppendLine(Format(joint.Actual, joint.Unit));
            }

            _labelsText.text = _labels.ToString();
            _targetsText.text = _targets.ToString();
            _actualsText.text = _actuals.ToString();
        }

        public void Configure(CraneController crane, Text labels, Text targets, Text actuals)
        {
            _crane = crane;
            _labelsText = labels;
            _targetsText = targets;
            _actualsText = actuals;
        }

        private static string Format(float value, string unit)
        {
            return unit == "°" ? $"{value:0.0}{unit}" : $"{value:0.00} {unit}";
        }
    }
}
