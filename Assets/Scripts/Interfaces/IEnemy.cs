using UnityEngine;

public interface IEnemy
{
    GameObject AssignedReference
    {
        get;
        set;
    }

    GameController GameController
    {
        get;
        set;
    }
}
