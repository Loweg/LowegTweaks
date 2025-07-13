using RimWorld;
using Verse;
using UnityEngine;

namespace LowegTweaks {
	public class Settings : ModSettings {
		// To add a setting:
		// 	add the variable here
		// 	save it in ExposeData
		// 	put another line in DoSettingsWindows
		// 	add another language key

		// Vanilla
		public bool temperature_overhaul = true;
		public bool sensible_lessons = true;
		public bool worktype_shuffle = false;
		public bool drug_crafting = true;
		public bool food_poisoning = true;
		public bool floor_affordance = true;
		public bool relic_diversification = true;
		public bool quality_rebalance = true;
		public bool blind_rebalance = true;
		public bool fancy_hats = true;
		public bool disable_events = true;
		// VE
		public bool memes_capable_serketist = false;
		public bool factions_tribal_research = false;
		// Alpha Biomes
		public bool forsaken_dark_mood = false;

		public override void ExposeData() {
			Scribe_Values.Look(ref temperature_overhaul, "temperature_overhaul", true);
			Scribe_Values.Look(ref sensible_lessons, "sensible_lessons", true);
			Scribe_Values.Look(ref worktype_shuffle, "worktype_shuffle", false);
			Scribe_Values.Look(ref drug_crafting, "drug_crafting", true);
			Scribe_Values.Look(ref food_poisoning, "food_poisoning", true);
			Scribe_Values.Look(ref floor_affordance, "floor_affordance", true);
			Scribe_Values.Look(ref relic_diversification, "relic_diversification", true);
			Scribe_Values.Look(ref quality_rebalance, "quality_rebalance", true);
			Scribe_Values.Look(ref blind_rebalance, "blind_rebalance", true);
			Scribe_Values.Look(ref fancy_hats, "fancy_hats", true);
            Scribe_Values.Look(ref disable_events, "disable_events", true);


            Scribe_Values.Look(ref memes_capable_serketist, "memes_capable_serketist", false);
			Scribe_Values.Look(ref factions_tribal_research, "factions_tribal_research", false);

			Scribe_Values.Look(ref forsaken_dark_mood, "forsaken_dark_mood", false);
		}

		public void DoSettingsWindowContents(Rect inRect) {
			Rect rectWeCanSee = inRect.ContractedBy(10f);
			rectWeCanSee.height -= 100f; // "close" button
			bool scrollBarVisible = totalContentHeight > rectWeCanSee.height;
			Rect rectThatHasEverything = new Rect(0f, 0f, rectWeCanSee.width - (scrollBarVisible ? ScrollBarWidthMargin : 0), totalContentHeight);
			Widgets.BeginScrollView(rectWeCanSee, ref scrollPosition, rectThatHasEverything);
			float curY = 0f;
			Rect r = new Rect(0, curY, rectThatHasEverything.width, LabelHeight);

			Widgets.Label(r, "LowegSettingsRestartWarning".Translate());
			curY += LabelHeight + 5f;

			Widgets.DrawLineHorizontal(10, curY + 7, rectThatHasEverything.width - 10);
			curY += 10;

			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakTemperatureOverhaul", ref temperature_overhaul);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakSensibleLessons", ref sensible_lessons);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakWorkType", ref worktype_shuffle);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakDrugCrafting", ref drug_crafting);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakFoodPoisoning", ref food_poisoning);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakFloorAffordance", ref floor_affordance);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakRelicDiversification", ref relic_diversification);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakQualityRebalance", ref quality_rebalance);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakBlindRebalance", ref blind_rebalance);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakFancyHats", ref fancy_hats);
            MakeBoolButton(ref curY, rectThatHasEverything.width,
                "LowegTweakDisableEvents", ref fancy_hats);

            Widgets.DrawLineHorizontal(10, curY + 7, rectThatHasEverything.width - 10);
			curY += 10;

			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakCapableSerketist", ref memes_capable_serketist);
			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakTribalResearch", ref factions_tribal_research);

			Widgets.DrawLineHorizontal(10, curY + 7, rectThatHasEverything.width - 10);
			curY += 10;

			MakeBoolButton(ref curY, rectThatHasEverything.width,
				"LowegTweakForsakenDarkMood", ref forsaken_dark_mood);

			Widgets.EndScrollView();
			totalContentHeight = curY + 50f;
		}
		private static Vector2 scrollPosition = new Vector2(0f, 0f);
		private static float totalContentHeight = 1000f;
		private const float ScrollBarWidthMargin = 18f;
		private const float LabelHeight = 22f;

		// Make the button/handle the setting change:
		void MakeBoolButton(ref float curY, float width, string labelKey, ref bool setting) {
			Rect r = new Rect(0, curY, width, LabelHeight);
			Widgets.CheckboxLabeled(r, labelKey.Translate(), ref setting);
			TooltipHandler.TipRegion(r, (labelKey + "Desc").Translate());
			if (Mouse.IsOver(r)) Widgets.DrawHighlight(r);
			curY += LabelHeight + 1f;
		}

		/// <summary>
		/// Grab a given setting given its string name
		/// </summary>
		public static bool IsOptionSet(string name) {
			var v = typeof(Settings).GetField(name, System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance);
			if (v == null) {
				Log.Error("LowegTweaks: option \"" + name + "\" is not a valid Settings variable. Failing.");
				return false;
			}
			if (v.FieldType != typeof(bool)) {
				Log.Error("LowegTweaks: option \"" + name + "\" is not a valid Settings boolean. Failing.");
				return false;
			}

			return (bool)v.GetValue(LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>());
		}
	}
}