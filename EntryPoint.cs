using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace Hikaria.HostEnemyLimbDestroyFix;

[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
public class EntryPoint : BasePlugin
{
    public override void Load()
    {
        Instance = this;

        m_Harmony = new Harmony(PluginInfo.GUID);
        m_Harmony.PatchAll();

        Logs.LogMessage("OK");
    }

    public static EntryPoint Instance { get; private set; }

    private static Harmony m_Harmony;
}
