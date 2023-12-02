﻿using JetBrains.Annotations;
using MobX.Inspector;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Deprecated;
using MobX.Mediator.Events;
using MobX.Serialization;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Localization;

namespace MobX.Statistics
{
    public abstract class StatAsset : ScriptableAsset
    {
        #region Fields

        [Foldout("Settings")]
        [Title("Localization")]
        [SerializeField] private LocalizedString displayName;
        [SerializeField] private LocalizedString description;

        [Title("Persistent Data")]
        [Tooltip("Determines the level on which the stat is saved. Profile specific or shared.")]
        [LabelText("Save To")]
        [SerializeField] private StorageLevel stage = StorageLevel.Profile;
        [Tooltip("When enabled, the stat is saved every time it is updated.")]
        [SerializeField] private bool autoSave;
        [Tooltip("When enabled, the stat is saved when the application is shutdown.")]
        [SerializeField] private bool saveOnQuit;
        [Tooltip("When enabled, the objects inspector is repainted when the stat is updated.")]
        [SerializeField] private bool repaint;

        #endregion


        #region Public

        [PublicAPI]
        public string Name => displayName.IsEmpty ? name : displayName.GetLocalizedString();

        [PublicAPI]
        public string Description => description.IsEmpty ? "Missing Description" : description.GetLocalizedString();

        [PublicAPI]
        public abstract string ValueString { get; }

        [PublicAPI]
        public abstract Modification Type();

        [PublicAPI]
        public static IBroadcast<StatAsset> Updated { get; } = new Broadcast<StatAsset>();

        #endregion


        #region Protected

        protected bool AutoSave => autoSave;
        protected bool SaveOnQuit => saveOnQuit;
        protected new bool Repaint => repaint;

        protected IProfile Profile =>
            stage switch
            {
                StorageLevel.Profile => FileSystem.Profile,
                StorageLevel.SharedProfile => FileSystem.SharedProfile,
                var _ => throw new ArgumentOutOfRangeException()
            };

        #endregion
    }
}