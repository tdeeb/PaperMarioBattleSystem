﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaperMarioBattleSystem.Extensions;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// The Invisible Status Effect.
    /// The entity becomes transparent and its Evasion is set to the max value until it ends.
    /// </summary>
    public sealed class InvisibleStatus : MessageEventStatus
    {
        /// <summary>
        /// The Evasion modifier, which is set to 0.
        /// This makes it so no moves can hit, unless those moves are guaranteed to hit (Ex. Crystal Star Special Moves).
        /// </summary>
        private const double EvasionValue = 0d;

        private float AlphaValue = .5f;

        public InvisibleStatus(int duration)
        {
            StatusType = Enumerations.StatusTypes.Invisible;
            Alignment = StatusAlignments.Positive;

            StatusIcon = new CroppedTexture2D(AssetManager.Instance.LoadRawTexture2D($"{ContentGlobals.UIRoot}/Battle/BattleGFX.png"),
                new Rectangle(707, 105, 40, 48));

            Duration = duration;

            AfflictedMessage = "Invisible! Attacks will now\nmiss!";
        }

        protected override void OnAfflict()
        {
            EntityAfflicted.AddEvasionMod(EvasionValue);

            EntityAfflicted.TintColor = EntityAfflicted.TintColor.CeilingMult(AlphaValue);

            base.OnAfflict();
        }

        protected override void OnEnd()
        {
            EntityAfflicted.RemoveEvasionMod(EvasionValue);

            Color color = EntityAfflicted.TintColor;
            float val = (1 / AlphaValue);
            EntityAfflicted.TintColor *= val;

            base.OnEnd();
        }

        protected override void OnPhaseCycleStart()
        {
            ProgressTurnCount();
        }

        protected override void OnSuppress(Enumerations.StatusSuppressionTypes statusSuppressionType)
        {
            if (statusSuppressionType == Enumerations.StatusSuppressionTypes.Effects)
            {
                OnEnd();
            }
        }

        protected override void OnUnsuppress(Enumerations.StatusSuppressionTypes statusSuppressionType)
        {
            if (statusSuppressionType == Enumerations.StatusSuppressionTypes.Effects)
            {
                OnAfflict();
            }
        }

        public override StatusEffect Copy()
        {
            return new InvisibleStatus(Duration);
        }
    }
}
