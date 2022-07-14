using System;
using System.Collections.Generic;
using UnityEngine;
using SepM.Physics;
using Unity.Mathematics.FixedPoint;
using IR = InputRegistry;
using UnityEngine.InputSystem;

/// <summary>
/// Class <c>RuntimeManager</c> Manages game time and coordinates inputs throughout the game session
/// </summary>
public class RuntimeManager : MonoBehaviour {
    public static RuntimeManager gm;

    fp timestep = 1m / 60m;
    public static PhysWorld world;
    PhysObject character; //TODO: Move this elsewhere
    public static List<Tuple<GameObject, PhysObject>> objectsMap = new List<Tuple<GameObject, PhysObject>>();
    public static List<Subscriber> subscribers = new List<Subscriber>();

    // Start is called before the first frame update
    void Awake() {
        DontDestroyOnLoad(gameObject);

        EnforceSingleton();

        LoadDefaultControls();

        // world = new PhysWorld();
        // // objectsMap.Add(world.CreateSphereObject(
        // //     new fp3(0,10,0), 2, true, true, Constants.GRAVITY*2, Constants.coll_layers.player
        // // ));
        // objectsMap.Add(world.CreateAABBoxObject(
        //     new fp3(0,10,0), new fp3(1,1,1), true, true, Constants.GRAVITY*2, Constants.coll_layers.player
        // ));
        // character = objectsMap[0].Item2;
        // objectsMap.Add(world.CreateAABBoxObject(
        //     fp3.zero, new fp3(5,2,3), true, false, fp3.zero, Constants.coll_layers.ground
        // ));
    }

    // Update view
    void Update() {
        UpdateGameObjects();
    }

    // Update world
    void FixedUpdate() {
        // TODO: Multiplayer -> put this in game running step logic
        //long inputs = InputHandler.ih.ReadInputs(1, InputHandler.ih.lastInputs);
        //Step(inputs, timestep);
    }

    private void EnforceSingleton() {
        //Singleton enforcement
        if (!(gm is null))
            Destroy(this.gameObject);
        gm = this;
    }

    /// <summary>
    /// Runs every frame (default: 60fps), manages game logic (will run on GGPO's time in the future)
    /// </summary>
    void Step(long inputs, fp timestep) {
        subscribers.ForEach(s => s.Step(timestep, inputs));
    }

    /// <summary>
    /// Reads from InputRegistry's default PlayerControls and assigns them to it
    /// </summary>
    private void LoadDefaultControls() {
        IR.AddControlsToMap(Keyboard.current, IR.DEFAULT_P1_KEYBOARD_BUTTON_MAPPING, 1);
        IR.AddControlsToMap(Keyboard.current, IR.DEFAULT_P2_KEYBOARD_BUTTON_MAPPING, 2);
    }

    /// <summary>
    /// Takes GameObjects assigned to PhysObjects and moves them according to their current positions
    /// </summary>
    void UpdateGameObjects() {
        // foreach(Tuple<GameObject, PhysObject> mapTuple in objectsMap){
        //     GameObject gameObject = mapTuple.Item1;
        //     PhysObject physObject = mapTuple.Item2;
        //     gameObject.transform.position = physObject.Transform.Position.toVector3();
        // }
    }
}
