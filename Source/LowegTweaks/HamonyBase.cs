using HarmonyLib;
using LowegTweaks.TemperatureOverhaul;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;

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

		[HarmonyPatch(typeof(JobDriver_Lessongiving), "ChooseSkill")]
		class LessonPickerPatch {
			[HarmonyPrefix]
			public static bool Prefix(JobDriver_Lessongiving __instance, ref SkillDef __result, Pawn student) {
				List<SkillDef> pool = new List<SkillDef>();
				foreach (SkillRecord skill in __instance.pawn.skills.skills) {
					SkillRecord student_skill = student.skills.GetSkill(skill.def);
					if (skill.PermanentlyDisabled || student_skill.PermanentlyDisabled) continue;
					int toAdd = skill.Level;

					if (student_skill.passion == Passion.Minor) {
						toAdd *= 2;
					} else if (student_skill.passion == Passion.Major) {
						toAdd *= 3;
					}

					for (int i = 0; i < toAdd; i++) {
						pool.Add(skill.def);
					}
				}
				__result = pool.RandomElement();
				return false;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().sensible_lessons;
			}
		}

		[HarmonyPatch(typeof(SkillRecord), "Learn")]
		class LearnDisabledSkillIfChildPatch {
			[HarmonyTranspiler]
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
				var codes = new List<CodeInstruction>(instructions);

				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Call, typeof(ChildLearningUtility).GetMethod(nameof(ChildLearningUtility.ShouldSkipDisabledCheck)));
				yield return new CodeInstruction(OpCodes.Brtrue_S, codes[2].operand);

				for (var i = 0; i < codes.Count; i++)
				{
					yield return codes[i];
				}
			}
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
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
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

		[HarmonyPatch(typeof(Plant), nameof(Plant.TrySpawnStump))]
		class StumpChopPatch {
			[HarmonyPostfix]
			public static void Postfix(ref Thing __result, Plant __instance) {
				if (__result == null || __instance.Map == null) {
					return;
				}
				Map map = __instance.Map;
				IntVec3 position = __result.Position;
				Pawn pawn = null;
				foreach (IntVec3 item in GenRadial.RadialCellsAround(position, 2f, useCenter: true)) {
					if (!item.InBounds(map)) {
						continue;
					}
					List<Thing> list = map.thingGrid.ThingsListAt(item);
					for (int j = 0; j < list.Count; j++) {
						if (list[j] is Pawn pawn2 && pawn2.Dead == false && (pawn2.Faction == Faction.OfPlayer || pawn2.IsColonist || pawn2.IsPrisonerOfColony || pawn2.IsSlaveOfColony) && pawn2.workSettings != null && pawn2.workSettings.EverWork && pawn2.workSettings.WorkIsActive(WorkTypeDefOf.Growing)) {
							pawn = pawn2;
							break;
						}
					}
					if (pawn == null) {
						continue;
					}
					break;
				}
				if (__result.def.defName.StartsWith("Chop")) {
					Designation newDes = new Designation(__result, DesignationDefOf.HarvestPlant);
					map.designationManager.AddDesignation(newDes);
					Job job = JobMaker.MakeJob(JobDefOf.CutPlant, __result);
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
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
		[HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(int), typeof(bool) })]
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

		// Not really sure what's causing exceptions on birth, maybe the new rimefeller patch but my tools are failing me so it's hard to tell.
		[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.SetPriority))]
		class FixExceptionPatch {
			[HarmonyPrefix]
			public static bool Prefix(WorkTypeDef w) {
				return w != null;
			}
		}
	}
}
