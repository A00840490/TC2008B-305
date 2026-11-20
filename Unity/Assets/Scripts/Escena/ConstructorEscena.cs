using UnityEngine;
using UnityEngine.UI;

namespace Codea.SharkTank
{
    // Crea las piezas de la escena. Lo usa el arranque automático (en Play)
    // y el menú Codea del editor, que además guarda las piezas como prefabs.
    public interface IFabrica
    {
        GameObject Juez(int indice);
        GameObject Emprendedor();
        GameObject Globo();
        GameObject Barra();
    }

    // Crea las piezas directamente, sin prefabs.
    public class FabricaDirecta : IFabrica
    {
        public GameObject Juez(int indice) => ConstructorEscena.CrearJuez(ConstructorEscena.Jueces[indice]);
        public GameObject Emprendedor() => ConstructorEscena.CrearEmprendedor();
        public GameObject Globo() => ConstructorEscena.CrearGlobo();
        public GameObject Barra() => ConstructorEscena.CrearBarra();
    }

    // Cómo se viste y se ve cada juez. Cada uno tiene un área de especialización distinta.
    public class EstiloJuez
    {
        public string nombre;
        public Color traje;
        public Color solapa;
        public Color camisa;
        public Color corbata;
        public bool mono;        // moño en lugar de corbata
        public bool panuelo;     // pañuelo en la bolsa del saco
        public Color piel;
        public Color pelo;
        public bool gafas;
        public bool barba;

        // Nombre sin acentos, para los nombres de objetos y archivos.
        public string Clave => nombre.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
    }

    public static class ConstructorEscena
    {
        // Colores de la identidad SharkMind (ver SharkMindColors).
        static readonly Color ColorPiso = new Color(0.05f, 0.09f, 0.16f);
        static readonly Color ColorPared = SharkMindColors.Ocean;
        static readonly Color ColorAcento = SharkMindColors.Oro;
        static readonly Color ColorMadera = new Color(0.58f, 0.38f, 0.23f);
        static readonly Color ColorPiel = new Color(0.93f, 0.76f, 0.62f);
        static readonly Color ColorEmprendedor = SharkMindColors.Aleta;
        static readonly Color ColorCuero = new Color(0.09f, 0.08f, 0.08f);
        static readonly Color ColorMetal = SharkMindColors.Tiburon;
        static readonly Color ColorPanel = new Color(11 / 255f, 31 / 255f, 58 / 255f, 0.92f);
        static readonly Color ColorGlobo = SharkMindColors.Espuma;
        static readonly Color ColorTextoSuave = new Color(0.72f, 0.78f, 0.84f);

        // Los tres jueces, de más cerca a más lejos de la cámara.
        public static readonly EstiloJuez[] Jueces =
        {
            // Finanzas: traje azul marino, corbata dorada, pañuelo, barba y pelo con canas.
            new EstiloJuez
            {
                nombre = "Finanzas", traje = new Color(0.035f, 0.09f, 0.17f), solapa = new Color(0.086f, 0.2f, 0.353f),
                camisa = SharkMindColors.Espuma, corbata = SharkMindColors.Oro, panuelo = true,
                piel = new Color(0.93f, 0.76f, 0.62f), pelo = new Color(0.8f, 0.8f, 0.82f), barba = true
            },
            // Innovación: traje gris carbón, camisa azul claro, corbata Aleta y gafas.
            new EstiloJuez
            {
                nombre = "Innovación", traje = new Color(0.15f, 0.16f, 0.18f), solapa = new Color(0.24f, 0.25f, 0.28f),
                camisa = new Color(0.82f, 0.89f, 0.96f), corbata = SharkMindColors.Aleta,
                piel = new Color(0.8f, 0.6f, 0.45f), pelo = new Color(0.16f, 0.1f, 0.07f), gafas = true
            },
            // Mercado: traje verde bosque y moño rojo.
            new EstiloJuez
            {
                nombre = "Mercado", traje = new Color(0.07f, 0.2f, 0.15f), solapa = new Color(0.11f, 0.29f, 0.22f),
                camisa = SharkMindColors.Espuma, corbata = SharkMindColors.Mordida, mono = true,
                piel = new Color(0.6f, 0.42f, 0.3f), pelo = new Color(0.07f, 0.06f, 0.06f)
            },
        };

        // Dónde va cada quien. Los jueces van en fila, de frente al emprendedor, que está arriba de la tarima.
        public static readonly Vector3 PosicionJuezCentral = new Vector3(2.8f, 0f, 2.2f);
        const float SeparacionJueces = 1.7f;
        public static readonly Vector3 PosicionEscenario = new Vector3(-2.6f, 0f, 2.2f);
        const float AlturaEscenario = 0.3f;

        // Tarima: más larga que ancha, se alarga hacia el juez.
        const float LargoTarima = 4.0f;  // en x, de lado a lado
        const float AnchoTarima = 2.5f;  // en z, de frente a fondo
        const float CentroTarimaX = 0.45f; // qué tanto se corre el centro hacia el juez desde donde está el emprendedor

        // Pared de madera con el logo, a la espalda del juez (el juez ve hacia el emprendedor, la pared ve hacia el mismo lado).
        // Es ancha para cubrir a los tres jueces.
        static readonly Vector3 PosicionPared = new Vector3(4.45f, 0f, 2.2f);
        const float ParedMaderaAncho = 9.6f;
        const float ParedMaderaAlto = 5.6f;
        // En coordenadas de la pared: x a lo ancho, y hacia arriba, -z hacia el frente.
        static readonly Vector3 CentroLogo = new Vector3(0f, 2.75f, 0f);
        const float DiametroLogo = 3.0f;

        static Font fuente;

        public static Font Fuente()
        {
            if (fuente != null)
                return fuente;
            // LegacyRuntime.ttf existe desde Unity 2022.2; Arial.ttf es el nombre en versiones anteriores.
            try { fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (fuente == null)
                try { fuente = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            return fuente;
        }

        // ---------- Escena completa ----------

        public static GestorConversacion Construir(IFabrica fabrica)
        {
            var raiz = new GameObject("SharkTank").transform;

            ConstruirSet(raiz);

            var emprendedor = fabrica.Emprendedor().GetComponent<PersonajeAgente>();
            emprendedor.transform.SetParent(raiz, false);
            emprendedor.transform.localPosition = PosicionEscenario + Vector3.up * AlturaEscenario;
            emprendedor.Configurar("Emprendedor", ColorEmprendedor);

            var jueces = new PersonajeAgente[Jueces.Length];
            for (int i = 0; i < jueces.Length; i++)
            {
                var j = fabrica.Juez(i).GetComponent<PersonajeAgente>();
                j.transform.SetParent(raiz, false);
                j.transform.localPosition = PosicionJuezCentral + new Vector3(0f, 0f, (i - (jueces.Length - 1) * 0.5f) * SeparacionJueces);
                j.Configurar(Jueces[i].nombre, Jueces[i].traje);
                // Cada juez ve hacia el emprendedor.
                Mirar(j.transform, emprendedor.transform);
                jueces[i] = j;
            }
            // El emprendedor empieza viendo al juez de en medio.
            Mirar(emprendedor.transform, jueces[jueces.Length / 2].transform);

            var camara = PrepararCamaraYLuz();

            // Interfaz
            var canvas = CrearCanvas(raiz);
            var panel = CrearPanel(canvas, fabrica);

            var globo = fabrica.Globo().GetComponent<GloboDialogo>();
            globo.transform.SetParent(canvas, false);
            globo.gameObject.SetActive(false);

            var titulo = CrearTexto(canvas, "Titulo", "SharkMind: Simulación", 40, TextAnchor.UpperLeft, Color.white, FontStyle.Bold);
            Colocar(titulo.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -20), new Vector2(800, 52));

            var banner = CrearImagen(canvas, "Resultado", ColorPanel);
            Colocar(banner.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(760, 90));
            var textoBanner = CrearTexto(banner.transform, "Texto", "", 38, TextAnchor.MiddleCenter, ColorAcento, FontStyle.Bold);
            Estirar(textoBanner.rectTransform, 10, 10, 10, 10);
            banner.gameObject.SetActive(false);

            // Lógica
            var logica = new GameObject("Logica");
            logica.transform.SetParent(raiz, false);
            var gestor = logica.AddComponent<GestorConversacion>();
            gestor.jueces = jueces;
            gestor.emprendedor = emprendedor;
            gestor.globo = globo;
            gestor.panel = panel;
            // Sin texto de estado en pantalla: el estado de la conexión va a la consola de Unity.
            gestor.bannerResultado = banner.gameObject;
            gestor.textoResultado = textoBanner;

            var director = logica.AddComponent<CamaraDirector>();
            director.camara = camara;
            director.jueces = jueces;
            director.emprendedor = emprendedor;
            gestor.director = director;

            var cliente = logica.AddComponent<ClienteAPI>();
            cliente.gestor = gestor;
            cliente.sesionEjemplo = Resources.Load<TextAsset>("sesion_ejemplo");

            return gestor;
        }

        static void ConstruirSet(Transform raiz)
        {
            var set = new GameObject("Escenario").transform;
            set.SetParent(raiz, false);
            Primitiva(PrimitiveType.Plane, "Piso", set, new Vector3(0f, 0f, 1.5f), new Vector3(2.4f, 1f, 2f), ColorPiso);
            Primitiva(PrimitiveType.Cube, "Pared", set, new Vector3(0f, 3.5f, 8f), new Vector3(24f, 7f, 0.3f), ColorPared);
            Primitiva(PrimitiveType.Cube, "Franja", set, new Vector3(0f, 4.4f, 7.83f), new Vector3(24f, 0.2f, 0.05f), ColorAcento);

            ConstruirParedMadera(set);
            ConstruirTarima(set);
        }

        // Pared de tablones de madera a la espalda del juez, con el icono redondo de SharkMind y dos focos que lo iluminan.
        // Se arma de frente (viendo hacia -z) y luego se gira para que vea hacia el emprendedor.
        static void ConstruirParedMadera(Transform set)
        {
            var pared = new GameObject("ParedMadera").transform;
            pared.SetParent(set, false);
            pared.localPosition = PosicionPared;
            // Con 90° en y, el frente de la pared (-z) queda viendo hacia -x, donde está el emprendedor.
            pared.localRotation = Quaternion.Euler(0f, 90f, 0f);

            var madera = Resources.Load<Texture2D>("SharkMind/madera");
            var icono = Resources.Load<Texture2D>("SharkMind/icono-sharkmind");

            // Fondo oscuro: se ve entre tablón y tablón como junta.
            float izquierda = -ParedMaderaAncho * 0.5f;
            Primitiva(PrimitiveType.Cube, "Fondo", pared, new Vector3(0f, ParedMaderaAlto * 0.5f, 0.06f),
                new Vector3(ParedMaderaAncho, ParedMaderaAlto, 0.04f), new Color(0.12f, 0.07f, 0.04f));

            const int tablones = 16;
            float ancho = ParedMaderaAncho / tablones;
            // Variaciones fijas para que cada tablón tenga un tono distinto pero la escena salga igual siempre.
            float[] tono = { 1.00f, 0.90f, 1.07f, 0.95f, 1.03f, 0.88f, 1.05f, 0.97f, 0.92f, 1.08f, 0.94f, 1.01f, 0.89f, 1.06f, 0.96f, 1.02f };
            for (int i = 0; i < tablones; i++)
            {
                var color = ColorMadera * tono[i];
                color.a = 1f;
                var tablon = Primitiva(PrimitiveType.Cube, "Tablon" + (i + 1), pared,
                    new Vector3(izquierda + ancho * (i + 0.5f), ParedMaderaAlto * 0.5f, 0f),
                    new Vector3(ancho - 0.03f, ParedMaderaAlto, 0.08f), color);
                // Cada tablón usa un pedazo distinto de la veta.
                tablon.GetComponent<ColorSolido>().Texturizar(madera, new Vector2(0.5f, 1f), 0.15f, new Vector2((i * 0.37f) % 1f, 0f));
            }

            // Molduras arriba y abajo.
            Primitiva(PrimitiveType.Cube, "MolduraArriba", pared, new Vector3(0f, ParedMaderaAlto + 0.06f, -0.02f),
                new Vector3(ParedMaderaAncho + 0.2f, 0.12f, 0.16f), new Color(0.2f, 0.12f, 0.07f));
            Primitiva(PrimitiveType.Cube, "Zoclo", pared, new Vector3(0f, 0.08f, -0.03f),
                new Vector3(ParedMaderaAncho + 0.2f, 0.16f, 0.14f), new Color(0.2f, 0.12f, 0.07f));

            // Solo el icono redondo, grande, sin nombre ni marco. El Quad ve hacia -z (el frente de la pared)
            // y lo de afuera del círculo se recorta con la transparencia del PNG.
            var logo = Primitiva(PrimitiveType.Quad, "IconoSharkMind", pared, CentroLogo + new Vector3(0f, 0f, -0.05f),
                new Vector3(DiametroLogo, DiametroLogo, 1f), Color.white);
            var pintura = logo.GetComponent<ColorSolido>();
            pintura.recorte = true;
            pintura.Texturizar(icono, Vector2.one, 0.1f);

            // Dos focos colgados de la parte de arriba de la pared, apuntando al icono.
            CrearFocoPared(pared, -2.0f);
            CrearFocoPared(pared, 2.0f);
        }

        static void CrearFocoPared(Transform pared, float desplazamientoX)
        {
            float x = desplazamientoX;
            var posicion = new Vector3(x, ParedMaderaAlto - 0.4f, -1.0f);
            string lado = desplazamientoX < 0f ? "Izquierdo" : "Derecho";

            var foco = new GameObject("Foco" + lado).transform;
            foco.SetParent(pared, false);
            foco.localPosition = posicion;
            // Apunta a la mitad del icono de su lado, para que entre los dos lo cubran parejo.
            foco.LookAt(pared.TransformPoint(CentroLogo + new Vector3(desplazamientoX * 0.3f, 0f, 0f)));

            // Brazo desde la pared hasta el foco.
            float largo = -posicion.z;
            Primitiva(PrimitiveType.Cube, "Brazo" + lado, pared, new Vector3(x, posicion.y + 0.2f, posicion.z + largo * 0.5f),
                new Vector3(0.05f, 0.05f, largo), new Color(0.08f, 0.08f, 0.09f));
            Primitiva(PrimitiveType.Cube, "Soporte" + lado, pared, new Vector3(x, posicion.y + 0.1f, posicion.z),
                new Vector3(0.04f, 0.2f, 0.04f), new Color(0.08f, 0.08f, 0.09f));

            // Cuerpo del foco: un cilindro acostado a lo largo de hacia dónde apunta.
            var cuerpo = Primitiva(PrimitiveType.Cylinder, "Carcasa", foco, new Vector3(0f, 0f, -0.05f), new Vector3(0.24f, 0.16f, 0.24f), new Color(0.08f, 0.08f, 0.09f));
            cuerpo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Brillo(cuerpo, 0.7f);
            var aro = Primitiva(PrimitiveType.Cylinder, "Aro", foco, new Vector3(0f, 0f, 0.11f), new Vector3(0.27f, 0.02f, 0.27f), ColorAcento);
            aro.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var lente = Primitiva(PrimitiveType.Cylinder, "Lente", foco, new Vector3(0f, 0f, 0.125f), new Vector3(0.2f, 0.005f, 0.2f), new Color(1f, 0.96f, 0.82f));
            lente.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Brillo(lente, 0.9f);

            var luz = new GameObject("Luz").AddComponent<Light>();
            luz.transform.SetParent(foco, false);
            luz.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            luz.type = LightType.Spot;
            luz.spotAngle = 60f;
            luz.innerSpotAngle = 28f;
            luz.range = 8f;
            luz.intensity = 3.5f;
            luz.color = new Color(1f, 0.93f, 0.8f);
            luz.shadows = LightShadows.Soft;
            // Siempre por pixel: si no, con pocas luces por pixel en Quality el cono no se ve en la pared.
            luz.renderMode = LightRenderMode.ForcePixel;
        }

        // Tarima ovalada donde se para el emprendedor a presentar, como escenario. Es más larga que ancha
        // y se alarga hacia el juez.
        static void ConstruirTarima(Transform set)
        {
            var tarima = new GameObject("Tarima").transform;
            tarima.SetParent(set, false);
            tarima.localPosition = PosicionEscenario + new Vector3(CentroTarimaX, 0f, 0f);

            // El cilindro de Unity mide 2 de alto, por eso la escala en y es la mitad de la altura.
            // Con escalas distintas en x y z el cilindro queda ovalado.
            Primitiva(PrimitiveType.Cylinder, "Base", tarima, new Vector3(0f, AlturaEscenario * 0.5f - 0.01f, 0f),
                new Vector3(LargoTarima, AlturaEscenario * 0.5f - 0.01f, AnchoTarima), new Color(0.06f, 0.07f, 0.09f));
            // Filo dorado y la superficie en Océano.
            Brillo(Primitiva(PrimitiveType.Cylinder, "Filo", tarima, new Vector3(0f, AlturaEscenario - 0.02f, 0f),
                new Vector3(LargoTarima + 0.04f, 0.02f, AnchoTarima + 0.04f), ColorAcento), 0.7f);
            Brillo(Primitiva(PrimitiveType.Cylinder, "Superficie", tarima, new Vector3(0f, AlturaEscenario - 0.005f, 0f),
                new Vector3(LargoTarima - 0.12f, 0.01f, AnchoTarima - 0.12f), SharkMindColors.Ocean), 0.45f);
            // Escalón al frente, hacia la cámara, abajo de donde está el emprendedor.
            float frente = -AnchoTarima * 0.5f;
            Primitiva(PrimitiveType.Cube, "Escalon", tarima, new Vector3(-CentroTarimaX, AlturaEscenario * 0.25f, frente - 0.15f),
                new Vector3(1.0f, AlturaEscenario * 0.5f, 0.4f), new Color(0.06f, 0.07f, 0.09f));
            Primitiva(PrimitiveType.Cube, "FiloEscalon", tarima, new Vector3(-CentroTarimaX, AlturaEscenario * 0.5f, frente - 0.33f),
                new Vector3(1.0f, 0.015f, 0.04f), ColorAcento);
        }

        static void Mirar(Transform quien, Transform aQuien)
        {
            Vector3 destino = aQuien.position;
            destino.y = quien.position.y;
            quien.LookAt(destino);
        }

        static Camera PrepararCamaraYLuz()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            // Misma toma general que usa CamaraDirector.
            // Misma toma general que usa CamaraDirector: alta y con perspectiva para que quepa todo el set.
            cam.transform.position = new Vector3(-0.5f, 6.5f, -10.5f);
            cam.transform.LookAt(new Vector3(0.2f, 1.2f, 2.2f));
            cam.fieldOfView = 42f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.04f, 0.08f);

            Light direccional = null;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional)
                    direccional = l;
            if (direccional == null)
            {
                direccional = new GameObject("Directional Light").AddComponent<Light>();
                direccional.type = LightType.Directional;
                direccional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                direccional.shadows = LightShadows.Soft;
            }
            // Luz general más baja para que se noten los focos sobre la pared de madera.
            direccional.intensity = 0.6f;
            return cam;
        }

        // ---------- Piezas (también se guardan como prefabs) ----------

        // El emprendedor: de pie, con el cuerpo de cápsula. Se para sobre la tarima.
        public static GameObject CrearEmprendedor()
        {
            var p = NuevoPersonaje("PersonajeEmprendedor", out var cuerpo);

            var torso = Primitiva(PrimitiveType.Capsule, "Torso", cuerpo, new Vector3(0f, 0.9f, 0f), new Vector3(0.75f, 0.9f, 0.75f), p.colorBase);
            p.torso = torso.GetComponent<ColorSolido>();
            p.ropa = new ColorSolido[0];
            CrearCabeza(p, cuerpo, 2.05f);
            TerminarPersonaje(p, 2.05f);
            return p.gameObject;
        }

        // Un juez sentado en su silla, con traje formal. Color del traje, camisa, corbata o moño, piel, pelo,
        // gafas y barba cambian según su estilo.
        public static GameObject CrearJuez(EstiloJuez e)
        {
            var p = NuevoPersonaje("PersonajeJuez" + e.Clave, out var cuerpo);
            p.colorBase = e.traje;
            CrearSilla(p.transform);

            const float asiento = 0.62f; // altura de la cadera, arriba del cojín de la silla
            var ropa = new System.Collections.Generic.List<ColorSolido>();

            // Saco
            var torso = Primitiva(PrimitiveType.Capsule, "Saco", cuerpo, new Vector3(0f, asiento + 0.52f, -0.05f), new Vector3(0.72f, 0.56f, 0.5f), p.colorBase);
            p.torso = torso.GetComponent<ColorSolido>();
            Brillo(torso, 0.25f);

            // Camisa: un cubo girado 45° que asoma en V por el cuello del saco.
            var camisa = Primitiva(PrimitiveType.Cube, "Camisa", cuerpo, new Vector3(0f, asiento + 0.9f, 0.17f), new Vector3(0.24f, 0.24f, 0.12f), e.camisa);
            camisa.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            // Solapas
            var solapaIzq = Primitiva(PrimitiveType.Cube, "SolapaIzquierda", cuerpo, new Vector3(-0.11f, asiento + 0.74f, 0.19f), new Vector3(0.09f, 0.36f, 0.04f), e.solapa);
            solapaIzq.transform.localRotation = Quaternion.Euler(-12f, 0f, -22f);
            var solapaDer = Primitiva(PrimitiveType.Cube, "SolapaDerecha", cuerpo, new Vector3(0.11f, asiento + 0.74f, 0.19f), new Vector3(0.09f, 0.36f, 0.04f), e.solapa);
            solapaDer.transform.localRotation = Quaternion.Euler(-12f, 0f, 22f);
            if (e.mono)
            {
                // Moño: dos rombos a los lados y un nudo en medio.
                Primitiva(PrimitiveType.Cube, "NudoMono", cuerpo, new Vector3(0f, asiento + 0.86f, 0.235f), new Vector3(0.05f, 0.05f, 0.05f), e.corbata);
                for (int lado = -1; lado <= 1; lado += 2)
                {
                    var ala = Primitiva(PrimitiveType.Cube, lado < 0 ? "MonoIzquierdo" : "MonoDerecho", cuerpo,
                        new Vector3(0.055f * lado, asiento + 0.86f, 0.225f), new Vector3(0.07f, 0.07f, 0.04f), e.corbata);
                    ala.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                }
            }
            else
            {
                Primitiva(PrimitiveType.Cube, "NudoCorbata", cuerpo, new Vector3(0f, asiento + 0.86f, 0.225f), new Vector3(0.08f, 0.07f, 0.06f), e.corbata);
                Primitiva(PrimitiveType.Cube, "Corbata", cuerpo, new Vector3(0f, asiento + 0.62f, 0.215f), new Vector3(0.08f, 0.4f, 0.03f), e.corbata);
                var punta = Primitiva(PrimitiveType.Cube, "PuntaCorbata", cuerpo, new Vector3(0f, asiento + 0.42f, 0.215f), new Vector3(0.0566f, 0.0566f, 0.03f), e.corbata);
                punta.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
            if (e.panuelo)
                Primitiva(PrimitiveType.Cube, "Panuelo", cuerpo, new Vector3(-0.2f, asiento + 0.74f, 0.17f), new Vector3(0.09f, 0.04f, 0.03f), ColorAcento);

            // Brazos sobre los descansabrazos de la silla.
            for (int lado = -1; lado <= 1; lado += 2)
            {
                string n = lado < 0 ? "Izquierdo" : "Derecho";
                var brazo = Primitiva(PrimitiveType.Capsule, "Brazo" + n, cuerpo, new Vector3(0.4f * lado, asiento + 0.55f, -0.05f), new Vector3(0.17f, 0.29f, 0.17f), p.colorBase);
                brazo.transform.localRotation = Quaternion.Euler(0f, 0f, 8f * lado);
                ropa.Add(brazo.GetComponent<ColorSolido>());
                var antebrazo = Primitiva(PrimitiveType.Capsule, "Antebrazo" + n, cuerpo, new Vector3(0.42f * lado, asiento + 0.27f, 0.13f), new Vector3(0.15f, 0.22f, 0.15f), p.colorBase);
                antebrazo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                ropa.Add(antebrazo.GetComponent<ColorSolido>());
                Primitiva(PrimitiveType.Cube, "Puno" + n, cuerpo, new Vector3(0.42f * lado, asiento + 0.27f, 0.34f), new Vector3(0.12f, 0.12f, 0.03f), e.camisa);
                Primitiva(PrimitiveType.Sphere, "Mano" + n, cuerpo, new Vector3(0.42f * lado, asiento + 0.27f, 0.41f), Vector3.one * 0.13f, e.piel);

                // Pantalón: muslo hacia el frente y pierna hacia abajo.
                var muslo = Primitiva(PrimitiveType.Capsule, "Muslo" + n, cuerpo, new Vector3(0.14f * lado, asiento + 0.04f, 0.2f), new Vector3(0.23f, 0.3f, 0.23f), p.colorBase);
                muslo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                ropa.Add(muslo.GetComponent<ColorSolido>());
                var pierna = Primitiva(PrimitiveType.Capsule, "Pierna" + n, cuerpo, new Vector3(0.14f * lado, (asiento + 0.04f) * 0.5f, 0.44f), new Vector3(0.19f, (asiento + 0.04f) * 0.5f, 0.19f), p.colorBase);
                ropa.Add(pierna.GetComponent<ColorSolido>());
                Brillo(Primitiva(PrimitiveType.Cube, "Zapato" + n, cuerpo, new Vector3(0.14f * lado, 0.05f, 0.52f), new Vector3(0.15f, 0.1f, 0.3f), new Color(0.05f, 0.05f, 0.05f)), 0.8f);
            }
            p.ropa = ropa.ToArray();

            // Cuello y cabeza
            Primitiva(PrimitiveType.Cylinder, "Cuello", cuerpo, new Vector3(0f, asiento + 1.05f, -0.02f), new Vector3(0.18f, 0.08f, 0.18f), e.piel);
            float alturaCabeza = asiento + 1.3f;
            CrearCabeza(p, cuerpo, alturaCabeza, e.piel);
            CrearPelo(cuerpo, alturaCabeza, e.pelo, e.barba);
            if (e.barba)
                CrearBarba(cuerpo, alturaCabeza, e.pelo);
            if (e.gafas)
                CrearGafas(cuerpo, alturaCabeza);
            TerminarPersonaje(p, alturaCabeza);
            return p.gameObject;
        }

        // Silla ejecutiva de piel. Va dentro del juez (fuera de "Cuerpo", para que no se mueva cuando habla).
        static void CrearSilla(Transform juez)
        {
            var silla = new GameObject("Silla").transform;
            silla.SetParent(juez, false);

            Brillo(Primitiva(PrimitiveType.Cylinder, "Base", silla, new Vector3(0f, 0.03f, -0.05f), new Vector3(0.7f, 0.03f, 0.7f), ColorMetal), 0.8f);
            Brillo(Primitiva(PrimitiveType.Cylinder, "Poste", silla, new Vector3(0f, 0.27f, -0.05f), new Vector3(0.09f, 0.22f, 0.09f), ColorMetal), 0.8f);
            Brillo(Primitiva(PrimitiveType.Cube, "Asiento", silla, new Vector3(0f, 0.53f, 0.02f), new Vector3(0.8f, 0.14f, 0.72f), ColorCuero), 0.55f);
            var respaldo = Primitiva(PrimitiveType.Cube, "Respaldo", silla, new Vector3(0f, 1.25f, -0.4f), new Vector3(0.8f, 1.3f, 0.14f), ColorCuero);
            respaldo.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            Brillo(respaldo, 0.55f);
            for (int lado = -1; lado <= 1; lado += 2)
            {
                string n = lado < 0 ? "Izquierdo" : "Derecho";
                Brillo(Primitiva(PrimitiveType.Cube, "Descansabrazos" + n, silla, new Vector3(0.45f * lado, 0.8f, 0.05f), new Vector3(0.1f, 0.06f, 0.6f), ColorCuero), 0.55f);
                Primitiva(PrimitiveType.Cube, "SoporteBrazo" + n, silla, new Vector3(0.42f * lado, 0.68f, -0.05f), new Vector3(0.05f, 0.2f, 0.05f), ColorMetal);
            }
        }

        static PersonajeAgente NuevoPersonaje(string nombre, out Transform cuerpo)
        {
            var go = new GameObject(nombre);
            var p = go.AddComponent<PersonajeAgente>();
            cuerpo = new GameObject("Cuerpo").transform;
            cuerpo.SetParent(go.transform, false);
            p.cuerpo = cuerpo;
            return p;
        }

        static void CrearCabeza(PersonajeAgente p, Transform cuerpo, float altura)
        {
            CrearCabeza(p, cuerpo, altura, ColorPiel);
        }

        static void CrearCabeza(PersonajeAgente p, Transform cuerpo, float altura, Color piel)
        {
            var cabeza = Primitiva(PrimitiveType.Sphere, "Cabeza", cuerpo, new Vector3(0f, altura, 0f), Vector3.one * 0.55f, piel);
            p.cabeza = cabeza.transform;
            // La cara ve hacia +z (forward), que es hacia donde gira el personaje.
            Primitiva(PrimitiveType.Sphere, "OjoIzquierdo", cuerpo, new Vector3(-0.1f, altura + 0.05f, 0.24f), Vector3.one * 0.08f, Color.black);
            Primitiva(PrimitiveType.Sphere, "OjoDerecho", cuerpo, new Vector3(0.1f, altura + 0.05f, 0.24f), Vector3.one * 0.08f, Color.black);
            Primitiva(PrimitiveType.Sphere, "Nariz", cuerpo, new Vector3(0f, altura - 0.03f, 0.27f), Vector3.one * 0.07f, piel * 0.88f + new Color(0f, 0f, 0f, 0.12f));
        }

        // Pelo: una esfera un poco más grande que la cabeza, corrida hacia arriba y atrás, para que la cara quede libre.
        // Con canas (barba) entra un poco más atrás, como entradas.
        static void CrearPelo(Transform cuerpo, float altura, Color pelo, bool entradas)
        {
            float atras = entradas ? -0.06f : -0.04f;
            Brillo(Primitiva(PrimitiveType.Sphere, "Pelo", cuerpo, new Vector3(0f, altura + 0.07f, atras), new Vector3(0.6f, 0.5f, 0.6f), pelo), 0.2f);
        }

        // Barba y bigote del mismo color que el pelo.
        static void CrearBarba(Transform cuerpo, float altura, Color pelo)
        {
            Brillo(Primitiva(PrimitiveType.Sphere, "Barba", cuerpo, new Vector3(0f, altura - 0.13f, 0.09f), new Vector3(0.44f, 0.3f, 0.36f), pelo), 0.1f);
            Primitiva(PrimitiveType.Cube, "Bigote", cuerpo, new Vector3(0f, altura - 0.08f, 0.262f), new Vector3(0.13f, 0.035f, 0.04f), pelo);
        }

        // Gafas de armazón oscuro: dos marcos cuadrados, puente y patillas.
        static void CrearGafas(Transform cuerpo, float altura)
        {
            var color = new Color(0.05f, 0.05f, 0.06f);
            var gafas = new GameObject("Gafas").transform;
            gafas.SetParent(cuerpo, false);
            gafas.localPosition = new Vector3(0f, altura + 0.05f, 0f);
            const float z = 0.285f, g = 0.016f;
            for (int lado = -1; lado <= 1; lado += 2)
            {
                float x = 0.1f * lado;
                string n = lado < 0 ? "Izquierda" : "Derecha";
                Primitiva(PrimitiveType.Cube, "Arriba" + n, gafas, new Vector3(x, 0.042f, z), new Vector3(0.13f, g, g), color);
                Primitiva(PrimitiveType.Cube, "Abajo" + n, gafas, new Vector3(x, -0.042f, z), new Vector3(0.13f, g, g), color);
                Primitiva(PrimitiveType.Cube, "Dentro" + n, gafas, new Vector3(x - 0.057f * lado, 0f, z), new Vector3(g, 0.1f, g), color);
                Primitiva(PrimitiveType.Cube, "Fuera" + n, gafas, new Vector3(x + 0.057f * lado, 0f, z), new Vector3(g, 0.1f, g), color);
                Primitiva(PrimitiveType.Cube, "Bisagra" + n, gafas, new Vector3(0.195f * lado, 0.03f, z - 0.01f), new Vector3(0.06f, g, g), color);
                Primitiva(PrimitiveType.Cube, "Patilla" + n, gafas, new Vector3(0.222f * lado, 0.03f, 0.15f), new Vector3(g, g, 0.27f), color);
            }
            Primitiva(PrimitiveType.Cube, "Puente", gafas, new Vector3(0f, 0.02f, z), new Vector3(0.06f, g, g), color);
        }

        // Globo, etiqueta con el nombre y foco de cuando habla, medidos desde la cabeza.
        static void TerminarPersonaje(PersonajeAgente p, float alturaCabeza)
        {
            var go = p.gameObject;

            var punto = new GameObject("PuntoGlobo").transform;
            punto.SetParent(go.transform, false);
            punto.localPosition = new Vector3(0f, alturaCabeza + 0.7f, 0f);
            p.puntoGlobo = punto;

            var etiqueta = new GameObject("Etiqueta");
            etiqueta.transform.SetParent(go.transform, false);
            etiqueta.transform.localPosition = new Vector3(0f, alturaCabeza + 0.45f, 0f);
            var renderTexto = etiqueta.AddComponent<MeshRenderer>();
            var tm = etiqueta.AddComponent<TextMesh>();
            tm.text = "Agente";
            tm.font = Fuente();
            tm.fontSize = 48;
            tm.characterSize = 0.05f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            if (tm.font != null)
                renderTexto.sharedMaterial = tm.font.material;
            etiqueta.AddComponent<MirarCamara>();
            p.etiqueta = tm;

            var foco = new GameObject("Foco").AddComponent<Light>();
            foco.transform.SetParent(go.transform, false);
            foco.transform.localPosition = new Vector3(0f, alturaCabeza + 2.45f, 0.8f);
            foco.transform.localRotation = Quaternion.Euler(100f, 0f, 0f);
            foco.type = LightType.Spot;
            foco.spotAngle = 35f;
            foco.range = 8f;
            foco.intensity = 4f;
            foco.color = new Color(1f, 0.93f, 0.75f);
            foco.renderMode = LightRenderMode.ForcePixel;
            foco.enabled = false;
            p.foco = foco;
        }

        public static GameObject CrearGlobo()
        {
            var fondo = CrearImagen(null, "GloboDialogo", ColorGlobo);
            var rect = fondo.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(640, 240);

            var cola = CrearImagen(rect, "Cola", ColorGlobo);
            cola.rectTransform.anchorMin = cola.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            cola.rectTransform.anchoredPosition = Vector2.zero;
            cola.rectTransform.sizeDelta = new Vector2(30, 30);
            cola.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            var titulo = CrearTexto(rect, "Titulo", "", 22, TextAnchor.UpperLeft, Color.black, FontStyle.Bold);
            titulo.rectTransform.anchorMin = new Vector2(0, 1);
            titulo.rectTransform.anchorMax = new Vector2(1, 1);
            titulo.rectTransform.pivot = new Vector2(0.5f, 1);
            titulo.rectTransform.offsetMin = new Vector2(20, -48);
            titulo.rectTransform.offsetMax = new Vector2(-20, -12);

            var texto = CrearTexto(rect, "Texto", "", 22, TextAnchor.UpperLeft, new Color(0.12f, 0.12f, 0.15f));
            // Abajo deja espacio para la flecha de avanzar.
            Estirar(texto.rectTransform, 20, 20, 50, 34);
            texto.resizeTextForBestFit = true;
            texto.resizeTextMinSize = 14;
            texto.resizeTextMaxSize = 22;
            texto.supportRichText = true;

            var g = fondo.gameObject.AddComponent<GloboDialogo>();
            g.rect = rect;
            g.titulo = titulo;
            g.texto = texto;
            g.cola = cola.gameObject;

            // Flecha chica para avanzar: una línea y una punta, en Océano. Su opacidad la anima GloboDialogo.
            var flecha = new GameObject("Flecha", typeof(RectTransform), typeof(CanvasGroup));
            var rf = (RectTransform)flecha.transform;
            rf.SetParent(rect, false);
            rf.anchorMin = rf.anchorMax = new Vector2(1f, 0f);
            rf.pivot = new Vector2(0.5f, 0.5f);
            rf.sizeDelta = new Vector2(26, 14);
            rf.anchoredPosition = g.posicionFlecha;
            var grupo = flecha.GetComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.interactable = false;
            grupo.blocksRaycasts = false;
            Color colorFlecha = SharkMindColors.Ocean;
            var linea = CrearImagen(rf, "Linea", colorFlecha);
            linea.rectTransform.sizeDelta = new Vector2(20, 2.5f);
            linea.rectTransform.anchoredPosition = new Vector2(-2f, 0f);
            var puntaArriba = CrearImagen(rf, "PuntaArriba", colorFlecha);
            puntaArriba.rectTransform.sizeDelta = new Vector2(9, 2.5f);
            puntaArriba.rectTransform.anchoredPosition = new Vector2(5f, 2.8f);
            puntaArriba.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -40f);
            var puntaAbajo = CrearImagen(rf, "PuntaAbajo", colorFlecha);
            puntaAbajo.rectTransform.sizeDelta = new Vector2(9, 2.5f);
            puntaAbajo.rectTransform.anchoredPosition = new Vector2(5f, -2.8f);
            puntaAbajo.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 40f);
            flecha.SetActive(false);
            g.flecha = rf;
            g.flechaGrupo = grupo;
            return fondo.gameObject;
        }

        public static GameObject CrearBarra()
        {
            var go = new GameObject("BarraCriterio", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(432, 56);
            var b = go.AddComponent<BarraCriterio>();

            b.etiqueta = CrearTexto(rect, "Etiqueta", "Criterio", 24, TextAnchor.UpperLeft, Color.white);
            Colocar(b.etiqueta.rectTransform, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(280, 30));

            b.textoValor = CrearTexto(rect, "Valor", "– / 10", 24, TextAnchor.UpperRight, Color.white, FontStyle.Bold);
            Colocar(b.textoValor.rectTransform, new Vector2(1, 1), new Vector2(1, 1), Vector2.zero, new Vector2(150, 30));

            var fondoBarra = CrearImagen(rect, "Fondo", new Color(1f, 1f, 1f, 0.15f));
            fondoBarra.rectTransform.anchorMin = new Vector2(0, 1);
            fondoBarra.rectTransform.anchorMax = new Vector2(1, 1);
            fondoBarra.rectTransform.pivot = new Vector2(0.5f, 1);
            fondoBarra.rectTransform.offsetMin = new Vector2(0, -52);
            fondoBarra.rectTransform.offsetMax = new Vector2(0, -34);

            var relleno = CrearImagen(fondoBarra.rectTransform, "Relleno", BarraCriterio.ColorPara(10));
            relleno.rectTransform.anchorMin = Vector2.zero;
            relleno.rectTransform.anchorMax = new Vector2(0, 1);
            relleno.rectTransform.offsetMin = Vector2.zero;
            relleno.rectTransform.offsetMax = Vector2.zero;
            b.relleno = relleno.rectTransform;
            b.imagenRelleno = relleno;

            // La evidencia ya no va debajo de la barra: se ve en la lista de comentarios del panel.
            return go;
        }

        // ---------- Interfaz ----------

        static Transform CrearCanvas(Transform raiz)
        {
            var go = new GameObject("Interfaz", typeof(RectTransform));
            go.transform.SetParent(raiz, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var escala = go.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920, 1080);
            escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escala.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        static PanelCalificaciones CrearPanel(Transform canvas, IFabrica fabrica)
        {
            const float arriba = 64f;    // espacio para el botón de comentarios
            const float paso = 72f;      // de una barra a la siguiente
            var fondo = CrearImagen(canvas, "PanelCalificaciones", ColorPanel);
            float yTextos = -(arriba + paso * PanelCalificaciones.Criterios.Length + 4f);
            float alto = -yTextos + 32f * 3f + 34f + 18f;
            Colocar(fondo.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(30, 60), new Vector2(480, alto), new Vector2(0, 0));
            var panel = fondo.gameObject.AddComponent<PanelCalificaciones>();

            panel.barras = new BarraCriterio[PanelCalificaciones.Criterios.Length];
            for (int i = 0; i < panel.barras.Length; i++)
            {
                var barra = fabrica.Barra().GetComponent<BarraCriterio>();
                barra.transform.SetParent(fondo.rectTransform, false);
                Colocar((RectTransform)barra.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -arriba - i * paso), new Vector2(432, 56));
                barra.Configurar(PanelCalificaciones.Criterios[i], PanelCalificaciones.Nombres[i]);
                panel.barras[i] = barra;
            }

            panel.textoPromedio = CrearTexto(fondo.rectTransform, "Promedio", "Promedio: –", 22, TextAnchor.UpperLeft, Color.white, FontStyle.Bold);
            Colocar(panel.textoPromedio.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, yTextos), new Vector2(432, 30));
            panel.textoTurno = CrearTexto(fondo.rectTransform, "Turno", "Turno: –", 22, TextAnchor.UpperLeft, Color.white);
            Colocar(panel.textoTurno.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, yTextos - 32), new Vector2(432, 30));
            panel.textoOferta = CrearTexto(fondo.rectTransform, "Oferta", "Oferta: –", 22, TextAnchor.UpperLeft, Color.white);
            Colocar(panel.textoOferta.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, yTextos - 64), new Vector2(432, 30));
            panel.textoDecision = CrearTexto(fondo.rectTransform, "Decision", "Decisión: –", 24, TextAnchor.UpperLeft, Color.white, FontStyle.Bold);
            Colocar(panel.textoDecision.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, yTextos - 96), new Vector2(432, 32));

            panel.comentarios = CrearComentarios(fondo.rectTransform);
            return panel;
        }

        // Botón arriba a la derecha del panel que cambia entre estadísticas y comentarios (toggle).
        // Con estadísticas muestra una burbuja de chat; con comentarios, una flecha para regresar.
        static ListaComentarios CrearComentarios(RectTransform panel)
        {
            // Lo que se esconde al ver los comentarios: todo lo que ya tiene el panel.
            var estadisticas = new System.Collections.Generic.List<GameObject>();
            foreach (Transform hijo in panel)
                estadisticas.Add(hijo.gameObject);

            var lista = panel.gameObject.AddComponent<ListaComentarios>();

            // Botón
            var boton = new GameObject("BotonComentarios", typeof(RectTransform)).GetComponent<RectTransform>();
            boton.SetParent(panel, false);
            Colocar(boton, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16, -12), new Vector2(48, 42), new Vector2(1, 1));
            var area = CrearImagen(boton, "Area", new Color(1f, 1f, 1f, 0f));
            Estirar(area.rectTransform, 0, 0, 0, 0);

            // Icono de burbuja de chat (vista de estadísticas).
            var chat = new GameObject("IconoChat", typeof(RectTransform)).GetComponent<RectTransform>();
            chat.SetParent(boton, false);
            Estirar(chat, 0, 0, 0, 0);
            var cuerpo = CrearImagen(chat, "Burbuja", Color.white);
            Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 3), new Vector2(38, 26), new Vector2(0.5f, 0.5f));
            var cola = CrearImagen(chat, "Cola", Color.white);
            Colocar(cola.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10, -10), new Vector2(10, 10), new Vector2(0.5f, 0.5f));
            cola.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            for (int i = 0; i < 3; i++)
            {
                var linea = CrearImagen(cuerpo.rectTransform, "Linea" + (i + 1), SharkMindColors.Ocean);
                Colocar(linea.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 7 - i * 7), new Vector2(24, 3), new Vector2(0.5f, 0.5f));
            }

            // Icono de flecha para regresar a estadísticas (vista de comentarios, sobre fondo claro).
            var regresar = new GameObject("IconoRegresar", typeof(RectTransform)).GetComponent<RectTransform>();
            regresar.SetParent(boton, false);
            Estirar(regresar, 0, 0, 0, 0);
            Color colorFlecha = SharkMindColors.Ocean;
            var palo = CrearImagen(regresar, "Linea", colorFlecha);
            Colocar(palo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(3, 0), new Vector2(26, 4), new Vector2(0.5f, 0.5f));
            var puntaArriba = CrearImagen(regresar, "PuntaArriba", colorFlecha);
            Colocar(puntaArriba.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-6, 4.5f), new Vector2(14, 4), new Vector2(0.5f, 0.5f));
            puntaArriba.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 40f);
            var puntaAbajo = CrearImagen(regresar, "PuntaAbajo", colorFlecha);
            Colocar(puntaAbajo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-6, -4.5f), new Vector2(14, 4), new Vector2(0.5f, 0.5f));
            puntaAbajo.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -40f);
            regresar.gameObject.SetActive(false);

            // Aviso de comentarios nuevos sin leer.
            var aviso = CrearImagen(boton, "Aviso", ColorAcento);
            Colocar(aviso.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(6, 6), new Vector2(20, 20), new Vector2(1, 1));
            var textoAviso = CrearTexto(aviso.rectTransform, "Numero", "", 13, TextAnchor.MiddleCenter, SharkMindColors.Ocean, FontStyle.Bold);
            Estirar(textoAviso.rectTransform, 0, 0, 0, 0);

            // Lista de comentarios: ocupa el mismo panel, debajo del botón.
            var caja = new GameObject("ListaComentarios", typeof(RectTransform), typeof(CanvasGroup));
            var rc = (RectTransform)caja.transform;
            rc.SetParent(panel, false);
            Estirar(rc, 0, 0, 64, 12);
            var grupo = caja.GetComponent<CanvasGroup>();
            grupo.interactable = false;
            grupo.blocksRaycasts = false;

            lista.boton = boton;
            lista.iconoChat = chat.gameObject;
            lista.iconoRegresar = regresar.gameObject;
            lista.estadisticas = estadisticas.ToArray();
            lista.fondoPanel = panel.GetComponent<Image>();
            lista.colorFondoEstadisticas = ColorPanel;
            lista.colorFondoComentarios = SharkMindColors.Espuma;
            lista.lista = rc;
            lista.grupoLista = grupo;
            lista.aviso = aviso.gameObject;
            lista.textoAviso = textoAviso;
            caja.SetActive(false);
            aviso.gameObject.SetActive(false);
            return lista;
        }

        // ---------- Ayudas ----------

        static GameObject Primitiva(PrimitiveType tipo, string nombre, Transform padre, Vector3 posicion, Vector3 escala, Color color)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nombre;
            go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            go.transform.localScale = escala;
            go.AddComponent<ColorSolido>().Cambiar(color);
            return go;
        }

        static void Brillo(GameObject go, float brillo)
        {
            var c = go.GetComponent<ColorSolido>();
            c.brillo = brillo;
            c.Aplicar();
        }

        public static Text CrearTexto(Transform padre, string nombre, string contenido, int tamano, TextAnchor alineacion, Color color, FontStyle estilo = FontStyle.Normal)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            if (padre != null)
                go.transform.SetParent(padre, false);
            var t = go.AddComponent<Text>();
            t.font = Fuente();
            t.text = contenido;
            t.fontSize = tamano;
            t.fontStyle = estilo;
            t.alignment = alineacion;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }

        static Image CrearImagen(Transform padre, string nombre, Color color)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            if (padre != null)
                go.transform.SetParent(padre, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        // Ancla el rect en un punto del padre. El pivote es la misma esquina que el ancla, salvo que se indique otro.
        static void Colocar(RectTransform r, Vector2 anclaMin, Vector2 anclaMax, Vector2 posicion, Vector2 tamano, Vector2? pivote = null)
        {
            r.anchorMin = anclaMin;
            r.anchorMax = anclaMax;
            r.pivot = pivote ?? anclaMin;
            r.anchoredPosition = posicion;
            r.sizeDelta = tamano;
        }

        static void Estirar(RectTransform r, float izquierda, float derecha, float arriba, float abajo)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(izquierda, abajo);
            r.offsetMax = new Vector2(-derecha, -arriba);
        }
    }
}
