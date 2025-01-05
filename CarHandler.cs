using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using SFCore;
using System.Linq;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using FrogCore;
using HutongGames.PlayMaker;
using Modding;
using System.Collections.Generic;
using GlobalEnums;
using IL;

namespace CarThingMod;
public class CarHandler : MonoBehaviour
{
	int currentCarIndex = 0; // Index in carTextures

    private static GameObject carPrefab;
	private GameObject carObject;
	List<GameObject> cars;

	void Start()
	{
		// Initialize car list
		cars = new List<GameObject>();

		// Create prefab
		base.StartCoroutine(CreateCarPrefab());
	}

    /// <summary>
    /// Spawns a car with a set force based on player's direction
    /// </summary>
    /// <param name="forceX"></param>
	/// <param name="forceY"></param>
    public void Run(float forceX, float forceY)
	{
		Modding.Logger.Log("[CarThingMod] Spawn Car", CarThingMod.GS.LogLevel);
        //StartCoroutine(CreateCarAnimation(forceX, forceY));
        GameObject car = SpawnCarFromKnight(forceX, forceY);
    }

	/// <summary>
	/// Deletes every car GameObject in the scene
	/// </summary>
	public void DeleteAllCars()
	{
		Modding.Logger.Log("[CarThingMod] Deleting all cars", CarThingMod.GS.LogLevel);

		// Iterate through the list and delete each car
		foreach (GameObject car in cars)
		{
			Destroy(car);
		}
        Modding.Logger.Log("[CarThingMod] Sucessfully deleted every car!", CarThingMod.GS.LogLevel);
    }

	//IEnumerator CreateCarAnimation(float forceX, float forceY)
	//{ 
 //       GameObject car = SpawnCarFromKnight(forceX, forceY);
 //       yield break;
 //   }

	GameObject SpawnCarFromKnight(float forceX, float forceY)
	{
		// Hollow Knight direction (horizontal)
		float directionMultiplierX = (HeroController.instance.cState.facingRight) ? 1f : -1f;
		float wallClimbMultiplier = (HeroController.instance.cState.wallSliding) ? -1f : 1f;
        directionMultiplierX *= wallClimbMultiplier;

		// Instantiate new car object
		carObject = Instantiate(carPrefab, HeroController.instance.transform.position
			- new Vector3(directionMultiplierX * -0.4f, 0.75f, 0), new Quaternion(0, 0, 0, 0));

		// Set sprite of the car
		SetSprite(carObject);

		// Make active
		carObject.SetActive(true);
		// carObject.layer = (int)PhysLayers.TERRAIN;
		carObject.layer = 0;

        // Change the object's physics material
        PhysicsMaterial2D physMat = new PhysicsMaterial2D();
        physMat.friction = CarThingMod.GS.carFriction;
        physMat.bounciness = CarThingMod.GS.carBounciness;
        carObject.GetComponent<Rigidbody2D>().sharedMaterial = physMat;

		// Set other physics components
        carObject.GetComponent<Rigidbody2D>().mass = CarThingMod.GS.carMass; // Mass
        carObject.GetComponent<Rigidbody2D>().drag = CarThingMod.GS.carDrag; // Linear drag
        carObject.GetComponent<Rigidbody2D>().angularDrag = CarThingMod.GS.carAngularDrag; // Angular drag
		// Load 0 (continuous) if true, else load 1 (discrete)
		carObject.GetComponent<Rigidbody2D>().collisionDetectionMode = 
			CarThingMod.GS.useContinuousCollision ? CollisionDetectionMode2D.Continuous : 
			CollisionDetectionMode2D.Discrete;
        carObject.transform.SetScaleX(CarThingMod.GS.carScaleX); // Horizontal scale
        carObject.transform.SetScaleY(CarThingMod.GS.carScaleY); // Vertical scale

        // Add force
        Vector2 carForce = new Vector2(forceX * directionMultiplierX, forceY);
        carObject.GetComponent<Rigidbody2D>().velocity = carForce;

		// Add car to list
		cars.Add(carObject);

		return carObject;
	}

	IEnumerator CreateCarPrefab()
	{
		do
		{
			yield return null;
		}
		while (HeroController.instance == null || GameManager.instance == null);
		Modding.Logger.Log("[CarThingMod] Instantiating Car Prefab", CarThingMod.GS.LogLevel);
		Resources.LoadAll<GameObject>("");

		// Prefab for the car object, instantiated on button press
		carPrefab = new GameObject("carPrefabObject",
			typeof(Rigidbody2D),
            typeof(BoxCollider2D),
            typeof(PlayMakerFSM), // For coloring collision correctly in debug mod
			typeof(SpriteRenderer)
		);

        // Collider changes
        BoxCollider2D carCol = carPrefab.GetComponent<BoxCollider2D>();
		carCol.size = new Vector2(1.2f, 0.7f);
		carCol.offset = new Vector2(0.0f, 0.0f);

		// FSM seems some hollow knight specific
        // Adding this so it appears blue on debug mod
        // https://github.com/TheMulhima/HollowKnight.DebugMod/blob/master/Source/Hitbox/HitboxRender.cs#L103
        PlayMakerFSM fsm = carPrefab.GetComponent<PlayMakerFSM>();
		fsm.FsmName = "health_manager"; // Not a great way of doing this, but oh well

		// Not active
		carPrefab.SetActive(false);
		DontDestroyOnLoad(carPrefab);
		Modding.Logger.Log("[CarThingMod] Created Car Prefab", CarThingMod.GS.LogLevel);
    }

    void SetSprite(GameObject obj, FilterMode filterMode = FilterMode.Point)
    {
        // Get the spriteRenderer from object
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

		// Load sprite from resources
		Sprite spr = GetNextSprite();
   //     spr = Satchel.AssemblyUtils.GetSpriteFromResources
			//("CarThingMod.Resources.CarImage2.png", CarThingMod.GS.carPixelsPerUnit);

		// Set filterMode and sprite for the gameObject
        spr.texture.filterMode = filterMode;
        spriteRenderer.sprite = spr;
    }

	Sprite GetNextSprite()
	{
		// Check if any custom cars have been found
		if (CarThingMod.customCarsFound)
		{
			// Cycle through custom cars
			Texture2D texture = CarThingMod.carTextures[currentCarIndex];
            Sprite spr = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 
				new Vector2(0.5f, 0.5f), CarThingMod.GS.carPixelsPerUnit, 0, SpriteMeshType.FullRect);

            // Increment index
            currentCarIndex++;
			if (currentCarIndex >= CarThingMod.carTextures.Count) currentCarIndex = 0;

			return spr;
		}
		else
		{
			// Use default car
			return Satchel.AssemblyUtils.GetSpriteFromResources(Constants.DEFAULT_CAR);
		}
	}
}
