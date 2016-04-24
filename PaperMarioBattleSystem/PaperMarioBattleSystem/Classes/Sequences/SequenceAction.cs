﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperMarioBattleSystem
{
    ///<summary>
    /// An action that occurs over time during a BattleAction.
    /// This can be used to do about anything to the BattleEntity performing the action.
    /// Examples include moving, waiting, playing an animation, and more
    /// </summary>
    public abstract class SequenceAction
    {
        public double Duration { get; protected set; } = 0d;
        protected double StartTime = 0d;
        protected bool Done { get; private set; } = false;

        public BattleEntity Entity => BattleManager.Instance.EntityTurn;

        public bool IsDone => Done;

        protected SequenceAction()
        {
            
        }

        protected SequenceAction(double duration)
        {
            Duration = duration;
            if (Duration < 0f)
            {
                Debug.LogWarning($"{nameof(Duration)} is negative - turning it positive!");
                Duration = Math.Abs(Duration);
            }
        }

        public void Start()
        {
            StartTime = Time.ActiveMilliseconds;

            OnStart();
        }

        protected virtual void OnStart()
        {
            
        }

        public void End()
        {
            Done = true;

            OnEnd();
        }

        protected virtual void OnEnd()
        {

        }

        public void Update()
        {
            if (Done == false)
                OnUpdate();
        }

        protected abstract void OnUpdate();
    }
}