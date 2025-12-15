namespace PawnPlus
{
    using System.Linq;

    using PawnPlus.AnimatorWindows;
    using PawnPlus.Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class QuadrupedDrawer : HumanBipedDrawer
    {
        public override bool GetLimbWorldTransform(string limbType, Vector3 rootLoc, Quaternion bodyQuat, out Vector3 finalWorldPosition, out Quaternion finalRotation)
        {
            finalWorldPosition = Vector3.zero;
            finalRotation = Quaternion.identity;

            if (this.ShouldBeIgnored())
            {
                return false;
            }

            float factor = 1f;
            if (this.Pawn.kindDef.lifeStages.Any())
            {
                Vector2 maxSize = this.Pawn.kindDef.lifeStages.Last().bodyGraphicData.drawSize;
                Vector2 sizePaws = this.Pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                factor = sizePaws.x / maxSize.x;
            }

            BodyAnimDef body = this.CompAnimator.BodyAnim;
            if (body == null)
            {
                return false;
            }

            Rot4 rot = this.BodyFacing;
            WalkCycleDef cycle = this.CompAnimator.WalkCycle;
            
            Vector3 cycleOffset = Vector3.zero;
            float angleOffset = 0f;
            float jointOffset = 0f;
            
            JointLister jointPositions;

            if (limbType.EndsWith("Foot"))
            {
                jointPositions = this.GetJointPositions(JointType.Hip, body.hipOffsets[rot.AsInt], body.hipOffsets[Rot4.North.AsInt].x);
                if (cycle != null)
                {
                    this.DoWalkCycleOffsets(ref cycleOffset, ref cycleOffset, ref angleOffset, ref angleOffset, ref jointOffset, cycle.FootPositionX, cycle.FootPositionZ, cycle.FootAngle, factor);
                }
            }
            else
            {
                jointPositions = this.GetJointPositions(JointType.Shoulder, body.shoulderOffsets[rot.AsInt], body.shoulderOffsets[Rot4.North.AsInt].x);
                JointLister groundPos = this.GetJointPositions(JointType.Hip, body.hipOffsets[rot.AsInt], body.hipOffsets[Rot4.North.AsInt].x);
                jointPositions.LeftJoint.z = groundPos.LeftJoint.z;
                jointPositions.RightJoint.z = groundPos.RightJoint.z;

                if (cycle != null)
                {
                    jointOffset = cycle.ShoulderOffsetHorizontalX.Evaluate(this.CompAnimator.MovedPercent);
                    this.DoWalkCycleOffsets(ref cycleOffset, ref cycleOffset, ref angleOffset, ref angleOffset, ref jointOffset, cycle.FrontPawPositionX, cycle.FrontPawPositionZ, cycle.FrontPawAngle, factor);
                }
            }

            Vector3 joint;
            if (limbType.StartsWith("Left"))
            {
                joint = jointPositions.LeftJoint;
            }
            else
            {
                joint = jointPositions.RightJoint;
            }
            
            Vector3 ground = rootLoc + (bodyQuat * new Vector3(0, 0, OffsetGroundZ)) * factor;
            ground.y += this.BodyFacing == Rot4.South ? -Offsets.YOffset_HandsFeetOver : 0;
            finalWorldPosition = ground + (joint + cycleOffset) * factor;
            finalRotation = bodyQuat * Quaternion.AngleAxis(angleOffset, Vector3.up);

            return true;
        }
    }
}