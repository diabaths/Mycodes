using System;
using System.Linq;
using System.Runtime.InteropServices;
using LeagueSharp;
using LeagueSharp.Common;


namespace D_Corki
{
    internal class Program
    {
        private const string ChampionName = "Corki";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r, _r1, _r2;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static Int32 _lastSkin;

        private static Items.Item _youmuu, _blade, _bilge;

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
            if (_player.ChampionName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 825f);
            _w = new Spell(SpellSlot.W, 800f);
            _e = new Spell(SpellSlot.E, 600f);
            _r = new Spell(SpellSlot.R);
            _r1 = new Spell(SpellSlot.R, 1300f);
            _r2 = new Spell(SpellSlot.R, 1500f);

            _q.SetSkillshot(0.35f, 250f, 1000f, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0f, (float) (45*Math.PI/180), 1500, false, SkillshotType.SkillshotCone);
            _r.SetSkillshot(0.20f, 40f, 2000f, true, SkillshotType.SkillshotLine);

            _youmuu = new Items.Item(3142, 10);
            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);

            //D Corki
            _config = new Menu("D-Corki", "D-Corki", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddSubMenu(new Menu("Use W", "Use W"));
            _config.SubMenu("Combo").SubMenu("Use W").AddItem(new MenuItem("UseWC", "Use W")).SetValue(true);
            _config.SubMenu("Combo")
                .SubMenu("Use W")
                .AddItem(new MenuItem("diveintower", "Dive In tower with W"))
                .SetValue(true);
            _config.SubMenu("Combo")
                .SubMenu("Use W")
                .AddItem(new MenuItem("UseWHE", "Your HP% Use W >").SetValue(new Slider(65, 1, 100)));
            _config.SubMenu("Combo")
                .SubMenu("Use W")
                .AddItem(new MenuItem("EnemyC", "Enemy in R.Range <").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Youmuu", "Use Youmuu's")).SetValue(true);
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
            _config.SubMenu("items").SubMenu("Deffensive").AddSubMenu(new Menu("Cleanse", "Cleanse"));
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
                .AddItem(new MenuItem("Cleansemode", "Use Cleanse"))
                .SetValue(new StringList(new string[2] {"Always", "In Combo"}));


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


            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseRH", "Use R")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("RlimH", "R Amount >").SetValue(new Slider(3, 1, 7)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Q LastHit")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseELH", "E LastHit")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(new MenuItem("Lastmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "LastHit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("LaneClear", "LaneClear"));
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseQL", "Q LaneClear")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseEL", "E LaneClear")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseRL", "R LaneClear")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("RlimL", "R Amount >").SetValue(new Slider(3, 1, 7)));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("ActiveLane", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("JungleClear", "JungleClear"));
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseQJ", "Q Jungle")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseEJ", "E Jungle")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseRJ", "R Jungle")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(new MenuItem("RlimJ", "R Amount >").SetValue(new Slider(3, 1, 7)));
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(
                    new MenuItem("ActiveJungle", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQM", "Use Q KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEM", "Use E KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRM", "Use R KillSteal")).SetValue(true);


            //HitChance
            _config.AddSubMenu(new Menu("HitChance", "HitChance"));

            _config.SubMenu("HitChance").AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("HitChance").SubMenu("Harass").AddItem(new MenuItem("QchangeHar", "Q Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").SubMenu("Harass").AddItem(new MenuItem("EchangeHar", "E Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").SubMenu("Harass").AddItem(new MenuItem("RchangeHar", "R Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("HitChance").SubMenu("Combo").AddItem(new MenuItem("Qchange", "Q Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").SubMenu("Combo").AddItem(new MenuItem("Echange", "E Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").SubMenu("Combo").AddItem(new MenuItem("Rchange", "R Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("KillSteal", "KillSteal"));
            _config.SubMenu("HitChance").SubMenu("KillSteal").AddItem(new MenuItem("Qchangekil", "Q Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").SubMenu("KillSteal").AddItem(new MenuItem("Echangekil", "E Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").SubMenu("KillSteal").AddItem(new MenuItem("Rchangekil", "R Hit").SetValue(
                new StringList(new[] {"Low", "Medium", "High", "Very High"})));

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawharass", "Draw AutoHarass")).SetValue(true);

            _config.AddToMainMenu();
            Game.PrintChat("<font color='#881df2'>D-Corki by Diabaths</font> Loaded.");
            Game.PrintChat(
                "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.HasBuff("CorkiMissileBarrageCounterBig"))
                _r.Range = _r2.Range;
            else
                _r.Range = _r1.Range;

            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 _config.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();

            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                Laneclear();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("Lastmana").GetValue<Slider>().Value)
            {
                LastHit();
            }

            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            Usecleanse();
            KillSteal();
            Usepotion();
        }

        private static void Usecleanse()
        {
            if (_player.IsDead ||
                (_config.Item("Cleansemode").GetValue<StringList>().SelectedIndex == 1 &&
                 !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)) return;
            if (Cleanse(_player) && _config.Item("useqss").GetValue<bool>())
            {
                if (_player.HasBuff("zedulttargetmark"))
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140))
                        Utility.DelayAction.Add(500, () => Items.UseItem(3140));
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139))
                        Utility.DelayAction.Add(500, () => Items.UseItem(3139));
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137))
                        Utility.DelayAction.Add(500, () => Items.UseItem(3137));
                }
                else
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140)) Items.UseItem(3140);
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139)) Items.UseItem(3139);
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137)) Items.UseItem(3137);
                }
            }
        }

        private static bool Cleanse(Obj_AI_Hero hero)
        {
            bool cc = false;
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

        private static int UltiStucks()
        {
            return _player.Spellbook.GetSpell(SpellSlot.R).Ammo;
        }

        private static void GenModelPacket(string champ, int skinId)
        {
            Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(_player.NetworkId, skinId, champ))
                .Process();
        }


        private static void Combo()
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var useE = _config.Item("UseEC").GetValue<bool>();
            var useR = _config.Item("UseRC").GetValue<bool>();


            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_q.Range) && _q.GetPrediction(t).Hitchance >= Qchangecombo())
                    _q.Cast(t, false, true);
            }
            if (useW)
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    Fuckingw(t);
            }
            if (useE && _e.IsReady())
            {
                var t = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_e.Range) && _e.GetPrediction(t).Hitchance >= Echangecombo())
                    _e.Cast(t, false, true);

            }
            if (useR && _r.IsReady())
            {
                var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_r.Range) && _r.GetPrediction(t).Hitchance >= Rchangecombo())
                    _r.Cast(t, false, true);

            }
            UseItemes();
        }

        private static void Fuckingw(Obj_AI_Hero hero)
        {
            if (hero == null) return;
            var qmana = _player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            var wmana = _player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
            var emana = _player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
            var rmana = _player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
            var diveTower = _config.Item("diveintower").GetValue<bool>();
            if ((hero.UnderTurret() && !diveTower) || !_w.IsReady()) return;
            var usewhE = (100*(_player.Health/_player.MaxHealth)) > _config.Item("UseWHE").GetValue<Slider>().Value;
            if (usewhE && hero.IsValidTarget(_q.Range + _w.Range) &&
                hero.CountEnemiesInRange(_q.Range + _w.Range) <= _config.Item("EnemyC").GetValue<Slider>().Value &&
                _player.Distance(hero) > Orbwalking.GetRealAutoAttackRange(_player))
            {
                if (hero.Health < (_player.GetAutoAttackDamage(hero, true)*2))
                {
                    _w.Cast(hero.Position);
                }
                else if (hero.Health < _q.GetDamage(hero) && _player.MaxMana > qmana + wmana && _q.IsReady())
                {
                    _w.Cast(hero.Position);
                }
                else if (hero.Health < _e.GetDamage(hero) && _player.MaxMana > emana + wmana && _e.IsReady())
                {
                    _w.Cast(hero.Position);
                }
                else if (hero.Health < _e.GetDamage(hero) + _q.GetDamage(hero) && _player.MaxMana > qmana + emana + wmana &&
                         _e.IsReady() && _e.IsReady())
                {
                    _w.Cast(hero.Position);
                }
                else if (hero.Health < _q.GetDamage(hero) + _r.GetDamage(hero) && _q.IsReady() && _r.IsReady() &&
                         _player.MaxMana > qmana + wmana + rmana)
                {
                    _w.Cast(hero.Position);
                }
            }
        }


        private static void Harass()
        {
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useE = _config.Item("UseEH").GetValue<bool>();
            var useR = _config.Item("UseRH").GetValue<bool>();
            var rlimH = _config.Item("RlimH").GetValue<Slider>().Value;
            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_q.Range) && _q.GetPrediction(t).Hitchance >= Qchangehar())
                    _q.Cast(t, false, true);

            }
            if (useE && _e.IsReady())
            {
                var t = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_e.Range) && _e.GetPrediction(t).Hitchance >= Echangehar())
                    _e.Cast(t, false, true);

            }
            if (useR && _r.IsReady() && rlimH < UltiStucks())
            {
                var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_r.Range) && _r.GetPrediction(t).Hitchance >= Rchangehar())
                    _r.Cast(t, false, true);

            }
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var combo = _config.Item("ActiveCombo").GetValue<KeyBind>().Active;
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useE = _config.Item("UseEC").GetValue<bool>();
            var useR = _config.Item("UseRC").GetValue<bool>();
            if (combo && unit.IsMe && (target is Obj_AI_Hero))
            {
                {
                    if (useQ && _q.IsReady())
                    {
                        var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                        if (t.IsValidTarget(_q.Range) && _q.GetPrediction(t).Hitchance >= Qchangecombo())
                            _q.Cast(t, false, true);

                    }
                    if (useE && _e.IsReady())
                    {
                        var t = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                        if (t.IsValidTarget(_e.Range) && _e.GetPrediction(t).Hitchance >= Echangecombo())
                            _e.Cast(t, false, true);

                    }
                    if (useR && _r.IsReady())
                    {
                        var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                        if (t.IsValidTarget(_r.Range) && _r.GetPrediction(t).Hitchance >= Rchangecombo())

                            _r.Cast(t, false, true);
                    }
                }
            }
        }

        private static void Laneclear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
                MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
                MinionTypes.All);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All);
            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _r.Range, MinionTypes.All);
            var rangedMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _r.Range + _r.Width,
                MinionTypes.Ranged);
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useEl = _config.Item("UseEL").GetValue<bool>();
            var useRl = _config.Item("UseRL").GetValue<bool>();
            var rlimL = _config.Item("RlimL").GetValue<Slider>().Value;
            if (_q.IsReady() && useQl)
            {
                var fl1 = _q.GetCircularFarmLocation(rangedMinionsQ, _q.Width);
                var fl2 = _q.GetCircularFarmLocation(allMinionsQ, _q.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _q.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                {
                    _q.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }
            if (_e.IsReady() && useEl)
            {
                var fl2 = _w.GetLineFarmLocation(allMinionsE, _e.Width);

                if (fl2.MinionsHit >= 2 || allMinionsE.Count == 1)
                {
                    _e.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsE)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                            _e.Cast(minion);
            }
            if (_r.IsReady() && useRl && rlimL < UltiStucks() && allMinionsR.Count > 3)
            {
                var fl1 = _w.GetLineFarmLocation(rangedMinionsR, _r.Width);
                var fl2 = _w.GetLineFarmLocation(allMinionsR, _r.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _r.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsR.Count == 1)
                {
                    _r.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsR)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.R))
                            _r.Cast(minion);
            }
        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useE = _config.Item("UseELH").GetValue<bool>();
            if (allMinions.Count < 3) return;
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }

                if (_w.IsReady() && useE && minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                {
                    _e.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useE = _config.Item("UseEJ").GetValue<bool>();
            var useR = _config.Item("UseRJ").GetValue<bool>();
            var rlimJ = _config.Item("RlimJ").GetValue<Slider>().Value;
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && _q.IsReady())
                {
                    _q.Cast(mob);
                }
                if (_e.IsReady() && useE)
                {
                    _e.Cast(mob);
                }
                if (_r.IsReady() && useR && rlimJ < UltiStucks())
                {
                    _r.Cast(mob);
                }
            }
        }

        /* private static bool Packets()
         {
             return _config.Item("usePackets").GetValue<bool>();
         }*/

        private static bool HasBigRocket()
        {
            return ObjectManager.Player.Buffs.Any(buff => buff.DisplayName.ToLower() == "corkimissilebarragecounterbig");
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if (_q.IsReady() && _config.Item("UseQM").GetValue<bool>())
                {
                    if (_q.GetDamage(hero) > hero.Health && hero.IsValidTarget(_q.Range) &&
                        _q.GetPrediction(hero).Hitchance >= Qchangekil())
                        _q.Cast(hero, false, true);

                }
                if (_e.IsReady() && _config.Item("UseEM").GetValue<bool>())
                {
                    if (_e.GetDamage(hero) > hero.Health && hero.IsValidTarget(_e.Range) &&
                        _e.GetPrediction(hero).Hitchance >= Echangekil())
                        _e.Cast(hero, false, true);

                }
                if (_r.IsReady() && _config.Item("UseRM").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                    var bigRocket = HasBigRocket();
                    if (hero.IsValidTarget(bigRocket ? _r2.Range : _r1.Range) &&
                        _r1.GetDamage(hero)*(bigRocket ? 1.5f : 1f) > hero.Health)
                        if (_r.GetPrediction(t).Hitchance >= Rchangekil())

                            _r.Cast(t, false, true);
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
                var iYoumuu = _config.Item("Youmuu").GetValue<bool>();

                if (hero.IsValidTarget(450) && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
                {
                    _bilge.Cast(hero);

                }
                if (hero.IsValidTarget(450) && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
                {
                    _blade.Cast(hero);

                }
                if (hero.IsValidTarget(450) && iYoumuu && _youmuu.IsReady())
                {
                    _youmuu.Cast();
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
                               (_player.MaxHealth*(_config.Item("usepotionhp").GetValue<Slider>().Value)/100);
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana <=
                               (_player.MaxMana*(_config.Item("usepotionmp").GetValue<Slider>().Value)/100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("ActiveLane").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
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
                if (iusepotionmp && iusemppotion &&
                    !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask") ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion") ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle") ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask")))
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

        private static HitChance Qchangecombo()
        {
            switch (_config.Item("Qchange").GetValue<StringList>().SelectedIndex)
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

        private static HitChance Echangecombo()
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

        private static HitChance Rchangecombo()
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

        private static HitChance Qchangehar()
        {
            switch (_config.Item("QchangeHar").GetValue<StringList>().SelectedIndex)
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
                    return HitChance.High;
            }
        }

        private static HitChance Echangehar()
        {
            switch (_config.Item("EchangeHar").GetValue<StringList>().SelectedIndex)
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
                    return HitChance.High;
            }
        }

        private static HitChance Rchangehar()
        {
            switch (_config.Item("RchangeHar").GetValue<StringList>().SelectedIndex)
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
                    return HitChance.High;
            }
        }

        private static HitChance Qchangekil()
        {
            switch (_config.Item("Qchangekil").GetValue<StringList>().SelectedIndex)
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
                    return HitChance.Low;
            }
        }

        private static HitChance Echangekil()
        {
            switch (_config.Item("Echangekil").GetValue<StringList>().SelectedIndex)
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
                    return HitChance.Low;
            }
        }

        private static HitChance Rchangekil()
        {
            switch (_config.Item("Rchangekil").GetValue<StringList>().SelectedIndex)
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
                    return HitChance.Low;
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