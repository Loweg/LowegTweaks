using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

using TemperatureOverhaul;

namespace LowegTweaks {
	public static class HarmonyBase {
		private static Harmony harmony = null;
		static internal Harmony instance {
			get {
				if (harmony == null)
					harmony = new Harmony("LowegTweaks.Harmony");
				return harmony;
			}
		}

		public static void InitPatches() {
			Log.Message("Executing patches...");
			new StabilityInfo().FinalizeInit();
			instance.PatchAll(Assembly.GetExecutingAssembly());
		}

		[HarmonyPatch(typeof(WorldGenStep_Terrain), "TemperatureReductionAtElevation")]
		class ElevationPatch {
			[HarmonyPostfix]
			static void Postfix(ref float __result) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= OverallStabilityUtility.GetScaleFactor(stability);
			}
		}

		[HarmonyPatch(typeof(TileTemperaturesComp), "OffsetFromDailyRandomVariation")]
		class DailyVariancePatch {
			[HarmonyPostfix]
			static void Postfix(ref float __result) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= OverallStabilityUtility.GetScaleFactor(stability);
			}
		}

		[HarmonyPatch(typeof(GenTemperature), "OffsetFromSunCycle")]
		class SunEffectPatch {
			[HarmonyPostfix]
			static void Postfix(ref float __result, int tile) {
				var rainfall = Find.WorldGrid[tile].rainfall;
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= OverallStabilityUtility.GetScaleFactor(stability) * OverallStabilityUtility.RainfallStabilityEffect.Evaluate(rainfall);
			}
		}
	}
}
