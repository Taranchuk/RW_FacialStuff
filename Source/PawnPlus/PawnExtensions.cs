namespace PawnPlus
{
    using JetBrains.Annotations;

    using RimWorld;

    using Verse;
    using Verse.AI;

    public class BodyProps
    {
        public BodyPartRecord _rightFoot;
        public BodyPartRecord _leftFoot;
        public BodyPartRecord _rightHand;
        public BodyPartRecord _leftHand;
        public BodyPartRecord _rightEar;
        public BodyPartRecord _leftEar;
        public CompBodyAnimator _anim;
        public Hediff _hediff;

        public BodyProps(Hediff hediff, CompBodyAnimator anim, BodyPartRecord leftHand, BodyPartRecord rightHand, BodyPartRecord leftFoot, BodyPartRecord rightFoot)
        {
            this._hediff = hediff;
            this._anim = anim;
            this._leftHand = leftHand;
            this._rightHand = rightHand;
            this._leftFoot = leftFoot;
            this._rightFoot = rightFoot;
        }
    }

    public static class PawnExtensions
    {
        public static bool Aiming(this Pawn pawn)
        {
            return 
                pawn.stances != null && 
                pawn.stances.curStance is Stance_Busy stanceBusy && 
                !stanceBusy.neverAimWeapon &&
                stanceBusy.focusTarg.IsValid;
        }

        public static bool ShowWeaponOpenly(this Pawn pawn)
        {
            return pawn.carryTracker?.CarriedThing == null && pawn.equipment?.Primary != null &&
                   (pawn.Drafted ||
                    (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) ||
                    (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
        }

        public static bool Fleeing(this Pawn pawn)
        {
            Job job = pawn.CurJob;
            return pawn.MentalStateDef == MentalStateDefOf.PanicFlee
                || (job != null && (job.def == JobDefOf.Flee || job.def == JobDefOf.FleeAndCower));
        }

        [CanBeNull]
        public static CompBodyAnimator GetCompAnim([NotNull] this Pawn pawn)
        {
            return pawn.GetComp<CompBodyAnimator>();
        }

        public static bool GetCompAnim([NotNull] this Pawn pawn, [NotNull] out CompBodyAnimator compAnim)
        {
            compAnim = pawn.GetComp<CompBodyAnimator>();
            return compAnim != null;
        }

        
        public static bool HasCompAnimator([NotNull] this Pawn pawn)
        {
            return pawn.def.HasComp(typeof(CompBodyAnimator));
        }
    }
}