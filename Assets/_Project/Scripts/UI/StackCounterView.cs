using SupercraneSimulator.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace SupercraneSimulator.UI
{
    /// <summary>
    /// Muestra cuántas cargas hay apiladas en la zona y el estado del spreader.
    /// </summary>
    public sealed class StackCounterView : MonoBehaviour
    {
        [SerializeField] private StackZone _zone;
        [SerializeField] private SpreaderGrabber _grabber;
        [SerializeField] private Text _counterText;
        [SerializeField] private Text _spreaderText;

        private void OnEnable()
        {
            _zone.CountChanged += ShowCount;
            _grabber.StateChanged += ShowState;
        }

        private void OnDisable()
        {
            _zone.CountChanged -= ShowCount;
            _grabber.StateChanged -= ShowState;
        }

        private void Start()
        {
            ShowCount(_zone.Count, _zone.Total);
            ShowState(_grabber.State);
        }

        public void Configure(StackZone zone, SpreaderGrabber grabber, Text counter, Text spreader)
        {
            _zone = zone;
            _grabber = grabber;
            _counterText = counter;
            _spreaderText = spreader;
        }

        private void ShowCount(int count, int total)
        {
            _counterText.text = $"Apiladas: {count} / {total}";
        }

        private void ShowState(GrabState state)
        {
            _spreaderText.text = state switch
            {
                GrabState.Grabbed => "Spreader: <color=#77ff77>enganchado</color> (Espacio suelta)",
                GrabState.Ready => "Spreader: <color=#ffcc33>carga lista</color> (Espacio engancha)",
                _ => "Spreader: <color=#ff8080>sin carga</color>"
            };
        }
    }
}
