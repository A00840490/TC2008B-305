using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Codea.SharkTank
{
    // Toggle del panel de calificaciones: el botón con burbuja de chat cambia el panel a la vista de comentarios
    // (la evidencia que ha ido tomando el juez, del más reciente arriba al más viejo abajo, sobre fondo blanquecino).
    // En esa vista el botón se vuelve una flecha para regresar a las estadísticas.
    // Corre antes que GestorConversacion para que el clic en el botón no avance también la conversación.
    [DefaultExecutionOrder(-50)]
    public class ListaComentarios : MonoBehaviour
    {
        [Header("Partes")]
        public RectTransform boton;
        public GameObject iconoChat;
        public GameObject iconoRegresar;
        [Tooltip("Lo que se esconde al ver los comentarios: barras, promedio, turno, oferta y decisión.")]
        public GameObject[] estadisticas;
        public Image fondoPanel;
        public Color colorFondoEstadisticas = new Color(11 / 255f, 31 / 255f, 58 / 255f, 0.92f);
        public Color colorFondoComentarios = new Color(244 / 255f, 247 / 255f, 250 / 255f, 0.97f);
        public RectTransform lista;
        public CanvasGroup grupoLista;
        public GameObject aviso;
        public Text textoAviso;

        [Header("Lista")]
        public float altoComentario = 78f;
        public float separacion = 8f;
        public float margen = 16f;
        [Tooltip("Segundos que tarda en aparecer la lista.")]
        public float duracionApertura = 0.2f;
        [Tooltip("Pixeles que baja la lista mientras aparece.")]
        public float caida = 10f;

        // La lista va sobre fondo blanquecino (Espuma), así que el texto es oscuro.
        public Color colorFondoComentario = new Color(11 / 255f, 31 / 255f, 58 / 255f, 0.06f);
        public Color colorTitulo = SharkMindColors.Ocean;
        public Color colorCita = new Color(0.25f, 0.3f, 0.36f);
        public Color colorSuave = new Color(0.45f, 0.5f, 0.56f);

        class Comentario
        {
            public string nombre;
            public int valor;
            public string evidencia;
        }

        // El más reciente va en la posición 0.
        readonly List<Comentario> comentarios = new List<Comentario>();
        readonly List<GameObject> filas = new List<GameObject>();
        bool abierta;
        int sinLeer;
        float inicioApertura;
        Vector2 posicionLista;
        bool posicionGuardada;

        void Awake()
        {
            GuardarPosicion();
            Cerrar();
            ActualizarAviso();
        }

        void GuardarPosicion()
        {
            if (posicionGuardada || lista == null)
                return;
            posicionLista = lista.anchoredPosition;
            posicionGuardada = true;
        }

        public void Agregar(string nombre, int valor, string evidencia)
        {
            comentarios.Insert(0, new Comentario { nombre = nombre, valor = valor, evidencia = evidencia });
            if (abierta)
                Reconstruir();
            else
                sinLeer++;
            ActualizarAviso();
        }

        public void Limpiar()
        {
            comentarios.Clear();
            sinLeer = 0;
            Reconstruir();
            ActualizarAviso();
        }

        public void Alternar()
        {
            if (abierta) Cerrar(); else Abrir();
        }

        public void Abrir()
        {
            GuardarPosicion();
            abierta = true;
            sinLeer = 0;
            inicioApertura = Time.time;
            if (lista != null) lista.gameObject.SetActive(true);
            CambiarVista();
            Reconstruir();
            Animar();
            ActualizarAviso();
        }

        public void Cerrar()
        {
            abierta = false;
            if (lista != null) lista.gameObject.SetActive(false);
            CambiarVista();
            ActualizarAviso();
        }

        // Muestra las estadísticas o los comentarios en el mismo panel, y cambia el icono del botón.
        void CambiarVista()
        {
            if (estadisticas != null)
                foreach (var e in estadisticas)
                    if (e != null)
                        e.SetActive(!abierta);
            if (fondoPanel != null)
                fondoPanel.color = abierta ? colorFondoComentarios : colorFondoEstadisticas;
            if (iconoChat != null) iconoChat.SetActive(!abierta);
            if (iconoRegresar != null) iconoRegresar.SetActive(abierta);
        }

        void Update()
        {
            if (Entrada.Clic(out var pos))
            {
                if (Entrada.Dentro(boton, pos))
                {
                    Alternar();
                    Entrada.Consumir();
                }
            }
            Animar();
        }

        // Baja un poco y aparece con fade.
        void Animar()
        {
            if (!abierta || lista == null)
                return;
            float t = Mathf.Clamp01((Time.time - inicioApertura) / Mathf.Max(0.01f, duracionApertura));
            float e = 1f - (1f - t) * (1f - t);
            lista.anchoredPosition = posicionLista + new Vector2(0f, caida * (1f - e));
            if (grupoLista != null)
                grupoLista.alpha = e;
        }

        void ActualizarAviso()
        {
            if (aviso != null)
                aviso.SetActive(sinLeer > 0 && !abierta);
            if (textoAviso != null)
                textoAviso.text = sinLeer > 9 ? "9+" : sinLeer.ToString();
        }

        void Reconstruir()
        {
            foreach (var f in filas)
                if (f != null)
                    Destroy(f);
            filas.Clear();
            if (lista == null)
                return;

            float y = -4f;
            float anchoLista = lista.rect.width;
            float anchoTexto = anchoLista - margen * 2f - 24f;
            // Cuántos caben en el panel, dejando lugar para el aviso de "más antiguos".
            int caben = Mathf.Max(1, Mathf.FloorToInt((lista.rect.height - margen - 28f + separacion) / (altoComentario + separacion)));

            if (comentarios.Count == 0)
            {
                var vacio = ConstructorEscena.CrearTexto(lista, "SinComentarios", "El juez todavía no ha tomado comentarios.", 17,
                    TextAnchor.MiddleLeft, colorSuave, FontStyle.Italic);
                Colocar(vacio.rectTransform, new Vector2(margen, y), new Vector2(anchoLista - margen * 2f, 40f));
                filas.Add(vacio.gameObject);
                y -= 40f;
            }

            int n = Mathf.Min(comentarios.Count, caben);
            for (int i = 0; i < n; i++)
            {
                var c = comentarios[i];
                var fila = new GameObject("Comentario" + (i + 1), typeof(RectTransform));
                var rf = (RectTransform)fila.transform;
                rf.SetParent(lista, false);
                Colocar(rf, new Vector2(margen, y), new Vector2(anchoLista - margen * 2f, altoComentario));
                filas.Add(fila);

                var fondo = fila.AddComponent<Image>();
                fondo.color = colorFondoComentario;
                fondo.raycastTarget = false;

                // Franja del color de la calificación, como la barra.
                var franja = new GameObject("Franja", typeof(RectTransform)).AddComponent<Image>();
                franja.rectTransform.SetParent(rf, false);
                franja.color = BarraCriterio.ColorPara(c.valor);
                franja.raycastTarget = false;
                Colocar(franja.rectTransform, Vector2.zero, new Vector2(4f, altoComentario));

                var titulo = ConstructorEscena.CrearTexto(rf, "Criterio", c.nombre + "  ·  " + c.valor + " / 10", 18,
                    TextAnchor.UpperLeft, colorTitulo, FontStyle.Bold);
                Colocar(titulo.rectTransform, new Vector2(16f, -8f), new Vector2(anchoTexto, 24f));

                var cita = ConstructorEscena.CrearTexto(rf, "Evidencia", "“" + c.evidencia + "”", 16,
                    TextAnchor.UpperLeft, colorCita, FontStyle.Italic);
                Colocar(cita.rectTransform, new Vector2(16f, -34f), new Vector2(anchoTexto, altoComentario - 38f));

                y -= altoComentario + separacion;
            }

            if (comentarios.Count > n)
            {
                var mas = ConstructorEscena.CrearTexto(lista, "Mas", "y " + (comentarios.Count - n) + " más antiguos", 15,
                    TextAnchor.MiddleLeft, colorSuave, FontStyle.Italic);
                Colocar(mas.rectTransform, new Vector2(margen, y), new Vector2(anchoLista - margen * 2f, 24f));
                filas.Add(mas.gameObject);
                y -= 24f + separacion;
            }

        }

        // Ancla arriba a la izquierda del padre.
        static void Colocar(RectTransform r, Vector2 posicion, Vector2 tamano)
        {
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = posicion;
            r.sizeDelta = tamano;
        }
    }
}
