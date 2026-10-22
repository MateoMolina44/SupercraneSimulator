using SupercraneSimulator.Input;
using UnityEngine;

namespace SupercraneSimulator.Crane
{
    /// <summary>
    /// Lee la entrada en Update y la aplica como target de cada drive en FixedUpdate.
    /// </summary>
    public sealed class CraneController : MonoBehaviour
    {
        [SerializeField] private CraneInput _input;

        [Header("Joints con drive")]
        [SerializeField] private DrivenJoint _gantry;
        [SerializeField] private DrivenJoint _trolley;
        [SerializeField] private DrivenJoint _hoist;
        [SerializeField] private DrivenJoint _spreader;

        private DrivenJoint[] _joints;
        private readonly float[] _axes = new float[4];

        /// <summary>Joints en el orden de <see cref="CraneAxis"/>, para la telemetría.</summary>
        public DrivenJoint[] Joints => _joints;

        private void Awake()
        {
            _joints = new[] { _gantry, _trolley, _hoist, _spreader };
        }

        private void Update()
        {
            for (var i = 0; i < _axes.Length; i++)
            {
                _axes[i] = _input.GetAxis((CraneAxis)i);
            }
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            for (var i = 0; i < _joints.Length; i++)
            {
                _joints[i].Step(_axes[i], deltaTime);
            }
        }

        /// <summary>Lo usa el SceneBuilder para conectar las referencias.</summary>
        public void Configure(CraneInput input, DrivenJoint gantry, DrivenJoint trolley, DrivenJoint hoist, DrivenJoint spreader)
        {
            _input = input;
            _gantry = gantry;
            _trolley = trolley;
            _hoist = hoist;
            _spreader = spreader;
        }
    }
}
