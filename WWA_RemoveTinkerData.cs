using System.Collections.Generic;
using UnityEngine;
using XRL.Messages;
using XRL.World;
using XRL.World.Tinkering;

namespace XRL
{
    [HasGameBasedStaticCache]
    public class WWA_RemoveTinkerData
    {

        [GameBasedCacheInit]
        public static void Reset()
        {
            Debug.Log("I'm gonna delete some tinker recipes");

            if (TinkerData.TinkerRecipes.Count > 0)
            {
                Remove();
            }

            string[] data = { "ModDrumLoaded", "ModScoped", "ModLiquidCooled" };

            foreach (KeyValuePair<string, List<ModEntry>> modEntryList in ModificationFactory.ModTable)
            {
                List<ModEntry> entriesInList = modEntryList.Value;

                for (int i = 0; i < entriesInList.Count; i++)
                {
                    ModEntry modEntry = entriesInList[i];

                    if (data.Contains(modEntry.Part))
                    {
                        modEntryList.Value.Remove(modEntry);
                        Debug.Log($"Removed {modEntry.Part} from mod list {modEntryList.Key}");
                        i--;
                    }
                }
            }
        }

        static void Remove(int n = -1)
        {
            string[] data = { "ModDrumLoaded", "ModScoped", "ModLiquidCooled" };

            if (n > 0)
            {
                //These mods are removed but still appear in game. How!?
                Debug.Log($"{TinkerData.TinkerRecipes[n].PartName} will be removed at {n}");
                TinkerData.TinkerRecipes.RemoveAt(n);
            }

            int wrongNamePos = -1;

            for (int i = 0; i < TinkerData.TinkerRecipes.Count; i++)
            {
                foreach (string s in data)
                {
                    if (TinkerData.TinkerRecipes[i].PartName != s)
                    {
                        continue;
                    }

                    wrongNamePos = i;
                    Debug.Log($"{TinkerData.TinkerRecipes[i].PartName} to be removed at {wrongNamePos}");
                    i = TinkerData.TinkerRecipes.Count;
                    break;
                }
            }
            if (wrongNamePos != -1)
            {
                Remove(wrongNamePos);
            }
            /*else
			{
				for (int i = 0; i < TinkerData.TinkerRecipes.Count; i++)
				{
					UnityEngine.Debug.Log($"HAS RECIPE: {TinkerData.TinkerRecipes[i].PartName}");
				}
			}*/
        }
    }
}