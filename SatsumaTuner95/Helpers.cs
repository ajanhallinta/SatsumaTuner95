using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker.Actions;

namespace SatsumaTuner95
{
    public class Helpers
    {
        public static void SetTransmission(Drivetrain drivetrain, Drivetrain.Transmissions transmission)
        {
            drivetrain.transmission = transmission;
            drivetrain.SetTransmission(transmission);
        }

        public static void SetGearRatio(Drivetrain drivetrain, int gear, float ratio)
        {
            if (drivetrain == null || gear < 0 || gear > drivetrain.gearRatios.Length)
            {
                SatsumaTuner95.DebugPrint("Error when trying to set gear " + gear + " with ratio " + ratio + ".");
                return;
            }
            drivetrain.gearRatios[gear] = ratio;
        }

        public static void SetGearRatios(List<float> newGearRatios, Drivetrain drivetrain)
        {
            if (newGearRatios == null || newGearRatios.Count == 0)
            {
                SatsumaTuner95.DebugPrint("Error; tried to set non-existent gear ratios!");
                return;
            }
            drivetrain.gearRatios = newGearRatios.ToArray();
        }

        public static bool HasGearRatiosBeenModified(List<float> originalGearRatios, Drivetrain drivetrain)
        {
            return !Enumerable.SequenceEqual(originalGearRatios, drivetrain.gearRatios);
        }

        public static void AddGear(Drivetrain drivetrain, bool first)
        {
            List<float> gears = drivetrain.gearRatios.ToList();
            if (first)
            {
                float newGear = gears.FirstOrDefault();
                gears.Insert(0, newGear);
            }
            else
            {
                float newGear = gears.LastOrDefault();
                gears.Add(newGear);
            }
            drivetrain.gearRatios = gears.ToArray();
        }

        public static void RemoveGear(Drivetrain drivetrain, int index)
        {
            List<float> gears = drivetrain.gearRatios.ToList();
            if (index < gears.Count)
            {
                gears.RemoveAt(index);
                drivetrain.gearRatios = gears.ToArray();
            }
        }

        public static FloatOperator GetFloatOperatorFromFSM(PlayMakerFSM fsm, string actionName, int actionIndex)
        {
            try
            {
                return (FloatOperator)fsm.FsmStates.Where(x => x.Name == actionName).FirstOrDefault().Actions[actionIndex];
            }
            catch
            {
                return null;
            }
        }

        public static FloatClamp GetFloatClampFromFSM(PlayMakerFSM fsm, string actionName, int actionIndex)
        {
            try
            {
                return (FloatClamp)fsm.FsmStates.Where(x => x.Name == actionName).FirstOrDefault().Actions[actionIndex];
            }
            catch
            {
                return null;
            }
        }

        public static SetProperty GetSetPropertyFromFSM(PlayMakerFSM fsm, string actionName, int actionIndex)
        {
            try
            {
                return (SetProperty)fsm.FsmStates.Where(x => x.Name == actionName).FirstOrDefault().Actions[actionIndex];
            }
            catch
            {
                return null;
            }
        }


        private static string donateString = "It would be cool to be a millionare just by making MSC mods.";

        public static string GetRandomDonateString()
        {
            string[] randomDonateStrings =
        {
            "It would be cool to be a millionare just by making MSC mods.",
"Override money multiplier on my bank account.",
"If you don't know where to spend your money, you can always give them to me."
        };
            try
            {
                string random = randomDonateStrings[UnityEngine.Random.Range(0, randomDonateStrings.Length)];
                if (!string.IsNullOrEmpty(random))
                    return random;
            }
            catch
            {
                return donateString;
            }
            return donateString;
        }
    }
}
