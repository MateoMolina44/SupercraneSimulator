using UnityEngine;

namespace SupercraneSimulator.Crane
{
    /// <summary>
    /// Dibuja un cable entre dos puntos con un LineRenderer, sin escalar transforms.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class RopeLine : MonoBehaviour
    {
        [SerializeField] private Transform _from;
        [SerializeField] private Transform _to;

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.useWorldSpace = true;
        }

        private void LateUpdate()
        {
            _line.SetPosition(0, _from.position);
            _line.SetPosition(1, _to.position);
        }

        public void Configure(Transform from, Transform to)
        {
            _from = from;
            _to = to;
        }
    }
}
