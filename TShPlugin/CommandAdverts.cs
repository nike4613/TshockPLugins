using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Utils;

namespace AdvertPlugin
{
    [AdvertRegistrar]
    class CommandAdverts : IAdvertRegistrar
    {
        public CommandAdverts()
        {

        }

        public void Dispose(bool disposing)
        {

        }

        public void GameInit()
        {
            Commands.ChatCommands.Add(new Command("adverts.config.create", CreateAd, "adcreate")
                {
                    HelpText = "Creates an advert and reloads adverts, but not from file. Prints the ID of the advert."
                });
            Commands.ChatCommands.Add(new Command("adverts.config.delete", DeleteAd, "adremove")
                {
                    HelpText = "Removes an advert."
                });
            Commands.ChatCommands.Add(new Command("adverts.config.list", ListAdverts, "adlist")
                {
                    HelpText = "Lists the configured adverts."
                });
            Commands.ChatCommands.Add(new Command("adverts.remind", RemindCommand, "remindme")
                {
                    HelpText = "Reminds you of an something."
                });
            Commands.ChatCommands.Add(new Command("adverts.remind", RemindersList, "reminders")
                {
                    HelpText = "Lists the queued reminders."
                });
            
        }

        private class RemindPlayerCheck
        {
            private TSPlayer player;
            private CommandAdverts self;

            public RemindPlayerCheck(TSPlayer ply, CommandAdverts ths)
            {
                player = ply;
                self = ths;
            }

            public bool Check(TSPlayer ply)
            {
                return player == ply || player.UUID == ply.UUID;
            }

            public bool Send(Advert.Message m, TSPlayer ply)
            {
                self.Reminders[ply.Index].Remove(m);
                Adverts.Deregister(m.Index);
                return true;
            }
        }
        private List2d<Advert.Message> Reminders = new List2d<Advert.Message>();
        private void RemindCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendMessage("Syntax: <message> <time>\nWhere 'time'is in the format '<num><unit>+', with unit being one of 'd', 'h', 'm', 's', or 'ms'.", Color.Red);
                return;
            }

            string message = args.Parameters[0];
            string time = args.Parameters[1];

            TimeSpan tim = time.ToTimeSpan();

            RemindPlayerCheck chk = new RemindPlayerCheck(args.Player, this);

            var msg = Adverts.Register("Reminder: " + message, Color.LimeGreen, tim, chk.Check, chk.Send);

            args.Player.SendMessage("OK! You will be reminded '{0}' in {1}".SFormat(message, tim.ToReadable()), Color.LimeGreen);

            Reminders.EnsurePosition(args.Player.Index);
            Reminders[args.Player.Index].Add(msg);
        }

        private void RemindersList(CommandArgs args)
        {
            args.Player.SendMessage("Upcoming reminders:", Color.Green);

            Reminders.EnsurePosition(args.Player.Index);
            foreach (Advert.Message m in Reminders[args.Player.Index])
            {
                TimeSpan timeleft = Advert.Instance.TimeUntil(m, args.Player);

                args.Player.SendMessage( "  '{0}' in {1}".SFormat(m.Text, timeleft.ToReadable() ), Color.Yellow);
            }
        }

        private void CreateAd(CommandArgs args)
        {
            if (args.Parameters.Count < 2 || ( args.Parameters.Count >= 3 && args.Parameters.Count < 5 ) )
            {
                args.Player.SendMessage("Syntax: <message> <time> [<r> <g> <b> [<playonce> [<offset> [<playermatch>]]]]", Color.Red);
                return;
            }

            List<string> parms = args.Parameters;

            string message = parms[0];
            string time = parms[1];
            int r=255, g=255, b=255;
            bool playonce = false;
            string offset = time;
            string player = "name=/.*/,group=/.*/";
            
            if (parms.Count >= 5)
            {
                r = int.Parse(parms[2]);
                g = int.Parse(parms[3]);
                b = int.Parse(parms[4]);

                if (parms.Count >= 6)
                {
                    playonce = bool.Parse(parms[5]);

                    if (parms.Count >= 7)
                    {
                        offset = parms[6];

                        if (parms.Count >= 8)
                        {
                            player = parms[7];
                        }
                    }
                }
            }

            int index = -1;
            string prefix = "";
            if (player == "name=/.*/,group=/.*/")
            {
                index = ConfigAdverts.config.Adverts.AddFirstDefault(new ConfigAdverts.Config.Advert()
                {
                    Text = message,
                    RunOnce = playonce,
                    Color = new int[3] { r, g, b },
                    Time = time,
                    Offset = offset
                }
                );

                ConfigAdverts.Instance.ReInitialize();

                prefix = "s";
            }
            else
            {
                prefix = "n";

                // Not implemented
                args.Player.SendMessage("Syntax: <message> <time> [<r> <g> <b> [<playonce> [<offset>]]]", Color.Red); // lol conflicting messages
                return;
            }

            args.Player.SendMessage("Your ID: {1}{0}".SFormat(index+1,prefix), Color.LimeGreen);

        }

        private void DeleteAd(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Syntax: <advert id>", Color.Red);
                return;
            }

            string id = args.Parameters[0];

            string prefix = id.Substring(0, 1);
            int index = int.Parse(id.Substring(1));

            switch (prefix)
            {
                case "s":
                    DeleteConfigAdvert(index-1);
                    break;
                default:
                    args.Player.SendMessage("Invalid ID", Color.Red);
                    return;
            }

            ConfigAdverts.Instance.ReInitialize();

            args.Player.SendMessage("Removed Advert {0}".SFormat(id), Color.LimeGreen);
        }

        private void DeleteConfigAdvert(int index)
        {
            ConfigAdverts.Config.Advert cfad = ConfigAdverts.config.Adverts[index];
            ConfigAdverts.config.Adverts.RemoveNoShift(index);

            foreach (var m in Advert.Instance.Messages)
            {
                if ( !EqualityComparer<Advert.Message>.Default.Equals(m, default(Advert.Message))
                    && m.Text == cfad.Text && m.Delay == cfad.Time.ToTimeSpan() && m.Offset == cfad.Offset.ToTimeSpan()
                    && new int[] { m.Color.R, m.Color.G, m.Color.B } == cfad.Color )
                {
                    Adverts.Deregister(m.Index);
                }
            }

        }

        private void ListAdverts(CommandArgs args)
        {
            args.Player.SendMessage("Adverts currently configured:", Color.Green);

            int i = 1;
            foreach (var ad in ConfigAdverts.config.Adverts)
            {
                if (ad == default(ConfigAdverts.Config.Advert))
                    continue;

                string restOfMes = "";

                if (ad.RunOnce)
                {
                    restOfMes += "running once after {0}".SFormat(ad.Time);
                }
                else
                {
                    restOfMes += "every {0} after the first time, {1} after login".SFormat(ad.Time, ad.Offset);
                }

                args.Player.SendMessage("  {0}: \"{1}\" in color ({2}) {3}".SFormat("s" + (i++), ad.Text, ", ".JoinObject(ad.Color), restOfMes), Color.Yellow);
            }
        }

        public void Initialize()
        {

        }
    }
}
