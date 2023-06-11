using MobX.Utilities;
using MobX.Utilities.Editor.AssetManagement;
using MobX.Utilities.Editor.Helper;
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

        private List<StatAsset> _statAssets;
        private ReorderableList _reorderableList;

        private void OnEnable()
        {
            _statAssets = AssetDatabaseUtilities.FindAssetsOfType<StatAsset>();
            _reorderableList = new ReorderableList(_statAssets, typeof(StatAsset), false, false, false, false);
            _reorderableList.drawElementCallback += DrawStat;

            foreach (var statAsset in _statAssets)
            {
                statAsset.Updated.Add(OnStatUpdated);
            }
        }

        private void OnDisable()
        {
            foreach (var statAsset in _statAssets)
            {
                statAsset.Updated.Remove(OnStatUpdated);
            }
        }

        private void OnStatUpdated(StatAsset asset)
        {
            Repaint();
        }

        private void DrawStat(Rect rect, int index, bool isActive, bool isFocused)
        {
            var stat = _statAssets[index];
            const float ColumnWidth = 200f;
            var lineRect = new Rect(0, rect.y, GUIHelper.GetViewWidth(), 1);
            UnityEditor.EditorGUI.DrawRect(lineRect, new Color(0f, 0f, 0f, 0.2f));

            if (index.IsEven())
            {
                var backgroundRect = new Rect(0, rect.y, GUIHelper.GetViewWidth(), rect.height);
                UnityEditor.EditorGUI.DrawRect(backgroundRect, new Color(0f, 0f, 0f, 0.03f));
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
            rectOffset += ColumnWidth;
        }

        private void OnGUI()
        {
            _reorderableList.DoLayoutList();
        }
    }
}