using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trash", menuName = "Trashes/TrashData")]
public class TrashData : ScriptableObject
{
<<<<<<< HEAD
    public string trashName;
    public int interactTool;
    public bool readyToThrowAway;
    public string trashbin;
=======
    [Header ("Trash Data")]
    public string trashName;
    public bool readyToThrowAway;

    [Space (10f)]
    public string interactTrashbin;

    [Space(10f)]
    public int interactTool;
    public int toolMaxUsage;
>>>>>>> seunghee
}
