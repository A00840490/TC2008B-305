using UnityEngine;

namespace Codea.SharkTank
{
    // Lee el clic y las teclas con el sistema de entrada que tenga activo el proyecto (el nuevo o el clásico).
    // Un clic que ya usó un botón de la interfaz se "consume" para que no avance también la conversación.
    public static class Entrada
    {
        static int frameConsumido = -1;

        public static bool Clic(out Vector2 posicion)
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                posicion = mouse.position.ReadValue();
                return true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0))
            {
                posicion = Input.mousePosition;
                return true;
            }
#endif
            posicion = Vector2.zero;
            return false;
        }

        // Clic que ningún botón de la interfaz usó en este frame.
        public static bool ClicLibre()
        {
            return Clic(out _) && frameConsumido != Time.frameCount;
        }

        public static void Consumir()
        {
            frameConsumido = Time.frameCount;
        }

        // Espacio, Enter o flecha derecha.
        public static bool TeclaAvanzar()
        {
#if ENABLE_INPUT_SYSTEM
            var teclado = UnityEngine.InputSystem.Keyboard.current;
            return teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame
                || teclado.numpadEnterKey.wasPressedThisFrame || teclado.rightArrowKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.RightArrow);
#else
            return false;
#endif
        }

        // Para un Canvas en Screen Space Overlay.
        public static bool Dentro(RectTransform rect, Vector2 posicion)
        {
            return rect != null && rect.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(rect, posicion, null);
        }
    }
}
