﻿#region

using System;
using LeagueSharp;
using System.Linq;
using LeagueSharp.Common;
#endregion

namespace D_Shyvana
{
    internal class Program
    {
        private static Orbwalking.Orbwalker _orbwalker;

        private const string ChampionName = "Shyvana";

        private static Spell _q, _w, _e, _r;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis;

        private static SpellSlot _igniteSlot;

        private static SpellSlot _smiteSlot = SpellSlot.Unknown;

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
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 0);
            _w = new Spell(SpellSlot.W, 350f);
            _e = new Spell(SpellSlot.E, 925f);
            _r = new Spell(SpellSlot.R, 1000f);

            _e.SetSkillshot(0.25f, 60f, 1700, false, SkillshotType.SkillshotLine);
            _r.SetSkillshot(0.25f, 150f, 1500, false, SkillshotType.SkillshotLine);

            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");
            SetSmiteSlot();

            //D Shyvana
            _config = new Menu("D-Shyvana", "D-Shyvana", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRE", "AutoR Min Targ")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("MinTargets", "Ult when>=min enemy(COMBO)").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
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
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));

            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
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
                .SubMenu("LaneClear")
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

            //Forest
            _config.AddSubMenu(new Menu("Forest Gump", "Forest Gump"));
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("UseWF", "Use W ")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("UseEF", "Use E ")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("UseRF", "Use R ")).SetValue(true);
            _config.SubMenu("Forest Gump")
                .AddItem(
                    new MenuItem("Forest", "Active Forest Gump!").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Press)));


            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEM", "Use E KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRM", "Use R KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Gap_E", "R GapClosers")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRInt", "R to Interrupt")).SetValue(true);

            //Misc
            _config.AddSubMenu(new Menu("HitChance", "HitChance"));
            _config.SubMenu("HitChance")
                .AddItem(new MenuItem("Echange", "E Hit").SetValue(
                    new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance")
                .AddItem(new MenuItem("Rchange", "R Hit").SetValue(
                    new StringList(new[] {"Low", "Medium", "High", "Very High"})));

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "Draw smite")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawharass", "Draw AutoHarass")).SetValue(true);

            _config.AddToMainMenu();
            Game.PrintChat("<font color='#881df2'>D-Shyvana by Diabaths</font> Loaded.");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.PrintChat(
                "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

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
            if (_config.Item("Forest").GetValue<KeyBind>().Active)
            {
                Forest();
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

      private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        { /* if (sender.IsMe)
            {
                 Game.PrintChat("Spell name: " + args.SData.Name.ToString());
            }*/
        if (!sender.IsMe)
        {
            return;
        }
        var spell = args.SData;
        if (spell.Name.ToLower().Contains("shyvanadoubleattack"))
        {
           Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
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
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q)*1.2;
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.W)*3;
            if (_e.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);
            if (_r.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.R);
            damage += _player.GetAutoAttackDamage(enemy, true)*2;
            return (float) damage;
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


        private static void Combo()
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var useE = _config.Item("UseEC").GetValue<bool>();
            var useR = _config.Item("UseRC").GetValue<bool>();
            var autoR = _config.Item("UseRE").GetValue<bool>();

            var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            Smiteontarget();
            if (t != null && _config.Item("UseIgnite").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (ComboDamage(t) > t.Health)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, t);
                }
            }
            if (useR && _r.IsReady())
            {
                if (t != null && _r.GetPrediction(t).Hitchance >= Rchange())
                    if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                        ComboDamage(t) > t.Health)
                        _r.CastIfHitchanceEquals(t, HitChance.Medium);
            }
            if (useW && _w.IsReady())
            {
                if (t != null && _player.Distance(t) < _e.Range)
                    _w.Cast();
            }

            if (useE && _e.IsReady())
            {

                if (t != null && _player.Distance(t) < _e.Range &&
                    _e.GetPrediction(t).Hitchance >= Echange())
                    _e.Cast(t);
            }

            if (useQ && _q.IsReady())
            {
                if (t != null && _player.Distance(t) < _w.Range)
                    _q.Cast();
            }

            if (_r.IsReady() && autoR)
            {
                if (ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(_r.Range)) >=
                    _config.Item("MinTargets").GetValue<Slider>().Value
                    && _r.GetPrediction(t).Hitchance >= Rchange())
                    _r.Cast(t);
            }

            UseItemes();
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useW = _config.Item("UseWH").GetValue<bool>();
            var useE = _config.Item("UseEH").GetValue<bool>();
            var useItemsH = _config.Item("UseItemsharass").GetValue<bool>();
            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                if (t != null && t.Distance(_player.Position) < _w.Range)
                    _q.Cast();
            }
            if (useW && _w.IsReady())
            {
                var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                if (t != null && _player.Distance(t) < _w.Range)
                    _w.Cast();
            }
            if (useE && _e.IsReady())
            {
                var t = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                if (t != null && _player.Distance(t) < _e.Range && _e.GetPrediction(t).Hitchance >= Echange())
                    _e.Cast(t);
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

        private static void Laneclear()
        {
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All);
            var rangedMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range + _e.Width,
                MinionTypes.Ranged);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range + _e.Width,
                MinionTypes.All);
            var useItemsl = _config.Item("UseItemslane").GetValue<bool>();
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useWl = _config.Item("UseWL").GetValue<bool>();
            var useEl = _config.Item("UseEL").GetValue<bool>();
            if (_q.IsReady() && useQl && allMinionsW.Count > 0)
            {
                _q.Cast();
            }

            if (_w.IsReady() && useWl)
            {

                if (allMinionsW.Count >= 2)
                {
                    _w.Cast();
                }
                else
                    foreach (var minion in allMinionsW)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.W))
                            _w.Cast();
            }
            if (_e.IsReady() && useEl)
            {
                var fl1 = _e.GetLineFarmLocation(rangedMinionsE, _e.Width);
                var fl2 = _e.GetLineFarmLocation(allMinionsE, _e.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _e.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsE.Count == 1)
                {
                    _e.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsE)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                            _e.Cast(minion);
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

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useW = _config.Item("UseWLH").GetValue<bool>();
            var useE = _config.Item("UseELH").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && _player.Distance(minion) < 200 &&
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
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _w.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useItemsJ = _config.Item("UseItemsjungle").GetValue<bool>();
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useW = _config.Item("UseWJ").GetValue<bool>();
            var useE = _config.Item("UseEJ").GetValue<bool>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && _q.IsReady() && !mob.Name.Contains("Mini"))
                {
                    _q.Cast();
                }
                if (_w.IsReady() && useW)
                {
                    _w.Cast();
                }
                if (_e.IsReady() && useE)
                {
                    _e.Cast(mob);
                }

                if (useItemsJ && _tiamat.IsReady() && mob.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();
                }
                if (useItemsJ && _hydra.IsReady() && mob.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();
                }
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

        private static HitChance Rchange()
        {
            switch (_config.Item("Rchange").GetValue<StringList>().SelectedIndex)
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

        /* private static bool Packets()
         {
             return _config.Item("usePackets").GetValue<bool>();
         }*/

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if (_e.IsReady() && _config.Item("UseEM").GetValue<bool>())
                {
                    if (_e.GetDamage(hero) > hero.Health && hero.IsValidTarget(_e.Range))
                    {
                        _e.CastIfHitchanceEquals(hero, Echange());
                    }
                }
                if (_r.IsReady() && _config.Item("UseRM").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                    if (t != null)
                        if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                            _r.GetDamage(t) > t.Health && _r.GetPrediction(t).Hitchance >= Rchange())
                            _r.Cast(t);
                }
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
                var iTiamat = _config.Item("Tiamat").GetValue<bool>();
                var iHydra = _config.Item("Hydra").GetValue<bool>();
                var iRighteous = _config.Item("Righteous").GetValue<bool>();
                var iRighteousenemys =
                    hero.CountEnemiesInRange(_config.Item("Righteousenemysrange").GetValue<Slider>().Value) >=
                    _config.Item("Righteousenemys").GetValue<Slider>().Value;


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

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var iusehppotion = _config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = _player.Health <=
                               (_player.MaxHealth * (_config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("Activejungle").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
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
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)
                      || ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle", true)
                      || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask", true)))
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
            }
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_r.IsReady() && gapcloser.Sender.IsValidTarget(_r.Range) && _config.Item("Gap_E").GetValue<bool>())
                if (!gapcloser.End.UnderTurret(true) && gapcloser.End.CountEnemiesInRange(700) < 1)
                {
                    _r.Cast(gapcloser.End);
                }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base target, InterruptableSpell spell)
        {
            if (!_config.Item("UseRInt").GetValue<bool>()) return;
            if (target.IsValidTarget(_r.Range) && spell.DangerLevel == InterruptableDangerLevel.High)
            {
                _r.Cast(target);
            }
        }

        private static void Forest()
        {
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (_config.Item("UseRF").GetValue<bool>() && _r.IsReady() && target != null)
            {
                _r.Cast(Game.CursorPos);
            }
            if (_config.Item("UseWF").GetValue<bool>() && _w.IsReady() && target != null)
            {
                _w.Cast();
            }
            if (_config.Item("UseEF").GetValue<bool>() && _e.IsReady() && target != null)
            {
                _e.Cast(target);
            }
        }

        //Credits to Kurisu
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

            if (_config.Item("DrawQ").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.GreenYellow);
            }
            if (_config.Item("DrawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.GreenYellow);
            }
            if (_config.Item("DrawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.GreenYellow);
            }

            if (_config.Item("DrawR").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.GreenYellow);
            }
        }
    }
}

