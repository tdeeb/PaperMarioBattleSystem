﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperMarioBattleSystem
{
    ///<summary>
    /// An action that occurs over time in a Sequence.
    /// This can be used to do about anything to the BattleEntity performing the action.
    /// Examples include moving, waiting, playing an animation, and more
    /// </summary>
    public abstract class SequenceAction
    {
        public delegate void Finish();
        public Finish OnFinish { get; protected set; } = null;

        public double Duration { get; protected set; } = 0d;
        protected double StartTime = 0d;
        protected bool Done { get; private set; } = false;

        public BattleEntity Entity => BattleManager.Instance.EntityTurn;

        public bool IsDone => Done;

        protected SequenceAction()
        {
            
        }

        protected SequenceAction(double duration, Finish onFinish = null)
        {
            Duration = duration;
            if (Duration < 0f)
            {
                Debug.LogWarning($"{nameof(Duration)} is negative - turning it positive!");
                Duration = Math.Abs(Duration);
            }

            OnFinish = onFinish;
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
            OnFinish?.Invoke();

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
