using JetBrains.Annotations;
using MiNET.Utils;
using MiNET.Utils.Vectors;

namespace OpenAPI.GameEngine.Models.Games
{
	public class GameStageConfiguration
	{
		[CanBeNull] public PlayerLocation Spawn { get; set; }
	}
}