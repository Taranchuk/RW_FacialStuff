namespace PawnPlus
{
    using System.Collections.Generic;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnBodyDrawer : BasicDrawer
    {
        #region Protected Fields

        protected Mesh HandMesh = MeshPool.plane10;


        #endregion Protected Fields

        #region Protected Constructors

        #endregion Protected Constructors

        #region Public Properties


        #endregion Public Properties

        #region Public Methods

        public virtual void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
        }

        public virtual void DrawApparel(Quaternion quat, Vector3 vector, bool renderBody, bool portrait)
        {
        }

        public virtual bool CarryStuff()
        {
            return false;
        }

        public virtual void DrawBody(PawnWoundDrawer woundDrawer, Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType, bool renderBody, bool portrait)
        {
        }

        public virtual void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
        }

        public virtual void DrawAlienBodyAddons(bool portrait, Vector3 rootLoc, Quaternion quat, bool renderBody,
                                                Rot4 rotation, bool invisible)
        {
        }

        public virtual bool GetLimbWorldTransform(
            string limbType,
            Vector3 rootLoc,
            Quaternion bodyQuat,
            out Vector3 finalWorldPosition,
            out Quaternion finalRotation)
        {
            finalWorldPosition = Vector3.zero;
            finalRotation = Quaternion.identity;
            return false;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Tick(Rot4 bodyFacing)
        {
            this.BodyFacing = bodyFacing;
        }

        #endregion Public Methods
    }
}
