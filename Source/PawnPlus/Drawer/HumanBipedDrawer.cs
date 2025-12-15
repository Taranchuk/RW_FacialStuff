namespace PawnPlus
{
    using System.Collections.Generic;
    using System.Linq;

    using PawnPlus.AnimatorWindows;
    using PawnPlus.Harmony;
    using PawnPlus.Tweener;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class HumanBipedDrawer : PawnBodyDrawer
    {
        #region Protected Fields

        protected const float OffsetGroundZ = -0.575f;

        protected DamageFlasher Flasher;

        #endregion Protected Fields

        #region Private Fields

        #endregion Private Fields

        #region Public Properties

        public Material LeftHandMat =>
        Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic?.HandGraphicLeft?.MatSingle);

        public Material LeftHandShadowMat => Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic
                                                                           ?.HandGraphicLeftShadow?.MatSingle);

        public Material RightHandMat =>
        Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic?.HandGraphicRight?.MatSingle);

        public Material RightHandShadowMat => Flasher.GetDamagedMat(CompAnimator.PawnBodyGraphic
                                                                            ?.HandGraphicRightShadow?.MatSingle);

        #endregion Public Properties

        #region Public Methods

        public override void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if(CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = CompAnimator.WalkCycle;
                if(walkCycle != null)
                {
                    float bam = CompAnimator.BodyOffsetZ;

                    rootLoc.z += bam;
                    quat = QuatBody(quat, CompAnimator.MovedPercent);
                }
            }
            if(CompAnimator.BodyAnim != null)
            {
                float legModifier = CompAnimator.BodyAnim.extraLegLength;
                float posModB = legModifier * 0.75f;
                float posModF = -legModifier * 0.25f;
                Vector3 vector3 = new Vector3(0, 0, posModB);
                Vector3 vector4 = new Vector3(0, 0, posModF);
                if(!CompAnimator.IsMoving)
                {
                    vector3 = quat * vector3;
                    vector4 = quat * vector4;
                }

                if(!CompAnimator.IsRider)
                {
                    rootLoc += vector3;
                }
                else
                {
                    footPos -= vector3;
                }

                footPos += vector4;

            }

            base.ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
        }

        public void ApplyEquipmentWobble(ref Vector3 rootLoc)
        {
            if(CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = CompAnimator.WalkCycle;
                if(walkCycle != null)
                {
                    float bam = CompAnimator.BodyOffsetZ;
                    rootLoc.z += bam;
                }
            }
            if(CompAnimator.BodyAnim != null)
            {
                float legModifier = CompAnimator.BodyAnim.extraLegLength;
                float posModB = legModifier * 0.85f;
                Vector3 vector3 = new Vector3(0, 0, posModB);
                rootLoc += vector3;
            }
        }

        public override bool CarryStuff()
        {
            Pawn pawn = Pawn;

            Thing carriedThing = pawn.carryTracker?.CarriedThing;
            if(carriedThing != null)
            {
                return true;
            }

            return base.CarryStuff();
        }

        public void DoAttackAnimationHandOffsets(ref List<float> weaponAngle, ref Vector3 weaponPosition, bool flipped)
        {
            Pawn pawn = Pawn;
            if(pawn.story != null && ((pawn.story.DisabledWorkTagsBackstoryAndTraits & WorkTags.Violent) != 0))
            {
                return;
            }

            if(pawn.health?.capacities != null)
            {
                if(!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    if(pawn.RaceProps != null && pawn.RaceProps.ToolUser)
                    {
                        return;
                    }
                }
            }
            int totalSwingAngle = 0;
            Vector3 currentOffset = CompAnimator.Jitterer.CurrentOffset;

            float jitterMax = CompAnimator.JitterMax;
            float magnitude = currentOffset.magnitude;
            float animationPhasePercent = magnitude / jitterMax;
            weaponPosition += currentOffset;

            float angle = animationPhasePercent * totalSwingAngle;
            weaponAngle[0] += (flipped ? -1f : 1f) * angle;
            weaponAngle[1] += (flipped ? -1f : 1f) * angle;
        }
        

        public override void Initialize()
        {
            Flasher = Pawn.Drawer.renderer.flasher;

            base.Initialize();
        }

        public Quaternion QuatBody(Quaternion quat, float movedPercent)
        {
            WalkCycleDef walkCycle = CompAnimator.WalkCycle;
            if(walkCycle != null)
            {
                float angle;
                if (BodyFacing.IsHorizontal)
                {
                    angle = (BodyFacing == Rot4.West ? -1 : 1)
                          * walkCycle.BodyAngle.Evaluate(movedPercent);
                }
                else
                {
                    angle = (BodyFacing == Rot4.South ? -1 : 1)
                          * walkCycle.BodyAngleVertical.Evaluate(movedPercent);
                }

                quat *= Quaternion.AngleAxis(angle, Vector3.up);
                CompAnimator.BodyAngle = angle;
            }

            return quat;
        }

        public Job lastJob;

        public virtual void SelectWalkcycle(bool pawnInEditor)
        {
            if(pawnInEditor)
            {
                CompAnimator.SetWalkCycle(Find.WindowStack.WindowOfType<MainTabWindow_WalkAnimator>().EditorWalkcycle);
                return;
            }


            if(Pawn.CurJob != null && Pawn.CurJob != lastJob)
            {
                BodyAnimDef animDef = CompAnimator.BodyAnim;

                Dictionary<LocomotionUrgency, WalkCycleDef> cycles = animDef?.walkCycles;

                if(cycles != null && cycles.Count > 0)
                {
                    if(cycles.TryGetValue(Pawn.CurJob.locomotionUrgency, out WalkCycleDef cycle))
                    {
                        if(cycle != null)
                        {
                            CompAnimator.SetWalkCycle(cycle);
                        }
                    }
                    else
                    {
                        CompAnimator.SetWalkCycle(animDef.walkCycles.FirstOrDefault().Value);
                    }
                }

                lastJob = Pawn.CurJob;
            }
        }

        public virtual void SelectPosecycle()
        {
        }

        public override void Tick(Rot4 bodyFacing)
        {
            base.Tick(bodyFacing);
            
            bool pawnInEditor = HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn == Pawn;
            if(!Find.TickManager.Paused || pawnInEditor)
            {
                SelectWalkcycle(pawnInEditor);
                SelectPosecycle();
                CompAnimator.FirstHandPosition = Vector3.zero;
                CompAnimator.SecondHandPosition = Vector3.zero;
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected void DoWalkCycleOffsets(ref Vector3 rightFoot,
                                          ref Vector3 leftFoot,
                                          ref float footAngleRight,
                                          ref float footAngleLeft,
                                          ref float offsetJoint,
                                          SimpleCurve offsetX,
                                          SimpleCurve offsetZ,
                                          SimpleCurve angle, float factor = 1f)
        {
            rightFoot = Vector3.zero;
            leftFoot = Vector3.zero;
            footAngleRight = 0;
            footAngleLeft = 0;

            if (!CompAnimator.IsMoving)
            {
                return;
            }

            float percent = CompAnimator.MovedPercent;
            float flot = percent;
            if (flot <= 0.5f)
            {
                flot += 0.5f;
            }
            else
            {
                flot -= 0.5f;
            }

            Rot4 rot = BodyFacing;
            if (rot.IsHorizontal)
            {
                rightFoot.x = offsetX.Evaluate(percent);
                leftFoot.x = offsetX.Evaluate(flot);

                footAngleRight = angle.Evaluate(percent);
                footAngleLeft = angle.Evaluate(flot);
                rightFoot.z = offsetZ.Evaluate(percent);
                leftFoot.z = offsetZ.Evaluate(flot);

                rightFoot.x += offsetJoint;
                leftFoot.x += offsetJoint;

                if (rot == Rot4.West)
                {
                    rightFoot.x *= -1f;
                    leftFoot.x *= -1f;
                    footAngleLeft *= -1f;
                    footAngleRight *= -1f;
                    offsetJoint *= -1;
                }
            }
            else
            {
                rightFoot.z = offsetZ.Evaluate(percent);
                leftFoot.z = offsetZ.Evaluate(flot);
                offsetJoint = 0;
            }
            if (factor < 1f)
            {
                SimpleCurve curve = new SimpleCurve { new CurvePoint(0f, 0.5f), new CurvePoint(1f, 1f) };
                float mod = curve.Evaluate(factor);
                rightFoot.x *= mod;
                rightFoot.z *= mod;
                leftFoot.x *= mod;
                leftFoot.z *= mod;
            }
        }

        protected void GetBipedMesh(out Mesh meshRight, out Mesh meshLeft)
        {
            Rot4 rot = BodyFacing;

            switch (rot.AsInt)
            {
                default:
                    meshRight = MeshPool.plane10;
                    meshLeft = MeshPool.plane10Flip;
                    break;

                case 1:
                    meshRight = MeshPool.plane10;
                    meshLeft = MeshPool.plane10;
                    break;

                case 3:
                    meshRight = MeshPool.plane10Flip;
                    meshLeft = MeshPool.plane10Flip;
                    break;
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private void DoWalkCycleOffsets(
            float armLength,
            ref Vector3 rightHand,
            ref Vector3 leftHand,
            ref List<float> shoulderAngle,
            ref List<float> handSwingAngle,
            ref JointLister shoulderPos,
            bool carrying,
            SimpleCurve cycleHandsSwingAngle,
            float offsetJoint)
        {
            if(carrying)
            {
                return;
            }

            Rot4 rot = BodyFacing;
            float x = 0;
            float x2 = -x;
            float y = Offsets.YOffset_Behind;
            float y2 = y;
            float z;
            float z2;
            z = z2 = -armLength;

            if(rot.IsHorizontal)
            {
                x = x2 = 0f;
                if(rot == Rot4.East)
                {
                    y2 = -0.5f;
                }
                else
                {
                    y = -0.05f;
                }
            }
            else if(rot == Rot4.North)
            {
                y = y2 = -0.02f;
                x *= -1;
                x2 *= -1;
            }
            if(CompAnimator.IsMoving)
            {
                WalkCycleDef walkCycle = CompAnimator.WalkCycle;
                float percent = CompAnimator.MovedPercent;
                if(rot.IsHorizontal)
                {
                    float lookie = rot == Rot4.West ? -1f : 1f;
                    float f = lookie * offsetJoint;

                    shoulderAngle[0] = shoulderAngle[1] = lookie * walkCycle?.shoulderAngle ?? 0f;

                    shoulderPos.RightJoint.x += f;
                    shoulderPos.LeftJoint.x += f;

                    handSwingAngle[0] = handSwingAngle[1] =
                                        (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(percent);
                }
                else
                {
                    z += cycleHandsSwingAngle.Evaluate(percent) / 500;
                    z2 -= cycleHandsSwingAngle.Evaluate(percent) / 500;

                    z += walkCycle?.shoulderAngle / 800 ?? 0f;
                    z2 += walkCycle?.shoulderAngle / 800 ?? 0f;
                }
            }

            if(MainTabWindow_BaseAnimator.Panic || Pawn.Fleeing() || Pawn.IsBurning())
            {
                float offset = 1f + armLength;
                x *= offset;
                z *= offset;
                x2 *= offset;
                z2 *= offset;
                handSwingAngle[0] += 180f;
                handSwingAngle[1] += 180f;
                shoulderAngle[0] = shoulderAngle[1] = 0f;
            }

            rightHand = new Vector3(x, y, z);
            leftHand = new Vector3(x2, y2, z2);
        }

        private void DoPoseCycleOffsets(
            ref Vector3 rightHand,
            ref List<float> shoulderAngle,
            ref List<float> handSwingAngle, PoseCycleDef pose)
        {
            if(!HarmonyPatchesFS.AnimatorIsOpen())
            {
                return;
            }

            SimpleCurve cycleHandsSwingAngle = pose.HandsSwingAngle;
            SimpleCurve rHandX = pose.HandPositionX;
            SimpleCurve rHandZ = pose.HandPositionZ;

            Rot4 rot = BodyFacing;
            float x = 0;
            float z;

            float percent = CompAnimator.MovedPercent;
            PoseCycleDef poseCycle = CompAnimator.PoseCycle;
            float lookie = rot == Rot4.West ? -1f : 1f;

            shoulderAngle[1] = lookie * poseCycle?.shoulderAngle ?? 0f;

            handSwingAngle[1] = (rot == Rot4.West ? -1 : 1) * cycleHandsSwingAngle.Evaluate(percent);

            x = rHandX.Evaluate(percent) * lookie;
            z = rHandZ.Evaluate(percent);

            rightHand += new Vector3(x, 0, z);
        }

        public override bool GetLimbWorldTransform(
            string limbType,
            Vector3 rootLoc,
            Quaternion bodyQuat,
            out Vector3 finalWorldPosition,
            out Quaternion finalRotation)
        {
            finalWorldPosition = Vector3.zero;
            finalRotation = Quaternion.identity;

            if (this.ShouldBeIgnored())
            {
                return false;
            }

            float factor = this.Pawn.GetBodysizeScaling();
            
            if (!this.CalculateRawLimbTransform(limbType, rootLoc, bodyQuat, factor, out Vector3 calculatedPosition, out Quaternion calculatedRotation, out bool noTween))
            {
                return false;
            }

            if (limbType.Contains("Hand"))
            {
                TweenThing tweenThing = limbType == "LeftHand" ? TweenThing.HandLeft : TweenThing.HandRight;
                
                if(!HarmonyPatchesFS.AnimatorIsOpen() &&
                   Find.TickManager.TicksGame == this.CompAnimator.LastPosUpdate[(int) tweenThing] ||
                   HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn != this.Pawn)
                {
                    finalWorldPosition = this.CompAnimator.LastPosition[(int) tweenThing];
                    finalRotation = calculatedRotation;
                }
                else
                {
                    Pawn_PathFollower pawnPathFollower = this.Pawn.pather;
                    if(pawnPathFollower != null && pawnPathFollower.MovedRecently(5))
                    {
                        noTween = true;
                    }

                    this.CompAnimator.LastPosUpdate[(int) tweenThing] = Find.TickManager.TicksGame;

                    Vector3Tween tween = this.CompAnimator.Vector3Tweens[(int) tweenThing];

                    switch(tween.State)
                    {
                        case TweenState.Running:
                            if(noTween || this.CompAnimator.IsMoving)
                            {
                                tween.Stop(StopBehavior.ForceComplete);
                            }
                            finalWorldPosition = tween.CurrentValue;
                            break;

                        case TweenState.Paused:
                            finalWorldPosition = calculatedPosition;
                            break;

                        case TweenState.Stopped:
                            if(noTween || (this.CompAnimator.IsMoving))
                            {
                                finalWorldPosition = calculatedPosition;
                                break;
                            }

                            ScaleFunc scaleFunc = ScaleFuncs.SineEaseOut;

                            Vector3 start = this.CompAnimator.LastPosition[(int) tweenThing];
                            float distance = Vector3.Distance(start, calculatedPosition);
                            float duration = Mathf.Abs(distance * 50f);
                            if (start != Vector3.zero && duration > 12f)
                            {
                                start.y = calculatedPosition.y;
                                tween.Start(start, calculatedPosition, duration, scaleFunc);
                                finalWorldPosition = start;
                            }
                            else
                            {
                                finalWorldPosition = calculatedPosition;
                            }
                            break;
                        
                        default:
                            finalWorldPosition = calculatedPosition;
                            break;
                    }
                    this.CompAnimator.LastPosition[(int) tweenThing] = finalWorldPosition;
                    finalRotation = calculatedRotation;
                }
            }
            else
            {
                finalWorldPosition = calculatedPosition;
                finalRotation = calculatedRotation;
            }

            return finalWorldPosition != Vector3.zero;
        }

        private bool CalculateRawLimbTransform(
            string limbType,
            Vector3 rootLoc,
            Quaternion bodyQuat,
            float factor,
            out Vector3 finalWorldPosition,
            out Quaternion finalRotation,
            out bool noTween)
        {
            finalWorldPosition = Vector3.zero;
            finalRotation = Quaternion.identity;
            noTween = false;
    
            Rot4 rot = this.BodyFacing;
            BodyAnimDef body = this.CompAnimator.BodyAnim;
            if (body == null)
            {
                return false;
            }

            if (limbType.Contains("Foot"))
            {
                Job curJob = this.Pawn.CurJob;
                if (curJob != null)
                {
                    if (curJob.def == JobDefOf.Ingest && !this.Pawn.Rotation.IsHorizontal)
                    {
                        if (curJob.targetB.IsValid)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Rot4 rotty = new Rot4(i);
                                IntVec3 intVec = this.Pawn.Position + rotty.FacingCell;
                                if (intVec == curJob.targetB)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                
                JointLister groundPos = this.GetJointPositions(
                                                               JointType.Hip,
                                                               body.hipOffsets[rot.AsInt],
                                                               body.hipOffsets[Rot4.North.AsInt].x);

                Vector3 rightFootCycle = Vector3.zero;
                Vector3 leftFootCycle = Vector3.zero;
                float footAngleRight = 0;
                float footAngleLeft = 0;
                float offsetJoint = 0;
                WalkCycleDef cycle = this.CompAnimator.WalkCycle;
                if (cycle != null)
                {
                    offsetJoint = cycle.HipOffsetHorizontalX.Evaluate(this.CompAnimator.MovedPercent);

                    this.DoWalkCycleOffsets(
                                        ref rightFootCycle,
                                        ref leftFootCycle,
                                        ref footAngleRight,
                                        ref footAngleLeft,
                                        ref offsetJoint,
                                        cycle.FootPositionX,
                                        cycle.FootPositionZ,
                                        cycle.FootAngle, factor);
                }
                
                Vector3 ground = rootLoc + bodyQuat * new Vector3(0, 0, OffsetGroundZ) * factor;

                if (limbType == "LeftFoot" && this.CompAnimator.BodyStat.FootLeft != PartStatus.Missing)
                {
                    Vector3 joint = bodyQuat * groundPos.LeftJoint;
                    Vector3 cycleVec = bodyQuat * leftFootCycle;
                    finalWorldPosition = ground + (joint + cycleVec) * factor;
                    finalWorldPosition.y = rootLoc.y;
                    finalRotation = bodyQuat * Quaternion.AngleAxis(footAngleLeft, Vector3.up);
                }
                else if (limbType == "RightFoot" && this.CompAnimator.BodyStat.FootRight != PartStatus.Missing)
                {
                    Vector3 joint = bodyQuat * groundPos.RightJoint;
                    Vector3 cycleVec = bodyQuat * rightFootCycle;
                    finalWorldPosition = ground + (joint + cycleVec) * factor;
                    finalWorldPosition.y = rootLoc.y;
                    finalRotation = bodyQuat * Quaternion.AngleAxis(footAngleRight, Vector3.up);
                }
            }
            else if (limbType.Contains("Hand"))
            {
                if (!this.CompAnimator.Props.bipedWithHands)
                {
                    return false;
                }
                
                bool carrying = this.CarryStuff();
                
                JointLister shoulderPos = this.GetJointPositions(
                                                                JointType.Shoulder,
                                                                body.shoulderOffsets[rot.AsInt],
                                                                body.shoulderOffsets[Rot4.North.AsInt].x,
                                                                carrying, this.Pawn.ShowWeaponOpenly());

                List<float> handSwingAngle = new List<float> { 0f, 0f };
                List<float> shoulderAngle = new List<float> { 0f, 0f };
                Vector3 rightHand = Vector3.zero;
                Vector3 leftHand = Vector3.zero;
                WalkCycleDef walkCycle = this.CompAnimator.WalkCycle;
                PoseCycleDef poseCycle = this.CompAnimator.PoseCycle;

                if (walkCycle != null && !carrying)
                {
                    float offsetJoint = walkCycle.ShoulderOffsetHorizontalX.Evaluate(this.CompAnimator.MovedPercent);
                    
                    this.DoWalkCycleOffsets(
                                        body.armLength * (factor < 1f ? factor * 0.75f : 1f),
                                        ref rightHand,
                                        ref leftHand,
                                        ref shoulderAngle,
                                        ref handSwingAngle,
                                        ref shoulderPos,
                                        carrying,
                                        walkCycle.HandsSwingAngle,
                                        offsetJoint);
                }

                if (poseCycle != null)
                {
                    this.DoPoseCycleOffsets(
                                        ref rightHand,
                                        ref shoulderAngle,
                                        ref handSwingAngle, poseCycle);
                }

                this.DoAttackAnimationHandOffsets(ref handSwingAngle, ref rightHand, false);

                if (limbType == "LeftHand" && this.CompAnimator.BodyStat.HandLeft != PartStatus.Missing)
                {
                    if (!this.CompAnimator.IsMoving && this.CompAnimator.HasLeftHandPosition)
                    {
                        finalWorldPosition = this.CompAnimator.SecondHandPosition;
                        finalRotation = this.CompAnimator.WeaponQuat;
                        noTween = true;
                    }
                    else
                    {
                        Vector3 joint = bodyQuat * shoulderPos.LeftJoint;
                        Vector3 handVec = bodyQuat * leftHand.RotatedBy(-handSwingAngle[0] - shoulderAngle[0]);
                        finalWorldPosition = rootLoc + (joint + handVec) * factor;
                        finalRotation = bodyQuat * Quaternion.AngleAxis(-handSwingAngle[0], Vector3.up);
                    }
                }
                else if (limbType == "RightHand" && this.CompAnimator.BodyStat.HandRight != PartStatus.Missing)
                {
                    if (this.CompAnimator.FirstHandPosition != Vector3.zero)
                    {
                        finalWorldPosition = this.CompAnimator.FirstHandPosition;
                        finalRotation = this.CompAnimator.WeaponQuat;
                        noTween = true;
                    }
                    else
                    {
                        Vector3 joint = bodyQuat * shoulderPos.RightJoint;
                        Vector3 handVec = bodyQuat * rightHand.RotatedBy(handSwingAngle[1] - shoulderAngle[1]);
                        finalWorldPosition = rootLoc + (joint + handVec) * factor;
                        finalRotation = bodyQuat * Quaternion.AngleAxis(handSwingAngle[1], Vector3.up);
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool ShouldBeIgnored()
        {
            return Pawn.Dead || !Pawn.Spawned || Pawn.InContainerEnclosed;
        }

        #endregion Private Methods
    }
}