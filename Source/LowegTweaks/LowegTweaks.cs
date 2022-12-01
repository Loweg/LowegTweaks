using UnityEngine;
using Verse;

namespace LowegTweaks {
	public class LowegTweaks : Mod {
		static LowegTweaks() {}
		public LowegTweaks(ModContentPack content) : base(content) {}

		public static void ApplySettings() {
			Log.Message("Applying Tweak settings");
			var settings = LoadedModManager.GetMod<LowegTweaks>().GetSettings<Settings>();
			Log.Message($"Settings: {settings}");
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
}
