using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using fastJSON;
using HarmonyLib;
using ItemManager;
using LocalizationManager;
using LocationManager;
using PieceManager;
using ServerSync;
using StatusEffectManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Player;
using static Skills;
using static TribeClasses.LevelsSystemTree;

namespace TribeClasses
{
    [BepInPlugin(ModGUID, ModName, ModVersion),
        BepInDependency("org.bepinex.plugins.groups", BepInDependency.DependencyFlags.SoftDependency),
        BepInDependency("org.bepinex.plugins.dualwield", BepInDependency.DependencyFlags.SoftDependency),
        BepInIncompatibility("peaceful_mode"),
        BepInDependency("org.bepinex.plugins.devmod", BepInDependency.DependencyFlags.SoftDependency),
        BepInDependency("com.Frogger.TribeClasses.DruidAddition", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        #region values
        internal const string Author = "Frogger", ModName = Author + "." + "TribeClasses", ModVersion = "0.0.8", ModGUID = "com." + ModName;
        private static readonly Harmony harmony = new(ModGUID);
        public static Plugin _self;
        public static bool isInputField = false;
        public static bool haveDualWieldInstaled = false;
        //public static bool haveDruidInstaled = false;


        public GameObject staff_heal_projectile;
        public GameObject staff_heal_aoe;
        public GameObject fx_heal_staff_explosion;
        public GameObject _JF_SFX_craftitem_altar;
        public AssetBundle assetBundle;
        #endregion
        #region ConfigSettings
        static readonly string ConfigFileName = $"{ModGUID}.cfg";
        DateTime LastConfigChange;
        private readonly string levelTreePath = Path.Combine(Paths.ConfigPath, $"{ModGUID}.LevelTree.json");
        private readonly string monstersSettingsPath = Path.Combine(Paths.ConfigPath, $"{ModGUID}.MonstersSettings.json");
        public static readonly ConfigSync configSync = new(ModName) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = _self.Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }
        private void SetCfgValue<T>(Action<T> setter, ConfigEntry<T> config)
        {
            setter(config.Value);
            config.SettingChanged += (_, _) => setter(config.Value);
        }
        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        #endregion

        private void Awake()
        {
            assetBundle = PrefabManager.RegisterAssetBundle("dungeonclases");
            _self = this;
            JSON.Parameters = new JSONParameters
            {
                UseExtensions = false,
                SerializeNullValues = false,
                DateTimeMilliseconds = false,
                UseUTCDateTime = true,
                UseOptimizedDatasetSchema = true,
                UseValuesOfEnums = true
            };

            if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.dualwield"))
            {
                haveDualWieldInstaled = true;
            }
            else
            {
                haveDualWieldInstaled = false;
            }

            #region Pieces
            #region ClassAltarMulti
            BuildPiece altarMulti = new(assetBundle, "_JF_Piece_ClassAltar_Multi");
            BuildPiece.ConfigurationEnabled = true;
            altarMulti.Name
                .Russian("Алтарь Древних Героев")
                .English("Altar of Ancient Heroes")
                .Czech("Oltář Starověkých Hrdinů");
            altarMulti.Description
                .Russian("Используется для получения классов.")
                .English("Used to get classes.")
                .Czech("Používá se k získání tříd.");
            altarMulti.Category.Add(BuildPieceCategory.Crafting);
            altarMulti.SpecialProperties.AdminOnly = true;
            MaterialReplacer.RegisterGameObjectForShaderSwap(altarMulti.Prefab, MaterialReplacer.ShaderType.UseUnityShader);
            #endregion
            #region ClassAltarBerserker
            BuildPiece altarBerserker = new(assetBundle, "_JF_Piece_ClassAltar_Berserker");
            BuildPiece.ConfigurationEnabled = true;
            altarBerserker.Name
                .Russian("Алтарь Берсерка")
                .English("Altar of the Berserker")
                .Czech("Oltář Berserk");
            altarBerserker.Description
                .Russian("Используется для получения класса берсерк.")
                .English("Used to get the berserk class.")
                .Czech("Používá se k získání třídy Berserk.");
            altarBerserker.Category.Add(BuildPieceCategory.Crafting);
            altarBerserker.SpecialProperties.AdminOnly = true;
            MaterialReplacer.RegisterGameObjectForShaderSwap(altarBerserker.Prefab, MaterialReplacer.ShaderType.UseUnityShader);
            #endregion
            #region ClassAltarDruid
            BuildPiece altarDruid = new(assetBundle, "_JF_Piece_ClassAltar_Druid");
            BuildPiece.ConfigurationEnabled = true;
            altarDruid.Name
                .Russian("Алтарь Друида")
                .English("Druid's Altar")
                .Czech("Druidův Oltář");
            altarDruid.Description
                .Russian("Используется для получения класса хранитель Друид.")
                .English("Used to get the Guardian Druid class.")
                .Czech("Používá se k získání třídy Guardian Druid.");
            altarDruid.Category.Add(BuildPieceCategory.Crafting);
            altarDruid.SpecialProperties.AdminOnly = true;
            MaterialReplacer.RegisterGameObjectForShaderSwap(altarDruid.Prefab, MaterialReplacer.ShaderType.UseUnityShader);
            #endregion
            #region ClassAltarGuardian
            BuildPiece altarGuardian = new(assetBundle, "_JF_Piece_ClassAltar_Guardian");
            BuildPiece.ConfigurationEnabled = true;
            altarGuardian.Name
                .Russian("Алтарь Хранителя")
                .English("The Guardian's Altar")
                .Czech("Oltář Strážce");
            altarGuardian.Description
                .Russian("Используется для получения класса хранитель.")
                .English("Used to get the guardian class.")
                .Czech("Používá se k získání třídy opatrovník.");
            altarGuardian.Category.Add(BuildPieceCategory.Crafting);
            altarGuardian.SpecialProperties.AdminOnly = true;
            MaterialReplacer.RegisterGameObjectForShaderSwap(altarGuardian.Prefab, MaterialReplacer.ShaderType.UseUnityShader);
            #endregion
            #region ClassAltarRanger
            BuildPiece altarRanger = new(assetBundle, "_JF_Piece_ClassAltar_Ranger");
            BuildPiece.ConfigurationEnabled = true;
            altarRanger.Name
                .Russian("Алтарь Рейнджера")
                .English("Ranger's Altar")
                .Czech("Oltář Strážce");
            altarRanger.Description
                .Russian("Used to get the ranger class.")
                .English("Используется для получения класса рейнджер.")
                .Czech("Používá se k získání třídy Ranger.");
            altarRanger.Category.Add(BuildPieceCategory.Crafting);
            altarRanger.SpecialProperties.AdminOnly = true;
            MaterialReplacer.RegisterGameObjectForShaderSwap(altarRanger.Prefab, MaterialReplacer.ShaderType.UseUnityShader);
            #endregion
            #endregion
            #region Items
            #region GuardianRune
            Item guardian = new(assetBundle, "_JF_GuardianRune");
            guardian.Configurable = Configurability.Full;
            guardian.Name.Russian("Руна хранителя");
            guardian.Name.English("Guardian rune");
            guardian.Name.Spanish("Runa del guardián");
            guardian.Name.German("Rune des Wächters");
            guardian.Name.Chinese("守护符文");
            guardian.Name.Ukrainian("Руна хранителя");
            guardian.Name.Czech("Runa strážce");
            guardian.Description.Russian("Руна хранителя, которая даёт вам класс хранителя.");
            guardian.Description.English("A guardian rune that gives you a guardian class.");
            guardian.Description.Spanish("La runa del guardián que te da una clase de guardián.");
            guardian.Description.German("Eine Wächterrune, die dir eine Wächterklasse gibt.");
            guardian.Description.Chinese("一个守护符文，给你一个守护类。");
            guardian.Description.Ukrainian("Руна хранителя, яка дає вам клас хранителя.");
            guardian.Description.Czech("Rune strážce, který vám dává třídu strážce.");
            guardian.Crafting.Add("_JF_Piece_ClassAltar_Multi", 1);
            guardian.Crafting.Add("_JF_Piece_ClassAltar_Guardian", 1);
            guardian.Crafting.Add("_JF_ClassAltar_Guardian", 1);
            guardian.RequiredItems.Add("Dandelion", 35);
            guardian.RequiredItems.Add("Mushroom", 35);
            guardian.RequiredItems.Add("_JF_RubyLargeСharged", 3);
            #endregion
            #region BerserkerRune
            Item berserker = new(assetBundle, "_JF_BerserkerRune");
            berserker.Configurable = Configurability.Full;
            berserker.Name.Russian("Руна берсерка");
            berserker.Name.English("Berserker Rune");
            berserker.Name.Spanish("Runa berserk");
            berserker.Name.German("Berserker-Rune");
            berserker.Name.Chinese("狂战士符文");
            berserker.Name.Ukrainian("Руна берсерка");
            berserker.Name.Czech("Runa berserka");
            berserker.Description.Russian("Руна берсерка, которая даёт вам класс берсерка.");
            berserker.Description.English("The Berserker rune that gives you the berserker class.");
            berserker.Description.Spanish("Runa Berserker que te da la clase Berserker.");
            berserker.Description.German("Eine Berserker-Rune, die dir eine Berserker-Klasse gibt.");
            berserker.Description.Chinese("狂战士符文，给你狂战士职业。");
            berserker.Description.Ukrainian("Руна берсерка, яка дає вам клас берсерка.");
            berserker.Description.Czech("Runa berserka, která vám dává třídu berserka.");
            berserker.Crafting.Add("_JF_Piece_ClassAltar_Multi", 1);
            berserker.Crafting.Add("_JF_Piece_ClassAltar_Berserker", 1);
            berserker.Crafting.Add("_JF_ClassAltar_Berserker", 1);
            berserker.RequiredItems.Add("_JF_RubyLargeСharged", 3);
            berserker.RequiredItems.Add("Chain", 15);
            berserker.RequiredItems.Add("Iron", 13);
            #endregion
            #region RangerRune
            Item ranger = new(assetBundle, "_JF_RangerRune");
            ranger.Configurable = Configurability.Full;
            ranger.Name.Russian("Руна Рейнджера");
            ranger.Name.English("Ranger Rune");
            ranger.Name.Spanish("Runa Ranger");
            ranger.Name.German("Rune des Rangers");
            ranger.Name.Chinese("游侠符文");
            ranger.Name.Ukrainian("Руна Рейнджера");
            ranger.Name.Czech("Rune Ranger");
            ranger.Description.Russian("Руна Рейнджера, которая даёт вам класс Рейнджера.");
            ranger.Description.English("Ranger Rune, which gives you a Ranger class.");
            ranger.Description.Spanish("Runa de Guardabosques que te da una clase de Guardabosques.");
            ranger.Description.German("Eine Ranger-Rune, die dir Ranger-Klasse gibt.");
            ranger.Description.Chinese("游侠符文，它给你一个游侠类。");
            ranger.Description.Ukrainian("Руна Рейнджера, яка дає вам клас Рейнджера.");
            ranger.Description.Czech("Rune Ranger, který vám dává třídu Ranger.");
            ranger.Crafting.Add("_JF_Piece_ClassAltar_Multi", 1);
            ranger.Crafting.Add("_JF_Piece_ClassAltar_Ranger", 1);
            ranger.Crafting.Add("_JF_ClassAltar_Ranger", 1);
            ranger.RequiredItems.Add("_JF_RubyLargeСharged", 3);
            ranger.RequiredItems.Add("WolfClaw", 5);
            ranger.RequiredItems.Add("Crystal", 5);
            #endregion

            #region DruidRune
            Item druid = new(assetBundle, "_JF_DruidRune");
            druid.Configurable = Configurability.Full;
            druid.Name.Russian("Руна друида");
            druid.Name.English("Druid Rune");
            druid.Name.Spanish("Runa druida");
            druid.Name.German("Druiden-Rune");
            druid.Name.Chinese("德鲁伊符文");
            druid.Name.Ukrainian("Руна друїда");
            druid.Name.Czech("Runa druida");
            druid.Description.Russian("Руна друида, которая даёт вам класс друида.");
            druid.Description.English("A druid rune that gives you a druid class.");
            druid.Description.Spanish("Runa druida que te da la clase druida.");
            druid.Description.German("Druiden-Rune, die dir eine Druidenklasse gibt.");
            druid.Description.Chinese("一个德鲁伊符文，给你一个德鲁伊类。");
            druid.Description.Ukrainian("Руна друїда, яка дає вам клас друїда.");
            druid.Description.Czech("Rune Druid, který vám dává druidovu třídu.");
            druid.Crafting.Add("_JF_Piece_ClassAltar_Multi", 1);
            druid.Crafting.Add("_JF_Piece_ClassAltar_Druid", 1);
            druid.Crafting.Add("_JF_ClassAltar_Druid", 1);
            druid.RequiredItems.Add("_JF_RubyLargeСharged", 3);
            druid.RequiredItems.Add("BlackCore", 3);
            druid.RequiredItems.Add("Flametal", 1);
            #endregion

            #region ResetRune
            Item reset = new(assetBundle, "_JF_ResetRune");
            reset.Configurable = Configurability.Full;
            reset.Name.Russian("Руна сброса");
            reset.Name.English("Reset Rune");
            reset.Name.Spanish("Runa del Reset");
            reset.Name.German("Reset-Rune");
            reset.Name.Chinese("重置符文");
            reset.Name.Czech("Rune reset");
            reset.Description.Russian("Сбрасывает класс");
            reset.Description.English("Resets the class");
            reset.Description.Spanish("Restablece la clase");
            reset.Description.German("Setzt die Klasse zurück");
            reset.Description.Chinese("重置类");
            reset.Description.Czech("Resetuje třídu");
            reset.Crafting.Add("_JF_Piece_ClassAltar_Multi", 1);
            reset.Crafting.Add("_JF_Piece_ClassAltar_Berserker", 1);
            reset.Crafting.Add("_JF_Piece_ClassAltar_Druid", 1);
            reset.Crafting.Add("_JF_Piece_ClassAltar_Ranger", 1);
            reset.Crafting.Add("_JF_Piece_ClassAltar_Guardian", 1);
            reset.Crafting.Add("_JF_ClassAltar_Berserker", 1);
            reset.Crafting.Add("_JF_ClassAltar_Druid", 1);
            reset.Crafting.Add("_JF_ClassAltar_Ranger", 1);
            reset.Crafting.Add("_JF_ClassAltar_Guardian", 1);
            reset.RequiredItems.Add("Coal", 3);
            reset.RequiredItems.Add("BoneFragments", 5);
            #endregion
            #region LargeRuby
            Item rubyLarge = new(assetBundle, "_JF_RubyLarge");
            rubyLarge.Configurable = Configurability.Full;
            rubyLarge.Name.Russian("Большой рубин");
            rubyLarge.Name.English("Large Ruby");
            rubyLarge.Name.Spanish("rubí grande");
            rubyLarge.Name.German("großer Rubin");
            rubyLarge.Name.Chinese("大红宝石");
            rubyLarge.Name.Czech("velký rubín");
            rubyLarge.Description.Russian("Красивый камень. Может продать его? И зачем было слушать глупого ворона?");
            rubyLarge.Description.English("A beautiful stone. Maybe sell it? And why listen to a stupid raven?");
            rubyLarge.Description.Spanish("Hermosa piedra. ¿Puedo venderlo? ¿Y por qué escuchar al tonto Cuervo?");
            rubyLarge.Description.German("Ein schöner Stein. Kann ich es verkaufen? Und warum sollte man einem dummen Raben zuhören?");
            rubyLarge.Description.Chinese("一块美丽的石头。 也许卖掉它？ 为什么要听一只愚蠢的乌鸦呢？");
            rubyLarge.Description.Czech("Krásný kámen. Může to prodat? A proč poslouchat hloupého havrana?");
            rubyLarge.Crafting.Add(ItemManager.CraftingTable.Workbench, 1);
            rubyLarge.RequiredItems.Add("Ruby", 15);
            #endregion
            #region LargeRubyСharged
            Item rubyLargeCharged = new(assetBundle, "_JF_RubyLargeСharged");
            rubyLargeCharged.Configurable = Configurability.Full;
            rubyLargeCharged.Name.Russian("Большой Заряженный рубин");
            rubyLargeCharged.Name.English("Large Charged Ruby");
            rubyLargeCharged.Name.Spanish("Gran rubí Cargado");
            rubyLargeCharged.Name.German("Großer geladener Rubin");
            rubyLargeCharged.Name.Chinese("一颗带电荷的大红宝石");
            rubyLargeCharged.Name.Czech("Velký nabitý rubín");
            rubyLargeCharged.Description.Russian("Энергия многих рубинов сосредоточена в нём. Когда вглядываетесь в него, вам мерещится странное нечто внутри. Вы без понятия, как вам удалось создать подобное и безопасно ли оно.");
            rubyLargeCharged.Description.English("The energy of many rubies is concentrated in it. When you look into it, you see a strange something inside. You have no idea how you managed to create such a thing and whether it is safe.");
            rubyLargeCharged.Description.Spanish("La energía de muchos rubíes se concentra en él. Cuando lo miras, ves algo extraño dentro. No tienes idea de cómo lograste crear algo así y si es seguro.");
            rubyLargeCharged.Description.German("Die Energie vieler Rubine ist in ihm konzentriert. Wenn man ihn ansieht, merkt man etwas Merkwürdiges im Inneren. Sie haben keine Ahnung, wie Sie es geschafft haben, so etwas zu erstellen und ob es sicher ist.");
            rubyLargeCharged.Description.Chinese("许多红宝石的能量集中在其中。 当你看着它，你看到一个奇怪的东西里面。 你不知道你是如何设法创造这样的东西，以及它是否安全。");
            rubyLargeCharged.Description.Czech("Energie mnoha rubínů je v něm soustředěna. Když se na něj podíváte, najdete uvnitř něco zvláštního. Netušíte, jak se vám podařilo něco takového vytvořit a zda je to bezpečné.");
            rubyLargeCharged.Crafting.Add("_JF_Piece_ClassAltar_Multi", 1);
            rubyLargeCharged.RequiredItems.Add("_JF_RubyLarge", 1);
            rubyLargeCharged.RequiredItems.Add("SurtlingCore", 1);
            #endregion
            #region StaffHeal
            Item healStaff = new(assetBundle, "_JF_StaffHeal");
            healStaff.Configurable = Configurability.Full;
            healStaff.Name.Russian("Посох жизненной силы");
            healStaff.Name.English("Staff of Life Force");
            healStaff.Crafting.Add("piece_magetable", 1);
            healStaff.RequiredItems.Add("YggdrasilWood", 25);
            healStaff.RequiredItems.Add("BlackCore", 1);
            healStaff.RequiredItems.Add("Eitr", 15);
            #endregion
            #region StaffBaff
            Item baffStaff = new(assetBundle, "_JF_StaffBuff");
            baffStaff.Configurable = Configurability.Full;
            baffStaff.Name.Russian("Посох усиления");
            baffStaff.Name.English("StaffBuff");
            baffStaff.Crafting.Add("piece_magetable", 1);
            baffStaff.RequiredItems.Add("YggdrasilWood", 25);
            baffStaff.RequiredItems.Add("BlackCore", 1);
            baffStaff.RequiredItems.Add("Eitr", 15);
            //MaterialReplacer.RegisterGameObjectForShaderSwap(baffStaff.Prefab, MaterialReplacer.ShaderType.CustomCreature);
            #endregion
            #region AncientsHorn
            Item ancientsHorn = new(assetBundle, "_JF_AncientsHorn");
            ancientsHorn.Configurable = Configurability.Full;
            ancientsHorn.Name
                .Russian("Рог Древних")
                .English("Horn of the Ancients")
                .Spanish("Cuerno De Los Antiguos")
                .German("Horn der Alten")
                .Chinese("古人的角")
                .Ukrainian("Ріг Древніх")
                .Czech("Roh Starověku");
            ancientsHorn.Description
                .Russian("О Воин! Воспой к богам семи миров, пусть даруют тебе и другам твоим толику великой силы своей.")
                .English("O Warrior! Sing to the gods of the seven worlds, may they grant you and your friends a little of their great power.")
                .Spanish("¡Guerrero! Canta a los dioses de los siete mundos, que te den a TI y a tus amigos un poco de su gran poder.")
                .German("Oh Krieger! Singe zu den Göttern der sieben Welten, damit sie dir und deinen Freunden einen Bruchteil ihrer großen Macht schenken.")
                .Chinese("战士啊！ 向七个世界的神歌唱，愿他们赐予你和你的朋友一点他们伟大的力量。")
                .Ukrainian("О Воїн! Заспівай до богів семи світів, нехай дарують тобі і другам твоїм дещицю великої сили своєї.")
                .Czech("Ó Válečníku! Opěvujte bohy sedmi světů, ať vám a vašim přátelům dávají velkou moc své.");
            ancientsHorn.Configurable = Configurability.Full;
            ancientsHorn.Crafting.Add("_JF_Piece_ClassAltar_Multi", 1);
            ancientsHorn.Crafting.Add("_JF_Piece_ClassAltar_Berserker", 1);
            ancientsHorn.Crafting.Add("_JF_Piece_ClassAltar_Druid", 1);
            ancientsHorn.Crafting.Add("_JF_Piece_ClassAltar_Ranger", 1);
            ancientsHorn.Crafting.Add("_JF_Piece_ClassAltar_Guardian", 1);
            ancientsHorn.Crafting.Add("_JF_ClassAltar_Berserker", 1);
            ancientsHorn.Crafting.Add("_JF_ClassAltar_Druid", 1);
            ancientsHorn.Crafting.Add("_JF_ClassAltar_Ranger", 1);
            ancientsHorn.Crafting.Add("_JF_ClassAltar_Guardian", 1);
            ancientsHorn.RequiredItems.Add("Iron", 3);
            ancientsHorn.RequiredItems.Add("FineWood", 15);
            ancientsHorn.RequiredItems.Add("_JF_RubyLargeСharged", 5);
            #endregion
            #endregion
            #region SE
            _ = new CustomSE(assetBundle, "_JF_SE_guardian_super");
            _ = new CustomSE(assetBundle, "_JF_SE_berserker_super");
            _ = new CustomSE(assetBundle, "_JF_SE_druid_super");
            _ = new CustomSE(assetBundle, "_JF_SE_ranger_super");
            CustomSE heal = new(assetBundle, "SE_JF_Staff_heal");
            heal.Name
                .English("A surge of energy")
                .Russian("Прилив сил");
            #endregion
            #region Locations
            _ = new LocationManager.Location(assetBundle, "_JF_ClassAltar_Guardian")
            {
                MapIcon = "GuardianRuneIcon.png",
                ShowMapIcon = ShowIcon.Explored,
                Biome = Heightmap.Biome.Meadows,
                SpawnDistance = new(200, 2000),
                SpawnAltitude = new(10, 100),
                MinimumDistanceFromGroup = 100,
                Count = 15,
                Unique = true
            };
            _ = new LocationManager.Location(assetBundle, "_JF_ClassAltar_Berserker")
            {
                MapIcon = "BerserkerRuneIcon.png",
                ShowMapIcon = ShowIcon.Explored,
                Biome = Heightmap.Biome.Meadows,
                SpawnDistance = new(200, 2000),
                SpawnAltitude = new(10, 100),
                MinimumDistanceFromGroup = 100,
                Count = 15,
                Unique = true
            };
            _ = new LocationManager.Location(assetBundle, "_JF_ClassAltar_Druid")
            {
                MapIcon = "DruidRuneIcon.png",
                ShowMapIcon = ShowIcon.Explored,
                Biome = Heightmap.Biome.Meadows,
                SpawnDistance = new(200, 2000),
                SpawnAltitude = new(10, 100),
                MinimumDistanceFromGroup = 100,
                Count = 15,
                Unique = true
            };
            _ = new LocationManager.Location(assetBundle, "_JF_ClassAltar_Ranger")
            {
                MapIcon = "RangerRuneIcon.png",
                ShowMapIcon = ShowIcon.Never,
                Biome = Heightmap.Biome.Meadows,
                SpawnDistance = new(200, 2000),
                SpawnAltitude = new(10, 100),
                MinimumDistanceFromGroup = 100,
                Count = 15,
                Unique = true
            };
            #endregion

            Localizer.Load();
            PlayerFVX.Init();

            #region config
            Config.SaveOnConfigSet = false;

            LevelSystem.Instance.maxLevelConfig = config("General", "Max Level", LevelSystem.Instance.maxLevel, "");
            LevelSystem.Instance.maxLevelConfig = config("General", "Max Level", LevelSystem.Instance.maxLevel, new ConfigDescription("", null, new ConfigurationManagerAttributes { DispName = "Amogus", Order = -1 }));
            LevelSystem.Instance.firstLevelExpConfig = config("General", "First Level Exp", LevelSystem.Instance.firstLevelExp, "");
            LevelSystem.Instance.healOnLevelUpConfig = config("General", "Heal On Level Up", Toggle.On, "");
            LevelSystem.Instance.groupExpFactorConfig = config("General", "Group Exp Factor", 0.7f, "");
            LevelSystem.Instance.levelsSystemTreeConfig = config("DONT TOUCH", "DONT TOUCH_1", "", new ConfigDescription("Please edit the generated json file in the configuration folder.", null, new ConfigurationManagerAttributes { Browsable = false }));
            LevelSystem.Instance.monstersSettingsConfig = config("DONT TOUCH", "DONT TOUCH_2", "", new ConfigDescription("Please edit the generated json file in the configuration folder.", null, new ConfigurationManagerAttributes { Browsable = false }));
            LevelSystem.Instance.openMenuKeyConfig = config("General", "Open Menu Key", KeyCode.U, "");
            LevelSystem.Instance.closeMenuKeyConfig = config("General", "Close Menu Key", KeyCode.Escape, "");


            SetupWatcherOnConfigFile();

            Config.ConfigReloaded += (_, _) => { UpdateConfiguration(); };

            Config.SaveOnConfigSet = true;
            Config.Save();
            #endregion
            #region Patch
            harmony.PatchAll(typeof(Patch));
            harmony.PatchAll(typeof(TerminalCommands.AddChatCommands));
            harmony.PatchAll(typeof(TerminalCommands.ZrouteMethodsServerFeedback));
            harmony.PatchAll(typeof(UI_Path));
            harmony.PatchAll(typeof(Stats_Path));
            harmony.PatchAll(typeof(MonsterDeath_Path.RegisterRpc));
            harmony.PatchAll(typeof(MonsterDeath_Path.GetExpFromMonster));
            harmony.PatchAll(typeof(Altar_Patch.CraftingStationGetLevel));
            harmony.PatchAll(typeof(Tutorial_Patch));
            harmony.PatchAll(typeof(Trader_Path));
            #endregion


            if (haveDualWieldInstaled)
            {
                harmony.PatchAll(typeof(DualWieldPatch));
            }

            _JF_SFX_craftitem_altar = assetBundle.LoadAsset<GameObject>("_JF_SFX_craftitem_altar");
            fx_heal_staff_explosion = assetBundle.LoadAsset<GameObject>("fx_heal_staff_explosion");
            staff_heal_projectile = assetBundle.LoadAsset<GameObject>("staff_heal_projectile");
            staff_heal_aoe = assetBundle.LoadAsset<GameObject>("staff_heal_aoe");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isInputField = false;
            }

            if (SceneManager.GetActiveScene().name == "main" && m_localPlayer && !m_localPlayer.InCutscene() && !m_localPlayer.IsTeleporting() && HaveClass())
            {
                LevelSystem.Instance.Update();
                if (!isInputField)
                {
                    UI.Instance.Update();
                }
            }
        }

        #region Config
        public void SetupWatcherOnConfigFile()
        {
            FileSystemWatcher fileSystemWatcherOnConfig = new(Paths.ConfigPath, ConfigFileName);
            fileSystemWatcherOnConfig.Changed += ConfigChanged;
            fileSystemWatcherOnConfig.IncludeSubdirectories = true;
            fileSystemWatcherOnConfig.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcherOnConfig.EnableRaisingEvents = true;
        }
        public void SetupWatcherOnLevelTree()
        {
            if (!ZNet.instance.IsServer())
            {
                return;
            }

            if (!File.Exists(levelTreePath))
            {
                File.WriteAllText(levelTreePath, JSON.ToNiceJSON(new LevelsSystemTree
                {
                    blocks = new()
                    {
                        new ClassInfo()
                        {
                            className = "berserker",

                            levelExpModifier = 1.04f,
                            everyLevelBonuses = new()
                            {
                                Health = 0,
                                HealthRegeneration = 0,
                                Stamina = 1,
                                StaminaRegeneration = 0,
                                Armor = 0,
                                MoveSpeed = 0,
                                Vampirism = 0,
                                ChanceToNotTakeDmg = 0,
                                ChanceToReturnDmg = 0,
                                ReturnDmg = 0,
                                ChanceToX2Dmg = 0,
                                MaxCarryWeight = 0,
                                AllAttackSpeed = 0,
                                m_ModifySkill = new()
                                {
                                    new()
                                    {
                                        skillName = SkillType.Swords.ToString(),
                                        add = 1
                                    }
                                },
                                unlockSuper = false
                            },
                            levelTree = new()
                            {
                                new LevelInfo
                                {
                                    level = 1,
                                    bonuses = new()
                                    {
                                        MeleDamageMod = 20,
                                        Defense = -20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 10,
                                    bonuses = new()
                                    {
                                        Stamina = 15,
                                        Health = 15
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 20,
                                    bonuses = new()
                                    {
                                        MoveSpeed = 5
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 30,
                                    bonuses = new()
                                    {
                                        m_ModifySkill = new()
                                        {
                                            new()
                                            {
                                                skillName = SkillType.Swords.ToString(),
                                                add = 20
                                            }
                                        },
                                        MeleDamageMod = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 40,
                                    bonuses = new()
                                    {
                                        Stamina = 25,
                                        Health = 25
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 50,
                                    bonuses = new()
                                    {
                                        unlockSuper = true
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 60,
                                    bonuses = new()
                                    {
                                        Vampirism = 3
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 70,
                                    bonuses = new()
                                    {
                                        StaminaRegeneration = 25
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 80,
                                    bonuses = new()
                                    {
                                        ChanceToX2Dmg = 5
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 90,
                                    bonuses = new()
                                    {
                                        Stamina = 50,
                                        Health = 50
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 100,
                                    bonuses = new()
                                    {
                                        MeleAttackSpeed = 30
                                    }
                                }
                            },
                            super = new()
                            {
                                name = "berserker_super",
                                range = 10,
                                time = 120,
                                cooldown = 240,
                                bonuses = new()
                                {
                                    AllDamageMod = 20
                                }
                            },
                            dualWieldExcludedTypesOfWeapons = new()
                            {
                                SkillType.Knives.ToString(),
                                SkillType.Polearms.ToString(),
                                SkillType.Swords.ToString()
                            }
                        },
                        new ClassInfo()
                        {
                            className = "guardian",
                            levelExpModifier = 1.04f,
                            everyLevelBonuses = new()
                            {
                                Health = 2,
                                HealthRegeneration = 0,
                                Stamina = 0,
                                StaminaRegeneration = 0,
                                Armor = 0,
                                MoveSpeed = 0,
                                Vampirism = 0,
                                ChanceToNotTakeDmg = 0,
                                MaxCarryWeight = 0,
                                m_ModifySkill = new() { new() { } },
                                unlockSuper = false
                            },
                            levelTree = new()
                            {
                                new LevelInfo
                                {
                                    level = 1,
                                    bonuses = new()
                                    {
                                        Defense = 20,
                                        MeleDamageMod = -20f,
                                        m_ModifySkill = new()
                                        {
                                            new()
                                            {
                                                skillName = SkillType.Swords.ToString(),
                                                add = 20
                                            }
                                        }
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 10,
                                    bonuses = new()
                                    {
                                        Health = 30
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 20,
                                    bonuses = new()
                                    {
                                        m_ModifySkill = new()
                                        {
                                            new()
                                            {
                                                skillName = SkillType.Blocking.ToString(),
                                                add = 10
                                            }
                                        }
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 30,
                                    bonuses = new()
                                    {
                                        Armor = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 40,
                                    bonuses = new()
                                    {
                                        Health = 50
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 50,
                                    bonuses = new()
                                    {
                                        unlockSuper = true
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 60,
                                    bonuses = new()
                                    {
                                        Vampirism = 10
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 70,
                                    bonuses = new()
                                    {
                                        HealthRegeneration = 1.3f
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 80,
                                    bonuses = new()
                                    {
                                        ChanceToNotTakeDmg = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 90,
                                    bonuses = new()
                                    {
                                        Health = 100
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 100,
                                    bonuses = new()
                                    {
                                        ChanceToReturnDmg = 5,
                                        ReturnDmg = 10
                                    }
                                }
                            },
                            super = new()
                            {
                                name = "guardian_super",
                                range = 10,
                                time = 120,
                                cooldown = 240,
                                bonuses = new()
                                {
                                    Health = 50
                                }
                            },
                            dualWieldExcludedTypesOfWeapons = new()
                            {
                                SkillType.Axes.ToString(),
                                SkillType.Clubs.ToString(),
                                SkillType.Knives.ToString(),
                                SkillType.Polearms.ToString()
                            }
                        },
                        new ClassInfo()
                        {
                            className = "ranger",

                            levelExpModifier = 1.04f,
                            everyLevelBonuses = new()
                            {
                                Health = 0,
                                HealthRegeneration = 0,
                                Stamina = 2,
                                StaminaRegeneration = 0,
                                Armor = 0,
                                MoveSpeed = 0,
                                Vampirism = 0,
                                ChanceToNotTakeDmg = 0,
                                MaxCarryWeight = 0,
                                m_ModifySkill = new()
                                {
                                    new()
                                    {

                                    }
                                },
                                unlockSuper = false
                            },
                            levelTree = new()
                            {
                                new LevelInfo
                                {
                                    level = 1,
                                    bonuses = new()
                                    {
                                        Defense = -30,
                                        BowDamageMod = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 10,
                                    bonuses = new()
                                    {
                                        Stamina = 30
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 20,
                                    bonuses = new()
                                    {
                                        MoveSpeed = 10
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 30,
                                    bonuses = new()
                                    {
                                        BowDamageMod = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 40,
                                    bonuses = new()
                                    {
                                        Stamina = 50
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 50,
                                    bonuses = new()
                                    {
                                        unlockSuper = true,
                                        BowReloadTime = 10
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 60,
                                    bonuses = new()
                                    {
                                        MoveSpeed = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 70,
                                    bonuses = new()
                                    {
                                        AllAttackSpeed = 22
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 80,
                                    bonuses = new()
                                    {
                                        EitrRegeneration = 45
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 90,
                                    bonuses = new()
                                    {
                                        ChanceToX2Dmg = 15
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 100,
                                    bonuses = new()
                                    {
                                        BowReloadTime = 50/*,
                                        NoAmmo = true*/
                                    }
                                }
                            },
                            super = new()
                            {
                                name = "ranger_super",
                                range = 10,
                                time = 120,
                                cooldown = 240,
                                bonuses = new()
                                {
                                    Stamina = 50,
                                    StaminaRegeneration = 35
                                }
                            },
                            dualWieldExcludedTypesOfWeapons = new()
                            {
                                SkillType.Axes.ToString(),
                                SkillType.Clubs.ToString(),
                                SkillType.Polearms.ToString(),
                                SkillType.Swords.ToString()
                            }
                        },
                        new ClassInfo()
                        {
                            className = "druid",

                            levelExpModifier = 1.04f,
                            everyLevelBonuses = new()
                            {
                                Health = 0.6f,
                                Eitr = 2.6f,
                                HealthRegeneration = 0,
                                EitrRegeneration = 0.2f,
                                Stamina = 0.2f,
                                StaminaRegeneration = 0,
                                Armor = 0,
                                MoveSpeed = 0.1f,
                                Vampirism = 0,
                                ChanceToNotTakeDmg = 0,
                                MaxCarryWeight = 0,
                                m_ModifySkill = new()
                                {
                                    new()
                                    {

                                    }
                                },
                                unlockSuper = false
                            },
                            levelTree = new()
                            {
                                new LevelInfo
                                {
                                    level = 1,
                                    bonuses = new()
                                    {
                                        Defense = -30,
                                        AllDamageMod = -18,
                                        SpellDamageMod = 38
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 10,
                                    bonuses = new()
                                    {
                                        Eitr = 30
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 20,
                                    bonuses = new()
                                    {
                                        MoveSpeed = 10
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 30,
                                    bonuses = new()
                                    {
                                        SpellDamageMod = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 40,
                                    bonuses = new()
                                    {
                                        Eitr = 50
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 50,
                                    bonuses = new()
                                    {
                                        unlockSuper = true
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 60,
                                    bonuses = new()
                                    {
                                        SpellAttackSpeed = 20
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 70,
                                    bonuses = new()
                                    {
                                        EitrRegeneration = 15
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 80,
                                    bonuses = new()
                                    {
                                        ChanceToX2Dmg = 15
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 90,
                                    bonuses = new()
                                    {
                                        Stamina = 100
                                    }
                                },
                                new LevelInfo
                                {
                                    level = 100,
                                    bonuses = new()
                                    {
                                        SpellAttackSpeed = 50
                                    }
                                }
                            },
                            super = new()
                            {
                                name = "druid_super",
                                range = 10,
                                time = 120,
                                cooldown = 240,
                                bonuses = new()
                                {
                                    HealthRegeneration = 25
                                }
                            }
                        }
                    }
                }));
            }

            LevelSystem.Instance.levelsSystemTreeConfig.Value = File.ReadAllText(levelTreePath);

            FileSystemWatcher fileSystemWatcherOnCreditsText = new(Paths.ConfigPath, $"{ModGUID}.LevelTree.json");
            fileSystemWatcherOnCreditsText.Changed += LevelTreeChanged;
            fileSystemWatcherOnCreditsText.IncludeSubdirectories = true;
            fileSystemWatcherOnCreditsText.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcherOnCreditsText.EnableRaisingEvents = true;
        }
        public void SetupWatcherOnMonstersSettings()
        {
            if (!ZNet.instance.IsServer())
            {
                return;
            }

            if (!File.Exists(monstersSettingsPath))
            {
                File.WriteAllText(monstersSettingsPath, JSON.ToNiceJSON(new MonstersSettings
                {
                    blocks = new()
                    {
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Boar",
                            exp = 15
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Deer",
                            exp = 20
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Neck",
                            exp = 15
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Greyling",
                            exp = 20
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Greydwarf",
                            exp = 50
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Greydwarf_Elite",
                            exp = 100
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Greydwarf_Shaman",
                            exp = 80
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Skeleton",
                            exp = 60
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Skeleton_Poison",
                            exp = 100
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Skeleton_NoArcher",
                            exp = 60
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Ghost",
                            exp = 80
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Troll",
                            exp = 300
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Blob",
                            exp = 120
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Leech",
                            exp = 120
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Wraith",
                            exp = 200
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Draugr",
                            exp = 100
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Draugr_Ranged",
                            exp = 180
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Surtling",
                            exp = 100
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Draugr_Elite",
                            exp = 300
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "BlobElite",
                            exp = 180
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Abomination",
                            exp = 400
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Wolf",
                            exp = 250
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Fenring",
                            exp = 300
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Hatchling",
                            exp = 250
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "StoneGolem",
                            exp = 450
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Goblin",
                            exp = 350
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "GoblinArcher",
                            exp = 380
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Deathsquito",
                            exp = 200
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Lox",
                            exp = 500
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "GoblinBrute",
                            exp = 450
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "GoblinShaman",
                            exp = 400
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Eikthyr",
                            exp = 150
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "gd_king",
                            exp = 300
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Serpent",
                            exp = 400
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Bonemass",
                            exp = 700
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "Dragon",
                            exp = 1000
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "GoblinKing",
                            exp = 1500
                        },
                        new MonstersSettings.MonstersInfo()
                        {
                            Name = "TrainingDummy",
                            exp = 313
                        },
                    }
                }));
            }

            LevelSystem.Instance.monstersSettingsConfig.Value = File.ReadAllText(monstersSettingsPath);

            FileSystemWatcher fileSystemWatcherOnCreditsText = new(Paths.ConfigPath, $"{ModGUID}.MonstersSettings.json");
            fileSystemWatcherOnCreditsText.Changed += MonstersSettingsChanged;
            fileSystemWatcherOnCreditsText.IncludeSubdirectories = true;
            fileSystemWatcherOnCreditsText.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcherOnCreditsText.EnableRaisingEvents = true;
        }
        private void LevelTreeChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                LevelSystem.Instance.levelsSystemTreeConfig.Value = File.ReadAllText(levelTreePath);
                Config.Reload();
            }
            catch
            {
                DebugError("Can't reload Config");
            }
        }
        private void MonstersSettingsChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                LevelSystem.Instance.monstersSettingsConfig.Value = File.ReadAllText(monstersSettingsPath);
                Config.Reload();
            }
            catch
            {
                DebugError("Can't reload Config");
            }
        }
        private void ConfigChanged(object sender, FileSystemEventArgs e)
        {
            if ((DateTime.Now - LastConfigChange).TotalSeconds <= 5.0)
            {
                return;
            }
            LastConfigChange = DateTime.Now;
            try
            {
                Config.Reload();
                Debug("Reloading Config...");
            }
            catch
            {
                DebugError("Can't reload Config");
            }
        }
        private void UpdateConfiguration()
        {
            Task task = null;
            task = Task.Run(() =>
            {
                LevelSystem.Instance.firstLevelExp = LevelSystem.Instance.firstLevelExpConfig.Value;
                LevelSystem.Instance.maxLevel = LevelSystem.Instance.maxLevelConfig.Value;
                LevelSystem.Instance.groupExpFactor = LevelSystem.Instance.groupExpFactorConfig.Value;
                LevelSystem.Instance.healOnLevelUp = LevelSystem.Instance.healOnLevelUpConfig.Value == Toggle.On;
                LevelSystem.Instance.openMenuKey = LevelSystem.Instance.openMenuKeyConfig.Value;
                LevelSystem.Instance.closeMenuKey = LevelSystem.Instance.closeMenuKeyConfig.Value;
                UpdateLevelsSystemTree();
                UpdateMonstersSettings();


                LevelSystem.Instance.UpdateDualWield();
            });

            Task.WaitAll();

            Debug("Configuration Received");
        }
        #endregion
        #region Patch
        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
            private static void ZNetScenePatch()
            {
                if (SceneManager.GetActiveScene().name == "main")
                {
                    _self.Config.Reload();
                    UI.Instance.Init();

                    GameObject staffSkeleton = ZNetScene.instance.GetPrefab("StaffSkeleton");
                    ItemDrop staffSkeletonItemDrop = staffSkeleton.GetComponent<ItemDrop>();
                    if (staffSkeleton)
                    {
                        staffSkeletonItemDrop.m_itemData.m_shared.m_maxQuality = 15;
                    }

                    if (!ZNetScene.instance.m_prefabs.Contains(_self._JF_SFX_craftitem_altar) && _self._JF_SFX_craftitem_altar)
                    {
                        ZNetScene.instance.m_prefabs.Add(_self._JF_SFX_craftitem_altar);
                        ZNetScene.instance.m_namedPrefabs.Add(_self._JF_SFX_craftitem_altar.name.GetStableHashCode(), _self._JF_SFX_craftitem_altar);
                    }
                    if (!ZNetScene.instance.m_prefabs.Contains(_self.fx_heal_staff_explosion) && _self.fx_heal_staff_explosion)
                    {
                        ZNetScene.instance.m_prefabs.Add(_self.fx_heal_staff_explosion);
                        ZNetScene.instance.m_namedPrefabs.Add(_self.fx_heal_staff_explosion.name.GetStableHashCode(), _self.fx_heal_staff_explosion);
                    }
                    if (!ZNetScene.instance.m_prefabs.Contains(_self.staff_heal_projectile) && _self.staff_heal_projectile)
                    {
                        ZNetScene.instance.m_prefabs.Add(_self.staff_heal_projectile);
                        ZNetScene.instance.m_namedPrefabs.Add(_self.staff_heal_projectile.name.GetStableHashCode(), _self.staff_heal_projectile);
                    }
                    if (!ZNetScene.instance.m_prefabs.Contains(_self.staff_heal_aoe) && _self.staff_heal_aoe)
                    {
                        ZNetScene.instance.m_prefabs.Add(_self.staff_heal_aoe);
                        ZNetScene.instance.m_namedPrefabs.Add(_self.staff_heal_aoe.name.GetStableHashCode(), _self.staff_heal_aoe);
                    }

                    if (!ZNet.instance.IsServer())
                    {
                        return;
                    }

                    _self.SetupWatcherOnLevelTree();
                    _self.SetupWatcherOnMonstersSettings();
                }
            }

            [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Pickup)), HarmonyPrefix]
            private static bool HumanoidGetRunePatch(GameObject go, Player __instance, ref bool __result)
            {
                if (SceneManager.GetActiveScene().name != "main")
                {
                    return true;
                }

                if (!__instance.IsPlayer() || __instance != m_localPlayer)
                {
                    return true;
                }

                Inventory inventory = __instance.GetInventory();
                if (__instance.IsTeleporting())
                {
                    return true;
                }

                ItemDrop component = go.GetComponent<ItemDrop>();
                if (component == null)
                {
                    return true;
                }

                component.Load();
                if (__instance.IsPlayer() && (component.m_itemData.m_shared.m_icons == null || component.m_itemData.m_shared.m_icons.Length == 0 || component.m_itemData.m_variant >= component.m_itemData.m_shared.m_icons.Length))
                {
                    return true;
                }

                if (component.m_itemData.m_shared.m_name == "$item_class_rune_RESET")
                {
                    component.m_nview.Destroy();
                    string msg = $"${GetClass()}_class_reseted";
                    ResetClass();
                    __result = true;
                    __instance.Message(MessageHud.MessageType.Center, msg, 0, null);
                    return false;
                }

                /*if (GetClass() != "none" && component.m_itemData.m_shared.m_name.StartsWith("$item_class_rune"))
                {
                    __instance.Message(MessageHud.MessageType.Center, "$msg_cantpickup", 0, null);
                    __result = false;
                    return false;
                }
                else */
                if (component.m_itemData.m_shared.m_name == "$item_class_rune_guardian")
                {
                    SetClass(Class.Guardian);
                    component.m_nview.Destroy();
                    inventory.RemoveOneItem(component.m_itemData);
                    __result = true;
                }
                else if (component.m_itemData.m_shared.m_name == "$item_class_rune_druid")
                {
                    SetClass(Class.Druid);
                    component.m_nview.Destroy();
                    inventory.RemoveOneItem(component.m_itemData);
                    __result = true;
                }
                else if (component.m_itemData.m_shared.m_name == "$item_class_rune_berserker")
                {
                    SetClass(Class.Berserker);
                    component.m_nview.Destroy();
                    inventory.RemoveOneItem(component.m_itemData);
                    __result = true;
                }
                else if (component.m_itemData.m_shared.m_name == "$item_class_rune_ranger")
                {
                    SetClass(Class.Ranger);
                    component.m_nview.Destroy();
                    inventory.RemoveOneItem(component.m_itemData);
                    __result = true;
                }

                if (__result == true)
                {
                    string msg = $"${GetClass()}_class_started";
                    __instance.Message(MessageHud.MessageType.Center, msg, 0, null);

                    return false;
                }
                return true;
            }

            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(string), typeof(int), typeof(float), typeof(Vector2i), typeof(bool), typeof(int), typeof(int), typeof(long), typeof(string), typeof(Dictionary<string, string>)), HarmonyPrefix]
            private static bool InventoryGetRune1Patch(string name, Inventory __instance)
            {
                if (SceneManager.GetActiveScene().name != "main")
                {
                    return true;
                }

                if (!m_localPlayer)
                {
                    return true;
                }

                bool result = false;
                GameObject prefab = ZNetScene.instance.GetPrefab(name);
                if (!prefab)
                {
                    return true;
                }

                ItemDrop component = prefab.GetComponent<ItemDrop>();
                if (!component)
                {
                    return true;
                }

                if (name == "ResetRune")
                {
                    string msg = $"${GetClass()}_class_reseted";
                    ResetClass();
                    m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);
                    return true;
                }

                if (HaveClass() && name.Contains("_JF_") && name.Contains("Rune"))
                {
                    m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_cantpickup", 0, null);
                    return false;
                }
                else if (name == "GuardianRune")
                {
                    SetClass(Class.Guardian);
                    __instance.RemoveOneItem(component.m_itemData);
                    result = true;
                }
                else if (name == "DruidRune")
                {
                    SetClass(Class.Druid);
                    __instance.RemoveOneItem(component.m_itemData);
                    result = true;
                }
                else if (name == "BerserkerRune")
                {
                    SetClass(Class.Berserker);
                    __instance.RemoveOneItem(component.m_itemData);
                    result = true;
                }
                else if (name == "RangerRune")
                {
                    SetClass(Class.Ranger);
                    __instance.RemoveOneItem(component.m_itemData);
                    result = true;
                }

                if (result == true)
                {
                    string msg = $"${GetClass()}_class_started";
                    m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);

                    return false;
                }
                return true;
            }
            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData)), HarmonyPrefix]
            private static bool InventoryGetRune2Patch(ItemDrop.ItemData item, Inventory __instance)
            {
                if (SceneManager.GetActiveScene().name != "main")
                {
                    return true;
                }

                if (!m_localPlayer)
                {
                    return true;
                }

                bool result = false;
                if (item.m_shared.m_name == "$item_class_rune_RESET")
                {
                    string msg = $"${GetClass()}_class_reseted";
                    ResetClass();
                    m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);
                    return false;
                }

                if (GetClass() != "none" && item.m_shared.m_name.EndsWith("Rune"))
                {
                    m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_cantpickup", 0, null);
                    return false;
                }
                else if (item.m_shared.m_name == "$item_class_rune_guardian")
                {
                    SetClass(Class.Guardian);
                    __instance.RemoveOneItem(item);
                    result = true;
                }
                else if (item.m_shared.m_name == "$item_class_rune_druid")
                {
                    SetClass(Class.Druid);
                    __instance.RemoveOneItem(item);
                    result = true;
                }
                else if (item.m_shared.m_name == "$item_class_rune_berserker")
                {
                    SetClass(Class.Berserker);
                    __instance.RemoveOneItem(item);
                    result = true;
                }
                else if (item.m_shared.m_name == "$item_class_rune_ranger")
                {
                    SetClass(Class.Ranger);
                    __instance.RemoveOneItem(item);
                    result = true;
                }

                if (result == true)
                {
                    string msg = $"${GetClass()}_class_started";
                    m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);

                    return false;
                }
                return true;
            }

            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int)), HarmonyPrefix]
            private static bool InventoryGetRune3Patch(ItemDrop.ItemData item, Inventory __instance)
            {
                if (SceneManager.GetActiveScene().name != "main")
                {
                    return true;
                }

                if (!m_localPlayer)
                {
                    return true;
                }

                bool result = false;
                if (item.m_shared.m_name == "$item_class_rune_RESET")
                {
                    string msg = $"${GetClass()}_class_reseted";
                    ResetClass();
                    m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);
                    return false;
                }

                if (GetClass() != "none" && item.m_shared.m_name.EndsWith("Rune"))
                {
                    m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_cantpickup", 0, null);
                    return false;
                }
                else if (item.m_shared.m_name == "$item_class_rune_guardian")
                {
                    SetClass(Class.Guardian);
                    __instance.RemoveOneItem(item);
                    result = true;
                }
                else if (item.m_shared.m_name == "$item_class_rune_druid")
                {
                    SetClass(Class.Druid);
                    __instance.RemoveOneItem(item);
                    result = true;
                }
                else if (item.m_shared.m_name == "$item_class_rune_berserker")
                {
                    SetClass(Class.Berserker);
                    __instance.RemoveOneItem(item);
                    result = true;
                }
                else if (item.m_shared.m_name == "$item_class_rune_ranger")
                {
                    SetClass(Class.Ranger);
                    __instance.RemoveOneItem(item);
                    result = true;
                }

                if (result == true)
                {
                    string msg = $"${GetClass()}_class_started";
                    m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);

                    return false;
                }
                return true;
            }

            [HarmonyPatch(typeof(Attack), nameof(Attack.Start)), HarmonyPrefix]
            private static bool ActivateSuper(Humanoid character, ItemDrop.ItemData weapon, Attack __instance)
            {
                if (SceneManager.GetActiveScene().name != "main")
                {
                    return true;
                }

                if (weapon?.m_shared.m_name == "$item_ancients_horn")
                {
                    _self.Debug($"ActivateSuper");
                    if (HaveClass() && LevelSystem.Instance.GetFullBonuses().unlockSuper)
                    {
                        if (character && character == m_localPlayer)
                        {
                            if (LevelSystem.Instance.UseSuper())
                            {
                                return true;
                            }
                            else if ((DateTime.Now - LevelSystem.Instance.LastSuper).TotalSeconds >= 1.0)
                            {
                                if (LevelSystem.Instance.HaveOwnSuper())
                                {
                                    character.Message(MessageHud.MessageType.Center, "$super_already_active");
                                }
                                else
                                {
                                    character.Message(MessageHud.MessageType.Center, "$super_not_ready");
                                }

                                return false;
                            }
                        }
                    }
                    else
                    {
                        character.Message(MessageHud.MessageType.Center, "$horn_wrong_usage");
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(DualWield.DualWield.Patch_Humanoid_EquipItem), nameof(DualWield.DualWield.Patch_Humanoid_EquipItem.CheckDualOneHandedWeaponEquip))]
        internal static class DualWieldPatch
        {
            private static bool Prefix(ItemDrop.ItemData item)
            {
                ClassInfo classTree = LevelSystem.Instance.GetClassTree();
                for (int i = 0; i < classTree?.dualWieldExcludedTypesOfWeapons.Count; i++)
                {
                    if (item != null)
                    {
                        if (item.m_shared.m_skillType == SkillTypeFromName(classTree.dualWieldExcludedTypesOfWeapons[i]))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        #endregion
        #region tools
        static byte[] StreamToByteArray(Stream input)
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                result = memoryStream.ToArray();
            }
            return result;
        }
        internal static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
        public static AssetBundle LoadAssetBundleFromResources(string bundleName)
        {
            Assembly resourceAssembly = Assembly.GetExecutingAssembly();
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException("Parameter resourceAssembly can not be null.");
            }
            string text = null;
            try
            {
                text = resourceAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith(bundleName));
            }
            catch (Exception)
            {
            }
            if (text == null)
            {
                _self.DebugError("AssetBundle " + bundleName + " not found in assembly manifest");
                return null;
            }
            AssetBundle result;
            using (Stream manifestResourceStream = resourceAssembly.GetManifestResourceStream(text))
            {
                result = AssetBundle.LoadFromStream(manifestResourceStream);
            }
            return result;
        }


        public static SkillType SkillTypeFromName(string skillName)
        {
            if (Enum.TryParse(skillName, out SkillType skill))
            {
                return skill;
            }
            else
            {
                return SkillType.None;
            }
        }
        public void Debug(string msg)
        {
            Logger.LogInfo(msg);
        }
        public void DebugError(string msg)
        {
            Logger.LogError($"{msg} Write to the developer and moderator if this happens often.");
        }
        public static string GetClass(Player player = null)
        {
            if (!m_localPlayer)
            {
                return "none";
            }

            if (!player)
            {
                player = m_localPlayer;
            }

            player.m_knownTexts.TryGetValue("TribeClasses_Class", out string savedClass);

            if (string.IsNullOrEmpty(savedClass))
            {
                return "none";
            }
            else
            {
                return savedClass;
            }
        }
        public static bool HaveClass(Player player = null)
        {
            if (!player)
            {
                player = Player.m_localPlayer;
            }

            string savedClass = GetClass(player);

            if (savedClass == "none")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static void SetClass(Class @class = Class.None, Player player = null)
        {
            if (player == null)
            {
                player = Player.m_localPlayer;
            }

            string text = @class switch
            {
                Class.None => "none",
                Class.Guardian => "guardian",
                Class.Druid => "druid",
                Class.Berserker => "berserker",
                Class.Ranger => "ranger",
                _ => "none",
            };

            if (player.m_knownTexts.ContainsKey("TribeClasses_Class"))
            {
                player.m_knownTexts.Remove("TribeClasses_Class");
            }

            player.m_knownTexts.Add("TribeClasses_Class", text);

            ZDO zdo = m_localPlayer.m_nview.GetZDO();
            zdo.Set($"TribeClasses_Class", text);
            ZDOMan.instance.ForceSendZDO(zdo.m_uid);

            LevelSystem.Instance.onClassGetReset?.Invoke();
            LevelSystem.Instance.ApplyBonuses();
        }
        public static void ResetClass()
        {
            LevelSystem.Instance.RemoveBonuses();

            SetClass();
            m_localPlayer.m_knownTexts.Remove("TribeClasses_Class");
            m_localPlayer.m_knownTexts["TribeClasses_Level"] = "0";
            m_localPlayer.m_knownTexts["TribeClasses_CurrentExp"] = "0";
            LevelSystem.Instance.RemooveSuperBonuses();
            LevelSystem.Instance.canUseSuper = true;

            UI.Instance.HideMenu();
        }


        public static void DestroyWithDelay(ZNetView view, float delay)
        {
            if (view)
            {
                _self.StartCoroutine(_self.DestroyWithDelayIEnumerator(view, delay));
            }
        }
        public enum Class
        {
            None,
            Guardian,
            Berserker,
            Ranger,
            Druid
        }
        public void UpdateLevelsSystemTree()
        {
            StartCoroutine(UpdateLevelsSystemTreeIEnumerator());
        }
        private IEnumerator UpdateLevelsSystemTreeIEnumerator()
        {
            LevelsSystemTree data = JSON.ToObject<LevelsSystemTree>(LevelSystem.Instance.levelsSystemTreeConfig.Value);

            yield return data;

            LevelSystem.Instance.levelsSystemTree = data;
        }
        public void UpdateMonstersSettings()
        {
            StartCoroutine(UpdateMonstersSettingsIEnumerator());
        }
        private IEnumerator UpdateMonstersSettingsIEnumerator()
        {
            MonstersSettings data = JSON.ToObject<MonstersSettings>(LevelSystem.Instance.monstersSettingsConfig.Value);

            yield return data;

            LevelSystem.Instance.monstersSettings = data;
        }
        public IEnumerator SuperСooldown(Super super)
        {
            yield return new WaitForSeconds(super.data.cooldown);
            LevelSystem.Instance.canUseSuper = true;
        }
        public IEnumerator AddSuper(Super super)
        {
            bool isMine = super.IsMine();
            if (isMine)
            {
                LevelSystem.Instance.canUseSuper = false;
            }

            LevelSystem.Instance.AddSuperBonuses(super);
            PlayerFVX.Instance.Super(super.data);

            yield return new WaitForSeconds(super.data.time);
            LevelSystem.Instance.RemooveSuperBonuses();
            if (isMine)
            {
                StartCoroutine(SuperСooldown(super));
            }
        }
        public IEnumerator DestroyWithDelayIEnumerator(ZNetView view, float delay)
        {
            yield return new WaitForSeconds(delay);
            view.Destroy();
        }
        #endregion
    }
}