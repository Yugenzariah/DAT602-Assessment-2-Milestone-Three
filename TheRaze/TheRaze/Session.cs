using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheRaze
{
    public static class Session
    {
        public static uint? PlayerId { get; set; }
        public static string Username { get; set; }
        public static bool IsAdmin { get; set; }
        public static uint? CurrentPlayerGameId { get; set; }
        public static uint? CurrentGameId { get; set; }

        public static void Clear()
        {
            PlayerId = null;
            Username = null;
            IsAdmin = false;
            CurrentPlayerGameId = null;
            CurrentGameId = null;
        }

        public static bool IsLoggedIn => PlayerId.HasValue;
        public static bool IsInGame => CurrentPlayerGameId.HasValue;
    }
}
