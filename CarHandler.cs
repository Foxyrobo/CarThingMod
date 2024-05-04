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
    private static GameObject carPrefab;
	private GameObject carObject;
	private tk2dSpriteCollectionData carSpriteCollection;
	List<GameObject> cars;

	void Start()
	{
		//Initialize car list
		cars = new List<GameObject>();

		//Create prefab
		CreateCollections();
		base.StartCoroutine(CreateCarPrefab());
	}

    /// <summary>
    /// Spawns a car with a set force based on player's direction
    /// </summary>
    /// <param name="forceX"></param>
	/// <param name="forceY"></param>
    public void Run(float forceX, float forceY)
	{
		Modding.Logger.Log("[CarThingMod] Car Trigger ", CarThingMod.GS.LogLevel);
		StartCoroutine(CreateCarAnimation(forceX, forceY));
	}

	/// <summary>
	/// Deletes every car GameObject in the scene
	/// </summary>
	public void DeleteAllCars()
	{
		Modding.Logger.Log("[CarThingMod] Deleting all cars", CarThingMod.GS.LogLevel);

		//Iterate through the list and delete each car
		foreach (GameObject car in cars)
		{
			Destroy(car);
		}
        Modding.Logger.Log("[CarThingMod] Sucessfully deleted every car!", CarThingMod.GS.LogLevel);
    }

	IEnumerator CreateCarAnimation(float forceX, float forceY)
	{ 
        GameObject car = SpawnCarFromKnight(forceX, forceY);
        yield return PlaySpriteAnimation(car);
        //Destroy(car);
        yield break;
    }

	IEnumerator PlaySpriteAnimation(GameObject car)
	{
		Modding.Logger.Log("[CarThingMod] PLAYING ANIMATION", CarThingMod.GS.LogLevel);
		tk2dSpriteAnimator anim = car.GetComponent<tk2dSpriteAnimator>();

		Modding.Logger.Log("[CarThingMod] Animation Clip: " + anim.Library.clips[0].name, CarThingMod.GS.LogLevel);
        Modding.Logger.Log("[CarThingMod] Animation Frames: " + anim.Library.clips[0].frames.Length, CarThingMod.GS.LogLevel);

		yield return anim.PlayAnimWait("CarAnimation");
		Modding.Logger.Log("[CarThingMod] Animation Complete", CarThingMod.GS.LogLevel);
	}

	GameObject SpawnCarFromKnight(float forceX, float forceY)
	{
		//Hollow Knight direction (horizontal)
		float directionMultiplierX = (HeroController.instance.cState.facingRight) ? 1f : -1f;
		float wallClimbMultiplier = (HeroController.instance.cState.wallSliding) ? -1f : 1f;
        directionMultiplierX *= wallClimbMultiplier;

		//Hollow Knight direction (vertical)
        float verticalMult = 1f;
		//if (HeroController.instance.cState.lookingUp)
		//{
		//	verticalMult = 2f;
		//}
		//else if (HeroController.instance.cState.lookingDown)
		//{
		//	verticalMult = -2f;
		//}

		//Instantiate new car object
		carObject = Instantiate(carPrefab, HeroController.instance.transform.position
			- new Vector3(directionMultiplierX * -0.4f, 0.75f * verticalMult, 0), new Quaternion(0, 0, 0, 0));

		//Manually inject sprite collection
		carObject.GetComponent<tk2dAnimatedSprite>().Collection = carSpriteCollection;

		//Make active
		carObject.SetActive(true);
		//carObject.layer = (int)PhysLayers.TERRAIN;
		carObject.layer = 0;

        //Change the object's physics material
        PhysicsMaterial2D physMat = new PhysicsMaterial2D();
        physMat.friction = CarThingMod.GS.carFriction;
        physMat.bounciness = CarThingMod.GS.carBounciness;
        carObject.GetComponent<Rigidbody2D>().sharedMaterial = physMat;

		//Set other physics components
        carObject.GetComponent<Rigidbody2D>().mass = CarThingMod.GS.carMass; //Mass
        carObject.GetComponent<Rigidbody2D>().drag = CarThingMod.GS.carDrag; //Linear drag
        carObject.GetComponent<Rigidbody2D>().angularDrag = CarThingMod.GS.carAngularDrag; //Angular drag
		//Load 0 (continuous) if true, else load 1 (discrete)
		carObject.GetComponent<Rigidbody2D>().collisionDetectionMode = 
			CarThingMod.GS.useContinuousCollision ? CollisionDetectionMode2D.Continuous : 
			CollisionDetectionMode2D.Discrete;
        carObject.transform.SetScaleX(CarThingMod.GS.carScaleX); //Horizontal scale
        carObject.transform.SetScaleY(CarThingMod.GS.carScaleY); //Vertical scale

		//Check if the knight is facing up, down, or neither
		Vector2 carForce = new Vector2(forceX * directionMultiplierX, forceY);
        /*if (HeroController.instance.cState.upAttacking)
		{
            carForce = new Vector2(0f, forceY);
            Modding.Logger.Log("[CarThingMod] Send upward with force " + forceY, CarThingMod.GS.LogLevel);
        }
		if (HeroController.instance.cState.downAttacking)
		{
            carForce = new Vector2(0f, -forceY);
			Modding.Logger.Log("[CarThingMod] Send downward with force " + -forceY, CarThingMod.GS.LogLevel);
        }*/
        //Add force
        carObject.GetComponent<Rigidbody2D>().velocity = carForce;

		//Add car to list
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

		//Prefab for the car object, instantiated on button press
		carPrefab = new GameObject("carPrefabObject",
			typeof(Rigidbody2D),
            typeof(BoxCollider2D),
            typeof(tk2dAnimatedSprite), // The sprite animation
            typeof(tk2dSpriteAnimator), // Helper for playing sprite animation
            typeof(PlayMakerFSM), // For coloring collision correctly in debug mod
            typeof(MeshFilter),
            typeof(MeshRenderer)
		);

		//Build the object mesh
		carPrefab.GetComponent<MeshFilter>().mesh = GetMesh();
		carPrefab.GetComponent<MeshRenderer>().enabled = true;

		//Set up animation clip
		tk2dSpriteAnimationClip carAnimationClip = new tk2dSpriteAnimationClip()
		{
			name = "CarAnimation",
			frames = new tk2dSpriteAnimationFrame[] {
			new() {spriteCollection = carSpriteCollection, spriteId = 0},
            new() {spriteCollection = carSpriteCollection, spriteId = 1}},
			fps = 12,
			wrapMode = tk2dSpriteAnimationClip.WrapMode.PingPong, //Do loop
		};

		Modding.Logger.Log("[CarThingMod] Creating tk2dAnimatedSprite", CarThingMod.GS.LogLevel);
		tk2dSpriteAnimation spriteAnimation = carPrefab.AddComponent<tk2dSpriteAnimation>();
		tk2dSpriteAnimationClip[] clips = { carAnimationClip };
		spriteAnimation.clips = clips;

		//Set up animated sprite
		tk2dAnimatedSprite animatedSprite = carPrefab.GetComponent<tk2dAnimatedSprite>();
		animatedSprite.Library = spriteAnimation;
		animatedSprite.Collection = carSpriteCollection;

        //Collider changes
        BoxCollider2D carCol = carPrefab.GetComponent<BoxCollider2D>();
		carCol.size = new Vector2(1.2f, 0.7f);
		carCol.offset = new Vector2(0.0f, 0.0f);

		// FSM seems some hollow knight specific
        // Adding this so it appears blue on debug mod
        // https://github.com/TheMulhima/HollowKnight.DebugMod/blob/master/Source/Hitbox/HitboxRender.cs#L103
        //PlayMakerFSM fsm = carPrefab.GetComponent<PlayMakerFSM>();
        //fsm.FsmName = "damages_enemy";

		//Not active
		carPrefab.SetActive(false);
		DontDestroyOnLoad(carPrefab);
		Modding.Logger.Log("[CarThingMod] Created Car Prefab", CarThingMod.GS.LogLevel);
    }
    private void CreateCollections()
    {
        // Creates sprite collection in tk2d, basically collection of textures
        // Some info here: https://www.2dtoolkit.com/docs/latest/tutorial/creating_a_sprite_collection.html
        Texture2D Idle = Satchel.AssemblyUtils.GetTextureFromResources("CarThingMod.Resources.CarImage2.png");
        GameObject IdleGo = new GameObject("CarSpriteCollection");

        int num_frames = 1;
        float width = Idle.height;
        float height = Idle.height;
        string[] names = new string[num_frames];
        Rect[] rects = new Rect[num_frames];
        Vector2[] anchors = new Vector2[num_frames];
        for (int i = 0; i < num_frames; i++)
        {
            names[i] = i.ToString();
            rects[i] = new Rect(width * i, 0, width, height);
            anchors[i] = new Vector2(140f, 128f);
        }
        // https://github.com/RedFrog6002/FrogCore/
        carSpriteCollection = FrogCore.Utils.CreateFromTexture(IdleGo, Idle, 
			tk2dSpriteCollectionSize.PixelsPerMeter(128f), new Vector2(width * num_frames, height), 
			names, rects, null, anchors, new bool[num_frames]);
        carSpriteCollection.hasPlatformData = false;
        Modding.Logger.Log("[CarThingMod] Created Collections!", CarThingMod.GS.LogLevel);

    }

    Mesh GetMesh(float scale = 0.0625f)
	{
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(0, 0, 0),
			new Vector3(scale*1, 0, 0),
			new Vector3(0, scale*1, 0),
			new Vector3(scale*1, scale*1, 0)
		};
		mesh.vertices = vertices;

		int[] tris = new int[6]
		{
			//Lower left triangle
			0, 2, 1,
			//Upper right triangle
			2, 3, 1
		};
		mesh.triangles = tris;

		Vector3[] normals = new Vector3[4]
		{
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward
		};
		mesh.normals = normals;

		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(0, 1),
			new Vector2(1, 1)
		};
		mesh.uv = uv;

		return mesh;
	}
}
