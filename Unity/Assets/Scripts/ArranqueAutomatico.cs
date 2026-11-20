using UnityEngine;

namespace Codea.SharkTank
{
    // Si la escena abierta no tiene un GestorConversacion, arma la escena completa al darle Play.
    // Así el proyecto funciona aunque todavía no se haya creado la escena con el menú Codea.
    public static class ArranqueAutomatico
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Arrancar()
        {
            if (Object.FindFirstObjectByType<GestorConversacion>() != null)
                return;
            ConstructorEscena.Construir(new FabricaDirecta());
        }
    }
}
