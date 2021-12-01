using UnityEngine;
using HutongGames.PlayMaker;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker.Actions;

namespace SatsumaTuner95
{
    public class Tuner : MonoBehaviour
    {
        public static Tuner Instance;

        // sub-menu toggles
        private bool guiShowPower = false;
        private bool guiShowTransmission = false;
        private bool guiShowGears = false;
        private bool guiShowWheels = false;
        private bool guiShowSuspension = false;
        private bool guiShowDrivingAssistance = false;

        // satsuma components
        private Transform satsuma;
        private Drivetrain drivetrain;
        private AxisCarController axisCarController;
        private PlayMakerFSM suspension;
        private PlayMakerFSM boostFsm;
        private PlayMakerFSM revLimiterFsm;

        // satsuma wheel transforms
        private Transform wheelFL;
        private Transform wheelFR;
        private Transform wheelRL;
        private Transform wheelRR;

        // various FSM State Actions to disable, when using power multiplier override
        private FloatClamp boostPowerFloatClamp;
        private SetProperty waitPlayerSetProperty;
        private FloatClamp revLimiterFloatClamp;
        private SetProperty revLimiterSetProperty;
        private FloatOperator revLimiterFloatOperator;
        private SetProperty boostSetMultiplierSetProperty2;

        // variable fields wheelpos
        private GUIFields.FsmFloatField wheelPosLong;
        private GUIFields.FsmFloatField wheelPosRally;
        private GUIFields.FsmFloatField wheelPosStock;

        // variable fields suspension properties
        // travel
        private GUIFields.FsmFloatField travelLong;
        private GUIFields.FsmFloatField travelRally;
        private GUIFields.FsmFloatField travelStock;

        // rates
        private GUIFields.FsmFloatField rallyFrontRate;
        private GUIFields.FsmFloatField stockFrontRate;
        private GUIFields.FsmFloatField longRearRate;
        private GUIFields.FsmFloatField rallyRearRate;
        private GUIFields.FsmFloatField stockRearRate;
        // bump and rebound
        private GUIFields.FsmFloatField rallyFrontLBump;
        private GUIFields.FsmFloatField rallyFrontLRebound;
        private GUIFields.FsmFloatField rallyFrontRBump;
        private GUIFields.FsmFloatField rallyFrontRRebound;
        private GUIFields.FsmFloatField rallyRearLBump;
        private GUIFields.FsmFloatField rallyRearLRebound;
        private GUIFields.FsmFloatField rallyRearRBump;
        private GUIFields.FsmFloatField rallyRearRRebound;

        private GUIFields.FsmFloatField stockFrontBump;
        private GUIFields.FsmFloatField stockFrontRebound;
        private GUIFields.FsmFloatField stockRearBump;
        private GUIFields.FsmFloatField stockRearRebound;

        // variable fields misc
        private GUIFields.FloatField powerMultiplierOverride;
        private GUIFields.FloatField tcsAllowedSlip;
        private GUIFields.FloatField tcsMinVelocity;
        private GUIFields.FloatField absAllowedSlip;
        private GUIFields.FloatField absMinVelocity;
        private GUIFields.FloatField espStrength;
        private GUIFields.FloatField espMinVelocity;
        private List<GUIFields.FloatField> gearRatios = new List<GUIFields.FloatField>();
        private List<float> defaultGearRatios = new List<float>();

        // custom wheel offset settings and FloatFields
        private bool useCustomWheelOffsets = false;
        private GUIFields.FloatField frontWheelsOffsetX, rearWheelsOffsetX;
        private float frontWheelOffsetX, rearWheelOffsetX;
        private Vector3 originalFlPosition, originalFrPosition, originalRlPosition, originalRrPosition;

        // increase amount field
        private float increaseAmount = 0.01f; // for + and - buttons
        private GUIFields.FloatField increaseAmountField;

        // misc
        private bool hasInitialized = false;
        private bool isEnabled = false;
        private bool useCustomPowerMultiplier = false;

        // gui positions
        Rect windowRect = new Rect(20, 20, 425, 150);
        Vector2 scrollPos = Vector2.zero;

        public void Initialize()
        {
            // Get references
            satsuma = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("TheCar").Value.transform;
            drivetrain = satsuma.GetComponent<Drivetrain>();
            axisCarController = satsuma.GetComponent<AxisCarController>();
            // TODO: failsafes
            suspension = satsuma.Find("CarSimulation/Car/Suspension").GetComponent<PlayMakerFSM>();
            revLimiterFsm = satsuma.Find("CarSimulation/Engine/RevLimiter").GetComponent<PlayMakerFSM>();

            // Get wheel transforms
            wheelFL = satsuma.Find("FL");
            wheelFR = satsuma.Find("FR");
            wheelRL = satsuma.Find("RL");
            wheelRR = satsuma.Find("RR");

            // Store wheel transform default localPositions for restoring later
            if (wheelFL)
                originalFlPosition = wheelFL.transform.localPosition;
            if (wheelFR)
                originalFrPosition = wheelFR.transform.localPosition;
            if (wheelRL)
                originalRlPosition = wheelRL.transform.localPosition;
            if (wheelRR)
                originalRrPosition = wheelRR.transform.localPosition;

            try
            {
                boostFsm = satsuma.Find("CarSimulation/Engine/N2O").GetComponent<PlayMakerFSM>();
            }
            catch
            {
                SatsumaTuner95.DebugPrint("Error when trying to get powerMultiplier!");
            }

            // Create GUI variable fields
            powerMultiplierOverride = GUIFields.FloatField.CreateFloatField(1.0f, "Override Power Multiplier");
            tcsAllowedSlip = GUIFields.FloatField.CreateFloatField(axisCarController.TCSAllowedSlip, "TCS Allowed Slip");
            tcsMinVelocity = GUIFields.FloatField.CreateFloatField(axisCarController.TCSAllowedSlip, "TCS Min Velocity");
            espStrength = GUIFields.FloatField.CreateFloatField(axisCarController.ESPStrength, "ESP Strength");
            espMinVelocity = GUIFields.FloatField.CreateFloatField(axisCarController.ESPMinVelocity, "ESP Min Velocity");
            absAllowedSlip = GUIFields.FloatField.CreateFloatField(axisCarController.ABSAllowedSlip, "ABS Allowed Slip");
            absMinVelocity = GUIFields.FloatField.CreateFloatField(axisCarController.ABSMinVelocity, "ABS Min Velocity");
            wheelPosLong = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "WheelPosLong"));
            wheelPosRally = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "WheelPosRally"));
            wheelPosStock = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "WheelPosStock"));

            frontWheelsOffsetX = GUIFields.FloatField.CreateFloatField(frontWheelOffsetX, "Front Wheels Offset X");
            rearWheelsOffsetX = GUIFields.FloatField.CreateFloatField(rearWheelOffsetX, "Rear Wheels Offset Y");

            // travel
            travelLong = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "TravelLong"));
            travelRally = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "TravelRally"));
            travelStock = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "TravelStock"));

            // rates
            rallyFrontRate = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyFrontRate"));
            stockFrontRate = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "StockFrontRate"));
            longRearRate = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "LongRearRate"));
            rallyRearRate = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyRearRate"));
            stockRearRate = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "StockRearRate"));

            // bump and rebound
            rallyFrontLBump = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyFrontLBump"));
            rallyFrontLRebound = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyFrontLRebound"));
            rallyFrontRBump = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyFrontRBump"));
            rallyFrontRRebound = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyFrontRRebound"));
            rallyRearLBump = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyRearLBump"));
            rallyRearLRebound = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyRearLRebound"));
            rallyRearRBump = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyRearRBump"));
            rallyRearRRebound = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "RallyRearRRebound"));

            stockFrontBump = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "StockFrontBump"));
            stockFrontRebound = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "StockFrontRebound"));
            stockRearBump = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "StockRearBump"));
            stockRearRebound = GUIFields.FsmFloatField.CreateFsmFloatField(MSCLoader.PlayMakerExtensions.GetVariable<FsmFloat>(suspension, "StockRearRebound"));

            // Store default gear ratios and create FloatFields
            if (drivetrain)
            {
                defaultGearRatios = drivetrain.gearRatios.ToList();
                CreateGearRatioFloatFields();
            }

            increaseAmountField = GUIFields.FloatField.CreateFloatField(increaseAmount, "+/- ");

            // Wait Satsuma to be Active before getting variables
            StartCoroutine(LateInitialize());

            hasInitialized = true;
        }

        // Wait Satsuma to be Active before getting variables
        private IEnumerator LateInitialize()
        {
            while (boostFsm == null || revLimiterFsm == null || !boostFsm.Active || !revLimiterFsm.Active)
                yield return null;

            boostPowerFloatClamp = Helpers.GetFloatClampFromFSM(boostFsm, "Boost", 5);
            boostSetMultiplierSetProperty2 = Helpers.GetSetPropertyFromFSM(boostFsm, "Boost", 18);
            waitPlayerSetProperty = Helpers.GetSetPropertyFromFSM(boostFsm, "Wait player", 4);
            revLimiterSetProperty = Helpers.GetSetPropertyFromFSM(revLimiterFsm, "Normal revs", 1);
            revLimiterFloatClamp = Helpers.GetFloatClampFromFSM(revLimiterFsm, "Valve float", 5);
            revLimiterFloatOperator = Helpers.GetFloatOperatorFromFSM(revLimiterFsm, "Valve float", 4);
        }

        private void CreateGearRatioFloatFields()
        {
            gearRatios.Clear();

            if (drivetrain)
            {
                for (int i = 0; i < drivetrain.gearRatios.Length; i++)
                {
                    gearRatios.Add(GUIFields.FloatField.CreateFloatField(drivetrain.gearRatios[i], "Gear " + i));
                }
            }
        }

        private bool wasPowerMultiplierHackActive = false;
        private bool wasWheelOffsetModActive = false;
        private void Update()
        {
            // power multiplier override settings was changed, toggle hack
            if (boostFsm && boostFsm.Active && wasPowerMultiplierHackActive != useCustomPowerMultiplier)
            {
                TogglePowerMultiplierOverrideHack();
                wasPowerMultiplierHackActive = useCustomPowerMultiplier;
            }

            // update power multiplier override
            if (useCustomPowerMultiplier && boostFsm && boostFsm.Active)
                UpdatePowerMultiplierHack();

            // wheel x offset hack
            if (useCustomWheelOffsets)
                UpdateCustomWheelOffsets();

            // wheel offset settings has changed, reset to defaults if needed
            if (wasWheelOffsetModActive != useCustomWheelOffsets)
            {
                if (!useCustomWheelOffsets)
                    ResetWheelOffsetX();
                wasWheelOffsetModActive = useCustomWheelOffsets;
            }

            // toggle GUI
            if (SatsumaTuner95.ActivateKey.GetKeybindDown())
                isEnabled = !isEnabled;
        }

        // Disable or Enable powerMultiplier related FSM State Actions from various Satsuma FSMs
        private void TogglePowerMultiplierOverrideHack()
        {
            bool value = !useCustomPowerMultiplier;
            boostPowerFloatClamp.Enabled = value;
            boostSetMultiplierSetProperty2.Enabled = value;
            waitPlayerSetProperty.Enabled = value;
            revLimiterFloatClamp.Enabled = value;
            revLimiterSetProperty.Enabled = value;
            revLimiterFloatOperator.Enabled = value;
            drivetrain.powerMultiplier = powerMultiplierOverride.FloatVariable;
        }

        private void UpdateCustomWheelOffsets()
        {
            if (wheelFL && wheelFR)
            {
                if (wheelFL)
                    wheelFL.transform.localPosition = originalFlPosition + new Vector3(-frontWheelOffsetX, 0, 0);
                if (wheelFR)
                    wheelFR.transform.localPosition = originalFrPosition + new Vector3(frontWheelOffsetX, 0, 0);
            }

            if (wheelRL && wheelRR)
            {
                if (wheelRL)
                    wheelRL.transform.localPosition = originalRlPosition + new Vector3(-rearWheelOffsetX, 0, 0);
                if (wheelRR)
                    wheelRR.transform.localPosition = originalRrPosition + new Vector3(rearWheelOffsetX, 0, 0);
            }
        }
        private void UpdatePowerMultiplierHack()
        {
            // prevent NaN
            if (powerMultiplierOverride.FloatVariable < 0.1f)
                powerMultiplierOverride.FloatVariable = 0.1f;
            drivetrain.powerMultiplier = powerMultiplierOverride.FloatVariable;
        }

        private void ResetWheelOffsetX()
        {
            if (wheelFL)
                wheelFL.transform.localPosition = new Vector3(originalFlPosition.x, wheelFL.localPosition.y, wheelFL.localPosition.z);
            if (wheelFR)
                wheelFR.transform.localPosition = new Vector3(originalFrPosition.x, wheelFR.localPosition.y, wheelFR.localPosition.z);
            if (wheelRL)
                wheelRL.transform.localPosition = new Vector3(originalRlPosition.x, wheelRL.localPosition.y, wheelRL.localPosition.z);
            if (wheelRR)
                wheelRR.transform.localPosition = new Vector3(originalRrPosition.x, wheelRR.localPosition.y, wheelRR.localPosition.z);

            frontWheelsOffsetX.FloatVariable = 0;
            rearWheelsOffsetX.FloatVariable = 0;
            frontWheelsOffsetX.RestoreValue();
            rearWheelsOffsetX.RestoreValue();
        }

        private void ResetSuspensionValues(int suspension = -1) // -1: All, 0: STOCK, 1: RALLY, 2: LONG
        {
            bool resetStock = (suspension == 0 || suspension == -1);
            bool resetRally = (suspension == 1 || suspension == -1);
            bool resetLong = (suspension == 2 || suspension == -1);

            if(resetStock)
            {
                // travel
                travelStock.RestoreValue();

                // rate
                stockFrontRate.RestoreValue();
                stockRearRate.RestoreValue();

                // bump and rebound
                stockFrontBump.RestoreValue();
                stockFrontRebound.RestoreValue();
                stockRearBump.RestoreValue();
                stockRearRebound.RestoreValue();
            }

            if(resetRally)
            {
                // travel
                travelRally.RestoreValue();

                // rate
                rallyFrontRate.RestoreValue();
                rallyRearRate.RestoreValue();

                // bump and rebound
                rallyFrontLBump.RestoreValue();
                rallyFrontLRebound.RestoreValue();
                rallyFrontRBump.RestoreValue();
                rallyFrontRRebound.RestoreValue();
                rallyRearLBump.RestoreValue();
                rallyRearLRebound.RestoreValue();
                rallyRearRBump.RestoreValue();
                rallyRearRRebound.RestoreValue();
            }

            if(resetLong)
            {
                // travel
                travelLong.RestoreValue();
                // rate
                longRearRate.RestoreValue();
            }
        }

        #region GUI
        private void OnGUI()
        {
            if (!isEnabled || !satsuma || !hasInitialized)
                return;
            windowRect = GUILayout.Window(0, windowRect, TunerGUI, "SatsumaTuner95" + SatsumaTuner95.VersionString, GUILayout.Width(405), GUILayout.Height(500));
        }

        private void TunerGUI(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            // Top bar
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
                SatsumaTuner95.SaveSaveDataToFile();
            if (GUILayout.Button("Load"))
                SatsumaTuner95.LoadSaveDataFromFile();
            if (GUILayout.Button("Restore Default Values"))
                RestoreDefaultValues();
            if (GUILayout.Button("Flip car") && satsuma)
                satsuma.transform.localEulerAngles = new Vector3(0, satsuma.transform.localEulerAngles.y, 0);
            if (GUIFields.FloatField.DrawFloatField(increaseAmountField))
                GUIFields.IncreaseAmount = increaseAmountField.FloatVariable;

            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            if (drivetrain != null)
            {
                // Power Multiplier Override
                if (GUILayout.Button("Power"))
                    guiShowPower = !guiShowPower;
                if (guiShowPower)
                    PowerGUI();

                // Transmission
                if (GUILayout.Button("Transmission"))
                    guiShowTransmission = !guiShowTransmission;
                if (guiShowTransmission)
                    TransmissionGUI();

                // Gears
                if (GUILayout.Button("Gears"))
                    guiShowGears = !guiShowGears;
                if (guiShowGears)
                    GearsGUI();
            }

            // Wheels
            if (GUILayout.Button("Wheels"))
                guiShowWheels = !guiShowWheels;
            if (guiShowWheels)
                WheelsGUI();

            // Suspension
            if (GUILayout.Button("Suspension"))
                guiShowSuspension = !guiShowSuspension;
            if (guiShowSuspension)
                SuspensionGUI();

            // Driving Assistance
            if (GUILayout.Button("Driving Assistance"))
                guiShowDrivingAssistance = !guiShowDrivingAssistance;
            if (guiShowDrivingAssistance)
                DrivingAssistanceGUI();

            GUILayout.EndScrollView();
        }

        private void DrivingAssistanceGUI()
        {
            GUILayout.BeginVertical("box");

            axisCarController.TCS = GUILayout.Toggle(axisCarController.TCS, "TCS");
            if (GUIFields.FloatField.DrawFloatField(tcsAllowedSlip))
                axisCarController.TCSAllowedSlip = tcsAllowedSlip.FloatVariable;
            if (GUIFields.FloatField.DrawFloatField(tcsMinVelocity))
                axisCarController.TCSMinVelocity = tcsMinVelocity.FloatVariable;
            axisCarController.ABS = GUILayout.Toggle(axisCarController.ABS, "ABS");
            if (GUIFields.FloatField.DrawFloatField(absAllowedSlip))
                axisCarController.ABSAllowedSlip = absAllowedSlip.FloatVariable;
            if (GUIFields.FloatField.DrawFloatField(absMinVelocity))
                axisCarController.ABSMinVelocity = absMinVelocity.FloatVariable;
            axisCarController.ESP = GUILayout.Toggle(axisCarController.ESP, "ESP");
            if (GUIFields.FloatField.DrawFloatField(espStrength))
                axisCarController.ESPStrength = espStrength.FloatVariable;
            if (GUIFields.FloatField.DrawFloatField(espMinVelocity))
                axisCarController.ESPMinVelocity = espMinVelocity.FloatVariable;

            GUILayout.EndVertical();
        }

        private void TransmissionGUI()
        {
            GUILayout.BeginVertical("box");

            // Automatic bools
            drivetrain.automatic = GUILayout.Toggle(drivetrain.automatic, "Automatic: " + drivetrain.automatic);
            drivetrain.autoReverse = GUILayout.Toggle(drivetrain.autoReverse, "Auto reverse: " + drivetrain.autoReverse);

            // Type of drive
            GUILayout.BeginHorizontal();
            GUILayout.Label("Type of drive: " + drivetrain.transmission);
            if (GUILayout.Button(Drivetrain.Transmissions.FWD.ToString(), GUILayout.Width(66.66f)))
                Helpers.SetTransmission(drivetrain, Drivetrain.Transmissions.FWD);
            if (GUILayout.Button(Drivetrain.Transmissions.RWD.ToString(), GUILayout.Width(66.66f)))
                Helpers.SetTransmission(drivetrain, Drivetrain.Transmissions.RWD);
            if (GUILayout.Button(Drivetrain.Transmissions.AWD.ToString(), GUILayout.Width(66.66f)))
                Helpers.SetTransmission(drivetrain, Drivetrain.Transmissions.AWD);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void WheelsGUI()
        {
            GUILayout.BeginVertical("box");

            // Suspension height (wheel position)
            GUIFields.FsmFloatField.DrawFsmFloatField(wheelPosStock, true, true);
            GUIFields.FsmFloatField.DrawFsmFloatField(wheelPosLong, true, true);
            GUIFields.FsmFloatField.DrawFsmFloatField(wheelPosRally, true, true);

            // Custom wheel offset X
            useCustomWheelOffsets = GUILayout.Toggle(useCustomWheelOffsets, "Use custom wheel offsets");
            GUIFields.FloatField.DrawFloatField(frontWheelsOffsetX, true);
            frontWheelOffsetX = frontWheelsOffsetX.FloatVariable;

            GUIFields.FloatField.DrawFloatField(rearWheelsOffsetX, true);
            rearWheelOffsetX = rearWheelsOffsetX.FloatVariable;

            GUILayout.EndVertical();
        }

        private int selectedSuspension = 0; // 0: STOCK, 1: RALLY, 2: LONG
        private void SuspensionGUI()
        {
            GUILayout.BeginVertical("box");

            // get selected suspension name
            string suspensionName = "Stock";
            if (selectedSuspension == 1)
                suspensionName = "Rally";
            else if (selectedSuspension == 2)
                suspensionName = "Long";

            // reset selected suspension
            GUILayout.BeginHorizontal();
            GUILayout.Label("Selected suspension: " + suspensionName);
            if (GUILayout.Button("Restore " + suspensionName + " values"))
                ResetSuspensionValues(selectedSuspension);
            GUILayout.EndHorizontal();

            // select suspension buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stock"))
                selectedSuspension = 0;
            if (GUILayout.Button("Rally"))
                selectedSuspension = 1;
            if (GUILayout.Button("Long"))
                selectedSuspension = 2;
            GUILayout.EndHorizontal();

            // draw selected suspension GUI
            GUILayout.BeginVertical("box");
            SelectedSuspensionGUI();
            GUILayout.EndVertical();

            // reset all suspensions
            if (GUILayout.Button("Restore all suspensions values"))
                ResetSuspensionValues(-1);

            GUILayout.EndVertical();
        }

        private void SelectedSuspensionGUI()
        {
            switch (selectedSuspension)
            {
                case 0: // STOCK
                    GUIFields.FsmFloatField.DrawFsmFloatField(travelStock, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(stockFrontRate, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(stockFrontBump, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(stockFrontRebound, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(stockRearRate, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(stockRearBump, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(stockRearRebound, true, true);
                    break;
                case 1: // RALLY
                    GUIFields.FsmFloatField.DrawFsmFloatField(travelRally, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyFrontRate, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyFrontRBump, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyFrontLBump, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyFrontRRebound, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyFrontLRebound, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyRearRate, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyRearRBump, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyRearLBump, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyRearRRebound, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(rallyRearLRebound, true, true);
                    break;
                case 2: // LONG
                    GUIFields.FsmFloatField.DrawFsmFloatField(travelLong, true, true);
                    GUIFields.FsmFloatField.DrawFsmFloatField(longRearRate, true, true);
                    break;
                default:
                    break;
            }
        }

        private void PowerGUI()
        {
            GUILayout.BeginVertical("box");

            // Power Multiplier (Override)
            string powerMultiplierString = "Power multiplier: (Satsuma is not running)";
            if (drivetrain && boostFsm != null && boostFsm.Active)
                powerMultiplierString = "Power multiplier: " + drivetrain.powerMultiplier;
            GUILayout.Label(powerMultiplierString);

            GUILayout.BeginHorizontal();
            useCustomPowerMultiplier = GUILayout.Toggle(useCustomPowerMultiplier, "Override power multiplier:");
            GUIFields.FloatField.DrawFloatField(powerMultiplierOverride, true, true, true);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void GearsGUI()
        {
            GUILayout.BeginVertical("box");

            GUILayout.Label("Total gears: " + gearRatios.Count);

            GUILayout.BeginVertical("box");
            // Gear ratios
            for (int i = 0; i < gearRatios.Count; i++)
            {
                GUILayout.BeginHorizontal();

                if (i < drivetrain.gearRatios.Length)
                {
                    GUILayout.Label("Gear " + i + ": " + drivetrain.gearRatios[i]);

                    if (GUIFields.FloatField.DrawFloatField(gearRatios[i], false, true, true, 100))
                        Helpers.SetGearRatio(drivetrain, i, gearRatios[i].FloatVariable);

                    if (GUILayout.Button("Remove", GUILayout.Width(100)))
                    {
                        Helpers.RemoveGear(drivetrain, i);
                        CreateGearRatioFloatFields();
                    }
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            // Add or reset gears
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add gear (top)"))
            {
                Helpers.AddGear(drivetrain, true);
                CreateGearRatioFloatFields();
            }
            if (GUILayout.Button("Add gear (bottom)"))
            {
                Helpers.AddGear(drivetrain, false);
                CreateGearRatioFloatFields();
            }
            if (GUILayout.Button("Reset gears"))
            {
                if (Helpers.HasGearRatiosBeenModified(defaultGearRatios, drivetrain))
                {
                    Helpers.SetGearRatios(defaultGearRatios, drivetrain);
                    CreateGearRatioFloatFields();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        #endregion

        #region Save, Load and Restore
        public SaveData CreateSaveData()
        {
            SaveData data = new SaveData();

            // wheelpos
            data.WheelPosLong = wheelPosLong.FloatVariable.Value;
            data.WheelPosRally = wheelPosRally.FloatVariable.Value;
            data.WheelPosStock = wheelPosStock.FloatVariable.Value;

            // wheels
            data.CustomWheelsOffsetEnabled = useCustomWheelOffsets;
            data.FrontWheelsOffsetX = frontWheelOffsetX;
            data.RearWheelsOffsetX = rearWheelOffsetX;

            // power multiplier
            data.PowerMultiplierOverride = powerMultiplierOverride.FloatVariable;
            data.PowerMultiplierOverrideEnabled = useCustomPowerMultiplier;

            if (drivetrain)
            {
                // transmission
                data.Automatic = drivetrain.automatic;
                data.AutoReverse = drivetrain.autoReverse;
                data.Transmission = drivetrain.transmission;

                // Save gear ratios only if they have been modified.
                if (Helpers.HasGearRatiosBeenModified(defaultGearRatios, drivetrain))
                    data.GearRatios = drivetrain.gearRatios.ToList();
            }

            // driving assistances
            if (axisCarController)
            {
                data.ABS = axisCarController.ABS;
                data.AbsAllowedSlip = axisCarController.ABSAllowedSlip;
                data.AbsMinVelocity = axisCarController.ABSMinVelocity;
                data.ESP = axisCarController.ESP;
                data.EspMinVelocity = axisCarController.ESPMinVelocity;
                data.EspStrength = axisCarController.ESPStrength;
                data.TCS = axisCarController.TCS;
                data.TcsAllowedSlip = axisCarController.TCSAllowedSlip;
                data.TcsMinVelocity = axisCarController.TCSMinVelocity;
            }

            // suspension
            // suspension travel
            data.TravelLong = travelLong.FloatVariable.Value;
            data.TravelRally = travelRally.FloatVariable.Value;
            data.TravelStock = travelStock.FloatVariable.Value;

            // suspension rates
            data.RallyFrontRate = rallyFrontRate.FloatVariable.Value;
            data.StockFrontRate = stockFrontRate.FloatVariable.Value;
            data.LongRearRate = longRearRate.FloatVariable.Value;
            data.RallyRearRate = rallyRearRate.FloatVariable.Value;
            data.StockRearRate = stockRearRate.FloatVariable.Value;

            // suspension bump and rebound
            data.RallyFrontLBump = rallyFrontLBump.FloatVariable.Value;
            data.RallyFrontLRebound = rallyFrontLRebound.FloatVariable.Value;
            data.RallyFrontRBump = rallyFrontRBump.FloatVariable.Value;
            data.RallyFrontRRebound = rallyFrontRRebound.FloatVariable.Value;
            data.RallyRearLBump = rallyRearLBump.FloatVariable.Value;
            data.RallyRearLRebound = rallyRearLRebound.FloatVariable.Value;
            data.RallyRearRBump = rallyRearRBump.FloatVariable.Value;
            data.RallyRearRRebound = rallyRearRRebound.FloatVariable.Value;

            data.StockFrontBump = stockFrontBump.FloatVariable.Value;
            data.StockFrontRebound = stockFrontRebound.FloatVariable.Value;
            data.StockRearBump = stockRearBump.FloatVariable.Value;
            data.StockRearRebound = stockRearRebound.FloatVariable.Value;

            SatsumaTuner95.DebugPrint("Created SaveData.");
            return data;
        }

        public void SetSaveData(SaveData saveData)
        {
            // power
            powerMultiplierOverride.UpdateNewValue(saveData.PowerMultiplierOverride);
            useCustomPowerMultiplier = saveData.PowerMultiplierOverrideEnabled;

            // transmission and gears
            if (drivetrain)
            {
                drivetrain.automatic = saveData.Automatic;
                drivetrain.autoReverse = saveData.AutoReverse;
                Helpers.SetTransmission(drivetrain, saveData.Transmission);
                if (saveData.GearRatios != null && saveData.GearRatios.Count > 0)
                    Helpers.SetGearRatios(saveData.GearRatios, drivetrain);
                else
                    Helpers.SetGearRatios(defaultGearRatios, drivetrain); // needed to reset gear ratios when they aren't present in savefile
                CreateGearRatioFloatFields();
            }

            // driving assistance
            if (axisCarController)
            {
                absAllowedSlip.UpdateNewValue(saveData.AbsAllowedSlip);
                absAllowedSlip.UpdateNewValue(saveData.AbsAllowedSlip);
                absMinVelocity.UpdateNewValue(saveData.AbsMinVelocity);
                espMinVelocity.UpdateNewValue(saveData.EspMinVelocity);
                espStrength.UpdateNewValue(saveData.EspStrength);
                tcsAllowedSlip.UpdateNewValue(saveData.TcsAllowedSlip);
                tcsMinVelocity.UpdateNewValue(saveData.TcsMinVelocity);

                axisCarController.ABS = saveData.ABS;
                axisCarController.ABSAllowedSlip = saveData.AbsAllowedSlip;
                axisCarController.ABSMinVelocity = saveData.AbsMinVelocity;
                axisCarController.ESP = saveData.ESP;
                axisCarController.ESPMinVelocity = saveData.EspMinVelocity;
                axisCarController.ESPStrength = saveData.EspStrength;
                axisCarController.TCS = saveData.TCS;
                axisCarController.TCSAllowedSlip = saveData.TcsAllowedSlip;
                axisCarController.TCSMinVelocity = saveData.TcsMinVelocity;
            }

            // wheels custom offset
            useCustomWheelOffsets = saveData.CustomWheelsOffsetEnabled;
            frontWheelOffsetX = saveData.FrontWheelsOffsetX;
            rearWheelOffsetX = saveData.RearWheelsOffsetX;
            frontWheelsOffsetX.UpdateNewValue(saveData.FrontWheelsOffsetX);
            rearWheelsOffsetX.UpdateNewValue(saveData.RearWheelsOffsetX);

            // wheelpos
            wheelPosLong.UpdateNewValue(saveData.WheelPosLong, true);
            wheelPosRally.UpdateNewValue(saveData.WheelPosRally, true);
            wheelPosStock.UpdateNewValue(saveData.WheelPosStock, true);

            // suspension
            // suspension travel
            travelLong.UpdateNewValue(saveData.TravelLong, true);
            travelRally.UpdateNewValue(saveData.TravelRally, true);
            travelStock.UpdateNewValue(saveData.TravelStock, true);

            // suspension rates
            rallyFrontRate.UpdateNewValue(saveData.RallyFrontRate, true);
            stockFrontRate.UpdateNewValue(saveData.StockFrontRate, true);
            longRearRate.UpdateNewValue(saveData.LongRearRate, true);
            rallyRearRate.UpdateNewValue(saveData.RallyRearRate, true);
            stockRearRate.UpdateNewValue(saveData.StockRearRate, true);

            // suspension bump and rebound
            rallyFrontLBump.UpdateNewValue(saveData.RallyFrontLBump, true);
            rallyFrontLRebound.UpdateNewValue(saveData.RallyFrontLRebound, true);
            rallyFrontRBump.UpdateNewValue(saveData.RallyFrontRBump, true);
            rallyFrontRRebound.UpdateNewValue(saveData.RallyFrontRRebound, true);
            rallyRearLBump.UpdateNewValue(saveData.RallyRearLBump, true);
            rallyRearLRebound.UpdateNewValue(saveData.RallyRearLRebound, true);
            rallyRearRBump.UpdateNewValue(saveData.RallyRearRBump, true);
            rallyRearRRebound.UpdateNewValue(saveData.RallyRearRRebound, true);

            stockFrontBump.UpdateNewValue(saveData.StockFrontBump, true);
            stockFrontRebound.UpdateNewValue(saveData.StockFrontRebound, true);
            stockRearBump.UpdateNewValue(saveData.StockRearBump, true);
            stockRearRebound.UpdateNewValue(saveData.StockRearRebound, true);

            SatsumaTuner95.DebugPrint("Applied SaveData.");
        }

        public void RestoreDefaultValues()
        {
            // wheel pos
            wheelPosLong.RestoreValue();
            wheelPosRally.RestoreValue();
            wheelPosStock.RestoreValue();

            // wheels
            if (useCustomWheelOffsets)
            {
                ResetWheelOffsetX();
            }
            useCustomWheelOffsets = false;
            frontWheelOffsetX = 0;
            frontWheelsOffsetX.UpdateNewValue(0);
            rearWheelOffsetX = 0;
            rearWheelsOffsetX.UpdateNewValue(0);

            // power
            powerMultiplierOverride.UpdateNewValue(1);
            useCustomPowerMultiplier = false;

            // transmission and gears
            if (drivetrain)
            {
                drivetrain.automatic = false;
                drivetrain.autoReverse = false;
                Helpers.SetTransmission(drivetrain, Drivetrain.Transmissions.FWD);
                if (Helpers.HasGearRatiosBeenModified(defaultGearRatios, drivetrain))
                    drivetrain.gearRatios = defaultGearRatios.ToArray();
                CreateGearRatioFloatFields();
            }

            // driving assistants
            if (axisCarController)
            {
                axisCarController.ABS = false;
                absAllowedSlip.RestoreValue();
                absMinVelocity.RestoreValue();
                axisCarController.ESP = false;
                espMinVelocity.RestoreValue();
                espStrength.RestoreValue();
                axisCarController.TCS = false;
                tcsAllowedSlip.RestoreValue();
                tcsMinVelocity.RestoreValue();
            }

            // suspension
            ResetSuspensionValues(-1);
            
            SatsumaTuner95.DebugPrint("Restored default values.");
        }
        #endregion
    }
}
