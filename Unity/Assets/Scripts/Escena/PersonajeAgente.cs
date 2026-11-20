using UnityEngine;

namespace Codea.SharkTank
{
    // Sirve para los dos personajes. Cuando habla, se ilumina y se mueve un poco.
    public class PersonajeAgente : MonoBehaviour
    {
        public string nombre = "Juez";
        public Color colorBase = new Color(0.25f, 0.45f, 0.8f);

        [Header("Partes")]
        public Transform cuerpo;
        public ColorSolido torso;
        [Tooltip("Otras piezas que se pintan del mismo color que el torso (mangas, pantalón del traje).")]
        public ColorSolido[] ropa;
        [Tooltip("Centro de la cabeza. La cámara la usa para el close up.")]
        public Transform cabeza;
        public Transform puntoGlobo;
        public TextMesh etiqueta;
        public Light foco;

        [Tooltip("Qué tan rápido gira cuando voltea a ver a alguien.")]
        public float velocidadGiro = 6f;

        bool hablando;
        float tiempo;
        bool girando;
        Quaternion giroObjetivo;

        // Voltea poco a poco hacia un punto, sin inclinarse.
        public void MirarHacia(Vector3 punto)
        {
            Vector3 d = punto - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude < 0.0001f)
                return;
            giroObjetivo = Quaternion.LookRotation(d);
            girando = true;
        }

        public void Configurar(string nuevoNombre, Color color)
        {
            nombre = nuevoNombre;
            colorBase = color;
            gameObject.name = nuevoNombre;
            if (etiqueta != null) etiqueta.text = nuevoNombre;
            Pintar(color);
        }

        public void Hablar(bool si)
        {
            hablando = si;
            if (foco != null) foco.enabled = si;
            Pintar(si ? Color.Lerp(colorBase, Color.white, 0.25f) : colorBase);
        }

        void Pintar(Color color)
        {
            if (torso != null) torso.Cambiar(color);
            if (ropa == null)
                return;
            foreach (var pieza in ropa)
                if (pieza != null)
                    pieza.Cambiar(color);
        }

        void Update()
        {
            if (girando)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, giroObjetivo, velocidadGiro * Time.deltaTime);
                if (Quaternion.Angle(transform.rotation, giroObjetivo) < 0.5f)
                {
                    transform.rotation = giroObjetivo;
                    girando = false;
                }
            }

            if (cuerpo == null)
                return;

            if (hablando)
            {
                tiempo += Time.deltaTime;
                float s = 1f + 0.03f * Mathf.Sin(tiempo * 9f);
                cuerpo.localScale = new Vector3(1f, s, 1f);
            }
            else
            {
                tiempo = 0f;
                cuerpo.localScale = Vector3.Lerp(cuerpo.localScale, Vector3.one, 10f * Time.deltaTime);
            }
        }
    }
}
