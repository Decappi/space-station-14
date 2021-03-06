using Content.Client.GameObjects.EntitySystems.AI;
using JetBrains.Annotations;
using Robust.Client.Interfaces.Console;
using Robust.Shared.GameObjects.Systems;

namespace Content.Client.Commands
{
    /// <summary>
    /// This is used to handle the tooltips above AI mobs
    /// </summary>
    [UsedImplicitly]
    internal sealed class DebugAiCommand : IConsoleCommand
    {
        // ReSharper disable once StringLiteralTypo
        public string Command => "debugai";
        public string Description => "Handles all tooltip debugging above AI mobs";
        public string Help => "debugai [hide/paths/thonk]";

        public bool Execute(IDebugConsole console, params string[] args)
        {
#if DEBUG
            if (args.Length < 1)
            {
                return true;
            }

            var anyAction = false;
            var debugSystem = EntitySystem.Get<ClientAiDebugSystem>();

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "hide":
                        debugSystem.Disable();
                        anyAction = true;
                        break;
                    // This will show the pathfinding numbers above the mob's head
                    case "paths":
                        debugSystem.ToggleTooltip(AiDebugMode.Paths);
                        anyAction = true;
                        break;
                    // Shows stats on what the AI was thinking.
                    case "thonk":
                        debugSystem.ToggleTooltip(AiDebugMode.Thonk);
                        anyAction = true;
                        break;
                    default:
                        continue;
                }
            }

            return !anyAction;
#else
            return true;
#endif
        }
    }
}
