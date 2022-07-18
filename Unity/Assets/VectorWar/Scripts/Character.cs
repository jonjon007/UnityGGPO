using System;
using System.IO;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;
using SepM.Utils;
using UnityEngine;
using IR = InputRegistry;
// Reloading level
using UnityEngine.SceneManagement;

[Serializable]
public class Character : ICollider
{
    private LevelManager lm;
    PhysObject physObj;
    public bool onGround;
    public int moveSpeed = 3;

    public void Serialize(BinaryWriter bw) {

    }

    public void Deserialize(BinaryReader br) {

    }

    public Character(PhysObject p, LevelManager l){
        physObj = p;
        lm = l;
        // Also set the physObject's ICollider to this
        physObj.IColl = this;
    }

    /// <summary>
    /// Takes given inputs and runs game logic passed on what was pressed
    /// </summary>
    public void Step(fp timestep, long inputs){
        Debug.Log("OnGround " + onGround.ToString());
        // Jumping
        if(onGround){
            if(IR.CheckInput(inputs, IR.INPUT_UP)){
                Debug.Log("JAMP!");
                physObj.SetVelocity(new fp3(physObj.Velocity.x, 0, physObj.Velocity.z))   ;
                physObj.AddForce(new fp3(0,3000,0));
                onGround = false;
            }
        }

        // Walking
        fp3 newVelocity = physObj.Velocity;
        newVelocity.x = 0;
        if(IR.CheckInput(inputs, IR.INPUT_RIGHT)) newVelocity.x = moveSpeed;
        if(IR.CheckInput(inputs, IR.INPUT_LEFT)) newVelocity.x = -moveSpeed;
        physObj.SetVelocity(newVelocity);

        Debug.DrawRay((physObj.Transform.Position + physObj.Transform.Right()*.25m + physObj.Transform.Up()*-.5m).toVector3(), new Vector3(0, -2.1f, 0), Color.red);
        Debug.DrawRay((physObj.Transform.Position - physObj.Transform.Right()*.25m + physObj.Transform.Up()*-.5m).toVector3(), new Vector3(0, -2.1f, 0), Color.red);

        // Ground detection
        bool isOnGround = false;
        // TODO: Is FindAll necessary since we filter in the Raycast method? Possibly.
        foreach(PhysObject otherObj in lm.GetPhysObjects().FindAll(p => !(p.Coll is null) && p.Coll.InLayers(Constants.layer_ground))){
            if((algo.Raycast(otherObj, physObj.Transform.Position + physObj.Transform.Right()*.25m + physObj.Transform.Up()*-.5m,
                new fp3(0,-.1m, 0), Constants.layer_ground).HasCollision
                || algo.Raycast(otherObj, physObj.Transform.Position - physObj.Transform.Right()*.25m + physObj.Transform.Up()*-.5m,
                new fp3(0,-.1m, 0), Constants.layer_ground).HasCollision)
                && physObj.Velocity.dot(physObj.Gravity) > 0 // And falling or stationary
            ){
                isOnGround = true;
                //Landing
                if(!onGround){
                    fp dt = physObj.Velocity.dot(physObj.Gravity);
                    Land();
                }
                break;
            }
        }
        onGround = isOnGround;

        // TODO: I think GGPO takes care of lastinputs?
        InputHandler.ih.lastInputs = inputs;
    }

    private void Land(){
        physObj.SetVelocity(fp3.zero);
    }

    public void OnCollision(PhysCollision c){
        PhysObject other = c.ObjA == physObj ? c.ObjB : c.ObjA;
        if(other.Coll.Layer == Constants.coll_layers.danger){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
        }
    }
}
