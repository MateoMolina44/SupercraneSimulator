using UnityEngine;

namespace SupercraneSimulator.Gameplay
{
    public enum LoadKind
    {
        Container,
        Crate
    }

    /// <summary>
    /// Carga que se puede enganchar con el spreader: un contenedor o una caja (Rigidbody común).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Load : MonoBehaviour
    {
        [SerializeField] private LoadKind _kind;

        public LoadKind Kind => _kind;
        public Rigidbody Body { get; private set; }
        public bool IsGrabbed { get; set; }

        /// <summary>
        /// Cada cuántos grados la carga se ve igual desde arriba: 180 para un contenedor
        /// (rectangular) y 90 para una caja (cuadrada).
        /// </summary>
        public float AlignmentSymmetry => _kind == LoadKind.Container ? 180f : 90f;

        [SerializeField, Tooltip("Si cae por debajo de esta altura (al agua), vuelve a su lugar en el barco")]
        private float _respawnBelowY = -8f;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        private void FixedUpdate()
        {
            // El agua no tiene collider: una carga que se suelta fuera del muelle o del barco se hunde.
            if (!IsGrabbed && Body.position.y < _respawnBelowY)
            {
                Body.linearVelocity = Vector3.zero;
                Body.angularVelocity = Vector3.zero;
                Body.position = _startPosition;
                Body.rotation = _startRotation;
            }
        }

        public void Configure(LoadKind kind)
        {
            _kind = kind;
        }
    }
}
