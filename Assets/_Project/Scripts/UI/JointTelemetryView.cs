using System.Text;
using SupercraneSimulator.Crane;
using SupercraneSimulator.Localization;
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

            _labels.Clear().Append("<b>").Append(Strings.Get("telemetry.joint")).AppendLine("</b>");
            _targets.Clear().Append("<b>").Append(Strings.Get("telemetry.target")).AppendLine("</b>");
            _actuals.Clear().Append("<b>").Append(Strings.Get("telemetry.real")).AppendLine("</b>");
            foreach (var joint in joints)
            {
                // El label del joint es una clave de texto.
                _labels.AppendLine(Strings.Get(joint.Label));
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
            return unit == "°" ? Strings.Number(value, "0.0") + unit : Strings.Number(value, "0.00") + " " + unit;
        }
    }
}
