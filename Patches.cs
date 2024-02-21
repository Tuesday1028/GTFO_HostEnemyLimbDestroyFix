using CharacterDestruction;
using Gear;
using HarmonyLib;
using SNetwork;

namespace Hikaria.HostEnemyLimbDestroyFix.Patches;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.Fire))]
    [HarmonyPrefix]
    private static void Shotgun__Fire__Prefix(Shotgun __instance)
    {
        AllowDestroyLimb = !(SNet.IsMaster && (__instance.Owner.IsLocallyOwned || __instance.Owner.Owner.IsBot));
    }

    [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi))]
    [HarmonyPrefix]
    private static void SentryGunInstance_Firing_Bullets__UpdateFireShotgunSemi__Prefix(bool isMaster)
    {
        AllowDestroyLimb = !(SNet.IsMaster && isMaster);
    }

    [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi))]
    [HarmonyPostfix]
    private static void SentryGunInstance_Firing_Bullets__UpdateFireShotgunSemi__Postfix()
    {
        if (AllowDestroyLimb)
            return;
        AllowDestroyLimb = true;
        while (DestroyLimbDataQueue.TryDequeue(out var tuple))
        {
            var dam = tuple.Item1;
            if ((dam?.DamageLimbs[tuple.Item2]?.IsDestroyed ?? true) || dam.Owner == null)
                continue;
            dam.SendDestroyLimb(tuple.Item2, tuple.Item3);
        }
    }


    [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.Fire))]
    [HarmonyPostfix]
    private static void Shotgun__Fire__Postfix()
    {
        if (AllowDestroyLimb)
            return;
        AllowDestroyLimb = true;
        while (DestroyLimbDataQueue.TryDequeue(out var tuple))
        {
            var dam = tuple.Item1;
            if ((dam?.DamageLimbs[tuple.Item2]?.IsDestroyed ?? true) || dam.Owner == null)
                continue;
            dam.SendDestroyLimb(tuple.Item2, tuple.Item3);
        }

    }

    [HarmonyPatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.SendDestroyLimb))]
    [HarmonyPrefix]
    private static bool Dam_EnemyDamageBase__SendDestroyLimb__Prefix(Dam_EnemyDamageBase __instance, int limbID, sDestructionEventData destructionEventData)
    {
        if (!AllowDestroyLimb)
        {
            DestroyLimbDataQueue.Enqueue(new(__instance, limbID, destructionEventData));
            return false;
        }
        return true;
    }

    private static bool AllowDestroyLimb;

    private static Queue<Tuple<Dam_EnemyDamageBase, int, sDestructionEventData>> DestroyLimbDataQueue = new();
}
