# Dialog Response System - Quick Start

## 5 Étapes pour Commencer

### ⚡ Étape 1: Setup Automatique (Recommandé)

1. Ouvrez **Window → Dialog System → Response Setup Helper**
2. Assignez votre **DialogUI** component
3. Cliquez **"Create Response Button Prefab"**
   - Choisissez où sauvegarder (ex: Assets/Prefabs/)
4. Cliquez **"Create Response Container (Panel)"**
   - Cela créé automatiquement un conteneur et l'assigne

✅ **Done!** Les références sont maintenant configurées.

---

### 2️⃣ Étape 2: Créer des Réponses

1. **Dans le Project:**
   - Clic droit sur un dossier → **Create → DialogSystem → Response**
   - Renommez: "Response_Yes", "Response_No", etc.
   - Éditez le texte: "Oui", "Non", etc.

Répétez pour chaque réponse possible que vous voulez.

---

### 3️⃣ Étape 3: Ajouter des Réponses aux Dialogues

1. **Dans l'inspecteur:**
   - Sélectionnez le GameObject avec **DialogUI**
   - Dans la liste **Conversation**, cliquez sur un dialogue
   - Descendez jusqu'à **Dialog Logic**
   - Trouvez **"Responses"** (la nouvelle section)
   - **Augmentez le Size** (ex: 2 pour 2 réponses)
   - Dragguez vos ResponseData dans les slots

**Avant:**
```
DialogLogic
├─ ID
├─ Dialog (vide)
└─ OnDialogValidate
```

**Après:**
```
DialogLogic
├─ ID
├─ Dialog
├─ Responses (2)
│  ├─ Element 0: Response_Yes
│  └─ Element 1: Response_No
└─ OnDialogValidate
```

---

### 4️⃣ Étape 4: Tester!

1. **Play dans l'éditeur**
2. Quand le dialogue avec réponses s'affiche, vous devriez voir les boutons
3. Cliquez sur un bouton

✅ **L'événement OnResponseSelected se déclenche!**

---

### 5️⃣ Étape 5 (Optionnel): Ajouter de la Logique

#### Option A: Via l'Inspecteur
1. Connectez **OnResponseSelected** à d'autres objets
2. Exemple: 
   - Connecter à `DialogUI.StartConversation(dialogID)`
   - Pour aller au dialogue suivant

#### Option B: Via un Script
Attachez [DialogResponseExample.cs](DialogResponseExample.cs) au même GameObject que DialogUI.
Il gère automatiquement les clics et vous pouvez ajouter votre logique.

---

## Vue d'Ensemble Complète

```
┌─────────────────────────────────────┐
│ DialogUI (inspecteur)               │
├─────────────────────────────────────┤
│ Conversation (ReorderableList)      │
│ ├─ DialogLogic #1                   │
│ │  ├─ ID: "intro"                   │
│ │  ├─ Dialog: DialogData_Intro      │
│ │  └─ Responses: (vide) ← affiche   │
│ │     boutons normaux               │
│ │                                   │
│ ├─ DialogLogic #2                   │
│ │  ├─ ID: "choice"                  │
│ │  ├─ Dialog: DialogData_Choice     │
│ │  └─ Responses: (2)                │
│ │     ├─ Response_Yes ─────────┐    │
│ │     └─ Response_No ──────┐   │    │
│ │                          │   │    │
│ └─ (... plus)              │   │    │
│                            │   │    │
│ Response UI References:    │   │    │
│ ├─ Response Container ─────┘   │    │
│ └─ Response Button Prefab ──────┤   │
│                                 │   │
│ Ils affichent les boutons ◄─────┴───┘
└─────────────────────────────────────┘
```

---

## Vérification Rapide

**Les boutons n'apparaissent pas?**

Checklist:
- [ ] Le dialogue a au moins une ResponseData dans "Responses"?
- [ ] DialogUI a **Response Container** assigné?
- [ ] DialogUI a **Response Button Prefab** assigné?
- [ ] Le préfab a un **Button** component?
- [ ] Le préfab a un **TextMeshProUGUI** enfant?

Si tout est ✅ et ça marche toujours pas → Vérifiez la console pour les warnings.

---

## Prochaines Étapes

**Maintenant que ça marche:**

1. **Ajouter de la logique:**
   - Lire [DialogResponseExample.cs](DialogResponseExample.cs)
   - Créer votre propre script qui écoute OnResponseSelected

2. **Style personnalisé:**
   - Modifiez le préfab (couleurs, sizes, fonts, etc.)
   - Ajoutez des animations aux boutons

3. **Arborescence de dialogue:**
   - Créez plusieurs dialogues avec des réponses
   - Connectez-les via `StartConversation(dialogID)`

4. **Effets:**
   - Les DialogEffects continuent de fonctionner
   - Les sons, animations, etc. s'appliquent aussi aux réponses

---

## Ressources

- [RESPONSES_SETUP_GUIDE.md](RESPONSES_SETUP_GUIDE.md) - Guide détaillé
- [CHANGES_LOG.md](CHANGES_LOG.md) - Détails techniques
- [DialogResponseExample.cs](DialogResponseExample.cs) - Exemples de code
- `Window → Dialog System → Response Setup Helper` - Menu d'aide
