﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// The Sequence for Kooper's Shell Toss.
    /// </summary>
    public sealed class ShellTossSequence : Sequence
    {
        private float WalkDuration = 500f;
        private float SpinMoveDuration = 1000f;
        private int DamageMod = 1;

        private HammerActionCommandUI ShellTossUI = null;

        public ShellTossSequence(MoveAction moveAction) : base(moveAction)
        {
            
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (Action.DrawActionCommandInfo == true)
            {
                ShellTossUI = new HammerActionCommandUI(actionCommand as HammerCommand);
                User.BManager.battleUIManager.AddUIElement(ShellTossUI);
            }
        }

        protected override void OnEnd()
        {
            base.OnEnd();

            if (ShellTossUI != null)
            {
                User.BManager.battleUIManager.RemoveUIElement(ShellTossUI);
                ShellTossUI = null;
            }
        }

        protected override void CommandSuccess()
        {
            DamageMod *= 2;
            SpinMoveDuration /= 2f;

            ChangeSequenceBranch(SequenceBranch.Success);
        }

        protected override void CommandFailed()
        {
            ChangeSequenceBranch(SequenceBranch.Failed);
        }

        protected override void SequenceStartBranch()
        {
            switch (SequenceStep)
            {
                case 0:
                    User.AnimManager.PlayAnimation(AnimationGlobals.ShelledBattleAnimations.EnterShellName);

                    //Kooper, Koops, Koopa Troopas, Buzzy Beetles, and etc. always use Shell Toss in place
                    Vector2 front = User.BattlePosition;

                    CurSequenceAction = new WaitForAnimationSeqAction(User, AnimationGlobals.ShelledBattleAnimations.EnterShellName);
                    ChangeSequenceBranch(SequenceBranch.Main);
                    break;
                default:
                    PrintInvalidSequence();
                    break;
            }
        }

        protected override void SequenceMainBranch()
        {
            switch (SequenceStep)
            {
                case 0:
                    User.AnimManager.PlayAnimation(AnimationGlobals.ShelledBattleAnimations.ShellSpinName, true);

                    double animLength = User.AnimManager.GetAnimation<Animation>(AnimationGlobals.ShelledBattleAnimations.ShellSpinName).GetTotalFrameLength();

                    StartActionCommandInput();
                    CurSequenceAction = new WaitForCommandSeqAction(animLength, actionCommand, CommandEnabled);
                    break;
                default:
                    PrintInvalidSequence();
                    break;
            }
        }

        protected override void SequenceSuccessBranch()
        {
            switch (SequenceStep)
            {
                case 0:
                    User.AnimManager.GetAnimation<Animation>(AnimationGlobals.ShelledBattleAnimations.ShellSpinName)?.SetSpeed(3f);

                    Vector2 pos = BattleManagerUtils.GetPositionInFront(EntitiesAffected[0], User.EntityType != Enumerations.EntityTypes.Enemy);

                    CurSequenceAction = new MoveToSeqAction(User, new Vector2(pos.X, User.Position.Y), SpinMoveDuration);
                    break;
                case 1:
                    InteractionResult[] interactions = AttemptDamage(BaseDamage * DamageMod, EntitiesAffected[0], Action.DamageProperties, false);

                    if (interactions[0] != null && interactions[0].WasVictimHit == true && interactions[0].WasAttackerHit == false)
                        ShowCommandRankVFX(HighestCommandRank, EntitiesAffected[0].Position);

                    ChangeSequenceBranch(SequenceBranch.End);
                    break;
                default:
                    PrintInvalidSequence();
                    break;
            }
        }

        protected override void SequenceFailedBranch()
        {
            switch (SequenceStep)
            {
                case 0:
                    User.AnimManager.GetAnimation<Animation>(AnimationGlobals.ShelledBattleAnimations.ShellSpinName)?.SetSpeed(2f);

                    Vector2 pos = BattleManagerUtils.GetPositionInFront(EntitiesAffected[0], User.EntityType != Enumerations.EntityTypes.Enemy);

                    CurSequenceAction = new MoveToSeqAction(User, new Vector2(pos.X, User.Position.Y), SpinMoveDuration);
                    break;
                case 1:
                    AttemptDamage(BaseDamage * DamageMod, EntitiesAffected[0], Action.DamageProperties, false);
                    ChangeSequenceBranch(SequenceBranch.End);
                    break;
                default:
                    PrintInvalidSequence();
                    break;
            }
        }

        protected override void SequenceEndBranch()
        {
            switch (SequenceStep)
            {
                case 0:
                    User.AnimManager.PlayAnimation(AnimationGlobals.ShelledBattleAnimations.ExitShellName, true);
                    CurSequenceAction = new WaitForAnimationSeqAction(User, AnimationGlobals.ShelledBattleAnimations.ExitShellName);
                    break;
                case 1:
                    User.AnimManager.PlayAnimation(AnimationGlobals.RunningName, true);
                    CurSequenceAction = new MoveToSeqAction(User, User.BattlePosition, WalkDuration);
                    break;
                case 2:
                    User.AnimManager.PlayAnimation(User.GetIdleAnim(), true);
                    EndSequence();
                    break;
                default:
                    PrintInvalidSequence();
                    break;
            }
        }

        protected override void SequenceMissBranch()
        {
            switch (SequenceStep)
            {
                default:
                    PrintInvalidSequence();
                    break;
            }
        }
    }
}
