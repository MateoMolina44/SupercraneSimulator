using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SupercraneSimulator.Input
{
    /// <summary>
    /// Combina el teclado (Input System) con los botones de UI en un eje -1..1 por joint
    /// y eventos para las acciones de un toque.
    /// </summary>
    public sealed class CraneInput : MonoBehaviour
    {
        private static readonly int AxisCount = Enum.GetValues(typeof(CraneAxis)).Length;

        private readonly InputAction[] _axisActions = new InputAction[AxisCount];
        private readonly float[] _uiAxes = new float[AxisCount];
        private InputAction _grab;
        private InputAction _switchCamera;
        private InputAction _pause;

        public event Action GrabPressed;
        public event Action CameraPressed;
        public event Action PausePressed;

        /// <summary>
        /// Con la pausa abierta la grúa no recibe entrada (ejes en 0, sin enganche ni cambio de
        /// cámara). La tecla de pausa sigue funcionando para poder salir.
        /// </summary>
        public bool Locked { get; set; }

        private void Awake()
        {
            _axisActions[(int)CraneAxis.Gantry] = CreateAxis("Gantry", "<Keyboard>/a", "<Keyboard>/d");
            _axisActions[(int)CraneAxis.Trolley] = CreateAxis("Trolley", "<Keyboard>/s", "<Keyboard>/w");
            // R sube el spreader, que es reducir el izaje.
            _axisActions[(int)CraneAxis.Hoist] = CreateAxis("Hoist", "<Keyboard>/r", "<Keyboard>/f");
            _axisActions[(int)CraneAxis.Spreader] = CreateAxis("Spreader", "<Keyboard>/q", "<Keyboard>/e");

            _grab = new InputAction("Grab", InputActionType.Button, "<Keyboard>/space");
            _switchCamera = new InputAction("SwitchCamera", InputActionType.Button, "<Keyboard>/c");
            _pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/p");

            _grab.performed += _ => RequestGrab();
            _switchCamera.performed += _ => RequestCameraSwitch();
            _pause.performed += _ => PausePressed?.Invoke();
        }

        private void OnEnable()
        {
            foreach (var action in _axisActions)
            {
                action.Enable();
            }

            _grab.Enable();
            _switchCamera.Enable();
            _pause.Enable();
        }

        private void OnDisable()
        {
            foreach (var action in _axisActions)
            {
                action.Disable();
            }

            _grab.Disable();
            _switchCamera.Disable();
            _pause.Disable();
        }

        private void OnDestroy()
        {
            foreach (var action in _axisActions)
            {
                action.Dispose();
            }

            _grab.Dispose();
            _switchCamera.Dispose();
            _pause.Dispose();
        }

        public float GetAxis(CraneAxis axis)
        {
            if (Locked)
            {
                return 0f;
            }

            var keyboard = _axisActions[(int)axis].ReadValue<float>();
            return Mathf.Clamp(keyboard + _uiAxes[(int)axis], -1f, 1f);
        }

        /// <summary>Lo llaman los botones de UI mientras están presionados.</summary>
        public void SetUiAxis(CraneAxis axis, float value)
        {
            _uiAxes[(int)axis] = Mathf.Clamp(value, -1f, 1f);
        }

        public void RequestGrab()
        {
            if (!Locked)
            {
                GrabPressed?.Invoke();
            }
        }

        public void RequestCameraSwitch()
        {
            if (!Locked)
            {
                CameraPressed?.Invoke();
            }
        }

        public void RequestPause() => PausePressed?.Invoke();

        private static InputAction CreateAxis(string name, string negative, string positive)
        {
            var action = new InputAction(name, InputActionType.Value, expectedControlType: "Axis");
            action.AddCompositeBinding("1DAxis")
                .With("Negative", negative)
                .With("Positive", positive);
            return action;
        }
    }
}
