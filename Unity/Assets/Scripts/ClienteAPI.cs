using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Codea.SharkTank
{
    public enum ModoCliente
    {
        Simulado,  // reproduce la sesión de ejemplo, sin servidor
        Servidor,  // consulta al Agente Juez de la versión anterior (Agentes/, GET /sesion/actual)
        Backend    // consulta el historial del backend con CrewAI (GET /api/conversation-history)
    }

    // Consulta la sesión al backend cada 2 segundos, convierte el JSON
    // y le pasa los datos a GestorConversacion. Solo lee: nunca le manda datos a los agentes.
    public class ClienteAPI : MonoBehaviour
    {
        [Header("Origen de los datos")]
        public ModoCliente modo = ModoCliente.Simulado;

        [Tooltip("Dirección del backend. Unity corre en la misma computadora, así que es localhost.")]
        public string urlBase = "http://localhost:8000";

        [Tooltip("Ruta de consulta en modo Servidor (versión anterior del juez).")]
        public string ruta = "/sesion/actual";

        [Tooltip("Ruta de consulta en modo Backend.")]
        public string rutaBackend = "/api/conversation-history";

        [Tooltip("Segundos entre cada consulta.")]
        public float intervalo = 2f;

        [Tooltip("Segundos máximos de espera por respuesta.")]
        public int tiempoMaximo = 5;

        [Header("Modo simulado")]
        public TextAsset sesionEjemplo;

        [Tooltip("Segundos entre cada mensaje de la sesión de ejemplo (solo si GestorConversacion no tiene avance manual).")]
        public float segundosPorMensaje = 3f;

        [Tooltip("Vuelve a empezar la sesión de ejemplo al terminar.")]
        public bool repetir = true;

        [Header("Referencias")]
        public GestorConversacion gestor;

        void Start()
        {
            if (gestor == null)
                gestor = GetComponent<GestorConversacion>();

            if (modo == ModoCliente.Servidor || modo == ModoCliente.Backend)
                StartCoroutine(ConsultarServidor());
            else
                StartCoroutine(Simular());
        }

        IEnumerator ConsultarServidor()
        {
            var espera = new WaitForSeconds(intervalo);
            bool esBackend = modo == ModoCliente.Backend;
            string url = urlBase.TrimEnd('/') + (esBackend ? rutaBackend : ruta);

            while (true)
            {
                using (var solicitud = UnityWebRequest.Get(url))
                {
                    solicitud.timeout = tiempoMaximo;
                    yield return solicitud.SendWebRequest();

                    if (solicitud.result == UnityWebRequest.Result.Success)
                    {
                        string texto = solicitud.downloadHandler.text;
                        var datos = esBackend ? ConvertirBackend(texto) : Convertir(texto);
                        if (datos != null && esBackend && datos.mensajes.Length == 0)
                        {
                            // El backend responde aunque no haya sesión: el historial viene vacío.
                            gestor.MostrarEstado("Conectado a " + urlBase + ". Esperando a que empiece una sesión…");
                        }
                        else if (datos != null)
                        {
                            gestor.MostrarEstado("Conectado a " + url);
                            gestor.Actualizar(datos);
                        }
                        else
                        {
                            gestor.MostrarEstado("El servidor respondió con un JSON que no se pudo leer.");
                        }
                    }
                    else if (solicitud.responseCode == 404)
                    {
                        gestor.MostrarEstado("Conectado a " + urlBase + ". Esperando a que empiece una sesión…");
                    }
                    else
                    {
                        gestor.MostrarEstado("Sin conexión con " + url + " (" + solicitud.error + "). Reintentando…");
                    }
                }

                yield return espera;
            }
        }

        public static SesionDatos Convertir(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            try
            {
                return JsonUtility.FromJson<SesionDatos>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("No se pudo leer la sesión: " + e.Message);
                return null;
            }
        }

        // ---------------------------------------------------------------- modo Backend
        // Pasa el historial del backend (lista de mensajes con role, content y judge_name o
        // sender_name) al formato que ya usa la escena. El backend solo regresa texto, así que
        // el tipo de cada mensaje y las calificaciones se sacan del texto cuando vienen en él.

        static readonly string[] Criterios = { "innovacion", "sostenibilidad", "mercado", "rentabilidad" };

        public static SesionDatos ConvertirBackend(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            HistorialBackend h;
            try
            {
                h = JsonUtility.FromJson<HistorialBackend>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("No se pudo leer el historial del backend: " + e.Message);
                return null;
            }
            if (h == null)
                return null;

            var historial = h.conversation_history ?? new MensajeBackend[0];
            var datos = new SesionDatos
            {
                estado = "en_curso",
                turno = 0,
                decision = "pendiente",
                oferta_actual = new Oferta(),
                calificaciones = new Calificacion[0],
                mensajes = new Mensaje[0]
            };

            // Identificador de la sesión: si cambia la propuesta o el pitch, es una sesión nueva.
            string nombre = h.business_idea != null ? h.business_idea.name : null;
            string primero = historial.Length > 0 && historial[0] != null ? historial[0].content ?? "" : "";
            datos.sesion_id = (nombre ?? "sesion") + "|" + primero.Length + "|" + primero.Substring(0, Math.Min(40, primero.Length));

            var mensajes = new List<Mensaje>();
            var lugares = new Dictionary<string, int>();  // juez de la escena (1, 2 o 3) de cada nombre
            var ultimas = new Dictionary<string, int[]>();  // últimas calificaciones de cada juez
            int turno = 0;

            for (int i = 0; i < historial.Length; i++)
            {
                var mb = historial[i];
                if (mb == null || string.IsNullOrEmpty(mb.content))
                    continue;

                bool esJuez = string.Equals(mb.role, "Judge", StringComparison.OrdinalIgnoreCase);
                var m = new Mensaje { id = "m" + (i + 1) };

                if (esJuez)
                {
                    string juez = string.IsNullOrEmpty(mb.judge_name) ? "Juez" : mb.judge_name.Trim();
                    m.emisor = "juez";
                    m.juez = NumeroDeJuez(juez, lugares);
                    m.texto = QuitarJson(QuitarPrefijo(mb.content, juez));
                    m.tipo = TipoDeJuez(m.texto);

                    var cal = BuscarCalificaciones(mb.content);
                    if (cal != null)
                        ultimas[juez] = cal;
                }
                else
                {
                    turno++;
                    m.emisor = "emprendedor";
                    m.texto = mb.content.Trim();
                    m.tipo = turno == 1 ? "pitch" : "respuesta";
                }
                m.turno = Math.Max(1, turno);
                mensajes.Add(m);
            }

            datos.turno = Math.Max(turno, mensajes.Count > 0 ? 1 : 0);
            datos.mensajes = mensajes.ToArray();
            datos.calificaciones = Promediar(ultimas);
            return datos;
        }

        // Juez 1, 2 o 3 de la escena (Finanzas, Innovación, Mercado). Si el nombre no dice el área,
        // toma el primer lugar libre, en el orden en que hablan por primera vez.
        static int NumeroDeJuez(string nombre, Dictionary<string, int> lugares)
        {
            if (lugares.TryGetValue(nombre, out int ya))
                return ya;
            string n = SinAcentos(nombre).ToLowerInvariant();
            int lugar = 0;
            if (n.Contains("finanz") || n.Contains("financ")) lugar = 1;
            else if (n.Contains("innova")) lugar = 2;
            else if (n.Contains("mercad") || n.Contains("market")) lugar = 3;
            else
            {
                for (int i = 1; i <= 3 && lugar == 0; i++)
                    if (!lugares.ContainsValue(i)) lugar = i;
                if (lugar == 0) lugar = lugares.Count % 3 + 1;
            }
            lugares[nombre] = lugar;
            return lugar;
        }

        // El backend le pide a cada juez empezar con "Nombre: "; en el globo ya sale el nombre.
        static string QuitarPrefijo(string texto, string juez)
        {
            string t = texto.Trim();
            var m = Regex.Match(t, @"^\**\s*" + Regex.Escape(juez) + @"\s*\**\s*:\s*\**\s*", RegexOptions.IgnoreCase);
            return m.Success ? t.Substring(m.Length).Trim() : t;
        }

        // Las calificaciones en JSON sirven para el panel; en el globo solo va el texto.
        static string QuitarJson(string texto)
        {
            string t = Regex.Replace(texto, @"```(json)?[\s\S]*?```", "", RegexOptions.IgnoreCase);
            t = Regex.Replace(t, @"\{[^{}]*\}", m => SinAcentos(m.Value).ToLowerInvariant().Contains("innovacion") ? "" : m.Value);
            return t.Trim();
        }

        static string TipoDeJuez(string texto)
        {
            string t = SinAcentos(texto).ToLowerInvariant();
            if (t.Contains("i'm out") || t.Contains("im out") || t.Contains("estoy fuera") || t.Contains("no invierto") || t.Contains("me salgo"))
                return "rechazar";
            if (t.Contains("%"))
                return "oferta";
            if (t.Contains("?"))
                return "pregunta";
            return "respuesta";
        }

        // Busca en el texto calificaciones como "innovacion": 7 o Innovación: 7/10.
        static int[] BuscarCalificaciones(string texto)
        {
            string t = SinAcentos(texto).ToLowerInvariant();
            var valores = new int[Criterios.Length];
            int encontrados = 0;
            for (int c = 0; c < Criterios.Length; c++)
            {
                valores[c] = -1;
                var m = Regex.Match(t, "\"?" + Criterios[c] + "\"?\\s*[:=]\\s*(\\d{1,2})");
                if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v <= 10)
                {
                    valores[c] = v;
                    encontrados++;
                }
            }
            return encontrados > 0 ? valores : null;
        }

        // Una barra por criterio con el promedio de las últimas calificaciones de cada juez.
        static Calificacion[] Promediar(Dictionary<string, int[]> ultimas)
        {
            if (ultimas.Count == 0)
                return new Calificacion[0];
            var lista = new List<Calificacion>();
            for (int c = 0; c < Criterios.Length; c++)
            {
                int suma = 0, n = 0;
                foreach (var v in ultimas.Values)
                    if (v[c] >= 0) { suma += v[c]; n++; }
                if (n > 0)
                    lista.Add(new Calificacion
                    {
                        criterio = Criterios[c],
                        valor = Mathf.RoundToInt((float)suma / n),
                        evidencia = n == 1 ? "Calificación de 1 juez" : "Promedio de " + n + " jueces"
                    });
            }
            return lista.ToArray();
        }

        static string SinAcentos(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            var sb = new StringBuilder(s.Length);
            foreach (char ch in s.Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        IEnumerator Simular()
        {
            if (sesionEjemplo == null)
                sesionEjemplo = Resources.Load<TextAsset>("sesion_ejemplo");
            if (sesionEjemplo == null)
            {
                gestor.MostrarEstado("No se encontró Resources/sesion_ejemplo.json.");
                yield break;
            }

            var simulada = JsonUtility.FromJson<SesionSimulada>(sesionEjemplo.text);
            int vuelta = 0;

            while (true)
            {
                vuelta++;
                gestor.MostrarEstado("Modo simulado: sesión de ejemplo, sin servidor. Cambia el modo en ClienteAPI para conectarte al juez.");

                var datos = new SesionDatos
                {
                    sesion_id = simulada.sesion_id + "-" + vuelta,
                    estado = "en_curso",
                    turno = 0,
                    turno_maximo = simulada.turno_maximo,
                    decision = "pendiente",
                    oferta_actual = new Oferta(),
                    calificaciones = new Calificacion[0],
                    mensajes = new Mensaje[0]
                };
                var mensajes = new List<Mensaje>();
                gestor.Actualizar(datos);

                for (int i = 0; i < simulada.pasos.Length; i++)
                {
                    if (i == 0)
                        yield return new WaitForSeconds(1f);
                    else if (gestor.avanceManual)
                    {
                        // No manda el siguiente paso hasta que se da clic para avanzar.
                        yield return new WaitUntil(() => gestor.ListoParaMas);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                        yield return new WaitForSeconds(segundosPorMensaje);

                    var paso = simulada.pasos[i];
                    if (paso.mensaje != null && !string.IsNullOrEmpty(paso.mensaje.texto))
                        mensajes.Add(paso.mensaje);

                    datos.mensajes = mensajes.ToArray();
                    if (paso.turno > 0) datos.turno = paso.turno;
                    if (!string.IsNullOrEmpty(paso.estado)) datos.estado = paso.estado;
                    if (!string.IsNullOrEmpty(paso.decision)) datos.decision = paso.decision;
                    if (paso.oferta_actual != null && paso.oferta_actual.Existe) datos.oferta_actual = paso.oferta_actual;
                    if (paso.calificaciones != null && paso.calificaciones.Length > 0) datos.calificaciones = paso.calificaciones;

                    gestor.Actualizar(datos);
                }

                if (!repetir)
                    yield break;

                // Deja ver el resultado antes de volver a empezar.
                if (gestor.avanceManual)
                    yield return new WaitUntil(() => gestor.ListoParaMas);
                yield return new WaitForSeconds(10f);
            }
        }
    }
}
