using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        UIMgr.Instance.ShowPanel<BeginPanel>();
    }
}
