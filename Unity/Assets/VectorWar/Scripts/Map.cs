using SepM.Physics;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Set in Inspector")]
    public SepObject StartPosition;
    void Start(){
        if(StartPosition is null)
            Debug.LogWarning("Level doesn't have start position!");
    }
    public void Initialize(PhysWorld world) {
        foreach (SepObject so in GetComponentsInChildren<SepObject>())
            so.Initialize(world);
    }
}
