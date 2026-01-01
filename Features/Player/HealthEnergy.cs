using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    /// <summary>
    /// Provides health and energy manipulation features.
    /// Includes infinite health, infinite energy, and manual controls.
    /// </summary>
    public class HealthEnergy : FeatureBase
    {
        public override string Id => "healthenergy";
        public override string Name => "Health & Energy";
        public override string Description => "Manipulate player health and energy";
        public override FeatureCategory Category => FeatureCategory.Player;

        private const float MaxHealth = 100f;
        private const float MaxEnergy = 100f;

        // Feature toggles
        public bool InfiniteHealth { get; set; } = false;
        public bool InfiniteEnergy { get; set; } = false;

        public override void OnEnable()
        {
            SewerLogger.Debug("HealthEnergy feature enabled");
        }

        public override void OnDisable()
        {
            SewerLogger.Debug("HealthEnergy feature disabled");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            SafeExecute(() =>
            {
                // Handle infinite health
                if (InfiniteHealth)
                {
                    var health = GameTypes.Health;
                    if (health != null)
                    {
                        try
                        {
                            if (health.CurrentHealth < MaxHealth)
                            {
                                health.SetHealth(MaxHealth);
                            }
                        }
                        catch { }
                    }
                }

                // Handle infinite energy
                if (InfiniteEnergy)
                {
                    var energy = GameTypes.Energy;
                    if (energy != null)
                    {
                        try
                        {
                            if (energy.CurrentEnergy < MaxEnergy)
                            {
                                energy.SetEnergy(MaxEnergy);
                            }
                        }
                        catch { }
                    }
                }
            }, "updating health/energy");
        }

        /// <summary>
        /// Gets the current health value.
        /// </summary>
        public float GetCurrentHealth()
        {
            try
            {
                var health = GameTypes.Health;
                if (health != null)
                    return health.CurrentHealth;
            }
            catch { }
            return 0f;
        }

        /// <summary>
        /// Gets the current energy value.
        /// </summary>
        public float GetCurrentEnergy()
        {
            try
            {
                var energy = GameTypes.Energy;
                if (energy != null)
                    return energy.CurrentEnergy;
            }
            catch { }
            return 0f;
        }

        /// <summary>
        /// Heals the player to full health.
        /// </summary>
        public void HealToFull()
        {
            SafeExecute(() =>
            {
                var health = GameTypes.Health;
                if (health != null)
                {
                    health.SetHealth(MaxHealth);
                    SewerLogger.Info("Healed to full health");
                }
                else
                {
                    SewerLogger.Warning("Health component not found");
                }
            }, "healing to full");
        }

        /// <summary>
        /// Sets the player's health to a specific value.
        /// </summary>
        public void SetHealth(float value)
        {
            SafeExecute(() =>
            {
                var health = GameTypes.Health;
                if (health != null)
                {
                    value = Mathf.Clamp(value, 0f, MaxHealth);
                    health.SetHealth(value);
                    SewerLogger.Info($"Set health to {value}");
                }
            }, "setting health");
        }

        /// <summary>
        /// Restores the player's energy to full.
        /// </summary>
        public void RestoreEnergy()
        {
            SafeExecute(() =>
            {
                var energy = GameTypes.Energy;
                if (energy != null)
                {
                    energy.RestoreEnergy();
                    SewerLogger.Info("Restored energy to full");
                }
                else
                {
                    SewerLogger.Warning("Energy component not found");
                }
            }, "restoring energy");
        }

        /// <summary>
        /// Sets the player's energy to a specific value.
        /// </summary>
        public void SetEnergy(float value)
        {
            SafeExecute(() =>
            {
                var energy = GameTypes.Energy;
                if (energy != null)
                {
                    value = Mathf.Clamp(value, 0f, MaxEnergy);
                    energy.SetEnergy(value);
                    SewerLogger.Info($"Set energy to {value}");
                }
            }, "setting energy");
        }

        /// <summary>
        /// Checks if the player is alive.
        /// </summary>
        public bool IsAlive()
        {
            try
            {
                var health = GameTypes.Health;
                if (health != null)
                    return health.IsAlive;
            }
            catch { }
            return true;
        }
    }
}
