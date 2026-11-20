using System;

namespace Codea.SharkTank
{
    // Estructuras del JSON que regresa la ruta de consulta del Agente Juez.
    // Los nombres de los campos tienen que ser iguales a los del JSON (JsonUtility).

    [Serializable]
    public class Calificacion
    {
        public string criterio;   // innovacion, sostenibilidad, mercado o rentabilidad
        public int valor;         // de 0 a 10
        public string evidencia;  // texto del pitch que justifica la calificación
    }

    [Serializable]
    public class Oferta
    {
        public long monto;
        public int porcentaje;

        public bool Existe => monto > 0 && porcentaje > 0;
    }

    [Serializable]
    public class Mensaje
    {
        public string id;
        public string emisor;  // juez o emprendedor
        public string tipo;    // pitch, pregunta, respuesta, oferta, aceptar, rechazar o retroalimentacion
        public string texto;
        public int turno;
        // Opcional: qué juez habla (1, 2 o 3). Si no viene (0), habla uno por turno, rotando.
        public int juez;
    }

    [Serializable]
    public class SesionDatos
    {
        public string sesion_id;
        public string estado;          // en_curso o terminada
        public int turno;
        public int turno_maximo = 8;
        public string decision;        // pendiente, contraoferta, invertir o no_invertir
        public Oferta oferta_actual;
        public Calificacion[] calificaciones;
        public Mensaje[] mensajes;

        public bool Terminada => estado == "terminada";
    }

    // Formato del archivo de la sesión simulada: cada paso agrega un mensaje
    // y, si trae datos, actualiza el turno, la decisión, la oferta o las calificaciones.
    [Serializable]
    public class PasoSimulado
    {
        public Mensaje mensaje;
        public string estado;
        public int turno;
        public string decision;
        public Oferta oferta_actual;
        public Calificacion[] calificaciones;
    }

    // Formato de GET /api/conversation-history del backend (shark-tank-app).
    // ClienteAPI.ConvertirBackend lo pasa a SesionDatos.
    [Serializable]
    public class MensajeBackend
    {
        public string role;         // Entrepreneur o Judge
        public string content;
        public string sender_name;  // nombre del emprendedor
        public string judge_name;   // nombre del juez
    }

    [Serializable]
    public class IdeaBackend
    {
        public string name;
        public string investment_needed;
    }

    [Serializable]
    public class HistorialBackend
    {
        public MensajeBackend[] conversation_history;
        public IdeaBackend business_idea;
    }

    [Serializable]
    public class SesionSimulada
    {
        public string sesion_id;
        public int turno_maximo = 8;
        public PasoSimulado[] pasos;
    }
}
