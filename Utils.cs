using Satchel;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Various utility functions
/// </summary>

namespace CarThingMod
{
	internal static class Utils
	{
		/// <summary>
		/// Copies a file from the embedded resources into a directory.
		/// </summary>
		/// <param name="directory">Where the copied file should go</param>
		/// <param name="resourceDirectory">Name of the file from the embedded resources</param>
		/// <param name="debugResourceName">The name of the item used when outputting an info message</param>
		internal static void CopyFromResources(string directory, string resourceDirectory, 
			string debugResourceName = "resource")
		{
			var path = Path.Combine(directory, resourceDirectory);
			if (!File.Exists(path))
			{
				// Add sample car file
				File.WriteAllBytes(path,
					Satchel.AssemblyUtils.GetBytesFromResources(resourceDirectory));

				Modding.Logger.Log("[CarThingMod] Created " + debugResourceName + " at " + directory);
			}
		}

		internal static CarClass CreateCar(string texturePath, string settingsPath, string carDirectory)
		{
			// Get the texture from file
            Texture2D texture = TextureUtils.LoadTextureFromFile(
				Path.Combine(carDirectory, texturePath));

            // Get the settings from file
            // https://stackoverflow.com/questions/7387085/how-to-read-an-entire-file-to-a-string-using-c
            string settings;
			using (StreamReader sr = new StreamReader(settingsPath, Encoding.UTF8))
			{
				settings = sr.ReadToEnd();
			}

			// Initialize variables for parsing settings file
			float colSizeX = 0f;
			float colSizeY = 0f;
			float offsetX = 0f;
			float offsetY = 0f;
			float ppu = 0f;
            string colSizeXName = "colXSize: ";
            string colSizeYName = "colYSize: ";
            string offsetXName = "colXOffset: ";
            string offsetYName = "colYOffset: ";
			string ppuName = "pixelsPerUnit: ";

            // ----- Parse settings file -----

            // Collider size X
            colSizeX = ParseFloat(colSizeXName, ref settings);

            // Collider size Y
            colSizeY = ParseFloat(colSizeYName, ref settings);

            // Offset X
            offsetX = ParseFloat(offsetXName, ref settings);

            // Offset Y
            offsetY = ParseFloat(offsetYName, ref settings);

            // Pixels per unit
            ppu = ParseFloat(ppuName, ref settings);

            // Get sprite from texture
            Sprite spr = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);

			// Create and return final car
            CarClass car = new CarClass(colSizeX, colSizeY, offsetX, offsetY, ppu, spr);
			return car;
        }

        /// <summary>
        /// Helper function for parsing the settings files.
        /// Reads in the name of the title (text before value in
        /// settings file) and the settings file as a string.
        /// Returns a float value, and removes that part of the
        /// string.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        static float ParseFloat(string title, ref string settings)
        {
            string subStr = "";

            // Check if this is the end of the file
            if (settings.IndexOf('\n') == -1)
            {
                // This is the end, so the substring extends to the end
                subStr = settings.Substring(
                    settings.IndexOf(title) + title.Length);
            }
            else
            {
                // This is not the end, so the substring extends to the next line break
                subStr = settings.Substring(
                    settings.IndexOf(title) + title.Length,
                    settings.IndexOf('\n') - title.Length - 1);
            }

            // Convert the substring to a float
            float value = float.Parse(subStr);

            //Remove previous line of string (not needed anymore)
            settings = settings.Remove(0, 
                settings.IndexOf('\n') + 1);

            // Return final value
            return value;
        }
	}
}