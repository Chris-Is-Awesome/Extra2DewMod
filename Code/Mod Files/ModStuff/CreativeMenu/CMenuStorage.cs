using System.Collections.Generic;
using UnityEngine;
using System;

namespace ModStuff.CreativeMenu
{
    public class CMenuStorage : Singleton<CMenuStorage>
    {
        public delegate void OnListUpdate();
        public event OnListUpdate onListUpdate;
        public event OnListUpdate onNameChange;

        List<Transform> _storedItems;
        List<Transform> StoredItems
        {
            get
            {
                if(_storedItems == null)
                {
                    _storedItems = new List<Transform>();
                }
                return _storedItems;
            }
        }

        void Awake()
        {
            _storedItems = new List<Transform>();
        }

        int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                int storedCount = StoredItems.Count;
                if (storedCount == 0)
                {
                    _index = 0;
                    return;
                }
                int newValue = value;
                if (newValue < 0) newValue = 0;
                else if (newValue >= storedCount) newValue = storedCount - 1;
                _index = newValue;
            }
        }

        public void StoreItem(Transform item)
        {
            if (item == null) return;
            GameObject newItem = Instantiate(item.gameObject);
            newItem.transform.rotation = item.rotation;
            newItem.name = item.name;
            StoredItems.Add(newItem.transform);
            newItem.SetActive(false);
            DontDestroyOnLoad(newItem);
            _index = 0;
            onListUpdate?.Invoke();
        }

        public void ClearItems()
        {
            for(int i = 0; i < StoredItems.Count; i++)
            {
                Destroy(StoredItems[i].gameObject);
            }
            StoredItems.Clear();
            _index = 0;
            onListUpdate?.Invoke();
        }

        public Transform RetrieveItem(int index)
        {
            if (index < 0 || index >= StoredItems.Count) return null;
            GameObject newItem = Instantiate(StoredItems[index].gameObject);
            newItem.transform.rotation = StoredItems[index].rotation;
            newItem.SetActive(true);
            newItem.name = StoredItems[index].name;
            return newItem.transform;
        }

        public void DeleteItem(int index)
        {
            if (index < 0 || index >= StoredItems.Count) return;
            Destroy(StoredItems[index].gameObject);
            StoredItems.RemoveAt(index);
            _index = 0;
            onListUpdate?.Invoke();
        }

        public Transform RetrieveItem()
        {
            return RetrieveItem(_index);
        }

        public void RenameCurrent(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            Rename(Index, name);
        }

        void Rename(int index, string name)
        {
            if (index < 0 || index >= StoredItems.Count) return;
            StoredItems[index].name = name;
            onNameChange?.Invoke();
        }

        public string[] ItemList()
        {
            List<string> items = new List<string>();
            for (int i = 0; i < StoredItems.Count; i++)
            {
                items.Add(StoredItems[i].name);
            }
            
            return items.ToArray();
        }
    }
}
