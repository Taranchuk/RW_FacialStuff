using System;
using System.Collections.Generic;
using System.Linq;
using PawnPlus.AnimatorWindows;
using PawnPlus.Defs;
using PawnPlus.Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnPlus
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
    [HotSwappable]
    public class PawnRenderNodeWorker_PawnPlusLimb : PawnRenderNodeWorker
    {
        private Quaternion? _cachedRotation;

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (parms.pawn.IsPsychologicallyInvisible())
            {
                return false;
            }
            if (parms.pawn.GetCompAnim(out var compAnim))
            {
                if (compAnim.Deactivated) return false;
            }
            if (!parms.pawn.GetCompAnim(out CompBodyAnimator _))
            {
                return false;
            }
            if (parms.flags.FlagSet(PawnRenderFlags.Portrait) && !HarmonyPatchesFS.AnimatorIsOpen() && !parms.pawn.IsChild())
            {
                return false;
            }
            return true;
        }

        public override Material GetMaterial(PawnRenderNode node, PawnDrawParms parms)
        {
            var limbNode = (PawnRenderNode_PawnPlusLimb)node;
            var compAnim = parms.pawn.GetCompAnim();
            if (compAnim?.PawnBodyGraphic == null) return base.GetMaterial(node, parms);

            Rot4 rot = parms.facing;
            DamageFlasher flasher = parms.pawn.Drawer.renderer.flasher;
            
            if (MainTabWindow_BaseAnimator.Colored)
            {
                switch (limbNode.GetLimbType())
                {
                    case "LeftHand":
                        return compAnim.PawnBodyGraphic?.HandGraphicLeftCol?.MatAt(rot);
                    case "RightHand":
                        return compAnim.PawnBodyGraphic?.HandGraphicRightCol?.MatAt(rot);
                    case "LeftFoot":
                        return compAnim.PawnBodyGraphic?.FootGraphicLeftCol?.MatAt(rot);
                    case "RightFoot":
                        return compAnim.PawnBodyGraphic?.FootGraphicRightCol?.MatAt(rot);
                }
            }
            
            switch (limbNode.GetLimbType())
            {
                case "LeftHand":
                    return rot == Rot4.East
                        ? flasher.GetDamagedMat(compAnim.PawnBodyGraphic.HandGraphicLeftShadow?.MatAt(rot))
                        : flasher.GetDamagedMat(compAnim.PawnBodyGraphic.HandGraphicLeft?.MatAt(rot));
                case "RightHand":
                    return rot == Rot4.West
                       ? flasher.GetDamagedMat(compAnim.PawnBodyGraphic.HandGraphicRightShadow?.MatAt(rot))
                       : flasher.GetDamagedMat(compAnim.PawnBodyGraphic.HandGraphicRight?.MatAt(rot));
                case "LeftFoot":
                    return rot == Rot4.East
                       ? flasher.GetDamagedMat(compAnim.PawnBodyGraphic.FootGraphicLeftShadow?.MatAt(rot))
                       : flasher.GetDamagedMat(compAnim.PawnBodyGraphic.FootGraphicLeft?.MatAt(rot));
                case "RightFoot":
                    return rot == Rot4.West
                        ? flasher.GetDamagedMat(compAnim.PawnBodyGraphic.FootGraphicRightShadow?.MatAt(rot))
                        : flasher.GetDamagedMat(compAnim.PawnBodyGraphic.FootGraphicRight?.MatAt(rot));
            }
            return base.GetMaterial(node, parms);
        }
        
        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            pivot = Vector3.zero;
            this._cachedRotation = null;
            
            PawnBodyDrawer drawer = this.GetDrawer(parms);
            if (drawer == null)
            {
                return Vector3.zero;
            }

            var limbNode = (PawnRenderNode_PawnPlusLimb)node;
            string limbType = limbNode.GetLimbType();

            Vector3 rootLoc = parms.matrix.GetColumn(3);
            Quaternion bodyQuat = parms.matrix.rotation;

            if (drawer.GetLimbWorldTransform(limbType, rootLoc, bodyQuat, out Vector3 finalWorldPosition, out Quaternion finalRotation))
            {
                this._cachedRotation = finalRotation;
                Vector3 offset = finalWorldPosition - rootLoc;
                offset.y = AltitudeFor(node, parms);
                return offset;
            }

            return Vector3.zero;
        }

        public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
        {
            return this._cachedRotation ?? Quaternion.identity;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            float bodySize = parms.pawn.GetBodysizeScaling();
            return Vector3.one * 0.75f * bodySize;
        }
        
        private PawnBodyDrawer GetDrawer(PawnDrawParms parms)
        {
            var compAnim = parms.pawn.GetCompAnim();
            if (compAnim.PawnBodyDrawers == null)
            {
                compAnim.InitializePawnDrawer();
            }
            return compAnim?.PawnBodyDrawers.FirstOrDefault();
        }
    }
}