using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Terraria;
using Newtonsoft.Json;

namespace AdvertPlugin
{
    [AdvertRegistrar]
    public class ConfigAdverts : IAdvertRegistrar
    {
        private string ConfigFile = Path.Combine(Resource.ConfigDir, Resource.ConfigFile);

        internal static ConfigAdverts Instance;

        public class Config
        {
            public class Advert
            {
                public string Text = "";
                public bool RunOnce = false;
                public int[] Color = new int[3] { 255, 255, 255 };
                public string Time = "0s";
                public string Offset = "0s";
            }
            public bool UseDebugMessages;
            public List<Advert> Adverts;
        }

        public static Config config;

        public ConfigAdverts(Main game)
        {
            Instance = this;

            if (!File.Exists(ConfigFile))
            {
                Directory.CreateDirectory(Resource.ConfigDir);
                File.AppendAllText(ConfigFile, Resource.config);
            }

            string json = File.OpenText(ConfigFile).ReadToEnd();

            config = JsonConvert.DeserializeObject<Config>(json);

        }

        List<Advert.Message> adverts = new List<Advert.Message>();

        public void ReInitialize()
        {
            if (config.UseDebugMessages)
            {
                Adverts.Register("Every Ten Seconds!?!?!?!?!?", Color.Chartreuse, TimeSpan.FromSeconds(10));
                Adverts.Register("Every Fifteen Seconds!?!?!?!?!?", Color.OrangeRed, TimeSpan.FromSeconds(15));
            }

            foreach (Advert.Message msg in adverts)
            {
                Adverts.Deregister(msg.Index);
            }

            adverts.Clear();

            foreach (var adv in config.Adverts)
            {
                string message = adv.Text;
                Color color = (Color)typeof(Color)
                    .GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int) })
                    .Invoke(adv.Color.Cast<object>().ToArray());
                TimeSpan delay = adv.Time.ToTimeSpan();
                TimeSpan offset = adv.Offset.ToTimeSpan();

                if (adv.RunOnce)
                {
                    offset = delay;

                    delay = TimeSpan.MaxValue - TimeSpan.FromDays(1);
                }

                adverts.Add(Adverts.Register(message, color, delay, offset));
            }

        }

        public void Initialize()
        {
            ReInitialize();
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                string json = JsonConvert.SerializeObject(config,Formatting.Indented);
                var file = new StreamWriter(ConfigFile);
                file.Write(json); // Y'know, just in case it was modified ;)
                file.Flush();
                file.Close();
            }
        }
    }
}
