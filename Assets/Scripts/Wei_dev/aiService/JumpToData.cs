using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/JumpTo Data")]
public class JumpToData : ScriptableObject
{
    public GameObject JumpPointObj;
    public float DetectRange=8;
    public float AddHeight=2;
}
