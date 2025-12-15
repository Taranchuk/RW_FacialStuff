using System.Linq;
using PawnPlus.Defs;
using RimWorld;
using Verse;

namespace PawnPlus
{
    public static class PoseCycleDefExtensions
    {
        public static PawnKeyframe GetKeyframe(this PoseCycleDef pose, float percent)
        {
            if (pose.keyframes.NullOrEmpty())
            {
                return null;
            }
            int keyIndex = (int)(percent * 100f);
            PawnKeyframe result = null;
            foreach (var key in pose.keyframes.OrderBy(k => k.KeyIndex))
            {
                if (key.KeyIndex <= keyIndex)
                {
                    result = key;
                }
                else
                {
                    break;
                }
            }
            return result;
        }
    }
}
