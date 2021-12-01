using UnityEngine;
using HutongGames.PlayMaker;

namespace SatsumaTuner95
{
    public class GUIFields
    {
        const float IncreaseButtonWidth = 50;
        const float TextFieldWidth = 100;

        public static float IncreaseAmount = 0.01f; // for + and - buttons

        #region FsmFloatField
        [System.Serializable]
        public class FsmFloatField
        {
            public string DisplayName; // Name to display in GUI
            public FsmFloat FloatVariable; // The actual modifiable variable
            public string ValueString; // Value string in Textfield
            public float OriginalValue; // Original value for restoring defaults

            public static FsmFloatField CreateFsmFloatField(FsmFloat fsmFloat, string displayName = "")
            {
                FsmFloatField field = new FsmFloatField();
                field.FloatVariable = fsmFloat;
                field.ValueString = fsmFloat.Value.ToString();
                field.DisplayName = string.IsNullOrEmpty(displayName) ? fsmFloat.Name : displayName;
                field.OriginalValue = fsmFloat.Value;
                return field;
            }

            public static void DrawFsmFloatField(FsmFloatField fsmFloatField, bool addIncreaseDecreaseButtons = false, bool printCurrentValue = false)
            {
                bool valueHasChanged = false; // Parses new value from TextField, when set to true.

                GUILayout.BeginHorizontal();
                GUILayout.Label(fsmFloatField.DisplayName + ": " + (printCurrentValue ? fsmFloatField.FloatVariable.Value.ToString() : ""));

                // Check for enter input
                if (Event.current.Equals(Event.KeyboardEvent("return")))
                    valueHasChanged = true;

                // Value TextField
                fsmFloatField.ValueString = GUILayout.TextField(fsmFloatField.ValueString, GUILayout.Width(TextFieldWidth));

                // Show + and - buttons, if set
                if (addIncreaseDecreaseButtons)
                {
                    if (GUILayout.Button("-", GUILayout.Width(IncreaseButtonWidth)))
                        fsmFloatField.IncreaseValue(-IncreaseAmount);
                    if (GUILayout.Button("+", GUILayout.Width(IncreaseButtonWidth)))
                        fsmFloatField.IncreaseValue(IncreaseAmount);
                }

                // Check for enter input
                if (!valueHasChanged)
                    valueHasChanged = Event.current.Equals(Event.KeyboardEvent("return"));

                // Parse and Apply new value from TextField
                if (valueHasChanged)
                {
                    float newValue = fsmFloatField.FloatVariable.Value;
                    if (float.TryParse(fsmFloatField.ValueString, out newValue))
                    {
                        fsmFloatField.UpdateNewValue(newValue);
                    }
                }

                GUILayout.EndHorizontal();
            }

            public void IncreaseValue(float amount)
            {
                UpdateNewValue(FloatVariable.Value += amount);
            }
            public void UpdateNewValue(float newValue, bool skipZeroValue = false)
            {
                if (skipZeroValue && newValue == 0) // (optional) prevent assigning zero values (ie. uninitialized suspension savedata)
                    return;

                FloatVariable.Value = newValue;
                ValueString = FloatVariable.Value.ToString();
            }
            public void RestoreValue()
            {
                UpdateNewValue(OriginalValue);
            }
        }
#endregion

        #region FloatField
        [System.Serializable]
        public class FloatField
        {
            public string DisplayName; // Name to display in GUI
            public float FloatVariable; // The actual modifiable variable/value
            public string ValueString; // Value string in Textfield
            public float OriginalValue; // Original value for restoring defaults

            public static FloatField CreateFloatField(float floatVariable, string displayName = "")
            {
                FloatField field = new FloatField();
                field.FloatVariable = floatVariable;
                field.ValueString = floatVariable.ToString();
                field.DisplayName = displayName;
                field.OriginalValue = floatVariable;
                return field;
            }

            /// <summary>
            /// Draws Label and TextField for modifiable float variable, with some optional GUI elements.
            /// </summary>
            /// <returns>true if value was modified.</returns>
            public static bool DrawFloatField(FloatField floatField, bool addIncreaseDecreaseButtons = false, bool dontShowDisplayName = false, bool dontBeginHorizontal = false, float textFieldWidth = 0)
            {
                float initialValue = floatField.FloatVariable;
                bool valueHasChanged = false; // Parses new value from TextField, when set to true.

                if (!dontBeginHorizontal)
                    GUILayout.BeginHorizontal();
                if (!dontShowDisplayName)
                    GUILayout.Label(floatField.DisplayName + ": ");

                // Check for enter input
                if (Event.current.Equals(Event.KeyboardEvent("return")))
                    valueHasChanged = true;

                // Value TextField
                floatField.ValueString = GUILayout.TextField(floatField.ValueString, GUILayout.Width(textFieldWidth != 0 ? textFieldWidth : TextFieldWidth));

                // Show + and - buttons, if set
                if (addIncreaseDecreaseButtons)
                {
                    if (GUILayout.Button("-" ,GUILayout.Width(IncreaseButtonWidth)))
                        floatField.IncreaseValue(-IncreaseAmount);
                    if (GUILayout.Button("+", GUILayout.Width(IncreaseButtonWidth)))
                        floatField.IncreaseValue(IncreaseAmount);
                }

                // Check for enter input
                if (!valueHasChanged)
                    valueHasChanged = Event.current.Equals(Event.KeyboardEvent("return"));

                // Parse and Apply new value from TextField
                if (valueHasChanged)
                {
                    float newValue = floatField.FloatVariable;
                    if (float.TryParse(floatField.ValueString, out newValue))
                    {
                        floatField.UpdateNewValue(newValue);
                    }
                }

                if (!dontBeginHorizontal)
                    GUILayout.EndHorizontal();

                return initialValue != floatField.FloatVariable;
            }

            public void IncreaseValue(float amount)
            {
                UpdateNewValue(FloatVariable += amount);
            }
            public void UpdateNewValue(float newValue)
            {
                FloatVariable = newValue;
                ValueString = FloatVariable.ToString();
            }
            public void RestoreValue()
            {
                UpdateNewValue(OriginalValue);
            }
        }
        #endregion
    }
}
