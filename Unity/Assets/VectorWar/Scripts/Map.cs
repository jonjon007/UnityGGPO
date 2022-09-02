using SepM.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public void Initialize(PhysWorld world) {
        foreach (SepObject so in GetComponentsInChildren<SepObject>())
            so.Initialize(world);
    }
}
