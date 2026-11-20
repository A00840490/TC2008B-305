using UnityEngine;

namespace Codea.SharkTank
{
    // Mantiene un texto 3D de frente a la cámara, aunque el personaje esté de perfil.
    public class MirarCamara : MonoBehaviour
    {
        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam != null)
                transform.rotation = cam.transform.rotation;
        }
    }
}
