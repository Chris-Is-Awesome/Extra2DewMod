using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    //This class is for simulation only. If an object doesnt impact conditions, it shouldnt be here
    public class ItemToRandomize
    {
        public delegate void ItemEffect(ItemRandomizationState state, string keyName);

        static Dictionary<string, ItemEffect> _funcDict;

        public string name = "";
        bool noEffectItem = false;
        ItemRandomizationState _state;
        ItemEffect _effect;

        public static ItemToRandomize CreateItem(string name, ItemRandomizationState state)
        {
            if(_funcDict == null)
            {
                _funcDict = new Dictionary<string, ItemEffect>()
                {
                    { "Roll", ItemAddRoll },
                    { "DefenseLvl", ItemAddRoll },
                    { "StickLvl", ItemLevelStick },
                    { "Stick", ItemStick },
                    { "FireSword", ItemFireSword },
                    { "FireMace", ItemFireMace },
                    { "EFCS", ItemEFCS },
                    { "FakeEFCS", ItemFakeEFCS },
                    { "ForceWand", ItemLevelForceWand },
                    { "Dynamite", ItemLevelDynamite },
                    { "DynamiteDev", ItemDynamiteLvl4 },
                    { "IceRing", ItemLevelIceRing },
                    { "Chain", ItemLevelChain },
                    { "Headband", ItemLevelHeadband },
                    { "RaftPiece", ItemAddRaftPiece },
                    { "SecretKey", ItemAddSecretKey },
                    { "Shard", ItemAddShard },
                    { "2Shards", ItemAdd2Shards },
                    { "4Shards", ItemAdd4Shards },
                    { "8Shards", ItemAdd8Shards },
                    { "16Shards", ItemAdd16Shards },
                    { "24Shards", ItemAdd24Shards },
                    { "Crayons", ItemAddHP},
                    { "NegaCrayons", ItemRemoveHP}
                };
            }
            ItemToRandomize item = new ItemToRandomize
            {
                _state = state,
                name = name
            };
            if (_funcDict.TryGetValue(name, out ItemEffect effect))
            {
                item._effect = effect;
            }
            else if (ModItemHandler.IsKeyTag(name))
            {
                item._effect = AddTagKey;
            }
            else if (ModItemHandler.IsDungeonKey(name))
            {
                item._effect = AddDungeonKey;
            }
            else if(ModItemHandler.IsCard(name, out int index))
            {
                item._effect = AddCard;
            }
            else
            {
                item.noEffectItem = true;
            }

            return item;
        }

        public void ActivateEffect()
        {
            _effect?.Invoke(_state, name);
        }

        public string GetNameForPrint()
        {
            return ModItemHandler.GetNickName(name);
        }

        #region Item effects
        static void AddCard(ItemRandomizationState state, string keyName)
        {
            if(ModItemHandler.IsCard(keyName, out int index))
            {
                state.AddCard(index);
            }
        }

        static void AddDungeonKey(ItemRandomizationState state, string keyName)
        {
            state.AddDungeonKey(keyName);
        }

        static void AddTagKey(ItemRandomizationState state, string keyName)
        {
            state.AddTagKey(keyName);
        }

        static void ItemLevelStick(ItemRandomizationState state, string keyName)
        {
            state.AddStick(1);
        }

        static void ItemStick(ItemRandomizationState state, string keyName)
        {
            state.SetStick(0);
        }

        static void ItemFireSword(ItemRandomizationState state, string keyName)
        {
            state.SetStick(1);
        }

        static void ItemFireMace(ItemRandomizationState state, string keyName)
        {
            state.SetStick(2);
        }
        
        static void ItemEFCS(ItemRandomizationState state, string keyName)
        {
            state.SetStick(3);
        }

        static void ItemFakeEFCS(ItemRandomizationState state, string keyName)
        {
            state.AddPuzzleDmg();
        }

        static void ItemLevelForceWand(ItemRandomizationState state, string keyName)
        {
            state.AddForceWand(1);
        }

        static void ItemLevelDynamite(ItemRandomizationState state, string keyName)
        {
            state.AddDynamite(1);
        }

        static void ItemDynamiteLvl4(ItemRandomizationState state, string keyName)
        {
            state.SetDynamite(4);
        }

        static void ItemAddShard(ItemRandomizationState state, string keyName)
        {
            state.AddShard(1);
        }

        static void ItemAdd2Shards(ItemRandomizationState state, string keyName)
        {
            state.AddShard(2);
        }

        static void ItemAdd4Shards(ItemRandomizationState state, string keyName)
        {
            state.AddShard(4);
        }

        static void ItemAdd8Shards(ItemRandomizationState state, string keyName)
        {
            state.AddShard(8);
        }

        static void ItemAdd16Shards(ItemRandomizationState state, string keyName)
        {
            state.AddShard(16);
        }

        static void ItemAdd24Shards(ItemRandomizationState state, string keyName)
        {
            state.AddShard(24);
        }

        static void ItemLevelIceRing(ItemRandomizationState state, string keyName)
        {
            state.AddIceRing(1);
        }

        static void ItemLevelChain(ItemRandomizationState state, string keyName)
        {
            state.AddChain(1);
        }

        static void ItemLevelHeadband(ItemRandomizationState state, string keyName)
        {
            state.AddHeadband(1);
        }

        static void ItemAddRoll(ItemRandomizationState state, string keyName)
        {
            state.AllowRoll();
        }

        static void ItemAddRaftPiece(ItemRandomizationState state, string keyName)
        {
            state.AddRaftPiece(1);
        }

        static void ItemAddSecretKey(ItemRandomizationState state, string keyName)
        {
            state.AddSecretKey(1);
        }

        static void ItemAddHP(ItemRandomizationState state, string keyName)
        {
            state.AddHP(1);
        }

        static void ItemRemoveHP(ItemRandomizationState state, string keyName)
        {
            state.AddHP(-1);
        }
        #endregion
    }
}
