using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.GameData.Shops;
using HarmonyLib;
using StardewValley.Tools;
using StardewValley.Monsters;
using Microsoft.Xna.Framework.Audio;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using xTile.Dimensions;
using StardewValley.GameData.Characters;
using System.Reflection.Emit;
using System.Reflection;
using StardewValley.Minigames;
using Microsoft.VisualBasic;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Inventories;
using StardewValley.GameData.HomeRenovations;
using System.Drawing;
using System.Xml.Linq;
using StardewValley.GameData.BigCraftables;
using Force.DeepCloner;
using chaosaddon;
using System.Xml.Serialization;
using Netcode;
using System.Reflection.PortableExecutable;
using StardewValley.GameData.Machines;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Immutable;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FishPonds;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using StardewValley.GameData.Locations;
using static HarmonyLib.Code;
using System.Linq.Expressions;
using StardewValley.TerrainFeatures;
using System.Diagnostics.CodeAnalysis;
using StardewValley.Extensions;
using StardewValley.Buildings;


// TO-ADD LIST














/// /// <inheritdoc cref="IContentEvents.AssetRequested"/>
/// <param name="sender">The event sender.</param>
/// <param name="e">The event data.</param>
/// 

namespace chaosaddon
{
    struct DCpair
    {
        public Chest donor;
        public Chest rec;
        public StardewValley.Object donorM;
        public StardewValley.Object recM;
        public List<Chest> Signals;

    }




    internal class ModEntry : Mod
    {
        //Reflection




        //data. readonly pls ;u; (bc of shallow copy)
        Dictionary<string, int> BPM = new Dictionary<string, int>();

        Thread Music;
        Thread Misc;
        bool GainExpHoe = false;

        int randomvar_startday = 10; /// for random chance in on daystart
        int randomvar_events1 = 1200; //Random chance in game, early
        int randomvar_events2 = 1800;  //random chance in game, late

        static bool GetValuesIsRunning = false;
        static bool MusicAttackIsRunning = false;
        static bool CurseTempActive = false;
        bool CurseActive = false;  //Says when a curse should be active.
        int CurCurse = 5;  // The current curse (part of switch case logic)
        //curses
        Thread SuperSpeed;

        Thread Jump;
        Thread Seasonal;

        BuffAttributesData buffDataMusic;

        BuffEffects buffEffectsMusic;
        Buff musicAttack;


        //OBJECTS

        /// DirectionChest

        BigCraftableData dcup = new BigCraftableData();
        BigCraftableData dcdown = new BigCraftableData();
        BigCraftableData dcleft = new BigCraftableData();
        BigCraftableData dcright = new BigCraftableData();
        BigCraftableData signalC = new BigCraftableData();

        ObjectData signal = new ObjectData();

        //automation stuff
        static int index = 0;
        static Dictionary<Chest, DCpair> listP1 = new Dictionary<Chest, DCpair>();
        static Dictionary<Chest, DCpair> listP2 = new Dictionary<Chest, DCpair>();
        static Dictionary<Chest, DCpair> listP3 = new Dictionary<Chest, DCpair>();
        static Dictionary<Chest, DCpair> listP4 = new Dictionary<Chest, DCpair>();

        BigCraftableData OreDestroyer = new BigCraftableData();
        BigCraftableData WoodDestroyer = new BigCraftableData();
        BigCraftableData CropHarvester = new BigCraftableData();

        //CRAFTING RECIPES
        CraftingRecipe Wood1;

        //CROPS
        ///BOMBSEEDS
        CropData Bomb = new CropData();
        ObjectData Bombseeds = new ObjectData();
        ShopItemData Bombshop = new ShopItemData();


        ///BEERSEEDS
        CropData Beer = new CropData();
        ObjectData Beerseeds = new ObjectData();
        ShopItemData Beershop = new ShopItemData();


        ///CATBULB
        CropData CATBULBCROP = new CropData();
        ObjectData CATBULBSEEDS = new ObjectData();
        ObjectData CATBULB = new ObjectData();
        ShopItemData CATBULBSEEDSHOP = new ShopItemData();

        //love interests

        Dictionary<string, bool> canRomance = new Dictionary<string, bool>();

        //Direction chest pairs.

        static Dictionary<Chest, DCpair> DCpairs = new Dictionary<Chest, DCpair>();

        static Dictionary<StardewValley.Buildings.Building, Chest> Bpairs = new Dictionary<StardewValley.Buildings.Building, Chest>();

        static List<Chest> Destroyers = new List<Chest>();

        //junimo chest hell

        static Inventory junimo = new Inventory();
        public override void Entry(IModHelper helper)
        {


            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;

            helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;


            var harmony = new Harmony(this.ModManifest.UniqueID);
            //Harmony.DEBUG = true;
            //*
            harmony.Patch(
            original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.GetAmmoDamage)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(GetAmmoDamage_Prefix))
            );
            //*/
            harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(GeteatObject))
            );
            harmony.Patch(
            original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(GetplacementAction)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(GetplacementAction2))
            );

            harmony.Patch(
            original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performRemoveAction)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(GetperformRemoveAction))
            );




            harmony.Patch(
            original: AccessTools.Method(typeof(BuffManager), nameof(BuffManager.Update)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(BuffUpdate_Transpiler))
            );







        }





        ///Objects CHANGES



        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/NPCDispositions") && Game1.player != null && canRomance != null)
            {

                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    foreach (var data1 in data)
                    {
                        if (data1.Value.Contains("/datable") && !canRomance[data1.Key])
                        {
                            data1.Value.Replace("/datable", "/not-datable");
                        }
                        else if (data1.Value.Contains("/not-datable") && canRomance[data1.Key])
                        {
                            data1.Value.Replace("/not-datable", "/datable");
                        }

                    }

                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ObjectData>().Data;
                    /// PIERRRE STORE ///



                    ///SEEDS///

                    ///Parsnip
                    data["472"].Price = 210;
                    data["472"].Description = "Parsnip seeds. Sells for more if not grown. Grows.";
                    ///potato
                    data["475"].Price = 210;
                    data["475"].Description = "Seems starchy. Grows in 5, and can be grown in winter.";
                    /// bean
                    data["473"].Price = 210;
                    data["473"].Description = "Beans, the ultimate power source. Grows in 7, regrows in 2.";
                    /// cauli
                    data["474"].Price = 690;
                    data["474"].Description = "Caule- Calli- Fuck my typing, just know that these are one hell of an investment. Grows in 12, regrows in 6.";
                    ///Kale
                    data["477"].Description += "  Who the fuck buys these?";
                    ///Melon
                    data["479"].Description = "heh. Grows in 12.";
                    ///Tomato
                    data["480"].Description = "Doesn't matter how you say it. Grows in 11, Regrows in 4.";
                    ///Blueberries
                    data["481"].Price = 100;
                    data["481"].Description += " (actually these are the best)";
                    ///Pepper Seeds
                    data["482"].Description = "Truly the best crop in the summer, but that's my opinion, and I made this mess of a mod. Grows in 5, regrows in 3.";
                    data["482"].Price = 70;
                    ///amaratnh
                    data["299"].Description += "I'm guessing you want to buy this for a quest?";
                    /// cranberries
                    data["493"].Name = "Holly Seeds";




                    ///CROP//

                    ///Parsnip
                    data["24"].Price = 1;
                    data["24"].Description = "Has a nice Banana Lady sticker on it.";
                    ///Potato
                    data["192"].Price = 800;
                    data["192"].Description = "Good communist investment, comrade.";
                    /// bean
                    data["188"].Price = 100;
                    data["188"].Description = "Dosh approved.";
                    /// cauli
                    data["190"].Price = 1000;
                    data["190"].Description = "Used to be worth 80085 gold, but then the stock market fell.";
                    /// cranberries
                    data["282"].Name = "Holly Berries";
                    data["282"].Edibility = -50;


                    /// MATERIALS


                    /// Stone
                    data["390"].Price = 2;
                    data["390"].Description = "Rocky.";
                    /// wood
                    data["388"].Price = 2;
                    data["388"].Description = "Woody.";

                    /// hard wood (Owo)
                    data["709"].Price = 1;
                    data["709"].Description = "Don't you dare snicker at me, this is some good wood right here.";

                    ///FOOD
                    ///cran sauce..?
                    data["238"].Name = "Holly Sauce";
                    data["238"].Description = "Seems edible...?";
                    ///cran candy
                    data["612"].Name = "Holly Candy";
                    data["612"].Description = "Barely edible.";



                    /// OTHER
                    /// purple shorts



                    /// custom

                    data.Add("BEERSEEDS", Beerseeds);
                    ///*
                    data.Add("CATBULB", CATBULB);
                    data.Add("CATBULBSEEDS", CATBULBSEEDS);

                    data.Add("BOMBSEEDS", Bombseeds);

                    data.Add("SIGNAL", signal);
                    //*/
                    /// REFRENCE ///
                    ///foreach ((string itemID, ObjectData itemData) in data)
                    ///{
                    ///    itemData.Price *= 2;
                    ///}
                    ///390 is stone

                });
            }

            ///DATA CROPS

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Crops"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, CropData>().Data;



                    ///SPRING
                    ///Parsnip
                    data["472"].DaysInPhase = new List<int> { 0, 0, 0, 0 }; ///auto harvest?

                    var Beans = data["473"]; ///creating an absolute unit... also you can notice I figured out what var does huh
                    Beans.DaysInPhase = new List<int> { 0, 0, 0, 2, 5 };
                    Beans.HarvestMaxStack = 2;
                    Beans.RegrowDays = 2;

                    var Potato = data["475"];
                    Potato.DaysInPhase = new List<int> { 0, 0, 0, 2, 3 };
                    Potato.IsRaised = true;
                    Potato.Seasons.Add(Season.Winter); ///russian potatoes. fuck it.


                    var Cauli = data["474"];
                    Cauli.DaysInPhase = new List<int> { 0, 0, 0, 6, 6 };
                    Cauli.IsRaised = true;




                    ///Summer
                    ///
                    ///Melons
                    data["479"].IsRaised = true;


                    /// Custom
                    data.Add("BEERSEEDS", Beer);
                    data.Add("CATBULBSEEDS", CATBULBCROP);
                    data.Add("BOMBSEEDS", Bomb);
                });


            }



            /// DATA SHOPS

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ShopData>().Data;


                    /// Custom 
                    ///Beerseeds 

                    data["SeedShop"].Items.Add(Beershop);
                    data["SeedShop"].Items.Add(CATBULBSEEDSHOP);
                    data["SeedShop"].Items.Add(Bombshop);

                });
            }

            /// DATA MAIL
            /// 
            if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    //who needs content packs when you can just do in it in c# /hj

                    string fourthwall = Environment.UserName;

                    data["Robin"] = "Heya. ^ Its been a while, right? ^ Anyways, here's some wood. Don't mind the description. ^ -Robin %item id (O)709 50 %%[#]Wood from your house.";
                    data["Demetrius"] = "Hello there, @. ^ I forgot what this was, it was back inside my house somewhere, and I decided I didn't need it anymore. ^ I Hope you find it useful. ^-Demetrius%item id (O)346 1 %%[#] Some stuff Demetrius found.";
                    data["afterSamShow"] = "TELL ME THE FICTION, ILL SIT BACK AND LISTEN ^ THIS TIME WILL BE DIFFERENT ^ THE B-B-BULLSHIT ^ THE ENDLESS COMMITMENT ^ (...It keeps going. Wow.) ^ -Sam[#]Seems someone is a Q-bomb fan. ";


                    data.Add("Chaosmod_custom1", "Seems like it's been a while since you have been on the farm. ^ Maybe you have seen how time works in these parts, it can be crazy sometimes, right? ^^ Anyways, good luck in your endeavors.  -???[#]???");
                    data.Add("Chaosmod_custom2", "Wow! You seem to like this place? That or, you might hate it. ^ Oh well. These letters are not as much as a letter to you sometimes as it is a writing prompt. ^^ It's been a while since ive seen a person so involved in what I make... Maybe as a made up challenge, sure, but it still means a lot to me. ^ Welp, this has gone on for long enough. Good luck! -???[#]???");
                    data.Add("Chaosmod_custom3", "This whole project of mine was originally why I started on my coding journey on the first place...^ although I don't really see it as something I directly play a lot. ^ There was a lot more that I have made and wanted to add, but I just can't for various reasons. ^^ Despite this, ill create new content when I can. ^ I might get in trouble for saying this, but thank you, " + fourthwall + ". -SomeoneEls[#]SomeoneEls");

                    data.Add("WrongAddress1", "Hey. It's been a while, Yu. It's been a while since the incident way back in Inaba. ^I doubt I would ever get parole for what I did, but who cares about that yeah? ^^ Haha.. just kidding, I'm making plenty enough friends down where I am. Anyways, make sure you visit that old geezer sometime to make sure he's fine yeah? ^^ Just between me and you, I think he would want the company.");
                    data.Add("WrongAddress2", "HEY MAN! It's Joe. ^ I know I have a lot of work nowadays with all the gangs and shit but I was wonderin if you still want that old guitar that Neko stole.  ^ It's a bit too old for me nowadays, so I bought another, ...ya want it?^ ANYWAYS. Take care of ya self man!");

                    data.Add("FunFact1", "Fun fact: this is the first fun fact in written in the mod.");
                    data.Add("FunFact2", "Fun fact: Im typing this on homestuck day.");
                    data.Add("FunFact3", "Fun fact: If you look on the forms, I had a whole issue dealing with the datable varible while making this mod.");
                    data.Add("FunFact4", "Fun fact: I should be doing an assignment rn but I don't wanna :V");
                    data.Add("FunFact5", "Fun fact: this is a fun fact.");
                    data.Add("FunFact6", "Fun fact: I started this mod way back in 2020.");
                    data.Add("FunFact7", "Fun fact: I like trains");
                    data.Add("FunFact8", "Fun fact: Transpliers in c# basically add MSIL code into your executables.");
                    data.Add("FunFact9", "Fun fact: Ive only shared the files of the uncut verison of this mod (with the copyright issues) with 1 person. Ive shown the uncut version with a bunch of people though.");
                    data.Add("FunFact10", "Fun fact: As of today, there are 2 goto statements in this mod. One is basically mandatory, the other one just reads better.");
                    data.Add("FunFact11", "Fun fact: Some of the code in this mod is cursed..");
                    data.Add("FunFact12", "Fun fact: There is a harmony prefix in this mod that allows custom side effects for buffs. It is currently unused.");
                    data.Add("FunFact13", "Fun fact: I am a qbomb fan.");
                    data.Add("FunFact14", "Fun fact: You can't get the peter griffin event if you are in a dungeon or mineshaft.");
                    data.Add("FunFact15", "Fun fact: The custom events in this mod are done via multithreading.");
                    data.Add("FunFact16", "Fun fact: You can get farming experience by using your hoe in this mod.");
                    data.Add("FunFact17", "Fun fact?: Yesterday, I asked you-");
                    data.Add("FunFact18", "Fun fact: Sometimes, I just look at the forms to help people with making mods (not to satiate my crippling depression).");
                    data.Add("FunFact19", "Fun fact: Rina-chan best girl, ROBOhead best boi.");
                    data.Add("FunFact20", "Fun fact: The random warp event fails if you have more than 1.5GB of trash in the garbage collector. If this is the case, it is probable that at least one of your mods has a memory leak.");


                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;

                    data.Add("DirectionChestUp", dcup);
                    data.Add("DirectionChestDown", dcdown);
                    data.Add("DirectionChestLeft", dcleft);
                    data.Add("DirectionChestRight", dcright);
                    data.Add("SignalChest", signalC);
                    data.Add("CropHarvester", CropHarvester);
                    data.Add("OreDestroyer", OreDestroyer);
                    data.Add("WoodDestroyer", WoodDestroyer);

                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    data.Add("DirectionChestUp", "388 50/Home/DirectionChestUp/true/default/");
                    data.Add("DirectionChestDown", "388 50/Home/DirectionChestDown/true/default/");
                    data.Add("DirectionChestLeft", "388 50/Home/DirectionChestLeft/true/default/");
                    data.Add("DirectionChestRight", "388 50/Home/DirectionChestRight/true/default/");
                    data.Add("SignalChest", "388 50/Home/SignalChest/true/default/");
                    data.Add("Signal", "388 0/Home/SIGNAL/false/default/");

                    data.Add("CropHarvester", "388 50 335 5/Home/CropHarvester/true/default/");
                    data.Add("OreDestroyer", "388 50 335 5/Home/OreDestroyer/true/default/");
                    data.Add("WoodDestroyer", "388 50 335 5/Home/WoodDestroyer/true/default/");

                    data.Add("Mini Shipping Bin", "388 50 336 5/Home/248/true/default/");
                    data.Add("Junimo Chest", "388 50 337 5/Home/256/true/default/");
                });
            }


        }



        ///DISPLAY

        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {


        }


        /// START OF DAY CHANGES 
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            //Thread teleport = new Thread(new ParameterizedThreadStart(randomWarpEvent));
            //teleport.Start(); //starts a teleport (see randomWarpEvent)
            //debug
           
            
            
            
            
            


            SoundBank soundBank = this.Helper.Reflection.GetField<SoundBank>(Game1.soundBank, "soundBank").GetValue();
            IEnumerable<CueDefinition> cues = this.Helper.Reflection.GetField<Dictionary<string, CueDefinition>>(soundBank, "_cues").GetValue().Values;


            ///Reflection







            //romances
            int seed = 0;
            for (int i = 0; i < Game1.player.Name.Length; i++)
            {
                seed += Game1.player.Name[i];

            }
            doRomance(seed);


            //Direction chests

            InitiateDirectionChests();

            InitiateBuildingChests();

            //Game1.player.addItemToInventory((Item)new StardewValley.Object("288", 1, false, 10, 0));
            /// Misc threads



            //DAY SPECIFIC

            ///adds bombs for 10 days
            if (Game1.season == Season.Spring && Game1.year == 1 && Game1.dayOfMonth <= 10)
            {
                Game1.player.addItemToInventory((Item)new StardewValley.Object("288", 1, false, 10, 0));
            }


            ///MAIL
            if (Game1.season == Season.Spring && Game1.year == 1 && Game1.dayOfMonth == 11)
            {
                Game1.addMail("Chaosmod_custom1");
            }
            if (Game1.season == Season.Spring && Game1.year == 2 && Game1.dayOfMonth == 1)
            {
                Game1.addMail("Chaosmod_custom2");
            }
            if (Game1.season == Season.Spring && Game1.year == 3 && Game1.dayOfMonth == 1)
            {
                Game1.addMail("Chaosmod_custom3");
            }

            /// fun fact mail
            if (Game1.dayOfMonth % 10 == 0)
            {
                try
                {
                    int random = new Random().Next(20);
                    random++;
                    Game1.addMail("FunFact" + random);
                }
                catch (Exception ek)
                {

                }
            }

            /// Rare mail
            int r = new Random(DateAndTime.Now.Millisecond).Next(128);
            if (r == 1)
            {
                Game1.addMail("WrongAddress1");
            }
            if (r == 2)
            {
                Game1.addMail("WrongAddress2");
            }


            //SLEEP CHANGES

            /// you sleep in every 2~ days you do in a session, from a time of 700 to 900

            if (randomvar_startday >= 10)
            {
                randomvar_startday = new Random().Next(0, 9); /// you wake up on time always the day after you sleep in, and on the first day
            }
            else
            {
                randomvar_startday = new Random().Next(0, 9); /// stores a random number from 0 to 8
                if (randomvar_startday < 3) /// you sleep in every 3~ days you do in a session 
                {
                    Game1.timeOfDay = 600 + 100 * (randomvar_startday + 1); /// sets a time from 700 to 900
                    randomvar_startday = 10; ///exit loop
                }

            }





            //CURSES

            /// for a day, picks a random curse each day that you have to live with. (or none)
            /// on day end, curseactive is set to false and the curse thread is killed.
            /// on day start, curseactive is set to true and a new curse is chosen.

            CurseActive = true;
            CurCurse = new Random().Next(0, 6);
            switch (CurCurse)
            {
                case 0:

                    SuperSpeed = new Thread(SuperSpeedCurse);
                    SuperSpeed.Start();
                    Game1.hudMessages.Add(new HUDMessage("You have Super Speed"));
                    break;
                case 1:
                    Jump = new Thread(JumpCurse);
                    Jump.Start();
                    Game1.hudMessages.Add(new HUDMessage("You now get scared easily."));
                    break;
                case 2:

                    break;
                case 3:
                    Seasonal = new Thread(SeasonalCurse);
                    Seasonal.Start();
                    break;

            }

        }






        ///END OF DAY CHECKS/CHANGES
        ///

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            ///Curse checks
            CurseActive = false;

            switch (CurCurse)
            {
                case 0:
                    try
                    {
                        while (SuperSpeed.IsAlive)
                        {

                        }
                        break;
                    }
                    catch (NullReferenceException a)
                    {
                        break;
                    }
                case 1:
                    try
                    {
                        while (Jump.IsAlive)
                        {

                        }
                        break;
                    }
                    catch (NullReferenceException b)
                    {
                        break;
                    }
                case 2:

                case 3:
                    try
                    {
                        while (Seasonal.IsAlive)
                        {

                        }
                        break;
                    }
                    catch (NullReferenceException d)
                    {
                        break;
                    }
            }
        }

        /// CONSTANT CHANGES
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            /*
            if (index != -1)
            {

                if (index < DCpairs.ToList().Count)
                {

                    DoTransfers(DCpairs.ToList()[index]);
                    index++;
                }
                else
                {
                    index = 0;
                }
            }
            */
        }

        /// EXP

        private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
        {
            if (Game1.player.UsingTool)
                if (Game1.player.CurrentTool.QualifiedItemId == "(T)Hoe")
                    GainExpHoe = true;
                else
                    GainExpHoe = false;

        }




        private async void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            /*
            if(!Game1.locations.Contains(Game1.currentLocation))
            {
                Game1.locations.Add(Game1.currentLocation);
            }
            Game1.currentLocation.reloadMap();
            foreach(GameLocation location in Game1.locations)
            {
                Console.WriteLine(location);
                foreach(Building b in location.buildings)
                {
                    Console.WriteLine(" :"+b.buildingType);
                }
            }
            */
           // InitiateBuildingChests();

            //debug 


            //fixing it so you can shop at clints even if you are married to him.

            if (Game1.timeOfDay == 900 && Game1.player.getSpouse() != null && Game1.player.getSpouse().Name == "Clint")
            {
                Game1.warpCharacter(Game1.getCharacterFromName("Clint"), "Blacksmith", new Vector2(3, 13));
            }
            else if (Game1.timeOfDay == 1600 && Game1.player.getSpouse() != null && Game1.player.getSpouse().Name == "Clint")
            {

                Game1.warpCharacter(Game1.getCharacterFromName("Clint"), Game1.locations[0], new Vector2(34, 5));
            }

            //Same for willy

            if (Game1.timeOfDay == 900 && Game1.player.getSpouse() != null && Game1.player.getSpouse().Name == "Willy")
            {
                Game1.warpCharacter(Game1.getCharacterFromName("Willy"), "FishShop", new Vector2(5, 4));
            }
            else if (Game1.timeOfDay == 1600 && Game1.player.getSpouse() != null && Game1.player.getSpouse().Name == "Willy")
            {
                Game1.warpCharacter(Game1.getCharacterFromName("Willy"), Game1.locations[0], new Vector2(34, 5));
            }


            //Direction chests
            // lag inducing code 
            foreach(KeyValuePair<Chest,DCpair> p4 in listP4)
            {
                DoTransfers(p4);
                DoTransfers(listP2);
                
                

            }
            DoTransfers(listP1);
            DoTransfers(listP3);
            





            foreach (KeyValuePair<StardewValley.Buildings.Building, Chest> pair in Bpairs)
            {
                transferB(pair.Key, pair.Value);
                
            }
            //destroyers
            foreach (Chest d in Destroyers)
            {
                if (d.ItemId == "OreDestroyer")
                {
                    OreDestroyerTransfer(d);
                }
                else if (d.ItemId == "WoodDestroyer")
                {
                    WoodDestroyerTransfer(d);
                }
                else if (d.ItemId == "CropHarvester")
                {
                    CropHarvesterTransfer(d);
                }

            }
            ///Random events

            if (e.NewTime == randomvar_events1) //Early 10 am to 2 pm, default starting time of 12 pm
            {
                switch (new Random().Next(0, 10))
                {
                    //new Random().Next(0, 10)
                    case 0:
                        Game1.timeOfDay = 600; //sets time to 6 a
                        Game1.showGlobalMessage("The time was changed..");
                        break;
                    case 1:

                        if (!Game1.isRaining && !Game1.isDebrisWeather)
                        {
                            Game1.showGlobalMessage("Time to take-a Piss!");
                            if (Game1.season != StardewValley.Season.Winter)
                                Game1.isRaining = true;
                            else
                                Game1.isSnowing = true;
                        }
                        else if (!Game1.isLightning && !Game1.isSnowing)
                        {
                            Game1.showGlobalMessage("The skies start to part suddenly.");
                            if (Game1.season != StardewValley.Season.Winter)
                                Game1.isRaining = false;
                            else
                                Game1.isSnowing = false;
                        }
                        break;
                    case 2:
                        Game1.player.addItemToInventory(RandomItem());  //adds random item
                        break;
                    case 3:
                        Monster HA;
                        try
                        {
                            Vector2 playlocation = Game1.player.Position;
                            playlocation.Y += 100;
                            Monster HB = new GreenSlime(playlocation);  // Creates a slime. MUST USE GREENSLIME CLASS
                            Game1.currentLocation.addCharacter(HB);
                            Game1.showGlobalMessage("A monster was spawned.");
                        }
                        catch (Exception ex)  // if  the vector2 does not exist
                        {

                        }
                        break;
                    case 4:
                        Game1.playSound("dog_bark");   /// makes a dog bark sound effect
                        break;
                    case 5:
                        Game1.playSound("questcomplete"); // confuses you with a quest complete sound effect
                        Game1.hudMessages.Add(new HUDMessage("Owo"));
                        break;
                    case 6:
                        Game1.toggleFullScreen = !Game1.toggleFullScreen; // toggle fullscreen from on to off and vice versa.
                        break;
                    case 7:
                        int health = Game1.player.health;
                        if (health / 2 == 0)      // snaps your health in half.
                        {
                            Game1.player.health = 1;
                        }
                        else
                        {
                            Game1.player.health /= 2;
                        }
                        Game1.hudMessages.Add(new HUDMessage("The fuck?"));
                        Game1.showGlobalMessage(Game1.player.Name + " got their health snapped in half.");
                        break;
                    case 8:
                        Game1.player.addedSpeed -= 3; //subtracts speed
                        break;
                    case 9:
                        if (MineShaft.IsGeneratedLevel(Game1.player.currentLocation, out int extrainfo) || VolcanoDungeon.IsGeneratedLevel(Game1.player.currentLocation.Name, out int extrainfo9))
                        {

                        }
                        else
                        {
                            Thread Peter = new Thread(new ParameterizedThreadStart(PeterEvent));
                            Peter.Start(); //starts a peter griffin event if not in a dungeon or mineshaft. (see PeterEvent)

                        }
                        break;



                }
                randomvar_events1 = new Random().Next(10, 15) * 100;  // changes the early event time
            }


            if (e.NewTime == randomvar_events2) // late 4 pm to 8 pm, default starting time of 6 pm
            {
                switch (new Random().Next(0, 10))
                {
                    //new Random().Next(0, 9)
                    case 0:
                        Game1.timeOfDay = 600;
                        break;
                    case 1:
                        Game1.weatherForTomorrow = Game1.weather_rain;
                        Game1.UpdateWeatherForNewDay();
                        Game1.showGlobalMessage("There was a short shower.");
                        break;
                    case 2:
                        Game1.player.addItemToInventory(RandomItem());
                        break;
                    case 3:
                        Game1.player.Stamina += 50; // adds 50 stamina
                        break;
                    case 4:
                        Game1.playSound("explosion");
                        break;
                    case 5:
                        Game1.playSound("goldenWalnut");   ///Confuses you with a fake walnut.
                        Game1.hudMessages.Add(new HUDMessage("UwU"));
                        break;
                    case 6:
                        Game1.player._money += 250;  /// adds money
                        Game1.hudMessages.Add(new HUDMessage("250 gold added for some reason."));
                        break;
                    case 7:
                        Game1.hudMessages.Add(new HUDMessage("Owo"));  //nothing
                        break;
                    case 8:
                        if (Game1.player.Money >= 500)
                        {
                            Game1.player._money -= 250;
                            Game1.hudMessages.Add(new HUDMessage("250 gold subtracted for some reason."));
                        }
                        break;
                    case 9:
                        Thread teleport = new Thread(new ParameterizedThreadStart(randomWarpEvent));
                        teleport.Start(); //starts a teleport (see randomWarpEvent)
                        break;





                }
                randomvar_events2 = new Random().Next(16, 20) * 100;
            }

            //EXP


        }




        // EVENTS

        private void randomWarpEvent(object obj)
        {
            int times = 0;
            while (Game1.player.IsBusyDoingSomething())
            {

            }
            try
            {
                int loc = new Random().Next(0, 32);

                //finds a valid location
                while (loc == 15 || (loc >= 17 && loc <= 21) || (loc >= 25 && loc <= 30))
                {
                    loc = new Random().Next(0, 32);
                }

                GameLocation locat = Game1.locations.ElementAt(loc);
                Vector2 vect;
                GameLocation playerLocat = Game1.player.currentLocation;
                Vector2 playerVect = Game1.player.Tile;

                try
                {

                    //Looks at the diagonal left half of the map for an empty tile.
                    for (int y = 1; true; y++)
                    {
                        for (int x = 1; x < y; x++)
                        {
                            Vector2 buf = new Vector2(x, y);
                            if (!locat.IsTileBlockedBy(buf))
                            {
                                vect = buf;


                                int r = new Random().Next(0, 64);
                                if (r == 3)
                                {
                                    // I fucking hate using goto here as goto sucks but its better than trying to create a complex break system that works with vector2 buf, as buf cannot be null.
                                    goto randomWarpEvent_outOfLoop;
                                }
                                times++;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    //keep doing it until it works, since using goto causes issues. This is pretty cursed though. I'm sorry for my crimes.
                    if (times > 100)
                    {
                        return;
                    }
                    randomWarpEvent(obj);
                    return;
                }

            randomWarpEvent_outOfLoop:
                Game1.warpFarmer(locat.Name, (int)vect.X, (int)vect.Y, false);
                Thread.Sleep(15000);

                //if the integer cast made the current tile blocked
                if (playerLocat.IsTileBlockedBy(new Vector2((int)playerVect.X, (int)playerVect.Y)))
                {

                    Game1.warpHome();
                }
                else
                {
                    Game1.warpFarmer(playerLocat.Name, (int)playerVect.X, (int)playerVect.Y, false);
                }




            }
            catch (Exception e)
            {

            }
        }
        public static void PeterEvent(object obj)
        {
            Game1.player.jump();
            Game1.player.canMove = false;
            Game1.showGlobalMessage(Game1.player.Name + " tripped.");
            Thread.Sleep(5000);
            Game1.showGlobalMessage("'Aughhhh...'");
            Thread.Sleep(5000);
            Game1.showGlobalMessage("'sssss.....Aughhhh...'");
            Thread.Sleep(5000);
            Game1.showGlobalMessage(Game1.player.Name + " is better now.");
            Game1.player.canMove = true;
            Game1.timeOfDay += 200;
        }

        public void MiscEvents(object obj)
        {
            while (true)
            {




                try
                {
                    // EXP
                    //Hoe --> Farming

                    if (Game1.player.UsingTool)
                        if (GainExpHoe)
                        {
                            Game1.player.gainExperience(0, 1);
                            Thread.Sleep(1000);
                            GainExpHoe = false;
                        }



                }
                catch (NullReferenceException e)
                {
                    break;
                }


            }
        }



        //CURSES

        public void SuperSpeedCurse()
        {

            try
            {
                while (CurseActive)
                {

                    Game1.player.Speed = 7;
                }
            }
            catch (Exception e)
            {

            }



        }

        public void JumpCurse()
        {
            try
            {
                while (CurseActive)
                {
                    int ticks = 0;

                    while (60000 * new Random().Next(1, 4) > ticks)
                    {
                        Thread.Sleep(1);
                        if (!CurseActive)
                        {
                            break;
                        }
                        ticks++;
                    }
                    Game1.player.canMove = false;
                    Game1.player.jump();
                    Game1.player.doEmote(16);
                    Thread.Sleep(300);
                    Game1.player.canMove = true;
                }
            }
            catch (NullReferenceException e)
            {

            }
        }



        public void SeasonalCurse()  /// 
        {
            try
            {
                if (Game1.season == Season.Spring)
                {
                    while (CurseActive)
                    {
                        int HealthOG = 0;
                        float EnergyOG = 0f;
                        while (!Game1.player.isEating)
                        {
                            if (!CurseActive)
                                break;

                        }
                        HealthOG = Game1.player.health;
                        EnergyOG = Game1.player.Stamina;
                        while (Game1.player.isEating)
                        {
                            if (!CurseActive)
                                break;

                        }
                        if (new Random().Next(0, 10) > 8 && !CurseActive)
                        {

                            Thread.Sleep(1000);
                            Game1.hudMessages.Add(new HUDMessage("Ew! A bug was in your food, and you spat it out."));
                            Game1.player.health = HealthOG;
                            Game1.player.Stamina = EnergyOG;
                        }
                    }
                }
                else if (Game1.season == Season.Summer)
                {
                    Game1.hudMessages.Add(new HUDMessage("It's hot as hell!"));
                    Game1.player.stamina -= 25f;
                }
                else if (Game1.season == Season.Fall)
                {
                    if (Game1.player.HouseUpgradeLevel == 3)
                    {
                        Game1.warpFarmer("Cellar", 10, 10, false);
                        Game1.hudMessages.Add(new HUDMessage("You fell through the floorboards, and landed in your cellar."));
                        Game1.player.health -= 25;
                    }
                }

            }
            catch (NullReferenceException e)
            {

            }
        }

        //SPECIAL BUFFS
        static public void BuffMethod(string key)
        {

            switch (key)
            {

            }
        }





        //MUSIC
        public void OSTBPM(object o)
        {
            string track = "";
            bool running = false;
            Action<object> wait = (j) =>
            {
                string t = track;
                Game1.showRedMessage("Great!", false);
                for (int tim = 0; tim < 50; tim++)
                {
                    if (t != track)
                    {
                        break;
                    }
                    Thread.Sleep(1);
                }
                running = false;
            };


            buffDataMusic = new BuffAttributesData();
            buffDataMusic.AttackMultiplier = 3;
            buffDataMusic.LuckLevel = 3;

            buffEffectsMusic = new BuffEffects(buffDataMusic);

            int val = 0;
            while (true)
            {
                try
                {
                    track = Game1.getMusicTrackName();

                    while (Game1.getMusicTrackName() != null && BPM.TryGetValue(Game1.getMusicTrackName(), out val) && Game1.player != null && val != null && track == Game1.getMusicTrackName())
                    {
                        val /= 2;
                        for (int tim = 0; tim < val - 75; tim++)
                        {
                            Thread.Sleep(1);
                            if (Game1.getMusicTrackName() != track)
                            {
                                break;
                            }

                        }
                        if (Game1.getMusicTrackName() == track)
                        {

                            musicAttack = new Buff("1337", "none", "none", 150, null, 20, buffEffectsMusic, false, "MusicBuff", "Only active now");
                            Game1.player.applyBuff(musicAttack);
                            MusicAttackIsRunning = true;


                            for (int tim = 0; tim < 150; tim++)
                            {




                                if (Game1.getMusicTrackName() != track)
                                {
                                    break;
                                }
                                if (Helper.Input.IsDown(SButton.MouseLeft) && running == false)
                                {

                                    running = true;
                                    new Thread(new ParameterizedThreadStart(wait)).Start();
                                }

                                Thread.Sleep(1);
                            }


                        }


                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        while (!(Game1.getMusicTrackName() != null && BPM.TryGetValue(Game1.getMusicTrackName(), out val) && Game1.player != null))
                        {
                            Thread.Sleep(1);
                            //Console.WriteLine("fail");
                        }
                        if (Game1.getMusicTrackName() != null)
                        {
                            Console.WriteLine("Chaosaddon: Ah fuck. Something in the method \"OSTBPM\" went wrong.");

                        }
                    }
                    catch
                    {
                        Console.WriteLine("Chaosaddon: Ah fuck. Something else in the method \"OSTBPM\" went wrong.");
                    }
                }


            }
        }

        // NEW METHODS

        public static Item RandomItem()
        {
            string itemnum;

            itemnum = "" + new Random().Next(0, 900);
            Item x = (Item)new StardewValley.Object(itemnum, 1, false, 10, 0);
            if (x.Name == "Error Item")
            {
                return RandomItem();
            }
            return x;

        }
        private bool wait(int time)
        {
            Thread.Sleep(time);
            return true;
        }


        NPC getCharacterLocation(string npcName)
        {
            StardewValley.Object item;
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc.characters.Count > 0)
                {

                    foreach (NPC npc in loc.characters)
                    {


                        if (npc.Name == npcName)
                        {
                            Game1.addHUDMessage(new HUDMessage(npc.Name + " : " + loc.Name + " at " + npc.Tile.X + "," + npc.Tile.Y));
                            return npc;

                        }

                    }
                }
            }
            return null;
        }


        void doRomance(int seed)
        {
            int x = 2;
            bool can = false;
            bool noCanidates = true;


            foreach (var data in Game1.characterData)
            {
                Netcode.NetBool t = new Netcode.NetBool(true);
                Netcode.NetBool f = new Netcode.NetBool(false);
                NPC n = Game1.getCharacterFromName(data.Key);
                if (n == null)
                {
                    continue; //if that charcter doesnt exist
                }
                foreach (var data2 in data.Value.FriendsAndFamily) //Remove any married ppl from the pool
                {

                    if (data2.Value == "[LocalizedText Strings\\Characters:Relative_Husband]" || data2.Value == "[LocalizedText Strings\\Characters:Relative_Wife]")
                    {
                        can = false;
                        Assembly.GetAssembly(typeof(NPC))
                             .GetType("StardewValley.NPC")
                            .GetField("datable")
                             .SetValue(n, f);
                        goto NPCoverride; //either its a goto or ANOTHER flag. It just reads better for me using goto, and the label is pretty clear on what it does.

                    }
                }

                if ((seed % x > x / 2) && (data.Value.Age == NpcAge.Adult)) //Adults only! 
                {
                    //Seed mod x must be greater than all the possible values.
                    //(not quite this in retrospect but it works)
                    can = true;
                    noCanidates = false;
                    Assembly.GetAssembly(typeof(NPC))
                    .GetType("StardewValley.NPC")
                   .GetField("datable")
                    .SetValue(n, t);

                }
                else
                {

                    can = false;
                    Assembly.GetAssembly(typeof(NPC))
                         .GetType("StardewValley.NPC")
                        .GetField("datable")
                         .SetValue(n, f);
                }

            NPCoverride:

                data.Value.CanBeRomanced = can;

                canRomance.TryAdd(data.Key, can);
                x++;

            }


            if (noCanidates)
            {
                doRomance(++seed);
            }


        }


        static void addStack(String id, Chest inv, int amount)  // adds to a stack
        {
            if (inv.GetActualCapacity() <= inv.Items.Count) // if the count is equal to or larger than count
            {
                //Console.WriteLine("false return" + inv.ItemId + inv.TileLocation);
                return;
            }
            if (inv.Items.ContainsId(id))  //if it contains the id
            {
                bool didAdd = false;
                foreach (Item i in inv.Items.GetById(id))
                {
                    if (i.getRemainingStackSpace() < amount)  //if not enough space
                    {
                        //Console.WriteLine("Not enough space" + inv.ItemId + inv.TileLocation);
                        continue;
                    }
                    else
                    {
                        didAdd = true;  //then add it
                        i.Stack += amount;
                        break;
                    }
                }
                if (!didAdd)
                {


                    inv.addItem(new StardewValley.Object(id, amount));
                }
            }
            else
            {

                inv.addItem(new StardewValley.Object(id, amount));
            }
        }

        //DIrection chests
        static void InitiateDirectionChests()
        {
            index = -1;
            DCpairs = new Dictionary<Chest, DCpair>();


            int x = 0;
            int y = 0;

            foreach (GameLocation loc in Game1.locations)  // for every game location
            {
                foreach (StardewValley.Object o in loc.Objects.Values)
                {

                    if (o.ItemId.Contains("DirectionChest")) //if the object in the tile is a direction chest
                    {
                        x = (int)o.TileLocation.X;
                        y = (int)o.TileLocation.Y;
                        Chest DC = (Chest)o;
                        Vector2 upv = new Vector2(x, y + 1);
                        Vector2 downv = new Vector2(x, y - 1);
                        Vector2 leftv = new Vector2(x - 1, y);
                        Vector2 rightv = new Vector2(x + 1, y);

                        // Does the up and down tiles have an object? Or do the left and right tiles have an object?
                        if ((!loc.CanItemBePlacedHere(upv) && !loc.CanItemBePlacedHere(downv)) || (!loc.CanItemBePlacedHere(leftv) && !loc.CanItemBePlacedHere(rightv)))
                        {
                            StardewValley.Object donor = null;
                            StardewValley.Object rec = null;

                            switch (DC.ItemId)  //find the first adjacent chests.
                            {
                                case "DirectionChestLeft":
                                    donor = DC.Location.getObjectAtTile((int)x + 1, (int)y);
                                    rec = DC.Location.getObjectAtTile((int)x - 1, (int)y);
                                    break;
                                case "DirectionChestRight":
                                    donor = DC.Location.getObjectAtTile((int)x - 1, (int)y);
                                    rec = DC.Location.getObjectAtTile((int)x + 1, (int)y);
                                    break;
                                case "DirectionChestUp":
                                    donor = DC.Location.getObjectAtTile((int)x, (int)y + 1);
                                    rec = DC.Location.getObjectAtTile((int)x, (int)y - 1);
                                    break;
                                case "DirectionChestDown":
                                    donor = DC.Location.getObjectAtTile((int)x, (int)y - 1);
                                    rec = DC.Location.getObjectAtTile((int)x, (int)y + 1);
                                    break;

                            }

                            if (donor != null && rec != null && donor.ItemId.Contains("DirectionChest") && rec.ItemId.Contains("DirectionChest")) // if both donor and rec are direction chests, then the given chest is on a path and is not counted.
                            {
                                continue;
                            }
                            else
                            {

                                startTransfer(DC, donor, rec); //else, try to start a transfer.
                            }

                        }

                    }
                }
            }


            listP1 = new Dictionary<Chest, DCpair>();
            listP2 = new Dictionary<Chest, DCpair>();
            listP3 = new Dictionary<Chest, DCpair>();
            listP4 = new Dictionary<Chest, DCpair>();

            foreach (KeyValuePair<Chest, DCpair> pair in DCpairs.ToList())
            {
                if (pair.Value.rec != null && pair.Value.donor != null)
                {
                    listP4.Add(pair.Key, pair.Value);
                }
                else if (pair.Value.recM != null && pair.Value.donorM != null)
                {
                    listP1.Add(pair.Key, pair.Value);

                }
                else if (pair.Value.rec != null && pair.Value.donorM != null)
                {
                    listP3.Add(pair.Key, pair.Value);
                }
                else if (pair.Value.recM != null && pair.Value.donor != null)
                {
                    listP2.Add(pair.Key, pair.Value);
                }

            }




            index = 0;



        }
        static public void startTransfer(Chest DC, StardewValley.Object donor, StardewValley.Object rec) // does a bunch of things except trasnfering
        {
            int uhoh = 0;
            float x = DC.TileLocation.X;
            float y = DC.TileLocation.Y;
            List<Chest> list = new List<Chest>();

            if (rec == null || donor == null)
            {
                return;
            }


            switch (DC.ItemId)  //  find all those sweet signal chests and add them to the list
            {
                case "DirectionChestLeft":

                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x, (int)y + 1)) && rec.Location.isTileOnMap(new Vector2((int)x, (int)y + 1)))
                    {
                        if (rec.Location.getObjectAtTile((int)x, (int)y + 1) != null && rec.Location.getObjectAtTile((int)x, (int)y + 1).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)donor.Location.getObjectAtTile((int)x, (int)y + 1)));
                        }
                    }
                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x, (int)y - 1)) && rec.Location.isTileOnMap(new Vector2((int)x, (int)y - 1)))
                    {
                        if (rec.Location.getObjectAtTile((int)x, (int)y - 1) != null && rec.Location.getObjectAtTile((int)x, (int)y - 1).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)rec.Location.getObjectAtTile((int)x, (int)y - 1)));
                        }
                    }
                    break;
                case "DirectionChestRight":

                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x, (int)y + 1)) && rec.Location.isTileOnMap(new Vector2((int)x, (int)y + 1)))
                    {
                        if (rec.Location.getObjectAtTile((int)x, (int)y + 1) != null && rec.Location.getObjectAtTile((int)x, (int)y + 1).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)donor.Location.getObjectAtTile((int)x, (int)y + 1)));
                        }
                    }
                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x, (int)y - 1)) && rec.Location.isTileOnMap(new Vector2((int)x, (int)y - 1)))
                    {
                        if (rec.Location.getObjectAtTile((int)x, (int)y - 1) != null && rec.Location.getObjectAtTile((int)x, (int)y - 1).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)rec.Location.getObjectAtTile((int)x, (int)y - 1)));
                        }
                    }
                    break;
                case "DirectionChestUp":

                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x - 1, (int)y)) && rec.Location.isTileOnMap(new Vector2((int)x - 1, (int)y)))
                    {
                        if (rec.Location.getObjectAtTile((int)x - 1, (int)y) != null && rec.Location.getObjectAtTile((int)x - 1, (int)y).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)donor.Location.getObjectAtTile((int)x - 1, (int)y)));
                        }
                    }
                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x + 1, (int)y)) && rec.Location.isTileOnMap(new Vector2((int)x + 1, (int)y)))
                    {
                        if (rec.Location.getObjectAtTile((int)x + 1, (int)y) != null && rec.Location.getObjectAtTile((int)x + 1, (int)y).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)rec.Location.getObjectAtTile((int)x + 1, (int)y)));
                        }
                    }
                    break;
                case "DirectionChestDown":

                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x - 1, (int)y)) && rec.Location.isTileOnMap(new Vector2((int)x - 1, (int)y)) && rec.Location.getObjectAtTile((int)x - 1, (int)y) != null)
                    {
                        if (rec.Location.getObjectAtTile((int)x - 1, (int)y) != null && rec.Location.getObjectAtTile((int)x - 1, (int)y).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)donor.Location.getObjectAtTile((int)x - 1, (int)y)));
                        }
                    }
                    if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x + 1, (int)y)) && rec.Location.isTileOnMap(new Vector2((int)x + 1, (int)y)) && rec.Location.getObjectAtTile((int)x + 1, (int)y) != null)
                    {
                        if (rec.Location.getObjectAtTile((int)x + 1, (int)y) != null && rec.Location.getObjectAtTile((int)x + 1, (int)y).ItemId == "SignalChest")
                        {
                            list.Add(((Chest)rec.Location.getObjectAtTile((int)x + 1, (int)y)));
                        }
                    }
                    break;

            }



            while (donor.ItemId.Contains("DirectionChest"))  //find the parent donor obj if it exists
            {
                float x1 = donor.TileLocation.X;
                float y1 = donor.tileLocation.Y;


                switch (donor.ItemId)
                {

                    case "DirectionChestLeft":
                        if (donor.Location.CanItemBePlacedHere(new Vector2((int)x1 + 1, (int)y1)) || !donor.Location.isTileOnMap(new Vector2((int)x1 + 1, (int)y1)))
                        {
                            return;  // if oob or no chest is there
                        }

                        donor = donor.Location.getObjectAtTile((int)x1 + 1, (int)y1);
                        if (donor == null)
                        {
                            return;
                        }

                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 + 1)) && donor.Location.isTileOnMap(new Vector2((int)x1, (int)y1 + 1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1, (int)y1 + 1) != null && donor.Location.getObjectAtTile((int)x1, (int)y1 + 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1, (int)y1 + 1)));
                            }
                        }
                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 - 1)) && donor.Location.isTileOnMap(new Vector2((int)x1, (int)y1 - 1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1, (int)y1 - 1) != null && donor.Location.getObjectAtTile((int)x1, (int)y1 - 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1, (int)y1 - 1)));
                            }
                        }


                        break;
                    case "DirectionChestRight":
                        if (donor.Location.CanItemBePlacedHere(new Vector2((int)x1 - 1, (int)y1)) || !donor.Location.isTileOnMap(new Vector2((int)x1 - 1, (int)y1)))
                        {
                            return;
                        }

                        donor = donor.Location.getObjectAtTile((int)x1 - 1, (int)y1);
                        if (donor == null)
                        {
                            return;
                        }


                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 + 1)) && donor.Location.isTileOnMap(new Vector2((int)x1, (int)y1 + 1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1, (int)y1 + 1) != null && donor.Location.getObjectAtTile((int)x1, (int)y1 + 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1, (int)y1 + 1)));
                            }
                        }
                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 - 1)) && donor.Location.isTileOnMap(new Vector2((int)x1, (int)y1 - 1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1, (int)y1 - 1) != null && donor.Location.getObjectAtTile((int)x1, (int)y1 - 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1, (int)y1 - 1)));
                            }
                        }
                        break;
                    case "DirectionChestUp":
                        if (donor.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 + 1)) || !donor.Location.isTileOnMap(new Vector2((int)x1, (int)y1 + 1)))
                        {
                            return;
                        }


                        donor = donor.Location.getObjectAtTile((int)x1, (int)y1 + 1);
                        if (donor == null)
                        {
                            return;
                        }



                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1 + 1, (int)y1)) && donor.Location.isTileOnMap(new Vector2((int)x1 + 1, (int)y1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1 + 1, (int)y1) != null && donor.Location.getObjectAtTile((int)x1 + 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1 + 1, (int)y1)));
                            }
                        }
                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1 - 1, (int)y1)) && donor.Location.isTileOnMap(new Vector2((int)x1 - 1, (int)y1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1 - 1, (int)y1) != null && donor.Location.getObjectAtTile((int)x1 - 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1 - 1, (int)y1)));
                            }
                        }

                        break;
                    case "DirectionChestDown":
                        if (donor.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 - 1)) || !donor.Location.isTileOnMap(new Vector2((int)x1, (int)y1 - 1)))
                        {
                            return;
                        }

                        donor = donor.Location.getObjectAtTile((int)x1, (int)y1 - 1);
                        if (donor == null)
                        {
                            return;
                        }


                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1 + 1, (int)y1)) && donor.Location.isTileOnMap(new Vector2((int)x1 + 1, (int)y1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1 + 1, (int)y1) != null && donor.Location.getObjectAtTile((int)x1 + 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1 + 1, (int)y1)));
                            }
                        }
                        if (!donor.Location.CanItemBePlacedHere(new Vector2((int)x1 - 1, (int)y1)) && donor.Location.isTileOnMap(new Vector2((int)x1 - 1, (int)y1)))
                        {
                            if (donor.Location.getObjectAtTile((int)x1 - 1, (int)y1) != null && donor.Location.getObjectAtTile((int)x1 - 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1 - 1, (int)y1)));
                            }
                        }
                        break;

                }
                if (++uhoh > 100)
                {

                    return;
                }
            }






            uhoh = 0;


            while (rec.ItemId.Contains("DirectionChest"))  // find parent rec if it exists
            {
                float x1 = rec.TileLocation.X;
                float y1 = rec.tileLocation.Y;

                switch (rec.ItemId)
                {
                    case "DirectionChestLeft":
                        if (rec.Location.CanItemBePlacedHere(new Vector2((int)x1 - 1, (int)y1)) || !rec.Location.isTileOnMap(new Vector2((int)x1 - 1, (int)y1)))
                        {
                            return;
                        }

                        rec = rec.Location.getObjectAtTile((int)x1 - 1, (int)y1);
                        if (rec == null)
                        {
                            return;
                        }


                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 + 1)) && rec.Location.isTileOnMap(new Vector2((int)x1, (int)y1 + 1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1, (int)y1 + 1) != null && rec.Location.getObjectAtTile((int)x1, (int)y1 + 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1, (int)y1 + 1)));
                            }
                        }
                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 - 1)) && rec.Location.isTileOnMap(new Vector2((int)x1, (int)y1 - 1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1, (int)y1 - 1) != null && rec.Location.getObjectAtTile((int)x1, (int)y1 - 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)rec.Location.getObjectAtTile((int)x1, (int)y1 - 1)));
                            }
                        }
                        break;
                    case "DirectionChestRight":
                        if (rec.Location.CanItemBePlacedHere(new Vector2((int)x1 + 1, (int)y1)) || !rec.Location.isTileOnMap(new Vector2((int)x1 + 1, (int)y1)))
                        {
                            return;
                        }

                        rec = rec.Location.getObjectAtTile((int)x1 + 1, (int)y1);
                        if (rec == null)
                        {
                            return;
                        }


                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 + 1)) && rec.Location.isTileOnMap(new Vector2((int)x1, (int)y1 + 1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1, (int)y1 + 1) != null && rec.Location.getObjectAtTile((int)x1, (int)y1 + 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1, (int)y1 + 1)));
                            }
                        }
                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 - 1)) && rec.Location.isTileOnMap(new Vector2((int)x1, (int)y1 - 1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1, (int)y1 - 1) != null && rec.Location.getObjectAtTile((int)x1, (int)y1 - 1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)rec.Location.getObjectAtTile((int)x1, (int)y1 - 1)));
                            }
                        }
                        break;
                    case "DirectionChestUp":
                        if (rec.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 - 1)) || !rec.Location.isTileOnMap(new Vector2((int)x1, (int)y1 - 1)))
                        {
                            return;
                        }

                        rec = rec.Location.getObjectAtTile((int)x1, (int)y1 - 1);
                        if (rec == null)
                        {
                            return;
                        }




                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1 - 1, (int)y1)) && rec.Location.isTileOnMap(new Vector2((int)x1 - 1, (int)y1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1 - 1, (int)y1) != null && rec.Location.getObjectAtTile((int)x1 - 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1 - 1, (int)y1)));
                            }
                        }
                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1 + 1, (int)y1)) && rec.Location.isTileOnMap(new Vector2((int)x1 + 1, (int)y1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1 + 1, (int)y1) != null && rec.Location.getObjectAtTile((int)x1 + 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)rec.Location.getObjectAtTile((int)x1 + 1, (int)y1)));
                            }
                        }
                        break;
                    case "DirectionChestDown":
                        if (rec.Location.CanItemBePlacedHere(new Vector2((int)x1, (int)y1 + 1)) || !rec.Location.isTileOnMap(new Vector2((int)x1, (int)y1 + 1)))
                        {
                            return;
                        }

                        rec = rec.Location.getObjectAtTile((int)x1, (int)y1 + 1);
                        if (rec == null)
                        {
                            return;
                        }


                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1 - 1, (int)y1)) && rec.Location.isTileOnMap(new Vector2((int)x1 - 1, (int)y1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1 - 1, (int)y1) != null && rec.Location.getObjectAtTile((int)x1 - 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)donor.Location.getObjectAtTile((int)x1 - 1, (int)y1)));
                            }
                        }
                        if (!rec.Location.CanItemBePlacedHere(new Vector2((int)x1 + 1, (int)y1)) && rec.Location.isTileOnMap(new Vector2((int)x1 + 1, (int)y1)))
                        {
                            if (rec.Location.getObjectAtTile((int)x1 + 1, (int)y1) != null && rec.Location.getObjectAtTile((int)x1 + 1, (int)y1).ItemId == "SignalChest")
                            {
                                list.Add(((Chest)rec.Location.getObjectAtTile((int)x1 + 1, (int)y1)));
                            }
                        }
                        break;

                }

                if (++uhoh > 100)
                {

                    return;
                }
            }



            if (rec == null || donor == null)
            {

                return;
            }






            foreach (Chest c in list)  // removes duplicates in list
            {
                Predicate<Chest> pre = delegate (Chest a) { return c.TileLocation == a.TileLocation; };
                if (list.FindAll(pre).Count > 1)
                {
                    list.Remove(c);
                }
            }






            if (rec as Chest != null && donor as Chest != null)
            {
                DCpair d = new DCpair();
                d.rec = (Chest)rec;
                d.donor = (Chest)donor;
                d.Signals = list;
                DCpairs.Add(DC, d);

            }
            else if (rec.GetMachineData() != null && donor.GetMachineData() != null)
            {

                DCpair d = new DCpair();
                d.recM = rec;
                d.donorM = donor;
                d.Signals = list;
                DCpairs.Add(DC, d);


            }
            else if (rec.GetMachineData() != null && donor as Chest != null)
            {
                DCpair d = new DCpair();
                d.recM = rec;
                d.donor = (Chest)donor;
                d.Signals = list;
                DCpairs.Add(DC, d);


            }
            else if (rec as Chest != null && donor.GetMachineData() != null)
            {

                DCpair d = new DCpair();
                d.rec = (Chest)rec;
                d.donorM = donor;
                d.Signals = list;
                DCpairs.Add(DC, d);


            }

        }


        static bool checkForSignal(Chest DC)
        {
            if (DCpairs[DC].Signals.Count == 0)
            {
                return true;
            }



            foreach (Chest signal in DCpairs[DC].Signals)
            {
                bool hasSignal = false;

                //Console.WriteLine(signal.TileLocation);
                foreach (Item i in signal.Items)
                {
                    //Console.WriteLine("Looking for signal at "+i.itemId);
                    if (i.ItemId == "SIGNAL")
                    {
                        hasSignal = true;
                    }
                }
                if (!hasSignal)
                {
                    return false;
                }
            }
            return true;
        }
        static void DotransferAdj(KeyValuePair<Chest, DCpair> pair)
        {
            Chest center = pair.Value.donor;
            int x = (int)center.TileLocation.X;
            int y = (int)center.TileLocation.Y;
            GameLocation loc = center.Location;
            StardewValley.Object north = null;
            StardewValley.Object south = null;
            StardewValley.Object east = null;
            StardewValley.Object west = null;

            int num = 0;

            if (loc.getObjectAtTile(x, y - 1) != null && loc.getObjectAtTile(x, y - 2) != null && loc.getObjectAtTile(x, y - 1).ItemId.Contains("Up") && (loc.getObjectAtTile(x, y - 2).GetMachineData() != null || loc.getObjectAtTile(x, y - 2) as Chest != null))
            {
                north = loc.getObjectAtTile(x, y - 2);
                num++;
            }
            if (loc.getObjectAtTile(x, y + 1) != null && loc.getObjectAtTile(x, y + 2) != null && loc.getObjectAtTile(x, y + 1).ItemId.Contains("Down") && (loc.getObjectAtTile(x, y + 2).GetMachineData() != null || loc.getObjectAtTile(x, y + 2) as Chest != null))
            {
                south = loc.getObjectAtTile(x, y + 2);
                num++;
            }
            if (loc.getObjectAtTile(x + 1, y) != null && loc.getObjectAtTile(x + 2, y) != null && loc.getObjectAtTile(x + 1, y).ItemId.Contains("Right") && (loc.getObjectAtTile(x + 2, y).GetMachineData() != null || loc.getObjectAtTile(x + 2, y) as Chest != null))
            {
                east = loc.getObjectAtTile(x + 2, y);
                num++;
            }
            if (loc.getObjectAtTile(x - 1, y) != null && loc.getObjectAtTile(x - 2, y) != null && loc.getObjectAtTile(x - 1, y).ItemId.Contains("Left") && (loc.getObjectAtTile(x - 2, y).GetMachineData() != null || loc.getObjectAtTile(x - 2, y) as Chest != null))
            {
                west = loc.getObjectAtTile(x - 2, y);
                num++;
            }
            /*
            if (north!= null && (!north.ItemId.Contains("Direction")))
            {
                
                //Console.WriteLine("north:"+center.TileLocation + "-->" + north.TileLocation);
                if (loc.getObjectAtTile(x, y - 2).GetMachineData() != null)
                {
                    
                    transfer3(loc.getObjectAtTile(x, y - 1) as Chest, center, north);
                }
                else
                {
                    transfer1(loc.getObjectAtTile(x, y - 1) as Chest, center, north as Chest);
                }
            }
            */
            if (south != null && (!south.ItemId.Contains("Direction")))
            {
                //Console.WriteLine("south:" + center.TileLocation + "-->" + south.TileLocation);
                if ((loc.getObjectAtTile(x, y + 2).GetMachineData() != null))
                {
                    transfer3(loc.getObjectAtTile(x, y + 1) as Chest, center, south);
                }
                else
                {
                    transfer1(loc.getObjectAtTile(x, y + 1) as Chest, center, south as Chest);

                }

                if (east != null && (!east.ItemId.Contains("Direction")))
                {
                    //Console.WriteLine("east:" + center.TileLocation + "-->" + east.TileLocation);
                    if (loc.getObjectAtTile(x + 2, y).GetMachineData() != null)
                    {
                        transfer3(loc.getObjectAtTile(x + 1, y) as Chest, center, east);
                    }
                    else
                    {
                        transfer1(loc.getObjectAtTile(x + 1, y) as Chest, center, east as Chest);
                    }
                }

                if (west != null && (!west.ItemId.Contains("Direction")))
                {
                    // Console.WriteLine("west:" + center.TileLocation + "-->" + west.TileLocation);
                    if (loc.getObjectAtTile(x - 2, y).GetMachineData() != null)
                    {
                        transfer3(loc.getObjectAtTile(x - 1, y) as Chest, center, west);
                    }
                    else
                    {
                        transfer1(loc.getObjectAtTile(x - 1, y) as Chest, center, west as Chest);
                    }
                }

            }
        }
            static void DoTransfers(KeyValuePair<Chest, DCpair> pair)
            {

                if (pair.Value.recM != null && pair.Value.donorM != null)
                {
                    //Console.WriteLine("obj->obj");
                    //Console.WriteLine("Rec:" + pair.Value.recM.tileLocation.X + " " + pair.Value.recM.tileLocation.Y);
                    //Console.WriteLine("Don:" + pair.Value.donorM.tileLocation.X + " " + pair.Value.donorM.tileLocation.Y);


                    transfer2(pair.Key, pair.Value.donorM, pair.Value.recM);
                }
                else if (pair.Value.recM != null && pair.Value.donor != null)
                {
                    // Console.WriteLine("chest->obj");
                    // Console.WriteLine("Rec:" + pair.Value.recM.tileLocation.X + " " + pair.Value.recM.tileLocation.Y);
                    // Console.WriteLine("Don:" + pair.Value.donor.tileLocation.X + " " + pair.Value.donor.tileLocation.Y);
                    transfer3(pair.Key, pair.Value.donor, pair.Value.recM);
                }
                else if (pair.Value.rec != null && pair.Value.donorM != null)
                {
                    //  Console.WriteLine("obj->chest");
                    //  Console.WriteLine("Rec:" + pair.Value.rec.tileLocation.X + " " + pair.Value.rec.tileLocation.Y);
                    //   Console.WriteLine("Don:" + pair.Value.donorM.tileLocation.X + " " + pair.Value.donorM.tileLocation.Y);
                    transfer4(pair.Key, pair.Value.donorM, pair.Value.rec);
                }

                else if (pair.Value.rec != null && pair.Value.donor != null)
                {
                    //   Console.WriteLine("chest->chest");
                    //  Console.WriteLine("Rec:" + pair.Value.rec.tileLocation.X + " " + pair.Value.rec.tileLocation.Y);
                    //   Console.WriteLine("Don:" + pair.Value.donor.tileLocation.X + " " + pair.Value.donor.tileLocation.Y);
                    // buugy code DotransferAdj(pair);
                    transfer1(pair.Key, pair.Value.donor, pair.Value.rec);

                }
                //  Console.WriteLine("---------");
            }

            static void DoTransfers(Dictionary<Chest, DCpair> dict)
            {
                foreach (KeyValuePair<Chest, DCpair> pair in dict)
                {
                    DoTransfers(pair);
                }
            }

            static void transfer1(Chest DC, Chest d, Chest r) //object transfer based on 2 chests
            {


                if (checkForSignal(DC) == false)  // if there is a signal chest with no signals, return.
                {

                    return;
                }
                Inventory inv = new Inventory();
                foreach (StardewValley.Object i in d.GetItemsForPlayer())
                {
                    if (i != null)
                    {
                        inv.Add(i);
                        //Console.WriteLine("Item i: " + i.Name + d.TileLocation);
                    }
                }



                foreach (StardewValley.Item obj in inv)
                {

                    foreach (StardewValley.Item item in DC.Items)
                    {

                        if (obj != null && item != null && r.Items.Count < r.GetActualCapacity() && (item.ItemId == obj.ItemId || DC.Items.ContainsId("SIGNAL")) && obj.Stack >= item.Stack)
                        {
                            if (obj as ColoredObject != null)
                            {

                                Farmer f = new Farmer();
                                d.GetItemsForPlayer().Reduce(obj, item.Stack);

                                addStack(obj.ItemId, r, item.Stack);

                            }
                            else
                            {


                                Farmer f = new Farmer();
                                d.GetItemsForPlayer().Reduce(obj, item.Stack);

                                addStack(obj.ItemId, r, item.Stack);

                            }


                        }
                    }


                }
            }

            static void transfer2(Chest DC, StardewValley.Object d, StardewValley.Object r)
            {

                if (checkForSignal(DC) == false)
                {
                    return;
                }

                Farmer f = new Farmer();
                f.Items.Add(d.heldObject.Value);
                if (d.heldObject.Value != null && d.MinutesUntilReady == 0 && r.MinutesUntilReady == 0 && d.readyForHarvest.Value && !r.readyForHarvest.Value && r.heldObject.Value == null)
                {
                    StardewValley.Object o = new StardewValley.Object();
                    d.heldObject.Value.DeepCloneTo(o);
                    d.heldObject.Value = null;
                    if (!MachineDataUtility.HasAdditionalRequirements(f.Items, r.GetMachineData().AdditionalConsumedItems, out var failedRequirement))
                    {
                        return;  // if the additional requirements for the machine recipe is not fuffilled
                    }
                    MachineOutputRule output = null;
                    MachineOutputTriggerRule outputrule = null;
                    MachineOutputRule ignorecount = null;
                    MachineOutputTriggerRule ignoreoutputrule = null;

                    //here is my little if statement pit. its better than having it in one big line so it exits.
                    //blah blah blah, check if the machine output rule for the item exists

                    if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.None, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                    {
                        if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.DayUpdate, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                        {
                            if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.ItemPlacedInMachine, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                            {
                                if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.MachinePutDown, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                                {
                                    if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.OutputCollected, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                                    {

                                        return;
                                    }
                                }
                            }
                        }

                    }
                    if (o.Stack < output.Triggers[0].RequiredCount)
                    {
                        return;
                    }
                    r.PlaceInMachine(r.GetMachineData(), o, false, f);
                }



            }

            static void transfer3(Chest DC, Chest d, StardewValley.Object r)
            {

                if (checkForSignal(DC) == false)
                {
                    return;
                }






                Farmer f = new Farmer();
                f.Items.AddRange(d.Items);
                foreach (StardewValley.Item o in d.Items)
                {
                    foreach (StardewValley.Item DCo in DC.Items) // for each object in d that matches the object in DC chest, unless its a signal then we ignore this.
                    {

                        if (o == null || DCo == null || r.heldObject == null)
                        {
                            continue;
                        }


                        if ((o.ItemId == DCo.ItemId || DC.Items.ContainsId("SIGNAL")) && !r.readyForHarvest.Value && r.MinutesUntilReady == 0 && r.heldObject.Value == null)
                        {

                            MachineOutputRule output = null;
                            MachineOutputTriggerRule outputrule = null;
                            MachineOutputRule ignorecount = null;
                            MachineOutputTriggerRule ignoreoutputrule = null;  //blah blah blah, check if the machine output rule for the item exists
                            if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.None, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                            {
                                if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.DayUpdate, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                                {
                                    if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.ItemPlacedInMachine, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                                    {
                                        if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.MachinePutDown, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                                        {
                                            if (!MachineDataUtility.TryGetMachineOutputRule(r, r.GetMachineData(), MachineOutputTrigger.OutputCollected, o, f, DC.Location, out output, out outputrule, out ignorecount, out ignoreoutputrule))
                                            {

                                                continue;
                                            }
                                        }
                                    }
                                }

                            }


                            if (o.Stack < output.Triggers[0].RequiredCount)
                            {

                                continue;
                            }
                            if (r.GetMachineData().AdditionalConsumedItems != null)
                            {
                                foreach (MachineItemAdditionalConsumedItems item in r.GetMachineData().AdditionalConsumedItems) //check if the additional items needed is met.
                                {
                                    bool hasItem = false;
                                    if (o.ItemId == item.ItemId)
                                    {

                                        continue;
                                    }
                                    foreach (StardewValley.Item oi in d.Items)
                                    {
                                        if (oi.ItemId == item.ItemId && (oi.Stack >= item.RequiredCount))
                                        {
                                            hasItem = true;
                                        }

                                    }
                                    if (!hasItem)
                                    {

                                        continue;
                                    }
                                }
                            }
                            if (o.Stack == output.Triggers[0].RequiredCount)
                            {
                                r.PlaceInMachine(r.GetMachineData(), o, false, f);
                                d.Items.Reduce(o, output.Triggers[0].RequiredCount);

                                return;
                            }

                            r.PlaceInMachine(r.GetMachineData(), o, false, f);
                            return;
                        }
                        else if (r.heldObject.Value == null && r.MinutesUntilReady != 0)
                        {
                            r.MinutesUntilReady = 0;

                        }
                    }
                }


            }
            static void transfer4(Chest DC, StardewValley.Object d, Chest r)  //for a object -->chest
            {

                if (checkForSignal(DC) == false)
                {
                    return;
                }





                foreach (StardewValley.Object DCo in DC.Items) // for each object that matches the object in DC chest, unless its a signal then we ignore this.
                {


                    if (DCo != null && d.heldObject.Value != null && d.readyForHarvest.Value && r.Items.Count < r.GetActualCapacity() && (d.heldObject.Value.ItemId == DCo.ItemId || DC.Items.ContainsId("SIGNAL")))
                    {
                        var obj = d.heldObject.Value;
                        if (obj as ColoredObject != null)
                        {

                            StardewValley.Objects.ColoredObject o = new ColoredObject();
                            d.heldObject.Value.DeepCloneTo(o);  //remove the item
                            d.heldObject.Value = null;

                            addStack(o.ItemId, r, o.Stack); // add it

                        }
                        else
                        {
                            StardewValley.Object o = new StardewValley.Object();
                            d.heldObject.Value.DeepCloneTo(o);  //remove the item
                            d.heldObject.Value = null;

                            addStack(o.ItemId, r, o.Stack); // add it

                        }



                    }
                }
            }




            static void InitiateBuildingChests()  //Initiate the building chests
            {
                Bpairs = new Dictionary<Building, Chest>();
            
                foreach (GameLocation locatio in Game1.locations)  // for every game location
                {
                    Chest B;
                    // Console.WriteLine(" -->" + locatio);
                    foreach (Building b in locatio.buildings) //search the surrounding tiles for a chest
                    {
                    //Console.WriteLine("    :" + b.buildingType);
                    for (int x = b.tileX.Value; x < b.tilesWide.Value + b.tileX.Value; x++)
                        {

                            if (locatio.isObjectAtTile(x, b.tileY.Value - 1) && locatio.getObjectAtTile(x, b.tileY.Value - 1).ItemId == "130")
                            {
                                B = (Chest)locatio.getObjectAtTile(x, b.tileY.Value - 1);
                                Bpairs.Add(b, B);
                                transferB(b, B);
                                return;
                            }
                            if (locatio.isObjectAtTile(x, b.tileY.Value + b.tilesHigh.Value) && locatio.getObjectAtTile(x, b.tileY.Value + b.tilesHigh.Value).ItemId == ("130"))
                            {
                                B = (Chest)locatio.getObjectAtTile(x, b.tileY.Value + b.tilesHigh.Value);
                                Bpairs.Add(b, B);
                                transferB(b, B);
                                return;
                            }
                        }
                        for (int y = b.tileY.Value; y < b.tileY.Value + b.tilesHigh.Value; y++)
                        {

                            if (locatio.isObjectAtTile(b.tileX.Value - 1, y) && locatio.getObjectAtTile(b.tileX.Value - 1, y).ItemId == "130")
                            {
                                B = (Chest)locatio.getObjectAtTile(b.tileX.Value - 1, y);
                                Bpairs.Add(b, B);
                                transferB(b, B);
                                return;
                            }
                            if (locatio.isObjectAtTile(b.tilesWide.Value + b.tileX.Value, y) && locatio.getObjectAtTile(b.tilesWide.Value + b.tileX.Value, y).ItemId == ("130"))
                            {
                                B = (Chest)locatio.getObjectAtTile(b.tilesWide.Value + b.tileX.Value, y);
                                Bpairs.Add(b, B);
                                transferB(b, B);
                                return;
                            }
                        }
                    }
                }
            }

            static void transferB(StardewValley.Buildings.Building build, Chest chst)  // transfer items from a building to the chest.
            {
            
                if (build is StardewValley.Buildings.FishPond fish)
                {
                    if (fish.FishCount > 2)
                    {
                        fish.CatchFish();
                        chst.addItem(fish.GetFishObject());
                    }
                    if (fish.output.Value != null)
                    {
                        Item o = fish.output.Value;
                        fish.output.Value.DeepCloneTo(o);
                        fish.output.Value = null;
                        chst.addItem(o);
                    }
                }

            }


            static void InitiateDestroyers() //Initaiate destroyers!
            {
                Destroyers.Clear();
                foreach (GameLocation loc in Game1.locations)
                {
                    foreach (StardewValley.Object obj in loc.Objects.Values)
                    {
                        if (obj.ItemId == "CropHarvester" || obj.ItemId == "OreDestroyer" || obj.ItemId == "WoodDestroyer")
                        {
                            Destroyers.Add((Chest)obj); //add the destroyers.
                        }

                    }
                }

                foreach (Chest d in Destroyers) //and run them!
                {
                    if (d.ItemId == "OreDestroyer")
                    {
                        OreDestroyerTransfer(d);
                    }
                    else if (d.ItemId == "WoodDestroyer")
                    {
                        WoodDestroyerTransfer(d);
                    }
                    else if (d.ItemId == "CropHarvester")
                    {
                        CropHarvesterTransfer(d);
                    }
                }
            }



            static void OreDestroyerTransfer(Chest ore)
            {
                float x = ore.tileLocation.X - 2;
                float y = ore.tileLocation.Y - 2;

                for (float y1 = y; y1 < y + 5; y1++)  // for a 5 by 5 area
                {
                    for (float x1 = x; x1 < x + 5; x1++)
                    {
                        if (ore.Location.hasTileAt((int)x1, (int)y1, "Back") && ore.Location.getObjectAtTile((int)x1, (int)y1) != null) // if has a tile
                        {
                            StardewValley.Object o = ore.Location.getObjectAtTile((int)x1, (int)y1);
                            if (o.isDebrisOrForage()) //if its a forage item
                            {
                                switch (o.ItemId)  // look at every single rock and give the proper items.
                                {
                                    case "2":
                                        addStack("72", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "4":
                                        addStack("64", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "6":
                                        addStack("70", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "8":
                                        addStack("66", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "10":
                                        addStack("68", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "12":
                                        addStack("60", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "14":
                                        addStack("62", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "75":
                                        addStack("535", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "76":
                                        addStack("536", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "77":
                                        addStack("537", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "751":
                                        addStack("378", ore, 3);
                                        ore.Location.removeObject(o.TileLocation, false);

                                        break;
                                    case "290":
                                        addStack("380", ore, 3);
                                        ore.Location.removeObject(o.TileLocation, false);

                                        break;
                                    case "764":
                                        addStack("384", ore, 3);
                                        ore.Location.removeObject(o.TileLocation, false);

                                        break;
                                    case "765":
                                        addStack("386", ore, 3);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "46":
                                        addStack("74", ore, 1);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "BasicCoalNode0":
                                        addStack("382", ore, 3);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;
                                    case "BasicCoalNode1":
                                        addStack("382", ore, 3);
                                        ore.Location.removeObject(o.TileLocation, false);
                                        break;

                                }

                                if (o.ItemId == "32" || o.ItemId == "34" || o.ItemId == "36" || o.ItemId == "34" || o.ItemId == "36" || o.ItemId == "38" || o.ItemId == "40" || o.ItemId == "42" || o.ItemId == "343" || o.ItemId == "450" || o.ItemId == "668" || o.ItemId == "670" || o.ItemId == "760" || o.ItemId == "762")
                                {
                                    addStack("390", ore, 5);


                                    if (new Random().Next(0, 20) > 10)
                                    {
                                        addStack("382", ore, 3);
                                    }
                                    ore.Location.removeObject(o.TileLocation, false);
                                }
                            }
                        }
                    }
                }
            }

            static void WoodDestroyerTransfer(Chest ore)
            {
                float x = ore.tileLocation.X - 2;
                float y = ore.tileLocation.Y - 2;

                for (float y1 = y; y1 < y + 5; y1++)
                {
                    for (float x1 = x; x1 < x + 5; x1++)
                    {
                        Vector2 vect = new Vector2(((int)x1), ((int)y1));


                        if (ore.Location.terrainFeatures.TryGetValue(vect, out var value) && !value.isPassable() && value as Tree != null)
                        {
                            value.Location.removeEverythingFromThisTile((int)x1, (int)y1);
                            StardewValley.Object o = new StardewValley.Object("388", 30);
                            ore.addItem(o);
                        }
                    }
                }

            }


            static void CropHarvesterTransfer(Chest ore) // transfer crops to the chest
            {
                int x = (int)ore.tileLocation.X - 2;
                int y = (int)ore.tileLocation.Y - 2;

                for (int y1 = y; y1 < y + 5; y1++)
                {
                    for (int x1 = x; x1 < x + 5; x1++) // in a 5 by 5 area
                    {
                        Vector2 vect = new Vector2(((int)x1), ((int)y1));

                        TerrainFeature value;
                        //check if a crop is at the tile, if it exists, and if its ready to harvest (and if its hoedirt)
                        if (ore.Location.isCropAtTile((int)x1, (int)y1) && ore.Location.terrainFeatures.TryGetValue(vect, out value) && value as HoeDirt != null && (value as HoeDirt).readyForHarvest())
                        {
                            ore.Location.terrainFeatures.TryGetValue(vect, out value);
                            HoeDirt crop = value as HoeDirt;
                            StardewValley.Object obj = new StardewValley.Object(crop.crop.GetData().HarvestItemId, crop.crop.GetData().HarvestMinStack);
                            ore.addItem(obj); //add it 
                            crop.crop.harvest(x1, y1, crop); //remove it
                            Game1.player.removeItemFromInventory(obj);

                        }
                    }
                }

            }

            // NEW CHANGES
            private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
            {
                //misc events
                Misc = new Thread(new ParameterizedThreadStart(MiscEvents));
                Misc.Start();
                Music = new Thread(new ParameterizedThreadStart(OSTBPM));
                Music.Start();
                //data

                BPMData.initalize();
                BPM = BPMData.getData();








                //NEW OBJECTS

                //Direction chests

                Game1.bigCraftableData["130"].DeepCloneTo(dcup);
                dcup.Description = "Its a Direction chest! It moves items up.";
                dcup.Name = "Direction Chest (Up)";
                dcup.DisplayName = "Direction Chest (Up)";
                dcup.Texture = "Tilesheets/DirectionChests";
                dcup.SpriteIndex = 0;



                Game1.bigCraftableData["130"].DeepCloneTo(dcdown);
                dcdown.Description = "Its a Direction chest! It moves items down.";
                dcdown.Name = "Direction Chest (Down)";
                dcdown.DisplayName = "Direction Chest (Down)";
                dcdown.Texture = "Tilesheets/DirectionChests";
                dcdown.SpriteIndex = 7;


                Game1.bigCraftableData["130"].DeepCloneTo(dcleft);
                dcleft.Description = "Its a Direction chest! It moves items left.";
                dcleft.Name = "Direction Chest (Left)";
                dcleft.DisplayName = "Direction Chest (Left)";
                dcleft.Texture = "Tilesheets/DirectionChests";
                dcleft.SpriteIndex = 21;

                Game1.bigCraftableData["130"].DeepCloneTo(dcright);
                dcright.Description = "Its a Direction chest! It moves items right.";
                dcright.Name = "Direction Chest (Right)";
                dcright.DisplayName = "Direction Chest (Right)";
                dcright.Texture = "Tilesheets/DirectionChests";
                dcright.SpriteIndex = 14;

                Game1.bigCraftableData["130"].DeepCloneTo(signalC);
                signalC.Description = "Its a Signal chest! It stops direction chests if it doesnt have a signal.";
                signalC.Name = "Signal chest";
                signalC.DisplayName = "Signal chest";

                Game1.objectData["64"].DeepCloneTo(signal);
                signal.Description = "A Signal";
                signal.Name = "Signal";
                signal.DisplayName = "Signal";
                signal.Price = 0;



                //Destroyers


                Game1.bigCraftableData["130"].DeepCloneTo(OreDestroyer);
                OreDestroyer.Description = "Its an Ore Destroyer! It destroys rocks in a 5 by 5 area.";
                OreDestroyer.Name = "Ore Destroyer";
                OreDestroyer.DisplayName = "Ore Destroyer";

                Game1.bigCraftableData["130"].DeepCloneTo(WoodDestroyer);
                WoodDestroyer.Description = "Its a Wood Destroyer! It destroys and plants trees in a 5 by 5 area.";
                WoodDestroyer.Name = "Wood Destroyer";
                WoodDestroyer.DisplayName = "Wood Destroyer";

                Game1.bigCraftableData["130"].DeepCloneTo(CropHarvester);
                CropHarvester.Description = "Its a Crop Harvester! It harvests crops in a 5 by 5 area.";
                CropHarvester.Name = "Crop Harvester";
                CropHarvester.DisplayName = "Crop Harvester";

                // NEW CROPS

                /// BEER SEEDS
                /// Beer SEEDS CROPDATA

                Beer.Seasons = new List<Season> { Season.Spring, Season.Fall };

                Beer.DaysInPhase = new List<int> { 0, 0, 0, 0 };
                Beer.RegrowDays = -1;
                Beer.IsRaised = false;
                Beer.IsPaddyCrop = false;
                Beer.NeedsWatering = true;
                Beer.PlantableLocationRules = null;
                Beer.HarvestItemId = "346";
                Beer.HarvestMinStack = 1;
                Beer.HarvestMaxStack = 1;
                Beer.HarvestMaxIncreasePerFarmingLevel = 0f;
                Beer.ExtraHarvestChance = 0.0;
                Beer.HarvestMethod = HarvestMethod.Grab;
                Beer.HarvestMinQuality = 0;
                Beer.HarvestMaxQuality = null;
                Beer.Texture = "TileSheets\\crops";
                Beer.SpriteIndex = 37;
                Beer.CountForMonoculture = true;
                Beer.CountForPolyculture = true;
                Beer.CustomFields = null;

                /// BEERSEEDS. OBJ DATA
                Beerseeds.Name = "Beer Seeds";
                Beerseeds.DisplayName = "Beer Seeds";
                Beerseeds.Description = "Fuck it; who needs hops to get drunk? Plant in Fall or Spring.";
                Beerseeds.Type = "Seeds";
                Beerseeds.Category = -74;
                Beerseeds.Price = 20;
                Beerseeds.Texture = null;
                Beerseeds.SpriteIndex = 484;
                Beerseeds.Edibility = -300;
                Beerseeds.IsDrink = false;
                Beerseeds.Buffs = null;
                Beerseeds.GeodeDropsDefaultItems = false;
                Beerseeds.GeodeDrops = null;
                Beerseeds.ArtifactSpotChances = null;
                Beerseeds.CanBeGivenAsGift = true;
                Beerseeds.CanBeTrashed = true;
                Beerseeds.ExcludeFromFishingCollection = false;
                Beerseeds.ExcludeFromShippingCollection = false;
                Beerseeds.ExcludeFromRandomSale = false;
                Beerseeds.ContextTags = null;
                Beerseeds.CustomFields = null;

                ///BEERSEEDS SHOPDATA
                Beershop.ActionsOnPurchase = null;
                Beershop.CustomFields = null;
                Beershop.TradeItemId = null;
                Beershop.TradeItemAmount = 1;
                Beershop.Price = 210;
                Beershop.ApplyProfitMargins = null;
                Beershop.AvailableStock = 3;
                Beershop.AvailableStockLimit = LimitedStockMode.Global;
                Beershop.AvoidRepeat = false;
                Beershop.UseObjectDataPrice = false;
                Beershop.IgnoreShopPriceModifiers = true;
                Beershop.PriceModifiers = null;
                Beershop.PriceModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Beershop.AvailableStockModifiers = null;
                Beershop.AvailableStockModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Beershop.Condition = "SEASON spring";
                Beershop.Id = "(O)BEERSEEDS";
                Beershop.ItemId = "(O)BEERSEEDS";
                Beershop.RandomItemId = null;
                Beershop.MaxItems = null;
                Beershop.MinStack = -1;
                Beershop.MaxStack = -1;
                Beershop.Quality = -1;
                Beershop.ObjectInternalName = null;
                Beershop.ObjectDisplayName = null;
                Beershop.ToolUpgradeLevel = -1;
                Beershop.IsRecipe = false;
                Beershop.StackModifiers = null;
                Beershop.StackModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Beershop.QualityModifiers = null;
                Beershop.QualityModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Beershop.ModData = null;
                Beershop.PerItemCondition = null;

                ///*
                ///Cat bulb
                ///
                ///CATBULB OBJ
                CATBULB.Name = "Catbulb";
                CATBULB.DisplayName = "Catbulb";
                CATBULB.Description = "Looks cute.";
                CATBULB.Type = "Basic";
                CATBULB.Category = -75;
                CATBULB.Price = 20;
                CATBULB.Texture = null;
                CATBULB.SpriteIndex = 936;
                CATBULB.Edibility = 1;
                CATBULB.IsDrink = false;
                CATBULB.Buffs = null;
                CATBULB.GeodeDropsDefaultItems = false;
                CATBULB.GeodeDrops = null;
                CATBULB.ArtifactSpotChances = null;
                CATBULB.CanBeGivenAsGift = true;
                CATBULB.CanBeTrashed = true;
                CATBULB.ExcludeFromFishingCollection = false;
                CATBULB.ExcludeFromShippingCollection = false;
                CATBULB.ExcludeFromRandomSale = false;
                CATBULB.ContextTags = null;
                CATBULB.CustomFields = null;

                /// CATBULB SEEDS

                CATBULBSEEDS.Name = "Catbulb Seeds";
                CATBULBSEEDS.DisplayName = "Catbulb Seeds";
                CATBULBSEEDS.Description = "Creates a catbulb, which can be used as slingshot ammo. Sells for absolutely nothing when grown. Grows in 3.";
                CATBULBSEEDS.Type = "Seeds";
                CATBULBSEEDS.Category = -74;
                CATBULBSEEDS.Price = 50;
                CATBULBSEEDS.Texture = null;
                CATBULBSEEDS.SpriteIndex = 937;
                CATBULBSEEDS.Edibility = -300;
                CATBULBSEEDS.IsDrink = false;
                CATBULBSEEDS.Buffs = null;
                CATBULBSEEDS.GeodeDropsDefaultItems = false;
                CATBULBSEEDS.GeodeDrops = null;
                CATBULBSEEDS.ArtifactSpotChances = null;
                CATBULBSEEDS.CanBeGivenAsGift = true;
                CATBULBSEEDS.CanBeTrashed = true;
                CATBULBSEEDS.ExcludeFromFishingCollection = false;
                CATBULBSEEDS.ExcludeFromShippingCollection = false;
                CATBULBSEEDS.ExcludeFromRandomSale = false;
                CATBULBSEEDS.ContextTags = null;
                CATBULBSEEDS.CustomFields = null;

                /// CATBULB CROP

                CATBULBCROP.Seasons = new List<Season> { Season.Spring, Season.Winter, Season.Fall, Season.Summer };

                CATBULBCROP.DaysInPhase = new List<int> { 1, 1, 1 };
                CATBULBCROP.RegrowDays = 1;
                CATBULBCROP.IsRaised = false;
                CATBULBCROP.IsPaddyCrop = false;
                CATBULBCROP.NeedsWatering = true;
                CATBULBCROP.PlantableLocationRules = null;
                CATBULBCROP.HarvestItemId = "CATBULB";
                CATBULBCROP.HarvestMinStack = 3;
                CATBULBCROP.HarvestMaxStack = 5;
                CATBULBCROP.HarvestMaxIncreasePerFarmingLevel = 0f;
                CATBULBCROP.ExtraHarvestChance = 0.0;
                CATBULBCROP.HarvestMethod = HarvestMethod.Grab;
                CATBULBCROP.HarvestMinQuality = 0;
                CATBULBCROP.HarvestMaxQuality = 0;
                CATBULBCROP.Texture = "TileSheets\\crops";
                CATBULBCROP.SpriteIndex = 52;
                CATBULBCROP.CountForMonoculture = true;
                CATBULBCROP.CountForPolyculture = true;
                CATBULBCROP.CustomFields = null;

                ///CATBULBSEEDSSHOP
                CATBULBSEEDSHOP.ActionsOnPurchase = null;
                CATBULBSEEDSHOP.CustomFields = null;
                CATBULBSEEDSHOP.TradeItemId = null;
                CATBULBSEEDSHOP.TradeItemAmount = 1;
                CATBULBSEEDSHOP.Price = 50;
                CATBULBSEEDSHOP.ApplyProfitMargins = null;
                CATBULBSEEDSHOP.AvailableStock = -1;
                CATBULBSEEDSHOP.AvailableStockLimit = LimitedStockMode.Global;
                CATBULBSEEDSHOP.AvoidRepeat = false;
                CATBULBSEEDSHOP.UseObjectDataPrice = false;
                CATBULBSEEDSHOP.IgnoreShopPriceModifiers = true;
                CATBULBSEEDSHOP.PriceModifiers = null;
                CATBULBSEEDSHOP.PriceModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                CATBULBSEEDSHOP.AvailableStockModifiers = null;
                CATBULBSEEDSHOP.AvailableStockModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                CATBULBSEEDSHOP.Condition = null;
                CATBULBSEEDSHOP.Id = "(O)CATBULBSEEDS";
                CATBULBSEEDSHOP.ItemId = "(O)CATBULBSEEDS";
                CATBULBSEEDSHOP.RandomItemId = null;
                CATBULBSEEDSHOP.MaxItems = null;
                CATBULBSEEDSHOP.MinStack = -1;
                CATBULBSEEDSHOP.MaxStack = -1;
                CATBULBSEEDSHOP.Quality = -1;
                CATBULBSEEDSHOP.ObjectInternalName = null;
                CATBULBSEEDSHOP.ObjectDisplayName = null;
                CATBULBSEEDSHOP.ToolUpgradeLevel = -1;
                CATBULBSEEDSHOP.IsRecipe = false;
                CATBULBSEEDSHOP.StackModifiers = null;
                CATBULBSEEDSHOP.StackModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                CATBULBSEEDSHOP.QualityModifiers = null;
                CATBULBSEEDSHOP.QualityModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                CATBULBSEEDSHOP.ModData = null;
                CATBULBSEEDSHOP.PerItemCondition = null;


                ///Bombseeds crop
                Bomb.Seasons = new List<Season> { Season.Summer, Season.Winter };
                Bomb.DaysInPhase = new List<int> { 1, 1, 1 };
                Bomb.RegrowDays = -1;
                Bomb.IsRaised = false;
                Bomb.IsPaddyCrop = false;
                Bomb.NeedsWatering = true;
                Bomb.PlantableLocationRules = null;
                Bomb.HarvestItemId = "287";
                Bomb.HarvestMinStack = 1;
                Bomb.HarvestMaxStack = 1;
                Bomb.HarvestMaxIncreasePerFarmingLevel = 0f;
                Bomb.ExtraHarvestChance = 0.0;
                Bomb.HarvestMethod = HarvestMethod.Grab;
                Bomb.HarvestMinQuality = 0;
                Bomb.HarvestMaxQuality = null;
                Bomb.Texture = "TileSheets\\crops";
                Bomb.SpriteIndex = 53;
                Bomb.CountForMonoculture = true;
                Bomb.CountForPolyculture = true;
                Bomb.CustomFields = null;

                ///Bombseeds 

                Bombseeds.Name = "Bomb Seeds";
                Bombseeds.DisplayName = "Bomb Seeds";
                Bombseeds.Description = "Easy way to get those rupees. Grows in summer or winter, and in 3 days.";
                Bombseeds.Type = "Seeds";
                Bombseeds.Category = -74;
                Bombseeds.Price = 20;
                Bombseeds.Texture = null;
                Bombseeds.SpriteIndex = 938;
                Bombseeds.Edibility = -300;
                Bombseeds.IsDrink = false;
                Bombseeds.Buffs = null;
                Bombseeds.GeodeDropsDefaultItems = false;
                Bombseeds.GeodeDrops = null;
                Bombseeds.ArtifactSpotChances = null;
                Bombseeds.CanBeGivenAsGift = true;
                Bombseeds.CanBeTrashed = true;
                Bombseeds.ExcludeFromFishingCollection = false;
                Bombseeds.ExcludeFromShippingCollection = false;
                Bombseeds.ExcludeFromRandomSale = false;
                Bombseeds.ContextTags = null;
                Bombseeds.CustomFields = null;

                ///bombseeds shop
                Bombshop.ActionsOnPurchase = null;
                Bombshop.CustomFields = null;
                Bombshop.TradeItemId = null;
                Bombshop.TradeItemAmount = 1;
                Bombshop.Price = 210;
                Bombshop.ApplyProfitMargins = null;
                Bombshop.AvailableStock = 30;
                Bombshop.AvailableStockLimit = LimitedStockMode.Global;
                Bombshop.AvoidRepeat = false;
                Bombshop.UseObjectDataPrice = false;
                Bombshop.IgnoreShopPriceModifiers = true;
                Bombshop.PriceModifiers = null;
                Bombshop.PriceModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Bombshop.AvailableStockModifiers = null;
                Bombshop.AvailableStockModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Bombshop.Condition = "SEASON summer";
                Bombshop.Id = "(O)BOMBSEEDS";
                Bombshop.ItemId = "(O)BOMBSEEDS";
                Bombshop.RandomItemId = null;
                Bombshop.MaxItems = null;
                Bombshop.MinStack = -1;
                Bombshop.MaxStack = -1;
                Bombshop.Quality = -1;
                Bombshop.ObjectInternalName = null;
                Bombshop.ObjectDisplayName = null;
                Bombshop.ToolUpgradeLevel = -1;
                Bombshop.IsRecipe = false;
                Bombshop.StackModifiers = null;
                Bombshop.StackModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Bombshop.QualityModifiers = null;
                Bombshop.QualityModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack;
                Bombshop.ModData = null;
                Bombshop.PerItemCondition = null;
                //*/













            }







            ///////////HARMONY PATCHING//////////////

            ///coding c# use, remember to comment out
            //StardewValley.Tools.Slingshot
            //public virtual int GetAmmoDamage(Object ammunition)
            // {
            //   return ammunition?.QualifiedItemId switch
            //    {
            //      "(O)388" => 2,
            //     "(O)390" => 5,
            //      "(O)378" => 10,
            //      "(O)380" => 20,
            //       "(O)384" => 30,
            //       "(O)382" => 15,
            //       "(O)386" => 50,
            //       "(O)441" => 20,
            //        _ => 1,
            //    };
            ///}



            ///Slingshot for CATBULB
            ///*
            public static bool GetAmmoDamage_Prefix(StardewValley.Object ammunition, ref int __result)
            {
                try
                {
                    if (ammunition?.QualifiedItemId == "(O)CATBULB")
                    {
                        __result = 15;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Chaosaddon: Ah fuck. Something in the Harmony patch \"GetAmmoDamage_Prefix\" went wrong.");
                    return true;
                }
            }
            //*/
            /// Special buffs
            public static void GeteatObject(StardewValley.Object o, bool overrideFullness = false)
            {
                try
                {
                    chaosaddon.ModEntry.BuffMethod(o.QualifiedItemId);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Chaosaddon: Ah fuck. Something in the Harmony patch \"GeteatObject\" went wrong.");

                }
            }

            public static bool GetplacementAction(GameLocation location, int x, int y, StardewValley.Object __instance, ref bool __result)
            {

                try
                {
                    Microsoft.Xna.Framework.Vector2 vector = new Microsoft.Xna.Framework.Vector2(x / 64, y / 64);

                    if (__instance.ItemId == "DirectionChestUp")
                    {
                        Chest chest3 = new Chest(true, vector, "DirectionChestUp");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                        __result = true;

                        return false;
                    }
                    if (__instance.ItemId == "DirectionChestDown")
                    {
                        Chest chest3 = new Chest(true, vector, "DirectionChestDown");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                        __result = true;

                        return false;
                    }
                    if (__instance.ItemId == "DirectionChestLeft")
                    {
                        Chest chest3 = new Chest(true, vector, "DirectionChestLeft");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                        __result = true;

                        return false;
                    }
                    if (__instance.ItemId == "DirectionChestRight")
                    {
                        Chest chest3 = new Chest(true, vector, "DirectionChestRight");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                        __result = true;

                        return false;
                    }
                    if (__instance.ItemId == "SignalChest")
                    {
                        Chest chest3 = new Chest(true, vector, "SignalChest");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                        __result = true;

                        return false;
                    }
                    if (__instance.ItemId == "OreDestroyer")
                    {
                        Chest chest3 = new Chest(true, vector, "OreDestroyer");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                    Game1.player.Stamina -= 10;
                        __result = true;

                        return false;
                    }
                    if (__instance.ItemId == "WoodDestroyer")
                    {
                        Chest chest3 = new Chest(true, vector, "WoodDestroyer");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                    Game1.player.Stamina -= 10;
                    __result = true;

                        return false;
                    }
                    if (__instance.ItemId == "CropHarvester")
                    {
                        Chest chest3 = new Chest(true, vector, "CropHarvester");
                        chest3.lidFrameCount.Value = 2;
                        location.playSound("axe");
                        location.objects.Add(vector, chest3);
                    Game1.player.Stamina -= 10;
                    __result = true;

                        return false;
                    }

                    return true;
                }
                catch (Exception e)
                {

                    Console.WriteLine("Chaosaddon: Ah fuck. Something in the Harmony patch \"GetplacementAction\" went wrong.");
                    return true;
                }
            }
            public static void GetplacementAction2()
            {

                try
                {
                    InitiateDirectionChests();
                    InitiateBuildingChests();
                    InitiateDestroyers();
                }
                catch (Exception e)
                {

                    Console.WriteLine("Chaosaddon: Ah fuck. Something in the Harmony patch \"GetplacementAction2\" went wrong.");

                }
            }

            public static void GetperformRemoveAction()
            {
                try
                {
                    InitiateDirectionChests();
                    InitiateBuildingChests();
                    InitiateDestroyers();
                }
                catch (Exception e)
                {

                    Console.WriteLine("Ah fuck. Something in the Harmony patch \"GetperformRemoveAction\" went wrong.");

                }
            }

            // REFRENCE

            /*
             Farm0
        FarmHouse1
        FarmCave2
        Town3
        JoshHouse4
        HaleyHouse5
        SamHouse6
        Blacksmith7
        ManorHouse8
        SeedShop9
        Saloon10
        Trailer11
        Hospital12
        HarveyRoom13
        Beach14
        BeachNightMarket15
        ElliottHouse16
        Mountain17
        ScienceHouse18
        SebastianRoom19
        Tent20
        Forest21
        WizardHouse22
        AnimalShop23
        LeahHouse24
        BusStop25
        Mine26
        Sewer27
        BugLand28
        Desert29
        Club30
        SandyHouse31
        ArchaeologyHouse32
        WizardHouseBasement33
        AdventureGuild34
        Woods35
        Railroad36
        WitchSwamp37
        WitchHut38
        WitchWarpCave39
        Summit40
        FishShop41
        BathHouse_Entry42
        BathHouse_MensLocker43
        BathHouse_WomensLocker44
        BathHouse_Pool45
        CommunityCenter46
        JojaMart47
        Greenhouse48
        SkullCave49
        Backwoods50
        Tunnel51
        Trailer_Big52
        Cellar53
        MermaidHouse54
        Submarine55
        AbandonedJojaMart56
        MovieTheater57
        Sunroom58
        BoatTunnel59
        IslandSouth60
        IslandSouthEast61
        IslandSouthEastCave62
        IslandEast63
        IslandWest64
        IslandNorth65
        IslandHut66
        IslandWestCave167
        IslandNorthCave168
        IslandFieldOffice69
        IslandFarmHouse70
        CaptainRoom71
        IslandShrine72
        IslandFarmCave73
        Caldera74
        LeoTreeHouse75
        QiNutRoom76
        MasteryCave77
        DesertFestival78
        LewisBasement79
        Cellar2 80
        Cellar3 81
        Cellar4 82
        Cellar5 83
        Cellar6 84
        Cellar7 85
        Cellar8 86
            */



            // transpliers. (sigh)




            public static IEnumerable<CodeInstruction> BuffUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo getAppliedID = AccessTools.PropertyGetter(typeof(StardewValley.Buffs.BuffManager), nameof(BuffManager.AppliedBuffIds));
                MethodInfo getGetCount = AccessTools.PropertyGetter(typeof(Netcode.NetList<string, Netcode.NetString>), nameof(NetList<string, NetString>.Count));

                Label l = generator.DefineLabel();
                CodeInstruction d = new CodeInstruction(OpCodes.Nop);
                d.labels.Add(l);





                matcher.MatchStartForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, getAppliedID),
                    new CodeMatch(OpCodes.Ldloc_0)
                    )
                    .ThrowIfNotMatch($"Could not find entry point for {nameof(BuffUpdate_Transpiler)}")
                    .Advance(5)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldnull),
                        new CodeInstruction(OpCodes.Ceq),
                        new CodeInstruction(OpCodes.Brtrue, l)


                    )
                    .Advance(27 + 3 + 3)
                    .Insert(d);





                return matcher.InstructionEnumeration();
            }














        }






    }


