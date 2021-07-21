using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    //This class handles the general state of the simulated run. It is not used during the game.
    public class ItemRandomizationState
    {
        public delegate bool ConditionFunc(object arg);
        Dictionary<string, ConditionFunc> _conditionFuncs;
        public delegate void ItemAction();

        int _currentHP = 20;
        int _secretKeys = 0;
        int _raftPieces = 0;
        int _lockpick = 0;
        int _shards = 0;
        
        int _stickLevel = 0;
        int _forceWandLevel = 0;
        int _dynamiteLevel = 0;
        int _iceRingLevel = 0;
        int _headbandLevel = 0;
        int _chainLevel = 0;

        bool _hasPuzzleDmg = false;
        bool _canRoll = true;
        bool _glitchAllowed = false;
        bool _noDevWeapons = false;
        bool _noEFCSUpgrade = false;
        bool _openDreams = false;
        bool _ignoreDKeys = false;

        List<string> _dungeonKeys = new List<string>();
        List<int> _cards = new List<int>();

        public ItemRandomizationState()
        {
            _conditionFuncs = new Dictionary<string, ConditionFunc>();
            _keyTagsDictionary = new Dictionary<string, DungeonKeyState>();
            _conditionFuncs.Add("Stick", HasStick);
            _conditionFuncs.Add("Sword", HasFireSword);
            _conditionFuncs.Add("Mace", HasFireMace);
            _conditionFuncs.Add("PuzzleDmg", HasPuzzleDamage);
            _conditionFuncs.Add("Wand", HasForceWand);
            _conditionFuncs.Add("Dynamite", HasDynamite);
            _conditionFuncs.Add("NotDynamite", HasNotDynamite);
            _conditionFuncs.Add("NotForce", HasNotForce);
            _conditionFuncs.Add("Ice", HasIceRing);
            _conditionFuncs.Add("Ice2", HasIceRing2);
            _conditionFuncs.Add("Chain", HasChain);
            _conditionFuncs.Add("Weapon", HasWeapon);
            _conditionFuncs.Add("Chain1", HasChain);
            _conditionFuncs.Add("Melee", HasMelee);
            _conditionFuncs.Add("Roll", HasRoll);
            _conditionFuncs.Add("Glitched", GlitchAllowed);
            _conditionFuncs.Add("BlockCannon", HasBlockCannon);
            _conditionFuncs.Add("AllRaftPieces", HasAllRaftPieces);
            _conditionFuncs.Add("LibraryAccess", LibraryAccess);
            _conditionFuncs.Add("8Shards", Has8Shards);
            _conditionFuncs.Add("16Shards", Has16Shards);
            _conditionFuncs.Add("24Shards", Has24Shards);
            _conditionFuncs.Add("AllSecretKeys", HasAllSecretKeys);
            _conditionFuncs.Add("Projectile", HasProjectile);
            _conditionFuncs.Add("80HP", PromisedRemedyAccess);
            _conditionFuncs.Add("AccessS4Yellow", HasAccessS4Yellow);
            _conditionFuncs.Add("AccessS4Green", HasAccessS4Green);
            _conditionFuncs.Add("OpenDream", HasOpenDream);
            _conditionFuncs.Add("ClosedDream", HasNotOpenDream);
            _conditionFuncs.Add("RaftPiece", HasRaftPiece);
            _conditionFuncs.Add("Headband", HasHeadband);
        }

        public ConditionFunc GetCondition(string condition, out string error, out object argument)
        {
            error = "";
            argument = null;
            if (_conditionFuncs.TryGetValue(condition, out ConditionFunc cond))
            {
                return cond;
            }
            else if (ModItemHandler.IsKeyTag(condition))
            {
                argument = condition;
                return CheckKeyTag;
            }
            else if(ModItemHandler.IsDungeonKey(condition))
            {
                argument = condition;
                return CheckDungeonKey;
            }
            error = "Warning: Invalid condition found: " + condition + ". Condition ignored.";
            return AlwaysPass;
        }

        public ConditionFunc GetCondition(string condition, out string error, out object argument, out string nickName)
        {
            error = "";
            argument = null;
            if (_conditionFuncs.TryGetValue(condition, out ConditionFunc cond))
            {
                nickName = condition;
                return cond;
            }
            else if (ModItemHandler.IsKeyTag(condition))
            {
                argument = condition;
                nickName = ModItemHandler.GetNickName(condition);
                return CheckKeyTag;
            }
            else if (ModItemHandler.IsDungeonKey(condition))
            {
                argument = condition;
                nickName = ModItemHandler.GetNickName(condition);
                return CheckDungeonKey;
            }
            error = "Warning: Invalid condition found: " + condition + ". Condition ignored.";
            nickName = condition;
            return AlwaysPass;
        }

        #region Randomization parameters
        public void Reset()
        {
            _dungeonKeys.Clear();
            _keyTagsDictionary.Clear();
            _cards.Clear();

            _currentHP = 20;
            _secretKeys = 0;
            _raftPieces = 0;
            _lockpick = 0;
            _shards = 0;

            _stickLevel = 0;
            _forceWandLevel = 0;
            _dynamiteLevel = 0;
            _iceRingLevel = 0;
            _chainLevel = 0;
            _headbandLevel = 0;

            _hasPuzzleDmg = false;
            _canRoll = true;
            _glitchAllowed = false;
            _noDevWeapons = false;
            _noEFCSUpgrade = false;
            _openDreams = false;
            _ignoreDKeys = false;
        }

        public void AllowGlitches()
        {
            _glitchAllowed = true;
        }

        public void IgnoreDKeys()
        {
            _ignoreDKeys = true;
        }

        public void OpenDreams()
        {
            _openDreams = true;
        }

        public void DisallowDevWeapons()
        {
            _noDevWeapons = true;
        }

        public void DisallowEFCSUpgrade()
        {
            _noEFCSUpgrade = true;
        }

        public void RemoveStick()
        {
            _stickLevel = -1;
        }

        public void RemoveRoll()
        {
            _canRoll = false;
        }

        public int[] GetCardList()
        {
            return _cards.ToArray();
        }

        #endregion

        #region Condition checks
        bool AlwaysPass(object arg)
        {
            return true;
        }

        bool AlwaysFail(object arg)
        {
            return false;
        }

        bool HasStick(object arg)
        {
            return _stickLevel >= 0;
        }

        bool HasFireSword(object arg)
        {
            return _stickLevel > 0;
        }

        bool HasFireMace(object arg)
        {
            return _stickLevel > 1;
        }

        bool HasWeapon(object arg)
        {
            return HasStick(null) || HasDynamite(null) || HasIceRing(null) || HasForceWand(null);
        }

        bool HasNotDynamite(object arg)
        {
            return HasStick(null) || HasIceRing(null) || HasForceWand(null);
        }

        bool HasNotForce(object arg)
        {
            return HasStick(null) || HasIceRing(null) || HasDynamite(null);
        }

        bool HasPuzzleDamage(object arg)
        {
            return _hasPuzzleDmg;
        }

        bool HasForceWand(object arg)
        {
            return _forceWandLevel > 0;
        }

        bool HasBlockCannon(object arg)
        {
            return HasRoll(null) || HasIceRing(null) || HasForceWand(null);
        }

        bool HasDynamite(object arg)
        {
            return _dynamiteLevel > 0;
        }

        bool HasHeadband(object arg)
        {
            return _headbandLevel > 0;
        }

        bool HasIceRing(object arg)
        {
            return _iceRingLevel > 0;
        }

        bool HasIceRing2(object arg)
        {
            return _iceRingLevel > 1;
        }

        bool HasChain(object arg)
        {
            return _chainLevel > 0;
        }

        bool HasMelee(object arg)
        {
            return HasStick(null) || HasIceRing(null);
        }

        bool GlitchAllowed(object arg)
        {
            return _glitchAllowed;
        }

        bool HasRoll(object arg)
        {
            return _canRoll;
        }

        bool HasAllRaftPieces(object arg)
        {
            return _raftPieces > 7;
        }

        bool LibraryAccess(object arg)
        {
            return _raftPieces > 6;
        }

        bool Has8Shards(object arg)
        {
            return _shards > 7;
        }

        bool Has16Shards(object arg)
        {
            return _shards > 15;
        }

        bool Has24Shards(object arg)
        {
            return _shards > 23;
        }

        bool HasAllSecretKeys(object arg)
        {
            return _secretKeys > 3;
        }

        bool PromisedRemedyAccess(object arg)
        {
            return _currentHP > 79;
        }
        
        bool HasAccessS4Yellow(object arg)
        {
            bool output = (HasDynamite(null) && CheckDungeonKey("DKEY_S4RedJournals1") && CheckDungeonKey("DKEY_S4RedJournals2") && CheckDungeonKey("DKEY_S4RedJournals3") && CheckDungeonKey("DKEY_S4RedJournals4") && CheckDungeonKey("DKEY_S4RedJournals5")) ||
                (HasDynamite(null) && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb"));
            ResetKeyTags();
            return output;
        }

        bool HasOpenDream(object arg)
        {
            return _openDreams;
        }

        bool HasNotOpenDream(object arg)
        {
            return !_openDreams;
        }

        bool HasAccessS4Green(object arg)
        {
            bool output = (HasAccessS4Yellow(null) && HasDynamite(null) && CheckDungeonKey("DKEY_S4GreenJournals1") && CheckDungeonKey("DKEY_S4GreenJournals2") && CheckDungeonKey("DKEY_S4GreenJournals3") && CheckDungeonKey("DKEY_S4GreenJournals4") && CheckDungeonKey("DKEY_S4GreenJournals5")) ||
                (HasDynamite(null) && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb") && CheckKeyTag("DKEY_Tomb"));
            ResetKeyTags();
            return output;
        }

        bool CheckDungeonKey(object arg)
        {
            if (arg is string identifier) return _dungeonKeys.Contains(identifier);
            else return false;
        }

        bool HasProjectile(object arg)
        {
            return HasFireMace(null) || HasForceWand(null);
        }

        bool HasRaftPiece(object arg)
        {
            return _raftPieces > 0;
        }
        #endregion

        #region States update
        public void AddCard(int index)
        {
            if(!_cards.Contains(index))
            {
                _cards.Add(index);
            }
        }

        void UpdatePuzzleDmg()
        {
            if(!_hasPuzzleDmg) _hasPuzzleDmg = _stickLevel > 2 || _dynamiteLevel > 3;
        }

        public void AddShard(int amount)
        {
            _shards += amount;
        }

        public void AddRaftPiece(int amount)
        {
            _raftPieces += amount;
        }

        public void AddSecretKey(int amount)
        {
            _secretKeys += amount;
        }

        public void AddStick(int amount)
        {
            if(_noEFCSUpgrade && (_stickLevel + amount) >= 3) return;

            _stickLevel += amount;
            UpdatePuzzleDmg();
        }

        public void SetStick(int level)
        {
            _stickLevel = level;
            UpdatePuzzleDmg();
        }

        public void AddForceWand(int amount)
        {
            _forceWandLevel += amount;
        }

        public void SetForceWand(int level)
        {
            _forceWandLevel = level;
        }

        public void AddHeadband(int amount)
        {
            _headbandLevel += amount;
        }

        public void SetHeadband(int level)
        {
            _headbandLevel = level;
        }

        public void AddDynamite(int amount)
        {
            if (_noDevWeapons && _dynamiteLevel == 3) return;
            _dynamiteLevel += amount;
            UpdatePuzzleDmg();
        }

        public void SetDynamite(int level)
        {
            _dynamiteLevel = level;
            UpdatePuzzleDmg();
        }

        public void AddIceRing(int amount)
        {
            _iceRingLevel += amount;
        }

        public void SetIceRing(int level)
        {
            _iceRingLevel = level;
        }

        public void AddChain(int amount)
        {
            _chainLevel += amount;
        }

        public void AddPuzzleDmg()
        {
            _hasPuzzleDmg = true;
        }

        public void SetChain(int level)
        {
            _chainLevel = level;
        }

        public void AllowRoll()
        {
            _canRoll = true;
        }

        public void AddHP(int amount)
        {
            _currentHP += amount;
            if (_currentHP < 1) _currentHP = 1;
        }

        public void SetHP(int level)
        {
            _currentHP = level;
        }

        public void AddDungeonKey(string identifier)
        {
            if (!_dungeonKeys.Contains(identifier)) _dungeonKeys.Add(identifier);
        }
        #endregion

        #region Keys
        Dictionary<string, DungeonKeyState> _keyTagsDictionary;

        public void AddTagKey(string name)
        {
            if (!_keyTagsDictionary.TryGetValue(name, out DungeonKeyState state))
            {
                state = new DungeonKeyState();
                _keyTagsDictionary.Add(name, state);
            }
            state.AddKey();
        }

        public bool CheckKeyTag(object arg)
        {
            if (_ignoreDKeys) return true;
            if (arg is string tag && _keyTagsDictionary.TryGetValue(tag, out DungeonKeyState state))
            {
                return state.CheckAvailability();
            }
            return false;
        }

        public void ResetKeyTags()
        {
            foreach(KeyValuePair<string, DungeonKeyState> state in _keyTagsDictionary)
            {
                state.Value.ResetAvailability();
            }
        }

        class DungeonKeyState
        {
            int _currentCount = 0;
            int _max = 0;

            public void AddKey()
            {
                _max++;
            }

            public void ResetAvailability()
            {
                _currentCount = 0;
            }

            public bool CheckAvailability()
            {
                _currentCount++;
                return _max >= _currentCount;
            }
        }
        #endregion
    }
}
