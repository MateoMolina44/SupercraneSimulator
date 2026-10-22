using System.Linq;
using System.Text;
using SupercraneSimulator.Crane;
using SupercraneSimulator.Input;
using SupercraneSimulator.UI;
using UnityEditor;
using UnityEngine;

namespace SupercraneSimulator.Editor
{
    /// <summary>
    /// Revisa en la escena abierta los requisitos verificables de la consigna del TP y escribe
    /// un reporte en la consola. Menú: Supercrane → Validate Consigna.
    /// </summary>
    public static class ConsignaValidator
    {
        private const float MaxMassRatio = 3f;
        private const float AnchorTolerance = 0.001f;

        [MenuItem("Supercrane/Validate Consigna")]
        public static void Validate()
        {
            var report = new StringBuilder("[Supercrane] Validación de la consigna\n");
            var allPassed = true;

            void Check(bool passed, string message)
            {
                allPassed &= passed;
                report.Append(passed ? "  OK    " : "  FALLA ").AppendLine(message);
            }

            var bodies = Object.FindObjectsByType<ArticulationBody>(FindObjectsSortMode.None);
            var joints = bodies.Where(b => !b.isRoot).ToArray();

            Check(bodies.Length >= 6 && bodies.Length <= 10, $"Articulation Bodies: {bodies.Length} (entre 6 y 10)");

            var jointTypes = joints.Select(b => b.jointType).Distinct().ToArray();
            Check(jointTypes.Length >= 3, $"Tipos de joint: {jointTypes.Length} ({string.Join(", ", jointTypes)})");

            var driven = joints.Count(b => b.TryGetComponent<DrivenJoint>(out _));
            var passive = joints.Length - driven;
            Check(driven >= 4, $"Joints con drive: {driven} (al menos 4)");
            Check(passive >= 1, $"Partes pasivas: {passive} (al menos 1)");

            var scaled = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Where(t => t.localScale != Vector3.one && !(t is RectTransform))
                .Select(t => t.name)
                .ToArray();
            Check(scaled.Length == 0, scaled.Length == 0
                ? "Escala (1,1,1) en toda la escena"
                : $"Objetos con escala distinta de (1,1,1): {string.Join(", ", scaled)}");

            // En reposo, el ancla del padre y la propia tienen que coincidir (anclas en los pivotes).
            var misplaced = joints.Where(b =>
            {
                var anchor = b.transform.TransformPoint(b.anchorPosition);
                var parentAnchor = b.transform.parent.TransformPoint(b.parentAnchorPosition);
                return Vector3.Distance(anchor, parentAnchor) > AnchorTolerance;
            }).Select(b => b.name).ToArray();
            Check(misplaced.Length == 0, misplaced.Length == 0
                ? "Anclas en los pivotes (ancla y ancla del padre coinciden)"
                : $"Anclas desalineadas: {string.Join(", ", misplaced)}");

            // Masas parecidas entre cuerpos conectados (la raíz inmóvil no cuenta).
            var worstRatio = 1f;
            var worstPair = "";
            foreach (var body in joints)
            {
                var parent = body.transform.parent.GetComponentInParent<ArticulationBody>();
                if (parent == null || parent.immovable)
                {
                    continue;
                }

                var ratio = Mathf.Max(body.mass, parent.mass) / Mathf.Min(body.mass, parent.mass);
                if (ratio > worstRatio)
                {
                    worstRatio = ratio;
                    worstPair = $"{parent.name} {parent.mass:0} kg / {body.name} {body.mass:0} kg";
                }
            }

            Check(worstRatio <= MaxMassRatio, $"Masas parecidas: peor relación {worstRatio:0.0}:1 ({worstPair}), máximo {MaxMassRatio:0}:1");

            Check(Object.FindAnyObjectByType<CraneController>() != null, "Controlador: CraneController (targets en FixedUpdate)");
            Check(Object.FindAnyObjectByType<JointTelemetryView>() != null, "Pantalla con target y valor real por joint (JointTelemetryView)");
            var holdButtons = Object.FindObjectsByType<HoldButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            Check(Object.FindAnyObjectByType<CraneInput>() != null && holdButtons > 0,
                $"Entrada por Input System (CraneInput) y por UI ({holdButtons} botones)");

            report.AppendLine(allPassed ? "Resultado: cumple todo lo verificable." : "Resultado: hay requisitos sin cumplir.");
            if (allPassed)
            {
                Debug.Log(report.ToString());
            }
            else
            {
                Debug.LogError(report.ToString());
            }
        }
    }
}
