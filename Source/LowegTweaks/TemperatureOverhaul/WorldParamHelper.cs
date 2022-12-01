using RimWorld;
using UnityEngine;
using Verse;

namespace LowegTweaks.TemperatureOverhaul {
	class WorldParamHelper {
		public static void SetStability(float value) {
			var stability = Mathf.RoundToInt(value);
			Current.Game.GetComponent<StabilityInfo>().overallStability = (OverallStability)stability;
		}

		public static float StabilityAsFloat() {
			return (float)Current.Game.GetComponent<StabilityInfo>().overallStability;
		}

		public static Rect LabelRect(float start_y) {
			return new Rect(0.0f, start_y, 200f, 30f);
		}
		public static Rect SliderRect(float start_y, float width) {
			return new Rect(200f, start_y, width, 30f);
		}

		public static TaggedString LabelText() {
			return "PlanetStability".Translate();
		}
		public static string SliderTextLow() {
			return "PlanetStability_Low".Translate();
		}
		public static string SliderTextNormal() {
			return "PlanetStability_Normal".Translate();
		}
		public static string SliderTextHigh() {
			return "PlanetStability_High".Translate();
		}

		public static float AdjustedStabilityEnum() {
			return OverallStabilityUtility.EnumValuesCount - 1;
		}
	}
}
