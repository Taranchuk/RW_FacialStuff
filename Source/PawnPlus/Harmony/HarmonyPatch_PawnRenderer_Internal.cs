using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnPlus.Harmony
{
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnInternal))]
    public static class HarmonyPatch_PawnRenderer_Internal
    {
        public static void Prefix(PawnRenderer __instance, ref PawnDrawParms parms)
        {
            if (__instance.pawn.GetCompAnim(out var compAnim) && !compAnim.Deactivated)
            {
                compAnim.TickDrawers(parms.facing);

                Vector3 rootLoc = parms.matrix.GetColumn(3);
                Quaternion quat = parms.matrix.rotation;
                Vector3 scale = parms.matrix.lossyScale;
                
                Vector3 footPos = rootLoc;
                compAnim.ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
                
                parms.matrix = Matrix4x4.TRS(rootLoc, quat, scale);
            }
        }
    }
}