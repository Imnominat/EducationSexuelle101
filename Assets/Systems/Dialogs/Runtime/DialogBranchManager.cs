using Dialogs;
using UnityEngine;

public class DialogBranchManager : MonoBehaviour
{
    [SerializeField] private DialogUI dialogUI;

    void Start()
    {
        // Quand on clique un bouton de réponse
        foreach (var dialog in dialogUI.Conversation)
        {
            dialog.OnResponseSelected.AddListener(HandleResponse);
        }
    }

    void HandleResponse(int responseIndex)
    {
        // Récupérer le dialogue actuel
        DialogLogic current = dialogUI.CurrentDialogLogic;
        
        // Si c'est Dialogue_1
        if (current.ID == "dialogue_1")
        {
            // responseIndex = 0, 1, ou 2 (quel bouton a été cliqué)
            if (responseIndex == 0) // Bouton "Oui"
            {
                dialogUI.StartConversation("dialogue_2");
            }
            else if (responseIndex == 1) // Bouton "Non"
            {
                dialogUI.StartConversation("dialogue_3");
            }
            else if (responseIndex == 2) // Bouton "Peut-être"
            {
                dialogUI.StartConversation("dialogue_4");
            }
        }
    }
}