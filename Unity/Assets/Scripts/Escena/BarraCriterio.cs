using UnityEngine;
using UnityEngine.UI;

namespace Codea.SharkTank
{
    // Muestra la calificación de un criterio, de 0 a 10. La evidencia ya no va aquí sino en ListaComentarios;
    // si se le asigna un texto de evidencia, también la escribe.
    // El color sigue la regla de decisión: menos de 4 es falla grave, de 7 en adelante es bueno.
    public class BarraCriterio : MonoBehaviour
    {
        public string criterio = "innovacion";
        public Text etiqueta;
        public Text textoValor;
        public Text textoEvidencia;
        public RectTransform relleno;
        public Image imagenRelleno;

        [Tooltip("Qué tan rápido se mueve la barra, en puntos por segundo.")]
        public float velocidad = 6f;

        float objetivo;
        float actual;

        public void Configurar(string clave, string nombre)
        {
            criterio = clave;
            if (etiqueta != null)
                etiqueta.text = nombre;
        }

        public void Mostrar(int valor, string evidencia)
        {
            objetivo = Mathf.Clamp(valor, 0, 10);
            if (textoValor != null) textoValor.text = valor + " / 10";
            if (textoEvidencia != null) textoEvidencia.text = string.IsNullOrEmpty(evidencia) ? "" : "“" + evidencia + "”";
            if (imagenRelleno != null) imagenRelleno.color = ColorPara(valor);
        }

        public void Limpiar()
        {
            objetivo = 0f;
            actual = 0f;
            if (textoValor != null) textoValor.text = "– / 10";
            if (textoEvidencia != null) textoEvidencia.text = "Sin calificar";
            Dibujar();
        }

        void Update()
        {
            if (!Mathf.Approximately(actual, objetivo))
            {
                actual = Mathf.MoveTowards(actual, objetivo, velocidad * Time.deltaTime);
                Dibujar();
            }
        }

        void Dibujar()
        {
            if (relleno != null)
                relleno.anchorMax = new Vector2(actual / 10f, 1f);
        }

        public static Color ColorPara(int valor)
        {
            if (valor < 4) return new Color(0.9f, 0.3f, 0.28f);
            if (valor < 7) return new Color(0.95f, 0.7f, 0.2f);
            return new Color(0.3f, 0.75f, 0.4f);
        }
    }
}
