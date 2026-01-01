using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    /// <summary>
    /// Prevents stamina from depleting by keeping it at max.
    /// Uses direct IL2CPP type access via GameTypes.
    /// PlayerMovement has CurrentStaminaReserve property.
    /// </summary>
    public class InfiniteStamina : FeatureBase
    {
        public override string Id => "infinitestamina";
        public override string Name => "Infinite Stamina";
        public override string Description => "Never run out of stamina";
        public override FeatureCategory Category => FeatureCategory.Player;

        private const float MaxStamina = 100f;

        public override void OnEnable()
        {
            SewerLogger.Debug("InfiniteStamina enabled");
        }

        public override void OnDisable()
        {
            SewerLogger.Debug("InfiniteStamina disabled");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            SafeExecute(() =>
            {
                var movement = GameTypes.Movement;
                if (movement == null) return;

                try
                {
                    // PlayerMovement has CurrentStaminaReserve property
                    float currentStamina = movement.CurrentStaminaReserve;
                    
                    if (currentStamina < MaxStamina)
                    {
                        // Use SetStamina method if available, otherwise try direct set
                        movement.SetStamina(MaxStamina, false);
                    }
                }
                catch (System.Exception ex)
                {
                    SewerLogger.Error("InfiniteStamina: Failed to set stamina", ex);
                }
            }, "updating infinite stamina");
        }
    }
}
