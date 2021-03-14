using Core.Kenshi_Data.Enums;
using Core.Kenshi_Data.Model;
using Core.Models;
using MMDHelpers.CSharp.Extensions;
using MMDHelpers.CSharp.LocalData;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Core
{
    public class ConflictManager
    {
        public List<string> Modlist = new List<string>();
        //public List<GameData> ListOfGameData = new List<GameData>();
        //public ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, List<GameChange>>>> Changes = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, List<GameChange>>>>();


        public ItemFilter Filter { get; set; }

        public void LoadMods(string file, ModMode mode, GameData gd)
        {

            if (file.EndsWithCaseSensitive(".mod") || file.EndsWithCaseSensitive(".translation"))
            {
                if (!this.Modlist.Any(c => c == file))
                    this.Modlist.Add(file);
            }
            gd.Load(file, mode);
        }
        //public void LoadBaseForConflicts(GameData mod)
        //{
        //    var db = new DataService();


        //    Parallel.ForEach(mod.items.Values, (obj) =>
        //    {
        //        if (obj.GetState() != State.ORIGINAL && (this.Filter == null || this.Filter.Test(obj)) && ((obj.GetState() != State.OWNED || true) && (obj.GetState() != State.MODIFIED || true)) && ((obj.GetState() != State.REMOVED || false) && (obj.GetState() != State.LOCKED && obj.GetState() != State.LOCKED_REMOVED) && (obj.GetState() != State.REMOVED || obj.OriginalName != null)))
        //        {
        //            State state1 = obj.GetState();
        //            if (state1 != State.OWNED)
        //            {
        //                foreach (string referenceList in obj.referenceLists())
        //                {
        //                    foreach (KeyValuePair<string, GameData.TripleInt> keyValuePair in obj.referenceData(referenceList, true))
        //                    {
        //                        State state2 = obj.GetState(referenceList, keyValuePair.Key);
        //                        switch (state2)
        //                        {
        //                            case State.ORIGINAL:
        //                            case State.LOCKED:
        //                            case State.LOCKED_REMOVED:
        //                                continue;
        //                            default:
        //                                GameChange changeData;
        //                                if (state2 == State.REMOVED)
        //                                    changeData = new GameChange(state2, mod.Filename, keyValuePair.Value);
        //                                else
        //                                    continue;

        //                                Helpers.AddToList(keyValuePair.Key, obj.type, obj.Name, changeData);

        //                                continue;
        //                        }
        //                    }
        //                }
        //                foreach (KeyValuePair<string, GameData.Instance> keyValuePair1 in obj.InstanceData())
        //                {
        //                    State state2 = keyValuePair1.Value.GetState();
        //                    switch (state2)
        //                    {
        //                        case State.ORIGINAL:
        //                        case State.LOCKED:
        //                        case State.LOCKED_REMOVED:
        //                            continue;
        //                        case State.OWNED:
        //                            if (mod.Id > 0)
        //                                db.Execute(@"
        //                                            INSERT INTO ModChange (
        //                                                ModId,
        //                                                Type,
        //                                                Section,
        //                                                [Key],
        //                                                OldVal,
        //                                                NewVal,
        //                                                State
        //                                            )
        //                                            VALUES (
        //                                                :ModId,
        //                                                :Type,
        //                                                :Section,
        //                                                :Key,
        //                                                :OldVal,
        //                                                :NewVal,
        //                                                :State
        //                                            )", new
        //                                {
        //                                    ModId = mod.Id,
        //                                    Type = ChangeType.NEWINST,
        //                                    Section = keyValuePair1.Key,
        //                                    Key = keyValuePair1.Key,
        //                                    OldValue = string.Empty,
        //                                    NewVal = $"Instance : {keyValuePair1.Key}",
        //                                    State = state2
        //                                });
        //                            break;

        //                        default:
        //                            if (mod.Id > 0)
        //                                db.Execute(@"
        //                                            INSERT INTO ModChange (
        //                                                ModId,
        //                                                Type,
        //                                                Section,
        //                                                [Key],
        //                                                OldVal,
        //                                                NewVal,
        //                                                State
        //                                            )
        //                                            VALUES (
        //                                                :ModId,
        //                                                :Type,
        //                                                :Section,
        //                                                :Key,
        //                                                :OldVal,
        //                                                :NewVal,
        //                                                :State
        //                                            )", new
        //                                {
        //                                    ModId = mod.Id,
        //                                    Type = ChangeType.MODINST,
        //                                    Section = keyValuePair1.Key,
        //                                    Key = keyValuePair1.Key,
        //                                    OldValue = string.Empty,
        //                                    NewVal = $"Instance : {keyValuePair1.Key}",
        //                                    State = state2
        //                                });
        //                            break;
        //                    };

        //                    if (state2 == State.MODIFIED)
        //                    {

        //                        foreach (KeyValuePair<string, object> keyValuePair2 in keyValuePair1.Value)
        //                        {
        //                            if (keyValuePair1.Value.GetState(keyValuePair2.Key) == State.MODIFIED)
        //                            {
        //                                if (mod.Id > 0)
        //                                    db.Execute(@"
        //                                            INSERT INTO ModChange (
        //                                                ModId,
        //                                                Type,
        //                                                Section,
        //                                                [Key],
        //                                                OldVal,
        //                                                NewVal,
        //                                                State
        //                                            )
        //                                            VALUES (
        //                                                :ModId,
        //                                                :Type,
        //                                                :Section,
        //                                                :Key,
        //                                                :OldVal,
        //                                                :NewVal,
        //                                                :State
        //                                            )", new
        //                                    {
        //                                        ModId = mod.Id,
        //                                        Type = ChangeType.INSTVALUE,
        //                                        Section = keyValuePair1.Key,
        //                                        Key = keyValuePair2.Key,
        //                                        OldValue = keyValuePair1.Value.OriginalValue(keyValuePair2.Key),
        //                                        NewVal = keyValuePair1.Value[keyValuePair2.Key],
        //                                        State = state2
        //                                    });

        //                            }
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //    });

        //}
        //public void LoadBaseChanges(GameData mod)
        //{
        //    var nodes = new ConcurrentStack<GameData.Item>();

        //    Parallel.ForEach(mod.items.Values, (obj) =>
        //    {
        //        if (obj.GetState() != State.ORIGINAL && (this.Filter == null || this.Filter.Test(obj)) && ((obj.GetState() != State.OWNED || true) && (obj.GetState() != State.MODIFIED || true)) && ((obj.GetState() != State.REMOVED || false) && (obj.GetState() != State.LOCKED && obj.GetState() != State.LOCKED_REMOVED) && (obj.GetState() != State.REMOVED || obj.OriginalName != null)))
        //        {
        //            nodes.Push(obj);
        //        }
        //    });

        //    Parallel.ForEach(nodes, (obj1) =>
        //    {
        //        State state1 = obj1.GetState();
        //        if (state1 != State.OWNED)
        //        {
        //            foreach (string referenceList in obj1.referenceLists())
        //            {
        //                Desc desc = GameData.getDesc(obj1.type, referenceList);
        //                if (!(desc.defaultValue is GameData.TripleInt))
        //                    desc = (Desc)null;
        //                foreach (KeyValuePair<string, GameData.TripleInt> keyValuePair in obj1.referenceData(referenceList, true))
        //                {
        //                    State state2 = obj1.GetState(referenceList, keyValuePair.Key);
        //                    switch (state2)
        //                    {
        //                        case State.ORIGINAL:
        //                        case State.LOCKED:
        //                        case State.LOCKED_REMOVED:
        //                            continue;
        //                        default:
        //                            GameData.Item obj2 = mod.getItem(keyValuePair.Key);
        //                            GameChange changeData;
        //                            if (state2 == State.REMOVED)
        //                                changeData = new GameChange(state2, mod.Filename, keyValuePair.Value);
        //                            else
        //                                continue;

        //                            Helpers.AddToList(keyValuePair.Key, obj1.type, obj1.Name, changeData);

        //                            continue;
        //                    }
        //                }
        //            }
        //            foreach (KeyValuePair<string, GameData.Instance> keyValuePair1 in obj1.InstanceData())
        //            {
        //                State state2 = keyValuePair1.Value.GetState();
        //                ChangeData changeData;
        //                switch (state2)
        //                {
        //                    case State.ORIGINAL:
        //                    case State.LOCKED:
        //                    case State.LOCKED_REMOVED:
        //                        continue;
        //                    case State.OWNED:
        //                        changeData = new ChangeData(ChangeType.NEWINST, keyValuePair1.Key, state2, $"Instance : {keyValuePair1.Key}");


        //                        break;

        //                    default:
        //                        changeData = new ChangeData(ChangeType.MODINST, keyValuePair1.Key, state2, $"Instance : {keyValuePair1.Key}");
        //                        break;
        //                }

        //                if (state2 == State.MODIFIED)
        //                {
        //                    foreach (KeyValuePair<string, object> keyValuePair2 in (GameData.Item)keyValuePair1.Value)
        //                    {
        //                        if (keyValuePair1.Value.GetState(keyValuePair2.Key) == State.MODIFIED)
        //                            changeData.Add((Changes)new ChangeData(ChangeType.INSTVALUE, keyValuePair1.Key, keyValuePair2.Key, keyValuePair1.Value.OriginalValue(keyValuePair2.Key), keyValuePair1.Value[keyValuePair2.Key], state2));
        //                    }
        //                }
        //                obj1.ChangeData = changeData;
        //            }
        //        }
        //    });

        //    nodes.Clear();
        //}

    }
}