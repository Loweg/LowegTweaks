using System;
using Verse;

namespace LowegTweaks.TemperatureOverhaul {
	public enum OverallStability {
		VeryExtreme,
		Extreme,
		LittleBitExtreme,
		Normal,
		LittleBitStable,
		Stable,
		VeryStable,
	}

	public static class OverallStabilityUtility {
		// Latitude
		private static readonly SimpleCurve Latitude_VeryExtreme = new SimpleCurve() {
			{new CurvePoint(0.0f, 60f),  true},
			{new CurvePoint(0.1f, 50f),  true},
			{new CurvePoint(0.5f, 7f),   true},
			{new CurvePoint(1f,   -74f), true},
		};
		private static readonly SimpleCurve Latitude_Extreme = new SimpleCurve() {
			{new CurvePoint(0.0f, 45f),  true},
			{new CurvePoint(0.1f, 42f),  true},
			{new CurvePoint(0.5f, 7f),   true},
			{new CurvePoint(1f,   -56f), true},
		};
		private static readonly SimpleCurve Latitude_LittleBitExtreme = new SimpleCurve() {
			{new CurvePoint(0.0f, 36f),  true},
			{new CurvePoint(0.1f, 34f),  true},
			{new CurvePoint(0.5f, 7f),   true},
			{new CurvePoint(1f,   -44f), true},
		};
		private static readonly SimpleCurve Latitude_Normal = new SimpleCurve() {
			{new CurvePoint(0.0f, 30f),  true},
			{new CurvePoint(0.1f, 29f),  true},
			{new CurvePoint(0.5f, 7f),   true},
			{new CurvePoint(1f,   -37f), true},
		};
		private static readonly SimpleCurve Latitude_LittleBitStable = new SimpleCurve() {
			{new CurvePoint(0.0f, 30f),  true},
			{new CurvePoint(0.1f, 29f),  true},
			{new CurvePoint(0.5f, 13f),   true},
			{new CurvePoint(1f,   -30f), true},
		};
		private static readonly SimpleCurve Latitude_Stable = new SimpleCurve() {
			{new CurvePoint(0.0f, 30f),  true},
			{new CurvePoint(0.1f, 29f),  true},
			{new CurvePoint(0.5f, 15f),   true},
			{new CurvePoint(1f,   -26f), true},
		};
		private static readonly SimpleCurve Latitude_VeryStable = new SimpleCurve() {
			{new CurvePoint(0.0f, 30f),  true},
			{new CurvePoint(0.1f, 28f),  true},
			{new CurvePoint(0.5f, 17f),   true},
			{new CurvePoint(1f,   -19f), true},
		};

		// Season
		private static readonly SimpleCurve Season_VeryExtreme = new SimpleCurve() {
			{new CurvePoint(0.0f, 10f),  true},
			{new CurvePoint(0.1f, 18f),  true},
			{new CurvePoint(1f,   90f), true},
		};
		private static readonly SimpleCurve Season_Extreme = new SimpleCurve() {
			{new CurvePoint(0.0f, 4f),  true},
			{new CurvePoint(0.1f, 6f),  true},
			{new CurvePoint(1f,   42f), true},
		};
		private static readonly SimpleCurve Season_LittleBitExtreme = new SimpleCurve() {
			{new CurvePoint(0.0f, 3f),  true},
			{new CurvePoint(0.1f, 5f),  true},
			{new CurvePoint(1f,   34f), true},
		};
		private static readonly SimpleCurve Season_Normal = new SimpleCurve() {
			{new CurvePoint(0.0f, 3f),  true},
			{new CurvePoint(0.1f, 4f),  true},
			{new CurvePoint(1f,   28f), true},
		};
		private static readonly SimpleCurve Season_LittleBitStable = new SimpleCurve() {
			{new CurvePoint(0.0f, 3f),  true},
			{new CurvePoint(0.1f, 4f),  true},
			{new CurvePoint(1f,   25f), true},
		};
		private static readonly SimpleCurve Season_Stable = new SimpleCurve() {
			{new CurvePoint(0.0f, 2f),  true},
			{new CurvePoint(0.1f, 3f),  true},
			{new CurvePoint(1f,   19f), true},
		};
		private static readonly SimpleCurve Season_VeryStable = new SimpleCurve() {
			{new CurvePoint(0.0f, 1f),  true},
			{new CurvePoint(0.1f, 2f),  true},
			{new CurvePoint(1f,   11f), true},
		};

		public static readonly SimpleCurve RainfallStabilityEffect = new SimpleCurve() {
			{new CurvePoint(7000f, 0.01f), true},
			{new CurvePoint(2000f, 0.5f),  true},
			{new CurvePoint(1300f, 0.9f),  true},
			{new CurvePoint(800f,  1.1f),  true},
			{new CurvePoint(500f,  1.5f),  true},
			{new CurvePoint(0f,    2.8f),  true},
		};


		public static float GetScaleFactor(this OverallStability overallStability) {
			switch (overallStability) {
				case OverallStability.VeryExtreme:
					return 2f;
				case OverallStability.Extreme:
					return 1.5f;
				case OverallStability.LittleBitExtreme:
					return 1.2f;
				case OverallStability.LittleBitStable:
					return 0.9f;
				case OverallStability.Stable:
					return 0.7f;
				case OverallStability.VeryStable:
					return 0.4f;
				default:
					return 1f;
			}
		}

		public static SimpleCurve GetLatitudeCurve(this OverallStability overallStability) {
			switch (overallStability) {
				case OverallStability.VeryExtreme:
					return OverallStabilityUtility.Latitude_VeryExtreme;
				case OverallStability.Extreme:
					return OverallStabilityUtility.Latitude_Extreme;
				case OverallStability.LittleBitExtreme:
					return OverallStabilityUtility.Latitude_LittleBitExtreme;
				case OverallStability.LittleBitStable:
					return OverallStabilityUtility.Latitude_LittleBitStable;
				case OverallStability.Stable:
					return OverallStabilityUtility.Latitude_Stable;
				case OverallStability.VeryStable:
					return OverallStabilityUtility.Latitude_VeryStable;
				default:
					return OverallStabilityUtility.Latitude_Normal;
			}
		}

		public static SimpleCurve GetSeasonCurve(this OverallStability overallStability) {
			switch (overallStability) {
				case OverallStability.VeryExtreme:
					return OverallStabilityUtility.Season_VeryExtreme;
				case OverallStability.Extreme:
					return OverallStabilityUtility.Season_Extreme;
				case OverallStability.LittleBitExtreme:
					return OverallStabilityUtility.Season_LittleBitExtreme;
				case OverallStability.LittleBitStable:
					return OverallStabilityUtility.Season_LittleBitStable;
				case OverallStability.Stable:
					return OverallStabilityUtility.Season_Stable;
				case OverallStability.VeryStable:
					return OverallStabilityUtility.Season_VeryStable;
				default:
					return OverallStabilityUtility.Season_Normal;
			}
		}

		private static int cachedEnumValuesCount = -1;
		public static int EnumValuesCount {
			get {
				if (OverallStabilityUtility.cachedEnumValuesCount < 0)
					OverallStabilityUtility.cachedEnumValuesCount = Enum.GetNames(typeof(OverallStability)).Length;
				return OverallStabilityUtility.cachedEnumValuesCount;
			}
		}
	}
}