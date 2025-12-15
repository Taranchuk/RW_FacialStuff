using RimWorld;
using UnityEngine;
using Verse;

namespace PawnPlus
{
    public class PawnRenderNode_PawnPlusLimb : PawnRenderNode
    {
        private readonly string limbType;

        public PawnRenderNode_PawnPlusLimb(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, string limbType) : base(pawn, props, tree)
        {
            this.limbType = limbType;
        }

        public string GetLimbType()
        {
            return limbType;
        }

        public override Mesh GetMesh(PawnDrawParms parms)
        {
            return MeshPool.plane10;
        }
    }
}