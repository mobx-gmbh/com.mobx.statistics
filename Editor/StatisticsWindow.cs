using MobX.Utilities;
using MobX.Utilities.Editor.AssetManagement;
using MobX.Utilities.Editor.Helper;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace MobX.Analysis.Editor
{
    public class StatisticsWindow : UnityEditor.EditorWindow
    {
        [UnityEditor.MenuItem("Tools/Statistics", priority = 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<StatisticsWindow>("Statistics");
            window.Show(false);
        }

        private readonly List<StatAsset> _stats = new();
        private readonly List<StatAsset> _filteredList = new();
        private ReorderableList _reorderableList;
        private Vector2 _scrollPosition;
        private bool _isMouseEvent;
        private string _filter;
        private StatAsset _selected;


        #region Setup

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Shutdown();
        }

        private void Initialize()
        {
            _filter = UnityEditor.EditorPrefs.GetString("StatisticsWindowFilter", string.Empty);
            _stats.Clear();
            _filteredList.Clear();
            _stats.AddRange(AssetDatabaseUtilities.FindAssetsOfType<StatAsset>());
            _reorderableList = new ReorderableList(_filteredList, typeof(StatAsset), true, false, false, false);
            _reorderableList.drawElementCallback += DrawStat;
            _reorderableList.onSelectCallback += OnSelect;

            foreach (var statAsset in _stats)
            {
                statAsset.Updated.Remove(OnStatUpdated);
                statAsset.Updated.Add(OnStatUpdated);
            }
        }

        private void Shutdown()
        {
            _stats.Clear();
            _filteredList.Clear();
            foreach (var statAsset in _stats)
            {
                statAsset.Updated.Remove(OnStatUpdated);
            }
        }

        private void OnStatUpdated(StatAsset asset)
        {
            Repaint();
        }

        #endregion


        #region GUI

        private void OnGUI()
        {
            _isMouseEvent = Event.current.isMouse;
            UnityEditor.EditorGUILayout.BeginHorizontal();
            _filter = GUIHelper.SearchBar(_filter);
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                Initialize();
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
            GUIHelper.DrawLine();
            ApplyFilter();
            DrawHeader();
            _scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(_scrollPosition);
            _reorderableList.DoLayoutList();
            UnityEditor.EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            var rect = GUIHelper.GetControlRect();
            const float ColumnWidth = 200f;

            UnityEditor.EditorGUI.LabelField(rect, "Stat", GUIHelper.BoldLabelStyle);

            var rectOffset = ColumnWidth;

            var valueRect = new Rect(rect.x + rectOffset, rect.y, rect.width - rectOffset, rect.height);
            UnityEditor.EditorGUI.LabelField(valueRect, "Value", GUIHelper.BoldLabelStyle);
            rectOffset += ColumnWidth;

            var typeRect = new Rect(rect.x + rectOffset, rect.y, rect.width - rectOffset, rect.height);
            UnityEditor.EditorGUI.LabelField(typeRect, "Type", GUIHelper.BoldLabelStyle);
            rectOffset += ColumnWidth;

            var descriptionRect = new Rect(rect.x + rectOffset, rect.y, rect.width - rectOffset, rect.height);
            UnityEditor.EditorGUI.LabelField(descriptionRect, "Description", GUIHelper.BoldLabelStyle);
        }

        private void DrawStat(Rect rect, int index, bool isActive, bool isFocused)
        {
            var stat = (StatAsset) _reorderableList.list[index];
            const float ColumnWidth = 200f;
            var lineRect = new Rect(0, rect.y, GUIHelper.GetViewWidth(), 1);
            UnityEditor.EditorGUI.DrawRect(lineRect, new Color(0f, 0f, 0f, 0.2f));

            if (index.IsEven())
            {
                var backgroundRect = new Rect(0, rect.y, GUIHelper.GetViewWidth(), rect.height);
                UnityEditor.EditorGUI.DrawRect(backgroundRect, new Color(0f, 0f, 0f, 0.06f));
            }

            if (_reorderableList.IsSelected(index))
            {
                var selectionRect = new Rect(0, rect.y, 3, rect.height);
                UnityEditor.EditorGUI.DrawRect(selectionRect, new Color(0.7f, 1f, 0.75f, 0.9f));
            }

            UnityEditor.EditorGUI.LabelField(rect, stat.Name);

            var rectOffset = ColumnWidth;

            var valueRect = new Rect(rect.x + rectOffset, rect.y, rect.width - rectOffset, rect.height);
            UnityEditor.EditorGUI.LabelField(valueRect, stat.ValueString);
            rectOffset += ColumnWidth;

            var typeRect = new Rect(rect.x + rectOffset, rect.y, rect.width - rectOffset, rect.height);
            UnityEditor.EditorGUI.LabelField(typeRect, stat.Type().ToString());
            rectOffset += ColumnWidth;

            var descriptionRect = new Rect(rect.x + rectOffset, rect.y, rect.width - rectOffset, rect.height);
            UnityEditor.EditorGUI.LabelField(descriptionRect, stat.Description);
        }

        private void ApplyFilter()
        {
            if (_filter.IsNotNullOrWhitespace())
            {
                _filteredList.Clear();
                foreach (var statAsset in _stats)
                {
                    if (statAsset.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                    {
                        _filteredList.Add(statAsset);
                        continue;
                    }
                    if (statAsset.Type().ToString().Contains(_filter, StringComparison.OrdinalIgnoreCase))
                    {
                        _filteredList.Add(statAsset);
                    }
                }
                _reorderableList.list = _filteredList;
            }
            else
            {
                _reorderableList.list = _stats;
            }

            UnityEditor.EditorPrefs.SetString("StatisticsWindowFilter", _filter);
        }

        private void OnSelect(ReorderableList reorderable)
        {
            var isMultiSelect = reorderable.selectedIndices.Count > 1;

            if (isMultiSelect || reorderable.list.Count == 0 || reorderable.list.Count <= reorderable.index)
            {
                return;
            }

            var selected = reorderable.list[reorderable.index] as StatAsset;
            _selected = selected;
            if (_selected == null)
            {
                return;
            }

            if (_isMouseEvent && GUIHelper.IsDoubleClick(_selected))
            {
                EditorHelper.SelectObject(_selected);
            }
        }

        #endregion
    }
}