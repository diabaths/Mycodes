﻿using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;


namespace D_Rengar
{
    internal class Program
    {
        private static Obj_AI_Hero _player;
        private static Spell _q, _w, _e, _r;
       
        private static Orbwalking.Orbwalker _orbwalker;
        private static SpellSlot _igniteSlot;
        private static Items.Item _youmuu, _tiamat, _hydra, _blade, _bilge, _rand, _lotis;
        private static SpellSlot _smiteSlot = SpellSlot.Unknown;
        private static Menu _config;
        private static Spell _smite;
        //Credits to Kurisu
        private static readonly int[] SmitePurple = {3713, 3726, 3725, 3724, 3723, 3933};
        private static readonly int[] SmiteGrey = {3711, 3722, 3721, 3720, 3719, 3932};
        private static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714, 3931};
        private static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707, 3930};

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.ChampionName != "Rengar")
                return;
            _q = new Spell(SpellSlot.Q, 250f);
            _w = new Spell(SpellSlot.W, 400);
            _e = new Spell(SpellSlot.E, 980f);
            _r = new Spell(SpellSlot.R, 2000f);

            _e.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine);

            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);
            _youmuu = new Items.Item(3142, 10);
            _igniteSlot = _player.GetSpellSlot("SummonerDot");
            SetSmiteSlot();

            //D rengar
            _config = new Menu("D-Rengar", "D-Rengar", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboPrio", "Empowered Priority").SetValue(new StringList(new[] {"Q", "W", "E"}, 2)));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEEC", "Use Empower E when Q range<target ")).SetValue(true);
           _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Youmuu", "Use Youmuu's")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));
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
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Righteous", "Use Righteous Glory"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Righteousenemys", "Righteous Glory if  Enemy >=").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(
                    new MenuItem("Righteousenemysrange", "Righteous Glory Range Check").SetValue(new Slider(800, 400,
                        1400)));

            _config.SubMenu("items").AddSubMenu(new Menu("Potions", "Potions"));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Flask/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "Use Mana potion/Flask/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "If Mana % <").SetValue(new Slider(35, 1, 100)));

            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(
                  new MenuItem("HarrPrio", "Empowered Priority").SetValue(new StringList(new[] { "Q", "W", "E" }, 0)));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseItemsharass", "Use Tiamat/Hydra")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //LaneClear
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
            _config.SubMenu("Farm")
               .SubMenu("LastHit")
               .AddItem(
                   new MenuItem("LastPrio", "Empowered Priority").SetValue(new StringList(new[] { "Q", "W", "E" }, 0)));

            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(new MenuItem("LastSave", "Save Ferocity"))
                .SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Q LastHit")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseWLH", "W LastHit")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseELH", "E LastHit")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "LastHit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("LaneClear", "LaneClear"));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LanePrio", "Empowered Priority").SetValue(new StringList(new[] {"Q", "W", "E"}, 0)));

            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneSave", "Save Ferocity"))
                .SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("UseItemslane", "Use Items"))
                .SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseQL", "Q LaneClear")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseWL", "W LaneClear")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseEL", "E LaneClear")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("ActiveLane", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("JungleClear", "JungleClear"));
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(
                    new MenuItem("JunglePrio", "Empowered Priority").SetValue(new StringList(new[] {"Q", "W", "E"}, 0)));

            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(new MenuItem("JungleSave", "Save Ferocity"))
                .SetValue(true);

            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(new MenuItem("UseItemsjungle", "Use Items"))
                .SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseQJ", "Q Jungle")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseWJ", "W Jungle")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseEJ", "E Jungle")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(
                    new MenuItem("ActiveJungle", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Smite 
            _config.AddSubMenu(new Menu("Smite", "Smite"));
            _config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseIgnite", "Use Ignite KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQM", "Use Q KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseWM", "Use W KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEM", "Use E KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEInt", "E to Interrupt")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("AutoW", "use W to Heal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("AutoWHP", "If Health % <").SetValue(new Slider(35, 1, 100)));


            //Misc
            _config.AddSubMenu(new Menu("HitChance", "HitChance"));
            _config.SubMenu("HitChance")
                .AddItem(new MenuItem("Echange", "E Hit").SetValue(
                    new StringList(new[] {"Low", "Medium", "High", "Very High"})));

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "Draw smite")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawharass", "Draw AutoHarass")).SetValue(true);
            _config.AddToMainMenu();

            Game.PrintChat("<font color='#881df2'>D-Rengar by Diabaths</font> Loaded.");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            //Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Game.PrintChat(
                "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_config.Item("AutoW").GetValue<bool>())
            {
                AutoHeal();
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (!_config.Item("ActiveCombo").GetValue<KeyBind>().Active &&
                (_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 _config.Item("harasstoggle").GetValue<KeyBind>().Active))
            {
                Harass();

            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active)
            {
                Laneclear();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active)
            {
                LastHit();
            }
            Usepotion();
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            KillSteal();

        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (_player.Mana < 5) return;
            if (_e.IsReady() && unit.IsValidTarget(_e.Range) &&
                _config.Item("UseEInt").GetValue<bool>())
            {
                var predE = _e.GetPrediction(unit);
                if  (predE.Hitchance >= Echange() && predE.CollisionObjects.Count == 0)
                    _e.Cast(unit);
            }
        }
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (_player.Buffs.Any(x => x.Name.Contains("rengarq")))
                {

                    Orbwalking.ResetAutoAttackTimer();
                }
            }
        }

        private static void Smiteontarget()
        {
            if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel" ||
                      _player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker")
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var smiteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Smite);
                var usesmite = _config.Item("smitecombo").GetValue<bool>();
                if (SmiteBlue.Any(i => Items.HasItem(i)) && usesmite &&
                    ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                    hero.IsValidTarget(_smite.Range))
                {
                    if (!hero.HasBuffOfType(BuffType.Stun) || !hero.HasBuffOfType(BuffType.Slow))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                    }
                    else if (smiteDmg >= hero.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                    }
                }
                if (SmiteRed.Any(i => Items.HasItem(i)) && usesmite &&
                    ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                    hero.IsValidTarget(_smite.Range))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
            }
        }

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _e.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var iusehppotion = _config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = _player.Health <=
                               (_player.MaxHealth*(_config.Item("usepotionhp").GetValue<Slider>().Value)/100);
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana <=
                               (_player.MaxMana*(_config.Item("usepotionmp").GetValue<Slider>().Value)/100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("ActiveJungle").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
                                                                                               SmiteBlue.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmiteRed.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmitePurple.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmiteBlue.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmiteGrey.Any(
                                                                                                   i => Items.HasItem(i))
                    )))
            {
                if (iusepotionhp && iusehppotion &&
                    !(ObjectManager.Player.HasBuff("RegenerationPotion", true) ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }
                }


                if (iusepotionmp && iusemppotion &&
                    !(ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true) ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2004) && Items.CanUseItem(2004))
                    {
                        Items.UseItem(2004);
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
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.92f, System.Drawing.Color.GreenYellow,
                        "Auto harass Enabled");
                }
                else
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.92f, System.Drawing.Color.OrangeRed,
                        "Auto harass Disabled");
            }
            if (_config.Item("Drawsmite").GetValue<bool>())
            {
                if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.88f, System.Drawing.Color.GreenYellow,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.88f, System.Drawing.Color.OrangeRed,
                        "Smite Jungle Off");
                if (SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i)))
                {
                    if (_config.Item("smitecombo").GetValue<bool>())
                    {
                        Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.90f, System.Drawing.Color.GreenYellow,
                            "Smite Target On");
                    }
                    else
                        Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.90f, System.Drawing.Color.OrangeRed,
                            "Smite Target Off");
                }
            }

            if (_config.Item("DrawQ").GetValue<bool>() && _q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
            }
            if (_config.Item("DrawW").GetValue<bool>()&&_w.Level>0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
            }
            if (_config.Item("DrawE").GetValue<bool>() && _e.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
            }
        }
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (_igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q) * 1.2;
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.W) * 3;
            if (_e.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);
            if (_r.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.R);
            damage += _player.GetAutoAttackDamage(enemy, true) * 2;
            return (float)damage;
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var useE = _config.Item("UseEC").GetValue<bool>();
            var useEE = _config.Item("UseEEC").GetValue<bool>();
            var iYoumuu = _config.Item("Youmuu").GetValue<bool>();
            Smiteontarget();
            if (target != null && _config.Item("UseIgnite").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
               _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (ComboDamage(target) > target.Health)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, target);
                }
            }
            if (iYoumuu && _youmuu.IsReady())
            {
                if (_player.HasBuff("RengarR"))
                {
                    Utility.DelayAction.Add(300, () => _youmuu.Cast());
                }
                else if (target.IsValidTarget())
                {
                    _youmuu.Cast();}
            }
            if (_player.Mana <= 4)
            {

                if (useQ)
                {
                    var tq = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                    if (tq.IsValidTarget(_q.Range) && _q.IsReady())
                        _q.Cast();
                }
                if (useW)
                {
                    var tw = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                    if (tw.IsValidTarget(_w.Range) && _w.IsReady())
                        _w.Cast();
                }
                if (useE)
                {
                    var te = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Physical);
                    var predE = _e.GetPrediction(te);
                    if (!_player.HasBuff("rengarpassivebuff")&& te.IsValidTarget(_e.Range) && _e.IsReady() && predE.Hitchance >= Echange() && predE.CollisionObjects.Count == 0)
                        _e.Cast(te);
                }
            }

            if (_player.Mana == 5)
            {
                if (useQ &&
                    _config.Item("ComboPrio").GetValue<StringList>().SelectedIndex == 0)
                {
                    var tq = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                    if (tq.IsValidTarget(_q.Range) && _q.IsReady())
                        _q.Cast();
                }
                if (useW &&
                    _config.Item("ComboPrio").GetValue<StringList>().SelectedIndex == 1)
                {
                    var tw = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                    if (tw.IsValidTarget(_w.Range) && _w.IsReady())
                        _w.Cast();
                }
                if (useE &&
                    _config.Item("ComboPrio").GetValue<StringList>().SelectedIndex == 2 )
                {
                    var te = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Physical);
                    var predE = _e.GetPrediction(te);
                    if (te.IsValidTarget(_e.Range) && _e.IsReady() && predE.Hitchance >= Echange() && predE.CollisionObjects.Count == 0)
                        _e.Cast(te);
                }
                if (useEE && !_player.HasBuff("RengarR")&&(_config.Item("ComboPrio").GetValue<StringList>().SelectedIndex == 1 ||
                              _config.Item("ComboPrio").GetValue<StringList>().SelectedIndex == 0))
                {
                    var te = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Physical);

                    if (_player.Distance(te) > _q.Range + 100f)
                    {
                        var predE = _e.GetPrediction(te);
                        if (te.IsValidTarget(_e.Range) && _e.IsReady() && predE.Hitchance >= Echange() && predE.CollisionObjects.Count == 0)
                        _e.Cast(te);
                    }
                }
            }
            UseItemes();
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useW = _config.Item("UseWH").GetValue<bool>();
            var useE = _config.Item("UseEH").GetValue<bool>();
            var useItemsH = _config.Item("UseItemsharass").GetValue<bool>();
            if (_player.Mana <= 4)
            {

                if (useQ)
                {
                    var tq = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                    if (tq.IsValidTarget(_q.Range) && _q.IsReady())
                        _q.Cast();
                }
                if (useW)
                {
                    var tw = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                    if (tw.IsValidTarget(_w.Range) && _w.IsReady())
                        _w.Cast();
                }
                if (useE)
                {
                    var te = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Physical);
                    var predE = _e.GetPrediction(te);
                    if (te.IsValidTarget(_e.Range) && _e.IsReady() && predE.Hitchance >= Echange() &&
                        predE.CollisionObjects.Count == 0)
                        _e.Cast(te);
                }
            }

            if (_player.Mana == 5)
            {
                if (useQ &&
                    _config.Item("HarrPrio").GetValue<StringList>().SelectedIndex == 0)
                {
                    var tq = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                    if (tq.IsValidTarget(_q.Range) && _q.IsReady())
                        _q.Cast();
                }
                if (useW &&
                    _config.Item("HarrPrio").GetValue<StringList>().SelectedIndex == 1)
                {
                    var tw = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                    if (tw.IsValidTarget(_w.Range) && _w.IsReady())
                        _w.Cast();
                }
                if (useE &&
                    _config.Item("HarrPrio").GetValue<StringList>().SelectedIndex == 2)
                {
                    var te = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Physical);
                    var predE = _e.GetPrediction(te);
                    if (te.IsValidTarget(_e.Range) && _e.IsReady() && predE.Hitchance >= Echange() &&
                        predE.CollisionObjects.Count == 0)
                        _e.Cast(te);
                }
            }
            if (useItemsH && _tiamat.IsReady() && target.IsValidTarget(_tiamat.Range))
            {
                _tiamat.Cast();
            }
            if (useItemsH && _hydra.IsReady() && target.IsValidTarget(_hydra.Range))
            {
                _hydra.Cast();
            }
        }
        private static HitChance Echange()
        {
            switch (_config.Item("Echange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }
        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                var qDmg = _player.GetSpellDamage(hero, SpellSlot.Q);
                var wDmg = _player.GetSpellDamage(hero, SpellSlot.Q);
                var eDmg = _player.GetSpellDamage(hero, SpellSlot.Q);

                if (hero.IsValidTarget(600) && _config.Item("UseIgnite").GetValue<bool>() &&
                    _igniteSlot != SpellSlot.Unknown &&
                    _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > hero.Health)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                }
                if (_q.IsReady() && _config.Item("UseQM").GetValue<bool>() && _player.Distance(hero) <= _q.Range)
                {
                    var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                        if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                            qDmg > t.Health)
                            _q.Cast(t);
                }

                if (_w.IsReady() && _config.Item("UseWM").GetValue<bool>() && _player.Distance(hero) <= _w.Range)
                {
                    var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                        if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                            wDmg > t.Health)
                            _w.Cast(t);
                }
                if (_q.IsReady() && _config.Item("UseEM").GetValue<bool>() && _player.Distance(hero) <= _e.Range)
                {
                    var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                        if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                            eDmg > t.Health && _e.GetPrediction(t).Hitchance >= HitChance.High)
                            _e.Cast(t);
                }
            }
        }

        private static string Smitetype()
        {
            if (SmiteBlue.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(i => Items.HasItem(i)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }


        //Credits to metaphorce
        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                _smiteSlot = spell.Slot;
                _smite = new Spell(_smiteSlot, 700);
                return;
            }
        }

        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level/5;
            float[] dmgs = {370 + 20*level, 330 + 30*level, 240 + 40*level, 100 + 50*level};
            return (int) dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = _config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var usered = _config.Item("Usered").GetValue<bool>();
            var health = (100*(_player.Health/_player.MaxHealth)) < _config.Item("healthJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                jungleMinions = new string[] {"TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf"};
            }
            else
            {
                jungleMinions = new string[]
                {
                    "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                    "SRU_Baron"
                };
            }
            var minions = MinionManager.GetMinions(_player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (Obj_AI_Base minion in minions)
                {
                    if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline &&
                        minion.Health <= smiteDmg &&
                        jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name)))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name)) &&
                        !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }

        private static void Laneclear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All);
            var useItemsl = _config.Item("UseItemslane").GetValue<bool>();
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useWl = _config.Item("UseWL").GetValue<bool>();
            var useEl = _config.Item("UseEL").GetValue<bool>();
            var save = _config.Item("LaneSave").GetValue<bool>();
            if (_player.Mana <= 4)
            {
                if (_q.IsReady() && useQl && allMinionsQ.Count > 0)
                {
                    _q.Cast();
                }
                if (_w.IsReady() && useWl && allMinionsW.Count > 0)
                {
                    _w.Cast();
                }
                if (_e.IsReady() && useEl && allMinionsE.Count > 0)
                {
                    var fl2 = _e.GetLineFarmLocation(allMinionsE, _e.Width);
                    if (fl2.MinionsHit >= 3)
                    {
                        _e.Cast(fl2.Position);
                    }
                    else
                        foreach (var minion in allMinionsE)
                            if (!Orbwalking.InAutoAttackRange(minion) &&
                                minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                                _e.Cast(minion);
                }
            }
            if (_player.Mana == 5)
            {
                if (save) return;
                if (_q.IsReady() && _config.Item("LanePrio").GetValue<StringList>().SelectedIndex == 0 && useQl &&
                    allMinionsQ.Count > 0)
                {
                    _q.Cast();
                }
                if (_w.IsReady() && _config.Item("LanePrio").GetValue<StringList>().SelectedIndex == 1 && useWl &&
                    allMinionsW.Count > 0)
                {
                    _w.Cast();
                }
                if (_e.IsReady() && _config.Item("LanePrio").GetValue<StringList>().SelectedIndex == 2 && useEl &&
                    allMinionsE.Count > 0)
                {
                    var fl2 = _e.GetLineFarmLocation(allMinionsE, _e.Width);
                    if (fl2.MinionsHit >= 3)
                    {
                        _e.Cast(fl2.Position);
                    }
                    else
                        foreach (var minion in allMinionsE)
                            if (!Orbwalking.InAutoAttackRange(minion) &&
                                minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                                _e.Cast(minion);
                }
            }
            foreach (var minion in allMinionsE)
            {
                if (useItemsl && _tiamat.IsReady() && minion.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();
                }
                if (useItemsl && _hydra.IsReady() && minion.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _e.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useItemsJ = _config.Item("UseItemsjungle").GetValue<bool>();
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useW = _config.Item("UseWJ").GetValue<bool>();
            var useE = _config.Item("UseEJ").GetValue<bool>();
            var save = _config.Item("JungleSave").GetValue<bool>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useItemsJ && _tiamat.IsReady() && mob.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();
                }
                if (useItemsJ && _hydra.IsReady() && mob.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();
                }
                if (_player.Mana <= 4)
                {
                    if (useQ && _q.IsReady() && mob.IsValidTarget(_q.Range))
                    {
                        _q.Cast();
                    }
                    if (_w.IsReady() && useW&& mob.IsValidTarget(_w.Range))
                    {
                        _w.Cast();
                    }
                    if (_e.IsReady() && useE && mob.IsValidTarget(_e.Range))
                    {
                        _e.Cast(mob);
                    }
                }
                if (_player.Mana == 5)
                {
                    if (save) return;
                    if (mob.IsValidTarget(_q.Range)&&_q.IsReady() && _config.Item("JunglePrio").GetValue<StringList>().SelectedIndex == 0 && useQ)
                    {
                        _q.Cast();
                    }
                    if (mob.IsValidTarget(_w.Range)&&_w.IsReady() && _config.Item("JunglePrio").GetValue<StringList>().SelectedIndex == 1 && useW)
                    {
                        _w.Cast();
                    }
                    if (mob.IsValidTarget(_e.Range)&&_e.IsReady() && _config.Item("JunglePrio").GetValue<StringList>().SelectedIndex == 2 && useE)
                    {

                        _e.Cast(mob);

                    }
                  
                }
            }
        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useW = _config.Item("UseWLH").GetValue<bool>();
            var useE = _config.Item("UseELH").GetValue<bool>();
            var save = _config.Item("LastSave").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (_player.Mana <= 4)
                {
                    if (useQ && _q.IsReady() && _player.Distance(minion) < _q.Range &&
                        minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        _q.Cast();
                    }

                    if (_w.IsReady() && useW && _player.Distance(minion) < _w.Range &&
                        minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.W))
                    {
                        _w.Cast();
                    }
                    if (_e.IsReady() && useE && _player.Distance(minion) < _e.Range &&
                        minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                    {
                        _e.Cast(minion);
                    }
                }
                if (_player.Mana == 5)
                {
                    if (save) return;
                    if (useQ && _q.IsReady() && _player.Distance(minion) < _q.Range &&
                        minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.Q) &&
                        _config.Item("LastPrio").GetValue<StringList>().SelectedIndex == 0)
                    {
                        _q.Cast();
                    }
                    if (_w.IsReady() && useW && _player.Distance(minion) < _w.Range &&
                        minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.W)
                        && _config.Item("LastPrio").GetValue<StringList>().SelectedIndex == 1)
                    {
                        _w.Cast();
                    }
                    if (_e.IsReady() && useE && _player.Distance(minion) < _e.Range &&
                        minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E) &&
                        _config.Item("LastPrio").GetValue<StringList>().SelectedIndex == 2)
                    {
                        _e.Cast(minion);
                    }
                }
            }
        }

        private static void AutoHeal()
        {
            var health = (100*(_player.Health/_player.MaxHealth)) < _config.Item("AutoWHP").GetValue<Slider>().Value;

            if (_player.HasBuff("Recall") || _player.Mana <= 4) return;


            if (_w.IsReady() && health)
            {
                _w.Cast();
            }
        }

        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iBilge = _config.Item("Bilge").GetValue<bool>();
                var iBilgeEnemyhp = hero.Health <=
                                    (hero.MaxHealth*(_config.Item("BilgeEnemyhp").GetValue<Slider>().Value)/100);
                var iBilgemyhp = _player.Health <=
                                 (_player.MaxHealth*(_config.Item("Bilgemyhp").GetValue<Slider>().Value)/100);
                var iBlade = _config.Item("Blade").GetValue<bool>();
                var iBladeEnemyhp = hero.Health <=
                                    (hero.MaxHealth*(_config.Item("BladeEnemyhp").GetValue<Slider>().Value)/100);
                var iBlademyhp = _player.Health <=
                                 (_player.MaxHealth*(_config.Item("Blademyhp").GetValue<Slider>().Value)/100);
                var iOmen = _config.Item("Omen").GetValue<bool>();
                var iOmenenemys = hero.CountEnemiesInRange(450) >= _config.Item("Omenenemys").GetValue<Slider>().Value;
                var iRighteous = _config.Item("Righteous").GetValue<bool>();
                var iRighteousenemys =
                    hero.CountEnemiesInRange(_config.Item("Righteousenemysrange").GetValue<Slider>().Value) >=
                    _config.Item("Righteousenemys").GetValue<Slider>().Value;
                var iTiamat = _config.Item("Tiamat").GetValue<bool>();
                var iHydra = _config.Item("Hydra").GetValue<bool>();

                if (hero.IsValidTarget(450) && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
                {
                    _bilge.Cast(hero);

                }
                if (hero.IsValidTarget(450) && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
                {
                    _blade.Cast(hero);

                }
                if (iTiamat && _tiamat.IsReady() && hero.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();

                }
                if (iHydra && _hydra.IsReady() && hero.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();

                }
                if (iOmenenemys && iOmen && _rand.IsReady() && hero.IsValidTarget(450))
                {
                    _rand.Cast();
                }
                if (iRighteousenemys && iRighteous && Items.HasItem(3800) && Items.CanUseItem(3800) &&
                    hero.IsValidTarget(_config.Item("Righteousenemysrange").GetValue<Slider>().Value))
                {
                    Items.UseItem(3800);
                }
            }
            var ilotis = _config.Item("lotis").GetValue<bool>();
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth*(_config.Item("lotisminhp").GetValue<Slider>().Value)/100) &&
                        hero.Distance(_player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }
        }
    }
}