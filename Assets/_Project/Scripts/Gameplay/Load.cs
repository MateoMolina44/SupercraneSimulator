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

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public void Configure(LoadKind kind)
        {
            _kind = kind;
        }
    }
}
