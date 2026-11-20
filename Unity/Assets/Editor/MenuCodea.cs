using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Codea.SharkTank.Herramientas
{
    // Menú "Codea > Crear escena Shark Tank": guarda los prefabs de los tres jueces (PersonajeJuezFinanzas,
    // PersonajeJuezInnovacion y PersonajeJuezMercado), PersonajeEmprendedor,
    // GloboDialogo y BarraCriterio, y arma la escena SharkTank.unity con ellos.
    public static class MenuCodea
    {
        const string CarpetaPrefabs = "Assets/Prefabs";
        const string CarpetaEscenas = "Assets/Scenes";
        const string RutaEscena = CarpetaEscenas + "/SharkTank.unity";

        [MenuItem("Codea/Crear escena Shark Tank")]
        static void CrearEscena()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (File.Exists(RutaEscena) &&
                !EditorUtility.DisplayDialog("Codea", "Ya existe " + RutaEscena + ". ¿Reemplazarla junto con los prefabs?", "Reemplazar", "Cancelar"))
                return;

            CrearCarpeta(CarpetaPrefabs);
            CrearCarpeta(CarpetaEscenas);

            var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var jueces = new GameObject[ConstructorEscena.Jueces.Length];
            for (int i = 0; i < jueces.Length; i++)
                jueces[i] = GuardarPrefab(ConstructorEscena.CrearJuez(ConstructorEscena.Jueces[i]), "PersonajeJuez" + ConstructorEscena.Jueces[i].Clave);
            var emprendedor = GuardarPrefab(ConstructorEscena.CrearEmprendedor(), "PersonajeEmprendedor");
            var globo = GuardarPrefab(ConstructorEscena.CrearGlobo(), "GloboDialogo");
            var barra = GuardarPrefab(ConstructorEscena.CrearBarra(), "BarraCriterio");

            ConstructorEscena.Construir(new FabricaPrefabs(jueces, emprendedor, globo, barra));
            RegistrarCambiosEnPrefabs();
            EditorSceneManager.SaveScene(escena, RutaEscena);

            var escenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!escenas.Exists(s => s.path == RutaEscena))
            {
                escenas.Insert(0, new EditorBuildSettingsScene(RutaEscena, true));
                EditorBuildSettings.scenes = escenas.ToArray();
            }

            EditorUtility.DisplayDialog("Codea", "Listo: " + RutaEscena + "\nPresiona Play para ver la sesión de ejemplo.", "OK");
        }

        static void CrearCarpeta(string ruta)
        {
            if (!AssetDatabase.IsValidFolder(ruta))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(ruta).Replace('\\', '/'), Path.GetFileName(ruta));
        }

        // Los cambios hechos por código a una instancia de prefab (nombre, color, posición)
        // hay que registrarlos para que se guarden en la escena.
        static void RegistrarCambiosEnPrefabs()
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(t))
                    continue;
                PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
                foreach (var c in t.GetComponents<Component>())
                    if (c != null)
                        PrefabUtility.RecordPrefabInstancePropertyModifications(c);
            }
        }

        static GameObject GuardarPrefab(GameObject temporal, string nombre)
        {
            string ruta = CarpetaPrefabs + "/" + nombre + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(temporal, ruta);
            Object.DestroyImmediate(temporal);
            return prefab;
        }

        class FabricaPrefabs : IFabrica
        {
            readonly GameObject[] jueces;
            readonly GameObject emprendedor, globo, barra;

            public FabricaPrefabs(GameObject[] jueces, GameObject emprendedor, GameObject globo, GameObject barra)
            {
                this.jueces = jueces;
                this.emprendedor = emprendedor;
                this.globo = globo;
                this.barra = barra;
            }

            public GameObject Juez(int indice) => (GameObject)PrefabUtility.InstantiatePrefab(jueces[indice]);
            public GameObject Emprendedor() => (GameObject)PrefabUtility.InstantiatePrefab(emprendedor);
            public GameObject Globo() => (GameObject)PrefabUtility.InstantiatePrefab(globo);
            public GameObject Barra() => (GameObject)PrefabUtility.InstantiatePrefab(barra);
        }
    }
}
