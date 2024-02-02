using GorillaLocomotion;
using HarmonyLib;
using monkeylove.Source.Tools;

namespace monkeylove.Source.Patches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    public static class ExamplePatch
    {
        static void Postfix() => Logging.Debug(PluginInfo.Name, "patches added successfully.");
    }
}
