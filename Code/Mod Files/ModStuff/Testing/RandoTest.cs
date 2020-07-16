using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class RandoTest : Singleton<RandoTest>
	{
		enum ItemTags
		{
			Damage,
			Dynamite,
			Efcs,
			Fire,
			Forcewand,
			Icering,
			Projectile
		}

		class LocationData
		{
			public string scene;
			public string room;
			public string saveKey;
			public string requiredItems;

			public LocationData(string sceneName, string roomName, string saveName, string itemRequirements = "")
			{
				scene = sceneName;
				room = roomName;
				saveKey = saveName;
				requiredItems = itemRequirements;
			}
		}

		class ItemPool
		{
			public string itemName;
			public List<ItemTags> tags;

			public ItemPool(string itemRealName, List<ItemTags> itemTags= null)
			{
				itemName = itemRealName;
				tags = itemTags;
			}
		}

		class RandomizedItemData
		{
			public LocationData location;
			public string item;
			public string forScene; // Used if item applies to a scene, such as a dungeon key

			public RandomizedItemData(string itemName, string keyForScene = "")
			{
				item = itemName;
				forScene = keyForScene;
			}
		}

		List<LocationData> locations = new List<LocationData>();
		List<ItemPool> itemPool = new List<ItemPool>();
		List<RandomizedItemData> randomizedItems = new List<RandomizedItemData>();

		public void Initialize()
		{
			FillItemPool();
			FillLocationData();
		}

		void FillItemPool()
		{
			itemPool = new List<ItemPool>()
			{
				new ItemPool(
					"melee",
					new List<ItemTags> { ItemTags.Damage, ItemTags.Fire } ),
			};
		}

		void FillLocationData()
		{
			locations = new List<LocationData>()
			{
				new LocationData(
					"sceneName", // Sand Castle
					"roomName", // Boss room
					"saveName", // Save key
					"itemReq1 | itemReq1 & itemReq2"), // "forcewand & key-1 | forcewand & melee"
			};
		}

		public void DoRandomize()
		{
			// For every location
			for (int i = 0; i < locations.Count; i++)
			{
				//
			}
		}
	}
}