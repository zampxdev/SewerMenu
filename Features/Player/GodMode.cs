using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    /// <summary>
    /// Makes the player invincible - prevents all damage by keeping health at max.
    /// Uses direct IL2CPP type access via GameTypes.
    /// </summary>
    public class GodMode : FeatureBase
    {
        public override string Id => "godmode";
        public override string Name => "God Mode";
        public override string Description => "Become invincible - take no damage";
        public override FeatureCategory Category => FeatureCategory.Player;

        private const float MaxHealth = 100f;

        public override void OnEnable()
        {
            SewerLogger.Debug("GodMode enabled - player is now invincible");
        }

        public override void OnDisable()
        {
            SewerLogger.Debug("GodMode disabled");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            SafeExecute(() =>
            {
                var health = GameTypes.Health;
                if (health == null) return;

                // Use direct property access - PlayerHealth has CurrentHealth property
                try
                {
                    float currentHealth = health.CurrentHealth;
                    
                    if (currentHealth < MaxHealth)
                    {
                        // Use SetHealth method for direct health setting
                        health.SetHealth(MaxHealth);
                    }
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("GodMode: Failed to set health", ex);
                }
            }, "updating god mode");
        }
    }
}
