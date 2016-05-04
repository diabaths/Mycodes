﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace D_Kayle
{
    internal class Program
    {
        private const string ChampionName = "Kayle";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static SpellSlot _igniteSlot;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static SpellSlot _smiteSlot;

        private static Spell _smite;

        private static Items.Item _rand, _lotis, _frostqueen, _mikael;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.ChampionName != ChampionName)
            {
                Game.PrintChat("Please use Kayle~");
                return;
            }

            _q = new Spell(SpellSlot.Q, 650f);
            _w = new Spell(SpellSlot.W, 900f);
            _e = new Spell(SpellSlot.E, 675f);
            _r = new Spell(SpellSlot.R, 900f);

            if (_player.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner1, 570f);
                _smiteSlot = SpellSlot.Summoner1;
            }
            else if (_player.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner2, 570f);
                _smiteSlot = SpellSlot.Summoner2;
            }

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);
            _frostqueen = new Items.Item(3092, 800f);
            _mikael = new Items.Item(3222, 600f);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");

            //D Kayle
            _config = new Menu("D-Kayle", "D-Kayle", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseIgnitecombo", "Use Ignite")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));


            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("frostQ", "Use Frost Queen"))
                .SetValue(true);
            _config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "Use Iron Solari"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("items").SubMenu("Deffensive").AddSubMenu(new Menu("Cleanse", "Cleanse"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddSubMenu(new Menu("Mikael's Crucible", "mikael"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .SubMenu("mikael")
                .AddItem(new MenuItem("usemikael", "Use Mikael's to remove Debuffs"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .SubMenu("mikael")
                .AddItem(new MenuItem("mikaelusehp", "Or Use if Mikael's Ally Hp <%").SetValue(new Slider(25, 1, 100)));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
                _config.SubMenu("items")
                    .SubMenu("Deffensive")
                    .SubMenu("Cleanse")
                    .SubMenu("mikael")
                    .AddItem(new MenuItem("mikaeluse" + hero.BaseSkinName, hero.BaseSkinName).SetValue(true));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("useqss", "Use QSS/Mercurial Scimitar/Dervish Blade"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("blind", "Blind"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("charm", "Charm"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("fear", "Fear"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("flee", "Flee"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("snare", "Snare"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("taunt", "Taunt"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("suppression", "Suppression"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("stun", "Stun"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("polymorph", "Polymorph"))
                .SetValue(false);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("silence", "Silence"))
                .SetValue(false);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("zedultexecute", "Zed Ult"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("Cleansemode", ""))
                .SetValue(new StringList(new string[2] { "Cleanse Always", "Cleanse in Combo" }));

            //potions
            _config.SubMenu("items").AddSubMenu(new Menu("Potions", "Potions"));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "Use Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "If Mana % <").SetValue(new Slider(35, 1, 100)));

            //utilities
            _config.AddSubMenu(new Menu("Utilities", "utilities"));
            _config.SubMenu("utilities").AddItem(new MenuItem("onmeW", "W Self")).SetValue(true);
            _config.SubMenu("utilities")
                .AddItem(new MenuItem("healper", "Self Health %"))
                .SetValue(new Slider(40, 1, 100));
            _config.SubMenu("utilities").AddItem(new MenuItem("onmeR", "R Self Use")).SetValue(true);
            _config.SubMenu("utilities")
                .AddItem(new MenuItem("ultiSelfHP", "Self Health %"))
                .SetValue(new Slider(40, 1, 100));

            _config.SubMenu("utilities").AddSubMenu(new Menu("Use W Ally", "Use W Ally"));
            _config.SubMenu("utilities").SubMenu("Use W Ally").AddItem(new MenuItem("allyW", "W Ally")).SetValue(true);
            _config.SubMenu("utilities")
                .SubMenu("Use W Ally")
                .AddItem(new MenuItem("allyhealper", "Ally Health %"))
                .SetValue(new Slider(40, 1, 100));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
                _config.SubMenu("utilities")
                    .SubMenu("Use W Ally")
                    .AddItem(new MenuItem("usewally" + hero.BaseSkinName, hero.BaseSkinName).SetValue(true));

            _config.SubMenu("utilities").AddSubMenu(new Menu("Use R Ally", "Use R Ally"));
            _config.SubMenu("utilities")
                .SubMenu("Use R Ally")
                .AddItem(new MenuItem("allyR", "R Ally Use"))
                .SetValue(true);
            _config.SubMenu("utilities")
                .SubMenu("Use R Ally")
                .AddItem(new MenuItem("ultiallyHP", "Ally Health %"))
                .SetValue(new Slider(40, 1, 100));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
                _config.SubMenu("utilities")
                    .SubMenu("Use R Ally")
                    .AddItem(new MenuItem("userally" + hero.BaseSkinName, hero.BaseSkinName).SetValue(true));

            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(
                        new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass key").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Farm
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("Laneclear", "Laneclear"));
            _config.SubMenu("Farm").SubMenu("Laneclear").AddItem(new MenuItem("UseQLane", "Use Q Lane")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Laneclear").AddItem(new MenuItem("UseELane", "Use E Lane")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Laneclear")
                .AddItem(new MenuItem("Farmmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("Laneclear")
                .AddItem(
                    new MenuItem("Activelane", "Lane Clear").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm").AddSubMenu(new Menu("Lasthit", "Lasthit"));
            _config.SubMenu("Farm").SubMenu("Lasthit").AddItem(new MenuItem("UseQLast", "Use Q Last")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lasthit").AddItem(new MenuItem("UseELast", "Use E Last")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Lasthit")
                .AddItem(new MenuItem("lasthitmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("Lasthit")
                .AddItem(
                    new MenuItem("activelast", "Last Hit").SetValue(
                        new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm").AddSubMenu(new Menu("Jungleclear", "Jungleclear"));
            _config.SubMenu("Farm")
                .SubMenu("Jungleclear")
                .AddItem(new MenuItem("UseQjungle", "Use Q Jungle"))
                .SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Jungleclear")
                .AddItem(new MenuItem("UseEjungle", "Use E Jungle"))
                .SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Jungleclear")
                .AddItem(new MenuItem("junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("Jungleclear")
                .AddItem(
                    new MenuItem("Activejungle", "Jungle Clear").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Smite 
            _config.AddSubMenu(new Menu("Smite", "Smite"));
            _config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(
                        new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Useblue", "Smite Blue Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("manaJ", "Smite Blue Early if MP% <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            //Kill Steal
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQKs", "Use Q KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseIgnite", "Use Ignite KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("GapCloserE", "Use Q to GapCloser")).SetValue(true);
            _config.SubMenu("Misc")
                .AddItem(
                    new MenuItem("Escape", "Escapes key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Misc").AddItem(new MenuItem("support", "Support Mode")).SetValue(false);

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawharass", "Draw AutoHarass")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "Draw smite")).SetValue(true);

            _config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Game.PrintChat("<font color='#881df2'>D-Kayle By Diabaths </font>Loaded!");
            Game.PrintChat(
                "<font color='#f2f21d'>Do you like it???  </font> <font color='#ff1900'>Drop 1 Upvote in Database </font>");
            Game.PrintChat(
                "<font color='#f2f21d'>Buy me cigars </font> <font color='#ff1900'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            _player = ObjectManager.Player;
            _orbwalker.SetAttack(true);

            if (_config.Item("Escape").GetValue<KeyBind>().Active)
            {
                Escape();
            }

            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (!_config.Item("ActiveCombo").GetValue<KeyBind>().Active
                && (_config.Item("ActiveHarass").GetValue<KeyBind>().Active
                    || _config.Item("harasstoggle").GetValue<KeyBind>().Active)
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();
            }

            if (_config.Item("activelast").GetValue<KeyBind>().Active
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("lasthitmana").GetValue<Slider>().Value)
            {
                Lasthit();
            }

            if (_config.Item("Activelane").GetValue<KeyBind>().Active
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Farmmana").GetValue<Slider>().Value)
            {
                Farm();
            }

            if (_config.Item("Activejungle").GetValue<KeyBind>().Active
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("junglemana").GetValue<Slider>().Value)
            {
                JungleFarm();
            }

            Usepotion();
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }

            AutoW();
            AutoR();
            AllyR();
            AllyW();
            KillSteal();
            Usecleanse();
        }

        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iOmen = _config.Item("Omen").GetValue<bool>();
                var iOmenenemys = hero.CountEnemiesInRange(450) >= _config.Item("Omenenemys").GetValue<Slider>().Value;
                var ifrost = _config.Item("frostQ").GetValue<bool>();

                if (ifrost && _frostqueen.IsReady() && hero.IsValidTarget(_frostqueen.Range))
                {
                    _frostqueen.Cast();
                }

                if (iOmenenemys && iOmen && _rand.IsReady() && hero.IsValidTarget(_rand.Range))
                {
                    _rand.Cast();
                }
            }

            var ilotis = _config.Item("lotis").GetValue<bool>();
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth * (_config.Item("lotisminhp").GetValue<Slider>().Value) / 100)
                        && hero.Distance(_player.ServerPosition) <= _lotis.Range && _lotis.IsReady()) _lotis.Cast();
                }
            }
        }


        private static void Usecleanse()
        {
            if (_player.IsDead
                || (_config.Item("Cleansemode").GetValue<StringList>().SelectedIndex == 1
                    && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)) return;
            if (Cleanse(_player) && _config.Item("useqss").GetValue<bool>())
            {
                if (_player.HasBuff("zedulttargetmark"))
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140)) Utility.DelayAction.Add(1000, () => Items.UseItem(3140));
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139)) Utility.DelayAction.Add(1000, () => Items.UseItem(3139));
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137)) Utility.DelayAction.Add(1000, () => Items.UseItem(3137));
                }
                else
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140)) Items.UseItem(3140);
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139)) Items.UseItem(3139);
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137)) Items.UseItem(3137);
                }
            }
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => (hero.IsAlly || hero.IsMe)))
            {
                var usemikael = _config.Item("usemikael").GetValue<bool>();
                var mikaeluse = hero.Health
                                <= (hero.MaxHealth * (_config.Item("mikaelusehp").GetValue<Slider>().Value) / 100);
                if (((Cleanse(hero) && usemikael) || mikaeluse) && _config.Item("mikaeluse" + hero.BaseSkinName) != null
                    && _config.Item("mikaeluse" + hero.BaseSkinName).GetValue<bool>() == true)
                {
                    if (_mikael.IsReady() && hero.Distance(_player.ServerPosition) <= _mikael.Range)
                    {
                        if (_player.HasBuff("zedulttargetmark")) Utility.DelayAction.Add(500, () => _mikael.Cast(hero));
                        else _mikael.Cast(hero);
                    }
                }
            }
        }

        private static bool Cleanse(Obj_AI_Hero hero)
        {
            var cc = false;
            if (_config.Item("blind").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Blind))
                {
                    cc = true;
                }
            }

            if (_config.Item("charm").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Charm))
                {
                    cc = true;
                }
            }

            if (_config.Item("fear").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Fear))
                {
                    cc = true;
                }
            }

            if (_config.Item("flee").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Flee))
                {
                    cc = true;
                }
            }

            if (_config.Item("snare").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Snare))
                {
                    cc = true;
                }
            }

            if (_config.Item("taunt").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Taunt))
                {
                    cc = true;
                }
            }

            if (_config.Item("suppression").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Suppression))
                {
                    cc = true;
                }
            }

            if (_config.Item("stun").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Stun))
                {
                    cc = true;
                }
            }

            if (_config.Item("polymorph").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Polymorph))
                {
                    cc = true;
                }
            }

            if (_config.Item("silence").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Silence))
                {
                    cc = true;
                }
            }

            if (_config.Item("zedultexecute").GetValue<bool>())
            {
                if (_player.HasBuff("zedulttargetmark"))
                {
                    cc = true;
                }
            }

            return cc;
        }

        // princer007  Code
        private static int Getallies(float range)
        {
            int allies = 0;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>()) if (hero.IsAlly && !hero.IsMe && _player.Distance(hero) <= range) allies++;
            return allies;
        }

        private static void Orbwalking_BeforeAttack(LeagueSharp.Common.Orbwalking.BeforeAttackEventArgs args)
        {
            if (Getallies(1000) > 0 && ((Obj_AI_Base)_orbwalker.GetTarget()).IsMinion
                && /*args.Unit.IsMinion &&*/ _config.Item("support").GetValue<bool>()) args.Process = false;
        }

        private static void Smiteontarget()
        {
            if (_smite == null) return;
            var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(570));
            var smiteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Smite);
            var usesmite = _config.Item("smitecombo").GetValue<bool>();
            if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker" && usesmite
                && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready)
            {
                if (hero != null && (!hero.HasBuffOfType(BuffType.Stun) || !hero.HasBuffOfType(BuffType.Slow)))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
                else if (smiteDmg >= hero.Health)
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
            }

            if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel" && usesmite
                && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready && hero.IsValidTarget(570))
            {
                ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
            }
        }

        private static void AutoR()
        {
            if (_player.HasBuff("Recall") || _player.InFountain()) return;
            if (_config.Item("onmeR").GetValue<bool>() && _config.Item("onmeR").GetValue<bool>()
                && (_player.Health / _player.MaxHealth) * 100 <= _config.Item("ultiSelfHP").GetValue<Slider>().Value
                && _r.IsReady() && Utility.CountEnemiesInRange(650) > 0)
            {
                _r.Cast(_player);
            }
        }

        private static void AllyR()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
            {
                if (_player.HasBuff("Recall") || hero.InFountain()) return;
                if (!hero.IsValidTarget(_r.Range)) return;
                if (_config.Item("allyR").GetValue<bool>()
                    && (hero.Health / hero.MaxHealth) * 100 <= _config.Item("ultiallyHP").GetValue<Slider>().Value
                    && _r.IsReady() && Utility.CountEnemiesInRange(1000) > 0
                    && hero.Distance(_player.ServerPosition) <= _r.Range)
                    if (_config.Item("userally" + hero.BaseSkinName) != null
                        && _config.Item("userally" + hero.BaseSkinName).GetValue<bool>() == true)
                    {
                        _r.Cast(hero);
                    }
            }
        }

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(
                _player.ServerPosition,
                _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            var iusehppotion = _config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = _player.Health
                               <= (_player.MaxHealth * (_config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana
                               <= (_player.MaxMana * (_config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0
                || (mobs.Count > 0 && _config.Item("Activejungle").GetValue<KeyBind>().Active && _smite != null))
            {
                if (iusepotionhp && iusehppotion
                    && !(ObjectManager.Player.HasBuff("RegenerationPotion")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                {
                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }

                    if (Items.HasItem(2031) && Items.CanUseItem(2031))
                    {
                        Items.UseItem(2031);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }

                if (iusepotionmp && iusemppotion
                    && !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }

                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }
            }
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var itemscheck = _player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker"
                             || _player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel";
            var damage = 0d;
            if (_igniteSlot != SpellSlot.Unknown && _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready) damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            if (_q.IsReady()) damage += _player.GetSpellDamage(enemy, SpellSlot.Q);
            if (_e.IsReady() || ObjectManager.Player.HasBuff("JudicatorRighteousFury"))
            {
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);
                damage = damage + _player.GetAutoAttackDamage(enemy, true) * 4;
            }

            if (itemscheck && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready)
            {
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Smite);
            }

            if (ObjectManager.Player.HasBuff("LichBane"))
            {
                damage += _player.BaseAttackDamage * 0.75 + _player.FlatMagicDamageMod * 0.5;
            }

            return (float)damage;
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(_r.Range + 200, TargetSelector.DamageType.Magical);
            if (target != null)
            {
                Smiteontarget();
                UseItemes();

                if (target.IsValidTarget(600) && _config.Item("UseIgnitecombo").GetValue<bool>()
                    && _igniteSlot != SpellSlot.Unknown
                    && _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (ComboDamage(target) > target.Health - 100)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, target);
                    }
                }

                if (_config.Item("UseQCombo").GetValue<bool>() && _q.IsReady() && target.IsValidTarget(_q.Range))
                {
                    _q.Cast(target);
                }

                if (_config.Item("UseECombo").GetValue<bool>() && _e.IsReady() && target.IsValidTarget(525))
                {
                    _e.Cast();
                }

                if (_w.IsReady() && _config.Item("UseWCombo").GetValue<bool>() && target.IsValidTarget(_w.Range)
                    && _player.Distance(target.Position) > _q.Range)
                {
                    _w.Cast(_player);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_q.IsReady() && gapcloser.Sender.IsValidTarget(_q.Range) && _config.Item("GapCloserE").GetValue<bool>())
            {
                _q.Cast(gapcloser.Sender);
            }
        }

        private static void Escape()
        {
            var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
            _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (_player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready && _player.IsMe)
            {
                if (_w.IsReady() && Utility.CountEnemiesInRange(1200) > 0)
                {
                    _player.Spellbook.CastSpell(SpellSlot.W, _player);
                }
            }

            if (target.IsValidTarget(_q.Range) && _q.IsReady())
            {
                _q.Cast(target);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget(_q.Range) && _q.IsReady() && _config.Item("UseQHarass").GetValue<bool>())
            {
                _q.Cast(target);
            }

            if (target.IsValidTarget(_q.Range) && _e.IsReady() && _config.Item("UseEHarass").GetValue<bool>()) _e.Cast();
        }


        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, _q.Range);
            foreach (var minion in minions)
            {
                if (_config.Item("UseQLane").GetValue<bool>() && _q.IsReady())
                {
                    if (minions.Count > 2)
                    {
                        _q.Cast(minion);

                    }

                    else
                        foreach (var minionQ in minions)
                            if (!Orbwalking.InAutoAttackRange(minion)
                                && minionQ.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q)) _q.Cast(minionQ);
                }

                if (_config.Item("UseELane").GetValue<bool>() && _e.IsReady())
                {
                    if (minions.Count > 4)
                    {
                        _e.Cast();

                    }
                }
            }
        }

        private static void Lasthit()
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLast").GetValue<bool>();
            var useE = _config.Item("UseELast").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }

                if (_e.IsReady() && useE && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.E)
                    && allMinions.Count > 2)
                {
                    _e.Cast();
                }
            }
        }

        private static void JungleFarm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var mobs = MinionManager.GetMinions(
                _player.ServerPosition,
                _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (_config.Item("UseQjungle").GetValue<bool>() && _q.IsReady() && mob.IsValidTarget(_q.Range)
                    && !mob.Name.Contains("Mini"))
                {
                    _q.Cast(mob);
                }

                if (_config.Item("UseEjungle").GetValue<bool>() && _e.IsReady() && mob.IsValidTarget(_q.Range))
                {
                    _e.Cast();
                }
            }
        }

        private static void AutoW()
        {
            if (_player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready && _player.IsMe)
            {
                if (_player.HasBuff("Recall") || _player.InFountain()) return;

                if (_config.Item("onmeW").GetValue<bool>() && _w.IsReady()
                    && _player.Health <= (_player.MaxHealth * (_config.Item("healper").GetValue<Slider>().Value) / 100))
                {
                    _player.Spellbook.CastSpell(SpellSlot.W, _player);
                }
            }
        }

        private static void AllyW()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
            {
                if (_player.HasBuff("Recall") || hero.HasBuff("Recall") || hero.InFountain()) return;
                if (!hero.IsValidTarget(_w.Range)) return;
                if (_config.Item("allyW").GetValue<bool>()
                    && (hero.Health / hero.MaxHealth) * 100 <= _config.Item("allyhealper").GetValue<Slider>().Value
                    && _w.IsReady() && Utility.CountEnemiesInRange(1200) > 0
                    && hero.Distance(_player.ServerPosition) <= _w.Range)
                    if (_config.Item("usewally" + hero.BaseSkinName) != null
                        && _config.Item("usewally" + hero.BaseSkinName).GetValue<bool>() == true)
                    {
                        _w.Cast(hero);
                    }
            }
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                var qhDmg = _player.GetSpellDamage(hero, SpellSlot.Q);

                if (hero.IsValidTarget(600) && _config.Item("UseIgnite").GetValue<bool>()
                    && _igniteSlot != SpellSlot.Unknown
                    && _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > hero.Health)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                }

                if (_q.IsReady() && hero.IsValidTarget(_q.Range) && _config.Item("UseQKs").GetValue<bool>())
                {
                    if (hero.Health <= qhDmg)
                    {
                        _q.Cast(hero);
                    }
                }
            }
        }

        public static readonly string[] Smitetype =
            {
                "s5_summonersmiteplayerganker", "s5_summonersmiteduel",
                "s5_summonersmitequick", "itemsmiteaoe", "summonersmite"
            };

        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = _config.Item("Activejungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var useblue = _config.Item("Useblue").GetValue<bool>();
            var usered = _config.Item("Usered").GetValue<bool>();
            var health = (100 * (_player.Health / _player.MaxHealth)) < _config.Item("healthJ").GetValue<Slider>().Value;
            var mana = (100 * (_player.Mana / _player.MaxMana)) < _config.Item("manaJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            }
            else
            {
                jungleMinions = new string[]
                                    {
                                        "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_RiftHerald",
                                        "SRU_Red", "SRU_Krug", "SRU_Dragon_Air", "SRU_Dragon_Water", "SRU_Dragon_Fire",
                                        "SRU_Dragon_Elder", "SRU_Baron"
                                    };
            }

            var minions = MinionManager.GetMinions(_player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (Obj_AI_Base minion in minions)
                {
                    if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline && minion.Health <= smiteDmg
                        && jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name)))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }

                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name))
                        && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && useblue && mana && minion.Health >= smiteDmg
                             && jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue"))
                             && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg
                             && jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red"))
                             && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var harass = (_config.Item("harasstoggle").GetValue<KeyBind>().Active);

            if (_config.Item("Drawharass").GetValue<bool>())
            {
                if (harass)
                {
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.92f,
                        System.Drawing.Color.GreenYellow,
                        "Auto harass Enabled");
                }
                else
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.92f,
                        System.Drawing.Color.OrangeRed,
                        "Auto harass Disabled");
            }

            if (_config.Item("Drawsmite").GetValue<bool>() && _smite != null)
            {
                if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.GreenYellow,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.OrangeRed,
                        "Smite Jungle Off");

                if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker"
                    || _player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel")
                {
                    if (_config.Item("smitecombo").GetValue<bool>())
                    {
                        Drawing.DrawText(
                            Drawing.Width * 0.02f,
                            Drawing.Height * 0.90f,
                            System.Drawing.Color.GreenYellow,
                            "Smite Target On");
                    }
                    else
                        Drawing.DrawText(
                            Drawing.Width * 0.02f,
                            Drawing.Height * 0.90f,
                            System.Drawing.Color.OrangeRed,
                            "Smite Target Off");
                }
            }

            if (_config.Item("DrawQ").GetValue<bool>() && _q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.GreenYellow);
            }

            if (_config.Item("DrawW").GetValue<bool>() && _w.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.GreenYellow);
            }

            if (_config.Item("DrawE").GetValue<bool>() && _e.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.GreenYellow);
            }

            if (_config.Item("DrawR").GetValue<bool>() && _r.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.GreenYellow);
            }
        }
    }
}


