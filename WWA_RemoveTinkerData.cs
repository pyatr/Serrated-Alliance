using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.Tinkering;

namespace XRL
{
    [HasGameBasedStaticCache]
    public class WWA_RemoveTinkerData
    {
        public static void Reset()
        {
            if (TinkerData.TinkerRecipes.Count > 0)
                Remove();
        }

        static void Remove(int n = -1)
        {
            string[] data = { "ModDrumLoaded", "ModScoped", "ModLiquidCooled" };
            if (n > 0)            
			{
                TinkerData.TinkerRecipes.RemoveAt(n);
				UnityEngine.Debug.Log($"Removed at {n}");
			}
            int wrongNamePos = -1;
            for (int i = 0; i < TinkerData.TinkerRecipes.Count; i++)
            {
                foreach (string s in data)
                {
                    if (TinkerData.TinkerRecipes[i].PartName == s)
                    {
                        wrongNamePos = i;
						UnityEngine.Debug.Log($"{TinkerData.TinkerRecipes[i].PartName} to be removed at {wrongNamePos}");
                        i = TinkerData.TinkerRecipes.Count;
                        break;
                    }
                }
            }
            if (wrongNamePos != -1)
                Remove(wrongNamePos);
        }
    }
}