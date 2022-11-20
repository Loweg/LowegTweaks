using System.Reflection;
using RimWorld.Planet;
using Verse;

namespace LowegTweaks.TemperatureOverhaul {
	class StabilityInfo : GameComponent {
		public OverallStability overallStability = OverallStability.Extreme;
		public StabilityInfo() {}
		public StabilityInfo(Game _) {}
		public override void FinalizeInit() {
			var latitudeCurve = OverallStabilityUtility.GetLatitudeCurve(this.overallStability);
			typeof(WorldGenStep_Terrain).GetField("AvgTempByLatitudeCurve", BindingFlags.Static | BindingFlags.NonPublic)
				.SetValue(null, latitudeCurve);

			var seasonCurve = OverallStabilityUtility.GetSeasonCurve(this.overallStability);
			typeof(TemperatureTuning).GetField("SeasonalTempVariationCurve", BindingFlags.Static | BindingFlags.Public)
				.SetValue(null, seasonCurve);
		}
		public override void ExposeData() {
			Scribe_Values.Look<OverallStability>(ref this.overallStability, "overallStability");
			BackCompatibility.PostExposeData((object)this);
		}
	}
}
