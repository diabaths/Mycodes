using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Sivir_is_Back
{
    public static class Program
    {
        private const string ChampionName = "Sivir";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r;

        private static Menu _config;

        private static Int32 _lastSkin;

        private static Obj_AI_Hero _player;

        private static string[] _useshied;

        private static int _champSkin;
        static List<Spells> SpellListt = new List<Spells>();

        private static bool _initialSkin = true;

        private static Items.Item _blade, _bilge, _youmuu;

        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static readonly List<string> Skins = new List<string>();

        public struct Spells
        {
            public string ChampionName;
            public string SpellName;
            public SpellSlot slot;
        }
        private static int ticktock;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 1250);
            _w = new Spell(SpellSlot.W, 593);
            _e = new Spell(SpellSlot.E, float.MaxValue);
            _r = new Spell(SpellSlot.R, 1000f);
            _q.SetSkillshot(0.25f, 85f, 1350f, false, SkillshotType.SkillshotLine);
            SpellListt.Add(new Spells { ChampionName = "akali", SpellName = "akalismokebomb", slot = SpellSlot.W });   //Akali W
            SpellListt.Add(new Spells { ChampionName = "shaco", SpellName = "deceive", slot = SpellSlot.Q }); //Shaco Q
            SpellListt.Add(new Spells { ChampionName = "khazix", SpellName = "khazixr", slot = SpellSlot.R }); //Khazix R
            SpellListt.Add(new Spells { ChampionName = "khazix", SpellName = "khazixrlong", slot = SpellSlot.R }); //Khazix R Evolved
            SpellListt.Add(new Spells { ChampionName = "talon", SpellName = "talonshadowassault", slot = SpellSlot.R }); //Talon R
            SpellListt.Add(new Spells { ChampionName = "monkeyking", SpellName = "monkeykingdecoy", slot = SpellSlot.W }); //Wukong W

            SpellListt.Add(new Spells { ChampionName = "Graves", SpellName = "GravesClusterShot", slot = SpellSlot.Q });

            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);
            _youmuu = new Items.Item(3142, 10);

            var enemy = from hero in ObjectManager.Get<Obj_AI_Hero>()
                        where hero.IsEnemy == true
                        select hero;
            //D-Sivir
            _config = new Menu("Sivir_is_Back", "Sivir_is_Back", true);

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
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W")).SetValue(true);
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
                .SetValue(new StringList(new string[2] { "Always", "In Combo" }));
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
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(65, 1, 100)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
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
            _config.SubMenu("Auto-Q").AddItem(new MenuItem("useQdash", "Auto Q dashing")).SetValue(true);
            _config.SubMenu("Auto-Q").AddItem(new MenuItem("useQimmo", "Auto Q Immobile")).SetValue(true);
            _config.SubMenu("Auto-Q").AddItem(new MenuItem("useQstun", "Auto Q Taunt/Fear/Charm/Snare")).SetValue(true);
            _config.SubMenu("Auto-Q").AddItem(new MenuItem("QMin", "Use Auto Q if hit x Enenys").SetValue(false));
            _config.SubMenu("Auto-Q")
                .AddItem(new MenuItem("minAutoQMT", "Min target to Hit").SetValue(new Slider(2, 1, 5)));

            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("skinsiv", "Use Custom Skin").SetValue(false));
            _config.SubMenu("Misc").AddItem(new MenuItem("skinsivir", "Skin Changer").SetValue(new Slider(4, 1, 7)));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQM", "Use KillSteal Q").SetValue(false));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEonSpell", "Use E on Spell").SetValue(true));

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Game.PrintChat("<font color='#881df2'>Sivir is Back  by Diabaths</font> Loaded.");
            if (_config.Item("skinsiv").GetValue<bool>())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinsivir").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinsivir").GetValue<Slider>().Value;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
            var qpred = _q.GetPrediction(target);
            var manacheck = _player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                            _player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
            if (target.IsValidTarget(_q.Range) && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                if (_player.Mana >= manacheck && qpred.Hitchance == HitChance.Immobile && _q.IsReady() &&
                    _config.Item("useQimmo").GetValue<bool>())
                {
                    _q.Cast(qpred.CastPosition);
                }
                if (_player.Mana >= manacheck && qpred.Hitchance == HitChance.Dashing && _q.IsReady() &&
                    _config.Item("useQdash").GetValue<bool>())
                {
                    _q.Cast(qpred.CastPosition);
                }
                if (_player.Mana >= manacheck && qpred.Hitchance == HitChance.High && _q.IsReady() &&
                    _config.Item("useQstun").GetValue<bool>() &&
                    (target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Charm) ||
                     target.HasBuffOfType(BuffType.Fear) ||
                     target.HasBuffOfType(BuffType.Taunt)))
                {
                    _q.Cast(qpred.CastPosition);
                }
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_q.Range)))
                {
                    var fuckq = _q.GetPrediction(hero, true);
                    if (_config.Item("QMin").GetValue<bool>() && _player.Mana >= manacheck &&
                        fuckq.AoeTargetsHitCount >= _config.Item("minAutoQMT").GetValue<Slider>().Value &&
                        _q.GetPrediction(hero).Hitchance >= HitChance.High)
                    {
                        _q.Cast(hero, false, true);
                    }
                }
            }
            if (_config.Item("skinsiv").GetValue<bool>() && SkinChanged())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinsivir").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinsivir").GetValue<Slider>().Value;
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (!_config.Item("ActiveCombo").GetValue<KeyBind>().Active &&
                (_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
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
            Usecleanse();
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

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            Game.PrintChat("stage 1");
            if (!sender.IsEnemy || sender.IsDead || !(sender is Obj_AI_Hero))
                return;
            Game.PrintChat("stage 2");
            if (!_config.Item("UseEonSpell").GetValue<bool>()) return;
            string[] Spells =
            {
                "AhriSeduce"
                , "InfernalGuardian"
                , "EnchantedCrystalArrow"
                , "InfernalGuardian"
                , "EnchantedCrystalArrow"
                , "RocketGrab"
                , "BraumQ"
                , "CassiopeiaPetrifyingGaze"
                , "DariusAxeGrabCone"
                , "DravenDoubleShot"
                , "DravenRCast"
                , "EzrealTrueshotBarrage"
                , "FizzMarinerDoom"
                , "GnarBigW"
                , "GnarR"
                , "GragasR"
                , "GravesChargeShot"
                , "GravesClusterShot"
                , "JarvanIVDemacianStandard"
                , "JinxW"
                , "JinxR"
                , "KarmaQ"
                , "KogMawLivingArtillery"
                , "LeblancSlide"
                , "LeblancSoulShackle"
                , "LeonaSolarFlare"
                , "LuxLightBinding"
                , "LuxLightStrikeKugel"
                , "LuxMaliceCannon"
                , "UFSlash"
                , "DarkBindingMissile"
                , "NamiQ"
                , "NamiR"
                , "OrianaDetonateCommand"
                , "RengarE"
                , "rivenizunablade"
                , "RumbleCarpetBombM"
                , "SejuaniGlacialPrisonStart"
                , "SionR"
                , "ShenShadowDash"
                , "SonaR"
                , "ThreshQ"
                , "ThreshEFlay"
                , "VarusQMissilee"
                , "VarusR"
                , "VeigarBalefulStrike"
                , "VelkozQ"
                , "Vi-q"
                , "Laser"
                , "xeratharcanopulse2"
                , "XerathArcaneBarrage2"
                , "XerathMageSpear"
                , "xerathrmissilewrapper"
                , "yasuoq3w"
                , "ZacQ"
                , "ZedShuriken"
                , "ZiggsQ"
                , "ZiggsW"
                , "ZiggsE"
                , "ZiggsR"
                , "ZileanQ"
                , "ZyraQFissure"
                , "ZyraGraspingRoots"
            };
            for (int i = 0; i <= 61; i++)
            {
                if (args.SData.Name == Spells[i])
                {
                    Game.PrintChat("stage 3");
                    if (sender.IsEnemy && args.Target.IsMe && !Orbwalking.IsAutoAttack(args.SData.Name)&&
                        _e.IsReady())
                    {
                        Game.PrintChat("stage 4");
                        _e.Cast();
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


        private static void GenModelPacket(string champ, int skinId)
        {
            Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(_player.NetworkId, skinId, champ))
                .Process();
        }

        private static bool SkinChanged()
        {
            return (_config.Item("skinsivir").GetValue<Slider>().Value != _lastSkin);
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
                    if (t != null && prediction.Hitchance >= HitChance.High)
                        _q.Cast(prediction.CastPosition);
                }
            }
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
                               (_player.MaxHealth * (_config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana <=
                               (_player.MaxMana * (_config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("Activejungle").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
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
        private static void Combo()
        {
            var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            if (qtarget == null) return;
            if (useW && _w.IsReady() && qtarget.IsValidTarget(450))
            {
                _w.Cast();
            }
            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_q.Range) && _q.GetPrediction(t).Hitchance >= Qchangecombo())
                    _q.Cast(t, false, true);
            }
            UseItemes();
        }


        private static void Harass()
        {
            var eTarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useW = _config.Item("UseWH").GetValue<bool>();
            if (eTarget.IsValidTarget(450) && useW && _w.IsReady())
            {
                _w.Cast();
            }
            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_q.Range) && _q.GetPrediction(t).Hitchance >= Qchangehar())
                    _q.Cast(t, false, true);
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
                        {
                            _q.Cast(minion);
                        }
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
                if (_q.IsReady() || _config.Item("UseQM").GetValue<bool>())
                {
                    if (_q.GetDamage(hero) > hero.Health && hero.IsValidTarget(_q.Range))
                    {
                        _q.Cast(hero);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _r.Range, Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, Color.White);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _r.Range, Color.White);
                }

            }
        }
    }
}