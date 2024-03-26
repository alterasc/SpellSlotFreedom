using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace SpellSlotFreedom;

#if DEBUG
[EnableReloading]
#endif
static class Main
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;

    static bool Load(UnityModManager.ModEntry modEntry)
    {
        log = modEntry.Logger;
#if DEBUG
        modEntry.OnUnload = OnUnload;
#endif
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

#if DEBUG
    static bool OnUnload(UnityModManager.ModEntry modEntry)
    {
        HarmonyInstance.UnpatchAll(modEntry.Info.Id);
        return true;
    }
#endif

    public static void LogTrace(string message)
    {
#if DEBUG
        log.Log(message);
#endif
    }
}