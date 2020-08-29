using Core.Kenshi_Data.Enums;
using Core.Kenshi_Data.Model;
using Core.Models;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class ConflictManager
    {
        public List<string> Modlist = new List<string>();
        public List<GameData> ListOfGameData = new List<GameData>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, List<GameChange>>>> Changes = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, List<GameChange>>>>();
        public ConcurrentDictionary<string, ModListChanges> conflictIndex = new ConcurrentDictionary<string, ModListChanges>();
        public ConcurrentDictionary<string, DetailChanges> DetailIndex = new ConcurrentDictionary<string, DetailChanges>();
        public object sync = new object();
        public Dictionary<string, List<ItemType>> listOfTags = new Dictionary<string, List<ItemType>>();

        public ItemFilter Filter { get; set; }

        public void LoadMods(string file, ModMode mode, GameData gd)
        {
            string extension = Path.GetExtension(file);
            if (extension == ".mod" || extension == ".translation")
            {
                if (!this.Modlist.Any(c => c == file))
                    this.Modlist.Add(file);
            }
            gd.Load(file, mode);
        }

        public void LoadBaseChanges(GameData mod)
        {
            var nodes = new ConcurrentStack<GameData.Item>();

            Parallel.ForEach(mod.items.Values, (obj) =>
            {
                if (obj.GetState() != State.ORIGINAL && (this.Filter == null || this.Filter.Test(obj)) && ((obj.GetState() != State.OWNED || true) && (obj.GetState() != State.MODIFIED || true)) && ((obj.GetState() != State.REMOVED || false) && (obj.GetState() != State.LOCKED && obj.GetState() != State.LOCKED_REMOVED) && (obj.GetState() != State.REMOVED || obj.OriginalName != null)))
                {
                    nodes.Push(obj);
                }
            });

            Parallel.ForEach(nodes, (obj1) =>
            {
                State state1 = obj1.GetState();
                if (state1 != State.OWNED)
                {
                    foreach (string referenceList in obj1.referenceLists())
                    {
                        Desc desc = GameData.getDesc(obj1.type, referenceList);
                        if (!(desc.defaultValue is GameData.TripleInt))
                            desc = (Desc)null;
                        foreach (KeyValuePair<string, GameData.TripleInt> keyValuePair in obj1.referenceData(referenceList, true))
                        {
                            State state2 = obj1.GetState(referenceList, keyValuePair.Key);
                            switch (state2)
                            {
                                case State.ORIGINAL:
                                case State.LOCKED:
                                case State.LOCKED_REMOVED:
                                    continue;
                                default:
                                    GameData.Item obj2 = mod.getItem(keyValuePair.Key);
                                    GameChange changeData;
                                    if (state2 == State.REMOVED)
                                        changeData = new GameChange { State = state2.ToString(), ModName = Path.GetFileName(mod.Filename), Value = keyValuePair.Value };
                                    else
                                        continue;

                                    AddToList(keyValuePair.Key, obj1.type, obj1.Name, changeData);

                                    continue;
                            }
                        }
                    }
                    foreach (KeyValuePair<string, GameData.Instance> keyValuePair1 in obj1.InstanceData())
                    {
                        State state2 = keyValuePair1.Value.GetState();
                        ChangeData changeData;
                        switch (state2)
                        {
                            case State.ORIGINAL:
                            case State.LOCKED:
                            case State.LOCKED_REMOVED:
                                continue;
                            case State.OWNED:
                                changeData = new ChangeData(ChangeType.NEWINST, keyValuePair1.Key, state2);
                                break;

                            default:
                                changeData = new ChangeData(ChangeType.MODINST, keyValuePair1.Key, state2);
                                break;
                        }
                        changeData.Text = "Instance : " + keyValuePair1.Key;

                        if (state2 == State.MODIFIED)
                        {
                            foreach (KeyValuePair<string, object> keyValuePair2 in (GameData.Item)keyValuePair1.Value)
                            {
                                if (keyValuePair1.Value.GetState(keyValuePair2.Key) == State.MODIFIED)
                                    changeData.Add((Changes)new ChangeData(ChangeType.INSTVALUE, keyValuePair1.Key, keyValuePair2.Key, keyValuePair1.Value.OriginalValue(keyValuePair2.Key), keyValuePair1.Value[keyValuePair2.Key], state2));
                            }
                        }
                        obj1.ChangeData = changeData;
                    }
                }
            });

            nodes.Clear();
        }

        public void LoadChanges()
        {
            foreach (var mod in ListOfGameData)
            {
                foreach (var obj in mod.items.Values)
                {
                    Parallel.ForEach(obj.modData, (item) =>
                    {
                        var change = new GameChange { State = obj.GetState().ToString(), ModName = Path.GetFileName(mod.Filename), Value = item.Value };
                        AddToList(item.Key, obj.type, obj.Name, change);
                    });
                }
                mod.items.Clear();
            }
        }

        public void AddToList(string key, ItemType type, string name, GameChange change)
        {
            Func<List<GameChange>> ObjectC = () => new List<GameChange>() { change };

            var hash = new Random($"{type}{name}{key}".GetHashCode()).Next().ToString();

            lock (sync)
            {
                if (listOfTags.ContainsKey(change.ModName))
                {
                    if (!listOfTags[change.ModName].Any(c => c == type))
                        listOfTags[change.ModName].Add(type);
                }
                else
                {
                    listOfTags.Add(change.ModName, new List<ItemType> { type });
                }
            }

            conflictIndex.AddOrUpdate(hash,
              addValue: new ModListChanges { Mod = new ConcurrentStack<string>(new List<string> { change.ModName }), ChangeList = new ConcurrentStack<GameChange>(new ConcurrentStack<GameChange>(ObjectC())) },
              updateValueFactory: (val, value) =>
              {
                  var current = conflictIndex.GetOrAdd(hash, value);

                  if (!current.Mod.Any(q => q == change.ModName))
                      current.Mod.Push(change.ModName);

                  current.ChangeList.Push(change);
                  return current;
              });

            DetailIndex.AddOrUpdate(hash,
              addValue: new DetailChanges() { Name = name, PropertyKey = key, Type = type.ToString() },
              updateValueFactory: (val, value) =>
              {
                  return DetailIndex.GetOrAdd(hash, value);
              });
        }
    }
}