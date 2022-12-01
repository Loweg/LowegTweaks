using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using UnityEngine;

using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LowegTweaks.TemperatureOverhaul {
	public static class HarmonyPatches {
		[HarmonyPatch(typeof(WorldGenStep_Terrain), "TemperatureReductionAtElevation")]
		class ElevationPatch {
			[HarmonyPostfix]
			public static void Postfix(ref float __result) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= stability.GetScaleFactor();
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
		}

		[HarmonyPatch(typeof(TileTemperaturesComp), "OffsetFromDailyRandomVariation")]
		class DailyVariancePatch {
			[HarmonyPostfix]
			public static void Postfix(ref float __result) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result *= stability.GetScaleFactor();
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
				__result *= stability.GetScaleFactor() * OverallStabilityUtility.RainfallStabilityEffect.Evaluate(rainfall);
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
		}

		[HarmonyPatch(typeof(WorldGenStep_Terrain), "BaseTemperatureAtLatitude")]
		class LatitudeCurvePatch {
			[HarmonyPrefix]
			public static bool Prefix(ref float __result, float lat) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				__result = stability.GetLatitudeCurve().Evaluate(Mathf.Abs(lat) / 90f);
				return false;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
		}

		[HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.SeasonalShiftAmplitudeAt))]
		class SeasonalCurvePatch {
			[HarmonyPrefix]
			public static bool Prefix(ref float __result, int tile) {
				var stability = Current.Game.GetComponent<StabilityInfo>().overallStability;
				var curve = stability.GetSeasonCurve();
				if (Find.WorldGrid.LongLatOf(tile).y >= 0.0) {
					__result = curve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
				} else {
					__result = -curve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
				}
				return false;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
		}

		[HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
		class WorldCreationPagePatch {
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
				var codes = new List<CodeInstruction>(instructions);

				LocalBuilder stability = generator.DeclareLocal(typeof(OverallStability).MakeByRefType());
				stability.SetLocalSymInfo("stability");

				for (var i = 0; i < codes.Count; i++) {
					yield return codes[i];
					if (codes[i].opcode == OpCodes.Stfld && (codes[i].operand as FieldInfo).Name == "temperature") {
						// y6 = y6 + 40f;
						yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
						yield return new CodeInstruction(OpCodes.Ldc_R4, 40f);
						yield return new CodeInstruction(OpCodes.Add);
						yield return new CodeInstruction(OpCodes.Stloc_S, 7);

						// Widgets.Label(new Rect(0.0f, y6, 200f, 30f), "PlanetStability".Translate());
						yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.LabelRect)));
						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.LabelText)));
						yield return new CodeInstruction(OpCodes.Call, typeof(Widgets).GetMethod(nameof(Widgets.Label), new[] { typeof(Rect), typeof(TaggedString) }));

						// this.stability = (OverallStability) Mathf.RoundToInt(Widgets.HorizontalSlider(
						//	new Rect(200f, y6, width2, 30f), (float) this.stability, 0.0f, (float) (OverallStabilityUtility.EnumValuesCount - 1), true, (string) "PlanetStability_Normal".Translate(), (string) "PlanetStability_Low".Translate(), (string) "PlanetStability_High".Translate(), 1f
						// ));
						yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
						yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.SliderRect)));

						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.StabilityAsFloat)));

						yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);

						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.AdjustedStabilityEnum)));

						yield return new CodeInstruction(OpCodes.Ldc_I4_1);

						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.SliderTextNormal)));
						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.SliderTextLow)));
						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.SliderTextHigh)));

						yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);

						yield return new CodeInstruction(OpCodes.Call, typeof(Widgets).GetMethod(nameof(Widgets.HorizontalSlider), new[] { typeof(Rect), typeof(float), typeof(float), typeof(float), typeof(bool), typeof(string), typeof(string), typeof(string), typeof(float) }));

						yield return new CodeInstruction(OpCodes.Call, typeof(WorldParamHelper).GetMethod(nameof(WorldParamHelper.SetStability)));
					}
				}
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().temperature_overhaul;
			}
		}
	}
}
