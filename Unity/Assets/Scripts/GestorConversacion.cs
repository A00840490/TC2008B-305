using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Codea.SharkTank
{
    // Detecta los mensajes nuevos, controla quién habla y reparte los datos:
    // el último mensaje va al globo de diálogo y las calificaciones al panel.
    public class GestorConversacion : MonoBehaviour
    {
        [Header("Escena")]
        [Tooltip("Los tres jueces. Por ahora habla uno por turno.")]
        public PersonajeAgente[] jueces;
        public PersonajeAgente emprendedor;
        public GloboDialogo globo;
        public PanelCalificaciones panel;
        public CamaraDirector director;

        [Header("Interfaz")]
        public Text textoEstado;
        public GameObject bannerResultado;
        public Text textoResultado;

        [Tooltip("La conversación solo avanza cuando se da clic (o Espacio, Enter o flecha derecha). Sin esto avanza sola.")]
        public bool avanceManual = true;

        [Tooltip("Solo sin avance manual: segundos extra que se queda cada mensaje después de escribirse, cuando llegan varios juntos.")]
        public float pausaEntreMensajes = 1f;

        readonly Queue<Mensaje> pendientes = new Queue<Mensaje>();
        readonly HashSet<string> vistos = new HashSet<string>();
        SesionDatos ultima;
        string sesionActual;
        float siguienteMensaje;
        bool esperandoClic;
        PersonajeAgente ultimoJuez;

        // Ya se mostró todo lo que llegó y, con avance manual, ya se dio clic en el último mensaje.
        public bool ListoParaMas => pendientes.Count == 0 && (avanceManual ? !esperandoClic : Time.time >= siguienteMensaje);

        void Start()
        {
            // ClienteAPI puede mandar datos antes de este Start; en ese caso no hay que borrarlos.
            if (sesionActual == null)
                Reiniciar();
        }

        // La llama ClienteAPI cada vez que recibe datos de la sesión.
        public void Actualizar(SesionDatos datos)
        {
            if (datos == null)
                return;

            if (datos.sesion_id != sesionActual)
            {
                Reiniciar();
                sesionActual = datos.sesion_id;
            }

            if (datos.mensajes != null)
            {
                foreach (var m in datos.mensajes)
                {
                    if (m == null)
                        continue;
                    string clave = string.IsNullOrEmpty(m.id) ? m.emisor + "|" + m.turno + "|" + m.texto : m.id;
                    if (vistos.Add(clave))
                        pendientes.Enqueue(m);
                }
            }

            ultima = datos;
            if (panel != null)
                panel.Mostrar(datos);
        }

        string ultimoEstado;

        // Si la escena tiene texto de estado lo muestra ahí; si no, lo manda a la consola (solo cuando cambia).
        public void MostrarEstado(string texto)
        {
            if (textoEstado != null)
                textoEstado.text = texto;
            else if (texto != ultimoEstado)
                Debug.Log("[SharkMind] " + texto);
            ultimoEstado = texto;
        }

        void Update()
        {
            if (avanceManual)
            {
                if (esperandoClic)
                {
                    bool escrito = globo == null || globo.TerminoDeEscribir;
                    if (globo != null) globo.MostrarFlecha(escrito);
                    if (PidioAvanzar())
                    {
                        // Si todavía se está escribiendo, el primer clic lo termina de escribir; el siguiente avanza.
                        if (!escrito)
                        {
                            globo.Completar();
                        }
                        else
                        {
                            esperandoClic = false;
                            if (globo != null) globo.MostrarFlecha(false);
                        }
                    }
                }
                if (!esperandoClic && pendientes.Count > 0)
                {
                    MostrarMensaje(pendientes.Dequeue());
                    esperandoClic = true;
                }
            }
            else if (pendientes.Count > 0 && Time.time >= siguienteMensaje)
            {
                var m = pendientes.Dequeue();
                float duracion = MostrarMensaje(m);
                siguienteMensaje = Time.time + duracion + pausaEntreMensajes;
            }

            bool mostrarResultado = ultima != null && ultima.Terminada && ListoParaMas;
            if (bannerResultado != null && bannerResultado.activeSelf != mostrarResultado)
            {
                if (mostrarResultado)
                {
                    if (textoResultado != null)
                        textoResultado.text = TextoResultado(ultima);
                    // El resultado se ve con los dos agentes en cuadro.
                    if (director != null)
                        director.General();
                    if (globo != null)
                        globo.fijoAbajo = false;
                }
                bannerResultado.SetActive(mostrarResultado);
            }
        }

        float MostrarMensaje(Mensaje m)
        {
            bool esJuez = m.emisor == "juez";
            PersonajeAgente habla, escucha;
            if (esJuez)
            {
                habla = JuezPara(m);
                if (habla != null) ultimoJuez = habla;
                escucha = emprendedor;
                // El emprendedor voltea a ver al juez que le habla.
                if (emprendedor != null && habla != null)
                    emprendedor.MirarHacia(habla.transform.position);
            }
            else
            {
                habla = emprendedor;
                escucha = ultimoJuez != null ? ultimoJuez : JuezCentral();
            }

            CallarATodosMenos(habla);
            if (habla != null) habla.Hablar(true);

            var toma = director != null ? director.Elegir(habla, m.tipo, escucha) : TipoToma.General;
            if (globo != null)
                globo.fijoAbajo = toma == TipoToma.Cercana;

            // Solo el nombre de quien habla, sin el tipo de mensaje.
            string titulo = habla != null ? habla.nombre : m.emisor;
            return globo != null ? globo.Mostrar(habla, titulo, m.texto) : 0f;
        }

        void Reiniciar()
        {
            pendientes.Clear();
            vistos.Clear();
            ultima = null;
            siguienteMensaje = 0f;
            esperandoClic = false;
            if (globo != null) globo.Ocultar();
            if (panel != null) panel.Limpiar();
            CallarATodosMenos(null);
            ultimoJuez = null;
            var centro = JuezCentral();
            if (emprendedor != null && centro != null)
                emprendedor.MirarHacia(centro.transform.position);
            if (bannerResultado != null) bannerResultado.SetActive(false);
            if (director != null) director.General();
            if (globo != null) globo.fijoAbajo = false;
        }

        // Qué juez dice el mensaje: el que indique el campo "juez" (1, 2 o 3) o, si no viene, uno por turno rotando.
        PersonajeAgente JuezPara(Mensaje m)
        {
            if (jueces == null || jueces.Length == 0)
                return null;
            int i = m.juez > 0 ? m.juez - 1 : Mathf.Max(0, m.turno - 1);
            return jueces[i % jueces.Length];
        }

        PersonajeAgente JuezCentral()
        {
            return jueces != null && jueces.Length > 0 ? jueces[jueces.Length / 2] : null;
        }

        void CallarATodosMenos(PersonajeAgente quien)
        {
            if (jueces != null)
                foreach (var j in jueces)
                    if (j != null && j != quien)
                        j.Hablar(false);
            if (emprendedor != null && emprendedor != quien)
                emprendedor.Hablar(false);
        }

        // Clic en cualquier parte (que no sea un botón de la interfaz), Espacio, Enter o flecha derecha.
        static bool PidioAvanzar()
        {
            return Entrada.TeclaAvanzar() || Entrada.ClicLibre();
        }

        public static string NombreTipo(string tipo)
        {
            switch (tipo)
            {
                case "pitch": return "Pitch";
                case "pregunta": return "Pregunta";
                case "respuesta": return "Respuesta";
                case "oferta": return "Oferta";
                case "aceptar": return "Acepta";
                case "rechazar": return "Rechaza";
                case "retroalimentacion": return "Retroalimentación";
                default: return tipo;
            }
        }

        public static string FormatoOferta(Oferta o)
        {
            if (o == null || !o.Existe)
                return "–";
            return "$" + o.monto.ToString("N0", CultureInfo.InvariantCulture) + " por " + o.porcentaje + " %";
        }

        static string TextoResultado(SesionDatos d)
        {
            if (d.decision == "invertir" && d.oferta_actual != null && d.oferta_actual.Existe)
                return "Trato cerrado: " + FormatoOferta(d.oferta_actual);
            if (d.decision == "no_invertir")
                return "El juez no invierte";
            return "Sesión terminada sin acuerdo";
        }
    }
}
