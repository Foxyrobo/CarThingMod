using System;
using System.Collections;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using System.IO;
using Satchel.BetterMenus;
using Satchel;

namespace CarThingMod
{
    public class CarThingMod : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
    {
        internal static GlobalSettings GS = new GlobalSettings();
        new public string GetName() => "Car Thing Mod";

        // Version number: MAJOR.MINOR.PATCH.BUILD
        public override string GetVersion() => "1.2.6.4";

        // Directories
        internal string carDirectory = Path.Combine(AssemblyUtils.getCurrentDirectory(),
            Constants.CAR_FOLDER_NAME);
        internal string sampleDirectory = Path.Combine(AssemblyUtils.getCurrentDirectory(),
            Constants.SAMPLE_FOLDER_NAME);
        internal string sampleCopyDirectory = Constants.SAMPLE_FOLDER_COPY;

        //internal static List<Texture2D> carTextures = new List<Texture2D>();
        internal static List<CarClass> carList = new List<CarClass>();
        internal static bool customCarsFound = false;

        Menu MenuRef;

        public override void Initialize()
        {
            // Subscribe to events
            ModHooks.AttackHook += ModHooks_AttackHook;
            On.HeroController.Awake += new On.HeroController.hook_Awake(this.OnHeroControllerAwake);
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.LanguageGetHook += LanguageGet;

            // Make sure there is a directory for car images and settings
            IoUtils.EnsureDirectory(carDirectory);

            // Make sure sample files exist
            EnsureSampleFiles();

            // Load all car textures
            LoadCars();
        }

        /// <summary>
        /// Ensures that the sample files for custom cars exist
        /// </summary>
        void EnsureSampleFiles()
        {
            // Ensure the directory exists
            IoUtils.EnsureDirectory(sampleDirectory);

            // Ensure the sample car is there
            Utils.CopyFromResources(sampleDirectory, Constants.SAMPLE_CAR, "sample car");

            // Ensure the sample settings text doc is there
            Utils.CopyFromResources(sampleDirectory, Constants.SAMPLE_TXT, "sample settings text doc");

            // Ensure the instructions are there
            Utils.CopyFromResources(sampleDirectory, Constants.SAMPLE_INSTRUCTIONS, "custom car instructions");
        }

        /// <summary>
        /// Loads in each car texture found in the car folder.
        /// </summary>
        public void LoadCars()
        {
            // Find all car image and text files
            string[] carpngs = Directory.GetFiles(carDirectory, $"*{Constants.DEFAULT_IMAGE_EXTENSION}");
            string[] cartxts = Directory.GetFiles(carDirectory, $"*{Constants.DEFAULT_TEXT_EXTENSION}");

            // Make sure the number of pngs and txt files are the same
            // 0 = the same
            // Positive value = carpngs > cartxts
            // Negative value = carpngs < cartxts
            int diff = carpngs.Length - cartxts.Length;

            // Give warnings
            if (diff > 0)
            {
                Modding.Logger.LogWarn("[CarThingMod] More car textures were found than settings files!");
            }
            if (diff < 0)
            {
                Modding.Logger.LogWarn("[CarThingMod] More car settings files were found than textures!");
            }

            // Iterate through each texture/text pair
            if (carpngs.Length > 0)
            {
                // Make it known that custom cars were found
                customCarsFound = true;

                // Iterate through all car pngs
                foreach (var png in carpngs)
                {
                    // Temporary remove the file extension
                    string pngNoExt = png.Substring(0, png.IndexOf(Constants.DEFAULT_IMAGE_EXTENSION));

                    // Iterate through all car settings files
                    foreach (var txt in cartxts)
                    {
                        // Temporary remove the file extension
                        string txtNoExt = txt.Substring(0, txt.IndexOf(Constants.DEFAULT_TEXT_EXTENSION));

                        // Compare them
                        if (pngNoExt == txtNoExt)
                        {
                            // Create a new car
                            CarClass newCar = Utils.CreateCar(png, txt, carDirectory);

                            // Error checking - If it's null, skip
                            if (newCar != null) carList.Add(newCar);
                            continue;
                        }
                    }
                }
            }

            // Logging statements
            if (customCarsFound)
            {
                Modding.Logger.Log("[CarThingMod] " + carpngs.Length + " custom cars were found!",
                    GS.LogLevel);
            }
            else
            {
                Modding.Logger.Log("[CarThingMod] No custom cars were found! Reverting to default sprite",
                    GS.LogLevel);
            }
        }

        /// <summary>
        /// Deletes all current car settings and re-loads them
        /// </summary>
        void RefreshCars()
        {
            // Delete current car settings
            carList.Clear();

            // Re-load all cars
            IoUtils.EnsureDirectory(carDirectory); // Make sure the user didn't delete the directory
            LoadCars();

            // Logging
            Modding.Logger.Log("[CarThingMod] Refreshed cars!", GS.LogLevel);
        }

        // Called when the player swings the nail
        private void ModHooks_AttackHook(GlobalEnums.AttackDirection obj)
        {
            // First, check if this is enabled
            if (GS.spawnCarOnSwing)
            {
                // Spawn a car with velocity
                HeroController.instance.GetComponent<CarHandler>().Run(GS.swingForceX, GS.swingForceY);
            }
        }

        private void OnHeroControllerAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig.Invoke(self);
            self.gameObject.AddComponent<CarHandler>();
        }

        public void OnHeroUpdate()
        {
            // Check if the spawn car key was pressed
            if (GS.KeyBinds.spawnCar.WasPressed)
            {
                HeroController.instance.GetComponent<CarHandler>().Run(GS.keyForceX, GS.keyForceY);
            }

            // Check if the delete all cars key was pressed
            if (GS.KeyBinds.deleteAllCars.WasPressed)
            {
                HeroController.instance.GetComponent<CarHandler>().DeleteAllCars();
            }
        }

        public string LanguageGet(string key, string sheetTitle, string orig)
        {
            // Make elderbug say "pee pee poo poo" when he's exhausted his dialogue
            if (key == "ELDERBUG_GENERIC_2" && sheetTitle == "Elderbug")
            {
                return "pee pee poo poo";
            }

            // When exhausting Iselda's dialogue (behind the counter), make her say "balls"
            if (key == "ISELDA_REPEAT" && sheetTitle == "Iselda")
            {
                return "balls";
            }

            // Make Quirrel's name be "Plural"
            if (key == "QUIRREL_MAIN" && sheetTitle == "Titles")
            {
                return "Plural";
            }

            // make sure to return orig for all the other texts you dont want to change
            return orig;
        }

        // Load settings from past session into current session
        void IGlobalSettings<GlobalSettings>.OnLoadGlobal(GlobalSettings s)
        {
            GS = s;
        }

        // Save settings from current session
        GlobalSettings IGlobalSettings<GlobalSettings>.OnSaveGlobal()
        {
            return GS;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            // return ModMenu.CreateMenuScreen(modListMenu).Build();
            // Create a new MenuRef if it's not null
            MenuRef ??= new Menu(
                name: "Car Thing Mod", // the title of the menu screen, it will appear on the top center of the screen 
                elements: new Element[]
                {
                    // Text - Settings title
                    new TextPanel(
                        name: "Settings",
                        fontSize: 60),

                    // Bool - Spawn car on nail swing
                    new HorizontalOption(
                        name: "Spawn Car On Nail Swing?",
                        description: "Should a car spawn when the player swings the nail?",
                        values: new [] { "Yes", "No" },
                        Id: "spawnOnSwing",
                        applySetting: index =>
                        {
                            GS.spawnCarOnSwing = index == 0; // "yes" is the 0th index in the values array
                        },
                        loadSetting: () => GS.spawnCarOnSwing ? 0 : 1), // return 0 ("Yes") if active and 1 ("No") if false
                    
                    // Keybind - Spawn car key
                    new KeyBind(
                        name: "Spawn car",
                        Id: "carKey",
                        playerAction: GS.KeyBinds.spawnCar),

                    // Keybind - Delete all cars key
                    new KeyBind(
                        name: "Delete all cars",
                        Id: "carDeleteKey",
                        playerAction: GS.KeyBinds.deleteAllCars),

                    // Text - Physics title
                    new TextPanel(
                        name: "Physics",
                        fontSize: 60),

                    // Text - Forces header
                    new TextPanel(
                        name: "Forces",
                        fontSize: 40),

                    // Float - Car horizontal force when nail is swung
                    new CustomSlider(
                        name: "Car X force on swing",
                        Id: "swingXForce",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.swingForceX = val;
                        },
                        loadValue: () => GS.swingForceX, // to load the value on menu creation
                        minValue: -100,
                        maxValue: 100,
                        wholeNumbers: true),

                    // Float - Car vertical force when nail is swung
                    new CustomSlider(
                        name: "Car Y force on swing",
                        Id: "swingYForce",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.swingForceY = val;
                        },
                        loadValue: () => GS.swingForceY, // to load the value on menu creation
                        minValue: -100,
                        maxValue: 100,
                        wholeNumbers: true),

                    // Float - Car horizontal force when key is pressed
                    new CustomSlider(
                        name: "Car X force on key press",
                        Id: "keyXForce",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.keyForceX = val;
                        },
                        loadValue: () => GS.keyForceX, // to load the value on menu creation
                        minValue: -100,
                        maxValue: 100,
                        wholeNumbers: true),

                    // Float - Car horizontal force when key is pressed
                    new CustomSlider(
                        name: "Car Y force on key press",
                        Id: "keyYForce",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.keyForceY = val;
                        },
                        loadValue: () => GS.keyForceY, // to load the value on menu creation
                        minValue: -100,
                        maxValue: 100,
                        wholeNumbers: true),

                    // Text - Properties header
                    new TextPanel(
                        name: "Physical Properties",
                        fontSize: 40),

                    // Float - Car mass
                    new CustomSlider(
                        name: "Car mass",
                        Id: "mass",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.carMass = val;
                        },
                        loadValue: () => GS.carMass, // to load the value on menu creation
                        minValue: 0.1f,
                        maxValue: 25,
                        wholeNumbers: false),

                    // Float - Car drag
                    new CustomSlider(
                        name: "Car drag",
                        Id: "drag",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.carDrag = val;
                        },
                        loadValue: () => GS.carDrag, // to load the value on menu creation
                        minValue: 0,
                        maxValue: 10,
                        wholeNumbers: false),

                    // Float - Car angular drag
                    new CustomSlider(
                        name: "Car angular drag",
                        Id: "angularDrag",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.carAngularDrag = val;
                        },
                        loadValue: () => GS.carAngularDrag, // to load the value on menu creation
                        minValue: 0,
                        maxValue: 2.5f,
                        wholeNumbers: false),

                    // Float - Car friction
                    new CustomSlider(
                        name: "Car friction",
                        Id: "friction",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.carFriction = val;
                        },
                        loadValue: () => GS.carFriction, // to load the value on menu creation
                        minValue: 0,
                        maxValue: 1,
                        wholeNumbers: false),

                    // Float - Car bounciness
                    new CustomSlider(
                        name: "Car bounciness",
                        Id: "bounciness",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.carBounciness = val;
                        },
                        loadValue: () => GS.carBounciness, // to load the value on menu creation
                        minValue: 0,
                        maxValue: 1,
                        wholeNumbers: false),

                    // Float - Scale X
                    new CustomSlider(
                        name: "Car scale X",
                        Id: "scaleX",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.carScaleX = val;
                        },
                        loadValue: () => GS.carScaleX, // to load the value on menu creation
                        minValue: 0.1f,
                        maxValue: 5,
                        wholeNumbers: false),

                    // Float - Scale Y
                    new CustomSlider(
                        name: "Car scale Y",
                        Id: "scaleY",
                        storeValue: val => // to store the value when the slider is changed by user
                        {
                            GS.carScaleY = val;
                        },
                        loadValue: () => GS.carScaleY, // to load the value on menu creation
                        minValue: 0.1f,
                        maxValue: 5,
                        wholeNumbers: false),

                    // Text - Advanced header
                    new TextPanel(
                        name: "Advanced",
                        fontSize: 60),

                    // Bool - Use continuous collision detection
                    // Having this on means the cars can't clip through walls, but it uses more performance
                    new HorizontalOption(
                        name: "Use continuous collision?",
                        description: "If yes, cars can't clip through walls, but the game might lag",
                        values: new [] { "Yes", "No" },
                        Id: "continuousCol",
                        applySetting: index =>
                        {
                            GS.useContinuousCollision = index == 0; // "yes" is the 0th index in the values array
                        },
                        loadSetting: () => GS.useContinuousCollision ? 0 : 1), // return 0 ("Yes") if active and 1 ("No") if false

                    // Button - Refresh car list
                    new MenuButton(
                        name: "Refresh cars",
                        description: "Applies any changes made in the custom car folder",
                        Id: "refreshButton",
                        submitAction: (_) =>
                        {
                            // Find element by Id
                            Element elem = MenuRef.Find("refreshButton");
                            MenuButton buttonElem = elem as MenuButton;
                            buttonElem.Update();

                            RefreshCars();
                        })
                }
            );

            // uses the GetMenuScreen function to return a menuscreen that MAPI can use. 
            // The "modlistmenu" that is passed into the parameter can be any menuScreen that you want to return to when "Back" button or "esc" key is pressed 
            return MenuRef.GetMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu => false;
    }
}