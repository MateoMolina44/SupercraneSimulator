using SupercraneSimulator.Cameras;
using SupercraneSimulator.Crane;
using SupercraneSimulator.Gameplay;
using SupercraneSimulator.Input;
using SupercraneSimulator.Localization;
using SupercraneSimulator.Settings;
using SupercraneSimulator.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SupercraneSimulator.Editor.SceneBuilder
{
    /// <summary>
    /// Genera la escena Main completa con las medidas de DESIGN.md.
    /// Todos los transforms quedan con escala (1,1,1): el tamaño está en las mallas.
    /// </summary>
    public static class SceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";

        // Medidas en metros (DESIGN.md, sección 2).
        private const float LandRailZ = -12f;
        private const float SeaRailZ = 8f;
        private const float LegHalfSpanX = 6f;
        private const float RailTopY = 0.2f;
        private const float GirderTopY = 16.2f;
        private const float QuayEdgeZ = 10f;
        private const float WaterY = -2f;
        private const float DeckY = 1f;

        // Zona de apilado en el muelle, al alcance del carro (z 2 a 10) y del pórtico (x ±15).
        private static readonly Vector3 StackZoneCenter = new Vector3(-5f, 0f, 4.7f);
        private static readonly Vector3 StackZoneSize = new Vector3(14f, 15f, 5f);

        // Medidas de las cargas (contenedor de 20 pies: largo × alto × ancho).
        private static readonly Vector3 ContainerSize = new Vector3(6.1f, 2.6f, 2.44f);
        private const float CrateSize = 1.5f;
        private const float ContainerMass = 800f;
        private const float CrateMass = 150f;

        private const float HoistTopY = GirderTopY - 0.7f;

        // Masas (kg). El conjunto que sostiene la carga es pesado, como en una STS real, y
        // los vecinos mantienen una relación de 2,5:1 como mucho.
        private const float GantryMass = 3000f;
        private const float TrolleyMass = 2000f;
        private const float HoistMass = 1500f;
        private const float CableMass = 800f;
        private const float SpreaderMass = 2000f;

        // Amortiguación de los eslabones de cable: hace de anti-balanceo (en una STS real los
        // cables van en trapecio y el balanceo muere rápido).
        private const float CableSwingDamping = 15f;

        // Posición inicial de cada joint (dentro del rango).
        private const float GantryStart = 0f;
        private const float TrolleyStart = 6f;
        private const float HoistStart = 3f;

        private static Materials _materials;

        [MenuItem("Supercrane/Build Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            BuilderAssets.ClearCache();
            _materials = new Materials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuilderAssets.DeleteGeneratedMeshes();

            var environment = Group("--- Environment ---");
            BuildEnvironment(environment);

            // La entrada se crea antes que la grúa porque los joysticks de la cabina la leen.
            var systems = Group("--- Systems ---");
            var input = new GameObject("CraneInput").AddComponent<CraneInput>();
            input.transform.SetParent(systems, false);

            Group("--- Crane ---");
            var crane = BuildCrane(input);

            var controller = new GameObject("CraneController").AddComponent<CraneController>();
            controller.transform.SetParent(systems, false);
            controller.Configure(input, crane.Gantry, crane.Trolley, crane.Hoist, crane.Spreader);

            var cameras = Group("--- Cameras ---");
            var external = BuildExternalCamera(cameras);
            var orbitCamera = external.gameObject.AddComponent<OrbitCamera>();
            orbitCamera.Configure(crane.Gantry.transform, new Vector3(0f, 8f, 4f));
            var switcher = new GameObject("CameraSwitcher").AddComponent<CameraSwitcher>();
            switcher.transform.SetParent(cameras, false);
            var loadsGroup = Group("--- Loads ---");
            var loads = BuildLoads(loadsGroup);
            var zone = BuildStackZone(environment, loads);

            var ui = Group("--- UI ---");
            var joints = new[] { crane.Gantry, crane.Trolley, crane.Hoist, crane.Spreader };
            var cameraLabel = BuildUi(ui, input, controller, switcher, zone, crane.Grabber, joints, orbitCamera);
            switcher.Configure(input, cameraLabel,
                new[] { external, crane.CabinCamera, crane.CraneCamera },
                new[] { "hud.camera.external", "hud.camera.cabin", "hud.camera.crane" });

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[Supercrane] Escena generada en {ScenePath}");
        }

        // ---------------------------------------------------------------- Entorno

        private static void BuildEnvironment(Transform parent)
        {
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.transform.SetParent(parent, false);
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.7f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.48f, 0.5f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.25f, 0.25f);

            // Muelle: la parte de arriba queda en y = 0 y el borde con el agua en z = QuayEdgeZ.
            const float quayDepth = 4f;
            const float quayWidthZ = 50f;
            Box(parent, "Quay", new Vector3(0f, -quayDepth / 2f, QuayEdgeZ - quayWidthZ / 2f),
                new Vector3(100f, quayDepth, quayWidthZ), _materials.Concrete);

            Box(parent, "Water", new Vector3(0f, WaterY - 0.05f, 60f), new Vector3(200f, 0.1f, 100f), _materials.Water, false);

            BuildShip(parent);
            BuildDockProps(parent, quayWidthZ);
        }

        /// <summary>Ambientación estática del muelle: bolardos, postes de luz y grúas de fondo
        /// a lo lejos. No agregan Articulation Bodies ni afectan la consigna.</summary>
        private static void BuildDockProps(Transform parent, float quayWidthZ)
        {
            var props = new GameObject("DockProps").transform;
            props.SetParent(parent, false);

            // Bolardos a lo largo del borde del muelle (z = QuayEdgeZ), fuera del paso de la grúa.
            var bollards = new GameObject("Bollards").transform;
            bollards.SetParent(props, false);
            for (var x = -48f; x <= 48f; x += 8f)
            {
                Shape(bollards, "Bollard", BuilderAssets.Cylinder(0.5f, 0.7f),
                    new Vector3(x, 0.35f, QuayEdgeZ - 0.3f), _materials.Dark);
            }

            // Postes de luz sobre el muelle, lejos de los rieles de la grúa.
            var poles = new GameObject("LightPoles").transform;
            poles.SetParent(props, false);
            var poleZ = QuayEdgeZ - quayWidthZ + 4f;
            foreach (var x in new[] { -40f, -20f, 20f, 40f })
            {
                var pole = Shape(poles, "LightPole", BuilderAssets.Cylinder(0.25f, 9f),
                    new Vector3(x, 4.5f, poleZ), _materials.Steel);
                Shape(pole.transform, "Lamp", BuilderAssets.Box(new Vector3(0.6f, 0.4f, 0.6f)),
                    new Vector3(0f, 4.7f, 0f), _materials.Yellow);
            }
        }

        private static void BuildShip(Transform parent)
        {
            var ship = new GameObject("Ship").transform;
            ship.SetParent(parent, false);

            // Casco: cubierta en y = DeckY, entre z = 11 y z = 23. Modelado en Blender (proa
            // afinada) vía Supercrane/Build Scene; el .fbx queda en Assets/Generated/Imported.
            const float hullBottom = -4f;
            var hullHeight = DeckY - hullBottom;
            ImportedShape(ship, "Hull", "Assets/Generated/Imported/Prefabs/Hull.prefab",
                new Vector3(0f, hullBottom + hullHeight / 2f, 17f), new Vector3(60f, hullHeight, 12f), _materials.Hull);
            Box(ship, "Bridge", new Vector3(24f, DeckY + 5f, 17f), new Vector3(7f, 10f, 11f), _materials.White);
            Box(ship, "BridgeWindows", new Vector3(24f, DeckY + 8.5f, 11.45f), new Vector3(6f, 1.2f, 0.1f), _materials.Glass, false);
        }

        // ---------------------------------------------------------------- Grúa

        private struct CraneParts
        {
            public DrivenJoint Gantry;
            public DrivenJoint Trolley;
            public DrivenJoint Hoist;
            public DrivenJoint Spreader;
            public Camera CabinCamera;
            public Camera CraneCamera;
            public SpreaderGrabber Grabber;
        }

        private static CraneParts BuildCrane(CraneInput input)
        {
            var parts = new CraneParts();

            // 1. Base: raíz inmóvil con los rieles.
            var baseBody = Body("Base", null, Vector3.zero, 1000f);
            baseBody.immovable = true;
            Box(baseBody.transform, "Rail_Land", new Vector3(0f, RailTopY / 2f, LandRailZ), new Vector3(70f, RailTopY, 0.4f), _materials.Steel);
            Box(baseBody.transform, "Rail_Sea", new Vector3(0f, RailTopY / 2f, SeaRailZ), new Vector3(70f, RailTopY, 0.4f), _materials.Steel);

            // Cada joint se arma en una posición dentro de su rango (nunca en un límite: PhysX
            // lo empuja de golpe hacia adentro). jointPosition vale 0 ahí; el offset de
            // DrivenJoint lo traduce a la medida real que se muestra.

            // 2. Pórtico: Prismatic sobre los rieles (eje X del mundo).
            var gantry = Body("Gantry", baseBody.transform, new Vector3(GantryStart, 0f, 0f), GantryMass);
            parts.Gantry = DrivenPrismatic(gantry, Quaternion.identity, "joint.gantry", -15f, 15f, GantryStart, 1.5f, 2.5f, 200f, 30f);
            BuildGantryVisuals(gantry.transform);

            // 3. Carro: Prismatic a lo largo de la viga (eje Z).
            var trolley = Body("Trolley", gantry.transform, new Vector3(GantryStart, GirderTopY, TrolleyStart), TrolleyMass);
            parts.Trolley = DrivenPrismatic(trolley, Quaternion.Euler(0f, -90f, 0f), "joint.trolley", 2f, 20f, TrolleyStart, 2.5f, 2.5f, 200f, 30f);
            Box(trolley.transform, "TrolleyFrame", new Vector3(0f, 0.5f, 0f), new Vector3(4f, 1f, 3f), _materials.CraneRed, false);
            parts.CabinCamera = BuildCabin(trolley.transform, input);
            parts.CraneCamera = BuildCraneCamera(trolley.transform);
            SetBoxInertia(trolley, new Vector3(4f, 1f, 3f), new Vector3(0f, 0.5f, 0f));

            // 4. Izaje: Prismatic vertical hacia abajo. 0 m es el gancho arriba de todo.
            var hoistPosition = new Vector3(GantryStart, HoistTopY - HoistStart, TrolleyStart);
            var hoist = Body("Hoist", trolley.transform, hoistPosition, HoistMass);
            parts.Hoist = DrivenPrismatic(hoist, Quaternion.Euler(0f, 0f, -90f), "joint.hoist", 0f, 12f, HoistStart, 1.5f, 1f, 400f, 40f);
            Box(hoist.transform, "HeadBlock", Vector3.zero, new Vector3(1.4f, 0.6f, 1.4f), _materials.Yellow, false);
            SetBoxInertia(hoist, new Vector3(1.4f, 0.6f, 1.4f), Vector3.zero);
            BuildRopes(trolley.transform, hoist.transform);

            // 5-6. Cable: dos eslabones Spherical pasivos de 1 m.
            var cableUpper = CableLink("CableUpper", hoist.transform, hoistPosition + new Vector3(0f, -0.3f, 0f));
            var cableLower = CableLink("CableLower", cableUpper.transform, cableUpper.transform.position + Vector3.down);

            // 7. Spreader: Revolute en Y, ±180°.
            var spreader = Body("Spreader", cableLower.transform, cableLower.transform.position + Vector3.down, SpreaderMass);
            spreader.jointType = ArticulationJointType.RevoluteJoint;
            spreader.anchorRotation = Quaternion.Euler(0f, 0f, 90f);
            spreader.twistLock = ArticulationDofLock.LimitedMotion;
            // El límite físico pasa apenas de ±180° para que el target nunca quede sobre el tope.
            spreader.xDrive = Drive(-185f, 185f, 0f, 400f, 40f);
            Box(spreader.transform, "SpreaderFrame", new Vector3(0f, -0.25f, 0f), new Vector3(6.1f, 0.5f, 2.5f), _materials.Yellow);
            parts.Spreader = Driven(spreader, "joint.spreader", -180f, 180f, 0f, 0f, 30f, 0.6f);

            // Luces de estado en las esquinas: rojo sin carga, amarillo carga lista, verde enganchado.
            var lights = new Renderer[4];
            var corner = 0;
            foreach (var x in new[] { -2.9f, 2.9f })
            {
                foreach (var z in new[] { -1.1f, 1.1f })
                {
                    lights[corner++] = Box(spreader.transform, "StatusLight", new Vector3(x, 0.1f, z), new Vector3(0.25f, 0.2f, 0.25f),
                        _materials.Light, false).GetComponent<Renderer>();
                }
            }

            parts.Grabber = spreader.gameObject.AddComponent<SpreaderGrabber>();
            parts.Grabber.Configure(input, lights);

            foreach (var body in new[] { gantry, trolley, hoist, cableUpper, cableLower, spreader })
            {
                MatchParentAnchor(body);
            }

            return parts;
        }

        private static void BuildGantryVisuals(Transform gantry)
        {
            // Las vigas transversales van por arriba del carro: la cabina cuelga debajo de las
            // vigas longitudinales y tiene que poder pasar entre las patas.
            const float portalBeamY = GirderTopY + 2.3f;
            var legHeight = portalBeamY + 0.5f - RailTopY;
            var legCenterY = RailTopY + legHeight / 2f;
            var legSize = new Vector3(1f, legHeight, 1f);
            foreach (var z in new[] { LandRailZ, SeaRailZ })
            {
                Box(gantry, $"Leg_L_{z}", new Vector3(-LegHalfSpanX, legCenterY, z), legSize, _materials.CraneRed);
                Box(gantry, $"Leg_R_{z}", new Vector3(LegHalfSpanX, legCenterY, z), legSize, _materials.CraneRed);
                Box(gantry, $"PortalBeam_{z}", new Vector3(0f, portalBeamY, z), new Vector3(LegHalfSpanX * 2f + 1f, 1f, 1f), _materials.CraneRed);
                Box(gantry, $"LowerBeam_{z}", new Vector3(0f, 6f, z), new Vector3(LegHalfSpanX * 2f, 0.7f, 0.7f), _materials.White);

                // Tensores de la viga transversal a las longitudinales, por fuera del carro.
                foreach (var x in new[] { -2.5f, 2.5f })
                {
                    var hangerBottom = GirderTopY - 1f;
                    var hangerTop = portalBeamY - 0.5f;
                    Box(gantry, $"Hanger_{x}_{z}", new Vector3(x, (hangerBottom + hangerTop) / 2f, z), new Vector3(0.3f, hangerTop - hangerBottom, 0.3f), _materials.CraneRed);
                    Box(gantry, $"Bracket_{x}_{z}", new Vector3(x * 0.88f, GirderTopY - 1.15f, z), new Vector3(1.2f, 0.3f, 0.5f), _materials.CraneRed);
                }
            }

            // Vigas por donde corre el carro: del lado tierra (backreach) hasta la punta de la pluma.
            const float girderStartZ = -16f;
            const float girderEndZ = 25f;
            var girderLength = girderEndZ - girderStartZ;
            var girderCenterZ = (girderStartZ + girderEndZ) / 2f;
            foreach (var x in new[] { -1.8f, 1.8f })
            {
                Box(gantry, $"Girder_{x}", new Vector3(x, GirderTopY - 0.5f, girderCenterZ), new Vector3(0.6f, 1f, girderLength), _materials.White);
            }

            Box(gantry, "MachineryHouse", new Vector3(0f, GirderTopY + 1.5f, -14f), new Vector3(6f, 3f, 4f), _materials.White);
        }

        /// <summary>
        /// Cabina vidriada colgada del carro, del lado tierra respecto del izaje y girada 180° para
        /// que el operador mire hacia el mar. Por dentro tiene piso de vidrio, asiento, consolas
        /// con joysticks y un tablero; la cámara va a la altura de la cabeza mirando hacia abajo,
        /// como en las STS reales. En coordenadas de la cabina el operador mira hacia -Z y su
        /// izquierda es +X.
        /// </summary>
        private static Camera BuildCabin(Transform trolley, CraneInput input)
        {
            const float width = 2.4f;   // X
            const float height = 2.4f;  // Y
            const float depth = 2.6f;   // Z
            const float top = -0.3f;
            const float bar = 0.1f;
            var center = new Vector3(0f, top - height / 2f, -2.2f);
            var halfW = width / 2f - bar / 2f;
            var halfD = depth / 2f - bar / 2f;

            var cabin = new GameObject("Cabin").transform;
            cabin.SetParent(trolley, false);
            cabin.localPosition = center;
            cabin.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var frame = _materials.CabinFrame;

            // Colgantes hasta el carro.
            foreach (var x in new[] { -0.8f, 0.8f })
            {
                Box(cabin, "Hanger", new Vector3(x, height / 2f - top / 2f, 0f), new Vector3(bar, -top, bar), _materials.Steel, false);
            }

            // Techo, marco del piso y parantes.
            Box(cabin, "Roof", new Vector3(0f, height / 2f - 0.05f, 0f), new Vector3(width, 0.1f, depth), frame, false);
            var floorLocalY = -height / 2f + 0.04f;
            Box(cabin, "FloorBar_Back", new Vector3(0f, floorLocalY, -halfD), new Vector3(width, 0.08f, bar), frame, false);
            Box(cabin, "FloorBar_Front", new Vector3(0f, floorLocalY, halfD), new Vector3(width, 0.08f, bar), frame, false);
            Box(cabin, "FloorBar_Left", new Vector3(-halfW, floorLocalY, 0f), new Vector3(bar, 0.08f, depth), frame, false);
            Box(cabin, "FloorBar_Right", new Vector3(halfW, floorLocalY, 0f), new Vector3(bar, 0.08f, depth), frame, false);
            Box(cabin, "FloorBar_Middle", new Vector3(0f, floorLocalY, 0.3f), new Vector3(width, 0.08f, bar), frame, false);
            foreach (var x in new[] { -halfW, halfW })
            {
                foreach (var z in new[] { -halfD, halfD })
                {
                    Box(cabin, "Pillar", new Vector3(x, 0f, z), new Vector3(bar, height, bar), frame, false);
                }

                // Barandas a media altura en los laterales.
                Box(cabin, "SideRail", new Vector3(x, -0.2f, 0f), new Vector3(bar, 0.06f, depth), frame, false);
            }

            // Vidrios: piso y las cuatro caras.
            Glass(cabin, "Glass_Floor", new Vector3(0f, -height / 2f + 0.02f, 0f), new Vector3(width - 0.1f, 0.02f, depth - 0.1f));
            Glass(cabin, "Glass_Left", new Vector3(-width / 2f, 0f, 0f), new Vector3(0.02f, height - 0.1f, depth - 0.1f));
            Glass(cabin, "Glass_Right", new Vector3(width / 2f, 0f, 0f), new Vector3(0.02f, height - 0.1f, depth - 0.1f));
            Glass(cabin, "Glass_Back", new Vector3(0f, 0f, -depth / 2f), new Vector3(width - 0.1f, height - 0.1f, 0.02f));
            Glass(cabin, "Glass_Front", new Vector3(0f, 0.5f, depth / 2f), new Vector3(width - 0.1f, height - 1.1f, 0.02f));

            // Tablero y armario detrás del operador (lado tierra).
            Box(cabin, "Panel_Front", new Vector3(0f, -height / 2f + 0.55f, halfD - 0.02f), new Vector3(width - 0.2f, 1f, 0.08f), frame, false);
            Box(cabin, "Cabinet", new Vector3(0.75f, -height / 2f + 0.85f, halfD - 0.3f), new Vector3(0.7f, 1.6f, 0.45f), _materials.CabinPanel, false);

            // Asiento y consolas con joysticks a los costados.
            var seatY = -height / 2f + 0.5f;
            // El asiento queda detrás de la cámara para no tapar el piso de vidrio.
            Box(cabin, "Seat", new Vector3(0f, seatY, 0.85f), new Vector3(0.6f, 0.15f, 0.6f), _materials.Dark, false);
            Box(cabin, "SeatBack", new Vector3(0f, seatY + 0.45f, 1.15f), new Vector3(0.6f, 0.8f, 0.12f), _materials.Dark, false);
            Box(cabin, "SeatPost", new Vector3(0f, seatY - 0.28f, 0.85f), new Vector3(0.12f, 0.42f, 0.12f), _materials.Steel, false);
            foreach (var x in new[] { -0.55f, 0.55f })
            {
                Box(cabin, "Console", new Vector3(x, seatY + 0.12f, 0.2f), new Vector3(0.32f, 0.22f, 0.7f), _materials.ConsoleGrey, false);
                Box(cabin, "Screen", new Vector3(x, seatY + 0.3f, -0.15f), new Vector3(0.28f, 0.2f, 0.03f), _materials.Screen, false);
            }

            // Joysticks. Inclinar hacia adelante es rotar en -X (el operador mira a -Z);
            // inclinar a la derecha del operador es rotar en +Z.
            // Izquierdo: adelante/atrás = carro (W/S), costados = pórtico (A/D).
            Joystick(cabin, "Joystick_Left", new Vector3(0.55f, seatY + 0.23f, 0.05f), input,
                (CraneAxis.Trolley, -20f), null, (CraneAxis.Gantry, 20f));
            // Derecho: adelante = bajar (F), atrás = subir (R); girar el palo = spreader (Q/E).
            Joystick(cabin, "Joystick_Right", new Vector3(-0.55f, seatY + 0.23f, 0.05f), input,
                (CraneAxis.Hoist, -20f), (CraneAxis.Spreader, 35f), null);

            // Cámara a la altura de la cabeza, mirando al mar y hacia abajo por el piso de vidrio.
            var camera = new GameObject("CabinCamera").AddComponent<Camera>();
            camera.transform.SetParent(cabin, false);
            camera.transform.localPosition = new Vector3(0f, seatY + 0.85f, 0.5f);
            camera.transform.localRotation = Quaternion.Euler(58f, 180f, 0f);
            camera.nearClipPlane = 0.05f;
            camera.fieldOfView = 80f;
            camera.farClipPlane = 500f;
            camera.gameObject.AddComponent<AudioListener>();
            return camera;
        }

        /// <summary>
        /// Cámara vertical colgada del carro, mirando derecho hacia abajo al spreader.
        /// Posición tomada de la "Crane Camera" que armó Mateo en el editor.
        /// </summary>
        private static Camera BuildCraneCamera(Transform trolley)
        {
            var camera = new GameObject("CraneCamera").AddComponent<Camera>();
            camera.transform.SetParent(trolley, false);
            camera.transform.localPosition = new Vector3(0f, 0.22f, 0.42f);
            camera.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            camera.fieldOfView = 60f;
            camera.farClipPlane = 500f;
            camera.gameObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void Joystick(Transform cabin, string name, Vector3 basePosition, CraneInput input,
            (CraneAxis, float)? pitch, (CraneAxis, float)? twist, (CraneAxis, float)? roll)
        {
            // El pivote está en la base: al rotarlo se inclinan el palo y la perilla.
            var pivot = new GameObject(name).transform;
            pivot.SetParent(cabin, false);
            pivot.localPosition = basePosition;
            Shape(pivot, "Stick", BuilderAssets.Cylinder(0.04f, 0.18f), new Vector3(0f, 0.09f, 0f), _materials.Dark);
            Box(pivot, "Knob", new Vector3(0f, 0.2f, 0f), new Vector3(0.08f, 0.08f, 0.1f), _materials.CraneRed, false);
            pivot.gameObject.AddComponent<CabinJoystick>().Configure(input, pitch, twist, roll);
        }

        private static void Glass(Transform parent, string name, Vector3 localPosition, Vector3 size)
        {
            var go = Box(parent, name, localPosition, size, _materials.Glass, false);
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void BuildRopes(Transform trolley, Transform hoist)
        {
            foreach (var x in new[] { -0.5f, 0.5f })
            {
                var from = new GameObject($"RopeAnchor_{x}").transform;
                from.SetParent(trolley, false);
                from.localPosition = new Vector3(x, 0f, 0f);

                var to = new GameObject($"RopeEnd_{x}").transform;
                to.SetParent(hoist, false);
                to.localPosition = new Vector3(x, 0.3f, 0f);

                var line = from.gameObject.AddComponent<LineRenderer>();
                line.widthMultiplier = 0.06f;
                line.sharedMaterial = _materials.Steel;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                from.gameObject.AddComponent<RopeLine>().Configure(from, to);
            }
        }

        private static ArticulationBody CableLink(string name, Transform parent, Vector3 pivot)
        {
            var link = Body(name, parent, pivot, CableMass);
            link.jointType = ArticulationJointType.SphericalJoint;
            // El eje de twist (X del ancla) apunta a lo largo del cable.
            link.anchorRotation = Quaternion.Euler(0f, 0f, 90f);
            link.twistLock = ArticulationDofLock.LockedMotion;
            link.swingYLock = ArticulationDofLock.LimitedMotion;
            link.swingZLock = ArticulationDofLock.LimitedMotion;
            // Pasivo: sin rigidez ni target, solo un poco de amortiguación.
            link.yDrive = Drive(-45f, 45f, 0f, 0f, CableSwingDamping);
            link.zDrive = Drive(-45f, 45f, 0f, 0f, CableSwingDamping);
            Shape(link.transform, "Wire", BuilderAssets.Cylinder(0.08f, 1f), new Vector3(0f, -0.5f, 0f), _materials.Steel);
            // Inercia inflada a propósito: con la real (Iyy ≈ 10) el solver no transmite el torque
            // del spreader (Iyy ≈ 7000) y el giro queda flojo y oscila.
            link.automaticCenterOfMass = false;
            link.centerOfMass = new Vector3(0f, -0.5f, 0f);
            link.automaticInertiaTensor = false;
            link.inertiaTensor = new Vector3(600f, 3200f, 600f);
            link.inertiaTensorRotation = Quaternion.identity;
            return link;
        }

        // ---------------------------------------------------------------- Cargas y zona

        /// <summary>4 contenedores y 4 cajas sobre la cubierta del barco (z 11 a 23).</summary>
        private static Load[] BuildLoads(Transform parent)
        {
            var friction = BuilderAssets.PhysicsMaterial("LoadFriction", 0.6f, 0.7f);
            var containerColors = new[] { _materials.ContainerBlue, _materials.ContainerGreen, _materials.ContainerOrange, _materials.ContainerGrey };
            var containerPositions = new[]
            {
                new Vector2(-4f, 14f), new Vector2(4f, 14f), new Vector2(-4f, 17.5f), new Vector2(4f, 17.5f)
            };
            var cratePositions = new[]
            {
                new Vector2(11f, 13f), new Vector2(11f, 16f), new Vector2(11f, 19f), new Vector2(-11f, 16f)
            };

            var loads = new Load[containerPositions.Length + cratePositions.Length];
            for (var i = 0; i < containerPositions.Length; i++)
            {
                var position = new Vector3(containerPositions[i].x, DeckY + ContainerSize.y / 2f + 0.01f, containerPositions[i].y);
                var container = LoadBody(parent, $"Container_{i + 1:00}", position, ContainerSize, containerColors[i], ContainerMass, friction, LoadKind.Container);
                // Puertas en un extremo, para que se note el sentido del contenedor.
                Box(container.transform, "Doors", new Vector3(ContainerSize.x / 2f + 0.01f, 0f, 0f),
                    new Vector3(0.02f, ContainerSize.y - 0.2f, ContainerSize.z - 0.2f), _materials.Dark, false);
                loads[i] = container;
            }

            for (var i = 0; i < cratePositions.Length; i++)
            {
                var position = new Vector3(cratePositions[i].x, DeckY + CrateSize / 2f + 0.01f, cratePositions[i].y);
                loads[containerPositions.Length + i] = LoadBody(parent, $"Crate_{i + 1:00}", position, Vector3.one * CrateSize,
                    _materials.Wood, CrateMass, friction, LoadKind.Crate);
            }

            return loads;
        }

        private static Load LoadBody(Transform parent, string name, Vector3 position, Vector3 size, Material material,
            float mass, PhysicsMaterial friction, LoadKind kind)
        {
            var go = Box(parent, name, position, size, material);
            go.GetComponent<BoxCollider>().sharedMaterial = friction;
            var body = go.AddComponent<Rigidbody>();
            body.mass = mass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            var load = go.AddComponent<Load>();
            load.Configure(kind);
            return load;
        }

        private static StackZone BuildStackZone(Transform parent, Load[] loads)
        {
            var zone = new GameObject("StackZone").transform;
            zone.SetParent(parent, false);
            zone.position = StackZoneCenter;

            // Pintura en el piso: relleno y borde amarillo (sin collider, apenas sobre el muelle).
            const float paintHeight = 0.01f;
            const float line = 0.2f;
            var size = StackZoneSize;
            Box(zone, "Fill", new Vector3(0f, 0.006f, 0f), new Vector3(size.x, paintHeight, size.z), _materials.ZoneFill, false);
            Box(zone, "Line_N", new Vector3(0f, 0.008f, size.z / 2f - line / 2f), new Vector3(size.x, paintHeight, line), _materials.Yellow, false);
            Box(zone, "Line_S", new Vector3(0f, 0.008f, -size.z / 2f + line / 2f), new Vector3(size.x, paintHeight, line), _materials.Yellow, false);
            Box(zone, "Line_E", new Vector3(size.x / 2f - line / 2f, 0.008f, 0f), new Vector3(line, paintHeight, size.z), _materials.Yellow, false);
            Box(zone, "Line_W", new Vector3(-size.x / 2f + line / 2f, 0.008f, 0f), new Vector3(line, paintHeight, size.z), _materials.Yellow, false);

            var stackZone = zone.gameObject.AddComponent<StackZone>();
            stackZone.Configure(loads, size);
            return stackZone;
        }

        // ---------------------------------------------------------------- Cámaras y UI

        private static Camera BuildExternalCamera(Transform parent)
        {
            var camera = new GameObject("ExternalCamera").AddComponent<Camera>();
            camera.transform.SetParent(parent, false);
            camera.transform.position = new Vector3(-38f, 22f, -26f);
            camera.transform.LookAt(new Vector3(0f, 7f, 5f));
            camera.fieldOfView = 55f;
            camera.farClipPlane = 500f;
            camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
            return camera;
        }

        /// <summary>Arma el HUD, la configuración y la pausa. Devuelve el texto del botón de cámara.</summary>
        private static Text BuildUi(Transform parent, CraneInput input, CraneController controller, CameraSwitcher switcher,
            StackZone zone, SpreaderGrabber grabber, DrivenJoint[] joints, OrbitCamera orbitCamera)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(parent, false);

            var canvas = Canvas(parent, "HUD", 0);

            // Telemetría: esquina superior izquierda.
            var telemetry = Panel(canvas, "Telemetry", new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(330f, 120f));
            var labels = Label(telemetry, "Labels", new Vector2(10f, -8f), new Vector2(110f, 104f), TextAnchor.UpperLeft);
            var targets = Label(telemetry, "Targets", new Vector2(120f, -8f), new Vector2(100f, 104f), TextAnchor.UpperRight);
            var actuals = Label(telemetry, "Actuals", new Vector2(220f, -8f), new Vector2(100f, 104f), TextAnchor.UpperRight);
            telemetry.gameObject.AddComponent<JointTelemetryView>().Configure(controller, labels, targets, actuals);

            // Botón de cámara: esquina superior derecha (CameraSwitcher pone el texto).
            var (cameraButton, buttonLabel) = TextButton(canvas, "CameraButton", new Vector2(1f, 1f), new Vector2(-16f, -16f),
                new Vector2(230f, 44f), Strings.Get("hud.camera.external"));
            UnityEventTools.AddPersistentListener(cameraButton.onClick, switcher.Next);

            // Contador de apiladas y estado del spreader: arriba al centro.
            var counterPanel = Panel(canvas, "StackCounter", new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(400f, 64f));
            var counter = Label(counterPanel, "Count", new Vector2(0f, 12f), new Vector2(400f, 30f), TextAnchor.MiddleCenter);
            counter.fontSize = 22;
            var spreaderState = Label(counterPanel, "SpreaderState", new Vector2(0f, -14f), new Vector2(400f, 26f), TextAnchor.MiddleCenter);
            spreaderState.fontSize = 15;
            counterPanel.gameObject.AddComponent<StackCounterView>().Configure(zone, grabber, counter, spreaderState);

            var onScreenControls = BuildOnScreenControls(canvas, input);

            var settings = new GameObject("GameSettings").AddComponent<GameSettings>();
            settings.transform.SetParent(parent, false);
            settings.Configure(onScreenControls, telemetry.gameObject, joints, orbitCamera);

            var pauseMenu = BuildPauseMenu(parent, input, settings);
            var (pauseButton, pauseLabel) = TextButton(canvas, "PauseButton", new Vector2(1f, 1f), new Vector2(-16f, -68f),
                new Vector2(230f, 36f), "", 15);
            Localize(pauseLabel, "hud.pause");
            UnityEventTools.AddPersistentListener(pauseButton.onClick, pauseMenu.Toggle);

            return buttonLabel;
        }

        /// <summary>
        /// Botones en pantalla abajo al centro: dos por eje (actúan mientras se mantienen) y
        /// el de enganchar. Funcionan con mouse y con touch. Devuelve el panel para poder ocultarlo.
        /// </summary>
        private static GameObject BuildOnScreenControls(Transform canvas, CraneInput input)
        {
            var panel = Panel(canvas, "OnScreenControls", new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(960f, 100f));
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

            var groups = new[]
            {
                ("joint.gantry", CraneAxis.Gantry, "ctrl.gantry"),
                ("joint.trolley", CraneAxis.Trolley, "ctrl.trolley"),
                ("joint.hoist", CraneAxis.Hoist, "ctrl.hoist"),
                ("joint.spreader", CraneAxis.Spreader, "ctrl.spreader")
            };
            const float groupWidth = 190f;
            const float margin = 10f;
            var bottomLeft = Vector2.zero;
            for (var i = 0; i < groups.Length; i++)
            {
                var (titleKey, axis, buttonKey) = groups[i];
                var x = margin + i * groupWidth;
                var titleLabel = Label(panel, $"{axis}Title", new Vector2(x, -6f), new Vector2(180f, 24f), TextAnchor.UpperLeft);
                titleLabel.fontSize = 15;
                titleLabel.fontStyle = FontStyle.Bold;
                Localize(titleLabel, titleKey);

                var (negativeButton, negativeLabel) = TextButton(panel, $"{axis}Negative", bottomLeft, new Vector2(x, margin), new Vector2(88f, 56f), "", 15);
                Localize(negativeLabel, buttonKey + ".negative");
                negativeButton.gameObject.AddComponent<HoldButton>().Configure(input, axis, -1f);
                var (positiveButton, positiveLabel) = TextButton(panel, $"{axis}Positive", bottomLeft, new Vector2(x + 92f, margin), new Vector2(88f, 56f), "", 15);
                Localize(positiveLabel, buttonKey + ".positive");
                positiveButton.gameObject.AddComponent<HoldButton>().Configure(input, axis, 1f);
            }

            var (grabButton, grabLabel) = TextButton(panel, "GrabButton", bottomLeft, new Vector2(margin + groups.Length * groupWidth, margin),
                new Vector2(180f, 80f), "", 15);
            Localize(grabLabel, "ctrl.grab");
            UnityEventTools.AddPersistentListener(grabButton.onClick, input.RequestGrab);
            return panel.gameObject;
        }

        /// <summary>Menú de pausa en su propio canvas, por encima del HUD.</summary>
        private static PauseMenu BuildPauseMenu(Transform parent, CraneInput input, GameSettings settings)
        {
            var canvas = Canvas(parent, "PauseCanvas", 10);

            var overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(canvas, false);
            Stretch((RectTransform)overlay.transform, Vector2.zero, Vector2.one);
            overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            var main = MenuPanel(overlay.transform, "MainPanel", new Vector2(360f, 380f));
            var title = Label(main, "Title", new Vector2(0f, 140f), new Vector2(340f, 44f), TextAnchor.MiddleCenter);
            title.fontSize = 30;
            Localize(title, "pause.title");
            var resume = MenuButton(main, "Resume", new Vector2(0f, 75f), "pause.resume");
            var controls = MenuButton(main, "Controls", new Vector2(0f, 17f), "pause.controls");
            var settingsButton = MenuButton(main, "Settings", new Vector2(0f, -41f), "pause.settings");
            var restart = MenuButton(main, "Restart", new Vector2(0f, -99f), "pause.restart");
            var hint = Label(main, "Hint", new Vector2(0f, -160f), new Vector2(340f, 24f), TextAnchor.MiddleCenter);
            hint.fontSize = 13;
            hint.color = new Color(1f, 1f, 1f, 0.6f);
            Localize(hint, "pause.hint");

            var controlsPanel = MenuPanel(overlay.transform, "ControlsPanel", new Vector2(560f, 460f));
            var controlsText = Label(controlsPanel, "Text", new Vector2(28f, -20f), new Vector2(504f, 360f), TextAnchor.UpperLeft);
            controlsText.fontSize = 17;
            controlsText.lineSpacing = 1.25f;
            Localize(controlsText, "pause.controls.text");
            var controlsBack = BackButton(controlsPanel, new Vector2(0f, 20f));

            var settingsPanel = BuildSettingsPanel(overlay.transform, settings, out var settingsBack);

            var pauseMenu = canvas.gameObject.AddComponent<PauseMenu>();
            pauseMenu.Configure(input, overlay, main.gameObject, controlsPanel.gameObject, settingsPanel);
            UnityEventTools.AddPersistentListener(resume.onClick, pauseMenu.Resume);
            UnityEventTools.AddPersistentListener(controls.onClick, pauseMenu.ShowControls);
            UnityEventTools.AddPersistentListener(settingsButton.onClick, pauseMenu.ShowSettings);
            UnityEventTools.AddPersistentListener(restart.onClick, pauseMenu.Restart);
            UnityEventTools.AddPersistentListener(controlsBack.onClick, pauseMenu.ShowMain);
            UnityEventTools.AddPersistentListener(settingsBack.onClick, pauseMenu.ShowMain);

            controlsPanel.gameObject.SetActive(false);
            settingsPanel.SetActive(false);
            overlay.SetActive(false);
            return pauseMenu;
        }

        /// <summary>
        /// Panel de configuración: dos toggles, cinco sliders (velocidades y sensibilidad), idioma,
        /// restablecer y volver. Las filas se ubican desde la esquina superior izquierda.
        /// </summary>
        private static GameObject BuildSettingsPanel(Transform overlay, GameSettings settings, out Button back)
        {
            var panel = MenuPanel(overlay, "SettingsPanel", new Vector2(560f, 520f));
            var title = Label(panel, "Title", new Vector2(0f, 222f), new Vector2(520f, 40f), TextAnchor.MiddleCenter);
            title.fontSize = 26;
            Localize(title, "settings.title");

            const float labelX = 30f;
            const float controlX = 300f;
            const float valueX = 490f;
            const float rowHeight = 30f;
            var y = -70f;

            Toggle ToggleRow(string key)
            {
                Localize(Label(panel, key, new Vector2(labelX, y), new Vector2(260f, rowHeight), TextAnchor.MiddleLeft), key);
                var toggle = ToggleControl(panel, key + ".toggle", new Vector2(controlX, y - 1f));
                y -= 42f;
                return toggle;
            }

            Slider SliderRow(string key, out Text value)
            {
                Localize(Label(panel, key, new Vector2(labelX, y), new Vector2(260f, rowHeight), TextAnchor.MiddleLeft), key);
                var slider = SliderControl(panel, key + ".slider", new Vector2(controlX, y - 5f), new Vector2(180f, 20f));
                value = Label(panel, key + ".value", new Vector2(valueX, y), new Vector2(60f, rowHeight), TextAnchor.MiddleRight);
                y -= 42f;
                return slider;
            }

            var controlsToggle = ToggleRow("settings.controls");
            var telemetryToggle = ToggleRow("settings.telemetry");
            y -= 10f;
            var speedKeys = new[] { "settings.speed.gantry", "settings.speed.trolley", "settings.speed.hoist", "settings.speed.spreader" };
            var speedSliders = new Slider[speedKeys.Length];
            var speedValues = new Text[speedKeys.Length];
            for (var i = 0; i < speedKeys.Length; i++)
            {
                speedSliders[i] = SliderRow(speedKeys[i], out speedValues[i]);
            }

            y -= 10f;
            var sensitivitySlider = SliderRow("settings.sensitivity", out var sensitivityValue);

            y -= 10f;
            Localize(Label(panel, "settings.language", new Vector2(labelX, y), new Vector2(260f, rowHeight), TextAnchor.MiddleLeft), "settings.language");
            var topLeft = new Vector2(0f, 1f);
            var (spanish, _) = TextButton(panel, "Spanish", topLeft, new Vector2(controlX, y), new Vector2(110f, 32f), "Español", 15);
            var (english, _) = TextButton(panel, "English", topLeft, new Vector2(controlX + 116f, y), new Vector2(110f, 32f), "English", 15);

            var (defaults, defaultsLabel) = TextButton(panel, "Defaults", new Vector2(0.5f, 0f), new Vector2(-110f, 20f), new Vector2(200f, 44f), "", 18);
            Localize(defaultsLabel, "settings.defaults");
            back = BackButton(panel, new Vector2(110f, 20f));

            panel.gameObject.AddComponent<SettingsView>().Configure(settings, controlsToggle, telemetryToggle, speedSliders, speedValues,
                sensitivitySlider, sensitivityValue, spanish, english, defaults);
            return panel.gameObject;
        }

        private static RectTransform MenuPanel(Transform parent, string name, Vector2 size)
        {
            var panel = Panel(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.14f, 0.95f);
            return panel;
        }

        private static Button MenuButton(Transform parent, string name, Vector2 position, string key)
        {
            var (button, label) = TextButton(parent, name, new Vector2(0.5f, 0.5f), position, new Vector2(260f, 48f), "", 19);
            Localize(label, key);
            return button;
        }

        private static Button BackButton(Transform parent, Vector2 position)
        {
            var (button, label) = TextButton(parent, "Back", new Vector2(0.5f, 0f), position, new Vector2(200f, 44f), "", 18);
            Localize(label, "pause.back");
            return button;
        }

        private static Toggle ToggleControl(Transform parent, string name, Vector2 position)
        {
            var box = Panel(parent, name, new Vector2(0f, 1f), position, new Vector2(30f, 30f));
            var background = box.GetComponent<Image>();
            background.color = new Color(0.22f, 0.25f, 0.3f, 1f);
            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(box, false);
            Stretch((RectTransform)check.transform, Vector2.zero, Vector2.one, 6f);
            var checkImage = check.GetComponent<Image>();
            checkImage.color = new Color(0.3f, 0.8f, 0.4f, 1f);

            var toggle = box.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = checkImage;
            toggle.isOn = true;
            return toggle;
        }

        private static Slider SliderControl(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(rect, false);
            Stretch((RectTransform)background.transform, new Vector2(0f, 0.3f), new Vector2(1f, 0.7f));
            background.GetComponent<Image>().color = new Color(0.22f, 0.25f, 0.3f, 1f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(rect, false);
            Stretch((RectTransform)fillArea.transform, new Vector2(0f, 0.3f), new Vector2(1f, 0.7f));
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            Stretch((RectTransform)fill.transform, Vector2.zero, Vector2.one);
            fill.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.75f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(rect, false);
            Stretch((RectTransform)handleArea.transform, Vector2.zero, Vector2.one);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            ((RectTransform)handle.transform).sizeDelta = new Vector2(14f, 0f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = Color.white;

            var slider = root.AddComponent<Slider>();
            slider.fillRect = (RectTransform)fill.transform;
            slider.handleRect = (RectTransform)handle.transform;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0.5f;
            slider.maxValue = 2f;
            slider.value = 1f;
            return slider;
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, float inset = 0f)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        /// <summary>Pone el texto de la clave y agrega LocalizedText para que cambie con el idioma.</summary>
        private static Text Localize(Text text, string key)
        {
            text.text = Strings.Get(key);
            text.gameObject.AddComponent<LocalizedText>().Configure(key);
            return text;
        }

        private static Transform Canvas(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        private static (Button button, Text label) TextButton(Transform parent, string name, Vector2 anchor, Vector2 position,
            Vector2 size, string text, int fontSize = 17)
        {
            var rect = Panel(parent, name, anchor, position, size);
            var image = rect.GetComponent<Image>();
            image.color = new Color(0.15f, 0.17f, 0.2f, 0.85f);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var label = Label(rect, "Text", Vector2.zero, size, TextAnchor.MiddleCenter);
            label.text = text;
            label.fontSize = fontSize;
            return (button, label);
        }

        private static RectTransform Panel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            return rect;
        }

        private static Text Label(Transform parent, string name, Vector2 position, Vector2 size, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            var centered = alignment == TextAnchor.MiddleCenter;
            rect.anchorMin = rect.anchorMax = rect.pivot = centered ? new Vector2(0.5f, 0.5f) : new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 17;
            text.lineSpacing = 1.1f;
            text.color = Color.white;
            text.alignment = alignment;
            text.supportRichText = true;
            text.raycastTarget = false;
            return text;
        }

        // ---------------------------------------------------------------- Helpers de física

        private static ArticulationBody Body(string name, Transform parent, Vector3 worldPosition, float mass)
        {
            var go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            go.transform.position = worldPosition;
            var body = go.AddComponent<ArticulationBody>();
            body.mass = mass;
            body.solverIterations = 16;
            body.solverVelocityIterations = 4;
            return body;
        }

        /// <summary>
        /// Prismatic con drive. <paramref name="min"/>, <paramref name="max"/> y <paramref name="start"/>
        /// están en la medida real; los límites físicos se corren para que start sea jointPosition 0.
        /// </summary>
        private static DrivenJoint DrivenPrismatic(ArticulationBody body, Quaternion axisRotation, string label,
            float min, float max, float start, float speed, float rampTime, float stiffness, float damping)
        {
            body.jointType = ArticulationJointType.PrismaticJoint;
            body.anchorRotation = axisRotation;
            body.linearLockX = ArticulationDofLock.LimitedMotion;
            body.linearLockY = ArticulationDofLock.LockedMotion;
            body.linearLockZ = ArticulationDofLock.LockedMotion;
            body.xDrive = Drive(min - start, max - start, 0f, stiffness, damping);
            return Driven(body, label, min, max, start, start, speed, rampTime);
        }

        /// <summary>
        /// Drive en modo Acceleration: no depende de la masa que cuelga, así que los mismos
        /// valores sirven con o sin carga enganchada.
        /// </summary>
        private static ArticulationDrive Drive(float lower, float upper, float target, float stiffness, float damping)
        {
            return new ArticulationDrive
            {
                driveType = ArticulationDriveType.Acceleration,
                lowerLimit = lower,
                upperLimit = upper,
                target = target,
                stiffness = stiffness,
                damping = damping,
                forceLimit = float.MaxValue
            };
        }

        private static DrivenJoint Driven(ArticulationBody body, string label, float min, float max, float initial, float offset, float speed, float rampTime)
        {
            var joint = body.gameObject.AddComponent<DrivenJoint>();
            joint.Configure(label, min, max, initial, offset, speed, rampTime);
            return joint;
        }

        /// <summary>
        /// Pone el ancla del padre en el mismo lugar que el ancla propia. Se hace a mano porque
        /// matchAnchors no la recalcula cuando el cuerpo se arma por código.
        /// </summary>
        private static void MatchParentAnchor(ArticulationBody body)
        {
            var self = body.transform;
            var parent = self.parent;
            body.matchAnchors = false;
            body.parentAnchorPosition = parent.InverseTransformPoint(self.TransformPoint(body.anchorPosition));
            body.parentAnchorRotation = Quaternion.Inverse(parent.rotation) * self.rotation * body.anchorRotation;
        }

        /// <summary>Inercia de caja para cuerpos sin collider (si no, Unity usa un valor por defecto).</summary>
        private static void SetBoxInertia(ArticulationBody body, Vector3 size, Vector3 centerOfMass)
        {
            var m = body.mass / 12f;
            body.automaticCenterOfMass = false;
            body.centerOfMass = centerOfMass;
            body.automaticInertiaTensor = false;
            body.inertiaTensor = new Vector3(
                m * (size.y * size.y + size.z * size.z),
                m * (size.x * size.x + size.z * size.z),
                m * (size.x * size.x + size.y * size.y));
            body.inertiaTensorRotation = Quaternion.identity;
        }

        // ---------------------------------------------------------------- Helpers de geometría

        private static Transform Group(string name)
        {
            return new GameObject(name).transform;
        }

        private static GameObject Box(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material, bool collider = true)
        {
            var go = Shape(parent, name, BuilderAssets.Box(size), localPosition, material);
            if (collider)
            {
                go.AddComponent<BoxCollider>().size = size;
            }

            return go;
        }

        /// <summary>
        /// Instancia un prefab modelado en Blender (escala 1,1,1, pivote en el centro de
        /// <paramref name="colliderSize"/>) con collider y material consistentes con el resto de
        /// la escena. Si el prefab no está (no se corrió el flujo de Blender), cae a una caja.
        /// </summary>
        private static GameObject ImportedShape(Transform parent, string name, string prefabPath,
            Vector3 localPosition, Vector3 colliderSize, Material material)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return Box(parent, name, localPosition, colliderSize, material);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            if (instance.TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.sharedMaterial = material;
            }

            instance.AddComponent<BoxCollider>().size = colliderSize;
            return instance;
        }

        private static GameObject Shape(Transform parent, string name, Mesh mesh, Vector3 localPosition, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        private sealed class Materials
        {
            public readonly Material CraneRed = BuilderAssets.Material("CraneRed", new Color(0.75f, 0.12f, 0.1f));
            public readonly Material White = BuilderAssets.Material("CraneWhite", new Color(0.92f, 0.92f, 0.9f));
            public readonly Material Yellow = BuilderAssets.Material("SpreaderYellow", new Color(0.95f, 0.75f, 0.1f));
            public readonly Material Steel = BuilderAssets.Material("Steel", new Color(0.35f, 0.36f, 0.38f), 0.6f, 0.8f);
            public readonly Material Concrete = BuilderAssets.Material("Concrete", new Color(0.55f, 0.55f, 0.53f), 0.1f);
            public readonly Material Water = BuilderAssets.Material("Water", new Color(0.12f, 0.3f, 0.45f), 0.9f);
            public readonly Material Hull = BuilderAssets.Material("ShipHull", new Color(0.15f, 0.18f, 0.22f));
            public readonly Material ContainerBlue = BuilderAssets.Material("ContainerBlue", new Color(0.12f, 0.3f, 0.6f));
            public readonly Material ContainerGreen = BuilderAssets.Material("ContainerGreen", new Color(0.15f, 0.45f, 0.25f));
            public readonly Material ContainerOrange = BuilderAssets.Material("ContainerOrange", new Color(0.85f, 0.4f, 0.1f));
            public readonly Material ContainerGrey = BuilderAssets.Material("ContainerGrey", new Color(0.5f, 0.52f, 0.55f));
            public readonly Material Wood = BuilderAssets.Material("Wood", new Color(0.72f, 0.55f, 0.33f), 0.15f);
            public readonly Material ZoneFill = BuilderAssets.Material("ZoneFill", new Color(0.62f, 0.6f, 0.45f), 0.1f);
            public readonly Material Light = BuilderAssets.Material("StatusLight", new Color(0.85f, 0.1f, 0.1f), 0.8f);
            public readonly Material Glass = BuilderAssets.TransparentMaterial("Glass", new Color(0.55f, 0.7f, 0.78f, 0.18f));
            public readonly Material CabinFrame = BuilderAssets.Material("CabinFrame", new Color(0.36f, 0.62f, 0.52f), 0.4f);
            public readonly Material CabinPanel = BuilderAssets.Material("CabinPanel", new Color(0.42f, 0.7f, 0.6f), 0.3f);
            public readonly Material ConsoleGrey = BuilderAssets.Material("ConsoleGrey", new Color(0.75f, 0.76f, 0.78f), 0.4f);
            public readonly Material Dark = BuilderAssets.Material("Dark", new Color(0.12f, 0.12f, 0.13f), 0.3f);
            public readonly Material Screen = BuilderAssets.Material("Screen", new Color(0.15f, 0.25f, 0.35f), 0.9f);
        }
    }
}
