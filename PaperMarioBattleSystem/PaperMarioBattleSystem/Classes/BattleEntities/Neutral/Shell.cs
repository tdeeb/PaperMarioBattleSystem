﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaperMarioBattleSystem.Utilities;
using PaperMarioBattleSystem.Extensions;
using static PaperMarioBattleSystem.Enumerations;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// The Shell in Koops' Shell Shield move.
    /// </summary>
    public sealed class Shell : BattleEntity
    {
        /// <summary>
        /// The BattleEntity the Shell is defending.
        /// </summary>
        public BattleEntity EntityDefending { get; private set; } = null;

        private bool SentDeathBattleEvent = false;

        public Shell(int hp, int maxHP) : base(new Stats(0, maxHP, 0, 0, 0))
        {
            Name = "Shell";

            EntityType = EntityTypes.Neutral;

            //Set the HP to the amount of HP the Shell should have
            int hpDiff = BattleStats.MaxHP - hp;
            if (hpDiff > 0)
            {
                LoseHP(hpDiff);
            }
            
            //The Shell doesn't take turns
            BaseTurns = -99;

            //The Shell isn't targetable by normal means. It makes all attacks target it instead of the normal BattleEntity when active
            this.AddIntAdditionalProperty(AdditionalProperty.Untargetable, 1);

            AddStatusImmunities();

            LoadAnimations();

            Scale = new Vector2(.5f, .5f);
            Layer = .15f;

            //Play the idle anim
            AnimManager.PlayAnimation(GetIdleAnim());
        }

        public override void CleanUp()
        {
            if (BManager != null)
            {
                BManager.EntityRemovedEvent -= EntityRemoved;
            }

            RemoveEntityDefending();

            base.CleanUp();
        }

        public override void LoadAnimations()
        {
            Texture2D spriteSheet = AssetManager.Instance.LoadRawTexture2D($"{ContentGlobals.SpriteRoot}/Neutral/ShellShieldShell.png");
            AnimManager.SetSpriteSheet(spriteSheet);

            AnimManager.AddAnimation(AnimationGlobals.ShellBattleAnimations.FullHealthStateName, new Animation(spriteSheet,
                new Animation.Frame(new Rectangle(7, 3, 186, 130), 1000d)));

            Animation mildlyDamaged = new Animation(spriteSheet, new Animation.Frame(new Rectangle(7, 153, 186, 130), 1000d));
            AnimManager.AddAnimation(AnimationGlobals.ShellBattleAnimations.MildlyDamagedStateName, mildlyDamaged);
            mildlyDamaged.SetChildFrames(new Animation.Frame(new Rectangle(217, 4, 13, 47), 1000d, new Vector2(0, -19)));

            Animation severelyDamaged = new Animation(spriteSheet, new Animation.Frame(new Rectangle(7, 153, 186, 130), 1000d));
            AnimManager.AddAnimation(AnimationGlobals.ShellBattleAnimations.SeverelyDamagedStateName, severelyDamaged);
            severelyDamaged.SetChildFrames(new Animation.Frame(new Rectangle(242, 4, 42, 98), 1000d, new Vector2(0, -5)));
        }

        public override string GetIdleAnim()
        {
            //The animation of the Shell depends on how much it's damaged
            float ratio = CurHP / (float)BattleStats.MaxHP;

            if (ratio >= .875f) return AnimationGlobals.ShellBattleAnimations.FullHealthStateName;
            else if (ratio >= .5f) return AnimationGlobals.ShellBattleAnimations.MildlyDamagedStateName;
            else return AnimationGlobals.ShellBattleAnimations.SeverelyDamagedStateName;
        }

        /// <summary>
        /// Clears the BattleEntity the Shell is defending.
        /// </summary>
        public void RemoveEntityDefending()
        {
            if (EntityDefending != null)
            {
                Debug.Log($"The Shell stopped defending {EntityDefending.Name}!");

                EntityDefending.EntityProperties.RemoveAdditionalProperty(AdditionalProperty.DefendedByEntity);
                EntityDefending = null;
            }
        }

        /// <summary>
        /// Sets the BattleEntity for the Shell to defend.
        /// </summary>
        /// <param name="battleEntity">The BattleEntity for the Shell to defend.</param>
        public void SetEntityToDefend(BattleEntity battleEntity)
        {
            RemoveEntityDefending();

            if (battleEntity == null) return;

            EntityDefending = battleEntity;
            EntityDefending.EntityProperties.AddAdditionalProperty(AdditionalProperty.DefendedByEntity, this);

            Debug.Log($"The Shell started defending {EntityDefending.Name}!");
        }

        public override void OnEnteredBattle()
        {
            base.OnEnteredBattle();

            //Subscribe to the removed event so we can remove the protection and clear the entity reference if it's removed
            BManager.EntityRemovedEvent -= EntityRemoved;
            BManager.EntityRemovedEvent += EntityRemoved;

            //Show the Shell's HP, which can only be viewed with the Peekaboo Badge since it can't be tattled (in the actual games, at least)
            if (BManager.Mario != null && BManager.Mario.GetPartyEquippedBadgeCount(BadgeGlobals.BadgeTypes.Peekaboo) > 0)
            {
                this.AddShowHPProperty();
            }
        }

        public override void OnTurnStart()
        {
            //In the event the Shell does start its turn, simply do nothing
            base.OnTurnStart();

            StartAction(new NoAction(this), false, null);
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            //Add a Battle Event to end the protection and play the animation of the Shell breaking at the end of the turn
            if (SentDeathBattleEvent == false)
            {
                BManager.battleEventManager.QueueBattleEvent((int)BattleGlobals.BattleEventPriorities.Status - 1,
                    new BattleGlobals.BattleState[] { BattleGlobals.BattleState.TurnEnd },
                    new ShellBreakBattleEvent(this));

                SentDeathBattleEvent = true;
            }
        }

        public override Item GetItemOfType(Item.ItemTypes itemTypes)
        {
            return null;
        }

        private void AddStatusImmunities()
        {
            //The Shell is immune to all Status Effects
            StatusTypes[] statuses = EnumUtility.GetValues<StatusTypes>.EnumValues;
            for (int i = 0; i < statuses.Length; i++)
            {
                this.AddRemoveStatusImmunity(statuses[i], true);
            }
        }
        
        private void EntityRemoved(BattleEntity entityRemoved)
        {
            //Remove the BattleEntity the Shell is defending if that BattleEntity or the Shell is removed from battle
            //Kill off the Shell since it won't be defending anyone
            if (entityRemoved == this || (EntityDefending != null && entityRemoved == EntityDefending))
            {
                RemoveEntityDefending();

                //Unsubscribe from this event
                if (BManager != null)
                    BManager.EntityRemovedEvent -= EntityRemoved;

                //Kill the Shell if it's not already dead
                if (IsDead == false)
                    Die();
            }
        }
    }
}
