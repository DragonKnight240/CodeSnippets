using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static DialogueSystem;

public class TypeWriterEffect : MonoBehaviour
{
    public GameObject UIPanel;
    TextMeshProUGUI textComponent;
    public float waitSecondsText = 0.2f;
    internal bool Finished = false;
    float timer = 0;
    string dialogue = "";
    int CharNum = 0;
    int CharNumMax;
    internal bool InUse = false;
    internal int CurrentCutsceneID;
    public GameObject SelfSpeaker;
    public GameObject OtherSpeaker;
    public GameObject TextUIPanel;
    public GameObject Mouse;
    internal Options Options;

    // Start is called before the first frame update
    void Start()
    {
        textComponent = TextUIPanel.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = "";
        Options = FindObjectOfType<Options>();
    }

    private void Update()
    {
        if(InUse && !DialogueSystem.Instance.ChoicePanel.activeInHierarchy)
        {
            timer += Time.unscaledDeltaTime;
            if (!Finished)
            {
                NextLetter();
            }
            OnPress();
        }
    }

    public void NewDialogue()
    {
        textComponent.text = " ";

        switch (DialogueSystem.Instance.CurrentTreeStat)
        {
            case DialogueTree.High:
                dialogue = DialogueSystem.Instance.currentDialogue.ScriptHigh;
                break;
            case DialogueTree.Neutral:
                dialogue = DialogueSystem.Instance.currentDialogue.Script;
                break;
            case DialogueTree.Low:
                dialogue = DialogueSystem.Instance.currentDialogue.ScriptLow;
                break;
            case DialogueTree.None:
                dialogue = DialogueSystem.Instance.currentDialogue.Script;
                break;
            default:
                dialogue = DialogueSystem.Instance.currentDialogue.Script;
                break;
        }

        if(DialogueSystem.Instance.currentDialogue.Speaker.Contains("Rin") || DialogueSystem.Instance.currentDialogue.Speaker.Contains("Allen"))
        {
            SelfSpeaker.GetComponentInChildren<TextMeshProUGUI>().text = DialogueSystem.Instance.currentDialogue.Speaker;
            OtherSpeaker.SetActive(false);
            SelfSpeaker.SetActive(true);
        }
        else if(DialogueSystem.Instance.currentDialogue.Speaker.Contains("None") || DialogueSystem.Instance.currentDialogue.Speaker == (""))
        {
            OtherSpeaker.SetActive(false);
            SelfSpeaker.SetActive(false);
        }
        else
        {
            OtherSpeaker.GetComponentInChildren<TextMeshProUGUI>().text = DialogueSystem.Instance.currentDialogue.Speaker;
            OtherSpeaker.SetActive(true);
            SelfSpeaker.SetActive(false);
        }

        CharNumMax = dialogue.Length;
        CharNum = 0;
        InUse = true;
        UIPanel.SetActive(true);
        Mouse.SetActive(true);
    }

    void NextLetter()
    {
        if (timer >= waitSecondsText)
        {
            textComponent.text += dialogue[CharNum];
            CharNum++;
            if(CharNum >= CharNumMax)
            {
                Finished = true;
                CharNum = 0;
            }
            timer = 0;
        }
    }

    void OnPress()
    {
        if (Input.GetButtonDown("Fire1") && !Options.OptionsMenuUI.activeInHierarchy)
        {
            if (UIPanel.activeInHierarchy && InUse && Finished)
            {
                CheckDialogue();
            }
            else
            {
                for (int i = CharNum; i < CharNumMax; i++)
                {
                    textComponent.text += dialogue[i];
                }

                Finished = true;
                CharNum = 0;
                timer = 0;
            }
        }
    }

    void CheckDialogue()
    {
        if (DialogueSystem.Instance.CurrentDialogueID++ >= DialogueSystem.Instance.CurrentDialogueMax-1 && !(DialogueSystem.Instance.currentDialogue.Choices.Count != 0))
        {
            EndDialogue();

            if(DialogueSystem.Instance.currentDialogue.LoadSceneAfter != "")
            {
                FindObjectOfType<LoadScene>().LoadNewScene(DialogueSystem.Instance.currentDialogue.LoadSceneAfter);
            }

        }
        else if(DialogueSystem.Instance.currentDialogue.Choices.Count != 0)
        {
            if (!DialogueSystem.Instance.ChoicePanel.activeInHierarchy)
            {
                DialogueSystem.Instance.SetChoices();
            }
        }
        else
        {
            NextDialogue();
        }
    }

    public void NextDialogue()
    {
        Finished = false;
        CharNum = 0;
        timer = 0;

        if (DialogueSystem.Instance.currentDialogue.ChangeImage)
        {
            GetComponent<ImageHolder>().ChangeImage();

            if(GetComponent<ImageHolder>().inactive)
            {
                GetComponent<ImageHolder>().Panel.SetActive(true);
            }
        }

        if (DialogueSystem.Instance.TWEffect.CurrentCutsceneID != -1)
        {
            DialogueSystem.Instance.updateDialogue(CurrentCutsceneID, DialogueSystem.Instance.CurrentNPC);
        }
    }

    public void EndDialogue()
    {
        Finished = false;
        CharNum = 0;
        InUse = false;
        UIPanel.SetActive(false);
        Mouse.SetActive(false);
        Time.timeScale = 1;
    }
}
