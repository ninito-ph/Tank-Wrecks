using UnityEngine;

public interface IEnemy
{
    GameObject AssignedReference
    {
        get;
        set;
    }

    GameManager GameManager
    {
        get;
        set;
    }

    PlayerController PlayerReference
    {
        get;
        set;
    }
}
