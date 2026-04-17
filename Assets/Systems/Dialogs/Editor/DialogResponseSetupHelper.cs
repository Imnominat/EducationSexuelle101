using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogs.Editor
{
	/// <summary>
	/// Helper script to assist in setting up the response system for DialogUI.
	/// Provides utility methods and quick setup options.
	/// </summary>
	public class DialogResponseSetupHelper : EditorWindow
	{
		private DialogUI targetDialogUI;
		private Button responseButtonTemplate;

		[MenuItem("Window/Dialog System/Response Setup Helper")]
		public static void ShowWindow()
		{
			GetWindow<DialogResponseSetupHelper>("Response Setup");
		}

		private void OnGUI()
		{
			GUILayout.Label("Dialog Response System Setup", EditorStyles.boldLabel);
			GUILayout.Space(10);

			targetDialogUI = EditorGUILayout.ObjectField("Target DialogUI", targetDialogUI, typeof(DialogUI), true) as DialogUI;
			
			if (targetDialogUI == null)
			{
				EditorGUILayout.HelpBox("Select a DialogUI component to get started.", MessageType.Info);
				return;
			}

			GUILayout.Space(10);
			EditorGUILayout.LabelField("Quick Setup Steps", EditorStyles.boldLabel);
			
			// Step 1: Create Response Button Prefab
			GUILayout.Label("Step 1: Create Response Button Prefab", EditorStyles.miniLabel);
			if (GUILayout.Button("Create Response Button Prefab", GUILayout.Height(30)))
			{
				CreateResponseButtonPrefab();
			}

			GUILayout.Space(10);

			// Step 2: Setup Container
			GUILayout.Label("Step 2: Setup Response Container", EditorStyles.miniLabel);
			
			var responseContainer = serializedObject?.FindProperty("m_ResponseContainer");
			if (GUILayout.Button("Create Response Container (Panel)", GUILayout.Height(30)))
			{
				CreateResponseContainer();
			}

			GUILayout.Space(10);

			// Step 3: Info about ResponseData
			GUILayout.Label("Step 3: Create ResponseData Assets", EditorStyles.miniLabel);
			EditorGUILayout.HelpBox(
				"Right-click in Project → Create → DialogSystem → Response\n" +
				"Create as many ResponseData assets as you need.",
				MessageType.Info
			);

			GUILayout.Space(10);

			// Step 4: Info about assigning to DialogUI
			GUILayout.Label("Step 4: Assign References in DialogUI Inspector", EditorStyles.miniLabel);
			EditorGUILayout.HelpBox(
				"In the DialogUI component:\n" +
				"1. Assign the Response Button Prefab\n" +
				"2. Assign the Response Container\n" +
				"3. Then add ResponseData to your DialogLogics in the Conversation list",
				MessageType.Info
			);

			GUILayout.Space(10);
			EditorGUILayout.HelpBox(
				"For more information, see RESPONSES_SETUP_GUIDE.md",
				MessageType.Warning
			);
		}

		private void CreateResponseButtonPrefab()
		{
			// Create a new GameObject with Button component
			GameObject buttonGO = new GameObject("ResponseButton");
			Button button = buttonGO.AddComponent<Button>();
			Image buttonImage = buttonGO.AddComponent<Image>();
			buttonImage.color = new Color(0.2f, 0.7f, 1f, 1f); // Light blue

			// Add LayoutElement for sizing
			LayoutElement layoutElement = buttonGO.AddComponent<LayoutElement>();
			layoutElement.preferredHeight = 60;
			layoutElement.layoutPriority = 1;

			// Create text child
			GameObject textGO = new GameObject("Text");
			textGO.transform.SetParent(buttonGO.transform);
			textGO.transform.localPosition = Vector3.zero;
			
			Image textImage = textGO.AddComponent<Image>();
			textImage.raycastTarget = false;
			
			TextMeshProUGUI textMPUGUI = textGO.AddComponent<TextMeshProUGUI>();
			textMPUGUI.text = "Response";
			textMPUGUI.fontSize = 36;
			textMPUGUI.alignment = TextAlignmentOptions.Center;
			textMPUGUI.color = Color.white;

			// Setup RectTransform for text
			RectTransform textRect = textGO.GetComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.offsetMin = Vector2.zero;
			textRect.offsetMax = Vector2.zero;

			// Setup button navigation
			button.navigation = new Navigation() { mode = Navigation.Mode.None };

			// Save as prefab
			string savePath = EditorUtility.SaveFilePanel(
				"Save Response Button Prefab",
				"Assets",
				"ResponseButton.prefab",
				"prefab"
			);

			if (string.IsNullOrEmpty(savePath))
			{
				DestroyImmediate(buttonGO);
				return;
			}

			savePath = FileUtil.GetProjectRelativePath(savePath);
			PrefabUtility.SaveAsPrefabAsset(buttonGO, savePath);
			DestroyImmediate(buttonGO);

			EditorUtility.DisplayDialog("Success", $"Response Button Prefab created at:\n{savePath}", "OK");
		}

		private void CreateResponseContainer()
		{
			// Find or create a Canvas
			Canvas canvas = FindObjectOfType<Canvas>();
			if (canvas == null)
			{
				EditorUtility.DisplayDialog("Error", "No Canvas found in scene. Please create a Canvas first.", "OK");
				return;
			}

			// Create container
			GameObject containerGO = new GameObject("ResponseContainer");
			containerGO.transform.SetParent(canvas.transform);
			if (targetDialogUI != null)
			{
				Transform dialogUITransform = targetDialogUI.transform;
				containerGO.transform.SetSiblingIndex(dialogUITransform.GetSiblingIndex() + 1);
			}

			RectTransform rectTransform = containerGO.AddComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0.5f, 0);
			rectTransform.anchorMax = new Vector2(0.5f, 0);
			rectTransform.sizeDelta = new Vector2(400, 300);
			rectTransform.anchoredPosition = new Vector2(0, 50);

			// Add vertical layout group
			VerticalLayoutGroup layoutGroup = containerGO.AddComponent<VerticalLayoutGroup>();
			layoutGroup.childForceExpandHeight = true;
			layoutGroup.childControlSize = true;
			layoutGroup.spacing = 10;
			layoutGroup.childScaleHeight = true;

			// Assign to DialogUI if possible
			if (targetDialogUI != null)
			{
				SerializedObject serializedDialogUI = new SerializedObject(targetDialogUI);
				SerializedProperty responseContainerProp = serializedDialogUI.FindProperty("m_ResponseContainer");
				if (responseContainerProp != null)
				{
					responseContainerProp.objectReferenceValue = containerGO.transform;
					serializedDialogUI.ApplyModifiedProperties();
				}
			}

			EditorUtility.DisplayDialog("Success", "Response Container created and assigned to DialogUI!", "OK");
		}
	}
}
