using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

using TemperatureOverhaul;

namespace LowegTweaks {
	[StaticConstructorOnStartup]
	public static class HarmonyBase {
		private static Harmony harmony = null;
		static internal Harmony instance {
			get {
				if (harmony == null)
					harmony = new Harmony("LowegTweaks.Harmony");
				return harmony;
			}
		}

		static HarmonyBase() {
			if (LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul) {
				new StabilityInfo().FinalizeInit();
			}
			instance.PatchAll();
		}

		[HarmonyPatch(typeof(WorldGenStep_Terrain), "TemperatureReductionAtElevation")]
		class ElevationPatch {
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
			[HarmonyPostfix]
			public static void Postfix(ref float __result) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= OverallStabilityUtility.GetScaleFactor(stability);
			}
		}

		[HarmonyPatch(typeof(TileTemperaturesComp), "OffsetFromDailyRandomVariation")]
		class DailyVariancePatch {
			[HarmonyPostfix]
			public static void Postfix(ref float __result) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= OverallStabilityUtility.GetScaleFactor(stability);
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
		}

		[HarmonyPatch(typeof(GenTemperature), "OffsetFromSunCycle")]
		class SunEffectPatch {
			[HarmonyPostfix]
			public static void Postfix(ref float __result, int tile) {
				var rainfall = Find.WorldGrid[tile].rainfall;
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= OverallStabilityUtility.GetScaleFactor(stability) * OverallStabilityUtility.RainfallStabilityEffect.Evaluate(rainfall);
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
		}

		[HarmonyPatch(typeof(TerrainDefGenerator_Carpet), "CarpetFromBlueprint")]
		class CarpetAffordancePatch {
			[HarmonyPostfix]
			public static void Postfix(ref TerrainDef __result) {
				__result.terrainAffordanceNeeded = TerrainAffordanceDefOf.Light;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().floor_affordance;
			}
		}
	}
}
