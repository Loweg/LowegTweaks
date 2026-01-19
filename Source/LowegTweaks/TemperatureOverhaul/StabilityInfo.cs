using System.Reflection;
using RimWorld.Planet;
using Verse;

namespace LowegTweaks.TemperatureOverhaul {
	class StabilityInfo : GameComponent {
		public OverallStability overallStability = OverallStability.Normal;
		public StabilityInfo() {}
		public StabilityInfo(Game _) {}
		public override void ExposeData() {
			Scribe_Values.Look(ref overallStability, "overallStability");
			BackCompatibility.PostExposeData(this);
		}
	}
}
