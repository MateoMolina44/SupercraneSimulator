using System;
using UnityEngine;

namespace SupercraneSimulator.Gameplay
{
    /// <summary>
    /// Zona de apilado en el muelle. Una carga cuenta si su centro está dentro del volumen,
    /// no está enganchada y estuvo quieta durante <see cref="_settleTime"/> segundos.
    /// </summary>
    public sealed class StackZone : MonoBehaviour
    {
        [SerializeField] private Load[] _loads;
        [SerializeField, Tooltip("Tamaño del volumen en metros; la base queda en el origen del objeto")]
        private Vector3 _size = new Vector3(14f, 15f, 5f);
        [SerializeField, Min(0f)] private float _settleTime = 1f;
        [SerializeField, Min(0f), Tooltip("Velocidad por debajo de la cual una carga se considera quieta (m/s)")]
        private float _restSpeed = 0.1f;

        private float[] _restTimers;
        private int _count = -1;

        public int Count => Mathf.Max(0, _count);
        public int Total => _loads.Length;

        public event Action<int, int> CountChanged;

        private void Awake()
        {
            _restTimers = new float[_loads.Length];
        }

        private void FixedUpdate()
        {
            var count = 0;
            for (var i = 0; i < _loads.Length; i++)
            {
                var load = _loads[i];
                var resting = load != null && !load.IsGrabbed && Contains(load.Body.worldCenterOfMass)
                    && load.Body.linearVelocity.magnitude < _restSpeed;
                _restTimers[i] = resting ? _restTimers[i] + Time.fixedDeltaTime : 0f;
                if (_restTimers[i] >= _settleTime)
                {
                    count++;
                }
            }

            if (count != _count)
            {
                _count = count;
                CountChanged?.Invoke(count, Total);
            }
        }

        public void Configure(Load[] loads, Vector3 size)
        {
            _loads = loads;
            _size = size;
        }

        private bool Contains(Vector3 point)
        {
            var local = transform.InverseTransformPoint(point);
            return Mathf.Abs(local.x) <= _size.x / 2f
                && local.y >= 0f && local.y <= _size.y
                && Mathf.Abs(local.z) <= _size.z / 2f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, _size.y / 2f, 0f), _size);
        }
    }
}
