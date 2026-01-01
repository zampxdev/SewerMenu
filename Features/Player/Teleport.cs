using System.Collections.Generic;
using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Config;
using SewerMenu.Utils;

namespace SewerMenu.Features.Player
{
    /// <summary>
    /// Allows saving and teleporting to locations.
    /// </summary>
    public class Teleport : FeatureBase
    {
        public override string Id => "teleport";
        public override string Name => "Teleport";
        public override string Description => "Save and teleport to locations";
        public override FeatureCategory Category => FeatureCategory.Player;
        public override bool IsToggleable => false;

        public string NewLocationName { get; set; } = "";

        /// <summary>
        /// Preset game locations - approximate coordinates for key areas.
        /// These are common locations in Schedule I.
        /// </summary>
        public static readonly List<PresetLocation> PresetLocations = new List<PresetLocation>
        {
            // Starting area / Hyland Point
            new PresetLocation("Motel (Start)", new Vector3(-102f, 2f, 52f)),
            new PresetLocation("Hyland Point Bus Stop", new Vector3(-85f, 2f, 75f)),
            
            // Downtown / Northtown
            new PresetLocation("Downtown Center", new Vector3(0f, 2f, 0f)),
            new PresetLocation("Bank", new Vector3(45f, 2f, -20f)),
            new PresetLocation("Police Station", new Vector3(80f, 2f, 30f)),
            
            // Docks area
            new PresetLocation("Docks Entrance", new Vector3(-150f, 2f, -100f)),
            new PresetLocation("Warehouse District", new Vector3(-180f, 2f, -80f)),
            
            // Westville
            new PresetLocation("Westville", new Vector3(-200f, 2f, 50f)),
            
            // Suburbs
            new PresetLocation("Suburbs North", new Vector3(100f, 2f, 150f)),
            new PresetLocation("Suburbs South", new Vector3(100f, 2f, -150f)),
            
            // Industrial
            new PresetLocation("Industrial Zone", new Vector3(-100f, 2f, -200f)),
            
            // Special locations
            new PresetLocation("Casino", new Vector3(150f, 2f, 0f)),
            new PresetLocation("Car Dealership", new Vector3(50f, 2f, 100f)),
            new PresetLocation("Dark Market", new Vector3(-50f, -5f, -50f)),
        };

        /// <summary>
        /// Gets the player's current position.
        /// </summary>
        public Vector3 GetCurrentPosition()
        {
            return GameTypes.PlayerPosition;
        }

        /// <summary>
        /// Teleports the player to the specified position.
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            SafeExecute(() =>
            {
                var transform = GameTypes.PlayerTransform;
                if (transform == null)
                    return;

                // Disable CharacterController temporarily for teleport
                var cc = transform.GetComponent<CharacterController>();
                bool wasEnabled = false;
                if (cc != null)
                {
                    wasEnabled = cc.enabled;
                    cc.enabled = false;
                }

                // Teleport
                transform.position = position;

                // Re-enable CharacterController
                if (cc != null)
                    cc.enabled = wasEnabled;
            }, "teleporting player");
        }

        /// <summary>
        /// Teleports to a preset location by index.
        /// </summary>
        public void TeleportToPreset(int index)
        {
            if (index >= 0 && index < PresetLocations.Count)
            {
                TeleportTo(PresetLocations[index].Position);
            }
        }

        /// <summary>
        /// Saves the current location with the given name.
        /// </summary>
        public void SaveCurrentLocation()
        {
            if (string.IsNullOrWhiteSpace(NewLocationName))
                return;

            var pos = GetCurrentPosition();
            if (pos == Vector3.zero)
                return;

            var location = new TeleportLocation
            {
                Name = NewLocationName.Trim(),
                X = pos.x,
                Y = pos.y,
                Z = pos.z
            };

            ConfigManager.Instance.Config.TeleportLocations.Add(location);
            ConfigManager.Instance.QueueSave();
            NewLocationName = "";
        }

        public override void Execute()
        {
            // Execute is used for action features - teleport uses TeleportTo directly
        }
    }

    /// <summary>
    /// A preset teleport location.
    /// </summary>
    public class PresetLocation
    {
        public string Name { get; }
        public Vector3 Position { get; }

        public PresetLocation(string name, Vector3 position)
        {
            Name = name;
            Position = position;
        }
    }
}
