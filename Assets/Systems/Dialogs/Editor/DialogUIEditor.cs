using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dialogs.Editor
{
	[CustomEditor(typeof(DialogUI))]
	public class DialogUIEditor : UnityEditor.Editor
	{
		private const string P_CONVERSATION = "Conversation";
		private const string P_RESPONSES = "Responses";
		private SerializedProperty list;
		private ReorderableList reorderableList;
		private ReorderableList responsesReorderableList;
		private int selectedIndex = -1;

		void OnEnable()
		{
			list = serializedObject.FindProperty(P_CONVERSATION);

			reorderableList = new ReorderableList(serializedObject, list, true, true, true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, P_CONVERSATION);
				},

				drawElementCallback = (rect, index, active, focused) =>
				{
					EditorGUI.LabelField(rect, GetName(index));
				},

				onSelectCallback = l =>
				{
					selectedIndex = l.index;
				}
			};
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var prop = serializedObject.GetIterator();
			prop.NextVisible(true);
			while (prop.NextVisible(false))
			{
				if (prop.name != P_CONVERSATION)
				{
					EditorGUILayout.PropertyField(prop, true);
					continue;
				}
				reorderableList.DoLayoutList();
				if (selectedIndex >= 0 && selectedIndex < list.arraySize)
				{
					var element = list.GetArrayElementAtIndex(selectedIndex);
					EditorGUILayout.PropertyField(element, new GUIContent("Dialog Logic"), true);

					// Display responses separately with better formatting
					var responsesProperty = element.FindPropertyRelative(P_RESPONSES);
					if (responsesProperty != null && responsesProperty.isArray)
					{
						EditorGUILayout.Space(10);
						EditorGUILayout.LabelField("Responses", EditorStyles.boldLabel);
						
						EditorGUI.indentLevel++;
						EditorGUILayout.PropertyField(responsesProperty, true);
						EditorGUI.indentLevel--;

						if (responsesProperty.arraySize == 0)
						{
							EditorGUILayout.HelpBox(
								"No responses defined. The dialog will use standard behavior (Validate, Cancel, etc.).",
								MessageType.Info
							);
						}
						else
						{
							EditorGUILayout.HelpBox(
								$"{responsesProperty.arraySize} response(s) defined. Response buttons will be displayed instead of standard buttons.",
								MessageType.Info
							);
						}
					}
				}
			}
			serializedObject.ApplyModifiedProperties();
		}


		private string GetName(SerializedProperty element)
		{
			var IDProp = element.FindPropertyRelative("_id");
			return IDProp == null ? "None" : IDProp.stringValue;
		}
		private string GetName(int index) => GetName(list.GetArrayElementAtIndex(index));
	}
}