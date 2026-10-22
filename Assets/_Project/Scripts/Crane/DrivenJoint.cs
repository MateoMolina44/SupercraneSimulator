using UnityEngine;

namespace SupercraneSimulator.Crane
{
    /// <summary>
    /// Envuelve un ArticulationBody de un solo eje con drive (Prismatic o Revolute).
    /// Integra un target incremental dentro del rango y expone target y valor real.
    /// </summary>
    [RequireComponent(typeof(ArticulationBody))]
    public sealed class DrivenJoint : MonoBehaviour
    {
        [SerializeField, Tooltip("Nombre que se muestra en la telemetría")]
        private string _label = "Joint";

        [Header("Rango (m en Prismatic, grados en Revolute)")]
        [SerializeField] private float _min;
        [SerializeField] private float _max = 1f;
        [SerializeField, Tooltip("Target inicial, dentro del rango")]
        private float _initialTarget;
        [SerializeField, Tooltip("Valor mostrado cuando jointPosition es 0 (posición en la que se armó el cuerpo)")]
        private float _offset;

        [Header("Velocidad")]
        [SerializeField, Min(0f), Tooltip("Unidades por segundo con la entrada al máximo")]
        private float _speed = 1f;
        [SerializeField, Min(0.01f), Tooltip("Segundos para llegar a la velocidad máxima (evita latigazos en el cable)")]
        private float _rampTime = 0.8f;

        private ArticulationBody _body;
        private float _target;
        private float _targetSpeed;
        private float _speedMultiplier = 1f;

        public string Label => _label;
        public bool IsAngular { get; private set; }
        public string Unit => IsAngular ? "°" : "m";
        public float Min => _min;
        public float Max => _max;
        public float Target => _target;

        /// <summary>Posición real del joint, en metros o grados.</summary>
        public float Actual
        {
            get
            {
                if (_body == null || _body.dofCount == 0)
                {
                    return 0f;
                }

                // jointPosition está en radianes para Revolute.
                var value = _body.jointPosition[0];
                return (IsAngular ? value * Mathf.Rad2Deg : value) + _offset;
            }
        }

        public float SpeedMultiplier
        {
            get => _speedMultiplier;
            set => _speedMultiplier = Mathf.Max(0f, value);
        }

        private void Awake()
        {
            _body = GetComponent<ArticulationBody>();
            IsAngular = _body.jointType == ArticulationJointType.RevoluteJoint;
            _target = Mathf.Clamp(_initialTarget, _min, _max);
            ApplyTarget();
        }

        /// <summary>Mueve el target según la entrada (-1..1). Llamar solo desde FixedUpdate.</summary>
        public void Step(float input, float deltaTime)
        {
            var maxSpeed = _speed * _speedMultiplier;
            var acceleration = maxSpeed / _rampTime;
            var desiredSpeed = input * maxSpeed;

            // Cerca de un tope, la velocidad se limita a la que permite frenar justo ahí.
            var distanceToLimit = desiredSpeed > 0f ? _max - _target : _target - _min;
            var brakingSpeed = Mathf.Sqrt(2f * acceleration * Mathf.Max(0f, distanceToLimit));
            desiredSpeed = Mathf.Clamp(desiredSpeed, -brakingSpeed, brakingSpeed);

            _targetSpeed = Mathf.MoveTowards(_targetSpeed, desiredSpeed, acceleration * deltaTime);
            if (Mathf.Approximately(_targetSpeed, 0f))
            {
                return;
            }

            var next = _target + _targetSpeed * deltaTime;
            _target = Mathf.Clamp(next, _min, _max);
            if (!Mathf.Approximately(next, _target))
            {
                // Llegó al tope: se frena en seco para no acumular velocidad contra el límite.
                _targetSpeed = 0f;
            }

            ApplyTarget();
        }

        /// <summary>Lo usa el SceneBuilder para dejar el joint configurado en la escena.</summary>
        public void Configure(string label, float min, float max, float initialTarget, float offset, float speed, float rampTime)
        {
            _label = label;
            _min = min;
            _max = max;
            _initialTarget = initialTarget;
            _offset = offset;
            _speed = speed;
            _rampTime = rampTime;
        }

        private void ApplyTarget()
        {
            // Todos los joints de la grúa usan el eje X del ancla. El drive trabaja sin offset.
            _body.SetDriveTarget(ArticulationDriveAxis.X, _target - _offset);
        }
    }
}
