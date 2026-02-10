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
		static HarmonyBase() {
			LowegTweaks mod = LoadedModManager.GetMod<LowegTweaks>();
			if (mod.GetSettings<Settings>().temperature_overhaul) {
				new StabilityInfo().FinalizeInit();
			}
			LowegTweaks.harmony.PatchAll();
		}

		[HarmonyPatch(typeof(Building_OutfitStand), "BeautyOffset", MethodType.Getter)]
		class PrettyOutfitStandPatch {
			[HarmonyPrefix]
			public static bool Prefix(float __result) {
				__result = 0f;
				return false;
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().pretty_outfit_stand;
			}
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
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().sensible_lessons;
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

        [HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
        class StumpChopPatch {
			[HarmonyTranspiler]
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
				var codes = new List<CodeInstruction>(instructions);
				for (var i = 0; i < codes.Count; i++) {
                    if (codes[i].opcode == OpCodes.Callvirt && (codes[i].operand as MethodInfo).Name == "Destroy") {
                        yield return codes[i]; // Destroy
						i++;
                        yield return codes[i]; // ldloc.1
                        i++;
                        yield return codes[i]; // brfalse
                        i++;
                        yield return codes[i]; // ldarg.2
                        i++;
						CodeInstruction code = codes[i];
						code.opcode = OpCodes.Ldc_I4_2;
						yield return code;
                    } else {
						yield return codes[i];
					}
				}
			}
			public static bool Prepare() {
				return LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>().stump_chop;
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

