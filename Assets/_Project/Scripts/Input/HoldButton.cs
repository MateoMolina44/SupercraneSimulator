using UnityEngine;
using UnityEngine.EventSystems;

namespace SupercraneSimulator.Input
{
    /// <summary>
    /// Botón de UI que mueve un eje de la grúa mientras se mantiene presionado (mouse o touch).
    /// </summary>
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private CraneInput _input;
        [SerializeField] private CraneAxis _axis;
        [SerializeField, Range(-1f, 1f)] private float _value = 1f;

        private bool _pressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            _input.SetUiAxis(_axis, _value);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }

        private void OnDisable()
        {
            Release();
        }

        public void Configure(CraneInput input, CraneAxis axis, float value)
        {
            _input = input;
            _axis = axis;
            _value = value;
        }

        private void Release()
        {
            if (!_pressed)
            {
                return;
            }

            _pressed = false;
            _input.SetUiAxis(_axis, 0f);
        }
    }
}
