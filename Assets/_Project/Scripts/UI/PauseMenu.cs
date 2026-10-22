using SupercraneSimulator.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SupercraneSimulator.UI
{
    /// <summary>
    /// Pausa con la tecla P (Esc no se usa: en WebGL saca al navegador de pantalla completa).
    /// Congela la simulación y muestra el menú; desde ahí se ven los controles o se reinicia.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private CraneInput _input;
        [SerializeField] private GameObject _root;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _controlsPanel;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            SetPaused(false);
        }

        private void OnEnable()
        {
            _input.PausePressed += Toggle;
        }

        private void OnDisable()
        {
            _input.PausePressed -= Toggle;
        }

        public void Toggle()
        {
            SetPaused(!IsPaused);
        }

        public void Resume()
        {
            SetPaused(false);
        }

        public void ShowControls()
        {
            _mainPanel.SetActive(false);
            _controlsPanel.SetActive(true);
        }

        public void ShowMain()
        {
            _controlsPanel.SetActive(false);
            _mainPanel.SetActive(true);
        }

        public void Restart()
        {
            SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Configure(CraneInput input, GameObject root, GameObject mainPanel, GameObject controlsPanel)
        {
            _input = input;
            _root = root;
            _mainPanel = mainPanel;
            _controlsPanel = controlsPanel;
        }

        private void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            _input.Locked = paused;
            _root.SetActive(paused);
            if (paused)
            {
                ShowMain();
            }
        }
    }
}
