using UnityEngine;

namespace Codea.SharkTank
{
    public enum TipoToma
    {
        General,  // los dos agentes en cuadro
        Cercana   // close up de quien habla
    }

    // Mueve la cámara entre una toma general y un close up de quien habla.
    // De la general al close up la cámara se desliza rápido; desde un close up siempre es corte.
    // Los momentos de negociación importantes (aceptar o rechazar) van en toma general,
    // y después de varios close ups seguidos regresa a la general para no perder el contexto.
    public class CamaraDirector : MonoBehaviour
    {
        public Camera camara;
        public PersonajeAgente[] jueces;
        public PersonajeAgente emprendedor;

        [Header("Toma general")]
        [Tooltip("Alta y de frente, con perspectiva, para que quepan el emprendedor, los tres jueces y la pared.")]
        public Vector3 posicionGeneral = new Vector3(-0.5f, 6.5f, -10.5f);
        public Vector3 objetivoGeneral = new Vector3(0.2f, 1.2f, 2.2f);
        public float campoGeneral = 42f;

        [Header("Close up")]
        [Tooltip("Distancia de la cámara a la cabeza, hacia el lado del otro agente.")]
        public float distanciaCercana = 1.8f;
        [Tooltip("Cuánto se recorre la cámara hacia el frente del set para ver la cara de tres cuartos.")]
        public float desplazamientoFrente = 0.9f;
        [Tooltip("Altura de la cabeza si el personaje no tiene asignada su Cabeza.")]
        public float alturaCabeza = 2.05f;
        public float campoCercano = 35f;
        [Tooltip("Close up de un juez: distancia de la cámara, de frente, para que quede centrado.")]
        public float distanciaJuez = 2.0f;

        [Header("Ritmo")]
        [Tooltip("Close ups seguidos antes de volver a la toma general.")]
        public int cercanasAntesDeGeneral = 2;
        [Tooltip("Segundos del movimiento de la toma general al close up. Con 0 también es corte.")]
        public float duracionHaciaCercana = 0.4f;

        public TipoToma TomaActual { get; private set; } = TipoToma.General;

        int cercanasSeguidas;
        Vector3 posDesde, posHacia;
        Quaternion rotDesde, rotHacia;
        float campoDesde, campoHacia;
        float inicio;
        float duracion;
        bool moviendo;

        // Elige la toma para un mensaje nuevo y regresa cuál eligió.
        public TipoToma Elegir(PersonajeAgente habla, string tipo, PersonajeAgente escucha = null)
        {
            bool momentoClave = tipo == "aceptar" || tipo == "rechazar";
            if (habla == null || momentoClave || cercanasSeguidas >= cercanasAntesDeGeneral)
            {
                General();
            }
            else
            {
                Cercana(habla, escucha);
                cercanasSeguidas++;
            }
            return TomaActual;
        }

        public void General()
        {
            cercanasSeguidas = 0;
            TomaActual = TipoToma.General;
            // Hacia la toma general siempre es corte.
            Ir(posicionGeneral, Quaternion.LookRotation(objetivoGeneral - posicionGeneral), campoGeneral, 0f);
        }

        public void Cercana(PersonajeAgente quien, PersonajeAgente otro = null)
        {
            if (quien != emprendedor)
            {
                CercanaJuez(quien);
                return;
            }
            if (otro == null && jueces != null && jueces.Length > 0)
                otro = jueces[jueces.Length / 2];
            Vector3 pies = quien.transform.position;
            // Si el personaje marca dónde está su cabeza (el juez va sentado), se usa esa altura.
            Vector3 cabeza = quien.cabeza != null ? quien.cabeza.position : pies + Vector3.up * alturaCabeza;

            Vector3 haciaOtro = otro != null ? otro.transform.position - pies : quien.transform.forward;
            haciaOtro.y = 0f;
            haciaOtro.Normalize();

            Vector3 haciaFrente = posicionGeneral - pies;
            haciaFrente.y = 0f;
            haciaFrente.Normalize();

            Vector3 posicion = cabeza + haciaOtro * distanciaCercana + haciaFrente * desplazamientoFrente + Vector3.up * 0.1f;
            Vector3 mira = cabeza - Vector3.up * 0.15f;

            // Solo se desliza si viene de la toma general; de un close up a otro es corte.
            float tiempo = TomaActual == TipoToma.General ? duracionHaciaCercana : 0f;
            TomaActual = TipoToma.Cercana;
            Ir(posicion, Quaternion.LookRotation(mira - posicion), campoCercano, tiempo);
        }

        // De frente al juez, a la altura de su cabeza, para que quede centrado en la toma.
        void CercanaJuez(PersonajeAgente quien)
        {
            Vector3 cabeza = quien.cabeza != null ? quien.cabeza.position : quien.transform.position + Vector3.up * alturaCabeza;
            Vector3 frente = quien.transform.forward;
            frente.y = 0f;
            frente.Normalize();
            Vector3 posicion = cabeza + frente * distanciaJuez + Vector3.up * 0.15f;
            Vector3 mira = cabeza - Vector3.up * 0.1f;
            float tiempo = TomaActual == TipoToma.General ? duracionHaciaCercana : 0f;
            TomaActual = TipoToma.Cercana;
            Ir(posicion, Quaternion.LookRotation(mira - posicion), campoCercano, tiempo);
        }

        void Ir(Vector3 posicion, Quaternion rotacion, float campo, float segundos)
        {
            if (camara == null)
                camara = Camera.main;
            if (camara == null)
                return;

            posDesde = camara.transform.position;
            rotDesde = camara.transform.rotation;
            campoDesde = camara.fieldOfView;
            posHacia = posicion;
            rotHacia = rotacion;
            campoHacia = campo;
            inicio = Time.time;
            duracion = segundos;
            moviendo = true;

            if (duracion <= 0f)
                Aplicar(1f);
        }

        void Update()
        {
            if (!moviendo)
                return;
            float t = duracion <= 0f ? 1f : Mathf.Clamp01((Time.time - inicio) / duracion);
            Aplicar(Mathf.SmoothStep(0f, 1f, t));
        }

        void Aplicar(float t)
        {
            camara.transform.SetPositionAndRotation(
                Vector3.Lerp(posDesde, posHacia, t),
                Quaternion.Slerp(rotDesde, rotHacia, t));
            camara.fieldOfView = Mathf.Lerp(campoDesde, campoHacia, t);
            if (t >= 1f)
                moviendo = false;
        }
    }
}
