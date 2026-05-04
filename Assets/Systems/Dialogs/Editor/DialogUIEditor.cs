using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dialogs.Editor
{
	[CustomEditor(typeof(DialogUI))]
	public class DialogUIEditor : UnityEditor.Editor
	{
		private const string P_CONVERSATION = "Conversation";
		private SerializedProperty list;
		private ReorderableList reorderableList;
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