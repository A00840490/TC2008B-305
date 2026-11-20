using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Codea.SharkTank
{
    // Actualiza las barras de los cuatro criterios, el turno, la oferta y la decisión.
    public class PanelCalificaciones : MonoBehaviour
    {
        public BarraCriterio[] barras;
        public Text textoPromedio;
        public Text textoTurno;
        public Text textoOferta;
        public Text textoDecision;
        [Tooltip("Lista que se despliega con el botón de la burbuja de chat.")]
        public ListaComentarios comentarios;

        // Último valor y evidencia de cada criterio, para saber cuándo el juez tomó un comentario nuevo.
        readonly Dictionary<string, string> vistos = new Dictionary<string, string>();

        public static readonly string[] Criterios = { "innovacion", "sostenibilidad", "mercado", "rentabilidad" };
        public static readonly string[] Nombres = { "Innovación", "Sostenibilidad", "Mercado", "Rentabilidad" };

        public void Mostrar(SesionDatos d)
        {
            int suma = 0, cuantos = 0;
            foreach (var barra in barras)
            {
                if (barra == null)
                    continue;
                var c = Buscar(d.calificaciones, barra.criterio);
                if (c != null)
                {
                    barra.Mostrar(c.valor, c.evidencia);
                    RegistrarComentario(barra, c);
                    suma += c.valor;
                    cuantos++;
                }
                else
                {
                    barra.Limpiar();
                }
            }

            if (textoPromedio != null)
                textoPromedio.text = "Promedio: " + (cuantos == Criterios.Length
                    ? (suma / (float)cuantos).ToString("0.0", CultureInfo.InvariantCulture)
                    : "–");
            if (textoTurno != null)
                textoTurno.text = "Turno: " + (d.turno > 0 ? d.turno.ToString() : "–") + " de " + d.turno_maximo;
            if (textoOferta != null)
                textoOferta.text = "Oferta: " + GestorConversacion.FormatoOferta(d.oferta_actual);
            if (textoDecision != null)
            {
                textoDecision.text = "Decisión: " + NombreDecision(d.decision);
                textoDecision.color = ColorDecision(d.decision);
            }
        }

        void RegistrarComentario(BarraCriterio barra, Calificacion c)
        {
            if (string.IsNullOrEmpty(c.evidencia))
                return;
            string clave = c.valor + "|" + c.evidencia;
            if (vistos.TryGetValue(c.criterio, out var anterior) && anterior == clave)
                return;
            vistos[c.criterio] = clave;
            if (comentarios != null)
                comentarios.Agregar(barra.etiqueta != null ? barra.etiqueta.text : c.criterio, c.valor, c.evidencia);
        }

        public void Limpiar()
        {
            vistos.Clear();
            if (comentarios != null)
                comentarios.Limpiar();
            foreach (var barra in barras)
                if (barra != null)
                    barra.Limpiar();
            if (textoPromedio != null) textoPromedio.text = "Promedio: –";
            if (textoTurno != null) textoTurno.text = "Turno: –";
            if (textoOferta != null) textoOferta.text = "Oferta: –";
            if (textoDecision != null)
            {
                textoDecision.text = "Decisión: –";
                textoDecision.color = Color.white;
            }
        }

        static Calificacion Buscar(Calificacion[] lista, string criterio)
        {
            if (lista == null)
                return null;
            foreach (var c in lista)
                if (c != null && c.criterio == criterio)
                    return c;
            return null;
        }

        static string NombreDecision(string decision)
        {
            switch (decision)
            {
                case "invertir": return "Invertir";
                case "contraoferta": return "Contraoferta";
                case "no_invertir": return "No invertir";
                default: return "Evaluando…";
            }
        }

        static Color ColorDecision(string decision)
        {
            switch (decision)
            {
                // Colores de estado de la paleta SharkMind.
                case "invertir": return SharkMindColors.EstadoInvertir;
                case "contraoferta": return SharkMindColors.AletaClara; // Aleta aclarada para que se lea sobre el panel oscuro
                case "no_invertir": return SharkMindColors.EstadoNoInvertir;
                default: return Color.white;
            }
        }
    }
}
