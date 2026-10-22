using UnityEngine;
using UnityEngine.UI;

namespace SupercraneSimulator.Localization
{
    /// <summary>
    /// Pone en un Text el texto de una clave y lo actualiza cuando cambia el idioma.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;

        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            Strings.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Strings.Changed -= Refresh;
        }

        public void Configure(string key)
        {
            _key = key;
        }

        private void Refresh()
        {
            _text.text = Strings.Get(_key);
        }
    }
}
