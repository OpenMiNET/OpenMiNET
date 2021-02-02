using JetBrains.Annotations;
using MiNET.Utils;

namespace OpenAPI.GameEngine.Models.Games
{
	public class GameStageConfiguration
	{
		[CanBeNull] public PlayerLocation Spawn { get; set; }
	}
}