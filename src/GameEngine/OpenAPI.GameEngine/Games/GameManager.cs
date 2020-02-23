using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenAPI.GameEngine.Games
{
    public class GameManager
    {
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
    }

    public class GameEntry : IGameOwner
    {
        private List<Game> Instances { get; }
        private object _lock = new object();
        
        protected GameEntry()
        {
            Instances = new List<Game>();
        }

        public bool TryGetInstance(GameState state, out Game instance)
        {
            lock (_lock)
            {
                instance = Instances.FirstOrDefault(x => x.State == state);
            }

            return instance != null;
        }

        protected virtual Game CreateInstance()
        {
            throw new NotImplementedException();
        }

        public bool TryAddInstance(Game game)
        {
            lock (_lock)
            {
                if (Instances.Contains(game))
                    return false;
                
                
                Instances.Add(game);
            }

            return true;
        }

        public bool TryRemoveInstance(Game game)
        {
            lock (_lock)
            {
                if (!Instances.Contains(game))
                    return false;

                Instances.Remove(game);
            }

            return true;
        }

        public void CheckInstances()
        {
            Game[] instances;
            lock (_lock)
            {
                instances = Instances.ToArray();
            }

            var hasJoinableGames = instances.Any(x => x.State == GameState.WaitingForPlayers);

            if (!hasJoinableGames)
            {
                TryAddInstance(CreateInstance());
            }
        }

        public void StateChanged(Game game, GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Ready:
                    
                    break;
                case GameState.WaitingForPlayers:
                    
                    break;
                case GameState.Starting:
                    
                    break;
                case GameState.InProgress:
                    
                    break;
                case GameState.Finished:
                    
                    break;
                case GameState.Empty:
                    TryRemoveInstance(game);
                    break;
            }
        }
    }
    
    public class GameEntry<TGame> : GameEntry where TGame : Game
    {
        private Func<IGameOwner, TGame> Generator { get; }
        public GameEntry(Func<IGameOwner, TGame> generator)
        {
            Generator = generator;
        }
        
        protected override Game CreateInstance()
        {
            return Generator(this);
        }
    }
}