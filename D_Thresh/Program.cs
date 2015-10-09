using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace D_Thresh
{
    public static class Program
    {
        private const string ChampionName = "Thresh";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r;

        private static Menu _config;

        private static Int32 _lastSkin;

        private static Obj_AI_Hero _player;

        private static int _champSkin;

        private static bool _initialSkin = true;

        private static Items.Item _blade, _bilge, _youmuu;

        private static readonly List<string> Skins = new List<string>();

        private static int ticktock;

        private static SpellSlot _igniteSlot;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
             _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 1025);
            _w = new Spell(SpellSlot.W, 950);
            _e = new Spell(SpellSlot.E, 400);
            _r = new Spell(SpellSlot.R, 400);
            _q.SetSkillshot(0.5f, 50f, 1900, true, SkillshotType.SkillshotCircle);

            _bilge = new Items.Item(3144, 475f);
            _blade = new Items.Item(3153, 425f);
            _youmuu = new Items.Item(3142, 10);
            _igniteSlot = _player.GetSpellSlot("SummonerDot");
           
            //D-Thresh
            _config = new Menu("Thresherino", "Thresherino", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCF", "Follow Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCS", "W for Shield").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCE", "W for Engage").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "E to me").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R if Hit").SetValue(new Slider(2, 5, 0)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("Items", "Items"));
            _config.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu's")).SetValue(true);
            _config.SubMenu("Items").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            _config.SubMenu("Items")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("Items")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("Items").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            _config.SubMenu("Items")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("Items")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));


            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "W for SafeFriend").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "E away").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Harass").AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(65, 1, 100)));
            _config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            /* //Farm
             _config.AddSubMenu(new Menu("Farm", "Farm"));
             _config.SubMenu("Farm").AddSubMenu(new Menu("Lasthit", "Lasthit"));
             _config.SubMenu("Farm").SubMenu("Lasthit").AddItem(new MenuItem("UseQLH", "Q LastHit")).SetValue(true);
             _config.SubMenu("Farm")
                 .SubMenu("Lasthit")
                 .AddItem(new MenuItem("Lastmana", "Minimum Mana").SetValue(new Slider(65, 1, 100)));
             _config.SubMenu("Farm")
                 .SubMenu("Lasthit")
                 .AddItem(
                     new MenuItem("ActiveLast", "LastHit").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
             _config.SubMenu("Farm").AddSubMenu(new Menu("Laneclear", "Laneclear"));
             _config.SubMenu("Farm").SubMenu("Laneclear").AddItem(new MenuItem("UseQL", "Q LaneClear")).SetValue(true);
             _config.SubMenu("Farm").SubMenu("Laneclear").AddItem(new MenuItem("UseWL", "W LaneClear")).SetValue(true);
             _config.SubMenu("Farm")
                 .SubMenu("Laneclear")
                 .AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(65, 1, 100)));
             _config.SubMenu("Farm")
                 .SubMenu("Laneclear")
                 .AddItem(
                     new MenuItem("ActiveLane", "Lane Clear").SetValue(new KeyBind("V".ToCharArray()[0],
                         KeyBindType.Press)));
             _config.SubMenu("Farm").AddSubMenu(new Menu("Jungleclear", "Jungleclear"));
             _config.SubMenu("Farm").SubMenu("Jungleclear").AddItem(new MenuItem("UseQJ", "Q Jungle")).SetValue(true);
             _config.SubMenu("Farm").SubMenu("Jungleclear").AddItem(new MenuItem("UseWJ", "W Jungle")).SetValue(true);
             _config.SubMenu("Farm")
                 .SubMenu("Jungleclear")
                 .AddItem(new MenuItem("junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
             _config.SubMenu("Farm")
                 .SubMenu("Jungleclear")
                 .AddItem(
                     new MenuItem("Activejungle", "Jungle Clear").SetValue(new KeyBind("V".ToCharArray()[0],
                         KeyBindType.Press)));
             */
            //HitChance
            _config.AddSubMenu(new Menu("HitChance", "HitChance"));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("HitChance").SubMenu("Harass").AddItem(new MenuItem("QchangeHar", "Q Hit").SetValue(
                new StringList(new[] { "Low", "Medium", "High", "Very High" })));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("HitChance").SubMenu("Combo").AddItem(new MenuItem("Qchange", "Q Hit").SetValue(
                new StringList(new[] { "Low", "Medium", "High", "Very High" })));

            //Auto Q
            _config.AddSubMenu(new Menu("Auto-Q", "Auto-Q"));
            _config.SubMenu("Auto-Q").AddItem(new MenuItem("QMin", "Use Auto Q").SetValue(false));
            _config.SubMenu("Auto-Q")
                .AddItem(new MenuItem("minAutoQMT", "Min target to Hit").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Auto-Q").AddItem(new MenuItem("automana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Usepackes")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseIgnite", "Use KillSteal Ignite").SetValue(false));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQM", "Use KillSteal Q").SetValue(false));
            

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
           
            _config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.PrintChat("<font color='#881df2'>D-Thresh By Diabaths</font> Loaded.");
            Game.PrintChat(
               "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
           if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 _config.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();

            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                Laneclear();
            }
            if (_config.Item("Activejungle").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("junglemana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lastmana").GetValue<Slider>().Value)
            {
                LastHit();
            }
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            KillSteal();

        }


        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

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
                    return HitChance.High;
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
                    return HitChance.VeryHigh;
            }
        }

        private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }

      
        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var combo = _config.Item("ActiveCombo").GetValue<KeyBind>().Active;
            if (combo && unit.IsMe && (target is Obj_AI_Hero))
            {
                if (useW && _w.IsReady())
                {
                    var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                    if (_player.Distance(t) < _w.Range)
                    {
                        _w.Cast();
                    }
                }
                if (useQ && _q.IsReady())
                {
                    var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                    var prediction = _q.GetPrediction(t);
                    if (_player.Distance(t) < _q.Range && prediction.Hitchance >= HitChance.High)
                        _q.Cast(prediction.CastPosition, Packets());
                }
            }
        }

        private static void UseItemes(Obj_AI_Hero target)
        {
            var iBilge = _config.Item("Bilge").GetValue<bool>();
            var iBilgeEnemyhp = target.Health <=
                                (target.MaxHealth * (_config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBilgemyhp = _player.Health <=
                             (_player.MaxHealth * (_config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
            var iBlade = _config.Item("Blade").GetValue<bool>();
            var iBladeEnemyhp = target.Health <=
                                (target.MaxHealth * (_config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBlademyhp = _player.Health <=
                             (_player.MaxHealth * (_config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
            var iZhonyas = _config.Item("Zhonyas").GetValue<bool>();
            var iZhonyashp = _player.Health <=
                             (_player.MaxHealth * (_config.Item("Zhonyashp").GetValue<Slider>().Value) / 100);
            var iYoumuu = _config.Item("Youmuu").GetValue<bool>();

            if (_player.Distance(target) <= 450 && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
            {
                _bilge.Cast(target);

            }
            if (_player.Distance(target) <= 450 && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
            {
                _blade.Cast(target);

            }
            if (_player.Distance(target) <= 450 && iYoumuu && _youmuu.IsReady())
            {
                _youmuu.Cast();
            }
        }

        private static void Combo()
        {
            var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
            var predic = _config.Item("Usepred").GetValue<bool>();
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();

            if (useW && _w.IsReady() && qtarget.Distance(_player.Position) < _w.Range)
            {
                _w.Cast();
            }
            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                var prediction = _q.GetPrediction(t);
                if (t != null && _player.Distance(t) < _q.Range && prediction.Hitchance >= Qchangecombo())
                {

                    _q.Cast(prediction.CastPosition, Packets());
                }
            }

            UseItemes(qtarget);
        }


        private static void Harass()
        {
            var eTarget = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Physical);
            var predic = _config.Item("Usepredh").GetValue<bool>();
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useW = _config.Item("UseWH").GetValue<bool>();
            if (useW && _w.IsReady() && eTarget.Distance(_player.Position) < _w.Range)
            {
                _w.Cast();
            }
            if (useQ && _q.IsReady())
            {

                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                var prediction = _q.GetPrediction(t);
                if (t != null && _player.Distance(t) < _q.Range && prediction.Hitchance >= Qchangecombo())
                {
                    _q.Cast(prediction.CastPosition, Packets());
                }

            }
        }

        private static void Laneclear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useWl = _config.Item("UseWL").GetValue<bool>();
            if (_q.IsReady() && useQl)
            {
                var fl2 = _q.GetLineFarmLocation(allMinionsQ, _q.Width);
                if (fl2.MinionsHit >= 3)
                {
                    _q.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }
            if (_w.IsReady() && useWl)
            {
                if (allMinionsQ.Count > 2)
                {
                    _w.Cast();
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.W))
                            _w.Cast();
            }
        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && _player.Distance(minion) < _q.Range &&
                    minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useW = _config.Item("UseWJ").GetValue<bool>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (_w.IsReady() && useW && _player.Distance(mob) < _q.Range)
                {
                    _w.Cast();
                }
                if (useQ && _q.IsReady() && _player.Distance(mob) < _q.Range)
                {
                    _q.Cast(mob);
                }
            }
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);

                if (hero.IsValidTarget(600) && _config.Item("UseIgnite").GetValue<bool>() &&
                    _igniteSlot != SpellSlot.Unknown &&
                    _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > hero.Health)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                }
                if (_q.IsReady() && _config.Item("UseQM").GetValue<bool>())
                {
                    var predQ = _e.GetPrediction(hero);
                    if (predQ.Hitchance >= HitChance.High && predQ.CollisionObjects.Count == 0)
                        if (_q.GetDamage(hero) > hero.Health && hero.IsValidTarget(_q.Range) && _q.IsReady())
                        {
                            _q.Cast(hero, Packets());
                        }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
          
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, Color.GreenYellow);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, Color.GreenYellow);
                }

            }
        }
    }
