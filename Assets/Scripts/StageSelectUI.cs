using System.Collections.Generic;
using UnityEngine;

public class StageSelectUI : MonoBehaviour
{
    public List<StageData> stageList;
    public GameObject stageButtonPrefab;
    public Transform gridParent;

    void Start()
    {
        foreach (Transform child in gridParent) { Destroy(child.gameObject); }

        foreach (StageData data in stageList)
        {
            GameObject btnObj = Instantiate(stageButtonPrefab, gridParent);
            StageButton stageBtn = btnObj.GetComponent<StageButton>();
            if (stageBtn != null) { stageBtn.Setup(data); }
        }
    }
}