using UnityEngine;
using SewerMenu.Features.Base;

namespace SewerMenu.UI.Pages
{
    /// <summary>
    /// Base class for all menu pages.
    /// Uses SewerSkin for consistent styling and IL2CPP-safe rendering.
    /// </summary>
    public abstract class PageBase : IPage
    {
        public abstract string Title { get; }
        public abstract FeatureCategory Category { get; }
        public bool IsInitialized { get; private set; }
        
        public virtual void Initialize()
        {
            IsInitialized = true;
        }
        
        public virtual void Shutdown()
        {
            IsInitialized = false;
        }
        
        public virtual void Draw()
        {
            try
            {
                // Page title with accent color
                SewerSkin.DrawHeader("═══ " + Title.ToUpper() + " ═══");
                
                // Draw page content
                DrawContent();
            }
            catch (System.Exception ex)
            {
                // Show error in a styled way
                SewerSkin.DrawStatus("Page Error: " + ex.Message, SewerSkin.StatusType.Error);
            }
        }
        
        protected abstract void DrawContent();
        
        /// <summary>
        /// Draws all features in this category with styled toggles.
        /// </summary>
        protected void DrawAllFeatures()
        {
            var features = FeatureManager.Instance.GetFeaturesByCategory(Category);
            
            foreach (var feature in features)
            {
                if (feature.IsToggleable)
                {
                    bool newValue = SewerSkin.DrawToggle(feature.Name, feature.IsEnabled, feature.Description);
                    if (newValue != feature.IsEnabled)
                    {
                        feature.IsEnabled = newValue;
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (SewerSkin.DrawButton(feature.Name, 150))
                    {
                        feature.Execute();
                    }
                    GUILayout.Label(feature.Description);
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.Space(2);
            }
        }
        
        /// <summary>
        /// Draws a section header.
        /// </summary>
        protected void DrawSection(string title)
        {
            SewerSkin.DrawSection(title);
        }
        
        /// <summary>
        /// Draw a toggle feature.
        /// </summary>
        protected bool DrawToggle(string name, bool currentValue, string description = null)
        {
            return SewerSkin.DrawToggle(name, currentValue, description);
        }
        
        /// <summary>
        /// Draw a slider.
        /// </summary>
        protected float DrawSlider(string label, float value, float min, float max, string format = "F1")
        {
            return SewerSkin.DrawSlider(label, value, min, max, format);
        }
        
        /// <summary>
        /// Draw a button.
        /// </summary>
        protected bool DrawButton(string text, float width = 0)
        {
            return SewerSkin.DrawButton(text, width);
        }
        
        /// <summary>
        /// Draw info line.
        /// </summary>
        protected void DrawInfo(string key, string value)
        {
            SewerSkin.DrawInfo(key, value);
        }
        
        /// <summary>
        /// Draw a quantity selector with preset buttons (IL2CPP-safe).
        /// </summary>
        protected int DrawQuantitySelector(string label, int currentValue, int[] presets)
        {
            return SewerSkin.DrawQuantitySelector(label, currentValue, presets);
        }
        
        /// <summary>
        /// Draw a numeric input with +/- buttons (IL2CPP-safe).
        /// </summary>
        protected float DrawNumericInput(string label, float value, float step, float[] presets = null)
        {
            return SewerSkin.DrawNumericInput(label, value, step, presets);
        }
    }
}
