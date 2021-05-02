using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace NoFriendshipDecay
{
    public class Config
    {
        /// <summary>Whether se should be prevented from having friendship decay. Effects everyone
        /// but the spouse if they are married.</summary>
        public bool PreventPeopleFriendshipDecay = true;
        /// <summary>Whether animals should be prevented from having friendship decay. Due to SMAPI limitations you have
        /// to sleep in the bed for this to work.</summary>
        public bool PreventAnimalFriendshipDecay = true;
    }

    public class ModEntry : Mod
    {
        internal Config Config;

        /// <summary>Mod entry point. Reads the config and adds the listeners.</summary>
        /// <param name="helper">Helper object for various mod functions (such as loading config files).</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<Config>();

            if (!Config.PreventPeopleFriendshipDecay && !Config.PreventAnimalFriendshipDecay)
                Monitor.Log("This mod can be removed, all features currently disabled.", LogLevel.Warn);
            else
                helper.Events.GameLoop.DayEnding += EndDay;
        }

        private static IEnumerable<Friendship> GetFriends()
        {
            var players = Game1.getAllFarmers();
            var npcs = Utility.getAllCharacters();
            var farmers = players as Farmer[] ?? players.ToArray();

            foreach (var character in npcs)
            {
                if (!character.isVillager() && !character.IsMonster)
                    continue;

                foreach (var farmer in farmers)
                {
                    //Set the flag for having talked to that character, but don't add any points.
                    //The player can talk to the person themselves and still get the 20 points.
                    if (farmer.friendshipData.TryGetValue(character.Name, out var friendship))
                    {
                        yield return friendship;
                    }
                }
            }
        }

        /// <summary>Before the day is done, we need to set all the talked-to flags.</summary>
        private void EndDay(object sender, DayEndingEventArgs e)
        {
            //This is a host-only mod:
            if (!Context.IsMainPlayer)
                return;

            if (Config.PreventAnimalFriendshipDecay)
            {
                foreach (var animal in Game1.getFarm().getAllFarmAnimals())
                {
                    animal.wasPet.Set(true);
                }
            }

            if (Config.PreventPeopleFriendshipDecay)
            {
                foreach (var friend in GetFriends())
                {
                    //Set the flag for having talked to that character, but don't add any points.
                    //The player can talk to the person themselves and still get the 20 points.
                    friend.TalkedToToday = true;
                }
            }
        }
    }
}