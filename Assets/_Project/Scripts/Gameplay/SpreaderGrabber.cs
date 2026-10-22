using System;
using SupercraneSimulator.Input;
using UnityEngine;

namespace SupercraneSimulator.Gameplay
{
    public enum GrabState
    {
        Empty,
        Ready,
        Grabbed
    }

    /// <summary>
    /// Engancha la carga que está debajo del spreader con un FixedJoint creado en runtime
    /// (no suma Articulation Bodies). Solo engancha si la carga está cerca y alineada.
    /// </summary>
    [RequireComponent(typeof(ArticulationBody))]
    public sealed class SpreaderGrabber : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private CraneInput _input;

        [Header("Detección (coordenadas locales del spreader)")]
        [SerializeField, Tooltip("Centro de la caja donde se buscan cargas, debajo del marco")]
        private Vector3 _probeCenter = new Vector3(0f, -1f, 0f);
        [SerializeField] private Vector3 _probeHalfExtents = new Vector3(2.8f, 0.5f, 1.1f);
        [SerializeField, Range(0f, 45f), Tooltip("Diferencia de giro máxima con la carga, en grados")]
        private float _maxAngle = 15f;

        [Header("Indicador")]
        [SerializeField] private Renderer[] _lights;
        [SerializeField] private Color _emptyColor = new Color(0.85f, 0.1f, 0.1f);
        [SerializeField] private Color _readyColor = new Color(1f, 0.8f, 0.1f);
        [SerializeField] private Color _grabbedColor = new Color(0.1f, 0.85f, 0.2f);

        private readonly Collider[] _hits = new Collider[16];
        private ArticulationBody _body;
        private MaterialPropertyBlock _block;
        private Load _candidate;
        private Load _grabbed;
        private FixedJoint _joint;
        private GrabState _state = (GrabState)(-1);

        public GrabState State => _state;
        public Load Grabbed => _grabbed;

        public event Action<GrabState> StateChanged;

        private void Awake()
        {
            _body = GetComponent<ArticulationBody>();
            _block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _input.GrabPressed += Toggle;
        }

        private void OnDisable()
        {
            _input.GrabPressed -= Toggle;
        }

        private void FixedUpdate()
        {
            // Si el joint se rompió o la carga desapareció, se considera suelta.
            if (_grabbed != null && _joint == null)
            {
                Release();
            }

            _candidate = _grabbed == null ? FindCandidate() : null;
            SetState(_grabbed != null ? GrabState.Grabbed : _candidate != null ? GrabState.Ready : GrabState.Empty);
        }

        /// <summary>Engancha si hay una carga válida debajo, o suelta la que lleva.</summary>
        public void Toggle()
        {
            if (_grabbed != null)
            {
                Release();
            }
            else
            {
                var load = FindCandidate();
                if (load != null)
                {
                    Attach(load);
                }
            }
        }

        public void Configure(CraneInput input, Renderer[] lights)
        {
            _input = input;
            _lights = lights;
        }

        private void Attach(Load load)
        {
            _grabbed = load;
            _grabbed.IsGrabbed = true;
            _joint = load.gameObject.AddComponent<FixedJoint>();
            _joint.connectedArticulationBody = _body;
            SetState(GrabState.Grabbed);
        }

        private void Release()
        {
            if (_joint != null)
            {
                Destroy(_joint);
            }

            if (_grabbed != null)
            {
                _grabbed.IsGrabbed = false;
                _grabbed.Body.WakeUp();
            }

            _joint = null;
            _grabbed = null;
        }

        private Load FindCandidate()
        {
            var center = transform.TransformPoint(_probeCenter);
            var count = Physics.OverlapBoxNonAlloc(center, _probeHalfExtents, _hits, transform.rotation,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            Load best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var load = _hits[i].GetComponentInParent<Load>();
                if (load == null || load.IsGrabbed || !IsAligned(load))
                {
                    continue;
                }

                var offset = load.transform.position - center;
                var horizontalDistance = new Vector2(offset.x, offset.z).sqrMagnitude;
                if (horizontalDistance < bestDistance)
                {
                    best = load;
                    bestDistance = horizontalDistance;
                }
            }

            return best;
        }

        private bool IsAligned(Load load)
        {
            // Ángulo entre el eje largo del spreader y el de la carga, visto desde arriba.
            var angle = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(transform.right, Vector3.up),
                Vector3.ProjectOnPlane(load.transform.right, Vector3.up),
                Vector3.up);
            var symmetry = load.AlignmentSymmetry;
            var difference = Mathf.Abs(Mathf.Repeat(angle + symmetry / 2f, symmetry) - symmetry / 2f);
            return difference <= _maxAngle;
        }

        private void SetState(GrabState state)
        {
            if (state == _state)
            {
                return;
            }

            _state = state;
            var color = state switch
            {
                GrabState.Grabbed => _grabbedColor,
                GrabState.Ready => _readyColor,
                _ => _emptyColor
            };
            _block.SetColor(BaseColorId, color);
            foreach (var light in _lights)
            {
                light.SetPropertyBlock(_block);
            }

            StateChanged?.Invoke(state);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(_probeCenter), transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, _probeHalfExtents * 2f);
        }
    }
}
