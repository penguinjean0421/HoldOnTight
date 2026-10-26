using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    public Image backgroundImage;
    public Button button;

    private StageData stageData;

    public void Setup(StageData data)
    {
        stageData = data;
        UpdateStageUI();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnStageButtonClicked);
        }
    }

    private void UpdateStageUI()
    {
        if (backgroundImage != null && stageData != null)
        {
            backgroundImage.sprite = stageData.stageIcon;
        }
    }

    private void OnStageButtonClicked()
    {
        if (stageData == null) return;

        if (StageManager.Instance != null) StageManager.Instance.ApplyStageData(stageData);
        if (GameManager.Instance != null && GameManager.Instance.board != null) GameManager.Instance.NewGame();
        if (UIManager.Instance != null) UIManager.Instance.PlayGame();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameplayBGM();
    }
}