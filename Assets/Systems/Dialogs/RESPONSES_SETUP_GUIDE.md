# Système de Réponses - Guide de Configuration

## Overview

Le système de dialogues a été étendu pour supporter les réponses possibles. Maintenant, un dialogue peut avoir plusieurs réponses sous forme de ScriptableObjects, affichées comme des boutons dans l'UI.

## Fichiers Modifiés/Créés

- **ResponseData.cs** (NOUVEAU) : ScriptableObject représentant une réponse possible
- **DialogLogic.cs** (MODIFIÉ) : Ajout de liste de réponses et événement `OnResponseSelected`
- **DialogUI.cs** (MODIFIÉ) : Logique pour afficher les boutons de réponse dynamiquement

## Configuration

### 1. Créer les ScriptableObjects de Réponse

1. Clic droit dans le Project → Create → DialogSystem → Response
2. Entrez le texte de la réponse que vous souhaitez afficher sur le bouton
3. (Optionnel) Ajoutez une icône

### 2. Créer un Préfab de Bouton de Réponse

1. Créez un GameObject avec un composant `Button` (UI Button)
2. Ajoutez un enfant TextMeshProUGUI pour afficher le texte
3. Positionnez et stylisez le bouton selon vos besoins
4. Dragguez-le dans le dossier Assets pour en faire un Préfab
5. Supprimez l'instance de la scène

### 3. Assigner les Références à DialogUI

1. Sélectionnez le GameObject contenant le composant DialogUI
2. Dans l'inspecteur, section "Response UI References":
   - **Response Container** : Le Transform où les boutons seront instantiés (ex: un Panel ou VerticalLayoutGroup)
   - **Response Button Prefab** : Le préfab de bouton créé à l'étape 2

### 4. Ajouter des Réponses à un Dialogue

1. Dans l'inspecteur de DialogUI
2. Sélectionnez un dialogue dans la liste Conversation
3. En bas, vous verrez une section "Responses"
4. Augmentez le size et assignez vos ScriptableObjects ResponseData

## Comportement

### Sans Réponses
Si la liste de réponses est vide, le dialogue s'affiche normalement avec les boutons standard (Validate, Cancel, Previous, Repeat).

### Avec Réponses
Si la liste de réponses contient au moins une réponse:
- Les boutons de réponse disparaissent quand vous changez de dialogue
- Des nouveaux boutons sont créés pour chaque réponse
- Le texte du bouton est le texte de ResponseData
- En cliquant sur une réponse, l'événement `OnResponseSelected` est invoqué avec l'index de la réponse

## Utilisation Avancée

### Gérer les Actions des Réponses

L'événement `OnResponseSelected` (dans DialogLogic) retourne l'index de la réponse sélectionnée:

```csharp
void OnResponseSelected(int responseIndex)
{
    ResponseData selectedResponse = CurrentDialogLogic.Responses[responseIndex];
    // Votre logique ici
}
```

Vous pouvez:
- Connecter cet événement à d'autres GameObjects via l'inspecteur
- Créer une classe de gestion qui écoute cet événement
- Décider quelle dialogue afficher suivant la réponse sélectionnée

### Exemple de Workflow

1. Dialogue: "Qu'allez-vous faire ?"
   - Réponse 1: "Aller à gauche"
   - Réponse 2: "Aller à droite"

2. Connecter `OnResponseSelected` pour charger le dialogue suivant applicable
3. Utiliser l'index pour déterminer la branche

## Notes Importants

- Le TextMeshProUGUI du bouton doit être enfant direct du bouton ou dans ses enfants directs
- Si le conteneur ou le préfab est null, un warning s'affichera et aucun bouton ne sera créé
- Les boutons de réponses remplacent complètement les boutons standard quand ils sont affichés
- Les effets de dialogue (DialogEffect) continueront de s'appliquer même avec des réponses
