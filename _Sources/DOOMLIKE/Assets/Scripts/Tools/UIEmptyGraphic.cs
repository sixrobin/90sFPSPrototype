﻿namespace Doomlike
{
	public class UIEmptyGraphic : UnityEngine.UI.Graphic
	{
		protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
		{
			vh.Clear();
		}
	}
}