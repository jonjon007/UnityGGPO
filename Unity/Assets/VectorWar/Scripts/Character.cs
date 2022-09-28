using System;
using System.IO;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;
using SepM.Utils;
using UnityEngine;
using IR = InputRegistry;

namespace SimpPlatformer{
    [Serializable]
    public class Character : ICollider
    {
        private fp3 startPos;
        public PhysObject physObj;
        public bool onGround;
        public int moveSpeed = 3;
        private PhysWorld world;
        public int instanceId; // TODO: Remove?
        private int playerNum;

        // TODO: Write tests
        public void Serialize(BinaryWriter bw) {
        //startPos
            bw.WriteFp(startPos.x);
            bw.WriteFp(startPos.y);
            bw.WriteFp(startPos.z);
        // physObj
            // Serialize PhysObject's index
            int poIndex = world.GetPhysObjectIndexById(physObj.InstanceId);
            bw.Write(poIndex);
        //onGround
            bw.Write(onGround);
        //moveSpeed
            bw.Write(moveSpeed);
        //playerNum;
            bw.Write(playerNum);
        }

        public void Deserialize(BinaryReader br) {
        //startPos
            startPos.x = br.ReadFp();
            startPos.y = br.ReadFp();
            startPos.z = br.ReadFp();
        // physObj
            // Grab PhysObject by the index
            physObj = world.GetPhysObjectByIndex(br.ReadInt32());
            physObj.IColl = this;
        //onGround
            onGround = br.ReadBoolean();
        //moveSpeed
            moveSpeed = br.ReadInt32();
        //playerNum;
            playerNum = br.ReadInt32();
        }

        public Character(PhysObject po, PhysWorld w, int pn) {
            physObj = po;
            // Also set the physObject's ICollider to this
            physObj.IColl = this;
            world = w;
            startPos = po.Transform.Position;
            playerNum = pn;
        }

        public void SetWorld(PhysWorld w){
            world = w;
        }

        /// <summary>
        /// Takes given inputs and runs game logic passed on what was pressed
        /// </summary>
        public void Step(fp timestep, long inputs){
            // Jumping
            if(onGround){
                if(IR.CheckInput(inputs, IR.INPUT_UP)){
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
            foreach(PhysObject otherObj in world.objectsMap.ConvertAll<PhysObject>(t => t.Item2).FindAll(p => !(p.Coll is null) && p.Coll.InLayers(Constants.layer_ground))){
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
                physObj.Transform.Position = startPos;
            }
            else if(other.Coll.Layer == Constants.coll_layers.win){
                SpGame.endLevel = playerNum;
            }
        }
    }
}