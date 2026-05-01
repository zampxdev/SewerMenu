using System.Collections.Generic;
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
        private readonly List<DebugLine> _cachedLines = new List<DebugLine>(16);

        public override void OnEnable()
        {
            _fpsTimeLeft = _fpsUpdateInterval;
            _fpsAccumulator = 0f;
            _fpsFrames = 0;
            RefreshCachedLines();
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
            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            _fpsTimeLeft -= deltaTime;
            _fpsAccumulator += 1f / deltaTime;
            _fpsFrames++;

            if (_fpsTimeLeft <= 0f)
            {
                _fps = _fpsFrames > 0 ? _fpsAccumulator / _fpsFrames : 0f;
                _fpsTimeLeft = _fpsUpdateInterval;
                _fpsAccumulator = 0f;
                _fpsFrames = 0;
                RefreshCachedLines();
            }
        }

        public override void OnGUI()
        {
            if (!IsEnabled) return;
            if (Event.current != null && Event.current.type != EventType.Repaint) return;

            InitializeStyles();

            float x = 10f;
            float y = 10f;
            float width = 250f;
            float lineHeight = 20f;
            float padding = 5f;

            int lines = 1 + _cachedLines.Count;
            float height = (lines * lineHeight) + (padding * 2);

            // Draw background
            GUI.Box(new Rect(x, y, width, height), "", _boxStyle);

            float currentY = y + padding;

            // Title
            GUI.Label(new Rect(x + padding, currentY, width - padding * 2, lineHeight), 
                "DEBUG INFO", _labelStyle);
            currentY += lineHeight;

            for (int i = 0; i < _cachedLines.Count; i++)
            {
                DrawLine(ref currentY, x, width, padding, lineHeight, _cachedLines[i].Label, _cachedLines[i].Value);
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

        private void RefreshCachedLines()
        {
            _cachedLines.Clear();

            if (ShowFPS)
            {
                _cachedLines.Add(new DebugLine("FPS", $"{_fps:F1}"));
            }

            var player = GameFinder.GetLocalPlayer();
            if (player != null)
            {
                if (ShowPosition)
                {
                    var pos = player.transform.position;
                    _cachedLines.Add(new DebugLine("Pos X", $"{pos.x:F2}"));
                    _cachedLines.Add(new DebugLine("Pos Y", $"{pos.y:F2}"));
                    _cachedLines.Add(new DebugLine("Pos Z", $"{pos.z:F2}"));
                }

                if (ShowRotation)
                {
                    var rot = player.transform.eulerAngles;
                    _cachedLines.Add(new DebugLine("Rot X", $"{rot.x:F1}"));
                    _cachedLines.Add(new DebugLine("Rot Y", $"{rot.y:F1}"));
                    _cachedLines.Add(new DebugLine("Rot Z", $"{rot.z:F1}"));
                }

                if (ShowVelocity)
                {
                    string speed = "";
                    var rb = player.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        speed = $"{rb.velocity.magnitude:F2} m/s";
                    }
                    else
                    {
                        var cc = player.GetComponent<CharacterController>();
                        if (cc != null)
                        {
                            speed = $"{cc.velocity.magnitude:F2} m/s";
                        }
                    }

                    if (!string.IsNullOrEmpty(speed))
                    {
                        _cachedLines.Add(new DebugLine("Speed", speed));
                    }
                }
            }

            if (ShowGameState)
            {
                RefreshGameStateLines();
            }
        }

        private void RefreshGameStateLines()
        {
            var timeManager = GameFinder.GetTimeManager();
            if (timeManager != null)
            {
                var timeProp = timeManager.GetType().GetProperty("TimeOfDay");
                if (timeProp != null)
                {
                    float time = (float)timeProp.GetValue(timeManager);
                    _cachedLines.Add(new DebugLine("Time", Features.World.TimeController.FormatTime(time)));
                }

                var dayProp = timeManager.GetType().GetProperty("Day");
                if (dayProp != null)
                {
                    int day = (int)dayProp.GetValue(timeManager);
                    _cachedLines.Add(new DebugLine("Day", day.ToString()));
                }
            }

            var moneyManager = GameFinder.GetMoneyManager();
            if (moneyManager != null)
            {
                var cashProp = moneyManager.GetType().GetProperty("Cash");
                if (cashProp != null)
                {
                    float cash = (float)cashProp.GetValue(moneyManager);
                    _cachedLines.Add(new DebugLine("Cash", $"${cash:N0}"));
                }
            }

            var lawManager = GameFinder.GetLawManager();
            if (lawManager != null)
            {
                var wantedProp = lawManager.GetType().GetProperty("WantedLevel");
                if (wantedProp != null)
                {
                    int wanted = (int)wantedProp.GetValue(lawManager);
                    _cachedLines.Add(new DebugLine("Wanted", new string('*', wanted)));
                }
            }
        }

        private readonly struct DebugLine
        {
            public readonly string Label;
            public readonly string Value;

            public DebugLine(string label, string value)
            {
                Label = label;
                Value = value;
            }
        }
    }
}
