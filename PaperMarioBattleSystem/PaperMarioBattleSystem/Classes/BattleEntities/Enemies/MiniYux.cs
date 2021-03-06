﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaperMarioBattleSystem.Extensions;
using static PaperMarioBattleSystem.Enumerations;

namespace PaperMarioBattleSystem
{
    /// <summary>
    /// A Mini-Yux. It is created by a Yux, which the Mini-Yux protects with a shield that makes it invincible.
    /// </summary>
    public class MiniYux : BattleEnemy, ITattleableEntity
    {
        public MiniYux() : base(new Stats(0, 1, 0, 0, 0))
        {
            Name = "Mini-Yux";

            //All types of Mini-Yuxes don't take turns
            BaseTurns = -99;

            Scale = new Vector2(.5f, .5f);

            ChangeHeightState(HeightStates.Airborne);

            //The Helper AdditionalProperty is added when the Yux creates the Mini-Yux
            //This helps allow Mini-Yuxes to be standalone enemies if desired

            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Sleep, new StatusPropertyHolder(80, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Dizzy, new StatusPropertyHolder(100, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Confused, new StatusPropertyHolder(90, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Tiny, new StatusPropertyHolder(90, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Stop, new StatusPropertyHolder(100, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.DEFDown, new StatusPropertyHolder(95, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Burn, new StatusPropertyHolder(0, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Frozen, new StatusPropertyHolder(0, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Fright, new StatusPropertyHolder(0, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.Blown, new StatusPropertyHolder(100, 0));
            EntityProperties.AddStatusProperty(Enumerations.StatusTypes.KO, new StatusPropertyHolder(100, 0));

            LoadAnimations();
        }

        public override void LoadAnimations()
        {
            Texture2D spriteSheet = AssetManager.Instance.LoadRawTexture2D($"{ContentGlobals.SpriteRoot}/Enemies/Yux.png");
            AnimManager.SetSpriteSheet(spriteSheet);

            AnimManager.AddAnimation(AnimationGlobals.IdleName, new Animation(spriteSheet,
                new Animation.Frame(new Rectangle(300, 14, 52, 48), 1000d)));
        }

        public override void OnEnteredBattle()
        {
            //The Mini-Yux's BattlePosition is set by the Yux, so do everything aside from that

            //Equip the held Badge, if one is held
            if (HeldCollectible?.CollectibleType == Enumerations.CollectibleTypes.Badge)
            {
                Badge heldBadge = (Badge)HeldCollectible;
                heldBadge.Equip(this);
            }

            //Check if the enemy has an entry in the Tattle table
            //If so, mark it to show its HP
            if (TattleDatabase.HasTattleDescription(Name) == true)
            {
                this.AddShowHPProperty();
            }
        }

        #region Tattle Information

        public bool CanBeTattled { get; set; } = true;

        public string[] GetTattleLogEntry()
        {
            return new string[]
            {
                "Each one can divide into two. As long as you clear these out first, ordinary Yux are nothing to be feared."
            };
        }

        public string GetTattleDescription()
        {
            return "That's a Mini-Yux.\n<wait value=\"250\">A creature made to protect\na Yux, it can split into two.\n<k><p>" +
                   "Max HP is 1, Attack is 0, and Defense is 0.\n<k><p>" +
                   "These twerps are the reason\nyou sometimes can't do any\ndamage to the main Yux.\n<k><p>" +
                   "They're a pain, but you HAVE\nto beat them before the Yux.\n<wait value=\"250\">Flurrie's pretty effective...<k>";
        }

        #endregion
    }
}
