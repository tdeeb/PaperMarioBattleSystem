﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PaperMarioBattleSystem
{
    #region Structs

    /// <summary>
    /// A structure containing all the stats in the game.
    /// Mario is the only character that uses all of them
    /// </summary>
    public struct Stats
    {
        public int Level;

        //Max stats
        public int MaxHP;
        public int MaxFP;

        //Base stats going into battle
        public int BaseAttack;
        public int BaseDefense;

        public int HP;
        public int FP;
        public int Attack;
        public int Defense;

        /// <summary>
        /// Default stats
        /// </summary>
        public static Stats Default => new Stats(1, 10, 5, 1, 0);

        public Stats(int level, int maxHp, int maxFP, int attack, int defense)
        {
            Level = level;
            MaxHP = HP = maxHp;
            MaxFP = FP = maxFP;
            BaseAttack = Attack = attack;
            BaseDefense = Defense = defense;
        }
    }

    #endregion

    public static class Enumerations
    {
        public enum EntityTypes
        {
            Player, Enemy
        }

        public enum PartnerTypes
        {
            None, Goombario, Kooper, Bombette, Parakarry, Bow, Watt, Sushie, Lakilester, Goompa, Goombaria, Twink
        }

        public enum BattleActions
        {
            Misc, Item, Jump, Hammer, Focus, Special
        }

        /// <summary>
        /// The types of damage elements
        /// </summary>
        public enum Elements
        {
            Normal, Sharp, Fire, Electric, Ice, Poison, Explosion
        }
    }

    /// <summary>
    /// Class for general global values and references
    /// </summary>
    public static class GeneralGlobals
    {
        public static readonly Random Randomizer = new Random();
    }

    /// <summary>
    /// Class for global values dealing with Animations
    /// </summary>
    public static class AnimationGlobals
    {
        public const string IdleName = "Idle";
        public const string JumpName = "Jump";
        public const string HammerName = "Hammer";
        public const string RunningName = "Run";

        public const string HurtName = "Hurt";
        public const string DeathName = "Death";
        public const string VictoryName = "Victory";
    }

    /// <summary>
    /// Class for global values dealing with Battles
    /// </summary>
    public static class BattleGlobals
    {
        public const int MinDamage = 0;
        public const int MaxDamage = 99;

        public const int MaxPowerBounces = 100;
    }

    /// <summary>
    /// Class for global values dealing with rendering
    /// </summary>
    public static class RenderingGlobals
    {
        public const int WindowWidth = 800;
        public const int WindowHeight = 600;
    }

    /// <summary>
    /// Class for global values dealing with loading and unloading content
    /// </summary>
    public static class ContentGlobals
    {
        public const string ContentRoot = "Content";
        public const string AudioRoot = "Audio";
        public const string SoundRoot = "Audio/SFX/";
        public const string MusicRoot = "Audio/Music/";
        public const string SpriteRoot = "Sprites";
        public const string UIRoot = "UI";
    }

    /// <summary>
    /// Class for global utility functions
    /// </summary>
    public static class UtilityGlobals
    {
        public static int Clamp(int value, int min, int max) => (value < min) ? min : (value > max) ? max : value;
        public static float Clamp(float value, float min, float max) => (value < min) ? min : (value > max) ? max : value;
        public static double Clamp(double value, double min, double max) => (value < min) ? min : (value > max) ? max : value;

        public static int Wrap(int value, int min, int max) => (value < min) ? max : (value > max) ? min : value;
        public static float Wrap(float value, float min, float max) => (value < min) ? max : (value > max) ? min : value;
        public static double Wrap(double value, double min, double max) => (value < min) ? max : (value > max) ? min : value;

        /// <summary>
        /// Chooses a random index in a list of percentages
        /// </summary>
        /// <param name="percentages">The container of percentages, each with positive values, with the sum adding up to 1</param>
        /// <returns>The index in the container of percentages that was chosen</returns>
        public static int ChoosePercentage(IList<double> percentages)
        {
            double randomVal = GeneralGlobals.Randomizer.NextDouble();
            double value = 0d;

            for (int i = 0; i < percentages.Count; i++)
            {
                value += percentages[i];
                if (value > randomVal)
                {
                    return i;
                }
            }

            //Return the last one if it goes through
            return percentages.Count - 1;
        }
    }
}
