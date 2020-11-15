using UnityEngine;

public class DisplayableObjectController : MonoBehaviour
{
    [Header("Loading Screen Description")]
    [SerializeField]
    [Tooltip("The tooltip displayed during the loading screen for this tank")]
    [Multiline(3)]
    private string loadingTip;

    public string LoadingTip
    {
        get { return loadingTip; }
    }
}
