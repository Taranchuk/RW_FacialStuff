namespace PawnPlus
{
    using System.Collections.Generic;

    using HugsLib;

    using PawnPlus.Defs;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
	class PawnPlusModBase : ModBase
	{
		static PawnPlusModBase()
		{
			if(ParseHelper.Parsers<Quaternion>.parser == null)
			{
				ParseHelper.Parsers<Quaternion>.Register(QuaternionFromString);
			}
		}

		public static Quaternion QuaternionFromString(string str)
		{
			Vector4 vec4 = ParseHelper.FromStringVector4Adaptive(str);
			return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
		}

		public override string ModIdentifier
		{
			get
			{
				return "PawnPlus";
			}
		}

		protected override bool HarmonyAutoPatch
		{
			get
			{
				return false;
			}
		}

		public override void DefsLoaded()
		{
			base.DefsLoaded();
		}
	}
}
