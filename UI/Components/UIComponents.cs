using System;
using UnityEngine;

namespace SewerMenu.UI.Components
{
    /// <summary>
    /// Static class providing reusable UI components.
    /// All components are styled according to the SewerMenu theme.
    /// </summary>
    public static class UIComponents
    {
        #region Toggle
        
        /// <summary>
        /// Draws a styled toggle switch.
        /// </summary>
        public static bool Toggle(bool value, string label = null, float width = -1)
        {
            GUILayout.BeginHorizontal();
            
            if (!string.IsNullOrEmpty(label))
            {
                GUILayout.Label(label, Styles.ToggleLabel, GUILayout.ExpandWidth(true));
            }
            
            // Draw custom toggle
            var toggleRect = GUILayoutUtility.GetRect(Theme.ToggleWidth, Theme.ToggleHeight);
            
            // Background
            Color bgColor = value ? Theme.ToggleOn : Theme.ToggleOff;
            Styles.DrawColoredBox(toggleRect, bgColor);
            
            // Knob
            float knobSize = Theme.ToggleHeight - 4;
            float knobX = value ? toggleRect.xMax - knobSize - 2 : toggleRect.x + 2;
            var knobRect = new Rect(knobX, toggleRect.y + 2, knobSize, knobSize);
            Styles.DrawColoredBox(knobRect, Theme.ToggleKnob);
            
            // Handle click
            if (Event.current.type == EventType.MouseDown && toggleRect.Contains(Event.current.mousePosition))
            {
                value = !value;
                Event.current.Use();
                GUI.changed = true;
            }
            
            GUILayout.EndHorizontal();
            
            return value;
        }
        
        /// <summary>
        /// Draws a toggle with a label on the left.
        /// </summary>
        public static bool LabeledToggle(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Styles.Label, GUILayout.ExpandWidth(true));
            value = Toggle(value);
            GUILayout.EndHorizontal();
            return value;
        }
        
        #endregion
        
        #region Slider
        
        /// <summary>
        /// Draws a styled horizontal slider.
        /// </summary>
        public static float Slider(float value, float min, float max, string format = "F1", string label = null)
        {
            GUILayout.BeginHorizontal();
            
            if (!string.IsNullOrEmpty(label))
            {
                GUILayout.Label(label, Styles.Label, GUILayout.Width(100));
            }
            
            // Slider
            value = GUILayout.HorizontalSlider(value, min, max, Styles.SliderBackground, Styles.SliderThumb, GUILayout.ExpandWidth(true), GUILayout.Height(Theme.SliderHeight));
            
            // Value display
            GUILayout.Label(value.ToString(format), Styles.SliderLabel, GUILayout.Width(50));
            
            GUILayout.EndHorizontal();
            
            return value;
        }
        
        /// <summary>
        /// Draws a slider with a label and value display.
        /// </summary>
        public static float LabeledSlider(string label, float value, float min, float max, string format = "F1", string suffix = "")
        {
            GUILayout.BeginVertical();
            
            // Label row
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Styles.Label);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{value.ToString(format)}{suffix}", Styles.LabelMuted);
            GUILayout.EndHorizontal();
            
            // Slider
            value = GUILayout.HorizontalSlider(value, min, max, Styles.SliderBackground, Styles.SliderThumb, GUILayout.Height(Theme.SliderHeight));
            
            GUILayout.EndVertical();
            
            return value;
        }
        
        /// <summary>
        /// Draws an integer slider.
        /// </summary>
        public static int IntSlider(string label, int value, int min, int max)
        {
            return Mathf.RoundToInt(LabeledSlider(label, value, min, max, "F0"));
        }
        
        #endregion
        
        #region Button
        
        /// <summary>
        /// Draws a styled button.
        /// </summary>
        public static bool Button(string text, float width = -1)
        {
            if (width > 0)
            {
                return GUILayout.Button(text, Styles.Button, GUILayout.Width(width));
            }
            return GUILayout.Button(text, Styles.Button);
        }
        
        /// <summary>
        /// Draws a small button.
        /// </summary>
        public static bool SmallButton(string text, float width = -1)
        {
            if (width > 0)
            {
                return GUILayout.Button(text, Styles.ButtonSmall, GUILayout.Width(width));
            }
            return GUILayout.Button(text, Styles.ButtonSmall);
        }
        
        /// <summary>
        /// Draws a primary (highlighted) button.
        /// </summary>
        public static bool PrimaryButton(string text, float width = -1)
        {
            if (width > 0)
            {
                return GUILayout.Button(text, Styles.ButtonPrimary, GUILayout.Width(width));
            }
            return GUILayout.Button(text, Styles.ButtonPrimary);
        }
        
        /// <summary>
        /// Draws a danger (red) button.
        /// </summary>
        public static bool DangerButton(string text, float width = -1)
        {
            if (width > 0)
            {
                return GUILayout.Button(text, Styles.ButtonDanger, GUILayout.Width(width));
            }
            return GUILayout.Button(text, Styles.ButtonDanger);
        }
        
        /// <summary>
        /// Draws a row of buttons.
        /// </summary>
        public static int ButtonRow(params string[] labels)
        {
            int clicked = -1;
            
            GUILayout.BeginHorizontal();
            for (int i = 0; i < labels.Length; i++)
            {
                if (GUILayout.Button(labels[i], Styles.Button))
                {
                    clicked = i;
                }
            }
            GUILayout.EndHorizontal();
            
            return clicked;
        }
        
        #endregion
        
        #region Input Field
        
        /// <summary>
        /// Draws a text input field.
        /// </summary>
        public static string TextField(string value, string placeholder = "", float width = -1)
        {
            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(placeholder))
            {
                GUI.color = Theme.TextMuted;
            }
            
            string displayValue = string.IsNullOrEmpty(value) ? placeholder : value;
            
            string result;
            if (width > 0)
            {
                result = GUILayout.TextField(displayValue, Styles.TextField, GUILayout.Width(width));
            }
            else
            {
                result = GUILayout.TextField(displayValue, Styles.TextField);
            }
            
            GUI.color = Color.white;
            
            // Clear placeholder if user starts typing
            if (result == placeholder && Event.current.type == EventType.KeyDown)
            {
                result = "";
            }
            
            return result == placeholder ? "" : result;
        }
        
        /// <summary>
        /// Draws a labeled text field.
        /// </summary>
        public static string LabeledTextField(string label, string value, string placeholder = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Styles.Label, GUILayout.Width(100));
            value = TextField(value, placeholder);
            GUILayout.EndHorizontal();
            return value;
        }
        
        /// <summary>
        /// Draws a number input field.
        /// </summary>
        public static float NumberField(float value, float min = float.MinValue, float max = float.MaxValue)
        {
            string text = GUILayout.TextField(value.ToString("F2"), Styles.TextField, GUILayout.Width(80));
            
            if (float.TryParse(text, out float result))
            {
                return Mathf.Clamp(result, min, max);
            }
            
            return value;
        }
        
        /// <summary>
        /// Draws an integer input field.
        /// </summary>
        public static int IntField(int value, int min = int.MinValue, int max = int.MaxValue)
        {
            string text = GUILayout.TextField(value.ToString(), Styles.TextField, GUILayout.Width(80));
            
            if (int.TryParse(text, out int result))
            {
                return Mathf.Clamp(result, min, max);
            }
            
            return value;
        }
        
        #endregion
        
        #region Dropdown
        
        private static int _activeDropdown = -1;
        private static Vector2 _dropdownScroll;
        
        /// <summary>
        /// Draws a dropdown selection.
        /// </summary>
        public static int Dropdown(int selectedIndex, string[] options, int dropdownId, float width = 150)
        {
            string selectedText = selectedIndex >= 0 && selectedIndex < options.Length 
                ? options[selectedIndex] 
                : "Select...";
            
            var buttonRect = GUILayoutUtility.GetRect(width, Theme.InputHeight);
            
            // Draw button
            if (GUI.Button(buttonRect, selectedText + " v", Styles.Button))
            {
                _activeDropdown = _activeDropdown == dropdownId ? -1 : dropdownId;
            }
            
            // Draw dropdown if active
            if (_activeDropdown == dropdownId)
            {
                float dropdownHeight = Mathf.Min(options.Length * 25, 200);
                var dropdownRect = new Rect(buttonRect.x, buttonRect.yMax, width, dropdownHeight);
                
                GUI.Box(dropdownRect, "", Styles.Box);
                
                GUILayout.BeginArea(dropdownRect);
                _dropdownScroll = GUILayout.BeginScrollView(_dropdownScroll);
                
                for (int i = 0; i < options.Length; i++)
                {
                    if (GUILayout.Button(options[i], i == selectedIndex ? Styles.ButtonPrimary : Styles.Button))
                    {
                        selectedIndex = i;
                        _activeDropdown = -1;
                    }
                }
                
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            
            return selectedIndex;
        }
        
        #endregion
        
        #region Section
        
        /// <summary>
        /// Begins a collapsible section.
        /// </summary>
        public static bool BeginSection(string title, bool expanded)
        {
            GUILayout.BeginVertical(Styles.Section);
            
            GUILayout.BeginHorizontal();
            
            string arrow = expanded ? "v" : ">";
            if (GUILayout.Button($"{arrow} {title}", Styles.SectionHeader))
            {
                expanded = !expanded;
            }
            
            GUILayout.EndHorizontal();
            
            return expanded;
        }
        
        /// <summary>
        /// Ends a section.
        /// </summary>
        public static void EndSection()
        {
            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draws a simple section header.
        /// </summary>
        public static void SectionHeader(string title)
        {
            GUILayout.Label(title, Styles.SectionHeader);
            Styles.DrawSeparator();
        }
        
        #endregion
        
        #region Info Display
        
        /// <summary>
        /// Draws an info box.
        /// </summary>
        public static void InfoBox(string message)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Theme.Info;
            GUILayout.Box(message, Styles.Box);
            GUI.backgroundColor = oldColor;
        }
        
        /// <summary>
        /// Draws a warning box.
        /// </summary>
        public static void WarningBox(string message)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Theme.Warning;
            GUILayout.Box(message, Styles.Box);
            GUI.backgroundColor = oldColor;
        }
        
        /// <summary>
        /// Draws an error box.
        /// </summary>
        public static void ErrorBox(string message)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Theme.Error;
            GUILayout.Box(message, Styles.Box);
            GUI.backgroundColor = oldColor;
        }
        
        /// <summary>
        /// Draws a key-value pair.
        /// </summary>
        public static void KeyValue(string key, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(key, Styles.Label, GUILayout.Width(120));
            GUILayout.Label(value, Styles.LabelMuted);
            GUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Spacing
        
        /// <summary>
        /// Adds vertical space.
        /// </summary>
        public static void Space(float pixels = -1)
        {
            GUILayout.Space(pixels > 0 ? pixels : Theme.ItemSpacing);
        }
        
        /// <summary>
        /// Adds a horizontal line separator.
        /// </summary>
        public static void Separator()
        {
            Styles.DrawSeparator();
        }
        
        #endregion
    }
}
