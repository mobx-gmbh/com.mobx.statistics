using JetBrains.Annotations;
using MobX.Mediator.Events;
using MobX.Serialization;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MobX.Analysis
{
    public abstract class StatAsset : RuntimeAsset
    {
        #region Fields

        [Foldout("Settings")]
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;

        [FormerlySerializedAs("elevation")]
        [SpaceBefore]
        [Tooltip("Determines the level on which the stat is saved. Profile specific or shared.")]
        [SerializeField] private StatStage stage = StatStage.Profile;
        [Tooltip("When enabled, the objects inspector is repainted when the stat is updated.")]
        [SerializeField] private bool repaint;
        [Tooltip("When enabled, the stat is saved every time it is updated.")]
        [SerializeField] private bool autoSave;
        [Tooltip("When enabled, the stat is saved when the application is shutdown.")] [SpaceAfter]
        [SerializeField] private bool saveOnQuit;

        [Foldout("Mediator", false)]
        [SerializeField] private EventAsset<StatAsset> statUpdated;

        #endregion


        #region Public

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

        #endregion


        #region Protected

        protected bool AutoSave => autoSave;
        protected bool SaveOnQuit => saveOnQuit;
        protected new bool Repaint => repaint;
        protected IProfile Profile =>
            stage switch
            {
                StatStage.Profile => FileSystem.Profile,
                StatStage.SharedProfile => FileSystem.SharedProfile,
                var _ => throw new ArgumentOutOfRangeException()
            };

        #endregion
    }
}
