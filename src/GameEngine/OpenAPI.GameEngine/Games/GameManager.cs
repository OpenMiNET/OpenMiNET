using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using MiNET.Utils;
using OpenAPI.GameEngine.Games.Configuration;
using OpenAPI.GameEngine.Models.Games;

namespace OpenAPI.GameEngine.Games
{
    public class GameManager
    {
        private Type Default { get; set; }
        private ConcurrentDictionary<Type, GameEntry> Games { get; }
        
        public GameManager()
        {
            Games = new ConcurrentDictionary<Type, GameEntry>();
        }

        public bool RegisterGame<TGame>(Func<IGameOwner, TGame> gameFactory) where TGame : Game
        {
            var type = typeof(TGame);
            return Games.TryAdd(type, new GameEntry<TGame>(gameFactory));
        }
        
        public bool RegisterGame<TGame>() where TGame : Game
        {
            ConstructorInfo suitableConstructor = null;
            
            var type = typeof(TGame);
            foreach (var constructor in type.GetConstructors())
            {
                if (!constructor.IsPublic)
                    continue;
                
                var parameters = constructor.GetParameters();
                if (parameters.Length != 1)
                    continue;
                
                if (parameters[0].GetType() != typeof(IGameOwner))
                    continue;

                suitableConstructor = constructor;
                break;
            }

            if (suitableConstructor == null)
            {
                throw new Exception("Could not find suitable constructor!");
            }
            
            return RegisterGame<TGame>((owner) => (TGame) suitableConstructor.Invoke(new object[]
            {
                owner
            }));
        }

        public void RemoveGame(GameEntry entry)
        {
            if (Games.TryRemove(entry.GameType, out var games))
            {
                games.Destroy();
            }
        }

        public void RemoveGame(Type type)
        {
            if (Games.TryGetValue(type, out var entry))
            {
                RemoveGame(entry);
            }
        }

        public void RemoveGame<TType>()
        {
            RemoveGame(typeof(TType));
        }

        public bool TryGetGame(string game, out GameEntry entry)
        {
            entry = default;
            
            var gEntry = Games.Values.FirstOrDefault(x =>
                x.Info.Name.Equals(game, StringComparison.InvariantCultureIgnoreCase));
            
            if (gEntry == null)
                return false;

            entry = gEntry;
            return true;
        }

        public GameEntry GetGame(string name)
        {
            if (!TryGetGame(name, out GameEntry entry))
                throw new GameNotFoundException(name);

            return entry;
        }

        public GameEntry GetDefault()
        {
            if (Games.TryGetValue(Default, out var defaultGame))
            {
                return defaultGame;
            }

            return null;
        }
        
        public void SetDefault<TGame>() where TGame : Game
        {
            Default = typeof(TGame);
        }
    }

    public class GameEntry : IGameOwner
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameEntry));
        
        public GameInfo Info { get; }
        private ConcurrentDictionary<string, Game> Instances { get; }
        
        internal Type GameType { get; }
        private long _instanceCounter = 0;
        
        private HighPrecisionTimer Ticker { get; set; }
        
        protected GameEntry(Type type)
        {
            GameType = type;
            Instances = new ConcurrentDictionary<string, Game>();
            Ticker = new HighPrecisionTimer(50, Tick);
            
            Info = ResolveInfo();
        }

        private void Tick(object obj)
        {
            foreach (var game in Instances.Values
                .Where(x => x.State == GameState.WaitingForPlayers 
                            || x.State == GameState.InProgress 
                            ||
                           x.State == GameState.Finished).ToArray())
            {
                game.Tick();
            }
        }

        private GameInfo ResolveInfo()
        {
            var instance = CreateInstance();
            
            GameInfo info = new GameInfo()
            {
                Name = string.IsNullOrWhiteSpace(instance.Config?.Name) ? GameType.Name : instance.Config.Name,
                Author = string.Empty,
                Version = string.Empty
            };
            
            var gameAttribute = GameType.GetCustomAttribute<GameAttribute>();
            if (gameAttribute != null)
            {
                if (!string.IsNullOrWhiteSpace(gameAttribute.Name))
                {
                    info.Name = gameAttribute.Name;
                }

                if (!string.IsNullOrWhiteSpace(gameAttribute.Author))
                {
                    info.Author = gameAttribute.Author;
                }

                if (!string.IsNullOrWhiteSpace(gameAttribute.Version))
                {
                    info.Version = gameAttribute.Version;
                }
            }
            
            instance.Dispose();
            
            return info;
        }

        public bool TryGetInstance(string name, out Game instance)
        {
            return Instances.TryGetValue(name, out instance);
        }
        
        public bool TryGetInstance(GameState state, out Game instance)
        {
            instance = Instances.FirstOrDefault(x => x.Value.State == state).Value;

            return instance != null;
        }

        protected virtual Game CreateInstance()
        {
            throw new NotImplementedException();
        }

        public void AddInstance(Game game)
        {
            if (game.State != GameState.Created)
                throw new InvalidGameStateException($"Cannot add a game that is not in the Created state");

            if (Instances.Values.Contains(game))
                throw new DuplicateGameInstanceException($"This instance has already been added!");

            var incremented = Interlocked.Increment(ref _instanceCounter);
            string instanceName = $"{Info.Name}:{incremented}";

            Instances.TryAdd(instanceName, game);
        }

        public bool TryRemoveInstance(Game game)
        {
            if (string.IsNullOrWhiteSpace(game.InstanceName))
                return false;
            
            if (!Instances.TryRemove(game.InstanceName, out var _))
                return false;
            
            return true;
        }

        private void CheckInstances()
        {
            Game[] instances;

            instances = Instances.Values.ToArray();

            var hasJoinableGames = instances.Any(x => x.State == GameState.WaitingForPlayers);

            if (!hasJoinableGames)
            {
                var newInstance = CreateInstance();

                AddInstance(newInstance);

                newInstance.Initialize();
            }
        }

        public void StateChanged(Game game, GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Ready:
                    Log.Info($"Game instance initialized...");

                    game.State = GameState.WaitingForPlayers;
                    break;
                case GameState.WaitingForPlayers:
                    Log.Info($"Game instance is waiting for players...");
                    break;
                case GameState.Starting:

                    break;
                case GameState.InProgress:
                    CheckInstances();
                    break;
                case GameState.Finished:
                    
                    break;
                case GameState.Empty:
                    TryRemoveInstance(game);
                    CheckInstances();
                    break;
            }
        }

        internal void Destroy()
        {
            Ticker.Dispose();
            
            Game[] instances;

            instances = Instances.Values.ToArray();

            foreach (var instance in instances)
            {
                instance.Dispose();
            }
        }
    }
    
    public class GameEntry<TGame> : GameEntry where TGame : Game
    {
        private Func<IGameOwner, TGame> Generator { get; }
        public GameEntry(Func<IGameOwner, TGame> generator) : base(typeof(TGame))
        {
            Generator = generator;
        }
        
        protected override Game CreateInstance()
        {
            return Generator(this);
        }
    }
}