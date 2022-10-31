using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace TemperatureOverhaul {
	class StabilityInfo : GameComponent {
		public OverallStability overallStability = OverallStability.VeryExtreme;
		public StabilityInfo() {
			overallStability = OverallStability.VeryExtreme;
		}
		public StabilityInfo(Game _) {
			overallStability = OverallStability.VeryExtreme;
		}
		public override void FinalizeInit() {
			var latitudeCurve = OverallStabilityUtility.GetLatitudeCurve(this.overallStability);
			Log.Message($"Latitude patch succeeded? {latitudeCurve != null}");
			typeof(WorldGenStep_Terrain).GetField("AvgTempByLatitudeCurve", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
				.SetValue(null, latitudeCurve);

			var seasonCurve = OverallStabilityUtility.GetSeasonCurve(this.overallStability);
			typeof(TemperatureTuning).GetField("SeasonalTempVariationCurve", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
				.SetValue(null, seasonCurve);
			Log.Message($"Seasonal patch succeeded? {seasonCurve != null}");
		}
		public override void ExposeData() {
			Scribe_Values.Look<OverallStability>(ref this.overallStability, "overallStability");
			BackCompatibility.PostExposeData((object)this);
		}
	}
}
