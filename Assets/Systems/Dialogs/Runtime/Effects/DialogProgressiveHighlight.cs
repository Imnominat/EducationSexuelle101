using TMPro;
using UnityEngine;
using System.Collections;

namespace Dialogs.Effects
{
	public class DialogProgressiveHighlight : DialogEffect
	{
		public enum ColorPreservation
		{
			/// <summary>Doesn't preserve color tags.</summary>
			[Tooltip("Does not preserve color tags.")]
			Override,

			/// <summary>Preserve the color of characters in a color tags as is.</summary>
			[Tooltip("Preserve color tags as is.")]
			Preserve,

			/// <summary>Tints the characters in color tags with <see cref="HighlightColor"/>.</summary>
			[Tooltip("Tints the characters in color tags with the Highlight Color.")]
			Blend
		}


		[Tooltip("The color shown over time.")]
		public Color32 HighlightColor = Color.yellow;

		[Tooltip("How does the Highlight color interact with rich text color tags.")]
		public ColorPreservation colorPreservation = ColorPreservation.Preserve;

		/// <summary>The rate of progression in characters per second.</summary>
		[Tooltip("The rate of progression in characters per second.")]
		public float ReadingSpeed = 25f;

		/// <summary>The current index that is highlighted.</summary>
		public int HighlightedIndex { get; private set; }
		public bool IsActive => coroutine != null;
		private Coroutine coroutine = null;

		private IEnumerator HighlightCoroutine()
		{
			TMP_Text textUI = p_DialogUI.DialogLabel; // p_DialogUI is protected in DialogEffect
			textUI.ForceMeshUpdate();
			Color32 baseColor = textUI.color;

			TMP_TextInfo textInfo = textUI.textInfo;
			int totalChars = textInfo.characterCount;
			TMP_CharacterInfo[] charactersInfos = textInfo.characterInfo;

			HighlightedIndex = -1;

			while (++HighlightedIndex < totalChars)
			{
				// if (!charactersInfos[HighlightedIndex].isVisible)
				// 	continue;

				int meshIndex = charactersInfos[HighlightedIndex].materialReferenceIndex;
				int vertexIndex = charactersInfos[HighlightedIndex].vertexIndex;

				// The colors of the 4 vertex (corners) of a character image
				Color32[] colors = textInfo.meshInfo[meshIndex].colors32;


				if (colorPreservation == ColorPreservation.Override)
				{
					for (int vertex = 0; vertex < 4; vertex++)
						colors[vertex] = HighlightColor;
				}
				else if (colorPreservation == ColorPreservation.Preserve)
				{
					for (int v = 0; v < 4; v++)
					{
						if (colors[vertexIndex + v].Equals(baseColor))
						{
							colors[vertexIndex + v] = HighlightColor;
						}
					}
				}
				else if (colorPreservation == ColorPreservation.Blend)
				{
					for (int vertex = 0; vertex < 4; vertex++)
					{
						Color32 original = colors[vertex];
						colors[vertex] = new Color32(
							(byte)((original.r + HighlightColor.r) >> 1),
							(byte)((original.g + HighlightColor.g) >> 1),
							(byte)((original.b + HighlightColor.b) >> 1),
							original.a
						);
					}
				}

				textUI.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

				yield return new WaitForSeconds(1f / ReadingSpeed);
			}

			coroutine = null;
		}

		public override void StartEffect()
		{
			if (IsActive) return;
			coroutine = StartCoroutine(HighlightCoroutine());
		}

		public override void StopEffect()
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
				coroutine = null;
			}
		}

		public override void ResetEffect()
		{
			StopEffect();
			p_DialogUI.DialogLabel.ForceMeshUpdate();
			HighlightedIndex = 0;
		}
	}
}