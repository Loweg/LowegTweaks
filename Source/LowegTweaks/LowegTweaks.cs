using RimWorld;
using UnityEngine;
using Verse;

namespace LowegTweaks {
	public class LowegTweaks : Mod {
		static LowegTweaks() {}
		public LowegTweaks(ModContentPack content) : base(content) {}

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
}
