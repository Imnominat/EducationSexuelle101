# Dialog System - Response Feature Implementation

## Summary

Le système de dialogues a été étendu pour supporter les réponses possibles à chaque dialogue. Cela permet de créer des conversations avec des branches basées sur les choix de l'utilisateur.

## Nouveaux Fichiers

### 1. **ResponseData.cs**
- ✨ NOUVEAU ScriptableObject pour définir les réponses
- Propriétés:
  - `ResponseText` : Le texte affiché sur le bouton
  - `ResponseIcon` : (Optionnel) Une icône pour le bouton

**Utilisation:** Create → DialogSystem → Response

---

## Fichiers Modifiés

### 2. **DialogLogic.cs**
Changes:
- ✅ Ajout: `List<ResponseData> Responses` - Liste des réponses possibles pour ce dialogue
- ✅ Ajout: `UnityEvent<int> OnResponseSelected` - Événement appelé quand une réponse est sélectionnée (paramètre: index)
- 📝 Import: `using System.Collections.Generic;` 

**Comportement:**
- Si `Responses.Count == 0` : Le dialogue fonctionne normalement
- Si `Responses.Count > 0` : Des boutons de réponse sont affichés

---

### 3. **DialogUI.cs**
Changes:
- ✅ Ajout: `Transform m_ResponseContainer` - Conteneur pour les boutons
- ✅ Ajout: `Button m_ResponseButtonPrefab` - Préfab pour les boutons
- ✅ Ajout: `List<Button> _currentResponseButtons` - Suivi des boutons actifs
- ✅ Ajout: `DisplayResponseButtons(DialogLogic)` - Affiche les boutons de réponse
- ✅ Ajout: `ClearResponseButtons()` - Nettoie les anciens boutons
- ✅ Ajout: `OnResponseButtonClicked(int)` - Gère le clic sur un bouton
- ✅ Modified: `PlayConversation(DialogLogic)` - Appelle DisplayResponseButtons si nécessaire

**Logique:**
1. Quand un dialogue est affiché via `PlayConversation()`
2. Si le dialogue a des réponses, `DisplayResponseButtons()` est appelé
3. Des boutons sont créés dynamiquement à partir du préfab
4. Chaque bouton affiche le texte de sa ResponseData
5. Cliquer sur un bouton appelle `OnResponseSelected` avec l'index de la réponse

---

### 4. **DialogUIEditor.cs** (Amélioration optionnelle)
Changes:
- ✅ Ajout: Affichage dédié pour les réponses dans l'inspecteur
- ✅ Ajout: Messages d'info pour guider l'utilisateur
- ✅ Ajout: `P_RESPONSES` constant et `responsesReorderableList`
- 📝 Modified: `OnInspectorGUI()` pour afficher les réponses avec mise en forme

**Bénéfice:** Meilleure visibilité des réponses directement dans l'inspecteur

---

## Nouveau: DialogResponseSetupHelper.cs

✨ NOUVEAU script éditeur avec menu pour faciliter la configuration

**Menu:** Window → Dialog System → Response Setup Helper

Fonctionnalités:
- 🔧 Création automatique d'un préfab de bouton
- 🔧 Création automatique du conteneur de réponses
- 📖 Guide pas à pas intégré

---

## Documentation

### **RESPONSES_SETUP_GUIDE.md**
✨ NOUVEAU - Guide complet de configuration et utilisation

Contient:
- Comment créer des ScriptableObjects ResponseData
- Comment créer un préfab de bouton
- Comment configurer DialogUI
- Exemples d'utilisation avancée

---

## Workflow Complet

### Configuration Initiale (Une seule fois)

```
1. Window → Dialog System → Response Setup Helper
   ├─ Create Response Button Prefab
   └─ Create Response Container (Panel)

2. Dans DialogUI Inspector:
   ├─ Assigner le préfab de bouton → "Response Button Prefab"
   └─ Assigner le conteneur → "Response Container"
```

### Utilisation dans une Scène

```
1. Créer des ResponseData (Create → DialogSystem → Response)

2. Créer des DialogData normalement

3. Dans DialogUI Inspector:
   ├─ Ajouter une DialogLogic à la Conversation
   ├─ Assigner une DialogData
   └─ (OPTIONNEL) Ajouter des ResponseData à la liste "Responses"

4. Si des réponses sont ajoutées:
   └─ Connecter OnResponseSelected à d'autres systèmes
      (ex: charger le dialogue suivant, animer, etc.)
```

---

## Exemple Pratique

**Dialogue avec Branches:**

```
DialogLogic "QuestionDialog"
├─ DialogData: "Quelle est votre couleur préférée ?"
├─ Response 1: "Rouge"
│  └─ OnResponseSelected(0) → Load "RedDialog"
├─ Response 2: "Bleu"
│  └─ OnResponseSelected(1) → Load "BlueDialog"
└─ Response 3: "Vert"
   └─ OnResponseSelected(2) → Load "GreenDialog"
```

---

## Backwards Compatibility

✅ COMPLÈTEMENT COMPATIBLE
- Les dialogues existants sans réponses fonctionnent exactement comme avant
- Les événements existants (OnDialogValidate, etc.) continuent de fonctionner
- Aucune modification breaking

---

## Notes Techniques

### Destruction des Boutons
- Les boutons sont détruits avec `Destroy()` lors du changement de dialogue
- Les références sont maintenues dans `_currentResponseButtons` pour un nettoyage rapide

### Gestion des Events
- `OnResponseSelected(int)` est invoqué avec l'index de la réponse sélectionnée
- Les autres événements (OnDialogLoaded, etc.) continuent de fonctionner
- Vous pouvez combiner les événements pour une logique complexe

### Performance
- Les boutons sont créés/détruits dynamiquement (pas de pooling)
- Pour un nombre très élevé de changements rapidement, considérez un système de pooling

---

## Troubleshooting

### "Warning: ResponseContainer or ResponseButtonPrefab is not assigned"
**Solution:** Assignez les références dans l'inspecteur de DialogUI

### Les boutons ne s'affichent pas
**Vérifier:**
1. ✅ ResponseContainer est assigné
2. ✅ ResponseButtonPrefab est assigné
3. ✅ Le préfab a un Button et un TextMeshProUGUI enfant
4. ✅ ResponseData contient du texte

### Les textes des réponses ne s'affichent pas
**Solution:** Vérifiez que le TextMeshProUGUI du préfab est bien en enfant du Button

---

## Future Enhancements

Idées pour évolutions:
- [ ] Pooling des boutons pour meilleure performance
- [ ] Support d'images dans les réponses
- [ ] Animation des boutons (fade in/out)
- [ ] Support des réponses dynamiques générées par code
- [ ] Système de conditionnement des réponses
