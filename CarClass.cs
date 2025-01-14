using UnityEngine;

namespace CarThingMod
{
	public class CarClass
	{
		public float colSizeX;
		public float colSizeY;
		public float colOffsetX;
		public float colOffsetY;
		public float pixelsPerUnit;
		public Sprite carSprite;

		public CarClass(float sizeX, float sizeY, float offsetX, float offsetY, 
			float ppu, Sprite sprite) 
		{
			colSizeX = sizeX;
			colSizeY = sizeY;
			colOffsetX = offsetX;
			colOffsetY = offsetY;
			pixelsPerUnit = ppu;
			carSprite = sprite;
		}
	}
}