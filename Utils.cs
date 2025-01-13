
using System.IO;

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
	}
}