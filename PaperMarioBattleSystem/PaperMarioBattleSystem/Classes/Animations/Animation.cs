﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// The base class for sprite animation.
    /// It plays an animation forwards once, stopping on the last frame.
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// The sprite sheet containing the sprites to be used in the animation
        /// </summary>
        public Texture2D SpriteSheet = null;
        protected Frame[] Frames = null;

        /// <summary>
        /// The max number of frames in the animation
        /// </summary>
        protected int MaxFrames = 1;
        protected int CurFrameNum = 0;

        /// <summary>
        /// Whether the animation is complete or not. This is a separate value to maximize flexibility
        /// </summary>
        protected bool AnimDone = false;
        protected double PrevFrameTimer { get; private set; } = 0d;

        /// <summary>
        /// Specifies whether the animation should be rendered on the UI layer or not
        /// </summary>
        public bool IsUIAnim = false;

        /// <summary>
        /// The speed the animation plays back at
        /// </summary>
        public float Speed { get; protected set; } = 1f;

        /// <summary>
        /// Whether the animation is paused or not
        /// </summary>
        public bool Paused { get; private set; } = false;
        private double PausedTime = 0d;

        public bool Finished => AnimDone;
        protected int MaxFrameIndex => MaxFrames - 1;
        protected Frame CurFrame => Frames[CurFrameNum];

        public Animation(Texture2D spriteSheet, params Frame[] frames)
        {
            SpriteSheet = spriteSheet;
            Frames = frames;

            MaxFrames = Frames.Length;
        }

        public Animation (Texture2D spriteSheet, bool isUIAnim, params Frame[] frames) : this(spriteSheet, frames)
        {
            IsUIAnim = isUIAnim;
        }

        public Animation(Texture2D spriteSheet, float speed, bool isUIAnim, params Frame[] frames) : this (spriteSheet, isUIAnim, frames)
        {
            
        }

        protected double ElapsedFrameTime => (Time.ActiveMilliseconds - PrevFrameTimer);
        protected double TrueCurFrameDuration => (CurFrame.Duration * (Speed <= 0f ? 0f : (1f/Speed)));

        /// <summary>
        /// Tells if the current frame is complete
        /// </summary>
        /// <returns>true if the time passed is greater than the current frame's duration, otherwise false</returns>
        protected bool FrameComplete()
        {
            return (ElapsedFrameTime >= TrueCurFrameDuration);
        }

        /// <summary>
        /// Progresses through the animation
        /// </summary>
        protected virtual void Progress()
        {
            CurFrameNum++;
            if (CurFrameNum >= MaxFrames)
            {
                CurFrameNum = MaxFrameIndex;

                AnimDone = true;
            }

            ResetFrameDur();
        }

        /// <summary>
        /// Plays the animation from the start
        /// </summary>
        public void Play()
        {
            Reset();
            Resume();
        }

        /// <summary>
        /// Pauses the animation
        /// </summary>
        public void Pause()
        {
            Paused = true;

            //Store the amount of time elapsed
            PausedTime = ElapsedFrameTime;
        }

        /// <summary>
        /// Resumes the animation
        /// </summary>
        public void Resume()
        {
            Paused = false;
            ResetFrameDur();

            //Put back the elapsed time
            PrevFrameTimer += PausedTime;
        }

        /// <summary>
        /// Resets the animation to the start
        /// </summary>
        public void Reset()
        {
            AnimDone = false;
            CurFrameNum = 0;
            PausedTime = 0d;
            ResetFrameDur();

            DerivedReset();
        }

        protected virtual void DerivedReset()
        {

        }

        /// <summary>
        /// Sets the Speed the animation plays at
        /// </summary>
        /// <param name="newSpeed">The new speed of the animation. This value cannot be lower than 0 as reversed playback is
        /// not supported through the speed</param>
        public void SetSpeed(float newSpeed)
        {
            Speed = UtilityGlobals.Clamp(newSpeed, 0, float.MaxValue);
        }

        /// <summary>
        /// Resets the frame timer to the start of the current frame
        /// </summary>
        protected void ResetFrameDur()
        {
            PrevFrameTimer = Time.ActiveMilliseconds;
        }

        public void Update()
        {
            if (Paused == false && AnimDone == false)
            {
                //Progress if it's over 
                if (FrameComplete() == true)
                {
                    Progress();
                }
            }
        }

        public void Draw(Vector2 position, Color color, bool flipped, float layer)
        {
            CurFrame.Draw(SpriteSheet, position, color, flipped, layer, IsUIAnim);
        }

        /// <summary>
        /// An animation frame
        /// </summary>
        public struct Frame
        {
            /// <summary>
            /// A rectangle designating the region on the sprite sheet to draw
            /// </summary>
            public Rectangle DrawRegion;

            /// <summary>
            /// How long the frame lasts, in milliseconds
            /// </summary>
            public double Duration;

            public Frame(Rectangle drawRegion, double duration)
            {
                DrawRegion = drawRegion;
                Duration = duration;
            }

            public void Draw(Texture2D spriteSheet, Vector2 position, Color color, bool flipped, float layer, bool uibatch)
            {
                SpriteRenderer.Instance.Draw(spriteSheet, position, DrawRegion, color, flipped, layer, uibatch);
            }
        }
    }
}
