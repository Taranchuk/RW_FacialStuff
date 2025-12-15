namespace PawnPlus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using JetBrains.Annotations;

    using PawnPlus.Defs;

    using RimWorld;
    using RimWorld.Planet;

    using UnityEngine;

    using Verse;

    public class GameComponent_PawnPlus : GameComponent
    {
        protected Animator animator;

        #region Public Constructors
        
        public GameComponent_PawnPlus()
        {
        }
        public GameComponent_PawnPlus(Game game)
        {
            WeaponCompsNew();
            AnimalPawnCompsBodyDefImport();
            AnimalPawnCompsImportFromAnimationTargetDefs();
            Controller.SetMainButtons();

        }

        #endregion Public Constructors

        #region Public Methods
        
        public override void GameComponentTick()
        {
            base.GameComponentTick();
        }

        public static void BuildWalkCycles([CanBeNull] WalkCycleDef defToRebuild = null)
        {
            List<WalkCycleDef> cycles = new List<WalkCycleDef>();
            if (defToRebuild != null)
            {
                cycles.Add(defToRebuild);
            }
            else
            {
                cycles = DefDatabase<WalkCycleDef>.AllDefsListForReading;
            }

            if (cycles == null)
            {
                return;
            }

            for (int index = 0; index < cycles.Count; index++)
            {
                WalkCycleDef cycle = cycles[index];
                if (cycle != null)
                {
                    cycle.HeadAngleX = new SimpleCurve();
                    cycle.HeadOffsetZ = new SimpleCurve();

                    cycle.BodyAngle = new SimpleCurve();
                    cycle.BodyAngleVertical = new SimpleCurve();
                    cycle.BodyOffsetZ = new SimpleCurve();
                    cycle.FootAngle = new SimpleCurve();
                    cycle.FootPositionX = new SimpleCurve();
                    cycle.FootPositionZ = new SimpleCurve();
                    cycle.HandsSwingAngle = new SimpleCurve();
                    cycle.HandsSwingPosVertical = new SimpleCurve();
                    cycle.ShoulderOffsetHorizontalX = new SimpleCurve();
                    cycle.HipOffsetHorizontalX = new SimpleCurve();
                    cycle.FrontPawAngle = new SimpleCurve();
                    cycle.FrontPawPositionX = new SimpleCurve();
                    cycle.FrontPawPositionZ = new SimpleCurve();
                    if (cycle.keyframes.NullOrEmpty())
                    {
                        cycle.keyframes = new List<PawnKeyframe>();
                        for (int i = 0; i < 9; i++)
                        {
                            cycle.keyframes.Add(new PawnKeyframe(i));
                        }
                    }
                    foreach (PawnKeyframe key in cycle.keyframes)
                    {
                        BuildAnimationKeys(key, cycle);
                    }
                }
            }
        }
        public static void BuildPoseCycles([CanBeNull] PoseCycleDef defToRebuild = null)
        {
            List<PoseCycleDef> cycles = new List<PoseCycleDef>();
            if (defToRebuild != null)
            {
                cycles.Add(defToRebuild);
            }
            else
            {
                cycles = DefDatabase<PoseCycleDef>.AllDefsListForReading;
            }

            if (cycles != null)
            {
                for (int index = 0; index < cycles.Count; index++)
                {
                    PoseCycleDef cycle = cycles[index];
                    if (cycle != null)
                    {
                        cycle.BodyAngle = new SimpleCurve();
                        cycle.BodyAngleVertical = new SimpleCurve();
                        cycle.BodyOffsetZ = new SimpleCurve();
                        cycle.FootAngle = new SimpleCurve();
                        cycle.FootPositionX = new SimpleCurve();
                        cycle.FootPositionZ = new SimpleCurve();
                        cycle.HandPositionX = new SimpleCurve();
                        cycle.HandPositionZ = new SimpleCurve();
                        cycle.HandsSwingAngle = new SimpleCurve();
                        cycle.HandsSwingPosVertical = new SimpleCurve();
                        cycle.ShoulderOffsetHorizontalX = new SimpleCurve();
                        cycle.HipOffsetHorizontalX = new SimpleCurve();
                        cycle.FrontPawAngle = new SimpleCurve();
                        cycle.FrontPawPositionX = new SimpleCurve();
                        cycle.FrontPawPositionZ = new SimpleCurve();
                        if (cycle.keyframes.NullOrEmpty())
                        {
                            cycle.keyframes = new List<PawnKeyframe>();
                            for (int i = 0; i < 9; i++)
                            {
                                cycle.keyframes.Add(new PawnKeyframe(i));
                            }
                        }
                        if (cycle.keyframes != null)
                        {
                            foreach (PawnKeyframe key in cycle.keyframes)
                            {
                                BuildAnimationKeys(key, cycle);
                            }
                        }
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static void BuildAnimationKeys(PawnKeyframe key, WalkCycleDef cycle)
        {
            List<PawnKeyframe> keyframes = cycle.keyframes;

            List<PawnKeyframe> autoKeys = keyframes.Where(x => x.Status != KeyStatus.Manual).ToList();

            List<PawnKeyframe> manualKeys = keyframes.Where(x => x.Status == KeyStatus.Manual).ToList();

            float autoFrames = (float)key.KeyIndex / (autoKeys.Count - 1);

            float frameAt;
            if (!manualKeys.NullOrEmpty())
            {
                frameAt = (float)key.KeyIndex / (autoKeys.Count - 1);
                float divider = (float)1 / (autoKeys.Count - 1);
                float? shift = manualKeys.Find(x => x.KeyIndex == key.KeyIndex)?.Shift;
                if (shift.HasValue)
                {
                    frameAt += divider * shift.Value;
                }
            }
            else
            {
                frameAt = (float)key.KeyIndex / (keyframes.Count - 1);
            }

            Dictionary<SimpleCurve, float?> dict = new Dictionary<SimpleCurve, float?>
                                                       {
                                                           {cycle.HeadAngleX, key.HeadAngleX },
                                                           {cycle.HeadOffsetZ, key.HeadOffsetZ },
                                                           {
                                                               cycle.ShoulderOffsetHorizontalX,
                                                               key.ShoulderOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.HipOffsetHorizontalX,
                                                               key.HipOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.BodyAngleVertical,
                                                               key.BodyAngleVertical
                                                           },
                                                           {
                                                               cycle.BodyOffsetZ,
                                                               key.BodyOffsetZ
                                                           },
                                                           {
                                                               cycle.FootAngle,
                                                               key.FootAngle
                                                           },
                                                           {
                                                               cycle.FootPositionX,
                                                               key.FootPositionX
                                                           },
                                                           {
                                                               cycle.FootPositionZ,
                                                               key.FootPositionZ
                                                           },
                                                           {
                                                               cycle.HandsSwingAngle,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.HandsSwingPosVertical,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.FrontPawAngle,
                                                               key.FrontPawAngle
                                                           },
                                                           {
                                                               cycle.FrontPawPositionX,
                                                               key.FrontPawPositionX
                                                           },
                                                           {
                                                               cycle.FrontPawPositionZ,
                                                               key.FrontPawPositionZ
                                                           }
                                                       };

            foreach (KeyValuePair<SimpleCurve, float?> pair in dict)
            {
                UpdateCurve(key, pair.Value, pair.Key, frameAt);
            }
        }

        private static void BuildAnimationKeys(PawnKeyframe key, PoseCycleDef cycle)
        {
            List<PawnKeyframe> keyframes = cycle.keyframes;

            List<PawnKeyframe> autoKeys = keyframes.Where(x => x.Status != KeyStatus.Manual).ToList();

            List<PawnKeyframe> manualKeys = keyframes.Where(x => x.Status == KeyStatus.Manual).ToList();

            float autoFrames = (float)key.KeyIndex / (autoKeys.Count - 1);

            float frameAt;
            if (!manualKeys.NullOrEmpty())
            {
                frameAt = (float)key.KeyIndex / (autoKeys.Count - 1);
                Log.Message("frameAt " + frameAt);
                float divider = (float)1 / (autoKeys.Count - 1);
                Log.Message("divider " + divider);
                float? shift = manualKeys.Find(x => x.KeyIndex == key.KeyIndex)?.Shift;
                if (shift.HasValue)
                {
                    Log.Message("Shift " + shift);
                    frameAt += divider * shift.Value;
                    Log.Message("new frameAt " + frameAt);
                }
            }
            else
            {
                frameAt = (float)key.KeyIndex / (keyframes.Count - 1);
            }

            Dictionary<SimpleCurve, float?> dict = new Dictionary<SimpleCurve, float?>
                                                       {
                                                           {
                                                               cycle.ShoulderOffsetHorizontalX,
                                                               key.ShoulderOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.HipOffsetHorizontalX,
                                                               key.HipOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.BodyAngle,
                                                               key.BodyAngle
                                                           },
                                                           {
                                                               cycle.BodyAngleVertical,
                                                               key.BodyAngleVertical
                                                           },
                                                           {
                                                               cycle.BodyOffsetZ,
                                                               key.BodyOffsetZ
                                                           },
                                                           {
                                                               cycle.FootAngle,
                                                               key.FootAngle
                                                           },
                                                           {
                                                               cycle.FootPositionX,
                                                               key.FootPositionX
                                                           },
                                                           {
                                                               cycle.FootPositionZ,
                                                               key.FootPositionZ
                                                           },
                                                           {
                                                               cycle.HandPositionX,
                                                               key.HandPositionX
                                                           },
                                                           {
                                                               cycle.HandPositionZ,
                                                               key.HandPositionZ
                                                           },
                                                           {
                                                               cycle.HandsSwingAngle,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.HandsSwingPosVertical,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.FrontPawAngle,
                                                               key.FrontPawAngle
                                                           },
                                                           {
                                                               cycle.FrontPawPositionX,
                                                               key.FrontPawPositionX
                                                           },
                                                           {
                                                               cycle.FrontPawPositionZ,
                                                               key.FrontPawPositionZ
                                                           }
                                                       };

            foreach (KeyValuePair<SimpleCurve, float?> pair in dict)
            {
                UpdateCurve(key, pair.Value, pair.Key, frameAt);
            }
        }

        private static void UpdateCurve(PawnKeyframe key, float? curvePoint, SimpleCurve simpleCurve, float frameAt)
        {
            if (curvePoint.HasValue)
            {
                simpleCurve.Add(frameAt, curvePoint.Value);
            }
            else
            {
                if (key.KeyIndex == 0)
                {
                    simpleCurve.Add(0, 0);
                    simpleCurve.Add(1, 0);
                }
            }
        }

        private bool HandCheck()
        {
            return ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Clutter Laser Rifle");
        }

        private void LaserLoad()
        {
            if (this.HandCheck())
            {
                ThingDef wepzie = ThingDef.Named("LaserRifle");
                if (wepzie != null)
                {
                    CompProperties_WeaponExtensions extensions =
                    new CompProperties_WeaponExtensions
                    {
                        compClass = typeof(CompWeaponExtensions),
                        RightHandPosition = new Vector3(-0.2f, 0.3f, -0.05f),
                        LeftHandPosition = new Vector3(0.25f, 0f, -0.05f)
                    };
                    wepzie.comps.Add(extensions);
                }
            }
        }

        private void WeaponCompsNew()
        {
            foreach (WeaponExtensionDef weaponExtensionDef in DefDatabase<WeaponExtensionDef>.AllDefsListForReading)
            {
                ThingDef thingDef;
                try
                {
                    thingDef = ThingDef.Named(weaponExtensionDef.weapon);

                }
                catch
                {
                    continue;
                }

                if (thingDef == null)
                {
                    continue;
                }

                if (thingDef.HasComp(typeof(CompProperties_WeaponExtensions)))
                {
                    return;
                }

                CompProperties_WeaponExtensions weaponExtensions =
                new CompProperties_WeaponExtensions
                {
                    compClass = typeof(CompWeaponExtensions),
                    AttackAngleOffset = weaponExtensionDef.attackAngleOffset,
                    WeaponPositionOffset = weaponExtensionDef.weaponPositionOffset,
                    AimedWeaponPositionOffset = weaponExtensionDef.aimedWeaponPositionOffset,
                    RightHandPosition = weaponExtensionDef.firstHandPosition,
                    LeftHandPosition = weaponExtensionDef.secondHandPosition
                };

                thingDef.comps?.Add(weaponExtensions);
            }

            this.LaserLoad();
        }

        private void AnimalPawnCompsImportFromAnimationTargetDefs()
        {
            foreach (AnimationTargetDef def in DefDatabase<AnimationTargetDef>.AllDefsListForReading)
            {
                if (def.CompLoaderTargets.NullOrEmpty())
                {
                    continue;
                }

                foreach (CompLoaderTargets pawnSets in def.CompLoaderTargets)
                {
                    if (pawnSets == null)
                    {
                        continue;
                    }

                    if (pawnSets.thingTargets.NullOrEmpty())
                    {
                        continue;
                    }

                    foreach (string target in pawnSets.thingTargets)
                    {
                        ThingDef thingDef = ThingDef.Named(target);
                        if (thingDef == null)
                        {
                            continue;
                        }
                        if (thingDef.HasComp(typeof(CompBodyAnimator)))
                        {
                            continue;
                        }

                        CompProperties_BodyAnimator bodyAnimator = new CompProperties_BodyAnimator
                                                                       {
                                                                           compClass = typeof(CompBodyAnimator),
                                                                           bodyDrawers = pawnSets.bodyDrawers,
                                                                           handType = pawnSets.handType,
                                                                           quadruped = pawnSets.quadruped,
                                                                           bipedWithHands = pawnSets.bipedWithHands
                                                                       };
                        thingDef.comps?.Add(bodyAnimator);
                    }
                }
            }

            this.LaserLoad();
        }

        private void AnimalPawnCompsBodyDefImport()
        {
            foreach (BodyAnimDef def in DefDatabase<BodyAnimDef>.AllDefsListForReading)
            {
                string target = def.thingTarget;
                if (target.NullOrEmpty())
                {
                    continue;
                }

                ThingDef thingDef = ThingDef.Named(target);
                if (thingDef == null)
                {
                    continue;
                }
                if (thingDef.HasComp(typeof(CompBodyAnimator)))
                {
                    continue;
                }

                CompProperties_BodyAnimator bodyAnimator = new CompProperties_BodyAnimator
                                                               {
                                                                   compClass = typeof(CompBodyAnimator),
                                                                   bodyDrawers = def.bodyDrawers,
                                                                   handType = def.handType,
                                                                   quadruped = def.quadruped,
                                                                   bipedWithHands = def.bipedWithHands
                                                               };

                thingDef.comps?.Add(bodyAnimator);
            }
        }

        #endregion Private Methods
    }
}