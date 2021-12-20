using MSCLoader;
using UnityEngine;
using System.IO;

namespace SatsumaTuner95
{
    public class SatsumaTuner95 : Mod
    {
        public override string ID => "SatsumaTuner95"; //Your mod ID (unique)
        public override string Name => "SatsumaTuner95"; //You mod name
        public override string Author => "ajanhallinta"; //Your Username
        public override string Version => "0.3"; //Version
        public override string Description => "A tuner mod for your precious Satsuma."; //Short description of your mod

        public static SatsumaTuner95 Instance;
        public static string VersionString = " (v0.3)";
        public static string SaveFilename = "satsuma_save.xml";
        public static Keybind ActivateKey;
        private static SettingsCheckBox saveTunerSettingsAutomatically;
        private static SettingsCheckBox loadTunerSettingsAutomatically;
        private static SettingsCheckBox debugLogCheckBox;

        public override void ModSetup()
        {
            Instance = this;
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.OnSave, Mod_OnSave);
        }

        // All Mod settings are created here. 
        public override void ModSettings()
        {
            ActivateKey = Keybind.Add(this, "activateKey", "Activate Key", KeyCode.F10);
            saveTunerSettingsAutomatically = Settings.AddCheckBox(this, "saveTunerAutomatically", "Save tuner settings when closing game", false);
            loadTunerSettingsAutomatically = Settings.AddCheckBox(this, "loadTunerAutomatically", "Load tuner settings when starting game", true);
            debugLogCheckBox = Settings.AddCheckBox(this, "debugLogCheckBox", "Enable Debug Log", false);
            Settings.AddHeader(this, "Support");
            Settings.AddText(this, Helpers.GetRandomDonateString());
            Settings.AddButton(this, "support", "PayPal", () =>
            {
                try
                {
                    Application.OpenURL("https://paypal.me/ajanhallinta");
                }
                catch
                {
                }
            });
        }

        // Called once, when mod is loading after game is fully loaded
        private void Mod_OnLoad()
        {
            VersionString = " (v" + Version + ")";

            if (Tuner.Instance)
            {
                GameObject.Destroy(Tuner.Instance);
                Tuner.Instance = null;
            }

            GameObject tunerGo = new GameObject();
            tunerGo.name = "SatsumaTuner95";
            Tuner.Instance = tunerGo.AddComponent<Tuner>();
            Tuner.Instance.Initialize();

            if (loadTunerSettingsAutomatically.GetValue())
                LoadSaveDataFromFile();
        }

        // Called once, when save and quit
        private void Mod_OnSave()
        {
            if (saveTunerSettingsAutomatically.GetValue())
                SaveSaveDataToFile();
        }

        public static void SaveSaveDataToFile()
        {
            if (Tuner.Instance == null || Instance == null)
            {
                DebugPrint("Error; Can't save SaveData, because of missing Instance(s)!");
                return;
            }

            SaveData data = Tuner.Instance.CreateSaveData();

            if (data == null)
            {
                DebugPrint("Error; can't save because SaveData is null!");
                return;
            }

            SaveLoad.SerializeSaveFile(Instance, data, SaveFilename);
            DebugPrint("Saved!");
        }

        public static void LoadSaveDataFromFile()
        {
            if (Tuner.Instance == null || Instance == null)
            {
                DebugPrint("Error; Can't load SaveData, because of missing Instance(s)!");
                return;
            }

            // check if savefile exists and load and apply if it does
            string filepath = Path.Combine(ModLoader.GetModSettingsFolder(Instance), SaveFilename);
            if (File.Exists(filepath))
            {
                SaveData data = SaveLoad.DeserializeSaveFile<SaveData>(Instance, SaveFilename);
                if (data == null)
                {
                    DebugPrint("Error; can't load save because SaveData is null!");
                    return;
                }
                Tuner.Instance.SetSaveData(data);
            }
            else
            {
                DebugPrint("No SaveData to load.");
            }
        }

        public static void DebugPrint(string msg)
        {
            if (!debugLogCheckBox.GetValue())
                return;

            ModConsole.Log("SatsumaTuner95: " + msg);
        }
    }
}
