﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// The Fast Status Effect.
    /// Entities afflicted with this can move an additional turn each phase it's active
    /// </summary>
    public sealed class FastStatus : StatusEffect
    {
        private const int AdditionalTurns = 1;

        public FastStatus(int duration)
        {
            StatusType = Enumerations.StatusTypes.Fast;
            Alignment = StatusAlignments.Positive;
            
            Duration = duration;
        }

        protected override void OnAfflict()
        {
            //Set this on affliction as well, as the entity could have not used its turn yet if it's in the same phase
            EntityAfflicted.SetMaxTurns(EntityAfflicted.BaseTurns + AdditionalTurns);
        }

        protected override void OnEnd()
        {
            EntityAfflicted.SetMaxTurns(EntityAfflicted.BaseTurns);
        }

        protected override void OnPhaseStart()
        {
            EntityAfflicted.SetMaxTurns(EntityAfflicted.BaseTurns + AdditionalTurns);
        }

        protected override void OnPhaseEnd()
        {
            IncrementTurns();
        }

        public override StatusEffect Copy()
        {
            return new FastStatus(Duration);
        }
    }
}