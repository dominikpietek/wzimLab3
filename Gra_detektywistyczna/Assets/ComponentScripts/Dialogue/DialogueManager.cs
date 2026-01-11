using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using DTOModel;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
public
class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

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

    private bool isDialogueFullyPrinted = false;


    void Awake()
    {
        Instance = this;
        dialoguesQueue = new Queue<Dialogue>();
        dialoguesHistory = new List<Dialogue>();
    }
    /*void Update()
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
    }*/
    /*void Update()
    {
        // 1. BEZWZGLĘDNA BLOKADA
        // Jeśli czekamy na odpowiedź NPC, kończymy funkcję natychmiast.
        // Żaden kod poniżej się nie wykona, więc Enter nie ma prawa zadziałać.
        if (isAwaitingNPCResponse)
        {
            return;
        }

        // 2. Wykrycie Entera (Input System)
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            // Sytuacja A: Gracz wpisał tekst i chce wysłać
            if (isAwaitingUserInput)
            {
                // Sprawdzamy czy tekst nie jest pusty (opcjonalne, ale zalecane)
                if (string.IsNullOrWhiteSpace(inputField.text)) return;

                HandlePlayerInputSubmission(); // Wydzieliłem to do osobnej funkcji dla czytelności
            }
            // Sytuacja B: Gracz chce przewinąć zwykły dialog
            else
            {
                PlayDialogue();
            }
        }
    }*/
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            // Blokada Entera podczas oczekiwania na AI
            if (isAwaitingNPCResponse)
            {
                return;
            }

            // Enter działa tylko gdy czekasz na wpis gracza
            if (isAwaitingUserInput)
            {
                HandlePlayerInputSubmission();
            }
            // Enter działa tylko gdy cała odpowiedź została wyświetlona
            else if (isDialogueFullyPrinted)
            {
                PlayDialogue();
            }
            // W każdym innym przypadku Enter jest ignorowany
        }
    }
    private void HandlePlayerInputSubmission()
    {
        // Wyłącz obsługę skryptu (Update przestanie działać)
        this.enabled = false;
        isAwaitingNPCResponse = true; // Flaga dla porządku


        // 1. ZABEZPIECZ STAN (Najważniejsze!)
        // Od razu ustawiamy flagi tak, żeby metoda Update w następnej klatce wiedziała, że ma blokować.
        isAwaitingUserInput = false;
        isAwaitingNPCResponse = true;

        // 2. ZABEZPIECZ DANE
        string userMessage = inputField.text;

        // Tworzymy DTO
        NPCRequestDTO npcRequestDTO = new NPCRequestDTO()
        {
            SceneDescription = sceneContext,
            UserText = userMessage,
            NPCName = currentNpcName,
        };

        // 3. AKTUALIZACJA UI
        // Dodajemy wpis do historii
        dialoguesHistory.Add(new Dialogue("Ty", userMessage));
        nameText.text = "Narrator"; // Zmieniamy nazwę

        // Ważne: To wywołuje naszą nową funkcję czyszczącą EventSystem (z Kroku 1)
        DisableUserInput();

        // Pokazujemy kropki "myślenia"
        ShowLoadingResponse();

        // 4. DOPIERO TERAZ WYSYŁAMY ZAPYTANIE
        // Dzięki temu, nawet jak sieć laguje, flagi (pkt 1) już dawno zablokowały Enter.
        SendNpcRequest(npcRequestDTO);
    }
    public void DisableUserInput()
    {
        inputField.text = "";
        inputField.interactable = false;
        inputField.DeactivateInputField();
        inputField.gameObject.SetActive(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
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
            PromptPlayer();
        }
        else
        {
            DisplayDialogue(dialog);
        }
    }
    public void PromptPlayer()
    {
        HideDialogueText();
        currentNpcName = dialoguesQueue.Peek().name;
        nameText.text = "Ty";
        EnableUserInput();
        isAwaitingUserInput = true;
    }
    /*public async void SendNpcRequest(NPCRequestDTO npcRequestDTO)
    {
        DialogueContextManager.AddPlayerDialogue("Ty", npcRequestDTO.UserText);
        NPCResponseDTO response =
            await DialogueEngineManager.Instance.AskNPCAsync(npcRequestDTO);
        StopAllCoroutines();
        isAwaitingNPCResponse = false;
        dialoguesQueue.Peek().sentence = response.Speech;
        dialoguesQueue.Peek().name = currentNpcName;
        DialogueContextManager.AddNPCDialogue(currentNpcName, response.Speech);
        PlayDialogue();
    }*/
    public async void SendNpcRequest(NPCRequestDTO npcRequestDTO)
    {
        // 1. DLA PEWNOŚCI: Ustaw flagę ponownie tutaj
        isAwaitingNPCResponse = true;

        Debug.Log("[AI] Rozpoczynam wysyłanie...");
        DialogueContextManager.AddPlayerDialogue("Ty", npcRequestDTO.UserText);

        try
        {
            // 2. Symuluj minimalne opóźnienie, żeby flagi zdążyły się "ułożyć" w silniku Unity
            await System.Threading.Tasks.Task.Delay(100);

            // Właściwe zapytanie
            NPCResponseDTO response = await DialogueEngineManager.Instance.AskNPCAsync(npcRequestDTO);

            Debug.Log("[AI] Przyszła odpowiedź!");

            // Przetwarzanie odpowiedzi
            if (dialoguesQueue.Count > 0)
            {
                dialoguesQueue.Peek().sentence = response.Speech;
                dialoguesQueue.Peek().name = currentNpcName;
            }
            DialogueContextManager.AddNPCDialogue(currentNpcName, response.Speech);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AI ERROR] {e.Message}");
            // Opcjonalnie wstaw tekst błędu do dialogu, żeby gracz wiedział co się stało
        }
        finally
        {
            Debug.Log("[AI] Koniec operacji - ODBLOKOWUJĘ Enter.");
            StopAllCoroutines();

            // KLUCZOWE: Dopiero tutaj zdejmujemy blokadę
            isAwaitingNPCResponse = false;
            this.enabled = true;
            PlayDialogue();
        }
    }
    public void DisplayDialogue(Dialogue dialogue)
    {
        dialoguesHistory.Add(dialogue);
        currentNpcName = dialogue.name;
        dialogueText.text = dialogue.sentence;
        nameText.text = dialogue.name;
        isDialogueFullyPrinted = false;
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
        isDialogueFullyPrinted = true;
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
        string baseText = currentNpcName + " myśli";
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
    /*public void DisableUserInput()
    {
        inputField.text = "";
        inputField.gameObject.SetActive(false);
        inputField.interactable = false;
        inputField.DeactivateInputField();
    }*/
}