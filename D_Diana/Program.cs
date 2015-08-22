using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace D_Diana
{
    internal class Program
    {
        private const string ChampionName = "Diana";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r;

        private static Obj_SpellMissile _qpos;

        private static bool _qcreated = false;

        public static Menu _config;

        public static Menu TargetSelectorMenu;

        private static Items.Item _dfg;

        private static Obj_AI_Hero _player;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static SpellSlot _igniteSlot;

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis;
        private static SpellSlot _smiteSlot = SpellSlot.Unknown;

        private static Spell _smite;

        private static readonly int[] DianaQwe = { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
        //Credits to Kurisu
        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 830f);
            _w = new Spell(SpellSlot.W, 200f);
            _e = new Spell(SpellSlot.E, 420f);
            _r = new Spell(SpellSlot.R, 825f);

            _q.SetSkillshot(0.35f, 200f, 1800, false, SkillshotType.SkillshotCircle);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _bilge = new Items.Item(3144, 475f);
            _blade = new Items.Item(3153, 425f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);
            _dfg = new Items.Item(3128, 750f);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");
            SetSmiteSlot();

            //D Diana
            _config = new Menu("D-Diana", "D-Diana", true);

            //TargetSelector
            TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            _config.AddSubMenu(TargetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseIgnitecombo", "Use Ignite(rush for it)")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRSecond", "Use Second R")).SetValue(false);
            _config.SubMenu("Combo").AddItem(new MenuItem("Normalcombo", "Q-R Combo")).SetValue(true);
            _config.Item("Normalcombo").ValueChanged += SwitchCombo;
            _config.SubMenu("Combo").AddItem(new MenuItem("Misayacombo", "R-Q Combo").SetValue(false));
            _config.Item("Misayacombo").ValueChanged += SwitchMisaya;
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            //_config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo2", "Combo2!").SetValue(new KeyBind(32, KeyBindType.Press)));


            //Items public static Int32 Tiamat = 3077, Hydra = 3074, Blade = 3153, Bilge = 3144, Rand = 3143, lotis = 3190;
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
            _config.SubMenu("items").SubMenu("Deffensive").AddSubMenu(new Menu("Cleanse", "Cleanse"));
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("useqss", "Use QSS/Mercurial Scimitar/Dervish Blade")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("blind", "Blind")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("charm", "Charm")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("fear", "Fear")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("flee", "Flee")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("snare", "Snare")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("taunt", "Taunt")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("suppression", "Suppression")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("stun", "Stun")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("polymorph", "Polymorph")).SetValue(false);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("silence", "Silence")).SetValue(false);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("zedultexecute", "Zed Ult")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").SubMenu("Cleanse").AddItem(new MenuItem("Cleansemode", "Use Cleanse")).SetValue(new StringList(new string[2] { "Always", "In Combo" }));
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

            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0],
                        KeyBindType.Press)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "Harass(toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Q LastHit")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseWLH", "W LaneClear")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(new MenuItem("lastmana", "Minimum Mana% >").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "LastHit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("Lane", "Lane"));
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseWLane", "Use W")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("ActiveLane", "Farm key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //jungle
            _config.SubMenu("Farm").AddSubMenu(new Menu("Jungle", "Jungle"));
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseQJungle", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseWJungle", "Use W")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(
                    new MenuItem("ActiveJungle", "Jungle key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Smite 
            _config.AddSubMenu(new Menu("Smite", "Smite"));
            _config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Useblue", "Smite Blue Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("manaJ", "Smite Blue Early if MP% <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            //Extra
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            //_config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Usepackes")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("AutoShield", "Auto W")).SetValue(true);
            // _config.SubMenu("Misc").AddItem(new MenuItem("Shieldper", "Self Health %")).SetValue(new Slider(40, 1, 100));
            _config.SubMenu("Misc")
                .AddItem(
                    new MenuItem("Escape", "Escape Key!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Misc").AddItem(new MenuItem("Inter_E", "Interrupter E")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Gap_W", "GapClosers W")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("dianaAutoLevel", "Auto Level")).SetValue(false);
            _config.SubMenu("Misc").AddItem(new MenuItem("dianaStyle", "Level Sequence").SetValue(
               new StringList(new[] { "Q-W-E" })));

            //Kill Steal
            _config.AddSubMenu(new Menu("KillSteal", "Ks"));
            _config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseRKs", "Use R")).SetValue(true);
            _config.SubMenu("Ks")
                .AddItem(new MenuItem("TargetRange", "R use if range >").SetValue(new Slider(400, 200, 600)));
            _config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);

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
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "Draw smite")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("ShowPassive", "Show Passive")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("combotext", "Show Selected Combo")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();

            new AssassinManager();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.PrintChat("<font color='#881df2'>Diana By Diabaths With Misaya Combo</font>Loaded!");
            Game.PrintChat(
                "<font color='#FF0000'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#FF9900'>ssssssssssmith@hotmail.com</font> (10) S");

            // Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            _config.Item("dianaAutoLevel").ValueChanged += LevelUpMode;
            if (_config.Item("dinaAutoLevel").GetValue<bool>())
            {
                new AutoLevel(Style());
            }
        }
        private static void SwitchCombo(object sender, OnValueChangeEventArgs e)
        {
            if (e.GetNewValue<bool>())
                _config.Item("Misayacombo").SetValue(false);
        }

        private static void SwitchMisaya(object sender, OnValueChangeEventArgs e)
        {
            if (e.GetNewValue<bool>())
                _config.Item("Normalcombo").SetValue(false);
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }
            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("lastmana").GetValue<Slider>().Value)
            {
                LastHit();
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                int assassinRange = TargetSelectorMenu.Item("AssassinSearchRange").GetValue<Slider>().Value;

                IEnumerable<Obj_AI_Hero> xEnemy = ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            TargetSelectorMenu.Item("Assassin" + enemy.ChampionName) != null &&
                            TargetSelectorMenu.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy) < assassinRange);

                Obj_AI_Hero[] objAiHeroes = xEnemy as Obj_AI_Hero[] ?? xEnemy.ToArray();

                if (objAiHeroes.Length > 2)
                {
                    Game.PrintChat(objAiHeroes[0].Distance(objAiHeroes[1]).ToString());
                }

                Obj_AI_Hero t = !objAiHeroes.Any()
                    ? TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical)
                    : objAiHeroes[0];
                if (_config.Item("Misayacombo").GetValue<bool>())
                {
                    Misaya(t);
                }
                else if (_config.Item("Normalcombo").GetValue<bool>())
                {
                    Combo(t);
                }
            }
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 _config.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                Farm();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            Usepotion();
            if (_config.Item("Escape").GetValue<KeyBind>().Active)
            {
                Tragic();
            }
            if (_config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
            Usecleanse();
            /* if (_config.Item("AutoShield").GetValue<bool>() && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                AutoW();
            }*/
        }
        private static int[] Style()
        {
            switch (_config.Item("dianaStyle").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return DianaQwe;
                default:
                    return null;
            }
        }
        private static void LevelUpMode(object sender, OnValueChangeEventArgs e)
        {
            AutoLevel.Enabled(e.GetNewValue<bool>());
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range) && _config.Item("Gap_W").GetValue<bool>())
            {
                _w.Cast();
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_e.IsReady() && unit.IsValidTarget(_e.Range) && _config.Item("Inter_E").GetValue<bool>())
                _e.Cast();
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                // Game.PrintChat("Spell name: " + args.SData.Name.ToString());
            }
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
                    if (Items.HasItem(3140) && Items.CanUseItem(3140)) Utility.DelayAction.Add(100, () => Items.UseItem(3140));
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139)) Utility.DelayAction.Add(100, () => Items.UseItem(3139));
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137)) Utility.DelayAction.Add(100, () => Items.UseItem(3137));
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

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var qmana = _player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            var rmana = _player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
            var useQ = _config.Item("UseQCombo").GetValue<bool>();
            var useR = _config.Item("UseRCombo").GetValue<bool>();
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active && unit.IsMe && (target is Obj_AI_Hero))
            {
                if (target.IsValidTarget(_q.Range) && useQ && useR)
                {
                    var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                    if (_config.Item("Misayacombo").GetValue<bool>() && _q.IsReady() && _r.IsReady())
                    {
                        if (_q.GetPrediction(t).Hitchance >= HitChance.High && _player.Mana > qmana + rmana)
                        {
                            _r.Cast(t);
                            _q.CastIfHitchanceEquals(t, HitChance.High);
                        }
                    }
                    else if (_config.Item("Normalcombo").GetValue<bool>())
                    {
                        if (_q.GetPrediction(t).Hitchance >= HitChance.High)
                        {
                            _q.CastIfHitchanceEquals(t, HitChance.High);
                        }
                        if ((_qcreated == true) || t.HasBuff("dianamoonlight", true))
                        {
                            _r.Cast(t);
                        }
                    }
                }
            }
        }


        private static void Smiteontarget()
        {
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

        private static void Misaya(Obj_AI_Hero t)
        {
            var target = t;
            var useQ = _config.Item("UseQCombo").GetValue<bool>();
            var useW = _config.Item("UseWCombo").GetValue<bool>();
            var useE = _config.Item("UseECombo").GetValue<bool>();
            var useR = _config.Item("UseRCombo").GetValue<bool>();
            var ignitecombo = _config.Item("UseIgnitecombo").GetValue<bool>();
            var qmana = _player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            var rmana = _player.Spellbook.GetSpell(SpellSlot.R).ManaCost;

            Smiteontarget();
            
            if (target.IsValidTarget(600) && _igniteSlot != SpellSlot.Unknown && ignitecombo &&
                  _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (target.Health <= ComboDamage(target))
                {
                    _player.Spellbook.CastSpell(_igniteSlot, target);
                }
            }

            if (target.IsValidTarget(_q.Range) && useQ && useR && _q.IsReady() && _r.IsReady())
            {
                if (_q.GetPrediction(target).Hitchance >= HitChance.High && _player.Mana > qmana + rmana)
                {
                    _r.Cast(target);
                    _q.CastIfHitchanceEquals(target, HitChance.High);

                }
            }
            if (target.IsValidTarget(_w.Range) && useW && _w.IsReady())
            {
                _w.Cast();
            }
            if (target.IsValidTarget(_e.Range) && !target.IsValidTarget(_w.Range) &&
                useE && _e.IsReady() && !_w.IsReady())
            {
                _e.Cast();
            }
            if (target.IsValidTarget(_r.Range) && _config.Item("UseRSecond").GetValue<bool>() && _r.IsReady() &&
                !_w.IsReady() && !_q.IsReady())
            {
                _r.Cast(target);
            }
            UseItemes();
        }

        private static void Combo(Obj_AI_Hero t)
        {
            var target = t;
            var ignitecombo = _config.Item("UseIgnitecombo").GetValue<bool>();
            Smiteontarget();
            
            if (_igniteSlot != SpellSlot.Unknown && ignitecombo && target.IsValidTarget(600) &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (target.Health <= ComboDamage(target))
                {
                    _player.Spellbook.CastSpell(_igniteSlot, target);
                }
            }
            if (target.IsValidTarget(_q.Range) && _config.Item("UseQCombo").GetValue<bool>() && _q.IsReady() &&
                _q.GetPrediction(target).Hitchance >= HitChance.High)
            {
                _q.CastIfHitchanceEquals(target, HitChance.High);
            }
            if (target.IsValidTarget(_r.Range) && _config.Item("UseRCombo").GetValue<bool>() && _r.IsReady() &&
                ((_qcreated == true)
                 || target.HasBuff("dianamoonlight", true)))
            {
                _r.Cast(target);
            }
            if (target.IsValidTarget(_w.Range) && _config.Item("UseWCombo").GetValue<bool>() && _w.IsReady() &&
                !_q.IsReady())
            {
                _w.Cast();
            }
            if (target.IsValidTarget(_e.Range) && _player.Distance(target) >= _w.Range &&
                _config.Item("UseECombo").GetValue<bool>() && _e.IsReady() && !_w.IsReady())
            {
                _e.Cast();
            }
            if (target.IsValidTarget(_r.Range) && _config.Item("UseRSecond").GetValue<bool>() && _r.IsReady() &&
                !_w.IsReady() && !_q.IsReady())
            {
                _r.Cast(target);
            }
            UseItemes();
        }


        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iBilge = _config.Item("Bilge").GetValue<bool>();
                var iBilgeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (_config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBilgemyhp = _player.Health <=
                                 (_player.MaxHealth * (_config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
                var iBlade = _config.Item("Blade").GetValue<bool>();
                var iBladeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (_config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBlademyhp = _player.Health <=
                                 (_player.MaxHealth * (_config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
                var iOmen = _config.Item("Omen").GetValue<bool>();
                var iOmenenemys = hero.CountEnemiesInRange(450) >= _config.Item("Omenenemys").GetValue<Slider>().Value;
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
            }
            var ilotis = _config.Item("lotis").GetValue<bool>();
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth * (_config.Item("lotisminhp").GetValue<Slider>().Value) / 100) &&
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
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana <=
                               (_player.MaxMana * (_config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("ActiveJungle").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
                 SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i)) || SmitePurple.Any(i => Items.HasItem(i)) ||
                  SmiteBlue.Any(i => Items.HasItem(i)) || SmiteGrey.Any(i => Items.HasItem(i))
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

        private static float ComboDamage(Obj_AI_Hero hero)
        {
            var dmg = 0d;

            if (_q.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.Q) * 2;
            if (_w.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.W);
            if (_r.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.R);
           
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            dmg += _player.GetAutoAttackDamage(hero, true) * 2;
            if (_player.HasBuff("dianaarcready"))
            {
                dmg += 15 + 5 * ObjectManager.Player.Level;
            }
            if (ObjectManager.Player.HasBuff("LichBane"))
            {
                dmg += _player.BaseAttackDamage * 0.75 + _player.FlatMagicDamageMod * 0.5;
            }
            return (float)dmg;
        }


        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);

            if (target.IsValidTarget(_q.Range) && _config.Item("UseQHarass").GetValue<bool>() && _q.IsReady())
            {
                _q.CastIfHitchanceEquals(target, HitChance.High);
            }
            if (target.IsValidTarget(200) && _config.Item("UseWHarass").GetValue<bool>() && _w.IsReady())
            {
                _w.Cast();
            }
        }



        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
                MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
                MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All);

            var useQ = _config.Item("UseQLane").GetValue<bool>();
            var useW = _config.Item("UseWLane").GetValue<bool>();
            if (_q.IsReady() && useQ)
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
                            minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }
            if (_w.IsReady() && useW && allMinionsW.Count > 2)
            {
                _w.Cast();
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
            int index = _player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = _config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var useblue = _config.Item("Useblue").GetValue<bool>();
            var usered = _config.Item("Usered").GetValue<bool>();
            var health = (100 * (_player.Mana / _player.MaxMana)) < _config.Item("healthJ").GetValue<Slider>().Value;
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
                    else if (jungle && useblue && mana && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue")) &&
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
        private static void Tragic()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
                MinionTypes.All);
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (_q.IsReady()) _q.Cast(Game.CursorPos);
            if (_r.IsReady())
            {
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    _r.CastOnUnit(mob);
                }
                else if (allMinionsQ.Count >= 1)
                {
                    _r.Cast(allMinionsQ[0]);
                }
            }
        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useW = _config.Item("UseWLH").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && _player.Distance(minion) < _q.Range &&
                    minion.Health < 0.95 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }

                if (_w.IsReady() && useW && _player.Distance(minion) < _w.Range &&
                    minion.Health < 0.95 * _player.GetSpellDamage(minion, SpellSlot.W))
                {
                    _w.Cast();
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useQ = _config.Item("UseQJungle").GetValue<bool>();
            var useW = _config.Item("UseWJungle").GetValue<bool>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && _q.IsReady() && _player.Distance(mob) < _q.Range)
                {
                    _q.Cast(mob);
                }
                if (_w.IsReady() && useW && _player.Distance(mob) < _w.Range)
                {
                    _w.Cast();
                }
            }
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                var qhDmg = _player.GetSpellDamage(hero, SpellSlot.Q);
                var rhDmg = _player.GetSpellDamage(hero, SpellSlot.R);
                var rRange = (_player.Distance(hero) >= _config.Item("TargetRange").GetValue<Slider>().Value);
                if (hero.IsValidTarget(600) && _config.Item("UseIgnite").GetValue<bool>() &&
                    _igniteSlot != SpellSlot.Unknown &&
                    _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
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

                if (_r.IsReady() && hero.IsValidTarget(_r.Range) && rRange && _config.Item("UseRKs").GetValue<bool>())
                {
                    if (hero.Health <= rhDmg)
                    {
                        _r.Cast(hero);
                    }
                }
            }
        }

        /* private static void AutoW()
        {
            if (_player.HasBuff("Recall") || Utility.InFountain()) return;
            if (_w.IsReady() &&
                _player.Health <= (_player.MaxHealth * (_config.Item("Shieldper").GetValue<Slider>().Value) / 100))
            {
                _w.Cast();
            }

        }*/

        /* private static bool Packets()
         {
             return _config.Item("usePackets").GetValue<bool>();
         }*/


        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as Obj_SpellMissile;
            if (missile != null)
            {
                var spell = missile;
                var unit = spell.SpellCaster.Name;
                var name = spell.SData.Name;
                var caster = spell.SpellCaster;

                if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
                {
                    _qpos = spell;
                    _qcreated = true;
                    return;
                }
                // credits 100% to brian0305
                if (missile.IsValid && _config.Item("AutoShield").GetValue<bool>() && _w.IsReady())
                {
                    if (caster.IsEnemy)
                    {
                        var shieldBuff = new Int32[] { 40, 55, 70, 85, 100 }[_w.Level - 1] +
                                         1.3 * _player.FlatMagicDamageMod;
                        if (spell.SData.Name.Contains("BasicAttack"))
                        {
                            if (spell.Target.IsMe && _player.Health <= caster.GetAutoAttackDamage(_player, true) &&
                                _player.Health + shieldBuff > caster.GetAutoAttackDamage(_player, true) &&
                                caster.IsValidTarget(_w.Range) && _w.IsReady()) _w.Cast();
                        }
                        else if (spell.Target.IsMe || spell.EndPosition.Distance(_player.Position) <= 130)
                        {
                            if (spell.SData.Name == "summonerdot")
                            {
                                if (_player.Health <=
                                    (caster as Obj_AI_Hero).GetSummonerSpellDamage(_player,
                                        Damage.SummonerSpell.Ignite) &&
                                    _player.Health + shieldBuff >
                                    (caster as Obj_AI_Hero).GetSummonerSpellDamage(_player,
                                        Damage.SummonerSpell.Ignite) && _w.IsReady())
                                    _w.Cast();
                            }
                            else if (_player.Health <=
                                     (caster as Obj_AI_Hero).GetSpellDamage(_player,
                                         (caster as Obj_AI_Hero).GetSpellSlot(spell.SData.Name), 1) &&
                                     _player.Health + shieldBuff >
                                     (caster as Obj_AI_Hero).GetSpellDamage(_player,
                                         (caster as Obj_AI_Hero).GetSpellSlot(spell.SData.Name), 1) && _w.IsReady())
                                _w.Cast();
                        }
                    }
                }
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {//if (sender is Obj_SpellMissile)
            var missile = sender as Obj_SpellMissile;
            if (missile == null) return;
            var spell = missile;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
            {
                _qpos = null;
                _qcreated = false;
                return;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var diana = Drawing.WorldToScreen(_player.Position);
            if (_config.Item("combotext").GetValue<bool>())
            {
                if (_config.Item("Misayacombo").GetValue<bool>())
                {
                    Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, System.Drawing.Color.DarkOrange,
                        "R-Q Combo On");
                }
                else if (_config.Item("Normalcombo").GetValue<bool>())
                {
                    Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, System.Drawing.Color.DarkOrange,
                        "Q-R Combo On");
                }
            }

            if (_config.Item("Drawsmite").GetValue<bool>())
            {
                if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
                {
                     Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.90f, System.Drawing.Color.DarkOrange,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.90f, System.Drawing.Color.DarkRed,
                        "Smite Jungle Off");
                if (_config.Item("smitecombo").GetValue<bool>())
                {
                    Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.88f, System.Drawing.Color.DarkOrange,
                        "Smite Target On");
                }
                else
                    Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.88f, System.Drawing.Color.DarkRed,
                        "Smite Target Off");
            }
            if (_qpos != null)
                Utility.DrawCircle(_qpos.Position, _qpos.BoundingRadius, System.Drawing.Color.Tomato, 5, 30, false);
            if (_config.Item("ShowPassive").GetValue<bool>())
            {
                if (_player.HasBuff("dianaarcready"))
                    Drawing.DrawText(diana[0] - 10, diana[1], Color.White, "P On");
                else
                    Drawing.DrawText(diana[0] - 10, diana[1], Color.Orange, "P Off");
            }
            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.DarkOrange,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.DarkOrange,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.DarkOrange,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.DarkOrange,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.White);
                }
            }
        }
    }
}


