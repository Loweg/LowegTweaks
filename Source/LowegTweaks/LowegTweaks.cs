using HarmonyLib;
using Verse;
using UnityEngine;

namespace LowegTweaks {
	[StaticConstructorOnStartup]
	public class LowegTweaks : Mod {
		static LowegTweaks() {
			Harmony.DEBUG = true;
			HarmonyBase.InitPatches();
		}
		public LowegTweaks(ModContentPack content) : base(content) {

		}
		public override string SettingsCategory() => "Loweg's Tweaks"; // todo: translate?

		public override void DoSettingsWindowContents(Rect inRect) {
			GetSettings<Settings>().DoSettingsWindowContents(inRect);
		}

		public static void ApplySettings() {
			var settings = LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>();
			if (settings.drug_crafting) {

			} else {

			}

			if (settings.food_poisoning) {
				//DefDatabase<PreceptDef>.GetNamed("Bed").label = EGB_Settings.Settings.BedLabel;
			} else {

			}

			if (settings.memes_capable_serketist) {

			} else {

			}

			if (settings.worktype_shuffle) {

			} else {

			}

			if (settings.forsaken_dark_mood) {

			} else {

			}
		}
	}

}
