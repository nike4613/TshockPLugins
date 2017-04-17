using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Utils;

namespace DeathWormholes
{
    [ApiVersion(2, 0)]
    public class Wormhole : TerrariaPlugin
    {
        /// <summary>
        /// Gets the author(s) of this plugin
        /// </summary>
        public override string Author
        {
            get { return "DaNike"; }
        }

        /// <summary>
        /// Gets the description of this plugin.
        /// A short, one lined description that tells people what your plugin does.
        /// </summary>
        public override string Description
        {
            get { return "Gives players one wormhole potion for a short time after death."; }
        }

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name
        {
            get { return "Death Wormholes"; }
        }

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version
        {
            get { return new Version(0, 0, 0, 0); }
        }

        public static string ConfigFile = Path.Combine(Resource.ConfigDir, Resource.ConfigFile);

        private class Config
        {
            public string WormholeRespawnTime;

            [JsonIgnore]
            public TimeSpan WormholeTimer;
        }

        private Config config;

        /// <summary>
        /// Initializes a new instance of the TestPlugin class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        /// </summary>
        public Wormhole(Main game) : base(game)
        {
            if (!File.Exists(ConfigFile))
            {
                Directory.CreateDirectory(Resource.ConfigDir);
                File.AppendAllText(ConfigFile, Resource.config);
            }

            string json = File.OpenText(ConfigFile).ReadToEnd();

            config = JsonConvert.DeserializeObject<Config>(json);
            config.WormholeTimer = config.WormholeRespawnTime.ToTimeSpan();
        }

        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        /// </summary
        public override void Initialize()
        {
            OTAPI.Hooks.Player.PostUpdate += PlayerUpdate;
            ServerApi.Hooks.GameUpdate.Register(this, GameUpdate);
            ServerApi.Hooks.GameInitialize.Register(this, GameInitialize);

            DeathEvent += (TSPlayer ply) => { Console.WriteLine("Player {0} died!".SFormat(ply.Name)); };
            RespawnEvent += (TSPlayer ply) => { Console.WriteLine("Player {0} respawned!".SFormat(ply.Name)); };

            InitWormholeEvents();

            for (int i = 0; i < holeDur.Length; i++)
            {
                holeDur[i] = TimeSpan.MinValue;
            }

            ticktimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                config.WormholeRespawnTime = config.WormholeTimer.ToCustomFormat();

                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                var file = new StreamWriter(ConfigFile);
                file.Write(json); // Y'know, just in case it was modified ;)
                file.Flush();
                file.Close();
            }
            base.Dispose(disposing);
        }

        private void GameInitialize(EventArgs args)
        {
            ticktimer.Start();

            Commands.ChatCommands.Add(new Command("wormhole.config.duration", SetDurationCommand, "wormholetime")
                {
                    HelpText = "Gets and sets the duration of wormholes after death."
                });
        }

        private void SetDurationCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendMessage("The current duration is {0}.".SFormat(config.WormholeTimer.ToCustomFormat()), Color.Yellow);
                args.Player.SendMessage("Use <time> to set the duration.", Color.LimeGreen);
            }
            else
            {
                config.WormholeTimer = args.Parameters[0].ToTimeSpan();
                args.Player.SendMessage("The duration has been set to {0}.".SFormat(config.WormholeTimer.ToCustomFormat()), Color.Yellow);
            }
        }

        private Stopwatch ticktimer = new Stopwatch();
        private void GameUpdate(EventArgs args)
        {
            TimeSpan tickdelta = ticktimer.Elapsed;
            ticktimer.Restart();

            GameTickEvent.Invoke(tickdelta);
        }

        private bool[] prevDeadState = new bool[256];
        private void PlayerUpdate(Player player, int i)
        {
            TSPlayer ply = new TSPlayer(i);
            if (ply.UUID == "") { return; }

            if ( player.dead && !prevDeadState[i] )
            { // Died
                DeathEvent.Invoke(ply);
            }
            if ( !player.dead && prevDeadState[i] )
            { // Respawned
                RespawnEvent.Invoke(ply);
            }

            prevDeadState[i] = player.dead;

        }

        public delegate void DeathEventHandler(TSPlayer ply);
        public event DeathEventHandler DeathEvent;

        public delegate void RespawnEventHandler(TSPlayer ply);
        public event RespawnEventHandler RespawnEvent;

        public delegate void GameTickHandler(TimeSpan delta);
        public event GameTickHandler GameTickEvent;

        private void InitWormholeEvents()
        {
            RespawnEvent += OnRespawn;
            GameTickEvent += PotionGameTick;
        }

        private TimeSpan[] holeDur = new TimeSpan[256];
        private void OnRespawn(TSPlayer ply)
        {
            holeDur[ply.Index] = TimeSpan.Zero;

            Console.WriteLine(ply);

        }

        private void PotionGameTick(TimeSpan delta)
        {
            for (int i = 0; i < holeDur.Length; i++)
            {
                if (holeDur[i] != TimeSpan.MinValue)
                    holeDur[i] += delta;
                if (holeDur[i] >= config.WormholeTimer)
                {
                    holeDur[i] = TimeSpan.MinValue;

                    Console.WriteLine("Player {0} got their wormhole potion taken away from them!".SFormat(i));
                }
            }
        }

    }
}
