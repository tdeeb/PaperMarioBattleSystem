﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// A SequenceAction that waits for an animation to finish before proceeding.
    /// Do not use this for animations that loop infinitely, as this will never end
    /// <para>Make sure to play the animation for the BattleEntity before waiting on this.</para>
    /// </summary>
    public class WaitForAnimationSeqAction : SequenceAction
    {
        protected IAnimation Anim = null;

        public WaitForAnimationSeqAction(BattleEntity entity, string animName) : base(entity)
        {
            Anim = Entity.AnimManager.GetAnimation<IAnimation>(animName);
        }

        public WaitForAnimationSeqAction(IAnimation anim)
        {
            Anim = anim;
        }

        protected override void OnUpdate()
        {
            //If the animation doesn't exist, end immediately
            if (Anim == null)
            {
                End();
                return;
            }

            if (Anim.Finished == true)
            {
                End();
            }
        }
    }
}
