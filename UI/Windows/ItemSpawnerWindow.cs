using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SewerMenu.Core.Logging;
using SewerMenu.Features.Items;
using SewerMenu.Features.Base;
using SewerMenu.Utils;
using Il2CppScheduleOne.ItemFramework;

namespace SewerMenu.UI.Windows
{
    // Uses direct drawing (not GUI.Window) to avoid IL2CPP delegate issues
    public class ItemSpawnerWindow
    {
        #region Singleton
        
        private static ItemSpawnerWindow _instance;
        public static ItemSpawnerWindow Instance => _instance ??= new ItemSpawnerWindow();
        
        private ItemSpawnerWindow() { }
        
        #endregion
        
        #region Fields
        
        private bool _isVisible = false;
        private Rect _windowRect = new Rect(100, 100, 750, 550);
        
        private List<string> _categories = new List<string>();
        private int _selectedCategoryIndex = 0;
        private Dictionary<string, List<ItemSpawner.ItemInfo>> _itemsByCategory = new Dictionary<string, List<ItemSpawner.ItemInfo>>();
        
        private Vector2 _categoryScrollPos;
        private Vector2 _itemScrollPos;
        private Vector2 _targetCategoryScrollPos;
        private Vector2 _displayCategoryScrollPos;
        private Vector2 _targetItemScrollPos;
        private Vector2 _displayItemScrollPos;
        private int _selectedItemIndex = -1;
        private ItemSpawner.ItemInfo _selectedItem = null;
        
        private string _searchFilter = "";
        private string _searchQuery = "";
        private string _appliedSearchFilter = "";
        private int _appliedCategoryIndex = -2;
        private bool _searchFocused = false;
        private int _spawnQuantity = 1;
        private SearchQuantityMode _searchQuantityMode = SearchQuantityMode.None;
        private string _searchCommandHint = "";
        private string _statusMessage = "";
        private bool _statusSuccess = true;
        private bool _refreshRequested = false;
        
        private bool _dataLoaded = false;
        private List<ItemSpawner.ItemInfo> _currentItems = new List<ItemSpawner.ItemInfo>();
        private readonly Dictionary<ItemSpawner.ItemInfo, SearchEntry> _searchEntryByItem = new Dictionary<ItemSpawner.ItemInfo, SearchEntry>();
        private readonly List<ScoredItem> _scoredItems = new List<ScoredItem>(256);
        
        private readonly int[] _quantityPresets = { 1, 5, 10, 25, 50, 100, 999 };
        
        private bool _isDragging = false;
        private Vector2 _dragOffset;
        private float _openAnim = 1f;
        private const int MaxSpawnQuantity = 9999;
        private const float ItemRowHeight = 40f;
        
        private static Texture2D _solidTex;
        
        #endregion
        
        #region Properties
        
        public bool IsVisible => _isVisible;
        
        #endregion
        
        #region Show/Hide
        
        public void Show()
        {
            _isVisible = true;
            _openAnim = 0f;
            _targetCategoryScrollPos = _categoryScrollPos;
            _displayCategoryScrollPos = _categoryScrollPos;
            _targetItemScrollPos = _itemScrollPos;
            _displayItemScrollPos = _itemScrollPos;
            
            // Center window on screen
            _windowRect.x = (Screen.width - _windowRect.width) / 2;
            _windowRect.y = (Screen.height - _windowRect.height) / 2;
            
            // Load data if not loaded
            if (!_dataLoaded)
            {
                RefreshData();
            }
            
            // Ensure texture exists
            if (_solidTex == null)
            {
                _solidTex = new Texture2D(1, 1);
                _solidTex.SetPixel(0, 0, Color.white);
                _solidTex.Apply();
                _solidTex.hideFlags = HideFlags.HideAndDontSave;
            }
            
            SewerLogger.Debug("ItemSpawnerWindow opened");
        }
        
        public void Hide()
        {
            _isVisible = false;
            _isDragging = false;
            SewerLogger.Debug("ItemSpawnerWindow closed");
        }
        
        public void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }
        
        #endregion
        
        #region Data Loading
        
        public void RefreshData()
        {
            _categories.Clear();
            _itemsByCategory.Clear();
            _currentItems.Clear();
            _searchEntryByItem.Clear();
            _scoredItems.Clear();
            _selectedItem = null;
            _selectedItemIndex = -1;
            _appliedSearchFilter = null;
            _appliedCategoryIndex = -2;
            
            try
            {
                var spawner = FeatureManager.Instance?.GetFeature<ItemSpawner>("itemspawner");
                if (spawner == null)
                {
                    SewerLogger.Warning("ItemSpawner feature not found");
                    return;
                }
                
                // Force a fresh registry scan. The first scan can happen before a save is fully loaded.
                spawner.RefreshItemCache();
                var allItems = spawner.GetAllItems();
                
                if (allItems == null || allItems.Count == 0)
                {
                    SewerLogger.Warning("No items found");
                    return;
                }
                
                // Add "All" category first
                _categories.Add("All");
                _itemsByCategory["All"] = new List<ItemSpawner.ItemInfo>(allItems);
                
                // Group by category
                var grouped = allItems.GroupBy(i => i.Category ?? "Misc").OrderBy(g => g.Key);
                foreach (var group in grouped)
                {
                    string category = group.Key;
                    if (!_categories.Contains(category))
                    {
                        _categories.Add(category);
                    }
                    _itemsByCategory[category] = group.OrderBy(i => i.Name).ToList();
                }

                RebuildSearchIndex(allItems);
                
                // Set initial view
                _selectedCategoryIndex = 0;
                _selectedItemIndex = _itemsByCategory["All"].Count > 0 ? 0 : -1;
                UpdateCurrentItems();
                
                _dataLoaded = true;
                SewerLogger.Info($"ItemSpawnerWindow: Loaded {allItems.Count} items in {_categories.Count} categories");
            }
            catch (Exception ex)
            {
                SewerLogger.Error("Failed to load item data", ex);
            }
        }
        
        private void UpdateCurrentItems()
        {
            if (_selectedCategoryIndex < 0 || _selectedCategoryIndex >= _categories.Count)
            {
                _currentItems.Clear();
                _selectedItem = null;
                _selectedItemIndex = -1;
                return;
            }

            string normalizedQuery = NormalizeSearchText(_searchQuery);
            if (_appliedCategoryIndex == _selectedCategoryIndex && _appliedSearchFilter == normalizedQuery)
            {
                return;
            }
            
            string category = _categories[_selectedCategoryIndex];
            
            if (!_itemsByCategory.TryGetValue(category, out var items))
            {
                _currentItems.Clear();
                _selectedItem = null;
                _selectedItemIndex = -1;
                return;
            }

            string previousSelectedId = _selectedItem?.Id;
            _currentItems.Clear();
            
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                _currentItems.AddRange(items);
            }
            else
            {
                ApplyFuzzySearch(items, normalizedQuery);
            }

            _appliedCategoryIndex = _selectedCategoryIndex;
            _appliedSearchFilter = normalizedQuery;

            if (!string.IsNullOrEmpty(previousSelectedId))
            {
                int preservedIndex = _currentItems.FindIndex(i => i.Id == previousSelectedId);
                if (preservedIndex >= 0)
                {
                    _selectedItemIndex = preservedIndex;
                }
            }
            
            if (_selectedItemIndex >= _currentItems.Count)
            {
                _selectedItemIndex = _currentItems.Count > 0 ? 0 : -1;
            }
            
            if (_selectedItemIndex >= 0 && _selectedItemIndex < _currentItems.Count)
            {
                _selectedItem = _currentItems[_selectedItemIndex];
            }
            else
            {
                _selectedItem = null;
            }

            ApplyStackQuantityCommand();
        }

        private void RebuildSearchIndex(List<ItemSpawner.ItemInfo> allItems)
        {
            _searchEntryByItem.Clear();
            if (allItems == null) return;

            foreach (var item in allItems)
            {
                if (item == null) continue;

                var entry = new SearchEntry
                {
                    Item = item,
                    Name = NormalizeSearchText(item.Name),
                    Id = NormalizeSearchText(item.Id),
                    Category = NormalizeSearchText(item.Category),
                    Acronym = BuildAcronym(item.Name)
                };

                _searchEntryByItem[item] = entry;
            }
        }

        private void ApplyFuzzySearch(List<ItemSpawner.ItemInfo> sourceItems, string normalizedQuery)
        {
            _scoredItems.Clear();
            if (sourceItems == null || string.IsNullOrEmpty(normalizedQuery)) return;

            for (int i = 0; i < sourceItems.Count; i++)
            {
                var item = sourceItems[i];
                var entry = FindSearchEntry(item);
                if (entry == null) continue;

                int score = ScoreItem(entry, normalizedQuery);
                if (score <= 0) continue;

                _scoredItems.Add(new ScoredItem(item, score));
            }

            _scoredItems.Sort((a, b) =>
            {
                int scoreCompare = b.Score.CompareTo(a.Score);
                if (scoreCompare != 0) return scoreCompare;
                return string.Compare(a.Item.Name, b.Item.Name, StringComparison.OrdinalIgnoreCase);
            });

            for (int i = 0; i < _scoredItems.Count; i++)
            {
                _currentItems.Add(_scoredItems[i].Item);
            }
        }

        private SearchEntry FindSearchEntry(ItemSpawner.ItemInfo item)
        {
            if (item == null) return null;
            return _searchEntryByItem.TryGetValue(item, out var entry) ? entry : null;
        }

        private static int ScoreItem(SearchEntry entry, string query)
        {
            if (entry == null || string.IsNullOrEmpty(query)) return 0;

            int best = 0;
            best = Math.Max(best, ScoreField(entry.Name, query, 100000, 82000, 64000));
            best = Math.Max(best, ScoreField(entry.Id, query, 93000, 76000, 60000));
            best = Math.Max(best, ScoreField(entry.Category, query, 52000, 46000, 38000));

            if (!string.IsNullOrEmpty(entry.Acronym))
            {
                best = Math.Max(best, ScoreField(entry.Acronym, query, 70000, 62000, 52000));
            }

            best = Math.Max(best, ScoreTokenMatch(entry, query));
            best = Math.Max(best, ScoreSubsequence(entry.Name, query));
            best = Math.Max(best, ScoreSubsequence(entry.Id, query));

            if (query.Length <= 18)
            {
                best = Math.Max(best, ScoreTypoDistance(entry.Name, query));
                best = Math.Max(best, ScoreTypoDistance(entry.Id, query));
            }

            return best;
        }

        private static int ScoreField(string field, string query, int exactScore, int startsWithScore, int containsScore)
        {
            if (string.IsNullOrEmpty(field)) return 0;
            if (field == query) return exactScore;
            if (field.StartsWith(query)) return startsWithScore - Math.Min(field.Length, 200);

            int index = field.IndexOf(query, StringComparison.Ordinal);
            if (index >= 0) return containsScore - Math.Min(index * 24 + field.Length, 1200);

            return 0;
        }

        private static int ScoreTokenMatch(SearchEntry entry, string query)
        {
            string[] tokens = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length <= 1) return 0;

            int score = 56000;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (entry.Name.Contains(token) || entry.Id.Contains(token) || entry.Category.Contains(token))
                {
                    score += 700;
                }
                else
                {
                    return 0;
                }
            }

            return score;
        }

        private static int ScoreSubsequence(string field, string query)
        {
            if (string.IsNullOrEmpty(field) || query.Length < 2) return 0;

            int queryIndex = 0;
            int firstMatch = -1;
            int lastMatch = -1;
            for (int i = 0; i < field.Length && queryIndex < query.Length; i++)
            {
                if (field[i] != query[queryIndex]) continue;

                if (firstMatch < 0) firstMatch = i;
                lastMatch = i;
                queryIndex++;
            }

            if (queryIndex < query.Length) return 0;

            int span = Mathf.Max(1, lastMatch - firstMatch + 1);
            int compactness = Mathf.Max(0, 1800 - span * 45);
            return 42000 + compactness - Math.Min(firstMatch * 30, 900);
        }

        private static int ScoreTypoDistance(string field, string query)
        {
            if (string.IsNullOrEmpty(field)) return 0;

            string candidate = field;
            int firstSpace = candidate.IndexOf(' ');
            if (firstSpace > 0)
            {
                candidate = candidate.Substring(0, firstSpace);
            }

            if (candidate.Length > 24 || Mathf.Abs(candidate.Length - query.Length) > 3)
            {
                return 0;
            }

            int distance = BoundedLevenshtein(candidate, query, 3);
            if (distance < 0) return 0;

            return 33000 - distance * 5000 - Math.Min(candidate.Length * 40, 900);
        }

        private static int BoundedLevenshtein(string a, string b, int maxDistance)
        {
            if (Mathf.Abs(a.Length - b.Length) > maxDistance) return -1;

            int[] previous = new int[b.Length + 1];
            int[] current = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++)
            {
                previous[j] = j;
            }

            for (int i = 1; i <= a.Length; i++)
            {
                current[0] = i;
                int rowBest = current[0];

                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    current[j] = Math.Min(
                        Math.Min(current[j - 1] + 1, previous[j] + 1),
                        previous[j - 1] + cost);
                    rowBest = Math.Min(rowBest, current[j]);
                }

                if (rowBest > maxDistance) return -1;

                var swap = previous;
                previous = current;
                current = swap;
            }

            return previous[b.Length] <= maxDistance ? previous[b.Length] : -1;
        }

        private static string NormalizeSearchText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";

            char[] buffer = new char[value.Length];
            int count = 0;
            bool lastWasSpace = true;

            for (int i = 0; i < value.Length; i++)
            {
                char c = char.ToLowerInvariant(value[i]);
                if (char.IsLetterOrDigit(c))
                {
                    buffer[count++] = c;
                    lastWasSpace = false;
                }
                else if (!lastWasSpace)
                {
                    buffer[count++] = ' ';
                    lastWasSpace = true;
                }
            }

            if (count > 0 && buffer[count - 1] == ' ')
            {
                count--;
            }

            return new string(buffer, 0, count);
        }

        private static string BuildAcronym(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";

            char[] buffer = new char[Mathf.Min(value.Length, 16)];
            int count = 0;
            bool atWordStart = true;

            for (int i = 0; i < value.Length && count < buffer.Length; i++)
            {
                char c = value[i];
                if (char.IsLetterOrDigit(c))
                {
                    if (atWordStart)
                    {
                        buffer[count++] = char.ToLowerInvariant(c);
                    }
                    atWordStart = false;
                }
                else
                {
                    atWordStart = true;
                }
            }

            return new string(buffer, 0, count);
        }
        
        #endregion
        
        #region OnGUI
        
        public void OnGUI()
        {
            if (!_isVisible) return;
            
            try
            {
                SewerSkin.BeginUI();
                SewerSkin.SetAnimationQuality(SewerMenu.Core.Config.ConfigManager.Instance.Config?.UI?.AnimationQuality);
                ApplyDeferredDataChanges();
                
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - 100);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - 100);
                float uiDelta = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
                float animStrength = SewerSkin.AnimationStrength;
                _openAnim = animStrength < 0.25f ? 1f : Mathf.MoveTowards(_openAnim, 1f, uiDelta * Mathf.Lerp(16f, 9f, animStrength));
                
                HandleDragging();
                Rect drawRect = new Rect(_windowRect.x, _windowRect.y - (1f - _openAnim) * 12f, _windowRect.width, _windowRect.height);
                DrawWindowBackground(drawRect);
                
                GUILayout.BeginArea(new Rect(drawRect.x + 10, drawRect.y + 8, drawRect.width - 20, drawRect.height - 16));
                DrawWindowContent();
                GUILayout.EndArea();
                
                SewerSkin.EndUI();
            }
            catch (Exception ex)
            {
                SewerLogger.Error("ItemSpawnerWindow.OnGUI error", ex);
            }
        }

        private void ApplyDeferredDataChanges()
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.Layout)
            {
                return;
            }

            if (_refreshRequested)
            {
                _refreshRequested = false;
                RefreshData();
                return;
            }

            if (_dataLoaded)
            {
                UpdateCurrentItems();
            }
        }
        
        private void DrawWindowBackground(Rect rect)
        {
            SewerSkin.DrawWindowPanel(rect);
        }
        
        private void HandleDragging()
        {
            Rect titleBar = new Rect(_windowRect.x, _windowRect.y, _windowRect.width - 40, 30);
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown && titleBar.Contains(e.mousePosition))
            {
                _isDragging = true;
                _dragOffset = e.mousePosition - new Vector2(_windowRect.x, _windowRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
            }
            else if (e.type == EventType.MouseDrag && _isDragging)
            {
                _windowRect.x = e.mousePosition.x - _dragOffset.x;
                _windowRect.y = e.mousePosition.y - _dragOffset.y;
                e.Use();
            }
        }
        
        private void DrawWindowContent()
        {
            Rect headerRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            
            SewerSkin.DrawRoundedRect(headerRect, new Color(0.035f, 0.046f, 0.041f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.58f), 6, 1);
            
            // Title
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            var titleStyle = SewerSkin.GetLabelStyle(14, FontStyle.Bold, TextAnchor.MiddleLeft);
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 4, 150, 24), "ITEM SPAWNER", titleStyle);
            GUI.contentColor = oldContentColor;
            
            Rect closeRect = new Rect(headerRect.x + headerRect.width - 32, headerRect.y + 4, 28, 24);
            if (DrawStyledButton(closeRect, "X", true))
            {
                _isVisible = false;
                _isDragging = false;
            }
            
            GUILayout.Space(6);
            DrawTopBar();
            GUILayout.Space(6);
            
            GUILayout.BeginHorizontal();
            DrawCategorySidebar();
            GUILayout.Space(6);
            DrawItemList();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(6);
            DrawBottomBar();
        }
        
        private void DrawTopBar()
        {
            Rect barRect = GUILayoutUtility.GetRect(0, 76, GUILayout.ExpandWidth(true));

            SewerSkin.DrawRoundedRect(barRect, new Color(0.052f, 0.067f, 0.061f, 0.96f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.45f), 6, 1);

            float xPos = barRect.x + 10;
            float yPos = barRect.y + 8;

            Rect searchRect = new Rect(xPos, yPos, Mathf.Max(260f, barRect.width - 188f), 28f);
            DrawSearchField(searchRect);
            DrawSearchCommandHint(new Rect(searchRect.x + 12f, searchRect.y + 30f, searchRect.width - 24f, 12f));

            float refreshX = searchRect.xMax + 8f;
            if (DrawStyledButton(new Rect(refreshX, yPos + 2f, 78f, 24f), "Refresh", false))
            {
                _refreshRequested = true;
            }

            yPos += 40f;
            xPos = barRect.x + 10f;

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(xPos, yPos, 60, 24), "Quantity:", SewerSkin.GetLabelStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft));
            GUI.contentColor = oldContentColor;
            xPos += 65;

            if (DrawStyledButton(new Rect(xPos, yPos, 38, 24), "-10", false))
            {
                _spawnQuantity = ClampSpawnQuantity(_spawnQuantity - 10);
                UpdateSearchCommandHint();
            }
            xPos += 42;

            if (DrawStyledButton(new Rect(xPos, yPos, 28, 24), "-", false))
            {
                _spawnQuantity = ClampSpawnQuantity(_spawnQuantity - 1);
                UpdateSearchCommandHint();
            }
            xPos += 32;

            Rect qtyRect = new Rect(xPos, yPos, 55, 24);
            SewerSkin.DrawRoundedRect(qtyRect, new Color(0.045f, 0.058f, 0.052f, 0.96f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.55f), 6, 1);

            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentGlow;
            var qtyStyle = SewerSkin.GetLabelStyle(12, FontStyle.Normal, TextAnchor.MiddleCenter);
            GUI.Label(qtyRect, _spawnQuantity.ToString(), qtyStyle);
            GUI.contentColor = oldContentColor;
            xPos += 59;
            
            if (DrawStyledButton(new Rect(xPos, yPos, 28, 24), "+", false))
            {
                _spawnQuantity = ClampSpawnQuantity(_spawnQuantity + 1);
                UpdateSearchCommandHint();
            }
            xPos += 32;
            
            if (DrawStyledButton(new Rect(xPos, yPos, 38, 24), "+10", false))
            {
                _spawnQuantity = ClampSpawnQuantity(_spawnQuantity + 10);
                UpdateSearchCommandHint();
            }
            xPos += 50;

            oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            GUI.Label(new Rect(xPos, yPos, 45, 24), "Quick:", SewerSkin.GetLabelStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft));
            GUI.contentColor = oldContentColor;
            xPos += 48;

            float rightLimit = barRect.x + barRect.width - 10f;
            foreach (int preset in _quantityPresets)
            {
                if (xPos + 42f > rightLimit) break;

                bool isSelected = _spawnQuantity == preset;
                if (DrawStyledButton(new Rect(xPos, yPos, 42, 24), preset.ToString(), false, isSelected))
                {
                    _spawnQuantity = preset;
                    UpdateSearchCommandHint();
                }
                xPos += 46;
            }
        }

        private void DrawSearchCommandHint(Rect rect)
        {
            if (string.IsNullOrEmpty(_searchCommandHint)) return;

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = new Color(SewerSkin.AccentGlow.r, SewerSkin.AccentGlow.g, SewerSkin.AccentGlow.b, 0.82f);
            GUI.Label(rect, _searchCommandHint, SewerSkin.GetLabelStyle(9, FontStyle.Normal, TextAnchor.MiddleLeft));
            GUI.contentColor = oldContentColor;
        }

        private void DrawSearchField(Rect rect)
        {
            HandleSearchInput(rect);

            bool hovered = rect.Contains(Event.current.mousePosition);
            Color border = _searchFocused
                ? new Color(SewerSkin.AccentGlow.r, SewerSkin.AccentGlow.g, SewerSkin.AccentGlow.b, 0.75f)
                : new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, hovered ? 0.58f : 0.42f);
            SewerSkin.DrawRoundedRect(rect, new Color(0.035f, 0.047f, 0.043f, 0.96f), border, 7, 1);

            var oldContentColor = GUI.contentColor;
            var style = SewerSkin.GetLabelStyle(12, FontStyle.Bold, TextAnchor.MiddleLeft);
            string display = string.IsNullOrEmpty(_searchFilter)
                ? "Search items by name, ID, category, or rough spelling..."
                : _searchFilter;

            GUI.contentColor = string.IsNullOrEmpty(_searchFilter)
                ? SewerSkin.TextMutedColor
                : SewerSkin.TextColor;
            GUI.Label(new Rect(rect.x + 12f, rect.y + 3f, rect.width - 86f, 22f), display, style);

            if (_searchFocused && Mathf.FloorToInt(Time.unscaledTime * 2.5f) % 2 == 0)
            {
                float caretX = rect.x + 14f + Mathf.Min(rect.width - 100f, style.CalcSize(new GUIContent(_searchFilter)).x);
                SewerSkin.DrawSolid(new Rect(caretX, rect.y + 7f, 1f, rect.height - 14f), SewerSkin.AccentGlow);
            }

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                Rect clearRect = new Rect(rect.xMax - 64f, rect.y + 4f, 56f, 20f);
                if (DrawStyledButton(clearRect, "Clear", false))
                {
                    SetSearchFilter("");
                    _searchFocused = true;
                }
            }

            GUI.contentColor = oldContentColor;
        }

        private void HandleSearchInput(Rect rect)
        {
            Event e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                _searchFocused = rect.Contains(e.mousePosition);
                return;
            }

            if (!_searchFocused || e.type != EventType.KeyDown) return;

            bool control = e.control || e.command;
            if (control && e.keyCode == KeyCode.V)
            {
                AppendSearchText(GUIUtility.systemCopyBuffer);
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Backspace)
            {
                if (_searchFilter.Length > 0)
                {
                    SetSearchFilter(_searchFilter.Substring(0, _searchFilter.Length - 1));
                }
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Delete)
            {
                SetSearchFilter("");
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                if (!string.IsNullOrEmpty(_searchFilter))
                {
                    SetSearchFilter("");
                }
                else
                {
                    _searchFocused = false;
                }
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                SpawnCurrentSearchResult();
                _searchFocused = false;
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Tab)
            {
                _searchFocused = false;
                e.Use();
                return;
            }

            if (!char.IsControl(e.character))
            {
                AppendSearchText(e.character.ToString());
                e.Use();
            }
        }

        private void AppendSearchText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            char[] buffer = new char[Mathf.Min(text.Length, 80)];
            int count = 0;
            for (int i = 0; i < text.Length && count < buffer.Length; i++)
            {
                char c = text[i];
                if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_' || c == '.' || c == '\'' || c == '*' || c == ':' || c == '=')
                {
                    buffer[count++] = c;
                }
            }

            if (count == 0) return;

            string next = (_searchFilter + new string(buffer, 0, count)).TrimStart();
            if (next.Length > 64)
            {
                next = next.Substring(0, 64);
            }

            SetSearchFilter(next);
        }

        private void SetSearchFilter(string value)
        {
            value ??= "";
            if (_searchFilter == value) return;

            string previousQuery = _searchQuery;
            _searchFilter = value;
            ApplySearchCommand(value);

            if (!string.Equals(_searchQuery, previousQuery, StringComparison.Ordinal))
            {
                _selectedItemIndex = 0;
                _itemScrollPos = Vector2.zero;
                _targetItemScrollPos = Vector2.zero;
                _displayItemScrollPos = Vector2.zero;
                _appliedSearchFilter = null;
            }
        }

        private void ApplySearchCommand(string rawValue)
        {
            _searchQuery = (rawValue ?? "").Trim();
            _searchQuantityMode = SearchQuantityMode.None;

            if (TryParseTrailingQuantityCommand(rawValue, out string query, out SearchQuantityMode mode, out int quantity))
            {
                _searchQuery = query;
                _searchQuantityMode = mode;

                if (mode == SearchQuantityMode.Exact)
                {
                    _spawnQuantity = ClampSpawnQuantity(quantity);
                }
                else if (mode == SearchQuantityMode.Max)
                {
                    _spawnQuantity = MaxSpawnQuantity;
                }
                else if (mode == SearchQuantityMode.Stack)
                {
                    ApplyStackQuantityCommand();
                }
            }

            UpdateSearchCommandHint();
        }

        private static bool TryParseTrailingQuantityCommand(string rawValue, out string query, out SearchQuantityMode mode, out int quantity)
        {
            query = (rawValue ?? "").Trim();
            mode = SearchQuantityMode.None;
            quantity = 0;

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            string trimmed = rawValue.Trim();
            int lastSpace = trimmed.LastIndexOf(' ');
            string token = lastSpace >= 0 ? trimmed.Substring(lastSpace + 1) : trimmed;
            string remainingQuery = lastSpace >= 0 ? trimmed.Substring(0, lastSpace).TrimEnd() : "";

            if (!TryParseQuantityToken(token, out mode, out quantity))
            {
                return false;
            }

            query = remainingQuery;
            return true;
        }

        private static bool TryParseQuantityToken(string token, out SearchQuantityMode mode, out int quantity)
        {
            mode = SearchQuantityMode.None;
            quantity = 0;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            string normalized = token.Trim().ToLowerInvariant();
            if (normalized == "max")
            {
                mode = SearchQuantityMode.Max;
                quantity = MaxSpawnQuantity;
                return true;
            }

            if (normalized == "stack")
            {
                mode = SearchQuantityMode.Stack;
                return true;
            }

            string numeric = normalized;
            if (numeric.StartsWith("qty:", StringComparison.Ordinal) || numeric.StartsWith("qty=", StringComparison.Ordinal))
            {
                numeric = numeric.Substring(4);
            }
            else if (numeric.StartsWith("qty", StringComparison.Ordinal) && numeric.Length > 3)
            {
                numeric = numeric.Substring(3);
            }
            else if (numeric.Length > 1 && (numeric[0] == 'x' || numeric[0] == '*' || numeric[0] == 'q'))
            {
                numeric = numeric.Substring(1);
            }

            if (!IsDigitsOnly(numeric) || !int.TryParse(numeric, out quantity))
            {
                return false;
            }

            mode = SearchQuantityMode.Exact;
            return true;
        }

        private static bool IsDigitsOnly(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsDigit(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyStackQuantityCommand()
        {
            if (_searchQuantityMode != SearchQuantityMode.Stack)
            {
                return;
            }

            var item = GetCurrentSelection();
            if (item == null)
            {
                return;
            }

            _spawnQuantity = ClampSpawnQuantity(item.StackLimit);
            UpdateSearchCommandHint();
        }

        private ItemSpawner.ItemInfo GetCurrentSelection()
        {
            if (_selectedItem != null)
            {
                return _selectedItem;
            }

            if (_selectedItemIndex >= 0 && _selectedItemIndex < _currentItems.Count)
            {
                return _currentItems[_selectedItemIndex];
            }

            return _currentItems.Count > 0 ? _currentItems[0] : null;
        }

        private void SpawnCurrentSearchResult()
        {
            var item = GetCurrentSelection();
            if (item == null)
            {
                _statusSuccess = false;
                _statusMessage = "No item selected";
                ToastManager.Show(_statusMessage, SewerSkin.StatusType.Warning);
                return;
            }

            SpawnItem(item, _spawnQuantity);
        }

        private void UpdateSearchCommandHint()
        {
            if (_searchQuantityMode == SearchQuantityMode.None)
            {
                _searchCommandHint = "";
                return;
            }

            string searchLabel = string.IsNullOrWhiteSpace(_searchQuery) ? "All items" : $"Search: {_searchQuery}";
            string quantityLabel;
            switch (_searchQuantityMode)
            {
                case SearchQuantityMode.Max:
                    quantityLabel = "Qty: max";
                    break;
                case SearchQuantityMode.Stack:
                    quantityLabel = $"Qty: stack ({_spawnQuantity})";
                    break;
                default:
                    quantityLabel = $"Qty: {_spawnQuantity}";
                    break;
            }

            _searchCommandHint = searchLabel + "  |  " + quantityLabel;
        }

        private static int ClampSpawnQuantity(int quantity)
        {
            return Mathf.Clamp(quantity, 1, MaxSpawnQuantity);
        }
        
        private void DrawCategorySidebar()
        {
            GUILayout.BeginVertical(GUILayout.Width(162));
            
            Rect headerRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(headerRect, new Color(0.048f, 0.062f, 0.057f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f), 6, 1);
            
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.TextMutedColor;
            var headerStyle = SewerSkin.GetLabelStyle(11, FontStyle.Bold, TextAnchor.MiddleLeft);
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 3, 120, 20), "CATEGORIES", headerStyle);
            GUI.contentColor = oldContentColor;
            
            Rect scrollAreaRect = GUILayoutUtility.GetRect(0, 0, GUILayout.Width(158), GUILayout.ExpandHeight(true));
            SewerSkin.DrawRoundedRect(scrollAreaRect, new Color(0.035f, 0.047f, 0.043f, 0.92f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.38f), 6, 1);
            
            SmoothScroll(ref _targetCategoryScrollPos, ref _displayCategoryScrollPos, ref _categoryScrollPos, scrollAreaRect);
            _categoryScrollPos = GUILayout.BeginScrollView(_displayCategoryScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(158), GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < _categories.Count; i++)
            {
                string category = _categories[i];
                bool isSelected = i == _selectedCategoryIndex;
                
                int count = _itemsByCategory.TryGetValue(category, out var items) ? items.Count : 0;
                string label = $"{category} ({count})";
                
                Rect btnRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
                
                oldColor = GUI.color;
                if (isSelected)
                {
                    SewerSkin.DrawRoundedRect(btnRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.88f), new Color(SewerSkin.AccentGlow.r, SewerSkin.AccentGlow.g, SewerSkin.AccentGlow.b, 0.75f), 6, 1);
                }
                else
                {
                    bool isHovered = btnRect.Contains(Event.current.mousePosition);
                    SewerSkin.DrawRoundedRect(btnRect, isHovered ? SewerSkin.ButtonHoverColor : SewerSkin.ButtonColor, new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, isHovered ? 0.62f : 0.35f), 6, 1);
                }
                GUI.color = oldColor;
                
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? new Color(0.02f, 0.04f, 0.06f, 1f) : SewerSkin.TextColor;
                var btnStyle = SewerSkin.GetLabelStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft);
                GUI.Label(new Rect(btnRect.x + 8f, btnRect.y, btnRect.width - 12f, btnRect.height), label, btnStyle);
                GUI.contentColor = oldContentColor;
                
                if (IsRectClicked(btnRect))
                {
                    _selectedCategoryIndex = i;
                    _selectedItemIndex = 0;
                    _itemScrollPos = Vector2.zero;
                    _targetItemScrollPos = Vector2.zero;
                    _displayItemScrollPos = Vector2.zero;
                }
                
                GUILayout.Space(2);
            }
            
            GUILayout.EndScrollView();
            if (_categoryScrollPos != _displayCategoryScrollPos)
            {
                _targetCategoryScrollPos = _categoryScrollPos;
                _displayCategoryScrollPos = _categoryScrollPos;
            }
            GUILayout.EndVertical();
        }
        
        private void DrawItemList()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            Rect headerRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(headerRect, new Color(0.048f, 0.062f, 0.057f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f), 6, 1);
            
            string categoryName = _selectedCategoryIndex < _categories.Count ? _categories[_selectedCategoryIndex] : "Items";
            var oldContentColor = GUI.contentColor;
            GUI.contentColor = SewerSkin.AccentColor;
            var headerStyle = SewerSkin.GetLabelStyle(11, FontStyle.Bold, TextAnchor.MiddleLeft);
            bool hasSearchQuery = !string.IsNullOrWhiteSpace(_searchQuery);
            string headerText = !hasSearchQuery
                ? categoryName.ToUpper()
                : $"SEARCH: {_searchQuery}";
            GUI.Label(new Rect(headerRect.x + 8, headerRect.y + 3, headerRect.width - 150, 20), headerText, headerStyle);
            
            GUI.contentColor = SewerSkin.TextMutedColor;
            var countStyle = SewerSkin.GetLabelStyle(11, FontStyle.Normal, TextAnchor.MiddleRight);
            int totalInCategory = _itemsByCategory.TryGetValue(categoryName, out var categoryItems) ? categoryItems.Count : _currentItems.Count;
            string countText = !hasSearchQuery
                ? $"{_currentItems.Count} items"
                : $"{_currentItems.Count} of {totalInCategory}";
            GUI.Label(new Rect(headerRect.x + headerRect.width - 112, headerRect.y + 3, 102, 20), countText, countStyle);
            GUI.contentColor = oldContentColor;
            
            Rect scrollAreaRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            SewerSkin.DrawRoundedRect(scrollAreaRect, new Color(0.035f, 0.047f, 0.043f, 0.92f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.38f), 6, 1);
            
            SmoothScroll(ref _targetItemScrollPos, ref _displayItemScrollPos, ref _itemScrollPos, scrollAreaRect);
            _itemScrollPos = GUILayout.BeginScrollView(_displayItemScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            int itemCount = _currentItems.Count;
            for (int i = 0; i < itemCount; i++)
            {
                var item = _currentItems[i];
                bool isSelected = i == _selectedItemIndex;
                
                Rect rowRect = GUILayoutUtility.GetRect(0, ItemRowHeight, GUILayout.ExpandWidth(true));
                
                oldColor = GUI.color;
                if (isSelected)
                {
                    SewerSkin.DrawRoundedRect(rowRect, new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.14f), new Color(SewerSkin.AccentColor.r, SewerSkin.AccentColor.g, SewerSkin.AccentColor.b, 0.55f), 6, 1);
                }
                else
                {
                    bool isHovered = rowRect.Contains(Event.current.mousePosition);
                    SewerSkin.DrawRoundedRect(rowRect, isHovered ? new Color(0.09f, 0.11f, 0.105f, 0.96f) : new Color(0.055f, 0.069f, 0.065f, 0.74f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, isHovered ? 0.55f : 0.28f), 6, 1);
                }
                
                if (isSelected)
                {
                    SewerSkin.DrawRoundedRect(new Rect(rowRect.x + 4, rowRect.y + 6, 3, rowRect.height - 12), SewerSkin.AccentColor, Color.clear, 2, 0);
                }
                GUI.color = oldColor;
                
                float xPos = rowRect.x + 10;
                
                Rect nameRect = new Rect(xPos, rowRect.y + 5, rowRect.width - 230, 18);
                oldContentColor = GUI.contentColor;
                GUI.contentColor = isSelected ? SewerSkin.AccentGlow : SewerSkin.TextColor;
                var nameStyle = SewerSkin.GetLabelStyle(11, isSelected ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleLeft);
                GUI.Label(nameRect, item.Name ?? "???", nameStyle);

                GUI.contentColor = SewerSkin.TextMutedColor;
                var idStyle = SewerSkin.GetLabelStyle(9, FontStyle.Normal, TextAnchor.MiddleLeft);
                GUI.Label(new Rect(xPos, rowRect.y + 23, rowRect.width - 230, 14), item.Id ?? "", idStyle);
                GUI.contentColor = oldContentColor;
                
                if (IsRectClicked(new Rect(rowRect.x, rowRect.y, rowRect.width - 70, rowRect.height)))
                {
                    _selectedItemIndex = i;
                    _selectedItem = item;
                    ApplyStackQuantityCommand();
                }
                
                float tagX = rowRect.x + rowRect.width - 200;
                oldContentColor = GUI.contentColor;
                GUI.contentColor = SewerSkin.TextMutedColor;
                var tagStyle = SewerSkin.GetLabelStyle(10);
                GUI.Label(new Rect(tagX, rowRect.y + 10, 92, 18), $"[{item.Category ?? "?"}]", tagStyle);
                
                GUI.contentColor = new Color(0.5f, 0.55f, 0.6f, 1f);
                GUI.Label(new Rect(tagX + 94, rowRect.y + 10, 40, 18), $"x{item.StackLimit}", tagStyle);
                GUI.contentColor = oldContentColor;
                
                Rect addRect = new Rect(rowRect.x + rowRect.width - 58, rowRect.y + 7, 52, 24);
                if (DrawStyledButton(addRect, "+ Add", false, false, true))
                {
                    SpawnItem(item, _spawnQuantity);
                }
                
                GUILayout.Space(2);
            }
            if (_currentItems.Count == 0)
            {
                GUILayout.Space(12);
                string message = !hasSearchQuery
                    ? "Try another category or refresh the item registry."
                    : "Try a shorter name, item ID, or a rough spelling.";
                SewerSkin.DrawEmptyState("No items found", message, SewerSkin.StatusType.Warning);
            }
            
            GUILayout.EndScrollView();
            if (_itemScrollPos != _displayItemScrollPos)
            {
                _targetItemScrollPos = _itemScrollPos;
                _displayItemScrollPos = _itemScrollPos;
            }
            GUILayout.EndVertical();
        }

        private void SmoothScroll(ref Vector2 target, ref Vector2 display, ref Vector2 current, Rect hitRect)
        {
            Event e = Event.current;
            if (e.type == EventType.ScrollWheel && hitRect.Contains(e.mousePosition))
            {
                target.y = Mathf.Max(0f, target.y + e.delta.y * 30f);
                e.Use();
            }

            float uiDelta = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
            float scrollSpeed = Mathf.Lerp(28f, 16f, SewerSkin.AnimationStrength);
            float lerp = 1f - Mathf.Exp(-uiDelta * scrollSpeed);
            display = Vector2.Lerp(display, target, lerp);
            current = display;
        }
        
        private void DrawBottomBar()
        {
            Rect lineRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
            var oldColor = GUI.color;
            GUI.color = SewerSkin.AccentColor;
            GUI.DrawTexture(lineRect, _solidTex);
            GUI.color = oldColor;
            
            GUILayout.Space(4);
            
            Rect footerRect = GUILayoutUtility.GetRect(0, 66, GUILayout.ExpandWidth(true));
            oldColor = GUI.color;
            SewerSkin.DrawRoundedRect(footerRect, new Color(0.035f, 0.046f, 0.043f, 0.98f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.5f), 6, 1);
            
            float xPos = footerRect.x + 10;
            float yPos = footerRect.y + 8;
            float actionWidth = 150f;
            float detailWidth = footerRect.width - actionWidth - 28f;

            var oldContentColor = GUI.contentColor;
            if (_selectedItem != null)
            {
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(new Rect(xPos, yPos, 55, 20), "Selected:", SewerSkin.GetLabelStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft));

                GUI.contentColor = SewerSkin.AccentGlow;
                var nameStyle = SewerSkin.GetLabelStyle(12, FontStyle.Bold, TextAnchor.MiddleLeft);
                GUI.Label(new Rect(xPos + 58, yPos, detailWidth - 58, 20), _selectedItem.Name, nameStyle);

                GUI.contentColor = SewerSkin.TextMutedColor;
                var detailStyle = SewerSkin.GetLabelStyle(10, FontStyle.Normal, TextAnchor.MiddleLeft);
                string detail = $"ID: {_selectedItem.Id}  |  Category: {_selectedItem.Category}  |  Stack: {_selectedItem.StackLimit}";
                GUI.Label(new Rect(xPos, yPos + 23, detailWidth, 18), detail, detailStyle);
            }
            else
            {
                GUI.contentColor = SewerSkin.TextMutedColor;
                GUI.Label(new Rect(xPos, yPos + 9, detailWidth, 22), "Select an item from the list above", SewerSkin.GetLabelStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft));
            }

            int totalItems = _itemsByCategory.TryGetValue("All", out var all) ? all.Count : 0;
            GUI.contentColor = SewerSkin.TextMutedColor;
            var countStyle = SewerSkin.GetLabelStyle(12, FontStyle.Normal, TextAnchor.MiddleRight);
            GUI.Label(new Rect(footerRect.x + footerRect.width - actionWidth - 108, footerRect.y + 9, 100, 20), $"{totalItems} total", countStyle);
            GUI.contentColor = oldContentColor;

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                GUI.contentColor = _statusSuccess ? SewerSkin.SuccessColor : SewerSkin.WarningColor;
                var statusStyle = SewerSkin.GetLabelStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft);
                GUI.Label(new Rect(footerRect.x + 10, footerRect.y + 45, footerRect.width - 170, 18), _statusMessage, statusStyle);
                GUI.contentColor = oldContentColor;
            }
            
            Rect spawnRect = new Rect(footerRect.x + footerRect.width - actionWidth, footerRect.y + 9, actionWidth - 10, 32);
            if (_selectedItem != null)
            {
                string btnText = _spawnQuantity > 1 ? $"SPAWN {_spawnQuantity}x" : "SPAWN ITEM";
                if (DrawStyledButton(spawnRect, btnText, false, true))
                {
                    SpawnItem(_selectedItem, _spawnQuantity);
                }
            }
            else
            {
                SewerSkin.DrawRoundedRect(spawnRect, new Color(0.10f, 0.115f, 0.11f, 0.78f), new Color(SewerSkin.BorderColor.r, SewerSkin.BorderColor.g, SewerSkin.BorderColor.b, 0.35f), 7, 1);
                
                oldContentColor = GUI.contentColor;
                GUI.contentColor = new Color(0.4f, 0.42f, 0.45f, 1f);
                var disabledStyle = SewerSkin.GetLabelStyle(12, FontStyle.Normal, TextAnchor.MiddleCenter);
                GUI.Label(spawnRect, "SPAWN ITEM", disabledStyle);
                GUI.contentColor = oldContentColor;
            }
        }
        
        private bool DrawStyledButton(Rect rect, string text, bool isDanger = false, bool isAccent = false, bool isSuccess = false)
        {
            Color bgColor, hoverColor, textColor;
            if (isDanger)
            {
                bgColor = new Color(0.6f, 0.15f, 0.15f, 1f);
                hoverColor = new Color(0.7f, 0.2f, 0.2f, 1f);
                textColor = SewerSkin.TextColor;
            }
            else if (isAccent)
            {
                bgColor = SewerSkin.AccentColor;
                hoverColor = SewerSkin.AccentGlow;
                textColor = new Color(0.02f, 0.04f, 0.06f, 1f);
            }
            else if (isSuccess)
            {
                bgColor = new Color(0.15f, 0.4f, 0.2f, 1f);
                hoverColor = new Color(0.2f, 0.5f, 0.25f, 1f);
                textColor = SewerSkin.TextColor;
            }
            else
            {
                bgColor = SewerSkin.ButtonColor;
                hoverColor = SewerSkin.ButtonHoverColor;
                textColor = SewerSkin.TextColor;
            }

            return SewerSkin.DrawButtonRect(rect, text, bgColor, hoverColor, textColor, 7, isAccent || isSuccess, 11);
        }

        private static bool IsRectClicked(Rect rect)
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.MouseDown || e.button != 0 || !rect.Contains(e.mousePosition))
            {
                return false;
            }

            e.Use();
            return true;
        }
        
        #endregion
        
        #region Spawning
        
        private void SpawnItem(ItemSpawner.ItemInfo item, int quantity)
        {
            if (item == null || item.Definition == null)
            {
                SewerLogger.Warning("Cannot spawn - invalid item");
                return;
            }
            
            try
            {
                bool success = GameTypes.AddItemToInventory(item.Definition, quantity);
                if (success)
                {
                    _statusSuccess = true;
                    _statusMessage = $"Spawned {quantity}x {item.Name}";
                    ToastManager.Show(_statusMessage, SewerSkin.StatusType.Success);
                    SewerLogger.Success($"Spawned {quantity}x {item.Name} ({item.Id})");
                }
                else
                {
                    _statusSuccess = false;
                    _statusMessage = GameTypes.LastInventoryError ?? $"Failed to spawn {item.Name}";
                    ToastManager.Show(_statusMessage, SewerSkin.StatusType.Warning);
                    SewerLogger.Warning($"Failed to spawn {item.Name} ({item.Id})");
                }
            }
            catch (Exception ex)
            {
                ToastManager.Show($"Spawn failed: {ex.Message}", SewerSkin.StatusType.Error);
                SewerLogger.Error($"Error spawning {item.Name}", ex);
            }
        }
        
        #endregion

        private sealed class SearchEntry
        {
            public ItemSpawner.ItemInfo Item;
            public string Name;
            public string Id;
            public string Category;
            public string Acronym;
        }

        private enum SearchQuantityMode
        {
            None,
            Exact,
            Max,
            Stack
        }

        private readonly struct ScoredItem
        {
            public readonly ItemSpawner.ItemInfo Item;
            public readonly int Score;

            public ScoredItem(ItemSpawner.ItemInfo item, int score)
            {
                Item = item;
                Score = score;
            }
        }
    }
}
