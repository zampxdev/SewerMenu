using UnityEngine;

namespace SewerMenu.Features.Base
{
    /// <summary>
    /// Interface defining the contract for all SewerMenu features.
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Unique identifier for this feature.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Display name shown in the UI.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Description of what this feature does.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Category for UI organization.
        /// </summary>
        FeatureCategory Category { get; }
        
        /// <summary>
        /// Whether this feature is currently enabled.
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Whether this feature can be toggled on/off.
        /// Some features are action-only (like "Spawn Item").
        /// </summary>
        bool IsToggleable { get; }
        
        /// <summary>
        /// Optional hotkey for this feature.
        /// </summary>
        KeyCode? Hotkey { get; set; }
        
        /// <summary>
        /// Whether this feature requires host privileges in multiplayer.
        /// </summary>
        bool RequiresHost { get; }
        
        /// <summary>
        /// Called when the feature is first registered.
        /// Use for one-time setup.
        /// </summary>
        void OnRegister();
        
        /// <summary>
        /// Called when the feature is enabled.
        /// </summary>
        void OnEnable();
        
        /// <summary>
        /// Called when the feature is disabled.
        /// </summary>
        void OnDisable();
        
        /// <summary>
        /// Called every frame while the feature is enabled.
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// Called every fixed update while the feature is enabled.
        /// Use for physics-related operations.
        /// </summary>
        void OnFixedUpdate();
        
        /// <summary>
        /// Called every late update while the feature is enabled.
        /// Use for camera control and post-movement adjustments.
        /// Runs AFTER all Update and LateUpdate calls.
        /// </summary>
        void OnLateUpdate();
        
        /// <summary>
        /// Called during OnGUI for custom rendering.
        /// </summary>
        void OnGUI();
        
        /// <summary>
        /// Called when the feature is unregistered/mod unloads.
        /// Use for cleanup.
        /// </summary>
        void OnUnregister();
        
        /// <summary>
        /// Toggles the feature on/off.
        /// </summary>
        void Toggle();
        
        /// <summary>
        /// Executes the feature's action (for non-toggleable features).
        /// </summary>
        void Execute();
    }
}
