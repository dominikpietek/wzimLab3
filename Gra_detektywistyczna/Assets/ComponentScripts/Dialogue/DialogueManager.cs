using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using DTOModel;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public
class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private Image NpcImage;

    public string sceneContext;
    public string currentNpcName;
    public bool isFreeDiscussionEnabled;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    public TMP_InputField inputField;

    public List<Dialogue> dialoguesHistory;
    private Queue<Dialogue> dialoguesQueue;
    public bool isAwaitingUserInput = false;
    public bool isAwaitingNPCResponse = false;
    
    void Awake()
    {
        Instance = this;
        dialoguesQueue = new Queue<Dialogue>();
        dialoguesHistory = new List<Dialogue>();
    }
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame && !isAwaitingNPCResponse)
        {
            if (isAwaitingUserInput)
            {
                
                dialoguesHistory.Add(new Dialogue("Ty", inputField.text));
                NPCRequestDTO npcRequestDTO = new NPCRequestDTO()
                {
                    SceneDescription = sceneContext,
                    UserText = inputField.text,
                    NPCName = currentNpcName,
                };
                SendNpcRequest(npcRequestDTO);
                isAwaitingUserInput = false;
                isAwaitingNPCResponse = true;
                nameText.text = "Narrator";
                DisableUserInput();
                ShowLoadingResponse();
            } 
            else
            {
                PlayDialogue();
            }
        } 
    }
    public void AskQuestion(string name)
    {
        
        Dialogue dialogue = new Dialogue("Ty", sceneContext);
        dialogue.isPlayerPrompt = true;
        EnqueueDialogue(dialogue);
        EnqueueDialogue(new Dialogue(name, ""));
    }
    public void EnqueueDialogue(Dialogue dialogue)
    {
        dialoguesQueue.Enqueue(dialogue);
    }
    public void PlayDialogue()
    {
        ShowDialogue();
        StopAllCoroutines();
        if (dialoguesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }
        Dialogue dialog = dialoguesQueue.Dequeue();
        if (dialog.isPlayerPrompt)
        {
            PromptPlayerAsync();
        }
        else
        {
            DisplayDialogue(dialog);
        }
    }
    public async Task PromptPlayerAsync()
    {
        HideDialogueText();
        currentNpcName = dialoguesQueue.Peek().name;

        //zmienianie sprite npc (nie wiem czy w dobrym miejscu najwyzej poprawie)
        
        string[] parameters = { MenuControl.CurrentScenarioName,MenuControl.CurrentSceneNumber.ToString()};
        SceneScriptDTO scene = await DialogueEngineManager.Instance.GetSceneAsync(parameters);
        foreach (NPCDTO npc in scene.Npcs)
        {
            if (currentNpcName == npc.name.Split(' ')[0])
            {
                Sprite newSprite = Resources.Load<Sprite>(currentNpcName);
                NpcImage.sprite = newSprite;
                if (NpcImage.color.a == 0f)
                {
                    NpcImage.color = Color.white;
                }
            }
        }
        nameText.text = "Ty";
        EnableUserInput();
        isAwaitingUserInput = true;
    }
    public async void SendNpcRequest(NPCRequestDTO npcRequestDTO)
    {
        DialogueContextManager.AddPlayerDialogue("Ty", npcRequestDTO.UserText);
        NPCResponseDTO response = await DialogueEngineManager.Instance.AskNPCAsync(npcRequestDTO);
        StopAllCoroutines();
        isAwaitingNPCResponse = false;
        dialoguesQueue.Peek().sentence = response.Speech;
        dialoguesQueue.Peek().name = currentNpcName;
        DialogueContextManager.AddNPCDialogue(currentNpcName, response.Speech);
        PlayDialogue();
    }
    public void DisplayDialogue(Dialogue dialogue)
    {
        dialoguesHistory.Add(dialogue);
        currentNpcName = dialogue.name;
        dialogueText.text = dialogue.sentence;
        nameText.text = dialogue.name;
        StartCoroutine(TypeSentence(dialogue.sentence));
    }
    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }
    void EndDialogue()
    {
        if (isFreeDiscussionEnabled)
        {
            AskQuestion(currentNpcName);
            PlayDialogue();
        } 
        else
        {
            HideDialogue();
        }
    }
    IEnumerator AnimateTypingDots()
    {
        string baseText = currentNpcName + " my�li";
        int dotCount = 0;

        while (true)
        {
            dialogueText.text = baseText + new string('.', dotCount % 4);
            dotCount++;
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void ShowLoadingResponse()
    {
        ShowDialogue();
        StartCoroutine(AnimateTypingDots());
    }
    public void ShowDialogue()
    {
        dialoguePanel.SetActive(true);
    }
    public void HideDialogueText()
    {
        dialogueText.text = "";
    }
    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }
    public void Reset()
    {
        dialoguePanel.SetActive(false);
    }
    public void EnableUserInput()
    {
        inputField.gameObject.SetActive(true);
        inputField.interactable = true;
        inputField.ActivateInputField();
    }
    public void DisableUserInput()
    {
        inputField.text = "";
        inputField.gameObject.SetActive(false);
        inputField.interactable = false;
        inputField.DeactivateInputField();
    }
}