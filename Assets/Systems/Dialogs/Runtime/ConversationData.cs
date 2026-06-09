using System.Collections.Generic;
using UnityEngine;

namespace Dialogs
{
    [CreateAssetMenu(fileName = "NewConversation", menuName = "DialogSystem/Conversation")]
    public class ConversationData : ScriptableObject
    {
        public List<DialogLogic> Dialogs = new List<DialogLogic>();
    }
}
