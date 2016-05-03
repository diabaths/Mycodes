using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_Nidalee
{
    internal class Program
    {
        private const string ChampionName = "Nidalee";

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R, QC, WC, EC;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis, _zhonya, _archangel;

        private static SpellSlot IgniteSlot;

        private static Menu Config;

        private static Obj_AI_Hero Player;

        private static bool IsHuman;

        private static bool IsCougar;

        private static readonly float[] HumanQcd = {6, 6, 6, 6, 6};

        private static readonly float[] HumanWcd = {12, 12, 12, 12, 12};

        private static readonly float[] HumanEcd = {13, 12, 11, 10, 9};

        private static readonly float[] CougarQcd = { 5, 5, 5, 5, 5 };

        private static readonly float[] CougarWcd = { 5, 5, 5, 5, 5 };

        private static readonly float[] CougarEcd = {5, 5, 5, 5, 5};

        private static float _humQcd, _humWcd, _humEcd;

        private static float _spidQcd, _spidWcd, _spidEcd;

        private static float _humaQcd, _humaWcd, _humaEcd;

        private static float _spideQcd, _spideWcd, _spideEcd;

        private static SpellSlot _smiteSlot;

        private static Spell _smite;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (ObjectManager.Player.ChampionName != ChampionName) return;



            Q = new Spell(SpellSlot.Q, 1500f);
            W = new Spell(SpellSlot.W, 875f);
            E = new Spell(SpellSlot.E, 600f);
            QC = new Spell(SpellSlot.Q, 400f);
            WC = new Spell(SpellSlot.W, 375f);
            EC = new Spell(SpellSlot.E, 300f);
            R = new Spell(SpellSlot.R, 0);

            Q.SetSkillshot(0.25f, 40f, 1300, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.500f, 90f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            WC.SetSkillshot(0.50f, 400f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            EC.SetSkillshot(0.50f, (float)(15 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            SpellList.Add(QC);
            SpellList.Add(WC);
            SpellList.Add(EC);

            _archangel = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline
                         || Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar
                             ? new Items.Item(3048, float.MaxValue)
                             : new Items.Item(3040, float.MaxValue);

            if (Player.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner1, 570f);
                _smiteSlot = SpellSlot.Summoner1;
            }
            else if (Player.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner2, 570f);
                _smiteSlot = SpellSlot.Summoner2;
            }

            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);
            _zhonya = new Items.Item(3157, float.MaxValue);


            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            //D Nidalee
            Config = new Menu("D-Nidalee", "D-Nidalee", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItemsignite", "Use Ignite")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQComboCougar", "Use Q Cougar")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWComboCougar", "Use W Cougar")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEComboCougar", "Use E Cougar")).SetValue(true);
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("QHitCombo", "Q HitChange").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" })));

            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Extra
            Config.AddSubMenu(new Menu("Heal", "Heal"));
            Config.SubMenu("Heal").AddItem(new MenuItem("MPPercent", "Mana percent")).SetValue(new Slider(40, 1, 100));
            Config.SubMenu("Heal").AddItem(new MenuItem("AutoSwitchform", "Auto Switch Forms")).SetValue(true);
            Config.SubMenu("Heal").AddItem(new MenuItem("UseAutoE", "Use Heal(E)")).SetValue(true);
            Config.SubMenu("Heal").AddItem(new MenuItem("HPercent", "Health percent")).SetValue(new Slider(40, 1, 100));
            Config.SubMenu("Heal").AddItem(new MenuItem("AllyUseAutoE", "Ally Use Heal(E)")).SetValue(true);
            Config.SubMenu("Heal")
                .AddItem(new MenuItem("AllyHPercent", " Ally Health percent"))
                .SetValue(new Slider(40, 1, 100));

            Config.AddSubMenu(new Menu("items", "items"));
            Config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "Use Iron Solari"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Righteous", "Use Righteous Glory"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Righteousenemys", "Righteous Glory if  Enemy >=").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(
                    new MenuItem("Righteousenemysrange", "Righteous Glory Range Check").SetValue(
                        new Slider(800, 400, 1400)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Zhonyas", "Use Zhonya's"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Zhonyashp", "Use Zhonya's if HP%<").SetValue(new Slider(20, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Archangel", "Seraph's Embrace"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Archangelmyhp", "If My HP% <").SetValue(new Slider(85, 1, 100)));

            //potions
            Config.SubMenu("items").AddSubMenu(new Menu("Potions", "Potions"));
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "Use Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "If Mana % <").SetValue(new Slider(35, 1, 100)));

            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("QHitharass", "Q HitChange").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" })));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(
                        new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass key").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));


            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
            Config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Use Q (Human)")).SetValue(true);
            Config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(new MenuItem("lastmana", "Minimum Mana% >").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "LastHit!").SetValue(
                        new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            Config.SubMenu("Farm").AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseQLane", "Use Q (Human)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseWLane", "Use W (Human)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("farm_E1", "Use E (Human)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseQCLane", "Use Q (Cougar)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseWCLane", "Use W (Cougar)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseECLane", "Use E (Cougar)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("LaneClear", "Clear key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("farm_R", "Auto Switch Forms(toggle)").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("Lane", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //jungle
            Config.SubMenu("Farm").AddSubMenu(new Menu("Jungle", "Jungle"));
            Config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseQJungle", "Use Human Q")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseWJungle", "Use Human W")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseQCJungle", "Use Cougar Q")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseWCJungle", "Use Cougar W")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseECJungle", "Use Cougar E")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Switchungle", "Switch Forms")).SetValue(true);
            Config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(new MenuItem("junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            Config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(
                    new MenuItem("ActiveJungle", "Jungle key").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            //Smite 
            Config.AddSubMenu(new Menu("Smite", "Smite"));
            Config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(
                        new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("Smite").AddItem(new MenuItem("Useblue", "Smite Blue Early ")).SetValue(true);
            Config.SubMenu("Smite")
                .AddItem(new MenuItem("manaJ", "Smite Blue Early if MP% <").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            Config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            //Kill Steal
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            Config.SubMenu("Misc").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            Config.SubMenu("Misc").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);
            Config.SubMenu("Misc")
                .AddItem(new MenuItem("escapeterino", "Escape!!!"))
                .SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press));

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };


            //Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q(human)")).SetValue(false);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W(human)")).SetValue(false);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E(human)")).SetValue(false);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawWC", "Draw W(Cougar)")).SetValue(false);
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "Draw smite")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("Drawharass", "Draw Auto Harass")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawCooldown", "Draw Cooldown")).SetValue(false);

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += OnCreateObj;
            GameObject.OnDelete += OnDeleteObj;
            //Game_OnGameEnd += Game_OnGameEnd;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.PrintChat("<font color='#881df2'>SKO Nidallee Reworked By Diabaths </font>Loaded!");
            Game.PrintChat(
                "<font color='#f2f21d'>Do you like it???  </font> <font color='#ff1900'>Drop 1 Upvote in Database </font>");
            Game.PrintChat(
                "<font color='#f2f21d'>Buy me cigars </font> <font color='#ff1900'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget(Q.Range)) WC.Range = target.HasBuff("nidaleepassivehunted") ? 730 : 375;
            if (Config.Item("UseAutoE").GetValue<bool>())
            {
                AutoE();
            }

            if (Config.Item("escapeterino").GetValue<KeyBind>().Active)
            {
                Escapeterino();
            }

            if (Config.Item("AllyUseAutoE").GetValue<bool>())
            {
                AllyAutoE();
            }

            Cooldowns();

            Player = ObjectManager.Player;
            QC = new Spell(SpellSlot.Q, Player.AttackRange + 50);
            Orbwalker.SetAttack(true);

            CheckSpells();
            if (Config.Item("ActiveLast").GetValue<KeyBind>().Active &&
                (100*(Player.Mana/Player.MaxMana)) > Config.Item("lastmana").GetValue<Slider>().Value)
            {
                LastHit();
            }

            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if ((Config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 Config.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                (100*(Player.Mana/Player.MaxMana)) > Config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();
            }

            if (Config.Item("LaneClear").GetValue<KeyBind>().Active)
            {
                Farm();
            }

            if (Config.Item("ActiveJungle").GetValue<KeyBind>().Active)
            {
                Jungleclear();
            }

            if (Config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }

            if (Config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }

            Usepotion();
        }


        private static void Escapeterino()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (IsHuman)
            {
                if (R.IsReady())
                {
                    R.Cast();
                }
            }

            if (IsCougar && WC.IsReady())
                WC.Cast(Game.CursorPos);
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }
            
            if (spell.Name.ToLower().Contains("takedown"))
            {
               // Game.PrintChat("reset");
                Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
            }

            /* if (sender.IsMe)
             {
                  Game.PrintChat("Spell name: " + args.SData.Name.ToString());
             }*/
            if (sender.IsMe && Config.Item("DrawCooldown").GetValue<bool>())
            {
                //Game.PrintChat("Spell name: " + args.SData.Name.ToString());
                if (IsHuman)
                {
                    if (args.SData.Name == "JavelinToss") _humQcd = Game.Time + CalculateCd(HumanQcd[Q.Level-1]);
                    if (args.SData.Name == "Bushwhack") _humWcd = Game.Time + CalculateCd(HumanWcd[W.Level-1]);
                    if (args.SData.Name == "PrimalSurge") _humEcd = Game.Time + CalculateCd(HumanEcd[E.Level-1]);
                }
                else
                {
                    if (args.SData.Name == "Takedown") _spidQcd = Game.Time + CalculateCd(CougarQcd[QC.Level-1]);
                    if (args.SData.Name == "Pounce") _spidWcd = Game.Time + CalculateCd(CougarWcd[WC.Level-1]);
                    if (args.SData.Name == "Swipe") _spidEcd = Game.Time + CalculateCd(CougarEcd[EC.Level-1]);
                }
            }
        }
       
        private static float CalculateCd(float time)
        {
            return time + (time * Player.PercentCooldownMod);
        }

        private static void Cooldowns()
        {
            _humaQcd = ((_humQcd - Game.Time) > 0) ? (_humQcd - Game.Time) : 0;
            _humaWcd = ((_humWcd - Game.Time) > 0) ? (_humWcd - Game.Time) : 0;
            _humaEcd = ((_humEcd - Game.Time) > 0) ? (_humEcd - Game.Time) : 0;
            _spideQcd = ((_spidQcd - Game.Time) > 0) ? (_spidQcd - Game.Time) : 0;
            _spideWcd = ((_spidWcd - Game.Time) > 0) ? (_spidWcd - Game.Time) : 0;
            _spideEcd = ((_spidEcd - Game.Time) > 0) ? (_spidEcd - Game.Time) : 0;
        }
        

        private static HitChance QHitChanceCombo()
        {
            switch (Config.Item("QHitCombo").GetValue<StringList>().SelectedIndex)
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

        private static HitChance QHitChanceHarass()
        {
            switch (Config.Item("QHitharass").GetValue<StringList>().SelectedIndex)
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

        private static void Smiteontarget()
        {
            if (_smite == null) return;
            var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(570));
            var smiteDmg = Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Smite);
            var usesmite = Config.Item("smitecombo").GetValue<bool>();
            if (Player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker" && usesmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready)
            {
                if (hero != null && (!hero.HasBuffOfType(BuffType.Stun) || !hero.HasBuffOfType(BuffType.Slow)))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
                else if (hero != null && smiteDmg >= hero.Health)
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
            }

            if (Player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel" && usesmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                hero.IsValidTarget(570))
            {
                ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
            }
        }

        public static readonly string[] Smitetype =
        {
            "s5_summonersmiteplayerganker", "s5_summonersmiteduel", "s5_summonersmitequick", "itemsmiteaoe",
            "summonersmite"
        };

        private static int GetSmiteDmg()
        {
            int level = Player.Level;
            int index = Player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = Config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var useblue = Config.Item("Useblue").GetValue<bool>();
            var usered = Config.Item("Usered").GetValue<bool>();
            var health = (100 * (Player.Health / Player.MaxHealth)) < Config.Item("healthJ").GetValue<Slider>().Value;
            var mana = (100 * (Player.Mana / Player.MaxMana)) < Config.Item("manaJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            }
            else
            {
                jungleMinions = new string[]
                {
                    "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_RiftHerald", "SRU_Red", "SRU_Krug",
                    "SRU_Dragon",
                    "SRU_Baron"
                };
            }
            var minions = MinionManager.GetMinions(Player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
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


        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var itemsIgnite = Config.Item("UseItemsignite").GetValue<bool>();
            if (target == null) return;
            Smiteontarget();

            if (itemsIgnite && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (ComboDamage(target) > target.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (Q.IsReady() && IsHuman && target.IsValidTarget(Q.Range) &&
                Config.Item("UseQCombo").GetValue<bool>())
            {
                var predictionq = Q.GetPrediction(target);
                if (predictionq.Hitchance >= QHitChanceCombo() && predictionq.CollisionObjects.Count == 0)
                    Q.Cast(predictionq.CastPosition);
            }

            if (W.IsReady() && IsHuman && target.IsValidTarget(W.Range) &&
                Config.Item("UseWCombo").GetValue<bool>())
            {
                W.Cast(target);
            }

            if (R.IsReady() && IsHuman && Config.Item("UseRCombo").GetValue<bool>())
                if(Player.Distance(target) <= 325 || (target.HasBuff("nidaleepassivehunted") && target.IsValidTarget(WC.Range)))
            {
                if (IsHuman)
                {
                    R.Cast();
                }

                if (IsCougar)
                {
                    if (WC.IsReady() && Config.Item("UseWComboCougar").GetValue<bool>() &&
                        target.IsValidTarget(WC.Range))
                    {
                        WC.Cast(target.ServerPosition);
                    }

                    if (EC.IsReady() && Config.Item("UseEComboCougar").GetValue<bool>() &&
                        target.IsValidTarget(EC.Range))
                    {
                        EC.Cast(target.ServerPosition);
                    }

                    if (QC.IsReady() && Config.Item("UseQComboCougar").GetValue<bool>() &&
                        target.IsValidTarget(QC.Range))
                    {
                        Orbwalker.SetAttack(true);
                        QC.Cast();
                    }
                }
            }

            if (IsCougar && Player.Distance(target) < 700)
            {
                if (IsHuman && R.IsReady())
                {
                    R.Cast();
                }

                if (IsCougar)
                {
                    if (WC.IsReady() && Config.Item("UseWComboCougar").GetValue<bool>() &&
                        target.IsValidTarget(WC.Range))
                    {
                        WC.Cast(target.ServerPosition);
                    }

                    if (EC.IsReady() && Config.Item("UseEComboCougar").GetValue<bool>() &&
                       target.IsValidTarget(EC.Range))
                    {
                        EC.Cast(target.ServerPosition);
                    }

                    if (QC.IsReady() && Config.Item("UseQComboCougar").GetValue<bool>() &&
                        target.IsValidTarget(QC.Range))
                    {
                        Orbwalker.SetAttack(true);
                        QC.Cast();
                    }
                }
            }

            if (R.IsReady() && IsCougar && Config.Item("UseRCombo").GetValue<bool>() &&
                Player.Distance(target) > WC.Range)
            {
                R.Cast();
            }

            if (R.IsReady() && IsCougar && !QC.IsReady() && !WC.IsReady() && !EC.IsReady() && Q.IsReady()
                && Config.Item("UseRCombo").GetValue<bool>())
            {
                R.Cast();
            }

            UseItemes();
        }

        private static float ComboDamage(Obj_AI_Base hero)
        {
            var dmg = 0d;
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }

            if (Items.HasItem(3153) && Items.CanUseItem(3153)) dmg += Player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144)) dmg += Player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            if (IsCougar)
            {
                if (QC.IsReady()) dmg += Player.GetSpellDamage(hero, SpellSlot.Q);
                if (EC.IsReady()) dmg += Player.GetSpellDamage(hero, SpellSlot.E);
                if (WC.IsReady()) dmg += Player.GetSpellDamage(hero, SpellSlot.W);
            }

            if (Q.IsReady() && !IsCougar)
                dmg += Player.GetSpellDamage(hero, SpellSlot.Q);
            return (float)dmg;
        }



        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iBilge = Config.Item("Bilge").GetValue<bool>();
                var iBilgeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (Config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBilgemyhp = Player.Health <=
                                 (Player.MaxHealth * (Config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
                var iBlade = Config.Item("Blade").GetValue<bool>();
                var iBladeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (Config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBlademyhp = Player.Health <=
                                 (Player.MaxHealth * (Config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
                var iOmen = Config.Item("Omen").GetValue<bool>();
                var iOmenenemys = hero.CountEnemiesInRange(450) >= Config.Item("Omenenemys").GetValue<Slider>().Value;
                var iTiamat = Config.Item("Tiamat").GetValue<bool>();
                var iHydra = Config.Item("Hydra").GetValue<bool>();
                var iRighteous = Config.Item("Righteous").GetValue<bool>();
                var iRighteousenemys =
                    hero.CountEnemiesInRange(Config.Item("Righteousenemysrange").GetValue<Slider>().Value) >=
                    Config.Item("Righteousenemys").GetValue<Slider>().Value;
                var iZhonyas = Config.Item("Zhonyas").GetValue<bool>();
                var iZhonyashp = Player.Health <=
                                 (Player.MaxHealth * (Config.Item("Zhonyashp").GetValue<Slider>().Value) / 100);
                var iArchange = Config.Item("Archangel").GetValue<bool>();
                var iArchangelmyhp = Player.Health <=
                                     (Player.MaxHealth * (Config.Item("Archangelmyhp").GetValue<Slider>().Value) / 100);

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
                    Utility.DelayAction.Add(100, () => _rand.Cast());
                }

                if (iRighteousenemys && iRighteous && Items.HasItem(3800) && Items.CanUseItem(3800) &&
                    hero.IsValidTarget(Config.Item("Righteousenemysrange").GetValue<Slider>().Value))
                {
                    Items.UseItem(3800);
                }

                if (iZhonyas && iZhonyashp && hero.CountEnemiesInRange(1000) > 0)
                {
                    _zhonya.Cast();
                }

                if (iArchange && iArchangelmyhp && _archangel.IsReady() && Utility.CountEnemiesInRange(800) > 0)
                {
                    _archangel.Cast();
                }
            }

            var ilotis = Config.Item("lotis").GetValue<bool>();
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth * (Config.Item("lotisminhp").GetValue<Slider>().Value) / 100) &&
                        hero.Distance(Player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }
        }

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(
                Player.ServerPosition,
                Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            var iusehppotion = Config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = Player.Health
                               <= (Player.MaxHealth * (Config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = Config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = Player.Mana
                               <= (Player.MaxMana * (Config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (Player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0
                || (mobs.Count > 0 && Config.Item("ActiveJungle").GetValue<KeyBind>().Active && _smite != null))
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

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target != null)
            {
                if (Q.IsReady() && IsHuman && target.IsValidTarget(Q.Range) &&
                    Config.Item("UseQHarass").GetValue<bool>())
                {
                    var prediction = Q.GetPrediction(target);
                    if (prediction.Hitchance >= QHitChanceHarass() && prediction.CollisionObjects.Count == 0)
                        Q.Cast(prediction.CastPosition);
                }

                if (W.IsReady() && IsHuman && target.IsValidTarget(W.Range) &&
                    Config.Item("UseWHarass").GetValue<bool>())
                {
                    W.Cast(target);
                }
            }
        }


        private static void Farm()
        {
            var Humanq = Config.Item("UseQLane").GetValue<bool>();
            var Humanw = Config.Item("UseWLane").GetValue<bool>();
            var Cougarq = Config.Item("UseQCLane").GetValue<bool>();
            var Cougarw = Config.Item("UseWCLane").GetValue<bool>();
            var Cougare = Config.Item("UseECLane").GetValue<bool>();
            var lanemana = Player.Mana
                             >= (Player.MaxMana * (Config.Item("Lane").GetValue<Slider>().Value) / 100);
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 700, MinionTypes.All);
           
            if (allMinions.Count > 0)
            {
                var Minion = allMinions[0];
                if (IsCougar)
                {
                    if (QC.IsReady() && Cougarq && Player.Distance(Minion) < QC.Range)
                    {
                        QC.Cast();
                    }

                    if (EC.IsReady() && Cougare && Player.Distance(Minion) < EC.Range)
                    {
                        EC.Cast(Minion.ServerPosition);
                    }

                    foreach (var Minio in allMinions)
                    {
                        WC.Range = Minion.HasBuff("nidaleepassivehunted") ? 730 : 375;
                        if (WC.IsReady() && Cougarw && Player.Distance(Minion) > 200f)
                        {
                            WC.Cast(Minio.ServerPosition);
                        }
                    }

                    if (Config.Item("farm_R").GetValue<KeyBind>().Active && lanemana && !QC.IsReady() && !WC.IsReady() && !EC.IsReady() && R.IsReady())
                    {
                        R.Cast();
                    }
                }

                if (IsHuman)
                {
                    if (Humanq && lanemana && Q.IsReady())
                    {
                        var prediction = Q.GetPrediction(Minion);
                        if (prediction.Hitchance >= HitChance.Medium) Q.Cast(Minion.ServerPosition);
                    }

                    if (Humanw && lanemana && W.IsReady())
                    {
                        var prediction = W.GetPrediction(Minion);
                        if (prediction.Hitchance >= HitChance.Medium) W.Cast(Minion.ServerPosition);
                    }

                    if (Config.Item("farm_R").GetValue<KeyBind>().Active && !Q.IsReady() || !lanemana || !Humanq)
                    {
                        if (R.IsReady())
                        {
                            R.Cast();
                        }
                   }
                }
                
                if (E.IsReady() && IsHuman && !Config.Item("farm_R").GetValue<KeyBind>().Active
                         && Config.Item("farm_E1").GetValue<bool>()
                         && (100 * (Player.Mana / Player.MaxMana)) > Config.Item("Lane").GetValue<Slider>().Value)
                {
                    E.CastOnUnit(Player);
                }
            }
        }

        private static void Jungleclear()
        {
            var Humanq = Config.Item("UseQJungle").GetValue<bool>();
            var Humanw = Config.Item("UseWJungle").GetValue<bool>();
            var Cougarq = Config.Item("UseQCJungle").GetValue<bool>();
            var Cougarw = Config.Item("UseWCJungle").GetValue<bool>();
            var Cougare = Config.Item("UseECJungle").GetValue<bool>();
            var Switch = Config.Item("Switchungle").GetValue<bool>();
            var junglemana = Player.Mana
                             >= Player.MaxMana * Config.Item("junglemana").GetValue<Slider>().Value / 100;
            var mobs = MinionManager.GetMinions(
                Player.ServerPosition,
                700,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (IsHuman)
                {
                    if (Humanq && !mob.Name.Contains("Mini") && junglemana && Q.IsReady())
                    {
                        var prediction = Q.GetPrediction(mob);
                        if (prediction.Hitchance >= HitChance.Low) Q.Cast(mob.ServerPosition);
                    }

                    if (Humanw && junglemana && W.IsReady() && !mob.Name.Contains("Mini"))
                    {
                        var prediction = W.GetPrediction(mob);
                        if (prediction.Hitchance >= HitChance.Medium) W.Cast(mob.ServerPosition);
                    }

                    if (Switch && (!Q.IsReady() || !Humanq) && (!W.IsReady() || !Humanw) || !junglemana)
                    {
                        if (R.IsReady())
                        {
                            R.Cast();
                        }
                    }
                }

                if (IsCougar)
                {
                    if (Cougarq && mob.IsValidTarget(QC.Range) && QC.IsReady())
                    {
                        QC.Cast();
                    }

                    foreach (var Minion in mobs)
                    {
                        WC.Range = Minion.HasBuff("nidaleepassivehunted") ? 730 : 375;
                        if (Cougarw && Minion.IsValidTarget(WC.Range) && WC.IsReady())
                        {
                            WC.Cast(Minion.ServerPosition);
                        }
                    }

                    if (Cougare && mob.IsValidTarget(EC.Range) && EC.IsReady())
                    {
                        EC.Cast(mob.ServerPosition);
                    }

                    if (Switch && junglemana && !QC.IsReady() && !WC.IsReady() && !EC.IsReady() && R.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
        }

        private static void AutoE()
        {
            if (Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready && Player.IsMe)
            {
               if(Config.Item("escapeterino").GetValue<KeyBind>().Active) return;
                var forms = Config.Item("AutoSwitchform").GetValue<bool>();
                var health = Player.Health
                             <= Player.MaxHealth * Config.Item("HPercent").GetValue<Slider>().Value / 100;
                var mana = Player.Mana >= Player.MaxMana * Config.Item("MPPercent").GetValue<Slider>().Value / 100;
                if (Player.HasBuff("Recall") || Player.InFountain()) return;
                if (E.IsReady() && health)
                {
                    if (IsHuman && mana)
                    {
                        Player.Spellbook.CastSpell(SpellSlot.E, Player);
                    }

                    if (IsCougar && R.IsReady() && mana && forms)
                    {
                        R.Cast();
                        Player.Spellbook.CastSpell(SpellSlot.E, Player);
                    }
                }
            }
        }



        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
            var useQ = Config.Item("UseQLH").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (Q.IsReady() && IsHuman && useQ && minion.IsValidTarget(Q.Range) &&
                    minion.Health <= 0.95 * Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }
        }

        private static void AllyAutoE()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe && hero.IsValidTarget(E.Range)))
            {
                var forms = Config.Item("AutoSwitchform").GetValue<bool>();
                var mana = Player.Mana >= Player.MaxMana * Config.Item("MPPercent").GetValue<Slider>().Value / 100;
                if (Player.HasBuff("Recall") || hero.HasBuff("Recall") || hero.InFountain()) return;
                if (E.IsReady() &&
                    hero.Health / hero.MaxHealth * 100 <= Config.Item("AllyHPercent").GetValue<Slider>().Value &&
                    Utility.CountEnemiesInRange(1200) > 0 &&
                    hero.Distance(Player.ServerPosition) <= E.Range)
                {
                    if (IsHuman && mana)
                    {
                        E.Cast(hero);
                    }

                    if (IsCougar && R.IsReady() && mana && forms)
                    {
                        R.Cast();
                    }
                }
            }
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                var qhDmg = Player.GetSpellDamage(hero, SpellSlot.Q);

                if (hero.IsValidTarget(600) && Config.Item("UseIgnite").GetValue<bool>() &&
                    IgniteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > hero.Health)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, hero);
                    }
                }

                if (Q.IsReady() && hero.IsValidTarget(Q.Range) && IsHuman &&
                    Config.Item("UseQKs").GetValue<bool>())
                {
                    var predictionq = Q.GetPrediction(hero);
                    if (hero.Health <= qhDmg && predictionq.Hitchance >= HitChance.High &&
                        predictionq.CollisionObjects.Count == 0)
                    {
                        Q.Cast(hero);
                    }
                }
            }
        }

        private static void CheckSpells()
        {
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "JavelinToss" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "Bushwhack" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "PrimalSurge")
            {
                IsHuman = true;
                IsCougar = false;
            }
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "Takedown" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "Pounce" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "Swipe")
            {
                IsHuman = false;
                IsCougar = true;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var harass = Config.Item("harasstoggle").GetValue<KeyBind>().Active;
            var cat = Drawing.WorldToScreen(Player.Position);
            if (Config.Item("Drawharass").GetValue<bool>())
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

            if (Config.Item("Drawsmite").GetValue<bool>() && _smite != null)
            {
                if (Config.Item("Usesmite").GetValue<KeyBind>().Active)
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

                if (Player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker"
                    || Player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel")
                {
                    if (Config.Item("smitecombo").GetValue<bool>())
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

            if (IsHuman)
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? System.Drawing.Color.GreenYellow : System.Drawing.Color.OrangeRed);
                }

                if (Config.Item("DrawW").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.GreenYellow);
                }

                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.GreenYellow);
                }
            }

            if (IsCougar)
            {
                if (Config.Item("DrawWC").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, WC.Range, System.Drawing.Color.GreenYellow);
                }
            }

            if (Config.Item("DrawCooldown").GetValue<bool>())
            {
                if (!IsCougar)
                {
                    if (_spideQcd == 0) Drawing.DrawText(cat[0] - 60, cat[1], Color.White, "CQ Rdy");
                    else Drawing.DrawText(cat[0] - 60, cat[1], Color.Orange, "CQ: " + _spideQcd.ToString("0.0"));
                    if (_spideWcd == 0) Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.White, "CW Rdy");
                    else Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.Orange, "CW: " + _spideWcd.ToString("0.0"));
                    if (_spideEcd == 0) Drawing.DrawText(cat[0], cat[1], Color.White, "CE Rdy");
                    else Drawing.DrawText(cat[0], cat[1], Color.Orange, "CE: " + _spideEcd.ToString("0.0"));
                }
                else
                {
                    if (_humaQcd == 0) Drawing.DrawText(cat[0] - 60, cat[1], Color.White, "HQ Rdy");
                    else Drawing.DrawText(cat[0] - 60, cat[1], Color.Orange, "HQ: " + _humaQcd.ToString("0.0"));
                    if (_humaWcd == 0) Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.White, "HW Rdy");
                    else Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.Orange, "HW: " + _humaWcd.ToString("0.0"));
                    if (_humaEcd == 0) Drawing.DrawText(cat[0], cat[1], Color.White, "HE Rdy");
                    else Drawing.DrawText(cat[0], cat[1], Color.Orange, "HE: " + _humaEcd.ToString("0.0"));
                }
            }
        }

        private static void OnCreateObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmitter)) return;
            var obj = (Obj_GeneralParticleEmitter)sender;
            if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
            {

            }
        }

        private static void OnDeleteObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmitter)) return;
            var obj = (Obj_GeneralParticleEmitter)sender;
            if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
            {

            }
        }
    }
}


