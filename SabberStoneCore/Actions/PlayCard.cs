﻿using System;
using System.Collections.Generic;
using SabberStoneCore.Enchants;
using SabberStoneCore.Model;
using SabberStoneCore.Enums;

namespace SabberStoneCore.Actions
{
    public partial class Generic
    {
        public static bool PlayCard(Controller c, IPlayable source, ICharacter target = null, int zonePosition = -1, int chooseOne = 0)
        {
            return PlayCardBlock.Invoke(c, source, target, zonePosition, chooseOne);
        }

        public static Func<Controller, IPlayable, ICharacter, int, int, bool> PlayCardBlock
            => delegate (Controller c, IPlayable source, ICharacter target, int zonePosition, int chooseOne)
            {
                if (!PrePlayPhase.Invoke(c, source, target, zonePosition, chooseOne))
                {
                    return false;
                }

                if (!PayPhase.Invoke(c, source))
                {
                    return false;
                }

                c.NumCardsPlayedThisTurn++;

                c.LastCardPlayed = source.Id;

                // target is beeing set onto this gametag
                if (target != null)
                {
                    source.CardTarget = target.Id;
                }

                if (source is Minion)
                {
                    PlayMinion.Invoke(c, (Minion)source, target, zonePosition);
                }
                else if (source is Weapon)
                {
                    // - OnPlay Phase --> OnPlay Trigger (Illidan)
                    //   (death processing, aura updates)
                    OnPlayTrigger.Invoke(c, (Weapon)source);

                    if (!RemoveFromZone.Invoke(c, source))
                        return false;

                    PlayWeapon.Invoke(c, (Weapon)source);
                }
                else if (source is Spell)
                {

                    // - OnPlay Phase --> OnPlay Trigger (Illidan)
                    //   (death processing, aura updates)
                    OnPlayTrigger.Invoke(c, (Spell)source);

                    // remove from hand zone
                    if (!RemoveFromZone.Invoke(c, source))
                        return false;

                    PlaySpell.Invoke(c, (Spell)source, target);
                }

                source.CardTarget = -1;

                c.NumOptionsPlayedThisTurn++;

                c.IsComboActive = true;

                return true;
            };

        public static Func<Controller, IPlayable, ICharacter, int, int, bool> PrePlayPhase
            => delegate(Controller c, IPlayable source, ICharacter target, int zonePosition, int chooseOne)
            {
                // can't play because we got already board full
                if (source is Minion && c.Board.IsFull)
                {
                    c.Game.Log(LogLevel.WARNING, BlockType.ACTION, "PrePlayPhase", $"Board has already {c.Board.MaxSize} minions.");
                    return false;
                }

                // TODO ChooseOne implementation: rework on it, later!
                // set choose one option
                source.ChooseOneOption = chooseOne;
                if (source.ChooseOne && chooseOne == 0)
                {
                    c.Game.Log(LogLevel.WARNING, BlockType.ACTION, "PrePlayPhase", $"Choose One, no option set for this card.");
                    return false;
                }

                // check if we can play this card and the target is valid
                if (!source.IsPlayable || !source.IsValidPlayTarget(target))
                {
                    return false;
                }

                // copy choose one enchantment to the actual source
                if (source.ChooseOne)
                {
                    source.Enchantments = source.RefCard.Enchantments;
                }

                // replace enchantments with the no combo or combo one ..
                if (source.Combo)
                {
                    if (source.Enchantments.Count > 1)
                    {
                        source.Enchantments = new List<Enchantment> {source.Enchantments[c.IsComboActive ? 1 : 0]};
                    }
                    else if (c.IsComboActive && source.Enchantments.Count > 0)
                    {
                        source.Enchantments = new List<Enchantment> {source.Enchantments[0]};
                    }
                    else
                    {
                        source.Enchantments = new List<Enchantment> {};
                    }
                }

                return true;
            };

        public static Func<Controller, IPlayable, bool> PayPhase
            => delegate(Controller c, IPlayable source)
            {
                c.OverloadOwed += source.Overload;
                var cost = source.Cost;
                if (cost > 0)
                {
                    var tempUsed = Math.Min(c.TemporaryMana, cost);
                    c.TemporaryMana -= tempUsed;
                    c.UsedMana += cost - tempUsed;
                    c.TotalManaSpentThisGame += cost;
                }
                c.Game.Log(LogLevel.INFO, BlockType.ACTION, "PayPhase", $"Paying {source} for {source.Cost} Mana, remaining mana is {c.RemainingMana}.");
                return true;
            };

        public static Func<Controller, Minion, ICharacter, int, bool> PlayMinion
            => delegate(Controller c, Minion minion, ICharacter target, int zonePosition)
            {
                // - PreSummon Phase --> PreSummon Trigger (TideCaller)
                //   (death processing, aura updates)

                // remove from hand zone
                if (!RemoveFromZone.Invoke(c, minion))
                    return false;

                if (!minion.HasCharge)
                    minion.IsExhausted = true;

                c.Game.Log(LogLevel.INFO, BlockType.ACTION, "PlayMinion", $"{c.Name} plays Minion {minion} {(target != null ? "with target " + target : "to board")} " +
                         $"{(zonePosition > -1 ? "position " + zonePosition : "")}.");

                // - PreSummon Phase --> PreSummon Phase Trigger (Tidecaller)
                //   (death processing, aura updates)
                c.Board.Add(minion, zonePosition);
                c.Game.DeathProcessingAndAuraUpdate();

                // - OnPlay Phase --> OnPlay Trigger (Illidan)
                //   (death processing, aura updates)
                OnPlayTrigger.Invoke(c, minion);

                // - BattleCry Phase --> Battle Cry Resolves
                //   (death processing, aura updates)
                minion.ApplyEnchantments(EnchantmentActivation.BATTLECRY, Zone.PLAY, target);
                // check if [LOE_077] Brann Bronzebeard aura is active
                if (minion[GameTag.BATTLECRY] == 2)
                {
                    minion.ApplyEnchantments(EnchantmentActivation.BATTLECRY, Zone.PLAY, target);
                }
                c.Game.DeathProcessingAndAuraUpdate();

                // - After Play Phase --> After play Trigger / Secrets (Mirror Entity)
                //   (death processing, aura updates)
                minion.JustPlayed = false;
                c.Game.DeathProcessingAndAuraUpdate();

                // - After Summon Phase --> After Summon Trigger
                //   (death processing, aura updates)
                AfterSummonTrigger.Invoke(c, minion);

                c.NumMinionsPlayedThisTurn++;

                return true;
            };

        public static Func<Controller, Spell, ICharacter, bool> PlaySpell
            => delegate(Controller c, Spell spell, ICharacter target)
            {
                c.Game.Log(LogLevel.INFO, BlockType.ACTION, "PlaySpell", $"{c.Name} plays Spell {spell} {(target != null ? "with target " + target.Card : "to board")}.");

                // trigger Spellbender Phase
                c.Game.Log(LogLevel.DEBUG, BlockType.ACTION, "PlaySpell", "trigger Spellbender Phase (not implemented)");

                // trigger SpellText Phase
                c.Game.Log(LogLevel.DEBUG, BlockType.ACTION, "PlaySpell", "trigger SpellText Phase (not implemented)");

                if (spell.IsCountered)
                {
                    c.Game.Log(LogLevel.INFO, BlockType.ACTION, "PlaySpell", $"Spell {spell} has been countred.");
                    c.Graveyard.Add(spell);
                }
                else if (spell.IsSecret)
                {
                    spell.ApplyEnchantments(EnchantmentActivation.SECRET, Zone.PLAY);
                    c.Secrets.Add(spell);

                    c.NumSecretsPlayedThisGame++;
                }
                else
                {
                    spell.ApplyEnchantments(EnchantmentActivation.SPELL, Zone.PLAY, target);
                    c.Graveyard.Add(spell);

                    c.NumSpellsPlayedThisGame++;
                }
                c.Game.DeathProcessingAndAuraUpdate();

                // trigger After Play Phase
                c.Game.Log(LogLevel.DEBUG, BlockType.ACTION, "PlaySpell", "trigger After Play Phase");
                spell.JustPlayed = false;
                c.Game.DeathProcessingAndAuraUpdate();

                return true;
            };

        public static Func<Controller, Weapon, bool> PlayWeapon
            => delegate(Controller c, Weapon weapon)
            {
                c.Hero.AddWeapon(weapon);

                c.Game.Log(LogLevel.INFO, BlockType.ACTION, "PlayWeapon", $"{c.Hero} gets Weapon {c.Hero.Weapon}.");

                // activate battlecry
                weapon.ApplyEnchantments(EnchantmentActivation.WEAPON, Zone.PLAY);
                weapon.ApplyEnchantments(EnchantmentActivation.BATTLECRY, Zone.PLAY);
                c.Game.DeathProcessingAndAuraUpdate();

                // trigger After Play Phase
                c.Game.Log(LogLevel.DEBUG, BlockType.ACTION, "PlayWeapon", "trigger After Play Phase");
                weapon.JustPlayed = false;

                return true;
            };

        private static Action<Controller, IPlayable> OnPlayTrigger
            => delegate(Controller c, IPlayable playable)
            {
                playable.JustPlayed = true;
                c.Game.DeathProcessingAndAuraUpdate();
            };
    }
}