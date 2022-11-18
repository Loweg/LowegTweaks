using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace TemperatureOverhaul {
	class StabilityInfo : GameComponent {
		public OverallStability overallStability = OverallStability.Extreme;
		public StabilityInfo() {
			overallStability = OverallStability.Extreme;
		}
		public StabilityInfo(Game _) {
			overallStability = OverallStability.Extreme;
		}
		public override void FinalizeInit() {
			var latitudeCurve = OverallStabilityUtility.GetLatitudeCurve(this.overallStability);
			typeof(WorldGenStep_Terrain).GetField("AvgTempByLatitudeCurve", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
				.SetValue(null, latitudeCurve);

			var seasonCurve = OverallStabilityUtility.GetSeasonCurve(this.overallStability);
			typeof(TemperatureTuning).GetField("SeasonalTempVariationCurve", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
				.SetValue(null, seasonCurve);
			Log.Message($"Temperature Overhaul patched? {seasonCurve != null && latitudeCurve != null} (Stability: {this.overallStability})");
		}
		public override void ExposeData() {
			Scribe_Values.Look<OverallStability>(ref this.overallStability, "overallStability");
			BackCompatibility.PostExposeData((object)this);
		}
	}
}
