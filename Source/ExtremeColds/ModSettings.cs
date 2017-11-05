using System.Collections.Generic;
using Verse;
using UnityEngine;
using SettingsHelper;

namespace ExtremeColds
{
    public class ExtremeColdsSettings : ModSettings
    {
        private const int currentRelease = 1;
        public int currentVersion; // sometimes need to force version without selection
        public int selectedVersion;

        public ExtremeColdsSettings() { this.selectedVersion = this.currentVersion = currentRelease; }

        public int CurrentRelease
        {
            get { return currentRelease; }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.selectedVersion, "versionNumber");
        }
    }

    class ExtremeColdsMod : Mod
    {
        public static ExtremeColdsSettings settings;

        private Dictionary<string, int> radioValues;

        public ExtremeColdsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ExtremeColdsSettings>();

            // TODO: find a better way to handle these radios...
            this.radioValues = new Dictionary<string, int>();
            for (int i = 0; i <= settings.CurrentRelease; i++)
                this.radioValues.Add(i.ToString(), i);
        }

        public override string SettingsCategory() => "ExtremeColds";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            DrawVersionNumberSelection(inRect);
            settings.Write();
        }

        private void DrawVersionNumberSelection(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);
            listing.AddLabeledRadioList<int>("The following radio values are allowed to set the current version of the Extreme Colds world generation.", this.radioValues, ref settings.selectedVersion);
            listing.AddLabelLine("NOTE: These values are only used during a new game. The value saved with the world will be used during load.");
            listing.End();
            settings.Write();
        }
    }

}
