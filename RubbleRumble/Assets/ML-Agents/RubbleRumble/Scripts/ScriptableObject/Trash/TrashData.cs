using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trash", menuName = "Trashes/TrashData")]
public class TrashData : ScriptableObject
{
    public string trashName;
    public int interactTool;
    public bool readyToThrowAway;
    public string trashbin;
}
