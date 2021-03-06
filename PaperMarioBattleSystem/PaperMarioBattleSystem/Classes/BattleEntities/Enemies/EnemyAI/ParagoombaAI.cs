﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaperMarioBattleSystem.Extensions;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// Paragoomba enemy AI.
    /// </summary>
    public sealed class ParagoombaAI : GoombaAI
    {
        private Paragoomba paragoomba = null;

        public ParagoombaAI(Paragoomba pGoomba) : base(pGoomba)
        {
            paragoomba = pGoomba;
        }

        public override void PerformAction()
        {
            if (paragoomba.WingedBehavior.Grounded == false)
            {
                //Try to use an item; if so, return
                if (TryUseItem() == true) return;

                Enemy.StartAction(new DiveKickAction(Enemy), false, Enemy.BManager.FrontPlayer.GetTrueTarget());
            }
            else
            {
                base.PerformAction();
            }
        }
    }
}
