using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    public class HealthEnergy : FeatureBase
    {
        public override string Id => "healthenergy";
        public override string Name => "Health & Energy";
        public override string Description => "Manipulate player health and energy";
        public override FeatureCategory Category => FeatureCategory.Player;

        private const float MaxHealth = 100f;
        private const float MaxEnergy = 100f;

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
