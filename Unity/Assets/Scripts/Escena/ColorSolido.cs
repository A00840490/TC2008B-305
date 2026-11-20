using UnityEngine;

namespace Codea.SharkTank
{
    // Pinta un objeto de un color (y opcionalmente una textura) sin crear materiales nuevos.
    // Funciona con el shader Standard y con el Lit de URP, y también en el editor.
    [ExecuteAlways]
    public class ColorSolido : MonoBehaviour
    {
        public Color color = Color.white;

        [Tooltip("Opcional. Se multiplica por el color (por ejemplo, la veta de la madera o el logo).")]
        public Texture textura;
        [Tooltip("Cuántas veces se repite la textura en cada eje.")]
        public Vector2 repetir = Vector2.one;
        [Tooltip("Desplazamiento de la textura (para que piezas iguales no se vean idénticas).")]
        public Vector2 desplazar = Vector2.zero;
        [Tooltip("Qué tan brillante se ve la superficie (0 = mate, 1 = espejo). Con -1 se queda la del material.")]
        public float brillo = -1f;
        [Tooltip("Recorta lo transparente de la textura (por ejemplo, lo de afuera del icono redondo).")]
        public bool recorte;

        static readonly int IdColor = Shader.PropertyToID("_Color");
        static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int IdMainTex = Shader.PropertyToID("_MainTex");
        static readonly int IdMainTexST = Shader.PropertyToID("_MainTex_ST");
        static readonly int IdBaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int IdBaseMapST = Shader.PropertyToID("_BaseMap_ST");
        static readonly int IdGlossiness = Shader.PropertyToID("_Glossiness");
        static readonly int IdSmoothness = Shader.PropertyToID("_Smoothness");

        MaterialPropertyBlock bloque;

        // Un solo material con recorte para todas las piezas que lo usan. No se guarda en la escena:
        // se vuelve a poner cada vez que se activa la pieza.
        static Material materialRecorte;

        void OnEnable()
        {
            PonerMaterial();
            Aplicar();
        }

        void OnValidate()
        {
            Aplicar();
        }

        void PonerMaterial()
        {
            if (!recorte)
                return;
            var r = GetComponent<Renderer>();
            var m = MaterialRecorte();
            if (r != null && m != null && r.sharedMaterial != m)
                r.sharedMaterial = m;
        }

        static Material MaterialRecorte()
        {
            if (materialRecorte != null)
                return materialRecorte;
            var shader = Shader.Find("Standard");
            bool urp = false;
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
                urp = shader != null;
            }
            if (shader == null)
                return null;

            var m = new Material(shader) { name = "SharkMind Recorte", hideFlags = HideFlags.DontSave };
            m.SetFloat("_Cutoff", 0.5f);
            if (urp)
            {
                m.SetFloat("_AlphaClip", 1f);
            }
            else
            {
                m.SetFloat("_Mode", 1f); // Cutout
                m.SetOverrideTag("RenderType", "TransparentCutout");
            }
            m.EnableKeyword("_ALPHATEST_ON");
            m.renderQueue = 2450; // AlphaTest
            materialRecorte = m;
            return m;
        }

        public void Aplicar()
        {
            var r = GetComponent<Renderer>();
            if (r == null)
                return;
            if (bloque == null)
                bloque = new MaterialPropertyBlock();
            r.GetPropertyBlock(bloque);
            bloque.SetColor(IdColor, color);
            bloque.SetColor(IdBaseColor, color);
            if (textura != null)
            {
                var st = new Vector4(repetir.x, repetir.y, desplazar.x, desplazar.y);
                bloque.SetTexture(IdMainTex, textura);
                bloque.SetVector(IdMainTexST, st);
                bloque.SetTexture(IdBaseMap, textura);
                bloque.SetVector(IdBaseMapST, st);
            }
            if (brillo >= 0f)
            {
                bloque.SetFloat(IdGlossiness, brillo);
                bloque.SetFloat(IdSmoothness, brillo);
            }
            r.SetPropertyBlock(bloque);
        }

        public void Cambiar(Color nuevo)
        {
            color = nuevo;
            Aplicar();
        }

        public void Texturizar(Texture nueva, Vector2 veces, float nuevoBrillo = -1f, Vector2 corrimiento = default)
        {
            textura = nueva;
            repetir = veces;
            desplazar = corrimiento;
            brillo = nuevoBrillo;
            PonerMaterial();
            Aplicar();
        }
    }
}
