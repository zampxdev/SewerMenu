using System;
using UnityEngine;
using SewerMenu.Core.Logging;

namespace SewerMenu.Features.Base
{
    /// <summary>
    /// Abstract base class for all SewerMenu features.
    /// Provides common functionality and default implementations.
    /// </summary>
    public abstract class FeatureBase : IFeature
    {
        #region Properties
        
        /// <inheritdoc/>
        public abstract string Id { get; }
        
        /// <inheritdoc/>
        public abstract string Name { get; }
        
        /// <inheritdoc/>
        public abstract string Description { get; }
        
        /// <inheritdoc/>
        public abstract FeatureCategory Category { get; }
        
        /// <inheritdoc/>
        public virtual bool IsToggleable => true;
        
        /// <inheritdoc/>
        public virtual bool RequiresHost => false;
        
        /// <inheritdoc/>
        public KeyCode? Hotkey { get; set; }
        
        private bool _isEnabled;
        
        /// <inheritdoc/>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                
                _isEnabled = value;
                
                try
                {
                    if (_isEnabled)
                    {
                        OnEnable();
                        SewerLogger.FeatureToggled(Name, true);
                    }
                    else
                    {
                        OnDisable();
                        SewerLogger.FeatureToggled(Name, false);
                    }
                }
                catch (Exception ex)
                {
                    SewerLogger.Error($"Error toggling {Name}", ex);
                    _isEnabled = !_isEnabled; // Revert on error
                }
            }
        }
        
        /// <summary>
        /// Whether this feature has been registered.
        /// </summary>
        public bool IsRegistered { get; private set; }
        
        /// <summary>
        /// Whether this feature encountered an error and is in a failed state.
        /// </summary>
        public bool HasError { get; protected set; }
        
        /// <summary>
        /// Error message if the feature is in a failed state.
        /// </summary>
        public string ErrorMessage { get; protected set; }
        
        #endregion
        
        #region Lifecycle Methods
        
        /// <inheritdoc/>
        public virtual void OnRegister()
        {
            IsRegistered = true;
            SewerLogger.Debug($"Feature registered: {Name}");
        }
        
        /// <inheritdoc/>
        public virtual void OnEnable()
        {
            // Override in derived classes
        }
        
        /// <inheritdoc/>
        public virtual void OnDisable()
        {
            // Override in derived classes
        }
        
        /// <inheritdoc/>
        public virtual void OnUpdate()
        {
            // Override in derived classes
        }
        
        /// <inheritdoc/>
        public virtual void OnFixedUpdate()
        {
            // Override in derived classes
        }
        
        /// <inheritdoc/>
        public virtual void OnLateUpdate()
        {
            // Override in derived classes - use for camera control
        }
        
        /// <inheritdoc/>
        public virtual void OnGUI()
        {
            // Override in derived classes
        }
        
        /// <inheritdoc/>
        public virtual void OnUnregister()
        {
            if (IsEnabled)
            {
                IsEnabled = false;
            }
            IsRegistered = false;
            SewerLogger.Debug($"Feature unregistered: {Name}");
        }
        
        #endregion
        
        #region Actions
        
        /// <inheritdoc/>
        public virtual void Toggle()
        {
            if (!IsToggleable)
            {
                SewerLogger.Warning($"{Name} is not toggleable");
                return;
            }
            
            IsEnabled = !IsEnabled;
        }
        
        /// <inheritdoc/>
        public virtual void Execute()
        {
            // Override in derived classes for action-only features
            SewerLogger.Debug($"Execute called on {Name}");
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Safely executes an action with error handling.
        /// </summary>
        protected void SafeExecute(Action action, string context = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                string message = context ?? "executing action";
                SewerLogger.Error($"Error {message} in {Name}", ex);
                SetError($"Error {message}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Sets the feature into an error state.
        /// </summary>
        protected void SetError(string message)
        {
            HasError = true;
            ErrorMessage = message;
            
            // Disable the feature if it's in an error state
            if (IsEnabled)
            {
                _isEnabled = false;
                OnDisable();
            }
        }
        
        /// <summary>
        /// Clears the error state.
        /// </summary>
        protected void ClearError()
        {
            HasError = false;
            ErrorMessage = null;
        }
        
        /// <summary>
        /// Checks if the player is in a valid state for this feature.
        /// </summary>
        protected virtual bool CanExecute()
        {
            // Check if game is in a valid state
            // This will be implemented once we have access to game managers
            return true;
        }
        
        #endregion
    }
}
