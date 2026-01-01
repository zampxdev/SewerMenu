using System;
using System.Collections.Generic;
using UnityEngine;

namespace SewerMenu.UI
{
    /// <summary>
    /// Helper class for handling text input in Unity IMGUI with IL2CPP.
    /// Provides focus management and fallback input handling.
    /// </summary>
    public static class InputHelper
    {
        private static string _focusedControlName = "";
        private static int _controlCounter = 0;
        private static Dictionary<string, string> _controlValues = new Dictionary<string, string>();
        
        /// <summary>
        /// Resets the control counter. Call at the start of each OnGUI.
        /// </summary>
        public static void BeginFrame()
        {
            _controlCounter = 0;
        }
        
        /// <summary>
        /// Draws a text field with proper focus management.
        /// </summary>
        public static string TextField(string value, float width = 100, string controlId = null)
        {
            // Generate control name if not provided
            if (string.IsNullOrEmpty(controlId))
            {
                controlId = "SewerInput_" + _controlCounter++;
            }
            
            // Set the control name for focus management
            GUI.SetNextControlName(controlId);
            
            // Style the field
            var oldBg = GUI.backgroundColor;
            bool isFocused = GUI.GetNameOfFocusedControl() == controlId;
            
            if (isFocused)
            {
                GUI.backgroundColor = new Color(0.2f, 0.3f, 0.35f, 1f); // Highlight when focused
            }
            else
            {
                GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
            }
            
            // Draw the text field
            string result = GUILayout.TextField(value ?? "", GUILayout.Width(width), GUILayout.Height(24));
            
            GUI.backgroundColor = oldBg;
            
            // Handle click to focus
            if (Event.current.type == EventType.MouseDown)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (lastRect.Contains(Event.current.mousePosition))
                {
                    GUI.FocusControl(controlId);
                    _focusedControlName = controlId;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Draws an integer input with +/- buttons (more reliable than text input).
        /// </summary>
        public static int IntFieldWithButtons(string label, int value, int min = 1, int max = 9999, int step = 1)
        {
            GUILayout.BeginHorizontal();
            
            if (!string.IsNullOrEmpty(label))
            {
                GUILayout.Label(label, GUILayout.Width(50));
            }
            
            // Decrease button
            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(24)))
            {
                value = Mathf.Max(min, value - step);
            }
            
            // Value display/input
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
            
            string textValue = GUILayout.TextField(value.ToString(), GUILayout.Width(60), GUILayout.Height(24));
            if (int.TryParse(textValue, out int parsed))
            {
                value = Mathf.Clamp(parsed, min, max);
            }
            
            GUI.backgroundColor = oldBg;
            
            // Increase button
            if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(24)))
            {
                value = Mathf.Min(max, value + step);
            }
            
            GUILayout.EndHorizontal();
            
            return value;
        }
        
        /// <summary>
        /// Draws a number input with quick preset buttons.
        /// </summary>
        public static int NumberWithPresets(string label, int value, int[] presets, int min = 1, int max = 9999)
        {
            GUILayout.BeginHorizontal();
            
            if (!string.IsNullOrEmpty(label))
            {
                GUILayout.Label(label, GUILayout.Width(50));
            }
            
            // Current value with +/- 
            if (GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(24)))
            {
                value = Mathf.Max(min, value - 1);
            }
            
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
            string textValue = GUILayout.TextField(value.ToString(), GUILayout.Width(50), GUILayout.Height(24));
            if (int.TryParse(textValue, out int parsed))
            {
                value = Mathf.Clamp(parsed, min, max);
            }
            GUI.backgroundColor = oldBg;
            
            if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(24)))
            {
                value = Mathf.Min(max, value + 1);
            }
            
            GUILayout.Space(10);
            
            // Preset buttons
            foreach (int preset in presets)
            {
                if (GUILayout.Button(preset.ToString(), GUILayout.Width(40), GUILayout.Height(24)))
                {
                    value = preset;
                }
            }
            
            GUILayout.EndHorizontal();
            
            return value;
        }
        
        /// <summary>
        /// Draws a float input field.
        /// </summary>
        public static float FloatField(float value, float width = 80)
        {
            string controlId = "SewerFloat_" + _controlCounter++;
            GUI.SetNextControlName(controlId);
            
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
            
            string textValue = GUILayout.TextField(value.ToString("F0"), GUILayout.Width(width), GUILayout.Height(24));
            
            GUI.backgroundColor = oldBg;
            
            if (float.TryParse(textValue, out float parsed))
            {
                return parsed;
            }
            
            return value;
        }
        
        /// <summary>
        /// Clears focus from all controls.
        /// </summary>
        public static void ClearFocus()
        {
            GUI.FocusControl("");
            _focusedControlName = "";
        }
        
        /// <summary>
        /// Checks if any input field is currently focused.
        /// </summary>
        public static bool IsAnyFieldFocused()
        {
            string focused = GUI.GetNameOfFocusedControl();
            return !string.IsNullOrEmpty(focused) && focused.StartsWith("Sewer");
        }
    }
}
