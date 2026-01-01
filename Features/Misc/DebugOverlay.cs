using UnityEngine;
using SewerMenu.Features.Base;
using SewerMenu.Core.Logging;
using SewerMenu.Utils;
using SewerMenu.UI;

namespace SewerMenu.Features.Misc
{
    /// <summary>
    /// Displays debug information overlay.
    /// </summary>
    public class DebugOverlay : FeatureBase
    {
        public override string Id => "debugoverlay";
        public override string Name => "Debug Overlay";
        public override string Description => "Display game state information";
        public override FeatureCategory Category => FeatureCategory.Misc;

        public bool ShowPosition { get; set; } = true;
        public bool ShowRotation { get; set; } = true;
        public bool ShowVelocity { get; set; } = true;
        public bool ShowFPS { get; set; } = true;
        public bool ShowGameState { get; set; } = true;

        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        
        private float _fps;
        private float _fpsUpdateInterval = 0.5f;
        private float _fpsAccumulator;
        private int _fpsFrames;
        private float _fpsTimeLeft;

        public override void OnEnable()
        {
            _fpsTimeLeft = _fpsUpdateInterval;
            SewerLogger.Debug("DebugOverlay enabled");
        }

        public override void OnDisable()
        {
            SewerLogger.Debug("DebugOverlay disabled");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            // Update FPS counter
            _fpsTimeLeft -= Time.deltaTime;
            _fpsAccumulator += Time.timeScale / Time.deltaTime;
            _fpsFrames++;

            if (_fpsTimeLeft <= 0f)
            {
                _fps = _fpsAccumulator / _fpsFrames;
                _fpsTimeLeft = _fpsUpdateInterval;
                _fpsAccumulator = 0f;
                _fpsFrames = 0;
            }
        }

        public override void OnGUI()
        {
            if (!IsEnabled) return;

            InitializeStyles();

            float x = 10f;
            float y = 10f;
            float width = 250f;
            float lineHeight = 20f;
            float padding = 5f;

            // Calculate height based on enabled options
            int lines = 1; // Title
            if (ShowFPS) lines++;
            if (ShowPosition) lines += 3;
            if (ShowRotation) lines += 3;
            if (ShowVelocity) lines++;
            if (ShowGameState) lines += 4;

            float height = (lines * lineHeight) + (padding * 2);

            // Draw background
            GUI.Box(new Rect(x, y, width, height), "", _boxStyle);

            float currentY = y + padding;

            // Title
            GUI.Label(new Rect(x + padding, currentY, width - padding * 2, lineHeight), 
                "DEBUG INFO", _labelStyle);
            currentY += lineHeight;

            var player = GameFinder.GetLocalPlayer();

            // FPS
            if (ShowFPS)
            {
                DrawLine(ref currentY, x, width, padding, lineHeight, "FPS", $"{_fps:F1}");
            }

            // Position
            if (ShowPosition && player != null)
            {
                var pos = player.transform.position;
                DrawLine(ref currentY, x, width, padding, lineHeight, "Pos X", $"{pos.x:F2}");
                DrawLine(ref currentY, x, width, padding, lineHeight, "Pos Y", $"{pos.y:F2}");
                DrawLine(ref currentY, x, width, padding, lineHeight, "Pos Z", $"{pos.z:F2}");
            }

            // Rotation
            if (ShowRotation && player != null)
            {
                var rot = player.transform.eulerAngles;
                DrawLine(ref currentY, x, width, padding, lineHeight, "Rot X", $"{rot.x:F1}");
                DrawLine(ref currentY, x, width, padding, lineHeight, "Rot Y", $"{rot.y:F1}");
                DrawLine(ref currentY, x, width, padding, lineHeight, "Rot Z", $"{rot.z:F1}");
            }

            // Velocity
            if (ShowVelocity && player != null)
            {
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    DrawLine(ref currentY, x, width, padding, lineHeight, "Speed", $"{rb.velocity.magnitude:F2} m/s");
                }
                else
                {
                    var cc = player.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        DrawLine(ref currentY, x, width, padding, lineHeight, "Speed", $"{cc.velocity.magnitude:F2} m/s");
                    }
                }
            }

            // Game State
            if (ShowGameState)
            {
                // Time
                var timeManager = GameFinder.GetTimeManager();
                if (timeManager != null)
                {
                    var timeProp = timeManager.GetType().GetProperty("TimeOfDay");
                    if (timeProp != null)
                    {
                        float time = (float)timeProp.GetValue(timeManager);
                        DrawLine(ref currentY, x, width, padding, lineHeight, "Time", Features.World.TimeController.FormatTime(time));
                    }

                    var dayProp = timeManager.GetType().GetProperty("Day");
                    if (dayProp != null)
                    {
                        int day = (int)dayProp.GetValue(timeManager);
                        DrawLine(ref currentY, x, width, padding, lineHeight, "Day", day.ToString());
                    }
                }

                // Money
                var moneyManager = GameFinder.GetMoneyManager();
                if (moneyManager != null)
                {
                    var cashProp = moneyManager.GetType().GetProperty("Cash");
                    if (cashProp != null)
                    {
                        float cash = (float)cashProp.GetValue(moneyManager);
                        DrawLine(ref currentY, x, width, padding, lineHeight, "Cash", $"${cash:N0}");
                    }
                }

                // Wanted Level
                var lawManager = GameFinder.GetLawManager();
                if (lawManager != null)
                {
                    var wantedProp = lawManager.GetType().GetProperty("WantedLevel");
                    if (wantedProp != null)
                    {
                        int wanted = (int)wantedProp.GetValue(lawManager);
                        DrawLine(ref currentY, x, width, padding, lineHeight, "Wanted", new string('*', wanted));
                    }
                }
            }
        }

        private void DrawLine(ref float y, float x, float width, float padding, float lineHeight, string label, string value)
        {
            GUI.Label(new Rect(x + padding, y, 80, lineHeight), label + ":", _labelStyle);
            GUI.Label(new Rect(x + 90, y, width - 100, lineHeight), value, _valueStyle);
            y += lineHeight;
        }

        private void InitializeStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                var bgTex = new Texture2D(1, 1);
                bgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.7f));
                bgTex.Apply();
                _boxStyle.normal.background = bgTex;
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
                _labelStyle.normal.textColor = Theme.Text;
            }

            if (_valueStyle == null)
            {
                _valueStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12
                };
                _valueStyle.normal.textColor = Theme.Highlight;
            }
        }
    }
}
