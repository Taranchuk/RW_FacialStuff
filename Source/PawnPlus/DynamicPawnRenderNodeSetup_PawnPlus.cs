using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using PawnPlus.Defs;

namespace PawnPlus
{
    public class DynamicPawnRenderNodeSetup_PawnPlus : DynamicPawnRenderNodeSetup
    {
        public override bool HumanlikeOnly => true;

        public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
        {
            CompBodyAnimator compAnim = pawn.GetCompAnim();
            if (compAnim != null && !compAnim.Deactivated)
            {
                if (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.Body, out PawnRenderNode bodyNode))
                {
                    float limbBaseLayer = bodyNode.Props.baseLayer + 2.0f;
                    if (compAnim.Props.bipedWithHands && Controller.settings.UseHands)
                    {
                        PawnRenderNodeProperties leftHandProps = new PawnRenderNodeProperties
                        {
                            debugLabel = "LeftHand",
                            workerClass = typeof(PawnRenderNodeWorker_PawnPlusLimb),
                            nodeClass = typeof(PawnRenderNode_PawnPlusLimb),
                            useGraphic = true,
                            rotDrawMode = RotDrawMode.Fresh | RotDrawMode.Rotting | RotDrawMode.Dessicated,
                            pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                            tagDef = null,
                            parentTagDef = PawnRenderNodeTagDefOf.Body,
                            baseLayer = limbBaseLayer
                        };

                        leftHandProps.texPath = "Things/Pawn/Humanlike/Hands/Human_Hand";
                        leftHandProps.colorType = PawnRenderNodeProperties.AttachmentColorType.Skin;
                        
                        PawnRenderNode leftHandNode = new PawnRenderNode_PawnPlusLimb(pawn, leftHandProps, tree, "LeftHand");
                        yield return (node: leftHandNode, parent: bodyNode);
                        PawnRenderNodeProperties rightHandProps = new PawnRenderNodeProperties
                        {
                            debugLabel = "RightHand",
                            workerClass = typeof(PawnRenderNodeWorker_PawnPlusLimb),
                            nodeClass = typeof(PawnRenderNode_PawnPlusLimb),
                            useGraphic = true,
                            rotDrawMode = RotDrawMode.Fresh | RotDrawMode.Rotting | RotDrawMode.Dessicated,
                            pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                            tagDef = null,
                            parentTagDef = PawnRenderNodeTagDefOf.Body,
                            baseLayer = limbBaseLayer
                        };

                        rightHandProps.texPath = "Things/Pawn/Humanlike/Hands/Human_Hand";
                        rightHandProps.colorType = PawnRenderNodeProperties.AttachmentColorType.Skin;
                        
                        PawnRenderNode rightHandNode = new PawnRenderNode_PawnPlusLimb(pawn, rightHandProps, tree, "RightHand");
                        yield return (node: rightHandNode, parent: bodyNode);
                    }
                    if (Controller.settings.UseFeet)
                    {
                        PawnRenderNodeProperties leftFootProps = new PawnRenderNodeProperties
                        {
                            debugLabel = "LeftFoot",
                            workerClass = typeof(PawnRenderNodeWorker_PawnPlusLimb),
                            nodeClass = typeof(PawnRenderNode_PawnPlusLimb),
                            useGraphic = true,
                            rotDrawMode = RotDrawMode.Fresh | RotDrawMode.Rotting | RotDrawMode.Dessicated,
                            pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                            tagDef = null,
                            parentTagDef = PawnRenderNodeTagDefOf.Body,
                            baseLayer = limbBaseLayer
                        };

                        leftFootProps.texPath = "Things/Pawn/Humanlike/Feet/Human_Foot";
                        leftFootProps.colorType = PawnRenderNodeProperties.AttachmentColorType.Skin;
                        
                        PawnRenderNode leftFootNode = new PawnRenderNode_PawnPlusLimb(pawn, leftFootProps, tree, "LeftFoot");
                        yield return (node: leftFootNode, parent: bodyNode);
                        PawnRenderNodeProperties rightFootProps = new PawnRenderNodeProperties
                        {
                            debugLabel = "RightFoot",
                            workerClass = typeof(PawnRenderNodeWorker_PawnPlusLimb),
                            nodeClass = typeof(PawnRenderNode_PawnPlusLimb),
                            useGraphic = true,
                            rotDrawMode = RotDrawMode.Fresh | RotDrawMode.Rotting | RotDrawMode.Dessicated,
                            pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                            tagDef = null,
                            parentTagDef = PawnRenderNodeTagDefOf.Body,
                            baseLayer = limbBaseLayer
                        };

                        rightFootProps.texPath = "Things/Pawn/Humanlike/Feet/Human_Foot";
                        rightFootProps.colorType = PawnRenderNodeProperties.AttachmentColorType.Skin;
                        
                        PawnRenderNode rightFootNode = new PawnRenderNode_PawnPlusLimb(pawn, rightFootProps, tree, "RightFoot");
                        yield return (node: rightFootNode, parent: bodyNode);
                    }
                }
            }
        }
    }
}