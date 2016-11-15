﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static PaperMarioBattleSystem.Enumerations;
using static PaperMarioBattleSystem.AnimationGlobals;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// Any fighter that takes part in battle
    /// </summary>
    public abstract class BattleEntity
    {
        /// <summary>
        /// Various unique properties belonging to the BattleEntity
        /// </summary>
        public readonly BattleEntityProperties EntityProperties = null;

        /// <summary>
        /// The animations, referred to by string, of the BattleEntity
        /// </summary>
        protected readonly Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();

        /// <summary>
        /// The HeightState of the entity
        /// </summary>
        public HeightStates HeightState { get; protected set; } = HeightStates.Grounded;

        /// <summary>
        /// The HealthState of the entity.
        /// This can apply to any entity, but only Mario and his Partners utilize Danger and Peril
        /// </summary>
        public HealthStates HealthState { get; private set; } = HealthStates.Normal;

        /// <summary>
        /// The previous animation that has been played
        /// </summary>
        protected Animation PreviousAnim = null;

        /// <summary>
        /// The current animation being played
        /// </summary>
        protected Animation CurrentAnim = null;

        public Stats BattleStats { get; protected set; } = Stats.Default;
        public int CurHP => BattleStats.HP;
        public int CurFP => BattleStats.FP;

        /// <summary>
        /// The base number of turns the BattleEntity has in each of its phases
        /// </summary>
        public int BaseTurns { get; protected set; } = BattleGlobals.DefaultTurnCount;

        /// <summary>
        /// The number of turns the BattleEntity used in this phase
        /// </summary>
        public int TurnsUsed { get; protected set; } = 0;

        /// <summary>
        /// The current max number of turns the BattleEntity has in this phase
        /// </summary>
        public int MaxTurns { get; protected set; } = BattleGlobals.DefaultTurnCount;

        public string Name { get; protected set; } = "Entity";
        
        /// <summary>
        /// The entity's current position
        /// </summary>
        public Vector2 Position { get; set; } = Vector2.Zero;

        /// <summary>
        /// The entity's battle position. The entity goes back to this after each action
        /// </summary>
        public Vector2 BattlePosition { get; protected set; } = Vector2.Zero;

        public float Rotation { get; set; } = 0f;
        public float Scale { get; set; } = 1f;

        public EntityTypes EntityType { get; protected set; } = EntityTypes.Enemy;

        /// <summary>
        /// The previous BattleAction the entity used
        /// </summary>
        public MoveAction PreviousAction { get; protected set; } = null;

        /// <summary>
        /// Tells whether the BattleEntity is in Danger or Peril
        /// </summary>
        public bool IsInDanger => (HealthState == HealthStates.Danger || HealthState == HealthStates.Peril);
        public bool IsDead => HealthState == HealthStates.Dead;
        public bool IsTurn => BattleManager.Instance.EntityTurn == this;

        public bool UsedTurn => (TurnsUsed >= MaxTurns || IsDead == true);

        protected readonly List<DefensiveAction> DefensiveActions = new List<DefensiveAction>();

        protected BattleEntity()
        {
            EntityProperties = new BattleEntityProperties(this);
        }

        protected BattleEntity(Stats stats) : this()
        {
            BattleStats = stats;
        }

        #region Stat Manipulations

        /// <summary>
        /// Makes the entity take damage from an attack, factoring in stats such as defense, weaknesses, and resistances
        /// </summary>
        /// <param name="damageResult">The InteractionHolder containing the result of a damage interaction</param>
        /*This is how Paper Mario: The Thousand Year Door calculates damage:
        1. Start with base attack
        2. Subtract damage from Defend Plus, Defend Command, and any additional Defense
        3. Subtract or Add from P-Down D-up and P-Up D-Down
        4. Reduce damage to 0 if superguarded. Reduce by 1 + each Damage Dodge if guarded
        5. Multiply by the number of Double Pains + 1
        6. Divide by the number of Last Stands + 1 (if in danger)
        
        Therefore, two Double Pains = Triple Pain.
        Max Damage is 99.*/

        public void TakeDamage(InteractionHolder damageResult)
        {
            Elements element = damageResult.DamageElement;
            int damage = damageResult.TotalDamage;
            bool piercing = damageResult.Piercing;
            StatusEffect[] statusesInflicted = damageResult.StatusesInflicted;

            //Check for a damage received multiplier on the entity. We need to check if it has one since the default value is 0
            //NOTE: Don't do this here, do it in Interactions according to the order in the comments above (Steps 5/6)
            //if (EntityProperties.HasMiscProperty(MiscProperty.DamageReceivedMultiplier) == true)
            //{
            //    damage *= EntityProperties.GetMiscProperty(MiscProperty.DamageReceivedMultiplier).IntValue;
            //}

            //Handle the elemental interaction results
            ElementInteractionResult elementResult = damageResult.ElementResult;

            //NOTE: Move this when it's relevant (Ex. implementing Bowser's Star Rod power)
            //Check for Invincibility
            //bool invincible = EntityProperties.GetMiscProperty(MiscProperty.Invincible).BoolValue;
            ////If the entity is invincible, don't deal any damage and negate all weaknesses and resistances
            ////The entity wouldn't heal if it should because invincibility means it can't get hit
            //if (invincible == true)
            //{
            //    damage = 0;
            //    elementResult = ElementInteractionResult.Damage;
            //    statusesInflicted = null;
            //}

            if (elementResult == ElementInteractionResult.Damage || elementResult == ElementInteractionResult.KO)
            {
                if (elementResult == ElementInteractionResult.Damage)
                {
                    Debug.Log($"{Name} was hit with {damage} {element} " + (piercing ? "piercing" : "non-piercing") + " damage!");

                    //Lose HP
                    LoseHP(damage);
                }
                //Kill the entity now on an instant KO
                else if (elementResult == ElementInteractionResult.KO)
                {
                    Debug.Log($"{Name} was instantly KO'd from {element} because it has a {nameof(WeaknessTypes.KO)} weakness");

                    Die();
                }
            }
            //Heal the entity
            else if (elementResult == ElementInteractionResult.Heal)
            {
                Debug.Log($"{Name} was healed for {damage} HP because it has a {nameof(ResistanceTypes.Heal)} resistance to Element {element}");

                //Heal the damage
                HealHP(damage);
            }

            //Inflict Statuses
            if (statusesInflicted != null)
            {
                for (int i = 0; i < statusesInflicted.Length; i++)
                {
                    EntityProperties.AfflictStatus(statusesInflicted[i]);
                }
            }

            //If this entity received damage during its action sequence, it has been interrupted
            //The null check is necessary in the event that a StatusEffect that deals damage at the start of the phase, such as Poison,
            //is inflicted at the start of the battle before any entity has moved
            if (IsTurn == true && PreviousAction?.InSequence == true)
            {
                PreviousAction.StartInterruption(element);
            }
        }

        /// <summary>
        /// Makes the entity take damage from an attack, factoring in stats such as defense, weaknesses, and resistances.
        /// <para>This method is a shorthand for inflicting simple damage such as Poison damage every turn from a Status Effect</para>
        /// </summary>
        /// <param name="element">The element to damage the entity with</param>
        /// <param name="damage">The damage to deal to the entity</param>
        /// <param name="piercing">Whether the attack penetrates Defense or not</param>
        public void TakeDamage(Elements element, int damage, bool piercing)
        {
            TakeDamage(new InteractionHolder(null, damage, element, ElementInteractionResult.Damage, ContactTypes.None, piercing, null, true));
        }

        public virtual void HealHP(int hp)
        {
            BattleStats.HP = UtilityGlobals.Clamp(BattleStats.HP + hp, 0, BattleStats.MaxHP);

            UpdateHealthState();
            Debug.Log($"{Name} healed {hp} HP!");
        }

        public void HealFP(int fp)
        {
            BattleStats.FP = UtilityGlobals.Clamp(BattleStats.FP + fp, 0, BattleStats.MaxFP);
        }

        public virtual void LoseHP(int hp)
        {
            BattleStats.HP = UtilityGlobals.Clamp(BattleStats.HP - hp, 0, BattleStats.MaxHP);
            UpdateHealthState();
            if (IsDead == true)
            {
                Die();
            }

            Debug.Log($"{Name} took {hp} points of damage!");
        }

        public void LoseFP(int fp)
        {
            BattleStats.FP = UtilityGlobals.Clamp(BattleStats.FP - fp, 0, BattleStats.MaxFP);
        }

        public void RaiseAttack(int attack)
        {
            BattleStats.Attack += attack;
        }
        
        public void RaiseDefense(int defense)
        {
            BattleStats.Defense += defense;
        }

        public void LowerAttack(int attack)
        {
            BattleStats.Attack -= attack;
        }

        public void LowerDefense(int defense)
        {
            BattleStats.Defense -= defense;
        }

        public void ModifyAccuracy(int accuracy)
        {
            BattleStats.Accuracy = UtilityGlobals.Clamp(BattleStats.Accuracy + accuracy, int.MinValue, int.MaxValue);
        }

        public void ModifyEvasion(int evasion)
        {
            BattleStats.Evasion = UtilityGlobals.Clamp(BattleStats.Evasion + evasion, int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Kills the entity instantly
        /// </summary>
        public void Die()
        {
            BattleStats.HP = 0;
            UpdateHealthState();
            PlayAnimation(AnimationGlobals.DeathName, true);

            //Remove all StatusEffects on the entity
            EntityProperties.RemoveAllStatuses();

            OnDeath();
        }

        /// <summary>
        /// Performs entity-specific logic on death
        /// </summary>
        public virtual void OnDeath()
        {
            Debug.Log($"{Name} has been defeated!");
        }

        /// <summary>
        /// Updates the entity's health state based on its current HP
        /// </summary>
        protected void UpdateHealthState()
        {
            HealthStates newHealthState = HealthState;
            if (CurHP > BattleGlobals.MaxDangerHP)
            {
                newHealthState = HealthStates.Normal;
            }
            else if (CurHP >= BattleGlobals.MinDangerHP)
            {
                newHealthState = HealthStates.Danger;
            }
            else if (CurHP == BattleGlobals.PerilHP)
            {
                newHealthState = HealthStates.Peril;
            }
            else
            {
                newHealthState = HealthStates.Dead;
            }

            //Change health states if they're no longer the same
            if (newHealthState != HealthState)
            {
                OnHealthStateChange(newHealthState);

                //Change to the new health state
                HealthState = newHealthState;
            }
        }

        /// <summary>
        /// What occurs when an entity changes HealthStates
        /// </summary>
        /// <param name="newHealthState">The new HealthState of the entity</param>
        protected virtual void OnHealthStateChange(HealthStates newHealthState)
        {

        }

        #endregion

        /// <summary>
        /// What occurs when the battle is started for the entity.
        /// </summary>
        public virtual void OnBattleStart()
        {
            
        }

        #region Damage Related

        /// <summary>
        /// Checks if the entity's attempt to hit another entity is successful based on the entity's Accuracy and the victim's Evasion
        /// </summary>
        /// <param name="victim">The entity trying to evade</param>
        /// <returns>true if the entity hits and the victim doesn't evade, false otherwise</returns>
        public bool AttemptHitEntity(BattleEntity victim)
        {
            return (AttemptHit() == true && victim.AttemptEvade() == false);
        }

        /// <summary>
        /// Performs a check to see if the entity hit based on its Accuracy stat
        /// </summary>
        /// <returns>true if the entity successfully hit, false if the entity fails</returns>
        private bool AttemptHit()
        {
            int valueTest = GeneralGlobals.Randomizer.Next(0, 100);
            return (valueTest < BattleStats.Accuracy);
        }

        /// <summary>
        /// Makes the entity attempt to evade an attack, returning a value indicating the result
        /// </summary>
        /// <returns>true if the entity successful evaded the attack, false if the attack hits</returns>
        //NOTE: When dealing with Badges such as Close Call, we should compare the entity's Evasion first, then perform
        //the test again with the Badges' Evasion added in. If the Badges' Evasion bonus allows the entity to evade the attack,
        //that's when we'd play the "LUCKY" animation
        private bool AttemptEvade()
        {
            int valueTest = GeneralGlobals.Randomizer.Next(0, 100);
            return (valueTest < BattleStats.Evasion);
        }

        /// <summary>
        /// Gets the result of the first successful Defensive Action performed.
        /// </summary>
        /// <param name="damage">The original damage of the attack.</param>
        /// <param name="statusesInflicted">The original set of StatusEffects inflicted.</param>
        /// <returns>A nullable DefensiveActionHolder? with a DefensiveAction's result if successful, otherwise null.</returns>
        public BattleGlobals.DefensiveActionHolder? GetDefensiveActionResult(int damage, StatusEffect[] statusesInflicted)
        {
            //Handle Defensive Actions
            for (int i = 0; i < DefensiveActions.Count; i++)
            {
                if (DefensiveActions[i].IsSuccessful == true)
                {
                    BattleGlobals.DefensiveActionHolder holder = DefensiveActions[i].HandleSuccess(damage, statusesInflicted);
                    return holder;
                }
            }

            return null;
        }

        #endregion

        #region Turn Methods

        /// <summary>
        /// What happens at the start of the phase cycle
        /// </summary>
        public virtual void OnPhaseCycleStart()
        {
            TurnsUsed = 0;
            MaxTurns = BaseTurns;

            StatusEffect[] statuses = EntityProperties.GetStatuses();
            for (int i = 0; i < statuses.Length; i++)
            {
                statuses[i].PhaseCycleStart();
            }
        }

        /// <summary>
        /// What happens at the start of the entity's phase
        /// </summary>
        public virtual void OnPhaseStart()
        {
            Debug.Log($"Started phase for {Name}!");
        }

        /// <summary>
        /// What happens at the end of the entity's phase
        /// </summary>
        public virtual void OnPhaseEnd()
        {
            Debug.Log($"Ended phase for {Name}");
        }

        /// <summary>
        /// What happens when the entity's turn starts
        /// </summary>
        public virtual void OnTurnStart()
        {
            Debug.LogWarning($"Started {Name}'s turn!");

            //End all DefensiveAction inputs
            for (int i = 0; i < DefensiveActions.Count; i++)
            {
                DefensiveActions[i].actionCommand.EndInput();
            }
        }

        /// <summary>
        /// What happens when the entity's turn ends
        /// </summary>
        public virtual void OnTurnEnd()
        {
            //Start all DefensiveAction inputs
            for (int i = 0; i < DefensiveActions.Count; i++)
            {
                DefensiveActions[i].actionCommand.StartInput();
            }
        }

        /// <summary>
        /// Notifies the BattleManager to officially end the BattleEntity's turn
        /// </summary>
        public void EndTurn()
        {
            if (this != BattleManager.Instance.EntityTurn)
            {
                Debug.LogError($"Attempting to end the turn of {Name} when it's not their turn!");
                return;
            }

            TurnsUsed++;
            BattleManager.Instance.TurnEnd();
        }

        /// <summary>
        /// What happens during the entity's turn (choosing action commmands, etc.)
        /// </summary>
        public virtual void TurnUpdate()
        {
            PreviousAction?.Update();
            PreviousAction?.PostUpdate();
        }

        /// <summary>
        /// Sets the max number of turns the entity has during this phase
        /// </summary>
        /// <param name="maxTurns">The new number of turns the entity has for this phase</param>
        public void SetMaxTurns(int maxTurns)
        {
            MaxTurns = maxTurns;
        }

        /// <summary>
        /// Sets the number of turns used by the entity during this phase.
        /// This is primarily used by StatusEffects
        /// </summary>
        /// <param name="turnsUsed">The new number of turns the entity used in this phase</param>
        public void SetTurnsUsed(int turnsUsed)
        {
            TurnsUsed = turnsUsed;
        }

        #endregion

        public void SetBattlePosition(Vector2 battlePos)
        {
            BattlePosition = battlePos;
        }

        /// <summary>
        /// Determines if the BattleEntity gets affected by the Confused status, if it has it.
        /// If the BattleEntity is affected, it may perform a different action or target different entities with its original one.
        /// </summary>
        /// <param name="action">The BattleAction originally used.</param>
        /// <param name="targets">The original set of BattleEntities to target</param>
        /// <returns>An ActionHolder with a new BattleAction to perform or a different target list if the entity was affected by Confused.
        /// If not affected, the originals of each will be returned.</returns>
        private BattleGlobals.ActionHolder HandleConfusion(MoveAction action, params BattleEntity[] targets)
        {
            MoveAction actualAction = action;
            BattleEntity[] actualTargets = targets;

            //Check for Confusion's effects and change actions or targets depending on what happens
            int percent = EntityProperties.GetMiscProperty(MiscProperty.ConfusionPercent).IntValue;

            //See if Confusion should take effect
            if (UtilityGlobals.TestRandomCondition(0, 100, percent, false) == true)
            {
                Debug.Log($"{Name} is affected by Confusion and will do something unpredictable!");

                int changeTargets = GeneralGlobals.Randomizer.Next(0, 2);
                //int changeAction;

                //NOTE: Find a way to make it so we can control what happens based on the type of action.
                //For example, if Tattle was used, it doesn't target anyone else.
                //If you use a Healing item, it uses it on the opposing side, whereas if you use an attack it uses it on an ally.

                //Change to an ally
                /*Steps:
                  1. If the action hits the first entity, find the adjacent allies. If not, find all allies
                  2. Filter by heights based on what the action can hit
                  3. Filter out dead allies
                  4. If the action hits everyone, go with the remaining list. Otherwise, choose a random ally to attack
                  5. If there are no allies to attack after all the filtering, make the entity do nothing*/
                if (changeTargets == 0)
                {
                    BattleEntity[] allies = null;

                    //If this action targets only the first player/enemy, look for adjacent allies
                    if (actualAction.SelectionType == TargetSelectionMenu.EntitySelectionType.First)
                    {
                        Debug.Log($"{Name} is looking for valid adjacent allies to attack!");

                        //Find adjacent allies and filter out all non-ally entities
                        List<BattleEntity> adjacentEntities = new List<BattleEntity>(BattleManager.Instance.GetAdjacentEntities(this));
                        adjacentEntities.RemoveAll((adjacent) => adjacent.EntityType != EntityType);
                        allies = adjacentEntities.ToArray();
                    }
                    else
                    {
                        //Find all allies
                        allies = BattleManager.Instance.GetEntityAllies(this);
                    }

                    //Filter by heights
                    allies = BattleManager.Instance.FilterEntitiesByHeights(allies, actualAction.HeightsAffected);

                    //Filter dead entities
                    allies = BattleManager.Instance.FilterDeadEntities(allies);

                    //Choose a random ally to attack if the action only targets one entity
                    if (allies.Length > 0 && actualAction.SelectionType != TargetSelectionMenu.EntitySelectionType.All)
                    {
                        int randTarget = GeneralGlobals.Randomizer.Next(0, allies.Length);
                        allies = new BattleEntity[] { allies[randTarget] };

                        Debug.Log($"{Name} is choosing to attack ally {allies[0].Name} in Confusion!");
                    }

                    //Set the actual targets to be the set of allies
                    actualTargets = allies;

                    //If you can't attack any allies, do nothing
                    if (actualTargets.Length == 0)
                    {
                        actualAction = new NoAction();

                        Debug.Log($"{Name} did nothing as there either are no allies to attack or they're not in range!");
                    }
                    else
                    {
                        //Disable action commands when attacking allies from Confusion, if the action has an Action Command
                        IActionCommand commandAction = actualAction as IActionCommand;
                        if (commandAction != null)
                            commandAction.DisableActionCommand = true;
                    }
                }
            }

            return new BattleGlobals.ActionHolder(actualAction, actualTargets);
        }

        public void StartAction(MoveAction action, params BattleEntity[] targets)
        {
            MoveAction actualAction = action;
            BattleEntity[] actualTargets = targets;

            //Check for Confused and handle it appropriately
            if (EntityProperties.HasMiscProperty(MiscProperty.ConfusionPercent) == true)
            {
                BattleGlobals.ActionHolder holder = HandleConfusion(action, targets);
                actualAction = holder.Action;
                actualTargets = holder.Targets;
            }

            PreviousAction = actualAction;
            PreviousAction.StartSequence(actualTargets);
        }

        #region Animation Methods

        /// <summary>
        /// Adds an animation for the entity.
        /// If an animation already exists, it will be replaced.
        /// </summary>
        /// <param name="animName">The name of the animation</param>
        /// <param name="anim">The animation reference</param>
        protected void AddAnimation(string animName, Animation anim)
        {
            //Return if trying to add null animation
            if (anim == null)
            {
                Debug.LogError($"Trying to add null animation called \"{animName}\" to entity {Name}, so it won't be added");
                return;
            }

            if (Animations.ContainsKey(animName) == true)
            {
                Debug.LogWarning($"Entity {Name} already has an animation called \"{animName}\" and will be replaced");

                //Clear the current animation reference if it is the animation being removed
                Animation prevAnim = Animations[animName];
                if (CurrentAnim == prevAnim)
                { 
                    CurrentAnim = null;
                }

                Animations.Remove(animName);
            }

            anim.SetKey(animName);
            Animations.Add(animName, anim);

            //Play the most recent animation that gets added is the default, and play it
            //This allows us to safely have a valid animation reference at all times, provided at least one is added
            if (CurrentAnim == null)
            {
                PlayAnimation(animName);
            }
        }

        public Animation GetAnimation(string animName)
        {
            //If animation cannot be found
            if (Animations.ContainsKey(animName) == false)
            {
                Debug.LogError($"Cannot find animation called \"{animName}\" for entity {Name} to play");
                return null;
            }

            return Animations[animName];
        }

        /// <summary>
        /// Plays an animation that the entity has, specified by its name. If the animation does not have the specified animation,
        /// nothing happens
        /// </summary>
        /// <param name="animName">The name of the animation to play</param>
        /// <param name="resetPrevious">If true, resets the previous animation that was playing, if any.
        /// This will also reset its speed</param>
        public void PlayAnimation(string animName, bool resetPrevious = false, Animation.AnimFinish onFinish = null)
        {
            Animation animToPlay = GetAnimation(animName);

            //If animation cannot be found, return
            if (animToPlay == null)
            {
                return;
            }

            //Reset the previous animation if specified
            if (resetPrevious == true)
            {
                CurrentAnim?.Reset(true);
            }

            //Set previous animation
            PreviousAnim = CurrentAnim;

            //Play animation
            CurrentAnim = animToPlay;
            CurrentAnim.Play(onFinish);
        }

        #endregion

        #region Equipment Methods

        /// <summary>
        /// Gets the number of Badges of a particular BadgeType that the BattleEntity has equipped.
        /// </summary>
        /// <param name="badgeType">The BadgeType to check for.</param>
        /// <returns>The number of Badges of the BadgeType that the BattleEntity has equipped.</returns>
        public abstract int GetEquippedBadgeCount(BadgeGlobals.BadgeTypes badgeType);

        #endregion

        /// <summary>
        /// Used for update logic that applies to the entity regardless of whether it is its turn or not
        /// </summary>
        public void Update()
        {
            CurrentAnim?.Update();

            //Update Defensive actions
            for (int i = 0; i < DefensiveActions.Count; i++)
            {
                DefensiveActions[i].Update();
            }
        }

        public virtual void Draw()
        {
            CurrentAnim?.Draw(Position, Color.White, EntityType != EntityTypes.Enemy, .1f);
            PreviousAction?.Draw();
        }
    }
}
