using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace LowegTweaks {
	public class LowegTweaks : Mod {
		public static Harmony harmony = null;
		static LowegTweaks() {}
		public LowegTweaks(ModContentPack content) : base(content) {
			harmony = new Harmony("loweg.lowegtweaks");
			Settings settings = GetSettings<Settings>();
			if (settings.temperature_overhaul) {
				new TemperatureOverhaul.StabilityInfo().FinalizeInit();
			}
			if (settings.worktype_shuffle) {
				harmony.Patch(AccessTools.Method(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve)),
					postfix: new HarmonyMethod(typeof(LowegTweaks), nameof(LowegTweaks.RemoveWorkTypesPatch)));
			}
		}
		private static void RemoveWorkTypesPatch() {
			PawnTableDef table = PawnTableDefOf.Work;
			table.columns.RemoveAll(c => c.workType?.defName == "Smithing" || c.workType?.defName == "Tailoring" || c.workType?.defName == "PlantCutting");
		}

		public static void ApplySettings() {
			var settings = LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>();
		}

		public override string SettingsCategory() => "Loweg's Tweaks";

		public override void DoSettingsWindowContents(Rect inRect) {
			GetSettings<Settings>().DoSettingsWindowContents(inRect);
		}
	}

	public class PatchOperationSettings : PatchOperationSequence {
		private string optionKey = "";
		protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
			if (optionKey == null || optionKey == "") {
				Log.Error("[PatchOperationSettings] No such option key\n" + xml);
				return false;
			}
			if (Settings.IsOptionSet(optionKey)) {
				return base.ApplyWorker(xml);
			}
			return true;
		}
	}
	public class QualityGenerator {
		public static QualityCategory GetQuality(int relevantSkillLevel, bool inspired) {
			// Div by 0 safeguard
			if (relevantSkillLevel < 0) return QualityCategory.Awful;
			float centerX = 0.75f + (relevantSkillLevel * 0.17f);
			int num = Mathf.Clamp((int)Rand.GaussianAsymmetric(centerX, 4f/(relevantSkillLevel + 4), 0.8f), 0, 5);
			int quality_boost = inspired ? 2 : 0;
			return (QualityCategory) Mathf.Min(num + quality_boost, 6);
		}
	}
	public class ChildLearningUtility {
		public static bool ShouldSkipDisabledCheck(SkillRecord skill) {
			return skill.Pawn.needs != null && skill.Pawn.needs.learning != null;
		}
	}

	public class LowegDebugTools {
		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static DebugActionNode SetDurability() {
			DebugActionNode debugActionNode = new DebugActionNode();
			for (int i = 0; i <= 100; i+=10) {
				int toSetPercent = Math.Max(1, i);
				debugActionNode.AddChild(new DebugActionNode(toSetPercent.ToString() + "%", DebugActionType.ToolMap, delegate {
					foreach (Thing thing in UI.MouseCell().GetThingList(Find.CurrentMap)) {
						float hp = (toSetPercent * 0.01f) * (float)thing.MaxHitPoints;
						thing.HitPoints = (int)hp;
					}
				}));
			}
			return debugActionNode;
		}
	}
}
