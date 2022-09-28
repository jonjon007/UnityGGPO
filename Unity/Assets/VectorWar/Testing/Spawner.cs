using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections;
using Unity.Mathematics.FixedPoint;
using SimpPlatformer;
using SepM.Physics;
using IR = InputRegistry;

public class Spawner : MonoBehaviour
{
    fp deltaTime = 1m/60m;
    SpGame game;
    SpGameManager spgm;
    NativeArray<byte> seriGame;

    void Awake(){
        spgm = this.gameObject.GetComponent<SpGameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // spgm.StartLocalGame();
        game = new SpGame(2);
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.digit0Key.wasPressedThisFrame){
            game._world.CleanUp();
            Debug.Log("Clear");
        }
        else if(Keyboard.current.digit1Key.wasPressedThisFrame){
            Tuple<GameObject, PhysObject> charTuple = game._world.CreateAABBoxObject(
                new fp3(0,-2,0), new fp3(1, 1, 1), true, true, Constants.GRAVITY * 0, Constants.coll_layers.player
            );
            Debug.Log("Create object");
        }
        else if(Keyboard.current.digit2Key.wasPressedThisFrame){
            CreateCharacter(0);
            CreateCharacter(1);
            Debug.Log("Create characters");
        }
        else if(Keyboard.current.digit3Key.wasPressedThisFrame){
            game.currentLevel = 1;
            game.SetUpMap();
            Debug.Log("Load map 1");
        }
        else if(Keyboard.current.digit4Key.wasPressedThisFrame){
            game.currentLevel = 2;
            game.SetUpMap();
            Debug.Log("Load map 2");
        }

        // Save
        else if(Keyboard.current.digit5Key.wasPressedThisFrame){
            seriGame = game.ToBytes();
            Debug.Log("Save");
        }

        // Load
        else if(Keyboard.current.digit6Key.wasPressedThisFrame){
            if(seriGame.IsCreated){
                game.FromBytes(seriGame);
                seriGame.Dispose();
                Debug.Log("Load");
            }
            else{
                Debug.LogWarning("No data to load");
            }
        }
    }

    void CreateCharacter(int pIndex){
        Tuple<GameObject, PhysObject> charTuple = game._world.CreateAABBoxObject(
                new fp3(pIndex,-2,0), new fp3(1, 1, 1), true, true, Constants.GRAVITY * 0, Constants.coll_layers.player
            );
            charTuple.Item2.DynamicFriction = 0;
            charTuple.Item2.StaticFriction = 0;
            Character newChar = new Character(charTuple.Item2, game._world, pIndex+1);
            game._characters[pIndex] = newChar;

            // Assign color
            charTuple.Item1.GetComponent<Renderer>().material.color = pIndex+1 == 1 ? Color.red : pIndex+1 == 2 ? Color.blue : pIndex+1 == 3 ? Color.yellow : Color.green;
    }

    void FixedUpdate(){
        long[] inputs = new long[2];
        if(Keyboard.current.aKey.isPressed)
            inputs[0] |= IR.INPUT_LEFT;
        if(Keyboard.current.dKey.isPressed)
            inputs[0] |= IR.INPUT_RIGHT;
        if(Keyboard.current.wKey.isPressed)
            inputs[0] |= IR.INPUT_UP;
        game.Update(inputs, 0);
    }
}
