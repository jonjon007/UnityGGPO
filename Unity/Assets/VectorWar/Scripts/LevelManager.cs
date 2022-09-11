using System;
using System.Collections.Generic;
using UnityEngine;
using SepM.Physics;
using SepM.Utils;
using Unity.Mathematics.FixedPoint;

public class LevelManager : MonoBehaviour, Subscriber
{
    public PhysWorld world;
    public List<Tuple<GameObject, PhysObject>> objectsMap = new List<Tuple<GameObject, PhysObject>>();
    public List<Character> characters = new List<Character>();
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("Starting level manager");
        // Initialize world
        world = new PhysWorld();
        CreateCharacter();

        // Subscribe to the GameMangager
        Subscribe();
    }
    void OnDestroy(){
        Unsubscribe();
    }

    // Update is called once per frame
    void Update(){
        UpdateGameObjects();
    }

    /// <summary>
    /// Takes a GameObject/PhysObject tuple and adds it to the world and the object map
    /// </summary>
    public void AddPhysGameObjectPair(Tuple<GameObject, PhysObject> pair){
        world.AddObject(pair.Item2);
        objectsMap.Add(pair);
    }

    void CreateCharacter(){
        // Create tuple (also adds aabbox to world)
        Tuple<GameObject, PhysObject> charTuple = world.CreateAABBoxObject(
            new fp3(0,10,0), new fp3(1,1,1), true, true, Constants.GRAVITY*2, Constants.coll_layers.player
        );
        charTuple.Item2.DynamicFriction = 0;
        charTuple.Item2.StaticFriction = 0;
        // Add to objects map
        objectsMap.Add(charTuple);
        // Create and add new character object
        Character newChar = new Character(charTuple.Item2, world);
        characters.Add(newChar);
    }

    /// <summary>
    /// Returns the PhysObjects in the objects map
    /// </summary>
    public List<PhysObject> GetPhysObjects(){
        return objectsMap.ConvertAll<PhysObject>(entry => entry.Item2);
    }

    private void UpdateGameObjects(){
        foreach(Tuple<GameObject, PhysObject> mapTuple in objectsMap){
            GameObject gameObject = mapTuple.Item1;
            PhysObject physObject = mapTuple.Item2;
            gameObject.transform.position = physObject.Transform.Position.toVector3();
        }
    }


    // Subscriber Interface
    public void Subscribe(){
        if(!RuntimeManager.subscribers.Contains(this)) RuntimeManager.subscribers.Add(this);
    }
    public void Unsubscribe(){
        if(RuntimeManager.subscribers.Contains(this)) RuntimeManager.subscribers.Remove(this);
    }
    public void Step(fp timestep, long inputs){
        // Run physics
        world.Step(timestep);

        // Run step per character
        characters.ForEach(c => c.Step(timestep, inputs));
    }

}
