using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
    
namespace ExtremeColds
{
    public static class OverallTemperatureUtility
    {
        private static int cachedEnumValuesCount = -1;

        private static readonly SimpleCurve Curve_VeryCold = new SimpleCurve
        {
            {
                new CurvePoint(-9999f, -9999f),
                true
            },
            {
                new CurvePoint(-100f, -125f),
                true
            },
            {
                new CurvePoint(-90f, -110f),
                true
            },
            {
                new CurvePoint(-50f, -85f),
                true
            },
            {
                new CurvePoint(-30f, -78f),
                true
            },
            {
                new CurvePoint(-25f, -68f),
                true
            },
            {
                new CurvePoint(-20f, -58.5f),
                true
            },
            {
                new CurvePoint(0f, -57f),
                true
            }
        };

        private static readonly SimpleCurve Curve_Cold = new SimpleCurve
        {
            {
                new CurvePoint(-9999f, -9999f),
                true
            },
            {
                new CurvePoint(-50f, -70f),
                true
            },
            {
                new CurvePoint(-25f, -40f),
                true
            },
            {
                new CurvePoint(-20f, -25f),
                true
            },
            {
                new CurvePoint(-13f, -15f),
                true
            },
            {
                new CurvePoint(0f, -12f),
                true
            },
            {
                new CurvePoint(30f, -3f),
                true
            },
            {
                new CurvePoint(60f, 25f),
                true
            }
        };

        private static readonly SimpleCurve Curve_LittleBitColder = new SimpleCurve
        {
            {
                new CurvePoint(-9999f, -9999f),
                true
            },
            {
                new CurvePoint(-20f, -22f),
                true
            },
            {
                new CurvePoint(-15f, -15f),
                true
            },
            {
                new CurvePoint(-5f, -13f),
                true
            },
            {
                new CurvePoint(40f, 30f),
                true
            },
            {
                new CurvePoint(9999f, 9999f),
                true
            }
        };

        private static readonly SimpleCurve Curve_LittleBitWarmer = new SimpleCurve
        {
            {
                new CurvePoint(-9999f, -9999f),
                true
            },
            {
                new CurvePoint(-45f, -35f),
                true
            },
            {
                new CurvePoint(40f, 50f),
                true
            },
            {
                new CurvePoint(120f, 120f),
                true
            },
            {
                new CurvePoint(9999f, 9999f),
                true
            }
        };

        private static readonly SimpleCurve Curve_Hot = new SimpleCurve
        {
            {
                new CurvePoint(-45f, -22f),
                true
            },
            {
                new CurvePoint(-25f, -12f),
                true
            },
            {
                new CurvePoint(-22f, 2f),
                true
            },
            {
                new CurvePoint(-10f, 25f),
                true
            },
            {
                new CurvePoint(40f, 57f),
                true
            },
            {
                new CurvePoint(120f, 120f),
                true
            },
            {
                new CurvePoint(9999f, 9999f),
                true
            }
        };

        private static readonly SimpleCurve Curve_VeryHot = new SimpleCurve
        {
            {
                new CurvePoint(-45f, 25f),
                true
            },
            {
                new CurvePoint(0f, 40f),
                true
            },
            {
                new CurvePoint(33f, 80f),
                true
            },
            {
                new CurvePoint(40f, 88f),
                true
            },
            {
                new CurvePoint(120f, 120f),
                true
            },
            {
                new CurvePoint(9999f, 9999f),
                true
            }
        };

        public static int EnumValuesCount
        {
            get
            {
                if (OverallTemperatureUtility.cachedEnumValuesCount < 0)
                {
                    OverallTemperatureUtility.cachedEnumValuesCount = Enum.GetNames(typeof(OverallTemperature)).Length;
                }
                return OverallTemperatureUtility.cachedEnumValuesCount;
            }
        }

        public static SimpleCurve GetTemperatureCurve(this OverallTemperature overallTemperature)
        {
            switch (overallTemperature)
            {
                case OverallTemperature.VeryCold:
                    return OverallTemperatureUtility.Curve_VeryCold;
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

    [StaticConstructorOnStartup]
    class OverallTemperatureUtilityPatches
    {
        static OverallTemperatureUtilityPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.why_is_that.seasonal_weather.overall_temp_utility");

            harmony.Patch(AccessTools.Method(typeof(Page_CreateWorldParams), nameof(Window.DoWindowContents)), null, null, new HarmonyMethod(typeof(OverallTemperatureUtilityPatches), nameof(DoWindowContentsTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(WorldGenStep_Terrain), "GenerateTileFor"), null, null, new HarmonyMethod(typeof(OverallTemperatureUtilityPatches), nameof(GenerateTileForTranspiler)));
        }

        public static IEnumerable<CodeInstruction> DoWindowContentsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo oldGetEnumValuesCount = AccessTools.Property(typeof(RimWorld.Planet.OverallTemperatureUtility), nameof(RimWorld.Planet.OverallTemperatureUtility.EnumValuesCount)).GetGetMethod();
            MethodInfo newGetEnumValuesCount = AccessTools.Property(typeof(OverallTemperatureUtility), nameof(OverallTemperatureUtility.EnumValuesCount)).GetGetMethod();

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand == oldGetEnumValuesCount)
                    yield return new CodeInstruction(OpCodes.Call, newGetEnumValuesCount);
                else
                    yield return instruction;
            }
        }


        public static IEnumerable<CodeInstruction> GenerateTileForTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo oldGetTemperatureCurve = AccessTools.Method(typeof(RimWorld.Planet.OverallTemperatureUtility), nameof(RimWorld.Planet.OverallTemperatureUtility.GetTemperatureCurve));
            MethodInfo newGetTemperatureCurve = AccessTools.Method(typeof(OverallTemperatureUtility), nameof(OverallTemperatureUtility.GetTemperatureCurve));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand == oldGetTemperatureCurve)
                    yield return new CodeInstruction(OpCodes.Call, newGetTemperatureCurve);
                else
                    yield return instruction;
            }
        }
    }
}
