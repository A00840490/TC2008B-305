using UnityEngine;
using UnityEngine.UI;

namespace Codea.SharkTank
{
    // Muestra el texto del último mensaje arriba del personaje que habla.
    public class GloboDialogo : MonoBehaviour
    {
        public RectTransform rect;
        public Text titulo;
        public Text texto;
        [Tooltip("El triángulo que señala a quien habla. Se oculta en close up, cuando el globo va abajo como subtítulo.")]
        public GameObject cola;

        [Header("Flecha para avanzar")]
        [Tooltip("Flecha que aparece cuando el mensaje ya se escribió y se puede pasar al siguiente.")]
        public RectTransform flecha;
        public CanvasGroup flechaGrupo;
        public Vector2 posicionFlecha = new Vector2(-34f, 18f);
        [Range(0f, 1f)] public float opacidadFlecha = 0.45f;
        [Tooltip("Pixeles que recorre la flecha de izquierda a derecha al aparecer.")]
        public float recorridoFlecha = 10f;
        [Tooltip("Segundos que tarda en aparecer. Después se queda quieta.")]
        public float duracionFlecha = 0.35f;

        [Tooltip("Letras por segundo al escribir el mensaje.")]
        public float letrasPorSegundo = 90f;

        [Tooltip("Separación entre la cabeza del personaje y el globo, en pixeles.")]
        public float separacion = 10f;

        [Tooltip("En close up el globo va abajo, como subtítulo, para no tapar la cara.")]
        public bool fijoAbajo;
        public float margenAbajo = 110f;

        PersonajeAgente objetivo;
        string completo = "";
        float inicio;
        int escritas = -1;
        bool flechaVisible;
        float inicioFlecha;

        public bool TerminoDeEscribir => escritas >= completo.Length;

        // Regresa cuántos segundos tarda en escribirse el mensaje.
        public float Mostrar(PersonajeAgente quien, string tituloMensaje, string mensaje)
        {
            objetivo = quien;
            completo = (mensaje ?? "").Replace("<", "‹").Replace(">", "›");
            inicio = Time.time;
            escritas = -1;
            MostrarFlecha(false);
            if (titulo != null)
            {
                titulo.text = tituloMensaje;
                titulo.color = quien != null ? Color.Lerp(quien.colorBase, Color.black, 0.35f) : Color.black;
            }
            gameObject.SetActive(true);
            Escribir();
            Posicionar();
            return completo.Length / Mathf.Max(1f, letrasPorSegundo);
        }

        public void Ocultar()
        {
            objetivo = null;
            MostrarFlecha(false);
            gameObject.SetActive(false);
        }

        // Escribe de golpe lo que falta del mensaje.
        public void Completar()
        {
            inicio = Time.time - completo.Length / Mathf.Max(1f, letrasPorSegundo) - 0.01f;
            Escribir();
        }

        public void MostrarFlecha(bool si)
        {
            if (si && !flechaVisible)
                inicioFlecha = Time.time;
            flechaVisible = si;
            if (flecha != null && flecha.gameObject.activeSelf != si)
                flecha.gameObject.SetActive(si);
            AnimarFlecha();
        }

        void LateUpdate()
        {
            Escribir();
            Posicionar();
            AnimarFlecha();
        }

        // Al aparecer entra deslizándose de izquierda a derecha con fade in, y se queda en su lugar.
        void AnimarFlecha()
        {
            if (!flechaVisible || flecha == null)
                return;
            float t = Mathf.Clamp01((Time.time - inicioFlecha) / Mathf.Max(0.01f, duracionFlecha));
            float e = 1f - (1f - t) * (1f - t); // frena al llegar
            flecha.anchoredPosition = posicionFlecha + new Vector2(Mathf.Lerp(-recorridoFlecha, 0f, e), 0f);
            if (flechaGrupo != null)
                flechaGrupo.alpha = opacidadFlecha * e;
        }

        void Escribir()
        {
            if (texto == null)
                return;
            int n = Mathf.Clamp(Mathf.FloorToInt((Time.time - inicio) * letrasPorSegundo), 0, completo.Length);
            if (n == escritas)
                return;
            escritas = n;
            // El resto del texto va transparente para que el tamaño de la letra no cambie mientras se escribe.
            texto.text = n >= completo.Length
                ? completo
                : completo.Substring(0, n) + "<color=#00000000>" + completo.Substring(n) + "</color>";
        }

        void Posicionar()
        {
            var padre = rect != null ? rect.parent as RectTransform : null;
            if (padre == null)
                return;
            if (cola == null && rect != null)
            {
                // Por si el prefab es de antes de tener este campo.
                var hijo = rect.Find("Cola");
                if (hijo != null) cola = hijo.gameObject;
            }
            if (cola != null && cola.activeSelf == fijoAbajo)
                cola.SetActive(!fijoAbajo);

            if (fijoAbajo)
            {
                rect.anchoredPosition = new Vector2(0f, -padre.rect.height * 0.5f + margenAbajo);
                return;
            }
            if (objetivo == null || objetivo.puntoGlobo == null)
                return;
            var cam = Camera.main;
            if (cam == null)
                return;

            Vector3 pantalla = cam.WorldToScreenPoint(objetivo.puntoGlobo.position);
            if (pantalla.z < 0f)
                return;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(padre, pantalla, null, out local))
                return;

            // Mantiene el globo dentro de la pantalla.
            Vector2 mitad = padre.rect.size * 0.5f;
            Vector2 tam = rect.rect.size;
            local.y += separacion;
            local.x = Mathf.Clamp(local.x, -mitad.x + tam.x * 0.5f + 20f, mitad.x - tam.x * 0.5f - 20f);
            local.y = Mathf.Clamp(local.y, -mitad.y + 20f, mitad.y - tam.y - 20f);
            rect.anchoredPosition = local;
        }
    }
}
