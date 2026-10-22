using System;
using System.Collections.Generic;
using System.Globalization;

namespace SupercraneSimulator.Localization
{
    public enum Language
    {
        Spanish,
        English
    }

    /// <summary>
    /// Textos de la interfaz en español e inglés. La tabla vive en código para no depender de
    /// un asset que se pueda desincronizar con la escena generada.
    /// </summary>
    public static class Strings
    {
        private static readonly Dictionary<string, (string Es, string En)> Table = new Dictionary<string, (string, string)>
        {
            // HUD
            ["hud.camera.external"] = ("Cámara: Externa (C)", "Camera: External (C)"),
            ["hud.camera.cabin"] = ("Cámara: Cabina (C)", "Camera: Cabin (C)"),
            ["hud.camera.crane"] = ("Cámara: Grúa (C)", "Camera: Crane (C)"),
            ["hud.pause"] = ("Pausa (P)", "Pause (P)"),
            ["hud.stacked"] = ("Apiladas: {0} / {1}", "Stacked: {0} / {1}"),
            ["hud.complete"] = ("<color=#77ff77>¡Barco descargado! {0} / {1}</color>", "<color=#77ff77>Ship unloaded! {0} / {1}</color>"),
            ["hud.spreader.empty"] = ("Spreader: <color=#ff8080>sin carga</color>", "Spreader: <color=#ff8080>empty</color>"),
            ["hud.spreader.ready"] = ("Spreader: <color=#ffcc33>carga lista</color> (Espacio engancha)", "Spreader: <color=#ffcc33>load ready</color> (Space grabs)"),
            ["hud.spreader.grabbed"] = ("Spreader: <color=#77ff77>enganchado</color> (Espacio suelta)", "Spreader: <color=#77ff77>grabbed</color> (Space releases)"),

            // Telemetría
            ["telemetry.joint"] = ("Joint", "Joint"),
            ["telemetry.target"] = ("Target", "Target"),
            ["telemetry.real"] = ("Real", "Actual"),
            ["joint.gantry"] = ("Pórtico", "Gantry"),
            ["joint.trolley"] = ("Carro", "Trolley"),
            ["joint.hoist"] = ("Izaje", "Hoist"),
            ["joint.spreader"] = ("Spreader", "Spreader"),

            // Controles en pantalla
            ["ctrl.gantry.negative"] = ("◄  A", "◄  A"),
            ["ctrl.gantry.positive"] = ("D  ►", "D  ►"),
            ["ctrl.trolley.negative"] = ("S  Tierra", "S  Land"),
            ["ctrl.trolley.positive"] = ("W  Mar", "W  Sea"),
            ["ctrl.hoist.negative"] = ("R  Subir", "R  Up"),
            ["ctrl.hoist.positive"] = ("F  Bajar", "F  Down"),
            ["ctrl.spreader.negative"] = ("Q  Izq.", "Q  Left"),
            ["ctrl.spreader.positive"] = ("E  Der.", "E  Right"),
            ["ctrl.grab"] = ("<b>Espacio</b>\nEnganchar / soltar", "<b>Space</b>\nGrab / release"),

            // Pausa
            ["pause.title"] = ("<b>Pausa</b>", "<b>Paused</b>"),
            ["pause.resume"] = ("Reanudar", "Resume"),
            ["pause.controls"] = ("Controles", "Controls"),
            ["pause.settings"] = ("Configuración", "Settings"),
            ["pause.restart"] = ("Reiniciar", "Restart"),
            ["pause.hint"] = ("P para volver al juego", "P to go back to the game"),
            ["pause.back"] = ("Volver", "Back"),
            ["pause.controls.text"] = (
                "<b>Controles</b>\n\n" +
                "<b>A / D</b>    Mover el pórtico sobre los rieles\n" +
                "<b>W / S</b>    Mover el carro hacia el mar / hacia tierra\n" +
                "<b>R / F</b>    Subir / bajar el spreader\n" +
                "<b>Q / E</b>    Girar el spreader\n" +
                "<b>Espacio</b>  Enganchar / soltar (luz amarilla = carga lista)\n" +
                "<b>C</b>          Cambiar de cámara\n" +
                "<b>P</b>          Pausa\n\n" +
                "<b>Cámara externa:</b> clic derecho y arrastrar para orbitar,\nrueda para acercar o alejar.\n\n" +
                "Objetivo: llevar las 8 cargas del barco a la zona amarilla.",
                "<b>Controls</b>\n\n" +
                "<b>A / D</b>    Move the gantry along the rails\n" +
                "<b>W / S</b>    Move the trolley to the sea / to land\n" +
                "<b>R / F</b>    Raise / lower the spreader\n" +
                "<b>Q / E</b>    Rotate the spreader\n" +
                "<b>Space</b>    Grab / release (yellow light = load ready)\n" +
                "<b>C</b>          Switch camera\n" +
                "<b>P</b>          Pause\n\n" +
                "<b>External camera:</b> right-click and drag to orbit,\nscroll to zoom in or out.\n\n" +
                "Goal: move the 8 loads from the ship to the yellow area."),

            // Configuración
            ["settings.title"] = ("<b>Configuración</b>", "<b>Settings</b>"),
            ["settings.controls"] = ("Controles en pantalla", "On-screen controls"),
            ["settings.telemetry"] = ("Telemetría", "Telemetry"),
            ["settings.speed.gantry"] = ("Velocidad del pórtico", "Gantry speed"),
            ["settings.speed.trolley"] = ("Velocidad del carro", "Trolley speed"),
            ["settings.speed.hoist"] = ("Velocidad del izaje", "Hoist speed"),
            ["settings.speed.spreader"] = ("Velocidad del spreader", "Spreader speed"),
            ["settings.sensitivity"] = ("Sensibilidad de cámara", "Camera sensitivity"),
            ["settings.language"] = ("Idioma", "Language"),
            ["settings.defaults"] = ("Restablecer", "Reset"),
        };

        private static Language _current = Language.Spanish;

        public static Language Current
        {
            get => _current;
            set
            {
                if (value == _current)
                {
                    return;
                }

                _current = value;
                Changed?.Invoke();
            }
        }

        /// <summary>Se dispara al cambiar el idioma, para que cada texto se actualice.</summary>
        public static event Action Changed;

        /// <summary>Texto de la clave en el idioma actual. Si la clave no existe, devuelve la clave.</summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key) || !Table.TryGetValue(key, out var entry))
            {
                return key;
            }

            return _current == Language.English ? entry.En : entry.Es;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        /// <summary>
        /// Número con el separador decimal del idioma (coma en español, punto en inglés).
        /// No usa la cultura del sistema: en WebGL siempre es la invariante.
        /// </summary>
        public static string Number(float value, string format)
        {
            return value.ToString(format, _current == Language.English ? CultureInfo.InvariantCulture : SpanishNumbers);
        }

        private static readonly NumberFormatInfo SpanishNumbers = new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
            NegativeSign = "-"
        };
    }
}
