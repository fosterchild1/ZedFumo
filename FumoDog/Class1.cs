using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace FumoDog
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class FumoDogBase: BaseUnityPlugin
    {
        private const string modGUID = "fosterchild.FumoDog";
        private const string modName = "FumoDog";
        private const string modVersion = "1.0.0";

        public static FumoDogBase Instance;

        private readonly Harmony harmony = new Harmony(modGUID);

        public ManualLogSource mls = null;
        internal static Dictionary<string, GameObject> Models { get; private set; } = new Dictionary<string, GameObject>();
        public void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            if (Instance == null)
            {
                Instance = this;
            }
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);

            string location = Instance.Info.Location;
            location = location.TrimEnd("FumoDog".ToCharArray());

            AssetBundle model = AssetBundle.LoadFromFile(location.Remove(location.IndexOf("FumoDog.dll"), 11) + "model");
            GameObject[] array2 = model.LoadAllAssets<GameObject>();
            List<GameObject> list3 = new List<GameObject>(array2.Length);
            list3.AddRange(array2);
            List<GameObject> list4 = list3;
            list4.ForEach(delegate (GameObject model2)
            {
                Models.Add(model2.name, model2);
            });

            mls.LogInfo("Loaded the fumo successfully");
        }


    }
}

namespace FumoDog.Patches
{

    [HarmonyPatch(typeof(GrabbableObject))]
    internal class PipePatch
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void ModelPatch(GrabbableObject __instance)
        {

            if (__instance != null && __instance.itemProperties.name == "Zeddog")
            {
                __instance.gameObject.AddComponent<Fumo>();
            }
        }
    }

    internal class Fumo : MonoBehaviour
    {
        private GrabbableObject instance;
        
        private void hideModel()
        {
            Renderer[] componentsInChildren = instance.GetComponentsInChildren<Renderer>();
            Renderer[] array = componentsInChildren;
            foreach (Renderer obj in array)
            {
                if (obj.name == "ZeddogPlushie(Clone)")
                {
                    Renderer[] components2 = obj.GetComponentsInChildren<Renderer>();
                    Renderer[] array2 = components2;
                    foreach (Renderer obj2 in array2)
                    {
                        obj2.enabled = false;
                    }
                }
            }
        }
        private void Start()
        {
            FumoDogBase.Instance.mls.LogInfo(this.name);
            instance = this.GetComponent<GrabbableObject>();

            if (FumoDogBase.Models.TryGetValue("fumo", out GameObject value))
            {
                GameObject fumo = UnityEngine.Object.Instantiate<GameObject>(value, instance.gameObject.transform);
                fumo.name = "Fumo(Clone)";
                fumo.transform.localPosition = Vector3.zero;
                fumo.transform.localScale = Vector3.one;
                Transform transform = fumo.transform;
                fumo.SetActive(true);
            }
        }
        private void Update()
        {
            hideModel();
        }
    }
}
