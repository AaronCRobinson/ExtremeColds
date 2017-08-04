using Harmony;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
    
namespace ExtremeColds
{
    public static class OverallTemperatureUtility
    {
        private static readonly SimpleCurve Curve_VeryCold_V0 = new SimpleCurve
        {
            { new CurvePoint(-9999f, -9999f), true },
            { new CurvePoint(-100f, -125f), true },
            { new CurvePoint(-90f, -110f), true },
            { new CurvePoint(-50f, -85f), true },
            { new CurvePoint(-30f, -78f), true },
            { new CurvePoint(-25f, -68f), true },
            { new CurvePoint(-20f, -58.5f), true },
            { new CurvePoint(0f, -57f), true }
        };

        private static readonly SimpleCurve Curve_VeryCold_V1 = new SimpleCurve
        {
            { new CurvePoint(-9999f, -9999f), true },
            { new CurvePoint(-250f, -1500f), true },
            { new CurvePoint(-50f, -600f), true },
            { new CurvePoint(-10f, -30f), true }
        };

        #region vanilla curves (copy pasta)

        private static readonly SimpleCurve Curve_Cold = new SimpleCurve
        {
            { new CurvePoint(-9999f, -9999f), true },
            { new CurvePoint(-50f, -70f), true },
            { new CurvePoint(-25f, -40f), true },
            { new CurvePoint(-20f, -25f), true },
            { new CurvePoint(-13f, -15f), true },
            { new CurvePoint(0f, -12f), true },
            { new CurvePoint(30f, -3f), true },
            { new CurvePoint(60f, 25f), true }
        };

        private static readonly SimpleCurve Curve_LittleBitColder = new SimpleCurve
        {
            { new CurvePoint(-9999f, -9999f), true },
            { new CurvePoint(-20f, -22f), true },
            { new CurvePoint(-15f, -15f), true },
            { new CurvePoint(-5f, -13f), true },
            { new CurvePoint(40f, 30f), true },
            { new CurvePoint(9999f, 9999f), true }
        };

        private static readonly SimpleCurve Curve_LittleBitWarmer = new SimpleCurve
        {
            { new CurvePoint(-9999f, -9999f), true },
            { new CurvePoint(-45f, -35f), true },
            { new CurvePoint(40f, 50f), true },
            { new CurvePoint(120f, 120f), true },
            { new CurvePoint(9999f, 9999f), true }
        };

        private static readonly SimpleCurve Curve_Hot = new SimpleCurve
        {
            { new CurvePoint(-45f, -22f), true },
            { new CurvePoint(-25f, -12f), true },
            { new CurvePoint(-22f, 2f), true },
            { new CurvePoint(-10f, 25f), true },
            { new CurvePoint(40f, 57f), true },
            { new CurvePoint(120f, 120f), true },
            { new CurvePoint(9999f, 9999f), true }
        };

        private static readonly SimpleCurve Curve_VeryHot = new SimpleCurve
        {
            { new CurvePoint(-45f, 25f), true },
            { new CurvePoint(0f, 40f), true },
            { new CurvePoint(33f, 80f), true },
            { new CurvePoint(40f, 88f), true },
            { new CurvePoint(120f, 120f), true },
            { new CurvePoint(9999f, 9999f), true }
        };

        #endregion  

        public static SimpleCurve curve; // not sure if this needs to be scoped here or not...

        public static SimpleCurve GetTemperatureCurve(int versionNumber, OverallTemperature overallTemperature)
        {
            switch (versionNumber)
            {
                case 0:
                    curve = OverallTemperatureUtility.Curve_VeryCold_V0;
                    break;
                case 1:
                    curve = OverallTemperatureUtility.Curve_VeryCold_V1;
                    break;
                default:
                    curve = OverallTemperatureUtility.Curve_VeryCold_V1;
                    break;
            }

            switch (overallTemperature)
            {
                case OverallTemperature.VeryCold:
                    return curve;
                case OverallTemperature.Cold:
                    return OverallTemperatureUtility.Curve_Cold;
                case OverallTemperature.LittleBitColder:
                    return OverallTemperatureUtility.Curve_LittleBitColder;
                case OverallTemperature.LittleBitWarmer:
                    return OverallTemperatureUtility.Curve_LittleBitWarmer;
                case OverallTemperature.Hot:
                    return OverallTemperatureUtility.Curve_Hot;
                case OverallTemperature.VeryHot:
                    return OverallTemperatureUtility.Curve_VeryHot;
            }
            return null;
        }
    }

    // NOTE: couldn't get calls to work when calling Func directly, so using this helper
    public static class GetTemperatureCurve_DynamicFunctionHelper
    {
        public static ExtremeColdsSettings settings;

        public static SimpleCurve GetTemperatureCurveHelper(OverallTemperature overallTemperature)
        {
            return OverallTemperatureUtility.GetTemperatureCurve(settings.currentVersion, overallTemperature);
        }       
    }

    [StaticConstructorOnStartup]
    public class OverallTemperatureUtilityPatches
    {
        static OverallTemperatureUtilityPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.extremecolds.overalltemputility");

            harmony.Patch(AccessTools.Method(typeof(WorldGenStep_Terrain), "GenerateTileFor"), null, null, new HarmonyMethod(typeof(OverallTemperatureUtilityPatches), nameof(DynamicFunctionHelperTranspiler)));

            harmony.Patch(typeof(PawnGenerationRequest).GetConstructors()[0], null, null, new HarmonyMethod(typeof(OverallTemperatureUtilityPatches), nameof(ForceAddFreeWarmLayerTranspiler)));
        }

        static MethodInfo oldGetTemperatureCurve = AccessTools.Method(typeof(RimWorld.Planet.OverallTemperatureUtility), nameof(RimWorld.Planet.OverallTemperatureUtility.GetTemperatureCurve));
        static MethodInfo getTemperatureCurveHelperMethodInfo = AccessTools.Method(typeof(GetTemperatureCurve_DynamicFunctionHelper), nameof(GetTemperatureCurve_DynamicFunctionHelper.GetTemperatureCurveHelper));

        public static IEnumerable<CodeInstruction> DynamicFunctionHelperTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand == oldGetTemperatureCurve)
                    yield return new CodeInstruction(OpCodes.Call, getTemperatureCurveHelperMethodInfo);
                else
                    yield return instruction;
            }
        }

        public static IEnumerable<CodeInstruction> ForceAddFreeWarmLayerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldarg_S && instruction.operand.ToString() == "12")
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1); // true
                }
                else
                {
                    yield return instruction;
                }
            }
        }

    }
}
