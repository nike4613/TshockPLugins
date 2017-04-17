using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Utils;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace AdvertPlugin
{
    [ApiVersion(2, 0)]
    public class Advert : TerrariaPlugin
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
            get { return "Displays adverts (short text clips) to all players every so often"; }
        }

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name
        {
            get { return "Adverts"; }
        }

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version
        {
            get { return new Version(0, 0, 0, 0); }
        }

        public static Advert Instance;
        private IAdvertRegistrar[] registrars;
        internal CommandAdverts cmdAdv;

        /// <summary>
        /// Initializes a new instance of the TestPlugin class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        /// </summary>
        public Advert(Main game) : base(game)
        {
            Instance = this;

            Type[] regrs = Util.GetTypesWithAttribute(typeof(AdvertRegistrarAttribute)).ToArray();
            List<IAdvertRegistrar> regr = new List<IAdvertRegistrar>();

            foreach (Type t in regrs)
            {
                if ( !t.GetInterfaces().Contains( typeof(IAdvertRegistrar) ) )
                {
                    Console.Error.WriteLine("AdvertRegistrar '{}' does not implement IAdvertRegistrar!");
                    continue;
                }

                var constr = t.GetConstructor( new Type[] { typeof(Main) } );
                var args = new object[] { game };

                if (constr == null)
                {
                    constr = t.GetConstructor( new Type[] {} );
                    args = new object[] {};
                }

                IAdvertRegistrar o = (IAdvertRegistrar) constr.Invoke(args);

                if (typeof(CommandAdverts).IsInstanceOfType(o))
                    cmdAdv = (CommandAdverts) o;

                regr.Add(o);
            }

            registrars = regr.ToArray();
        }

        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        /// </summary>
        public override void Initialize()
        {
            ServerApi.Hooks.GameUpdate.Register(this, GameTick);
            ServerApi.Hooks.ServerJoin.Register(this, PlayerJoin);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);

            foreach (var r in registrars)
            {
                r.Initialize();
            }
            

        }

        private void OnInitialize(EventArgs args)
        {
            cmdAdv.GameInit();
            ticktimer.Start();
        }

        /// <summary>
        /// Handles plugin disposal logic.
        /// *Supposed* to fire when the server shuts down.
        /// You should deregister hooks and free all resources here.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, GameTick);
                ServerApi.Hooks.ServerJoin.Deregister(this, PlayerJoin);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);

                ticktimer.Stop();

                foreach (var r in registrars)
                {
                    r.Dispose(disposing);
                }
            }
            base.Dispose(disposing);
        }



        public class Message
        {
            public string Text;
            public Color Color;
            public TimeSpan Delay;
            public TimeSpan Offset;
            public PlayerFilter Filter;
            public MessageSend Send;
            public int Index;
        }
        private List<Message> messages = new List<Message>();
        private List2d<TimeSpan> delays = new List2d<TimeSpan>();

        public TimeSpan TimeUntil(Message m, TSPlayer ply)
        {
            if (messages.Contains(m))
                return m.Delay - delays[ply.Index, m.Index];
            else
                return TimeSpan.MinValue;
        }

        public IReadOnlyCollection<Message> Messages
        {
            get
            {
                return messages.AsReadOnly();
            }
        }

        public delegate bool PlayerFilter(TSPlayer player);
        public delegate bool MessageSend(Message message, TSPlayer player);

        protected internal Message Register(string message, Color color, TimeSpan delay, TimeSpan begotoff, PlayerFilter filter, MessageSend sent)
        {

            Message msg = new Message()
            {
                Text = message,
                Color = color,
                Delay = delay,
                Offset = begotoff,
                Filter = filter,
                Send = sent
            };

            msg.Index = messages.AddFirstDefault(msg);

            delays.SublistInitCapacity = messages.Count;

            foreach (List<TimeSpan> la in delays)
            {
                la.Capacity = Math.Max(messages.Count, la.Capacity);
                la.FillToCapacity(TimeSpan.MinValue);
            }

            foreach (TSPlayer ply in TShock.Players)
            {
                if (ply != null)
                    foreach (Message m in messages)
                    {
                        if ( !EqualityComparer<Message>.Default.Equals(m, default(Message)) 
                            && delays[ply.Index, m.Index] == TimeSpan.MinValue)
                            delays[ply.Index ,m.Index] = m.Delay - m.Offset;
                    }
            }

            return msg;

        }

        List<int> toDeregister = new List<int>();
        protected internal void Deregister(int index)
        {
            toDeregister.Add(index);
        }

        private Stopwatch ticktimer = new Stopwatch();
        private void GameTick(EventArgs args)
        {
            TimeSpan ticktime = ticktimer.Elapsed;
            ticktimer.Restart();

            var msgs = messages.ToArray();

            foreach(TSPlayer ply in TShock.Players)
            {
                if (ply != null)
                    foreach (Message m in msgs)
                    {
                        if ( !EqualityComparer<Message>.Default.Equals(m, default(Message)) && m.Filter(ply))
                        {

                            if (delays[ply.Index, m.Index] >= m.Delay && m.Send(m, ply))
                            {
                                ply.SendMessage(m.Text, m.Color);
                                delays[ply.Index, m.Index] = TimeSpan.Zero;
                            }

                            delays[ply.Index, m.Index] += ticktime;
                            
                        }
                    }
            }

            foreach (int i in toDeregister)
                messages.RemoveNoShift(i);
            toDeregister.Clear();

        }

        private void PlayerJoin(JoinEventArgs e)
        {
            delays.EnsurePosition(e.Who);

            delays[e.Who] = new List<TimeSpan>();
            List<TimeSpan> l = delays[e.Who];
            l.Capacity = Math.Max(messages.Count, l.Capacity); ;
            l.FillToCapacity(TimeSpan.Zero);

            foreach (Message m in messages)
            {
                l[m.Index] = m.Delay-m.Offset;
            }

        }
    }
}