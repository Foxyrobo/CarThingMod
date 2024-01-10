using InControl;
using Modding;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Modding.Converters;
using Modding.Menu.Config;

namespace CarThingMod
{
    public class GlobalSettings
    {
        //Bools
        public bool spawnCarOnSwing = true;

        //Forces
        public float swingForceX = 30f; //Horizontal force of car when nail is swung
        public float swingForceY = 18f; //Vertical force of car when nail is swung
        public float keyForceX = 0f; //Horizontal force of car when key is pressed
        public float keyForceY = 0f; //Vertical force of car when key is pressed

        //Physics
        public float carMass = 1.5f;
        public float carDrag = 1f;
        public float carAngularDrag = 0.05f;
        public float carFriction = 0.35f;
        public float carBounciness = 0.7f;

        //Advanced
        public bool useContinuousCollision = false;

        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds KeyBinds = new KeyBinds();

        [JsonIgnore]
        public LogLevel LogLevel = LogLevel.Info;
    }

    public class KeyBinds: PlayerActionSet
    {
        public PlayerAction spawnCar; //Key to spawn a car
        public PlayerAction deleteAllCars; //Key to delete every car

        public KeyBinds()
        {
            //Set key name
            spawnCar = CreatePlayerAction("SpawnCar");

            //Set default key
            spawnCar.AddDefaultBinding(Key.O);

            //Set key name
            deleteAllCars = CreatePlayerAction("DeleteAllCars");

            //Set default key
            deleteAllCars.AddDefaultBinding(Key.P);
        }
    }
}
