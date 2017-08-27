using Harmony;
using RimWorld.Planet;
using Verse;

namespace ExtremeColds
{
    // Abstract this later.
    public class WorldGenStepComponent : IExposable
    {
        //public static ExtremeColdsSettings settings;
        int versionNumber; // this is the version used for this given world (instance)

        public WorldGenStepComponent() { }

        public WorldGenStepComponent(LookMode mode, object[] ctorArgs) { }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.versionNumber, "versionNumber", 0, true);
        }

        public void StartedNewGame()
        {
            ExtremeColdsMod.settings.currentVersion = ExtremeColdsMod.settings.selectedVersion;
            this.versionNumber = ExtremeColdsMod.settings.selectedVersion;
        }

        public void StartedLoadGame()
        {
            ExtremeColdsMod.settings.currentVersion = ExtremeColdsMod.settings.selectedVersion;
            if (this.versionNumber != ExtremeColdsMod.settings.CurrentRelease) 
            {
                Log.Message("Older version detected: " + versionNumber);
                if (this.versionNumber != ExtremeColdsMod.settings.selectedVersion)
                {
                    Log.Message($"Discrepancy between saved version and selected.\nSelected: {ExtremeColdsMod.settings.selectedVersion}\nSaved: {this.versionNumber}\nDefauling to saved value.");
                    ExtremeColdsMod.settings.currentVersion = this.versionNumber;
                }
            }
        }
    }

    public static class WorldGenStepComponentHelper
    {
        private static WorldGenStepComponent wgsc = new WorldGenStepComponent();

        public static void ExposeData()
        {
            Scribe_Deep.Look<WorldGenStepComponent>(ref wgsc, "worldGenStepComponent", LookMode.Deep, new object[0]);
        }

        public static void StartedNewGame()
        {
            wgsc.StartedNewGame();
        }

        public static void StartedLoadGame()
        {
            // NOTE: sometimes null...
            wgsc?.StartedLoadGame();
        }
    }

    [StaticConstructorOnStartup]
    class BackwardsCompatibilityPatches
    {
        static BackwardsCompatibilityPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.extremecolds.backwardscompatibility");

            harmony.Patch(AccessTools.Method(typeof(Game), nameof(Game.ExposeData)), null, new HarmonyMethod(typeof(BackwardsCompatibilityPatches), nameof(ExposeData)));
            harmony.Patch(AccessTools.Method(typeof(World), nameof(World.ExposeData)), new HarmonyMethod(typeof(BackwardsCompatibilityPatches), nameof(ExposeData)), null);

            harmony.Patch(AccessTools.Method(typeof(WorldGenStep_Terrain), nameof(WorldGenStep_Terrain.GenerateFresh)), new HarmonyMethod(typeof(BackwardsCompatibilityPatches), nameof(StartedNewGame)), null);
            harmony.Patch(AccessTools.Method(typeof(WorldGenStep_Terrain), nameof(WorldGenStep_Terrain.GenerateFromScribe)), new HarmonyMethod(typeof(BackwardsCompatibilityPatches), nameof(StartedLoadGame)), null);
        }

        public static void ExposeData()
        {
            WorldGenStepComponentHelper.ExposeData();
        }

        public static void StartedNewGame()
        {
            WorldGenStepComponentHelper.StartedNewGame();
        }

        public static void StartedLoadGame()
        {
            WorldGenStepComponentHelper.StartedLoadGame();
        }
    }

}
