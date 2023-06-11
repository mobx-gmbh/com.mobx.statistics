﻿using MobX.Utilities.Inspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobX.Analysis
{
    public class StatAssetInt : StatAsset<ulong>
    {
        #region Fields

        [Foldout("Settings")]
        [SerializeField] private Modification type;

        [ConditionalShow(nameof(type), Modification.Increment)]
        [SerializeField] [Min(1)] private ulong increment = 1;

        [ConditionalShow(nameof(type), Modification.Minimal)]
        [SerializeField] private ulong defaultMinimal = ulong.MaxValue;

        #endregion


        #region Overrides

        protected override ulong DefaultValue()
        {
            return type == Modification.Minimal ? defaultMinimal : 0;
        }

        public override Modification Type()
        {
            return type;
        }

        public override string ValueString => Value.ToString();

        #endregion


        #region Public

        public UpdateResult<ulong> IncrementStat()
        {
            Assert.IsTrue(type == Modification.Increment);
            var from = StatData.value;
            var to = from + increment;
            StatData.value = to;
            SetStatDirty();
            return new UpdateResult<ulong>(type, from, to, true);
        }

        public UpdateResult<ulong> IncrementStat(ulong value)
        {
            Assert.IsTrue(type == Modification.Increment);
            var from = StatData.value;
            var to = from + value;
            StatData.value = to;
            SetStatDirty();
            return new UpdateResult<ulong>(type, from, to, true);
        }

        public UpdateResult<ulong> UpdateStat(ulong value)
        {
            Assert.IsTrue(type == Modification.Increment);
            var from = StatData.value;
            StatData.value = value;
            SetStatDirty();
            return new UpdateResult<ulong>(type, from, value, true);
        }

        public UpdateResult<ulong> HighscoreStat(ulong value)
        {
            Assert.IsTrue(type == Modification.Highscore);
            var current = StatData.value;
            if (current < value)
            {
                StatData.value = value;
                SetStatDirty();
                return new UpdateResult<ulong>(type, current, value, true);
            }
            return new UpdateResult<ulong>(type, current, current, false);
        }

        public UpdateResult<ulong> MinimalStat(ulong value)
        {
            Assert.IsTrue(type == Modification.Minimal);
            var current = StatData.value;
            if (current > value)
            {
                StatData.value = value;
                SetStatDirty();
                return new UpdateResult<ulong>(type, current, value, true);
            }
            return new UpdateResult<ulong>(type, current, current, false);
        }

        #endregion


        #region Debug

        [Button("Increment")]
        [Foldout("Controls")]
        [ConditionalShow(nameof(type), Modification.Increment)]
        public void ButtonIncrementStat()
        {
            IncrementStat();
        }

        [Button("Increment")]
        [Foldout("Controls")]
        [ConditionalShow(nameof(type), Modification.Increment)]
        public void ButtonIncrementStat(ulong value)
        {
            IncrementStat(value);
        }

        [Button("Update")]
        [Foldout("Controls")]
        [ConditionalShow(nameof(type), Modification.Update)]
        public void ButtonUpdateStat(ulong value)
        {
            UpdateStat(value);
        }

        [Button("Update Highscore")]
        [Foldout("Controls")]
        [ConditionalShow(nameof(type), Modification.Highscore)]
        public void ButtonHighscoreStat(ulong value)
        {
            HighscoreStat(value);
        }

        [Button("Update Minimal")]
        [Foldout("Controls")]
        [ConditionalShow(nameof(type), Modification.Minimal)]
        public void ButtonMinimalStat(ulong value)
        {
            MinimalStat(value);
        }

        #endregion
    }
}