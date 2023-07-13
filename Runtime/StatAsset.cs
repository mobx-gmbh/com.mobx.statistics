using MobX.Mediator.Events;
using MobX.Serialization;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using System;
using System.Linq;
using UnityEngine;

namespace MobX.Analysis
{
    public abstract class StatAsset<T> : StatAsset, IOnQuit
    {
        #region Fields & Properties

        [HideInInspector]
        [SerializeField] private string guid;
        [NonSerialized] private StatData<T> _statData = StatData<T>.Empty;

        public event Action<T> Changed
        {
            add => _changedBroadcast.Add(value);
            remove => _changedBroadcast.Remove(value);
        }

        [ReadonlyInspector]
        [Foldout("Debug")]
        public T Value => _statData != null ? _statData.value : default(T);

        protected StatData<T> StatData => _statData;
        private readonly Broadcast<T> _changedBroadcast = new();

        #endregion


        #region Setup

        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
#endif
            FileSystem.InitializationCompleted += Initialize;
            FileSystem.ShutdownCompleted += Shutdown;
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

        protected override void OnDisable()
        {
            base.OnDisable();
            FileSystem.InitializationCompleted -= Initialize;
            FileSystem.InitializationCompleted -= Shutdown;
        }

        private void Initialize()
        {
            Profile.TryGetFile(guid, out _statData);
            _statData ??= new StatData<T>(guid, Name, Description, DefaultValue(), Type());

            _statData.description = Description;
            _statData.name = Name;
            _statData.type = Type();

            Profile.StoreFile(guid, _statData, new StoreOptions("Statistics", typeof(T).Name));
        }

        private void Shutdown()
        {
            _statData = null;
        }

        #endregion


        #region Protected

        protected abstract T DefaultValue();

        protected void SetStatDirty()
        {
            _changedBroadcast.Raise(Value);
            Profile.SetDirty(guid);
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

        [Foldout("Debug")]
        [Button]
        public void Save()
        {
            Profile.SaveFile(guid);
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
        private string DebugGuid => guid;

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
