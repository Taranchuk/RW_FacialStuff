namespace PawnPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using JetBrains.Annotations;

    using PawnPlus.AnimatorWindows;
    using PawnPlus.Graphics;
    using PawnPlus.Harmony;
    using PawnPlus.Tweener;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class CompBodyAnimator : ThingComp
    {
        #region Public Fields

        public bool Deactivated;
        public bool IgnoreRenderer;

        [CanBeNull] public BodyAnimDef BodyAnim;

        public BodyPartStats BodyStat;

        public float JitterMax = 0.35f;
        public readonly Vector3Tween[] Vector3Tweens = new Vector3Tween[(int)TweenThing.Max];
        [CanBeNull] public PawnBodyGraphic PawnBodyGraphic;

        [CanBeNull] public PoseCycleDef PoseCycle;

        public Vector3 FirstHandPosition;
        public Vector3 SecondHandPosition;

        [CanBeNull] public WalkCycleDef WalkCycle => _walkCycle;

        public Quaternion WeaponQuat = new Quaternion();

        #endregion Public Fields

        #region Private Fields

        [System.Diagnostics.CodeAnalysis.NotNull] private readonly List<Material> _cachedNakedMatsBodyBase = new List<Material>();

        private readonly List<Material> _cachedSkinMatsBodyBase = new List<Material>();

        private int _cachedNakedMatsBodyBaseHash = -1;
        private int _cachedSkinMatsBodyBaseHash = -1;
        private int _lastRoomCheck;

        private bool _initialized;

        [CanBeNull] private Room _theRoom;

        #endregion Private Fields

        #region Public Properties
        
        public bool InRoom
        {
            get
            {
                if(TheRoom != null && !TheRoom.UsesOutdoorTemperature)
                {
                    return !Pawn.Drafted || !Controller.settings.IgnoreWhileDrafted;
                }

                return false;
            }
        }

        public JitterHandler Jitterer => Pawn.Drawer.jitterer;

        [System.Diagnostics.CodeAnalysis.NotNull]
        public Pawn Pawn => parent as Pawn;

        public List<PawnBodyDrawer> PawnBodyDrawers { get; private set; }

        public CompProperties_BodyAnimator Props => (CompProperties_BodyAnimator)props;

        public bool HideHat => InRoom && Controller.settings.HideHatWhileRoofed && (Pawn.IsColonistPlayerControlled && Pawn.Faction.IsPlayer && !Pawn.HasExtraHomeFaction());

        #endregion Public Properties

        #region Private Properties

        [CanBeNull]
        private Room TheRoom
        {
            get
            {
                if (Pawn.Dead)
                {
                    return null;
                }

                if (Find.TickManager.TicksGame < _lastRoomCheck + 60f)
                {
                    return _theRoom;
                }

                _theRoom = Pawn.GetRoom();
                _lastRoomCheck = Find.TickManager.TicksGame;

                return _theRoom;
            }
        }

        #endregion Private Properties

        #region Public Methods

        public void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if(PawnBodyDrawers == null)
            {
                return;
            }

            int i = 0;
            int count = PawnBodyDrawers.Count;
            while(i < count)
            {
                PawnBodyDrawers[i].ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
                i++;
            }
        }
        public void ClearCache()
        {
            _cachedSkinMatsBodyBaseHash = -1;
            _cachedNakedMatsBodyBaseHash = -1;
        }
        

        public void InitializePawnDrawer()
        {
            if(Props.bodyDrawers.Any())
            {
                PawnBodyDrawers = new List<PawnBodyDrawer>();
                for(int i = 0; i < Props.bodyDrawers.Count; i++)
                {
                    PawnBodyDrawer thingComp =
                    (PawnBodyDrawer)Activator.CreateInstance(Props.bodyDrawers[i].GetType());
                    thingComp.CompAnimator = this;
                    thingComp.Pawn = Pawn;
                    PawnBodyDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
            else
            {
                PawnBodyDrawers = new List<PawnBodyDrawer>();
                bool isQuaduped = Pawn.GetCompAnim().BodyAnim.quadruped;
                PawnBodyDrawer thingComp = isQuaduped
                                               ? (PawnBodyDrawer)Activator.CreateInstance(typeof(QuadrupedDrawer))
                                               : (PawnBodyDrawer)Activator.CreateInstance(typeof(HumanBipedDrawer));
                thingComp.CompAnimator = this;
                thingComp.Pawn = Pawn;
                PawnBodyDrawers.Add(thingComp);
                thingComp.Initialize();
            }
        }

        public List<Material> NakedMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if(num != _cachedNakedMatsBodyBaseHash)
            {
                _cachedNakedMatsBodyBase.Clear();
                _cachedNakedMatsBodyBaseHash = num;
                
                var bodyGraphic = Pawn.Drawer.renderer.BodyGraphic;
                if (bodyGraphic == null) return _cachedNakedMatsBodyBase;

                if(bodyCondition == RotDrawMode.Fresh)
                {
                    _cachedNakedMatsBodyBase.Add(bodyGraphic.MatAt(facing));
                }
                else if(bodyCondition == RotDrawMode.Rotting)
                {
                    _cachedNakedMatsBodyBase.Add(bodyGraphic.MatAt(facing));
                }
                else if(bodyCondition == RotDrawMode.Dessicated)
                {
                    _cachedNakedMatsBodyBase.Add(bodyGraphic.MatAt(facing));
                }

                Pawn.Drawer.renderer.renderTree.TraverseTree(node =>
                {
                    if (node is PawnRenderNode_Apparel apparelNode)
                    {
                        ApparelLayerDef lastLayer = apparelNode.apparel.def.apparel.LastLayer;

                        if (Pawn.Dead)
                        {
                            if (lastLayer != ApparelLayerDefOf.Shell && lastLayer != ApparelLayerDefOf.Overhead)
                            {
                                _cachedNakedMatsBodyBase.Add(apparelNode.GraphicFor(Pawn).MatAt(facing));
                            }
                        }
                    }
                });
            }

            return _cachedNakedMatsBodyBase;
        }
        
        public override void PostDraw()
        {
            base.PostDraw();

            if(Pawn.Map == null || !Pawn.Spawned || Pawn.Dead || Deactivated)
            {
                return;
            }
            
            Vector3Tween eqTween = Vector3Tweens[(int)HarmonyPatchesFS.equipment];
            FloatTween angleTween = AimAngleTween;
            Vector3Tween leftHand = Vector3Tweens[(int)TweenThing.HandLeft];
            Vector3Tween rightHand = Vector3Tweens[(int)TweenThing.HandRight];
            if(!Find.TickManager.Paused)
            {
                if(leftHand.State == TweenState.Running)
                {
                    leftHand.Update(1f * Find.TickManager.TickRateMultiplier);
                }

                if(rightHand.State == TweenState.Running)
                {
                    rightHand.Update(1f * Find.TickManager.TickRateMultiplier);
                }

                if(eqTween.State == TweenState.Running)
                {
                    eqTween.Update(1f * Find.TickManager.TickRateMultiplier);
                }

                if(angleTween.State == TweenState.Running)
                {
                    AimAngleTween.Update(3f * Find.TickManager.TickRateMultiplier);
                }

                CheckMovement();

                if(Pawn.IsChild())
                {
                    TickDrawers(Pawn.Rotation);
                }
            }

        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _lastRoomCheck, "lastRoomCheck");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            for (int i = 0; i < Vector3Tweens.Length; i++)
            {
                Vector3Tweens[i] = new Vector3Tween();
            }

            PawnBodyGraphic = new PawnBodyGraphic(this);

            string bodyType = "Undefined";

            if(Pawn.story?.bodyType != null)
            {
                bodyType = Pawn.story.bodyType.ToString();
            }

            string defaultName = "BodyAnimDef_" + Pawn.def.defName + "_" + bodyType;
            List<string> names = new List<string>
                                     {
                                         defaultName,
                                     };

            bool needsNewDef = true;
            foreach(string name in names)
            {
                BodyAnimDef dbDef = DefDatabase<BodyAnimDef>.GetNamedSilentFail(name);
                if(dbDef == null)
                {
                    continue;
                }

                BodyAnim = dbDef;
                needsNewDef = false;
                break;
            }

            if(needsNewDef)
            {
                BodyAnim = new BodyAnimDef { defName = defaultName, label = defaultName };
                DefDatabase<BodyAnimDef>.Add(BodyAnim);
            }
        }

        public void TickDrawers(Rot4 bodyFacing)
        {
            if(!_initialized)
            {
                InitializePawnDrawer();
                _initialized = true;
            }

            if(PawnBodyDrawers.NullOrEmpty()) return;
            int i = 0;
            int count = PawnBodyDrawers.Count;
            while(i < count)
            {
                PawnBodyDrawers[i].Tick(bodyFacing);
                i++;
            }
        }

        public List<Material> UnderwearMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if(num != _cachedSkinMatsBodyBaseHash)
            {
                _cachedSkinMatsBodyBase.Clear();
                _cachedSkinMatsBodyBaseHash = num;
                
                var bodyGraphic = Pawn.Drawer.renderer.BodyGraphic;
                if (bodyGraphic == null) return _cachedSkinMatsBodyBase;
                
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    _cachedSkinMatsBodyBase.Add(bodyGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    _cachedSkinMatsBodyBase.Add(bodyGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    _cachedSkinMatsBodyBase.Add(bodyGraphic.MatAt(facing));
                }

                Pawn.Drawer.renderer.renderTree.TraverseTree(node =>
                {
                    if (node is PawnRenderNode_Apparel apparelNode)
                    {
                        ApparelLayerDef lastLayer = apparelNode.apparel.def.apparel.LastLayer;

                        if (lastLayer == ApparelLayerDefOf.OnSkin)
                        {
                            _cachedSkinMatsBodyBase.Add(apparelNode.GraphicFor(Pawn).MatAt(facing));
                        }
                    }
                });
                if (_cachedSkinMatsBodyBase.Count < 1)
                {
                    Pawn.Drawer.renderer.renderTree.TraverseTree(node =>
                    {
                        if (node is PawnRenderNode_Apparel apparelNode)
                        {
                            ApparelLayerDef lastLayer = apparelNode.apparel.def.apparel.LastLayer;

                            if (lastLayer == ApparelLayerDefOf.Middle)
                            {
                                _cachedSkinMatsBodyBase.Add(apparelNode.GraphicFor(Pawn).MatAt(facing));
                            }
                        }
                    });
                }
            }

            return _cachedSkinMatsBodyBase;
        }

        #endregion Public Methods

        public float MovedPercent => _movedPercent;
        public float BodyAngle;

        public float LastAimAngle = 143f;
        public readonly Vector3[] LastPosition = new Vector3[(int)TweenThing.Max];

        public readonly FloatTween AimAngleTween = new FloatTween();
        public bool HasLeftHandPosition => SecondHandPosition != Vector3.zero;

        public Vector3 LastEqPos = Vector3.zero;
        public float DrawOffsetY;

        public void CheckMovement()
        {
            if (HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn == Pawn)
            {
                _isMoving = true;
                _movedPercent = MainTabWindow_BaseAnimator.AnimationPercent;
                return;
            }

            if (IsRider)
            {
                _isMoving = false;
                return;
            }
            Pawn_PathFollower pather = Pawn.pather;
            if ((pather != null) && (pather.Moving) && !Pawn.stances.FullBodyBusy
                && (pather.BuildingBlockingNextPathCell() == null)
                && (pather.NextCellDoorToWaitForOrManuallyOpen() == null)
                && !PawnUtility.AnyPawnBlockingPathAt(pather.nextCell, Pawn))
            {
                _movedPercent = 1f - pather.nextCellCostLeft / pather.nextCellCostTotal;
                _isMoving = true;
            }
            else
            {
                _isMoving = false;
            }
        }

        public bool IsRider { get; set; }

        public void DrawAlienBodyAddons(
            Quaternion quat, 
            Vector3 vector, 
            bool portrait, 
            bool renderBody, 
            Rot4 rotation,
            bool invisible)
        {
            if(PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = PawnBodyDrawers.Count;
            while(i < count)
            {
                PawnBodyDrawers[i].DrawAlienBodyAddons(portrait, vector, quat, renderBody, rotation, invisible);
                i++;
            }
        }

        public void SetWalkCycle(WalkCycleDef walkCycleDef)
        {
            _walkCycle = walkCycleDef;
        }
        
        public float BodyOffsetZ
        {
            get
            {
                if(Controller.settings.UseFeet)
                {
                    WalkCycleDef walkCycle = WalkCycle;
                    if(walkCycle != null)
                    {
                        SimpleCurve curve = walkCycle.BodyOffsetZ;
                        if(curve.PointsCount > 0)
						{
                            return curve.Evaluate(MovedPercent);
                        }
                    }
                }

                return 0f;
            }
        }

        public bool IsMoving => _isMoving;
        internal bool MeshFlipped;
        internal float LastWeaponAngle;
        internal readonly int[] LastPosUpdate = new int[(int)TweenThing.Max];
        internal int LastAngleTick;
        private float _movedPercent;
        private bool _isMoving;
        private WalkCycleDef _walkCycle;
    }
}