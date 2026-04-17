using TMPro;
using UnityEngine;

namespace Dialogs.Effects
{
	[RequireComponent(typeof(DialogUI))]
	public abstract class DialogEffect : MonoBehaviour
	{
		protected DialogUI p_DialogUI;
		public abstract void StartEffect();
		public abstract void StopEffect();
		public abstract void ResetEffect();
		void Awake()
		{
			p_DialogUI = GetComponent<DialogUI>();
		}
		void Reset()
		{
			p_DialogUI = GetComponent<DialogUI>();
		}
	}
}
