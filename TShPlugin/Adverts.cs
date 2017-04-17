using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using TShockAPI;

namespace AdvertPlugin
{
    class Adverts
    {
        public static Advert.Message Register(string message, Color color, TimeSpan delay, TimeSpan offset, Advert.PlayerFilter filter, Advert.MessageSend send)
        {
            return Advert.Instance.Register(message, color, delay, offset, filter, send);
        }

        public static Advert.Message Register(string message, Color color, TimeSpan delay, TimeSpan offset, Advert.PlayerFilter filter)
        {
            return Register(message, color, delay, offset, filter, (Advert.Message m, TSPlayer p) => true );
        }

        public static Advert.Message Register(string message, Color color, TimeSpan delay, TimeSpan offset, Advert.MessageSend send)
        {
            return Register(message, color, delay, offset, (TSPlayer ply) => true, send);
        }

        public static Advert.Message Register(string message, Color color, TimeSpan delay, TimeSpan offset)
        {
            return Register(message, color, delay, offset, (TSPlayer ply) => true);
        }

        public static Advert.Message Register(string message, Color color, TimeSpan delay, Advert.PlayerFilter filter, Advert.MessageSend send)
        {
            return Register(message, color, delay, delay, filter, send);
        }

        public static Advert.Message Register(string message, Color color, TimeSpan delay, Advert.PlayerFilter filter)
        {
            return Register(message, color, delay, delay, filter);
        }

        public static Advert.Message Register(string message, Color color, TimeSpan delay, Advert.MessageSend send)
        {
            return Register(message, color, delay, delay, (TSPlayer ply) => true, send);
        }

        public static Advert.Message Register(string message, Color color, TimeSpan delay)
        {
            return Register(message, color, delay, delay);
        }

        public static Advert.Message Register(string message, TimeSpan delay, TimeSpan offset, Advert.PlayerFilter filter, Advert.MessageSend send)
        {
            return Register(message, new Color(1.0f, 1.0f, 1.0f, 1.0f), delay, offset, filter, send);
        }

        public static Advert.Message Register(string message, TimeSpan delay, TimeSpan offset, Advert.PlayerFilter filter)
        {
            return Register(message, new Color(1.0f, 1.0f, 1.0f, 1.0f), delay, delay, filter);
        }

        public static Advert.Message Register(string message, TimeSpan delay, TimeSpan offset, Advert.MessageSend send)
        {
            return Register(message, new Color(1.0f, 1.0f, 1.0f, 1.0f), delay, delay, (TSPlayer ply) => true, send);
        }

        public static Advert.Message Register(string message, TimeSpan delay, TimeSpan offset)
        {
            return Register(message, new Color(1.0f, 1.0f, 1.0f, 1.0f), delay, delay);
        }

        public static Advert.Message Register(string message, TimeSpan delay, Advert.PlayerFilter filter, Advert.MessageSend send)
        {
            return Register(message, delay, delay, filter, send);
        }

        public static Advert.Message Register(string message, TimeSpan delay, Advert.PlayerFilter filter)
        {
            return Register(message, delay, delay, filter);
        }

        public static Advert.Message Register(string message, TimeSpan delay, Advert.MessageSend send)
        {
            return Register(message, delay, delay, (TSPlayer ply) => true, send);
        }

        public static Advert.Message Register(string message, TimeSpan delay)
        {
            return Register(message, delay, delay);
        }

        public static void Deregister(int index)
        {
            Advert.Instance.Deregister(index);
        }

    }
}
