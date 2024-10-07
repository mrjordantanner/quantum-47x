using UnityEngine;
using System.Linq;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;


public class NameEntryPanel : MenuPanel
{
    public TMP_InputField nameInputField;
    public TextMeshProUGUI nameInputLabel;

    public override void Show(float fadeDuration = 0.2f, bool setActivePanel = true)
    {
        nameInputField.onEndEdit.AddListener(OnInputEndEdit);
        nameInputField.characterLimit = 12;

        if (PlayerData.Instance.Data.PlayerName != PlayerData.Instance.Data.defaultPlayerName)
        {
            //nameInputLabel.text = "";
            nameInputField.text = PlayerData.Instance.Data.PlayerName;
        } 
        else
        {
            //nameInputLabel.text = "Enter Name";
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }

        base.Show(fadeDuration);
    }

    public void OnInputEndEdit(string playerName)
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            Menu.Instance.ConfirmNameEntry();
        }
    }

    public override void Hide(float fadeDuration = 0.1f)
    {
        base.Hide(fadeDuration);

    }

}
