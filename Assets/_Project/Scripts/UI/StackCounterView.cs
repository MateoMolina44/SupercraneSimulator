using SupercraneSimulator.Gameplay;
using SupercraneSimulator.Localization;
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
            Strings.Changed += Refresh;
        }

        private void OnDisable()
        {
            _zone.CountChanged -= ShowCount;
            _grabber.StateChanged -= ShowState;
            Strings.Changed -= Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        public void Configure(StackZone zone, SpreaderGrabber grabber, Text counter, Text spreader)
        {
            _zone = zone;
            _grabber = grabber;
            _counterText = counter;
            _spreaderText = spreader;
        }

        private void Refresh()
        {
            ShowCount(_zone.Count, _zone.Total);
            ShowState(_grabber.State);
        }

        private void ShowCount(int count, int total)
        {
            _counterText.text = count >= total && total > 0
                ? Strings.Format("hud.complete", count, total)
                : Strings.Format("hud.stacked", count, total);
        }

        private void ShowState(GrabState state)
        {
            _spreaderText.text = Strings.Get(state switch
            {
                GrabState.Grabbed => "hud.spreader.grabbed",
                GrabState.Ready => "hud.spreader.ready",
                _ => "hud.spreader.empty"
            });
        }
    }
}
