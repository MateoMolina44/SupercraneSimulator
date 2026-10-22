using System;
using SupercraneSimulator.Input;
using UnityEngine;

namespace SupercraneSimulator.Crane
{
    /// <summary>
    /// Inclina y gira un joystick de la cabina según la entrada del jugador.
    /// Se rota el pivote (base del joystick); el palo y la perilla son hijos.
    /// </summary>
    public sealed class CabinJoystick : MonoBehaviour
    {
        [Serializable]
        private struct Binding
        {
            public bool Enabled;
            public CraneAxis Axis;
            [Tooltip("Grados con la entrada al máximo; el signo define el sentido")]
            public float Degrees;
        }

        [SerializeField] private CraneInput _input;
        [SerializeField, Tooltip("Inclinación adelante/atrás (eje X local)")]
        private Binding _pitch;
        [SerializeField, Tooltip("Giro sobre el propio palo (eje Y local)")]
        private Binding _twist;
        [SerializeField, Tooltip("Inclinación a los costados (eje Z local)")]
        private Binding _roll;
        [SerializeField, Min(0.01f), Tooltip("Segundos que tarda en seguir a la entrada")]
        private float _smoothTime = 0.08f;

        private Quaternion _restRotation;
        private Vector3 _angles;
        private Vector3 _velocity;

        private void Awake()
        {
            _restRotation = transform.localRotation;
        }

        private void Update()
        {
            var target = new Vector3(Read(_pitch), Read(_twist), Read(_roll));
            _angles = Vector3.SmoothDamp(_angles, target, ref _velocity, _smoothTime);
            transform.localRotation = _restRotation * Quaternion.Euler(_angles);
        }

        /// <summary>Lo usa el SceneBuilder. Pasar null en un eje para no usarlo.</summary>
        public void Configure(CraneInput input, (CraneAxis axis, float degrees)? pitch,
            (CraneAxis axis, float degrees)? twist, (CraneAxis axis, float degrees)? roll)
        {
            _input = input;
            _pitch = ToBinding(pitch);
            _twist = ToBinding(twist);
            _roll = ToBinding(roll);
        }

        private float Read(Binding binding)
        {
            return binding.Enabled ? _input.GetAxis(binding.Axis) * binding.Degrees : 0f;
        }

        private static Binding ToBinding((CraneAxis axis, float degrees)? value)
        {
            return value.HasValue
                ? new Binding { Enabled = true, Axis = value.Value.axis, Degrees = value.Value.degrees }
                : default;
        }
    }
}
