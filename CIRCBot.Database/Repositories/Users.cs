using System.Collections.Generic;
using System.Linq;

namespace CIRCBot
{
    public static class Users
    {

        #region Private variables

        /// <summary>
        /// List of all users.
        /// </summary>
        private static List<User> users = new List<User>();

        /// <summary>
        /// Dictionary with admin addresses as keys, bound to user name.
        /// </summary>
        private static Dictionary<string, string> adminAddresses = new Dictionary<string, string>();

        #endregion Private variables

        #region Public methods and accessors

        /// <summary>
        /// Get all listed users.
        /// </summary>
        public static User[] All
        {
            get { return users.ToArray(); }
        }

        /// <summary>
        /// Get all registered admins.
        /// </summary>
        public static User[] Admins
        {
            get { return users.Where(x => x.IsAdmin).ToArray(); }
        }

        public static Score[] Scores(int gameId, int seasonId = 0)
        {
            if (seasonId == 0)
            {
                seasonId = Seasons.Current.SeasonId;
            }

            List<Score> scores = new List<Score>();
            foreach(var user in users)
            {
                var score = user.Scores.FirstOrDefault(x => x.GameId == gameId && x.SeasonId == seasonId);
                if(score != null)
                {
                    scores.Add(score);
                }
            }
            return scores.ToArray();
        }

        /// <summary>
        /// User data of the bot.
        /// </summary>
        public static User House { get; set; }

        /// <summary>
        /// Get user by user name. Returns invalid user if not found.
        /// </summary>
        /// <param name="userName">Username of user to get.</param>
        /// <returns>User with given username or a default user object.</returns>
        public static User Get(string userName)
        {
            User user = getUser(userName);
            return user != null ? user : new User();
        }

        public static User Get(int userId)
        {
            User user = users.FirstOrDefault(x => x.UserId == userId);
            return user != null ? user : new User();
        }

        /// <summary>
        /// Checks if username is found in users list.
        /// </summary>
        /// <param name="userName">Username to check</param>
        /// <returns>True if found, false if not</returns>
        public static bool IsUser(string userName)
        {
            return getUser(userName) != null ? true : false;
        }

        /// <summary>
        /// Check if user's host is listed in the admin addresses.
        /// </summary>
        /// <param name="hostAddress">The address of the host</param>
        /// <returns>True if found, false if not</returns>
        public static bool IsAdmin(string hostAddress)
        {
            return adminAddresses.ContainsKey(hostAddress);
        }

        /// <summary>
        /// Get user by admin address.
        /// </summary>
        /// <param name="hostAddress">The address of the host</param>
        /// <returns></returns>
        public static User GetAdmin(string hostAddress)
        {
            return Get(adminAddresses[hostAddress]);
        }

        /// <summary>
        /// Add user to users list.
        /// </summary>
        /// <param name="newUser">User to add.</param>
        public static void Add(User newUser)
        {
            users.Add(newUser);
        }

        /// <summary>
        /// Add admin address to admin addresses.
        /// </summary>
        /// <param name="userName">Admin username</param>
        /// <param name="address">Admin address</param>
        public static void Add(string userName, string address)
        {
            if(!adminAddresses.ContainsKey(address))
            {
                adminAddresses.Add(address, userName);
            }
        }

        public static void Add(Score score)
        {
            var targetUser = users.FirstOrDefault(x => x.UserId == score.UserId);
            if(targetUser != null)
            {
                targetUser.Scores.Add(score);
            }
        }

        /// <summary>
        /// Empty the user lists.
        /// </summary>
        public static void Reset()
        {
            users = new List<User>();
            adminAddresses = new Dictionary<string, string>();
        }

        /// <summary>
        /// Update all user information to the database.
        /// </summary>
        public static void UpdateAll()
        {
            foreach(User user in users)
            {
                user.Update();
            }
        }

        #endregion Public methods and accessors

        #region Private methods

        /// <summary>
        /// Get user from users list by username.
        /// </summary>
        /// <param name="userName">Username to fetch.</param>
        /// <returns>User with username or null</returns>
        private static User getUser(string userName)
        {
            return users.Where(x => x.Username == userName).FirstOrDefault();
        }

        #endregion Private methods

    }
}
