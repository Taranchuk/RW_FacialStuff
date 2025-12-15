using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class AnimationDef : Def
{
	public int durationTicks;

	public bool startOnRandomTick;

	public bool playWhenDowned;

	public float posRotScale = 1f;

	public LoopMode loopMode;

	public Dictionary<PawnRenderNodeTagDef, KeyframeAnimationPart> keyframeParts;

	public Dictionary<PawnRenderNodeTagDef, CurveKeyAnimationPart> curveParts;

	public bool TryGetPartForNodeTag(PawnRenderNodeTagDef nodeTag, out AnimationPart part)
	{
		if (keyframeParts != null && keyframeParts.TryGetValue(nodeTag, out var value))
		{
			part = value;
			return true;
		}
		if (curveParts != null && curveParts.TryGetValue(nodeTag, out var value2))
		{
			part = value2;
			return true;
		}
		part = null;
		return false;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (keyframeParts == null || curveParts == null)
		{
			yield break;
		}
		PawnRenderNodeTagDef pawnRenderNodeTagDef = default(PawnRenderNodeTagDef);
		KeyframeAnimationPart keyframeAnimationPart = default(KeyframeAnimationPart);
		foreach (KeyValuePair<PawnRenderNodeTagDef, KeyframeAnimationPart> keyframePart in keyframeParts)
		{
			keyframePart.Deconstruct(ref pawnRenderNodeTagDef, ref keyframeAnimationPart);
			PawnRenderNodeTagDef pawnRenderNodeTagDef2 = pawnRenderNodeTagDef;
			if (curveParts.ContainsKey(pawnRenderNodeTagDef2))
			{
				yield return $"Animation def has a render node tag {pawnRenderNodeTagDef2} which is driven by both keyframes and curve parts. You can only use one or the other for a given node tag";
			}
		}
	}

	public virtual IEnumerable<(GraphicStateDef state, Graphic graphic)> GetGraphicStates(PawnRenderNode node)
	{
		PawnRenderNodeTagDef pawnRenderNodeTagDef = default(PawnRenderNodeTagDef);
		if (keyframeParts != null)
		{
			KeyframeAnimationPart keyframeAnimationPart = default(KeyframeAnimationPart);
			foreach (KeyValuePair<PawnRenderNodeTagDef, KeyframeAnimationPart> keyframePart in keyframeParts)
			{
				keyframePart.Deconstruct(ref pawnRenderNodeTagDef, ref keyframeAnimationPart);
				PawnRenderNodeTagDef pawnRenderNodeTagDef2 = pawnRenderNodeTagDef;
				KeyframeAnimationPart keyframeAnimationPart2 = keyframeAnimationPart;
				if (pawnRenderNodeTagDef2 != node.Props.tagDef || keyframeAnimationPart2.keyframes.NullOrEmpty())
				{
					continue;
				}
				foreach (Keyframe keyframe in keyframeAnimationPart2.keyframes)
				{
					if (keyframe.graphicState == null || !keyframe.graphicState.TryGetDefaultGraphic(out var graphic))
					{
						continue;
					}
					if (keyframe.graphicState.applySkinColorTint)
					{
						Color baseColor = graphic.Color;
						Color baseColor2 = graphic.ColorTwo;
						Pawn pawn = node.tree.pawn;
						if (pawn.IsMutant)
						{
							baseColor = MutantUtility.GetMutantSkinColor(pawn, baseColor);
							baseColor2 = MutantUtility.GetMutantSkinColor(pawn, baseColor2);
						}
						baseColor = pawn.health.hediffSet.GetSkinColor(baseColor);
						baseColor2 = pawn.health.hediffSet.GetSkinColor(baseColor2);
						graphic = graphic.GetColoredVersion(graphic.Shader, baseColor, baseColor2);
					}
					yield return (state: keyframe.graphicState, graphic: graphic);
				}
			}
		}
		if (curveParts == null)
		{
			yield break;
		}
		CurveKeyAnimationPart curveKeyAnimationPart = default(CurveKeyAnimationPart);
		foreach (KeyValuePair<PawnRenderNodeTagDef, CurveKeyAnimationPart> curvePart in curveParts)
		{
			curvePart.Deconstruct(ref pawnRenderNodeTagDef, ref curveKeyAnimationPart);
			PawnRenderNodeTagDef pawnRenderNodeTagDef3 = pawnRenderNodeTagDef;
			CurveKeyAnimationPart curveKeyAnimationPart2 = curveKeyAnimationPart;
			if (pawnRenderNodeTagDef3 != node.Props.tagDef || curveKeyAnimationPart2.keyframes.NullOrEmpty())
			{
				continue;
			}
			foreach (CurveKey keyframe2 in curveKeyAnimationPart2.keyframes)
			{
				if (keyframe2.graphicState != null && keyframe2.graphicState.TryGetDefaultGraphic(out var graphic2))
				{
					yield return (state: keyframe2.graphicState, graphic: graphic2);
				}
			}
		}
	}
}
