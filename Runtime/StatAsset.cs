using JetBrains.Annotations;
using MobX.Mediator.Events;
using MobX.Serialization;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using System;
using System.Linq;
using UnityEngine;

namespace MobX.Analysis
{
    public abstract class StatAsset : RuntimeAsset
    {
        [Foldout("Settings")]
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;

        [SpaceBefore]
        [Tooltip("When enabled, the objects inspector is repainted when the stat is updated.")]
        [SerializeField] private bool repaint;
        [Tooltip("When enabled, the stat is saved every time it is updated.")]
        [SerializeField] private bool autoSave;
        [SpaceAfter]
        [SerializeField] private bool saveOnQuit;

        [Foldout("Mediator", false)]
        [SerializeField] private EventAsset<StatAsset> statUpdated;

        [PublicAPI]
        public string Name => displayName;

        [PublicAPI]
        public string Description => description;

        [PublicAPI]
        public abstract string ValueString { get; }

        [PublicAPI]
        public abstract Modification Type();

        [PublicAPI]
        public EventAsset<StatAsset> Updated => statUpdated;

        protected bool AutoSave => autoSave;
        protected bool SaveOnQuit => saveOnQuit;
        protected new bool Repaint => repaint;
    }

    public abstract class StatAsset<T> : StatAsset, IOnQuit
    {
        #region Fields & Properties

        [HideInInspector] [SerializeField] private string guid;
        [NonSerialized] private StatData<T> _statData = StatData<T>.Empty;

        public event Action<T> Changed
        {
            add => _changedBroadcast.Add(value);
            remove => _changedBroadcast.Remove(value);
        }

        [ReadonlyInspector]
        public T Value => _statData.value;

        protected StatData<T> StatData => _statData;
        private readonly Broadcast<T> _changedBroadcast = new();

        #endregion


        #region Setup

        private void OnEnable()
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
#endif
            FileSystem.InitializationCompleted += Initialize;
            if (FileSystem.IsInitialized)
            {
                Initialize();
            }
        }

        private void Reset()
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
#endif
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);

            if (_statData != null)
            {
                _statData.description = Description;
                _statData.name = Name;
                _statData.type = Type();
            }
#endif
        }

        private void OnDisable()
        {
            FileSystem.InitializationCompleted -= Initialize;
        }

        private void Initialize()
        {
            FileSystem.Profile.TryGetFile(guid, out _statData);
            _statData ??= new StatData<T>(guid, Name, Description, DefaultValue(), Type());

            _statData.description = Description;
            _statData.name = Name;
            _statData.type = Type();

            FileSystem.Profile.StoreFile(guid, _statData, new StoreOptions("Statistics", typeof(T).Name));
        }

        #endregion


        #region Protected

        protected abstract T DefaultValue();

        protected void SetStatDirty()
        {
            _changedBroadcast.Raise(Value);
            FileSystem.Profile.SetDirty(guid);
            if (AutoSave)
            {
                Save();
            }
            Updated.TryRaise(this);
#if UNITY_EDITOR
            if (Repaint && UnityEditor.Selection.objects.Contains(this))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        #endregion


        #region Saving

        public void Save()
        {
            FileSystem.Profile.SaveFile(guid);
        }

        public void OnQuit()
        {
            if (SaveOnQuit)
            {
                Save();
            }
        }

        #endregion


        #region Debug

#if UNITY_EDITOR

        [Foldout("Debug")]
        [ReadonlyInspector]
        [Label("GUID")]
        private string DebugGUID => guid;

        [ReadonlyInspector]
        [Foldout("Debug")]
        [Label("Type")]
        private Modification DebugModification => Type();

        [ReadonlyInspector]
        [Foldout("Debug")]
        [Label("Default")]
        private T DebugDefaultValue => DefaultValue();

#endif

        #endregion
    }
}