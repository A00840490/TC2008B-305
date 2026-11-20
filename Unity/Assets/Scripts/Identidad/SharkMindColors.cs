using UnityEngine;

// SharkMind · paleta v1 (misma que identidad/sharkmind/paleta.css).
public static class SharkMindColors
{
    public static readonly Color32 Ocean = new Color32(11, 31, 58, 255); // #0B1F3A
    public static readonly Color32 Aleta = new Color32(14, 124, 134, 255); // #0E7C86
    public static readonly Color32 Oro = new Color32(242, 181, 68, 255); // #F2B544
    public static readonly Color32 Mordida = new Color32(255, 90, 95, 255); // #FF5A5F
    public static readonly Color32 Tiburon = new Color32(138, 155, 168, 255); // #8A9BA8
    public static readonly Color32 Espuma = new Color32(244, 247, 250, 255); // #F4F7FA
    public static readonly Color32 AletaClara = new Color32(63, 184, 194, 255); // #3FB8C2

    // Estados de decisión del juez
    public static readonly Color32 EstadoInvertir = new Color32(242, 181, 68, 255); // Invertir
    public static readonly Color32 EstadoContraoferta = new Color32(14, 124, 134, 255); // Contraoferta
    public static readonly Color32 EstadoNoInvertir = new Color32(255, 90, 95, 255); // No invertir
    public static readonly Color32 EstadoPregunta = new Color32(63, 184, 194, 255); // Pregunta / pensando
    public static readonly Color32 EstadoEsperando = new Color32(138, 155, 168, 255); // Esperando respuesta
}
