using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour
{
    public Choice choice;
    internal Character NPCTalkingTo;
    internal bool Doable = true;

    private void Update()
    {
        if(choice && Doable)
        {
            CanClick();
            Doable = false;
        }
    }

    public void OnChoose()
    {

        if (choice.ItemTaken != null)
        {
            Inventory.Instance.RemoveFromInventory(choice.ItemTaken);
        }

        if (choice.CharacterImpact != 0)
        {
            GameManager.Instance_.GermaineOpinion += choice.CharacterImpact;
        }

        if (choice.ItemRecieved != null)
        {
            Inventory.Instance.AddToInventory(choice.ItemRecieved);
        }

        if(choice.MoneyRecieved != 0)
        {
            GameManager.Instance_.increaseMoney(choice.MoneyRecieved);
        }

        if(choice.MoneyTaken != 0)
        {
            GameManager.Instance_.decreaseMoney(choice.MoneyTaken);
        }


        if(choice.KarmaImpact != 0)
        {
            GameManager.Instance_.increaseKarma(choice.KarmaImpact);
        }

        if(choice.NewDialogueID != -1)
        {
            Time.timeScale = 0;
            DialogueSystem.Instance.CurrentDialogueMax = DialogueSystem.Instance.Cutscenes[choice.NewDialogueID].script.Count;
            DialogueSystem.Instance.CurrentDialogueID = 0;
            DialogueSystem.Instance.TWEffect.CurrentCutsceneID = choice.NewDialogueID;
            //DialogueSystem.Instance.updateDialogue(choice.NewDialogueID);
        }
        else
        {
            DialogueSystem.Instance.TWEffect.EndDialogue();
        }

        if(choice.RemoveOnChoose)
        {
            DialogueSystem.Instance.Cutscenes[0].script[0].Choices.Remove(choice);
        }

        DialogueSystem.Instance.LastChoosen = choice;

        DialogueSystem.Instance.Choosen();
    }

    public void CanClick()
    {
        if (choice.ItemTaken != null)
        {
            if (!Inventory.Instance.isInInventory(choice.ItemTaken))
            {
                GetComponent<Button>().interactable = false;
            }
        }

        if(GameManager.Instance_.GetMoney() - choice.MoneyTaken < 0)
        {
            GetComponent<Button>().interactable = false;
        }
    }
}
