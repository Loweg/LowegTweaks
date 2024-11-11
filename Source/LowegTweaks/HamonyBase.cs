using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using UnityEngine;

using RimWorld;
using RimWorld.Planet;
using Verse;

using LowegTweaks.TemperatureOverhaul;

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

		// Floor affordance
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

		// Work types
		[HarmonyPatch(typeof(Alert_ConnectedPawnNotAssignedToPlantCutting), "GetTargets")]
		class AlertPlantCuttingPatch {
			// Simple replacement of vanilla with growing instead of plantcutting
			[HarmonyPrefix]
			public static bool Prefix(Alert_ConnectedPawnNotAssignedToPlantCutting __instance) {
				List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; ++i) {
					if (maps[i].IsPlayerHome) {
						List<Thing> thingList = maps[i].listerThings.ThingsInGroup(ThingRequestGroup.DryadSpawner);
						for (int j = 0; j < thingList.Count; ++j) {
							CompTreeConnection comp = thingList[j].TryGetComp<CompTreeConnection>();
							if (comp != null && comp.Connected && comp.DesiredConnectionStrength > 0.0 && comp.ConnectedPawn.workSettings.GetPriority(WorkTypeDefOf.Growing) == 0)
								targets.Add(comp.ConnectedPawn);
						}
					}
				}
				typeof(Alert_ConnectedPawnNotAssignedToPlantCutting).GetField("targets", BindingFlags.Instance | BindingFlags.NonPublic)
					.SetValue(__instance, targets);

				return false;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().worktype_shuffle;
			}
		}

		[HarmonyPatch(typeof(ThoughtWorker_GauranlenConnectionDesired), "ShouldHaveThought")]
		class ThoughtGauranlenPatch {
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				var codes = new List<CodeInstruction>(instructions);

				for (var i = 0; i < codes.Count; i++) {
					if (codes[i].opcode == OpCodes.Ldsfld && (codes[i].operand as FieldInfo).Name == "PlantCutting") {
						yield return new CodeInstruction(OpCodes.Ldsfld, typeof(WorkTypeDefOf).GetField("Growing", BindingFlags.Static | BindingFlags.Public));
					} else {
						yield return codes[i];
					}
				}
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().worktype_shuffle;
			}
		}

		[HarmonyPatch(typeof(CompUseEffect_InstallImplantMechlink), nameof(CompUseEffect_InstallImplantMechlink.ConfirmMessage))]
		class InstallMechlinkPatch {
			[HarmonyPrefix]
			public static bool Prefix(ref TaggedString __result, Pawn p) {
				__result = p.WorkTypeIsDisabled(WorkTypeDefOf.Crafting) ? "ConfirmInstallMechlink".Translate() : null;
				return false;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().worktype_shuffle;
			}
		}

		// Quality rebalance
		[HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn))]
		class QualityPatch {
			[HarmonyPrefix]
			public static bool Prefix(ref QualityCategory __result, int relevantSkillLevel, bool inspired) {
				__result = QualityGenerator.GetQuality(relevantSkillLevel, inspired);
				return false;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().quality_rebalance;
			}
		}
	}
}
