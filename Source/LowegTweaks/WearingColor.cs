using RimWorld;
using UnityEngine;
using Verse;

namespace LowegTweaks {
    public class ThoughtWorker_WearingAnyColor : ThoughtWorker {
        public const float RequiredMinPercentage = 0.6f;

        protected override ThoughtState CurrentStateInternal(Pawn p) {
			if (p.DevelopmentalStage.Baby()) return false;
            Color? fave_color = p.story.favoriteColor?.color;
			Color? ideo_color = p.Ideo?.ApparelColor;
            if (!fave_color.HasValue || !ideo_color.HasValue) {
                return false;
            }
            int matches = 0;
            foreach (Apparel item in p.apparel.WornApparel) {
                CompColorable comp = item.TryGetComp<CompColorable>();
                if (comp != null && comp.Active && (comp.Color.IndistinguishableFrom(fave_color.Value) || comp.Color.IndistinguishableFrom(ideo_color.Value))) {
                    matches++;
                }
            }
            return (float)matches / (float)p.apparel.WornApparelCount >= 0.6f;
        }
    }
}

